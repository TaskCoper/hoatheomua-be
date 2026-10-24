using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PostImport;
using HoaTheoMua.Service.Test.PostPublishingTest;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;

namespace HoaTheoMua.Service.Test.PostImportTest;

public class AiWorkbook
{
    private static Dictionary<string, object?> Post(bool ai = false, bool scheduled = false)
    {
        var row = Imports.Post(channels: "Facebook, Instagram"); row["AI nội dung"] = ai ? "Có" : "Không";
        if (!scheduled) row["Ngày giờ đăng"] = null;
        return row;
    }
    private static Dictionary<string, object?> Image(string platform, bool ai = false, string ratio = "Gốc", string url = "https://example.com/one.jpg")
    {
        var row = Imports.Image(url: url); row["Nền tảng"] = platform; row["Xử lý ảnh"] = ai ? "Xử lý bằng AI" : "Giữ nguyên";
        row["Tỉ lệ đầu ra"] = ratio; row["Yêu cầu AI"] = ai ? "Mở rộng nền, giữ nguyên hoa" : ""; return row;
    }
    [Fact]
    public async Task BlankScheduleCreatesWaitingPostsWithoutAccountsOrPublicationJobs()
    {
        await using var db = Lifecycle.Db(); var ai = new Mock<Ai.IService>(MockBehavior.Strict); var setup = Imports.Create(db, ai: ai.Object);
        var batch = await Imports.Upload(setup, await Imports.Book([Post()], [Image("Facebook"), Image("Instagram")]));
        Assert.True(await setup.Worker.ProcessOne(batch.Id));
        var detail = await setup.Service.Get(batch.Id);
        Assert.Equal(2, detail.Batch.Waiting); Assert.Equal("Waiting", Assert.Single(detail.Rows).Status);
        Assert.Empty(await db.PostPublications.ToListAsync()); Assert.All(setup.Adapters, a => Assert.Equal(0, a.Sends));
        Assert.All(await db.GeneratedPosts.ToListAsync(), p => Assert.Equal(ContentSource.Manual, p.Source));
        ai.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ImagesKeepSeparatePlatformAssignmentsAndIndependentOrder()
    {
        await using var db = Lifecycle.Db(); var setup = Imports.Create(db);
        var batch = await Imports.Upload(setup, await Imports.Book([Post()], [Image("Facebook", url:"https://example.com/fb.jpg"), Image("Instagram", url:"https://example.com/ig.jpg")]));
        await setup.Worker.ProcessOne(batch.Id);
        foreach (var target in await db.PlatformPosts.ToListAsync())
        {
            var slot = await db.PlatformPostImages.SingleAsync(x => x.PlatformPostId == target.Id);
            var image = await db.GeneratedPostImages.SingleAsync(x => x.Id == slot.SelectedGeneratedPostImageId);
            Assert.Equal(target.Platform == PlatformType.Facebook ? "https://example.com/fb.jpg" : "https://example.com/ig.jpg", image.ImageUrl);
        }
    }
    [Theory]
    [InlineData(false, "4:5")]
    [InlineData(true, "Gốc")]
    [InlineData(true, "2:7")]
    public async Task InvalidImageModeRatioCombinationIsRejected(bool ai, string ratio)
    {
        await using var db = Lifecycle.Db(); var setup = Imports.Create(db);
        var parsed = setup.Reader.Read(new MemoryStream(await Imports.Book([Post()], [Image("Facebook",ai,ratio),Image("Instagram")])));
        Assert.Contains(parsed.Rows[0].Issues, x => x.Code == "IMPORT_IMAGE_RATIO_INVALID" && x.Sheet == "Anh" && x.RowNumber == 2);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AiRunsPerPlatformAndOnlyForSelectedImagesAndDoesNotRepeatOnReimport(bool scheduled)
    {
        await using var db = Lifecycle.Db(); var ai = new Mock<Ai.IService>(MockBehavior.Strict);
        ai.Setup(x => x.GenerateContent(It.IsAny<Ai.Request.ContentInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ai.Request.ContentInput input, CancellationToken _) => new Ai.Response.GeneratedContent("AI " + input.Platform, "Caption " + input.Platform, "#AI", "test-model", "test-content", 1));
        ai.Setup(x => x.GenerateImage(It.Is<Ai.Request.ImageInput>(x => x.RatioCode == "Portrait4x5" && x.Feedback == "Mở rộng nền, giữ nguyên hoa"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ai.Response.GeneratedImage("https://example.com/ai-4x5.jpg", "test-model", "test-image", 1));
        var setup = Imports.Create(db, ai: ai.Object);
        foreach (var name in new[] { "facebook", "instagram" })
            db.Configs.Add(new() { Id=Guid.NewGuid(), Group="postgen", Key="postgen.system-prompt."+name, Kind="Setting", Value="{\"isActive\":true,\"content\":\"Write caption\"}" });
        db.Configs.Add(new() { Id=Guid.NewGuid(), Group="postgen", Key="postgen.image_instruction", Kind="Setting", Value="\"Keep product\"" });
        await db.SaveChangesAsync();
        if (scheduled)
        {
            var facebook = await Imports.Account(db, setup, PlatformType.Facebook);
            var instagram = await Imports.Account(db, setup, PlatformType.Instagram);
            await setup.Service.SaveSettings(new() { Accounts = [new() { Platform="facebook",AccountId=facebook },new() { Platform="instagram",AccountId=instagram }] });
        }
        var bytes = await Imports.Book([Post(true,scheduled)], [Image("Facebook"), Image("Instagram",true,"4:5")]);
        var batch = await Imports.Upload(setup, bytes); await setup.Worker.ProcessOne(batch.Id);
        var detail = await setup.Service.Get(batch.Id); Assert.Empty(detail.Rows[0].Issues); Assert.True((scheduled ? detail.Batch.Scheduled : detail.Batch.Waiting) == 2, System.Text.Json.JsonSerializer.Serialize(detail));
        var versions = await db.GeneratedPosts.Where(x => x.Source == ContentSource.Ai).ToListAsync(); Assert.Equal(2, versions.Count);
        var image = await db.GeneratedPostImages.SingleAsync(x => x.StorageProvider == "ImportAi"); Assert.Equal(AspectRatioType.Portrait4x5,image.RatioCode);
        Assert.Equal("https://example.com/one.jpg",image.SourceImageUrl);
        var second = await Imports.Upload(setup, bytes); await setup.Worker.ProcessOne(second.Id);
        ai.Verify(x => x.GenerateContent(It.IsAny<Ai.Request.ContentInput>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        ai.Verify(x => x.GenerateImage(It.IsAny<Ai.Request.ImageInput>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(scheduled ? 2 : 0, await db.PostPublications.CountAsync());
    }
    [Fact]
    public async Task AiFailureNeverPublishesOrAutomaticallyRepeatsPaidCalls()
    {
        await using var db = Lifecycle.Db(); var ai = new Mock<Ai.IService>();
        ai.Setup(x => x.GenerateContent(It.IsAny<Ai.Request.ContentInput>(),It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("provider unreachable"));
        var setup = Imports.Create(db, ai:ai.Object);
        db.Configs.Add(new() { Id=Guid.NewGuid(), Group="postgen", Key="postgen.system-prompt.facebook", Kind="Setting", Value="{\"isActive\":true,\"content\":\"Write caption\"}" });
        await db.SaveChangesAsync();
        var batch = await Imports.Upload(setup, await Imports.Book([Post(true,true)], [Image("Facebook"),Image("Instagram")]));
        await setup.Worker.ProcessOne(batch.Id); await setup.Worker.ProcessOne(batch.Id);
        Assert.Empty(await db.PostPublications.ToListAsync());
        Assert.Contains((await setup.Service.Get(batch.Id)).Rows[0].Issues,x=>x.Code=="IMPORT_AI_FAILED");
        ai.Verify(x=>x.GenerateContent(It.IsAny<Ai.Request.ContentInput>(),It.IsAny<CancellationToken>()),Times.Once);
        Assert.True(!await db.PlatformPosts.AnyAsync(x=>x.Status==PlatformPostStatus.Generating), (await db.PostImportRows.SingleAsync()).Preparation);
    }
    [Fact]
    public async Task InterruptedAiIsNotRepeatedAfterLeaseRecovery()
    {
        await using var db = Lifecycle.Db(); using var cancel = new CancellationTokenSource();
        var ai = new Mock<Ai.IService>();
        ai.Setup(x => x.GenerateContent(It.IsAny<Ai.Request.ContentInput>(), It.IsAny<CancellationToken>()))
            .Returns((Ai.Request.ContentInput _, CancellationToken ct) => { cancel.Cancel(); throw new OperationCanceledException(ct); });
        var setup = Imports.Create(db, ai: ai.Object);
        db.Configs.Add(new() { Id=Guid.NewGuid(), Group="postgen", Key="postgen.system-prompt.facebook", Kind="Setting", Value="{\"isActive\":true,\"content\":\"Write caption\"}" });
        await db.SaveChangesAsync();
        var batch = await Imports.Upload(setup, await Imports.Book([Post(true)], [Image("Facebook"),Image("Instagram")]));
        Assert.False(await setup.Worker.ProcessOne(batch.Id,cancel.Token));
        setup.Clock.Now = setup.Clock.Now.AddMinutes(5); db.ChangeTracker.Clear();
        Assert.True(await setup.Worker.ProcessOne(batch.Id));
        Assert.Contains((await setup.Service.Get(batch.Id)).Rows[0].Issues,x=>x.Code=="IMPORT_AI_INTERRUPTED");
        Assert.Empty(await db.PostPublications.ToListAsync());
        Assert.False(await db.PlatformPosts.AnyAsync(x=>x.Status==PlatformPostStatus.Generating));
        ai.Verify(x=>x.GenerateContent(It.IsAny<Ai.Request.ContentInput>(),It.IsAny<CancellationToken>()),Times.Once);
    }

}

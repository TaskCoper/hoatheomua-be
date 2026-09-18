using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Gen = HoaTheoMua.Service.PostGeneration;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class GetContentVersions
{
    [Fact]
    public async Task HistoryIsScopedStableAndReadOnlyWhileDetailOnlyReturnsOfficialContent()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var id = await GeneratePlatformPosts.Seed(db);
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetContentVersions(id, "facebook"));
        var initial = await service.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook, PlatformType.Instagram));
        await service.EditPlatformContent(id, "facebook", new() { Content = "Manual" });
        var v1 = initial.Platforms.Single(x => x.Platform == "facebook").Content!.GenerationId;
        await service.SelectOfficialContent(id, "facebook", new() { GenerationId = v1 });
        var first = await service.GetContentVersions(id, "facebook");
        Assert.Equal(2, first.Versions.Count); Assert.Equal(v1, Assert.Single(first.Versions.Where(x => x.IsOfficial)).GenerationId);
        Assert.Equal(1, (await service.GetContentVersions(id, "instagram")).Versions.Count);
        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(await service.GetContentVersions(id, "facebook")));
        var detail = await new HoaTheoMua.Service.PostCore.Service(db).GetPostCoreDetail(id);
        var json = JsonSerializer.SerializeToElement(detail, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.False(json.TryGetProperty("images", out _)); Assert.False(json.GetProperty("platforms")[0].TryGetProperty("versions", out _));
        Assert.Equal(2, handler.Calls);
        (await db.BasePosts.SingleAsync(x => x.Id == id)).IsDeleted = true; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetContentVersions(id, "facebook"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("tiktok")]
    public async Task UnknownPlatformIsValidationError(string platform)
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(new AiContentClient.Handler((_, _) => throw new Exception("No AI expected"))));
        await Assert.ThrowsAsync<ValidationExceptionError>(() => service.GetContentVersions(Guid.NewGuid(), platform));
    }

    [Fact]
    public void ValidatorsRejectInvalidIdsLengthsAndEmptyBody()
    {
        var edit = new Gen.Validate.EditContentValidator();
        Assert.False(edit.Validate(new Gen.Request.EditContentRequest() { Content = " " }).IsValid);
        Assert.False(edit.Validate(new Gen.Request.EditContentRequest() { Content = new string('x', 20001) }).IsValid);
        Assert.False(edit.Validate(new Gen.Request.EditContentRequest() { Content = "ok", Title = new string('x', 501) }).IsValid);
        Assert.False(edit.Validate(new Gen.Request.EditContentRequest() { Content = "ok", Hashtag = new string('x', 1001) }).IsValid);
        Assert.True(edit.Validate(new Gen.Request.EditContentRequest() { Content = new string('x', 20000), Title = new string('x', 500), Hashtag = new string('x', 1000) }).IsValid);
        var regen = new Gen.Validate.RegenerateContentValidator();
        Assert.True(regen.Validate(new Gen.Request.RegenerateContentRequest()).IsValid);
        Assert.True(regen.Validate(new Gen.Request.RegenerateContentRequest() { Feedback = new string('x', 2000) }).IsValid);
        Assert.False(regen.Validate(new Gen.Request.RegenerateContentRequest() { Feedback = new string('x', 2001) }).IsValid);
        Assert.False(regen.Validate(new Gen.Request.RegenerateContentRequest() { SourceGenerationId = Guid.Empty }).IsValid);
        Assert.False(new Gen.Validate.SelectContentValidator().Validate(new Gen.Request.SelectContentRequest()).IsValid);
    }
}

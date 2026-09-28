using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Gen = HoaTheoMua.Service.PostGeneration;
namespace HoaTheoMua.Service.Test.PostGenTest;

public class GetImageVersions
{
    [Fact]
    public async Task GroupsAndNumbersEachPairIncludesFailedAndDeletedSourcesAndScopesSelections()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var (id, sources) = await GenerateImages.Seed(db, 2);
        var handler = new AiContentClient.Handler((_, _) => throw new Exception("History must not call AI"));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        Assert.Empty((await service.GetImageVersions(id)).Groups);
        var time = DateTimeOffset.UtcNow;
        var first = new HoaTheoMua.Repository.Entity.GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = id, SourceBasePostImageId = sources[0], RatioCode = AspectRatioType.Square1x1, SourceImageUrl = "https://example.com/a.png", Status = ImageStatus.Succeeded, ImageUrl = "https://example.com/v1.png", CreatedAt = time };
        db.GeneratedPostImages.Add(first);
        db.GeneratedPostImages.Add(new() { Id = Guid.NewGuid(), BasePostId = id, SourceBasePostImageId = sources[0], RatioCode = first.RatioCode, SourceImageUrl = first.ImageUrl!, ParentGeneratedPostImageId = first.Id, Status = ImageStatus.Failed, ErrorMessage = "Failed", CreatedAt = time.AddSeconds(1) });
        db.GeneratedPostImages.Add(new() { Id = Guid.NewGuid(), BasePostId = id, SourceBasePostImageId = sources[0], RatioCode = AspectRatioType.Story9x16, SourceImageUrl = first.SourceImageUrl, Status = ImageStatus.Succeeded, ImageUrl = "https://example.com/story.png", CreatedAt = time });
        foreach (var type in new[] { PlatformType.Facebook, PlatformType.Instagram, PlatformType.Zalo })
        {
            var platform = new HoaTheoMua.Repository.Entity.PlatformPost { Id = Guid.NewGuid(), BasePostId = id, Platform = type, IsDeleted = type == PlatformType.Zalo };
            db.PlatformPosts.Add(platform); db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = platform.Id, SourceBasePostImageId = sources[0], RatioCode = first.RatioCode, SelectedGeneratedPostImageId = first.Id, SortOrder = 1 });
        }
        (await db.BasePostImages.SingleAsync(x => x.Id == sources[0])).IsDeleted = true;
        await db.SaveChangesAsync();
        var history = await service.GetImageVersions(id);
        Assert.Equal(2, history.Groups.Count); Assert.All(history.Groups, g => Assert.True(g.SourceIsDeleted));
        var square = history.Groups.Single(x => x.RatioCode == "Square1x1");
        Assert.Equal(new[] { 1, 2 }, square.Versions.Select(x => x.VersionNo));
        Assert.Equal(new[] { "facebook", "instagram" }, square.Versions[0].SelectedPlatforms);
        Assert.Equal("Failed", square.Versions[1].Status); Assert.Null(square.Versions[1].ImageUrl);
        Assert.Equal(1, history.Groups.Single(x => x.RatioCode == "Story9x16").Versions[0].VersionNo);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(history), System.Text.Json.JsonSerializer.Serialize(await service.GetImageVersions(id)));
        var other = await new HoaTheoMua.Service.PostCore.Service(db).CreatePostCore(new() { Title = "Other", Content = "Other" });
        Assert.Empty((await service.GetImageVersions(other.BasePostId)).Groups);
        (await db.BasePosts.SingleAsync(x => x.Id == id)).IsDeleted = true; await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetImageVersions(id));
    }

    [Fact]
    public void ValidatorsRejectMalformedInputsButAllowEmptySelectionAndOptionalFeedback()
    {
        var regen = new Gen.Validate.RegenerateImageValidator();
        Assert.False(regen.Validate(new Gen.Request.RegenerateImageRequest()).IsValid);
        var input = new Gen.Request.RegenerateImageRequest { SourceBasePostImageId = Guid.NewGuid(), RatioCode = AspectRatioType.Square1x1 };
        Assert.True(regen.Validate(input).IsValid);
        input.Feedback = new string('a', 2001); Assert.False(regen.Validate(input).IsValid);
        input.Feedback = null; input.SourceGeneratedPostImageId = Guid.Empty; Assert.False(regen.Validate(input).IsValid);
        input.SourceGeneratedPostImageId = null; input.RatioCode = (AspectRatioType)999; Assert.False(regen.Validate(input).IsValid);
        var update = new Gen.Validate.UpdatePlatformImagesValidator();
        Assert.False(update.Validate(new Gen.Request.UpdatePlatformImagesRequest()).IsValid);
        Assert.True(update.Validate(new Gen.Request.UpdatePlatformImagesRequest { Images = new() }).IsValid);
        Assert.False(update.Validate(new Gen.Request.UpdatePlatformImagesRequest { Images = new() { null! } }).IsValid);
        Assert.False(update.Validate(new Gen.Request.UpdatePlatformImagesRequest { Images = new() { new() { SourceBasePostImageId = Guid.NewGuid(), RatioCode = AspectRatioType.Square1x1, SelectedGeneratedPostImageId = Guid.Empty, SortOrder = 0 } } }).IsValid);
    }
}

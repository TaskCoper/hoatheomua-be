using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Gen = HoaTheoMua.Service.PostGeneration;
namespace HoaTheoMua.Service.Test.PostGenTest;

public class UpdatePlatformImages
{
    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    internal static Gen.Request.UpdatePlatformImagesRequest Select(params GeneratedPostImage[] images) => new()
    {
        Images = images.Select((x, i) => new Gen.Request.PlatformImageInput { SourceBasePostImageId = x.SourceBasePostImageId,
            RatioCode = x.RatioCode, SelectedGeneratedPostImageId = x.Id, SortOrder = i + 1 }).ToList()
    };
    [Fact]
    public async Task ChoosesSharedRevisionsPerPlatformReordersAndClearsWithoutChangingContentOrHistory()
    {
        await using var db = Db(); var (id, sources) = await GenerateImages.Seed(db, 2);
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
            return AiContentClient.Completion(GenerateImages.IsImage(body.RootElement) ? "![image](https://example.com/out.png)" : "{\"title\":\"Hoa\",\"content\":\"Content\",\"hashtag\":\"#hoa\"}");
        });
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        await service.GeneratePlatformPosts(id, GenerateImages.Input(sources, PlatformType.Facebook, PlatformType.Instagram));
        var images = await db.GeneratedPostImages.OrderBy(x => x.SourceBasePostImageId).ToArrayAsync();
        var official = await db.PlatformPosts.OrderBy(x => x.Id).Select(x => x.OfficialGenerationId).ToListAsync();
        var old = images[0];
        var v2 = await service.RegenerateImage(id, new() { SourceBasePostImageId = old.SourceBasePostImageId, RatioCode = old.RatioCode, SourceGeneratedPostImageId = old.Id });
        var newer = await db.GeneratedPostImages.SingleAsync(x => x.Id == v2.GeneratedPostImageId);
        var calls = handler.Calls;
        var result = await service.UpdatePlatformImages(id, "facebook", Select(images[1], newer));
        var facebook = result.Platforms.Single(x => x.Platform == "facebook"); var instagram = result.Platforms.Single(x => x.Platform == "instagram");
        Assert.Equal("Ready", facebook.Status);
        Assert.Equal(new[] { images[1].Id, newer.Id }, facebook.Images.Select(x => x.SelectedGeneratedPostImageId));
        Assert.Contains(instagram.Images, x => x.SelectedGeneratedPostImageId == old.Id);
        var slotIds = await db.PlatformPostImages.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        await service.UpdatePlatformImages(id, "facebook", Select(newer, images[1]));
        Assert.Equal(slotIds, await db.PlatformPostImages.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync());
        await service.UpdatePlatformImages(id, "instagram", Select(newer)); // Reuse v2 without AI.
        var history = await service.GetImageVersions(id);
        Assert.Equal(new[] { "facebook", "instagram" }, history.Groups.SelectMany(x => x.Versions).Single(x => x.GeneratedPostImageId == newer.Id).SelectedPlatforms);
        var cleared = await service.UpdatePlatformImages(id, "facebook", new() { Images = new() });
        Assert.Empty(cleared.Platforms.Single(x => x.Platform == "facebook").Images);
        Assert.Equal("Generated", cleared.Platforms.Single(x => x.Platform == "facebook").Status);
        Assert.Equal(3, await db.GeneratedPostImages.CountAsync()); Assert.Equal(2, await db.GeneratedPosts.CountAsync());
        Assert.Equal(official, await db.PlatformPosts.OrderBy(x => x.Id).Select(x => x.OfficialGenerationId).ToListAsync());
        Assert.Equal(calls, handler.Calls);
    }

    [Fact]
    public async Task RejectsWrongPostPairFailureDuplicatesAndBusyStateWithoutPartialChanges()
    {
        await using var db = Db(); var (id, sources) = await GenerateImages.Seed(db, 2);
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion("![image](https://example.com/out.png)")));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var platform = new PlatformPost { Id = Guid.NewGuid(), BasePostId = id, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Draft };
        db.PlatformPosts.Add(platform); await db.SaveChangesAsync();
        var v1 = await service.RegenerateImage(id, new() { SourceBasePostImageId = sources[0], RatioCode = AspectRatioType.Square1x1 });
        var image = await db.GeneratedPostImages.SingleAsync();
        await service.UpdatePlatformImages(id, "facebook", Select(image));
        Assert.Equal(PlatformPostStatus.Draft, platform.Status); // Image only is not Ready.
        var before = JsonSerializer.Serialize(await db.PlatformPostImages.ToListAsync());
        var request = Select(image); request.Images![0].SelectedGeneratedPostImageId = Guid.NewGuid();
        Assert.Equal("Selected image does not belong to this post", (await Assert.ThrowsAsync<BadRequestException>(() => service.UpdatePlatformImages(id, "facebook", request))).Message);
        request = Select(image); request.Images![0].RatioCode = AspectRatioType.Story9x16;
        Assert.Equal("Selected image does not match the slot", (await Assert.ThrowsAsync<BadRequestException>(() => service.UpdatePlatformImages(id, "facebook", request))).Message);
        request = Select(image); request.Images![0].SourceBasePostImageId = sources[1];
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdatePlatformImages(id, "facebook", request));
        request = Select(image, image);
        Assert.Equal("Duplicate image slot", (await Assert.ThrowsAsync<BadRequestException>(() => service.UpdatePlatformImages(id, "facebook", request))).Message);
        request.Images![1].SortOrder = 1;
        Assert.Equal("Duplicate image order", (await Assert.ThrowsAsync<BadRequestException>(() => service.UpdatePlatformImages(id, "facebook", request))).Message);
        image.Status = ImageStatus.Failed; image.ImageUrl = null; await db.SaveChangesAsync();
        Assert.Equal("Selected image is not available", (await Assert.ThrowsAsync<BadRequestException>(() => service.UpdatePlatformImages(id, "facebook", Select(image)))).Message);
        Assert.Equal(before, JsonSerializer.Serialize(await db.PlatformPostImages.ToListAsync()));
        foreach (var status in new[] { PlatformPostStatus.Generating, PlatformPostStatus.Scheduled, PlatformPostStatus.Published })
        {
            platform.Status = status; await db.SaveChangesAsync();
            await Assert.ThrowsAsync<ConflictException>(() => service.UpdatePlatformImages(id, "facebook", new() { Images = new() }));
        }
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdatePlatformImages(id, "zalo", new() { Images = new() }));
        await Assert.ThrowsAsync<ValidationExceptionError>(() => service.UpdatePlatformImages(id, "0", new() { Images = new() }));
        Assert.Equal(1, handler.Calls);
    }
}

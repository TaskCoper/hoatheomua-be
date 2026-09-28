using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HoaTheoMua.Service.Test.PostGenTest;

public class SelectOfficialContent
{
    [Fact]
    public async Task SelectingOldVersionChangesOnlyPointerAndRejectsForeignOrMissingVersions()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var id = await GeneratePlatformPosts.Seed(db);
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(AiContentClient.Completion()));
        var service = GeneratePlatformPosts.Service(db, AiContentClient.Client(handler));
        var first = await service.GeneratePlatformPosts(id, GeneratePlatformPosts.Input(PlatformType.Facebook, PlatformType.Instagram));
        var v1 = first.Platforms.Single(p => p.Platform == "facebook").Content!.GenerationId;
        var foreign = first.Platforms.Single(p => p.Platform == "instagram").Content!.GenerationId;
        await service.EditPlatformContent(id, "facebook", new() { Content = "Manual" });
        await Assert.ThrowsAsync<BadRequestException>(() => service.SelectOfficialContent(id, "facebook", new() { GenerationId = foreign }));
        await Assert.ThrowsAsync<BadRequestException>(() => service.SelectOfficialContent(id, "facebook", new() { GenerationId = Guid.NewGuid() }));
        var result = await service.SelectOfficialContent(id, "facebook", new() { GenerationId = v1 });
        var content = result.Platforms.Single(p => p.Platform == "facebook").Content!;
        Assert.Equal(v1, content.GenerationId); Assert.Equal(1, content.VersionNo); Assert.Equal("Ai", content.Source);
        Assert.Equal(foreign, result.Platforms.Single(p => p.Platform == "instagram").Content!.GenerationId);
        Assert.Equal(3, await db.GeneratedPosts.CountAsync()); Assert.Equal(2, handler.Calls);
        await service.SelectOfficialContent(id, "facebook", new() { GenerationId = v1 });
        Assert.Equal(3, await db.GeneratedPosts.CountAsync());
    }
}

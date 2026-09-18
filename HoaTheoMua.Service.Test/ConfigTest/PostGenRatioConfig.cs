using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ConfigService = HoaTheoMua.Service.Config.Service;

namespace HoaTheoMua.Service.Test.ConfigTest;

public class PostGenRatioConfig
{
    [Fact]
    public async Task GenericConfigSupportsSharedPresetLifecycle()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var creatorId = Guid.NewGuid();
        var first = CreateService(db, creatorId);
        var second = CreateService(db, Guid.NewGuid());
        const string key = "postgen.ratio-preset";
        Assert.Empty(await first.GetConfigSetting("postgen", key));
        var created = await first.CreateConfig(new()
        {
            Key = key, Group = "postgen", Kind = "Setting",
            Value = JsonSerializer.SerializeToElement(new { facebook = new { ratios = new[] { "Square1x1" }, imageSelectionMode = "all_images" } })
        });
        Assert.Equal(creatorId, created.UserId);
        Assert.False(created.IsPublic);
        Assert.Equal(created.Id, Assert.Single(await second.GetConfigSetting("postgen", key)).Id);

        await second.UpdateConfig(created.Id, new()
        {
            Key = key, Group = "postgen", Kind = "Setting", IsPublic = false,
            Value = JsonSerializer.SerializeToElement(new { instagram = new { ratios = new[] { "Landscape16x9" }, imageSelectionMode = "first_n", imageCount = 3 } })
        });
        var updated = (await first.GetConfigById(created.Id))!;
        Assert.False(updated.Value.TryGetProperty("facebook", out _));
        Assert.Equal("Landscape16x9", updated.Value.GetProperty("instagram").GetProperty("ratios")[0].GetString());
        Assert.Single(await db.Configs.ToListAsync());

        await second.DeleteConfig(created.Id);
        Assert.Empty(await first.GetConfigSetting("postgen", key));
        Assert.True((await db.Configs.SingleAsync()).IsDeleted);
    }

    private static ConfigService CreateService(AppDbContext db, Guid id)
    {
        var user = new Mock<ICurrentUserService>();
        user.Setup(x => x.GetRequiredUserId()).Returns(id);
        return new(db, user.Object);
    }
}

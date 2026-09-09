using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SystemPromptEntity = HoaTheoMua.Repository.Entity.SystemPrompt;
using SystemPromptService = HoaTheoMua.Service.SystemPrompt.Service;

namespace HoaTheoMua.Service.Test.SystemPrompt;

public class GetSystemPrompt
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetSystemPrompt_ReturnsOnlyActivePromptsOrderedByUpdatedAtDescending()
    {
        using var dbContext = CreateDbContext();
        var now = DateTimeOffset.UtcNow;
        var newestId = Guid.NewGuid();
        var olderId = Guid.NewGuid();

        await dbContext.SystemPrompts.AddRangeAsync(
            new SystemPromptEntity
            {
                Id = olderId,
                Type = SystemPromptType.Flower,
                Content = "Older active prompt",
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-1)
            },
            new SystemPromptEntity
            {
                Id = newestId,
                Type = SystemPromptType.Card,
                Content = "Newest active prompt",
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now
            },
            new SystemPromptEntity
            {
                Id = Guid.NewGuid(),
                Type = SystemPromptType.Post,
                Content = "Deleted prompt",
                CreatedAt = now,
                UpdatedAt = now.AddDays(1),
                IsDeleted = true
            });
        await dbContext.SaveChangesAsync();

        var result = await new SystemPromptService(dbContext).GetSystemPrompt();

        Assert.Equal(new[] { newestId, olderId }, result.Select(x => x.Id));
        Assert.Equal(new[] { SystemPromptType.Card, SystemPromptType.Flower }, result.Select(x => x.Type));
    }

    [Fact]
    public async Task GetSystemPrompt_WithNoActivePrompts_ReturnsEmptyList()
    {
        using var dbContext = CreateDbContext();

        var result = await new SystemPromptService(dbContext).GetSystemPrompt();

        Assert.Empty(result);
    }
}

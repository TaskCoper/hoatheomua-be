using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SystemPromptEntity = HoaTheoMua.Repository.Entity.SystemPrompt;
using SystemPromptService = HoaTheoMua.Service.SystemPrompt.Service;

namespace HoaTheoMua.Service.Test.SystemPrompt;

public class GetSystemPromptInfo
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetSystemPromptInfo_WithActivePrompt_ReturnsPromptDetails()
    {
        using var dbContext = CreateDbContext();
        var prompt = new SystemPromptEntity
        {
            Id = Guid.NewGuid(),
            Type = SystemPromptType.Flower,
            Content = "# Flower prompt\nKeep this content.",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await dbContext.SystemPrompts.AddAsync(prompt);
        await dbContext.SaveChangesAsync();

        var result = await new SystemPromptService(dbContext).GetSystemPromptInfo(prompt.Id);

        Assert.Equal(prompt.Id, result.Id);
        Assert.Equal(prompt.Type, result.Type);
        Assert.Equal(prompt.Content, result.Content);
    }

    [Fact]
    public async Task GetSystemPromptInfo_WithUnknownId_ThrowsNotFound()
    {
        using var dbContext = CreateDbContext();

        await Assert.ThrowsAsync<NotFoundException>(
            () => new SystemPromptService(dbContext).GetSystemPromptInfo(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetSystemPromptInfo_WithDeletedPrompt_ThrowsNotFound()
    {
        using var dbContext = CreateDbContext();
        var prompt = new SystemPromptEntity
        {
            Id = Guid.NewGuid(),
            Type = SystemPromptType.Flower,
            Content = "Deleted prompt",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsDeleted = true
        };
        await dbContext.SystemPrompts.AddAsync(prompt);
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(
            () => new SystemPromptService(dbContext).GetSystemPromptInfo(prompt.Id));
    }
}

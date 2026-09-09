using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SystemPromptEntity = HoaTheoMua.Repository.Entity.SystemPrompt;
using SystemPromptRequest = HoaTheoMua.Service.SystemPrompt.Request.SystemPromptRequest;
using SystemPromptService = HoaTheoMua.Service.SystemPrompt.Service;

namespace HoaTheoMua.Service.Test.SystemPrompt;

public class CreateSystemPromptDefault
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateSystemPromptDefault_WithUnusedType_PersistsPromptWithMatchingTimestamps()
    {
        using var dbContext = CreateDbContext();
        var service = new SystemPromptService(dbContext);
        var request = new SystemPromptRequest
        {
            Type = SystemPromptType.Flower,
            Content = "System prompt for flowers"
        };

        var result = await service.CreateSystemPromptDefault(request);

        var savedPrompt = await dbContext.SystemPrompts.SingleAsync();
        Assert.Equal("Thêm mới System Prompt thành công.", result);
        Assert.Equal(request.Type, savedPrompt.Type);
        Assert.Equal(request.Content, savedPrompt.Content);
        Assert.NotEqual(Guid.Empty, savedPrompt.Id);
        Assert.Equal(savedPrompt.CreatedAt, savedPrompt.UpdatedAt);
        Assert.False(savedPrompt.IsDeleted);
    }

    [Fact]
    public async Task CreateSystemPromptDefault_WithExistingActiveType_ThrowsConflict()
    {
        using var dbContext = CreateDbContext();
        var now = DateTimeOffset.UtcNow;
        await dbContext.SystemPrompts.AddAsync(new SystemPromptEntity
        {
            Id = Guid.NewGuid(),
            Type = SystemPromptType.Flower,
            Content = "Existing prompt",
            CreatedAt = now,
            UpdatedAt = now
        });
        await dbContext.SaveChangesAsync();

        var service = new SystemPromptService(dbContext);
        var request = new SystemPromptRequest
        {
            Type = SystemPromptType.Flower,
            Content = "New prompt"
        };

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateSystemPromptDefault(request));

        Assert.Single(await dbContext.SystemPrompts.ToListAsync());
    }
}

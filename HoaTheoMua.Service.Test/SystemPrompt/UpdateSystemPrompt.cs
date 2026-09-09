using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.SystemPrompt.Validate;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SystemPromptEntity = HoaTheoMua.Repository.Entity.SystemPrompt;
using UpdateSystemPromptRequest = HoaTheoMua.Service.SystemPrompt.Request.UpdateSystemPromptRequest;
using SystemPromptService = HoaTheoMua.Service.SystemPrompt.Service;

namespace HoaTheoMua.Service.Test.SystemPrompt;

public class UpdateSystemPrompt
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static SystemPromptEntity CreatePrompt(string content, DateTimeOffset updatedAt) => new()
    {
        Id = Guid.NewGuid(),
        Type = SystemPromptType.Flower,
        Content = content,
        CreatedAt = updatedAt.AddDays(-1),
        UpdatedAt = updatedAt
    };

    [Fact]
    public async Task UpdateSystemPrompt_WithValidContent_UpdatesOnlyContentAndUpdatedAt()
    {
        using var dbContext = CreateDbContext();
        var originalUpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var prompt = CreatePrompt("Old prompt", originalUpdatedAt);
        await dbContext.SystemPrompts.AddAsync(prompt);
        await dbContext.SaveChangesAsync();

        var result = await new SystemPromptService(dbContext).UpdateSystemPrompt(prompt.Id, new UpdateSystemPromptRequest
        {
            Content = "Updated prompt"
        });

        var savedPrompt = await dbContext.SystemPrompts.SingleAsync();
        Assert.Equal(prompt.Id, result.Id);
        Assert.Equal(SystemPromptType.Flower, result.Type);
        Assert.Equal("Updated prompt", result.Content);
        Assert.Equal("Updated prompt", savedPrompt.Content);
        Assert.Equal(prompt.CreatedAt, savedPrompt.CreatedAt);
        Assert.Equal(prompt.Type, savedPrompt.Type);
        Assert.False(savedPrompt.IsDeleted);
        Assert.True(savedPrompt.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task UpdateSystemPrompt_WithUnicodeMarkdownAndWhitespace_PersistsExactContent()
    {
        using var dbContext = CreateDbContext();
        var prompt = CreatePrompt("Old prompt", DateTimeOffset.UtcNow.AddDays(-1));
        await dbContext.SystemPrompts.AddAsync(prompt);
        await dbContext.SaveChangesAsync();
        const string content = " # Hướng dẫn cho AI\n\tGiữ  nguyên tiếng Việt\r\n ";

        var result = await new SystemPromptService(dbContext).UpdateSystemPrompt(prompt.Id, new UpdateSystemPromptRequest
        {
            Content = content
        });

        Assert.Equal(content, result.Content);
        Assert.Equal(content, (await dbContext.SystemPrompts.SingleAsync()).Content);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateSystemPromptValidator_WithMissingOrBlankContent_ReturnsContentError(string? content)
    {
        var result = new UpdateSystemPromptRequestValidator().Validate(new UpdateSystemPromptRequest
        {
            Content = content
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Content" && error.ErrorMessage == "Content không được để trống.");
    }

    [Fact]
    public void UpdateSystemPromptValidator_With20001Characters_ReturnsLengthError()
    {
        var result = new UpdateSystemPromptRequestValidator().Validate(new UpdateSystemPromptRequest
        {
            Content = new string('a', 20_001)
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Content" && error.ErrorMessage == "Content tối đa 20.000 ký tự.");
    }

    [Fact]
    public void UpdateSystemPromptValidator_WithUnsupportedControlCharacter_ReturnsContentError()
    {
        var result = new UpdateSystemPromptRequestValidator().Validate(new UpdateSystemPromptRequest
        {
            Content = "Valid\0content"
        });

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Content" && error.ErrorMessage == "Content chứa ký tự điều khiển không hỗ trợ.");
    }

    [Fact]
    public async Task UpdateSystemPrompt_WithSameContent_RefreshesUpdatedAtAndKeepsOtherFields()
    {
        using var dbContext = CreateDbContext();
        var originalUpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var prompt = CreatePrompt("Current prompt", originalUpdatedAt);
        await dbContext.SystemPrompts.AddAsync(prompt);
        await dbContext.SaveChangesAsync();

        var result = await new SystemPromptService(dbContext).UpdateSystemPrompt(prompt.Id, new UpdateSystemPromptRequest
        {
            Content = "Current prompt"
        });

        var savedPrompt = await dbContext.SystemPrompts.SingleAsync();
        Assert.Equal("Current prompt", result.Content);
        Assert.Equal(prompt.Id, savedPrompt.Id);
        Assert.Equal(prompt.Type, savedPrompt.Type);
        Assert.Equal(prompt.CreatedAt, savedPrompt.CreatedAt);
        Assert.True(savedPrompt.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task UpdateSystemPrompt_WithUnknownId_ThrowsNotFoundWithoutCreatingPrompt()
    {
        using var dbContext = CreateDbContext();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new SystemPromptService(dbContext).UpdateSystemPrompt(Guid.NewGuid(), new UpdateSystemPromptRequest
            {
                Content = "Valid content"
            }));

        Assert.Empty(await dbContext.SystemPrompts.ToListAsync());
    }
}

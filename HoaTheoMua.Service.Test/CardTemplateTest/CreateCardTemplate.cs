using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.CardTemplate;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CardTemplateService = HoaTheoMua.Service.CardTemplate.Service;

namespace HoaTheoMua.Service.Test.CardTemplateTest;

public class CreateCardTemplate
{
    [Fact]
    public async Task CreateCardTemplate_ValidRequest_PersistsTemplateWithCurrentDefaults()
    {
        await using var db = CreateDbContext();
        var request = new Request.CreateCartTemplateRequest
        {
            Name = "Thiệp sinh nhật",
            Description = "Mẫu do Admin thiết kế",
            TemplateType = "handmade",
            ImageUrl = "https://storage.example.com/hm.png"
        };

        var result = await CreateService(db).CreateCardTemplate(request);
        var persisted = await db.CardTemplates.SingleAsync(x => x.Id == result.Id);

        Assert.Equal("Thiệp sinh nhật", result.Name);
        Assert.Equal("Mẫu do Admin thiết kế", result.Description);
        Assert.Equal("handmade", result.TemplateType);
        Assert.Equal("https://storage.example.com/hm.png", result.ImageUrl);
        Assert.False(result.IsActive);
        Assert.False(result.IsDeleted);
        Assert.Equal(CardTemplateType.Handmade, persisted.TemplateType);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(default, result.CreatedAt);
    }

    [Fact]
    public async Task CreateCardTemplate_DuplicateName_ThrowsBadRequest()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var request = new Request.CreateCartTemplateRequest
        {
            Name = "Thiệp sinh nhật",
            TemplateType = "ai",
            ImageUrl = "https://storage.example.com/ai.png"
        };

        await service.CreateCardTemplate(request);
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateCardTemplate(request));

        Assert.Equal("CARD_TEMPLATE_NAME_ALREADY_EXISTS", exception.Message);
        Assert.Equal(1, await db.CardTemplates.CountAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("other")]
    public async Task CreateCardTemplate_InvalidTemplateType_ThrowsBadRequest(string? templateType)
    {
        await using var db = CreateDbContext();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => CreateService(db).CreateCardTemplate(
            new Request.CreateCartTemplateRequest
            {
                Name = "Thiệp mới",
                TemplateType = templateType,
                ImageUrl = "https://storage.example.com/new.png"
            }));

        Assert.Equal("CARD_TEMPLATE_TYPE_INVALID", exception.Message);
        Assert.Equal(0, await db.CardTemplates.CountAsync());
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static CardTemplateService CreateService(AppDbContext dbContext) => new(
        dbContext,
        new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
}

using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.NhanhVnService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using NhanhVnServiceImplementation = HoaTheoMua.Service.NhanhVnService.Service;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using RevalidateService = HoaTheoMua.Service.Revalidate.IService;
using FulfillmentService = HoaTheoMua.Service.Fulfillment.IService;

namespace HoaTheoMua.Service.Test.NhanhVnServiceTest;

public class ProductLabelWebhookTest
{
    private const string ProductTagsConfigValue = """
        {
          "group": "nhanhvn",
          "label": "Nhãn sản phẩm",
          "value": [33, 32],
          "options": [
            { "label": "Test", "value": 34 },
            { "label": "VẬT LIỆU", "value": 33 },
            { "label": "PHỤ LIỆU", "value": 32 }
          ],
          "required": true,
          "inputType": "multiSelect",
          "validations": []
        }
        """;

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static NhanhVnServiceImplementation CreateService(AppDbContext dbContext)
    {
        var configuration = new ConfigurationBuilder().Build();
        return new NhanhVnServiceImplementation(
            new HttpClient(),
            dbContext,
            configuration,
            Mock.Of<RevalidateService>(),
            Mock.Of<FulfillmentService>());
    }

    private static async Task AddProductTagsConfig(AppDbContext dbContext)
    {
        dbContext.Configs.Add(new ConfigEntity
        {
            Id = Guid.NewGuid(),
            Key = "nhanhvn.product_labels",
            Group = "nhanhvn",
            Kind = "setting",
            Value = ProductTagsConfigValue,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetProductLabelByTagIds_ReturnsLabelsInPayloadOrder()
    {
        await using var dbContext = CreateDbContext();
        await AddProductTagsConfig(dbContext);
        var service = CreateService(dbContext);

        var result = await service.GetProductLabelByTagIds([33, 999, 32, 33]);

        Assert.Equal("VẬT LIỆU, PHỤ LIỆU", result);
    }

    [Fact]
    public async Task WebhookNhanhVnProductAdd_SavesProductLabelFromTagIds()
    {
        await using var dbContext = CreateDbContext();
        await AddProductTagsConfig(dbContext);
        var service = CreateService(dbContext);

        await service.WebhookNhanhVnProductAdd(new NhanhProductAddWebhookRequest
        {
            Data = new NhanhProductDetailDto
            {
                Id = 123,
                Name = "Sản phẩm mới",
                TagIds = [33, 32]
            }
        });

        var product = await dbContext.Products.SingleAsync(x => x.NhanhProductId == "123");
        Assert.Equal("VẬT LIỆU, PHỤ LIỆU", product.ProductLabel);
    }

    [Fact]
    public async Task WebhookNhanhVnProductUpdate_UpdatesProductLabelFromTagIds()
    {
        await using var dbContext = CreateDbContext();
        await AddProductTagsConfig(dbContext);
        dbContext.Products.Add(new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "456",
            ProductLabel = "Nhãn cũ",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await service.WebhookNhanhVnProductUpdate(new NhanhProductAddWebhookRequest
        {
            Data = new NhanhProductDetailDto
            {
                Id = 456,
                TagIds = [34, 32]
            }
        });

        var product = await dbContext.Products.SingleAsync(x => x.NhanhProductId == "456");
        Assert.Equal("Test, PHỤ LIỆU", product.ProductLabel);
    }

    [Fact]
    public async Task WebhookNhanhVnProductUpdate_EmptyTagIds_ClearsProductLabel()
    {
        await using var dbContext = CreateDbContext();
        await AddProductTagsConfig(dbContext);
        dbContext.Products.Add(new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "789",
            ProductLabel = "Nhãn cũ",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await service.WebhookNhanhVnProductUpdate(new NhanhProductAddWebhookRequest
        {
            Data = new NhanhProductDetailDto
            {
                Id = 789,
                TagIds = []
            }
        });

        var product = await dbContext.Products.SingleAsync(x => x.NhanhProductId == "789");
        Assert.Null(product.ProductLabel);
    }

    [Fact]
    public async Task WebhookNhanhVnProductUpdate_MissingTagIds_PreservesProductLabel()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "987",
            ProductLabel = "Nhãn hiện tại",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await service.WebhookNhanhVnProductUpdate(new NhanhProductAddWebhookRequest
        {
            Data = new NhanhProductDetailDto
            {
                Id = 987,
                TagIds = null
            }
        });

        var product = await dbContext.Products.SingleAsync(x => x.NhanhProductId == "987");
        Assert.Equal("Nhãn hiện tại", product.ProductLabel);
    }
}

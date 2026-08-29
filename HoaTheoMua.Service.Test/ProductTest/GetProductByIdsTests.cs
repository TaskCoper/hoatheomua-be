using HoaTheoMua.Repository;
using HoaTheoMua.Service.Product;
using HoaTheoMua.Service.Test.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using ProductService = HoaTheoMua.Service.Product.Service;

namespace HoaTheoMua.Service.Test.ProductTest;

public class GetProductByIdsTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private static ProductService CreateService(AppDbContext dbContext)
    {
        var httpContextMock = new Mock<IHttpContextAccessor>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "NhanhVnOption:AppId", "12345" },
                { "NhanhVnOption:BusinessId", "67890" },
                { "NhanhVnOption:Token", "test_token" },
                { "NhanhVnOption:BaseUrl", "https://nhanh.vn" }
            })
            .Build();

        return new ProductService(
            dbContext,
            httpContextMock.Object,
            new HttpClient(),
            configuration,
            MapperFactory.Create());
    }

    [Fact]
    public async Task GetProductByIds_ChildWithInactiveParent_ReturnsInactive()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "parent-inactive",
            ProductType = "Sản phẩm",
            IsActive = false,
            IsDeleted = false
        };
        var child = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Sản phẩm",
            Stock = 10,
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(parent, child);
        await dbContext.SaveChangesAsync();

        var result = await service.GetProductByIds(new Request.GetProductByIds
        {
            Items = new List<Request.GetProductByIdsItem>
            {
                new() { ProductId = child.Id, Number = 1 }
            }
        });

        Assert.False(Assert.Single(result.Products).IsActive);
    }

    [Fact]
    public async Task GetProductByIds_ChildrenWithActiveParent_ReturnOwnStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "parent-active",
            ProductType = "Sản phẩm",
            IsActive = true,
            IsDeleted = false
        };
        var activeChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Sản phẩm",
            Stock = 10,
            IsActive = true,
            IsDeleted = false
        };
        var inactiveChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Sản phẩm",
            Stock = 10,
            IsActive = false,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(parent, activeChild, inactiveChild);
        await dbContext.SaveChangesAsync();

        var result = await service.GetProductByIds(new Request.GetProductByIds
        {
            Items = new List<Request.GetProductByIdsItem>
            {
                new() { ProductId = activeChild.Id, Number = 1 },
                new() { ProductId = inactiveChild.Id, Number = 1 }
            }
        });

        Assert.True(result.Products.Single(x => x.Id == activeChild.Id).IsActive);
        Assert.False(result.Products.Single(x => x.Id == inactiveChild.Id).IsActive);
    }

    [Fact]
    public async Task GetProductByIds_StandaloneProduct_ReturnsOwnStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            Stock = 10,
            IsActive = false,
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.GetProductByIds(new Request.GetProductByIds
        {
            Items = new List<Request.GetProductByIdsItem>
            {
                new() { ProductId = product.Id, Number = 1 }
            }
        });

        Assert.False(Assert.Single(result.Products).IsActive);
    }

    [Fact]
    public async Task GetProductByIds_ActiveParentWithInactiveChild_ReturnsInactive()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "active-parent-with-inactive-child",
            ProductType = "Sản phẩm",
            Stock = 10,
            IsActive = true,
            IsDeleted = false
        };
        var activeChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Sản phẩm",
            IsActive = true,
            IsDeleted = false
        };
        var inactiveChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Sản phẩm",
            IsActive = false,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(parent, activeChild, inactiveChild);
        await dbContext.SaveChangesAsync();

        var result = await service.GetProductByIds(new Request.GetProductByIds
        {
            Items = new List<Request.GetProductByIdsItem>
            {
                new() { ProductId = parent.Id, Number = 1 }
            }
        });

        Assert.False(Assert.Single(result.Products).IsActive);
    }

    [Fact]
    public async Task GetProductByIds_ActiveParentWithAllActiveChildren_ReturnsActive()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "active-parent-with-active-children",
            ProductType = "Sản phẩm",
            Stock = 10,
            IsActive = true,
            IsDeleted = false
        };
        var firstChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Sản phẩm",
            IsActive = true,
            IsDeleted = false
        };
        var secondChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Sản phẩm",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(parent, firstChild, secondChild);
        await dbContext.SaveChangesAsync();

        var result = await service.GetProductByIds(new Request.GetProductByIds
        {
            Items = new List<Request.GetProductByIdsItem>
            {
                new() { ProductId = parent.Id, Number = 1 }
            }
        });

        Assert.True(Assert.Single(result.Products).IsActive);
    }

    [Fact]
    public async Task GetProductByIds_InactiveParentWithActiveChild_ReturnsInactive()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "inactive-parent-with-active-child",
            ProductType = "Sản phẩm",
            Stock = 10,
            IsActive = false,
            IsDeleted = false
        };
        var child = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Sản phẩm",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(parent, child);
        await dbContext.SaveChangesAsync();

        var result = await service.GetProductByIds(new Request.GetProductByIds
        {
            Items = new List<Request.GetProductByIdsItem>
            {
                new() { ProductId = parent.Id, Number = 1 }
            }
        });

        Assert.False(Assert.Single(result.Products).IsActive);
    }
}

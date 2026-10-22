using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using CategoryProductEntity = HoaTheoMua.Repository.Entity.CategoryProduct;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using HoaTheoMua.Service.Test.TestSupport;
using Xunit;
using ProductService = HoaTheoMua.Service.Product.Service;

namespace HoaTheoMua.Service.Test.ProductTest;

public class PinAndMoveParentOnlyTest
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
        var httpClient = new HttpClient();

        var myConfiguration = new Dictionary<string, string?>
        {
            {"NhanhVnOption:AppId", "12345"},
            {"NhanhVnOption:BusinessId", "67890"},
            {"NhanhVnOption:Token", "test_token"},
            {"NhanhVnOption:BaseUrl", "https://nhanh.vn"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

        return new ProductService(dbContext, httpContextMock.Object, httpClient, configuration, MapperFactory.Create());
    }

    [Fact]
    public async Task MoveProduct_ChildProductWithoutCategory_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Child",
            ParentProductId = "parent-1",
            IsPin = true,
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => service.MoveProduct(product.Id,
            new Request.MoveProductRequest { AfterProductId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task MoveProduct_ChildProductInCategory_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var category = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Category",
            Name_En = "Category",
            Description = "Category",
            Description_En = "Category",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false
        };
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Child",
            ParentProductId = "parent-1",
            IsDeleted = false
        };

        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddAsync(product);
        await dbContext.CategoryProductDetails.AddAsync(new CategoryProductDetail
        {
            Id = Guid.NewGuid(),
            CategoryProductId = category.Id,
            ProductId = product.Id,
            IsPin = true,
            IsDeleted = false
        });
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => service.MoveProduct(product.Id,
            new Request.MoveProductRequest { AfterProductId = Guid.NewGuid(), CategoryId = category.Id }));
    }

    [Fact]
    public async Task PinProduct_ChildProduct_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Child",
            ParentProductId = "parent-1",
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.PinProduct(
            new Request.PinProductRequest { ProductId = product.Id, IsPin = true }));

        Assert.Equal("Product not found or is not a parent product.", exception.Message);
    }

    // Bảo vệ case: ParentProductId là chuỗi rỗng "" (legacy data từ Nhanh.vn) được coi là parent
    [Fact]
    public async Task PinProduct_EmptyStringParentProductId_TreatedAsParent()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Root with empty string parent",
            ParentProductId = "",
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        // Should NOT throw — empty string ParentProductId is treated as parent
        var result = await service.PinProduct(
            new Request.PinProductRequest { ProductId = product.Id, IsPin = true });

        Assert.NotNull(result);
    }

    [Fact]
    public async Task MoveProduct_EmptyStringParentProductId_TreatedAsParent()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = "Root with empty string parent",
            ParentProductId = "",
            IsPin = true,
            IsDeleted = false
        };

        var anchor = NewProduct("Anchor", true, 32768);

        await dbContext.Products.AddRangeAsync(product, anchor);
        await dbContext.SaveChangesAsync();

        // Should NOT throw
        var result = await service.MoveProduct(product.Id,
            new Request.MoveProductRequest { AfterProductId = anchor.Id });

        Assert.NotNull(result);
        Assert.Equal(49152, product.Position);
    }

    // ---------------------------------------------------------------------
    // Helpers cho các ca ghim / đổi vị trí tự tính vị trí
    // ---------------------------------------------------------------------

    private static ProductEntity NewProduct(string name, bool isPin, double? position = null)
    {
        return new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductName = name,
            ParentProductId = "",
            IsPin = isPin,
            Position = position,
            IsDeleted = false
        };
    }

    private static CategoryProductEntity NewCategory()
    {
        return new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Category",
            Name_En = "Category",
            Description = "Category",
            Description_En = "Category",
            Type = CategoryProductType.Fix,
            IsActive = true,
            IsDeleted = false
        };
    }

    // Danh sách ghim toàn cục P01, P02, ... với Position 16384, 32768, ... (tên tăng dần để thứ tự ổn định)
    private static async Task<List<ProductEntity>> SeedPinned(AppDbContext dbContext, int count)
    {
        var products = new List<ProductEntity>();
        for (var i = 0; i < count; i++)
        {
            products.Add(NewProduct($"P{i + 1:00}", true, (i + 1) * 16384d));
        }

        await dbContext.Products.AddRangeAsync(products);
        await dbContext.SaveChangesAsync();
        return products;
    }

    // ---------------------------------------------------------------------
    // PinProduct: ghim
    // ---------------------------------------------------------------------

    [Fact]
    public async Task PinProduct_EmptyList_PositionIsFirstSlot()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = NewProduct("X", false);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        await service.PinProduct(new Request.PinProductRequest { ProductId = product.Id, IsPin = true });

        Assert.True(product.IsPin);
        Assert.Equal(140737488355328d, product.Position);
    }

    [Fact]
    public async Task PinProduct_NoAnchor_GoesToEndAndKeepsOthers()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 2);
        var product = NewProduct("X", false);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        await service.PinProduct(new Request.PinProductRequest { ProductId = product.Id, IsPin = true });

        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(49152, product.Position);
    }

    [Fact]
    public async Task PinProduct_AfterAnchor_InsertsRightAfterAnchor()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 3);
        var product = NewProduct("X", false);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        await service.PinProduct(new Request.PinProductRequest
        {
            ProductId = product.Id, IsPin = true, AfterProductId = pinned[0].Id
        });

        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(24576, product.Position);
        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(49152, pinned[2].Position);
    }

    [Fact]
    public async Task PinProduct_BeforeAnchor_InsertsAtTop()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 2);
        var product = NewProduct("X", false);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        await service.PinProduct(new Request.PinProductRequest
        {
            ProductId = product.Id, IsPin = true, BeforeProductId = pinned[0].Id
        });

        Assert.Equal(0, product.Position);
        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(32768, pinned[1].Position);
    }

    [Fact]
    public async Task PinProduct_AlreadyPinnedAtLimit_ReturnsSuccessWithoutChange()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 32);

        var result = await service.PinProduct(
            new Request.PinProductRequest { ProductId = pinned[5].Id, IsPin = true });

        Assert.NotNull(result);
        Assert.Equal(6 * 16384d, pinned[5].Position);
    }

    [Fact]
    public async Task PinProduct_ListIsFull_ThrowsBadRequest()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        await SeedPinned(dbContext, 32);
        var product = NewProduct("X", false);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.PinProduct(
            new Request.PinProductRequest { ProductId = product.Id, IsPin = true }));

        Assert.Equal("Only 32 products can be pinned.", exception.Message);
        Assert.False(product.IsPin);
    }

    [Fact]
    public async Task PinProduct_DuplicatePositions_AreRenumbered()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var a = NewProduct("A", true, 100);
        var b = NewProduct("B", true, 100);
        var c = NewProduct("C", true, 100);
        var product = NewProduct("X", false);
        await dbContext.Products.AddRangeAsync(a, b, c, product);
        await dbContext.SaveChangesAsync();

        await service.PinProduct(new Request.PinProductRequest { ProductId = product.Id, IsPin = true });

        Assert.Equal(16384, a.Position);
        Assert.Equal(32768, b.Position);
        Assert.Equal(49152, c.Position);
        Assert.Equal(65536, product.Position);
    }

    [Fact]
    public async Task PinProduct_AnchorNotPinned_ThrowsConflict()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        await SeedPinned(dbContext, 2);
        var notPinned = NewProduct("N", false);
        var product = NewProduct("X", false);
        await dbContext.Products.AddRangeAsync(notPinned, product);
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => service.PinProduct(new Request.PinProductRequest
        {
            ProductId = product.Id, IsPin = true, AfterProductId = notPinned.Id
        }));
    }

    // Doc 5.1 và 5.2: chỉ được gửi một trong hai mốc. Gửi cả hai luôn là 400,
    // dù hai mốc liền kề, không liền kề hay đảo thứ tự
    [Theory]
    [InlineData(0, 1)]   // liền kề
    [InlineData(0, 2)]   // không liền kề
    [InlineData(1, 0)]   // đảo thứ tự
    public async Task PinProduct_BothAnchors_ThrowsBadRequestAndPinsNothing(int afterIndex, int beforeIndex)
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 3);
        var product = NewProduct("X", false);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.PinProduct(new Request.PinProductRequest
        {
            ProductId = product.Id, IsPin = true,
            AfterProductId = pinned[afterIndex].Id, BeforeProductId = pinned[beforeIndex].Id
        }));

        Assert.Equal("Invalid pin anchor.", exception.Message);
        Assert.False(product.IsPin);
        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(49152, pinned[2].Position);
    }

    [Theory]
    [InlineData(0, 1)]   // liền kề
    [InlineData(0, 2)]   // không liền kề
    [InlineData(1, 0)]   // đảo thứ tự
    public async Task MoveProduct_BothAnchors_ThrowsBadRequestAndKeepsOrder(int afterIndex, int beforeIndex)
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 4);   // A, B, C, D

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.MoveProduct(pinned[3].Id,
            new Request.MoveProductRequest
            {
                AfterProductId = pinned[afterIndex].Id, BeforeProductId = pinned[beforeIndex].Id
            }));

        Assert.Equal("Invalid move anchor.", exception.Message);
        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(49152, pinned[2].Position);
        Assert.Equal(65536, pinned[3].Position);
    }

    [Fact]
    public async Task PinProduct_InCategory_IsIndependentFromGlobalPin()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var category = NewCategory();
        var product = NewProduct("X", false);
        var detail = new CategoryProductDetail
        {
            Id = Guid.NewGuid(), CategoryProductId = category.Id, ProductId = product.Id, IsPin = false, IsDeleted = false
        };
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddAsync(product);
        await dbContext.CategoryProductDetails.AddAsync(detail);
        await dbContext.SaveChangesAsync();

        await service.PinProduct(new Request.PinProductRequest
        {
            ProductId = product.Id, IsPin = true, CategoryId = category.Id
        });

        Assert.True(detail.IsPin);
        Assert.Equal(140737488355328d, detail.Position);
        Assert.False(product.IsPin);
        Assert.Null(product.Position);
    }

    [Fact]
    public async Task PinProduct_ProductNotInCategory_ThrowsBadRequest()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var category = NewCategory();
        var product = NewProduct("X", false);
        await dbContext.CategoryProducts.AddAsync(category);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.PinProduct(
            new Request.PinProductRequest { ProductId = product.Id, IsPin = true, CategoryId = category.Id }));

        Assert.Equal("Product does not belong to this category.", exception.Message);
    }

    // ---------------------------------------------------------------------
    // PinProduct: bỏ ghim
    // ---------------------------------------------------------------------

    [Fact]
    public async Task PinProduct_Unpin_ClearsPositionAndKeepsOthers()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 3);

        await service.PinProduct(new Request.PinProductRequest { ProductId = pinned[1].Id, IsPin = false });

        Assert.False(pinned[1].IsPin);
        Assert.Null(pinned[1].Position);
        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(49152, pinned[2].Position);
    }

    [Fact]
    public async Task PinProduct_UnpinNotPinned_ReturnsSuccessWithoutChange()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var product = NewProduct("X", false);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.PinProduct(new Request.PinProductRequest { ProductId = product.Id, IsPin = false });

        Assert.NotNull(result);
        Assert.False(product.IsPin);
        Assert.Null(product.Position);
    }

    [Fact]
    public async Task PinProduct_PinAgainAfterUnpin_GoesToEnd()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 3);

        await service.PinProduct(new Request.PinProductRequest { ProductId = pinned[0].Id, IsPin = false });
        await service.PinProduct(new Request.PinProductRequest { ProductId = pinned[0].Id, IsPin = true });

        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(49152, pinned[2].Position);
        Assert.Equal(65536, pinned[0].Position);
    }

    // ---------------------------------------------------------------------
    // MoveProduct: đổi vị trí theo mốc
    // ---------------------------------------------------------------------

    [Fact]
    public async Task MoveProduct_AfterAnchor_PlacesRightAfterAnchor()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 4);   // A, B, C, D

        await service.MoveProduct(pinned[3].Id, new Request.MoveProductRequest { AfterProductId = pinned[0].Id });

        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(24576, pinned[3].Position);
        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(49152, pinned[2].Position);
    }

    [Fact]
    public async Task MoveProduct_BeforeAnchor_PlacesAtTop()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 4);

        await service.MoveProduct(pinned[2].Id, new Request.MoveProductRequest { BeforeProductId = pinned[0].Id });

        Assert.Equal(0, pinned[2].Position);
        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(65536, pinned[3].Position);
    }

    [Fact]
    public async Task MoveProduct_AfterLast_PlacesAtBottom()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 4);

        await service.MoveProduct(pinned[0].Id, new Request.MoveProductRequest { AfterProductId = pinned[3].Id });

        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(49152, pinned[2].Position);
        Assert.Equal(65536, pinned[3].Position);
        Assert.Equal(81920, pinned[0].Position);
    }

    [Fact]
    public async Task MoveProduct_AnchorIsItself_ThrowsBadRequest()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 2);

        await Assert.ThrowsAsync<BadRequestException>(() => service.MoveProduct(pinned[0].Id,
            new Request.MoveProductRequest { AfterProductId = pinned[0].Id }));
    }

    [Fact]
    public async Task MoveProduct_AnchorNotPinned_ThrowsConflict()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 2);
        var notPinned = NewProduct("N", false);
        await dbContext.Products.AddAsync(notPinned);
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => service.MoveProduct(pinned[0].Id,
            new Request.MoveProductRequest { AfterProductId = notPinned.Id }));
    }

    [Fact]
    public async Task MoveProduct_NotPinned_ThrowsBadRequest()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 2);
        var product = NewProduct("X", false);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.MoveProduct(product.Id,
            new Request.MoveProductRequest { AfterProductId = pinned[0].Id }));

        Assert.Equal("Only product with pin can update position", exception.Message);
    }

    [Fact]
    public async Task MoveProduct_NoPositionAndNoAnchor_ThrowsBadRequest()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 1);

        await Assert.ThrowsAsync<BadRequestException>(() => service.MoveProduct(pinned[0].Id,
            new Request.MoveProductRequest()));
    }

    [Fact]
    public async Task MoveProduct_OldPositionOnly_WritesPositionDirectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 2);

        await service.MoveProduct(pinned[0].Id,
            new Request.MoveProductRequest { AfterProductId = pinned[1].Id });

        Assert.Equal(49152, pinned[0].Position);
    }

    // ---------------------------------------------------------------------
    // PinProducts: ghim hàng loạt
    // ---------------------------------------------------------------------

    [Fact]
    public async Task PinProducts_AppendsInSelectedOrder()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 2);
        var p1 = NewProduct("N1", false);
        var p2 = NewProduct("N2", false);
        var p3 = NewProduct("N3", false);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        await service.PinProducts(new Request.PinProductsRequest { ProductIds = new List<Guid> { p3.Id, p1.Id, p2.Id } });

        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(32768, pinned[1].Position);
        Assert.Equal(49152, p3.Position);
        Assert.Equal(65536, p1.Position);
        Assert.Equal(81920, p2.Position);
    }

    [Fact]
    public async Task PinProducts_AlreadyPinnedIsSkipped()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 1);
        var p1 = NewProduct("N1", false);
        await dbContext.Products.AddAsync(p1);
        await dbContext.SaveChangesAsync();

        await service.PinProducts(new Request.PinProductsRequest { ProductIds = new List<Guid> { pinned[0].Id, p1.Id } });

        Assert.Equal(16384, pinned[0].Position);
        Assert.Equal(32768, p1.Position);
    }

    [Fact]
    public async Task PinProducts_DuplicateIds_PinnedOnce()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var p1 = NewProduct("N1", false);
        var p2 = NewProduct("N2", false);
        await dbContext.Products.AddRangeAsync(p1, p2);
        await dbContext.SaveChangesAsync();

        await service.PinProducts(new Request.PinProductsRequest { ProductIds = new List<Guid> { p1.Id, p1.Id, p2.Id } });

        Assert.Equal(140737488355328d, p1.Position);
        Assert.Equal(140737488371712d, p2.Position);
    }

    [Fact]
    public async Task PinProducts_ExceedsLimit_ThrowsAndPinsNothing()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        await SeedPinned(dbContext, 30);
        var p1 = NewProduct("N1", false);
        var p2 = NewProduct("N2", false);
        var p3 = NewProduct("N3", false);
        await dbContext.Products.AddRangeAsync(p1, p2, p3);
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => service.PinProducts(
            new Request.PinProductsRequest { ProductIds = new List<Guid> { p1.Id, p2.Id, p3.Id } }));

        Assert.False(p1.IsPin);
        Assert.False(p2.IsPin);
        Assert.False(p3.IsPin);
    }

    [Fact]
    public async Task PinProducts_UnknownProduct_ThrowsAndPinsNothing()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var p1 = NewProduct("N1", false);
        await dbContext.Products.AddAsync(p1);
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => service.PinProducts(
            new Request.PinProductsRequest { ProductIds = new List<Guid> { p1.Id, Guid.NewGuid() } }));

        Assert.False(p1.IsPin);
    }

    // ---------------------------------------------------------------------
    // GetProductPin
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetProductPin_AfterUnpin_DoesNotContainUnpinnedProduct()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);
        var pinned = await SeedPinned(dbContext, 3);

        await service.PinProduct(new Request.PinProductRequest { ProductId = pinned[1].Id, IsPin = false });
        var result = await service.GetProductPin(new Request.GetProductPinRequest());

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, x => x.ProductId == pinned[1].Id);
    }
}

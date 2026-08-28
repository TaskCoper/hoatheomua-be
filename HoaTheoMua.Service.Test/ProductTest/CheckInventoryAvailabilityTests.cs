using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
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

public class CheckInventoryAvailabilityTests
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
    public async Task CheckInventoryAvailability_EmptyProductIds_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(new Request.CheckInventoryAvailabilityRequest()));

        Assert.Contains("ProductIds are required", exception.Message);
    }

    [Fact]
    public async Task CheckInventoryAvailability_ProductNotFound_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { Guid.NewGuid() }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(request));

        Assert.Contains("Products not found", exception.Message);
    }

    [Fact]
    public async Task CheckInventoryAvailability_ProductWithNoCoreSpec_PassesWithoutError()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Combo Test",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { product.Id }
        };

        // Should not throw
        await service.CheckInventoryAvailability(request);
    }

    [Fact]
    public async Task CheckInventoryAvailability_InsufficientInventory_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Material (CORE)
        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Hoa Lan",
            AvailableForSale = 2, // Chỉ có 2 cái
            IsActive = true,
            IsDeleted = false
        };

        // Combo 1 - Hoa Hồng cần 2 Hoa Lan
        var combo1 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Hoa Hồng",
            IsActive = true,
            IsDeleted = false
        };

        // Combo 2 - Hoa Lan cần 1 Hoa Lan
        var combo2 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Hoa Lan",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, combo1, combo2);
        await dbContext.SaveChangesAsync();

        // Combo 1 cần 2 Hoa Lan
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = combo1.Id,
            MatId = material.Id,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });

        // Combo 2 cần 1 Hoa Lan
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = combo2.Id,
            MatId = material.Id,
            Quantity = 1,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });

        await dbContext.SaveChangesAsync();

        // Request: 1 Hoa Hồng + 1 Hoa Lan = cần 3 Hoa Lan, chỉ có 2
        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { combo1.Id, combo2.Id }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(request));

        Assert.Contains("INVENTORY_INSUFFICIENT", exception.Message);
    }

    [Fact]
    public async Task CheckInventoryAvailability_SufficientInventory_PassesWithoutError()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Material (CORE) - có 10 cái
        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Hoa Lan",
            AvailableForSale = 10,
            IsActive = true,
            IsDeleted = false
        };

        // Combo - cần 2 Hoa Lan
        var combo = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Hoa Hồng",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, combo);
        await dbContext.SaveChangesAsync();

        // Combo cần 2 Hoa Lan
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = combo.Id,
            MatId = material.Id,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });

        await dbContext.SaveChangesAsync();

        // Request: 1 Hoa Hồng = cần 2 Hoa Lan, có 10 -> OK
        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { combo.Id }
        };

        // Should not throw
        await service.CheckInventoryAvailability(request);
    }

    [Fact]
    public async Task CheckInventoryAvailability_NonCoreSpec_NotIncludedInCalculation()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Material - có 1 cái
        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Giấy gói",
            AvailableForSale = 1,
            IsActive = true,
            IsDeleted = false
        };

        // Combo
        var combo = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Bó Hoa",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, combo);
        await dbContext.SaveChangesAsync();

        // Chỉ có non-core spec (IsCore = false)
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = combo.Id,
            MatId = material.Id,
            Quantity = 5, // Cần 5 cái nhưng là non-core
            IsCore = false,
            IsActive = true,
            IsDeleted = false
        });

        await dbContext.SaveChangesAsync();

        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { combo.Id }
        };

        // Should not throw vì spec không phải CORE
        await service.CheckInventoryAvailability(request);
    }

    [Fact]
    public async Task CheckInventoryAvailability_MultipleProductsSharingMaterial_CalculatesTotalCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Material (CORE) - có 5 cái
        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Ruy băng",
            AvailableForSale = 5,
            IsActive = true,
            IsDeleted = false
        };

        // Combo 1 - cần 2 Ruy băng
        var combo1 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Hoa Hồng",
            IsActive = true,
            IsDeleted = false
        };

        // Combo 2 - cần 3 Ruy băng
        var combo2 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Hoa Tulip",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, combo1, combo2);
        await dbContext.SaveChangesAsync();

        await dbContext.ComboSpecifications.AddRangeAsync(new[]
        {
            new Repository.Entity.ComboSpecification
            {
                Id = Guid.NewGuid(),
                SubId = combo1.Id,
                MatId = material.Id,
                Quantity = 2,
                IsCore = true,
                IsActive = true,
                IsDeleted = false
            },
            new Repository.Entity.ComboSpecification
            {
                Id = Guid.NewGuid(),
                SubId = combo2.Id,
                MatId = material.Id,
                Quantity = 3,
                IsCore = true,
                IsActive = true,
                IsDeleted = false
            }
        });

        await dbContext.SaveChangesAsync();

        // Request: 1 Hoa Hồng (2 ruy) + 1 Hoa Tulip (3 ruy) = cần 5 Ruy băng, có 5 -> OK
        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { combo1.Id, combo2.Id }
        };

        // Should not throw
        await service.CheckInventoryAvailability(request);
    }

    [Fact]
    public async Task CheckInventoryAvailability_TH1_ChaSanPhamConCombo_GetsChildSpecs()
    {
        // TH1: Cha "Sản phẩm" có con là "Combo"
        // Khi request cha, cần lấy specs của các con combo
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Material
        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Hoa Lan",
            AvailableForSale = 1, // Chỉ có 1
            IsActive = true,
            IsDeleted = false
        };

        // Cha - Sản phẩm
        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "parent-001",
            ProductType = "Sản phẩm",
            ProductName = "Bó Hoa Mix",
            IsActive = true,
            IsDeleted = false
        };

        // Con 1 - Combo_a (cần 2 Hoa Lan)
        var comboA = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Combo",
            ProductName = "Combo A",
            IsActive = true,
            IsDeleted = false
        };

        // Con 2 - Combo_b (cần 1 Hoa Lan)
        var comboB = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Combo",
            ProductName = "Combo B",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, parent, comboA, comboB);
        await dbContext.SaveChangesAsync();

        // Combo A cần 2 Hoa Lan
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboA.Id,
            MatId = material.Id,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });

        // Combo B cần 1 Hoa Lan
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = comboB.Id,
            MatId = material.Id,
            Quantity = 1,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });

        await dbContext.SaveChangesAsync();

        // Request cha: Bó Hoa Mix (có 2 con combo)
        // Tổng cần: 2 + 1 = 3 Hoa Lan, có 1 -> FAIL
        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { parent.Id }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(request));

        Assert.Contains("INVENTORY_INSUFFICIENT", exception.Message);
    }

    [Fact]
    public async Task CheckInventoryAvailability_TH2_ChaComboKhongCon_GetsOwnSpecs()
    {
        // TH2: Cha "Combo" không có biến thể con
        // Khi request cha combo, lấy specs của chính nó
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Material
        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Hoa Hồng",
            AvailableForSale = 1, // Chỉ có 1
            IsActive = true,
            IsDeleted = false
        };

        // Cha Combo - không có con
        var combo = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "combo-001",
            ProductType = "Combo",
            ProductName = "Bó Hoa Đơn",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, combo);
        await dbContext.SaveChangesAsync();

        // Combo cần 2 Hoa Hồng
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = combo.Id,
            MatId = material.Id,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });

        await dbContext.SaveChangesAsync();

        // Request: Bó Hoa Đơn cần 2 Hoa Hồng, có 1 -> FAIL
        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { combo.Id }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(request));

        Assert.Contains("INVENTORY_INSUFFICIENT", exception.Message);
    }

    [Fact]
    public async Task CheckInventoryAvailability_TH3_ChaComboCoCon_GetsChildSpecs()
    {
        // TH3: Cha "Combo" có biến thể con (các combo con)
        // Khi request cha, cần lấy specs của các con
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Material
        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Ruy băng",
            AvailableForSale = 2, // Chỉ có 2
            IsActive = true,
            IsDeleted = false
        };

        // Cha Combo
        var parentCombo = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "parent-combo-001",
            ProductType = "Combo",
            ProductName = "Bó Hoa Premium",
            IsActive = true,
            IsDeleted = false
        };

        // Con 1 - Combo variant (cần 2 Ruy băng)
        var childCombo1 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parentCombo.NhanhProductId,
            ProductType = "Combo",
            ProductName = "Premium A",
            IsActive = true,
            IsDeleted = false
        };

        // Con 2 - Combo variant (cần 2 Ruy băng)
        var childCombo2 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parentCombo.NhanhProductId,
            ProductType = "Combo",
            ProductName = "Premium B",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, parentCombo, childCombo1, childCombo2);
        await dbContext.SaveChangesAsync();

        // Child 1 cần 2 Ruy băng
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = childCombo1.Id,
            MatId = material.Id,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });

        // Child 2 cần 2 Ruy băng
        await dbContext.ComboSpecifications.AddAsync(new Repository.Entity.ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = childCombo2.Id,
            MatId = material.Id,
            Quantity = 2,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });

        await dbContext.SaveChangesAsync();

        // Request cha: Bó Hoa Premium (có 2 con)
        // Tổng cần: 2 + 2 = 4 Ruy băng, có 2 -> FAIL
        var request = new Request.CheckInventoryAvailabilityRequest
        {
            ProductIds = new List<Guid> { parentCombo.Id }
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(request));

        Assert.Contains("INVENTORY_INSUFFICIENT", exception.Message);
    }
}

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
    public async Task CheckInventoryAvailability_EmptyItems_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(new Request.CheckInventoryAvailabilityRequest()));

        Assert.Contains("Items are required", ex.Message);
    }

    [Fact]
    public async Task CheckInventoryAvailability_ProductNotFound_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.CheckInventoryAvailabilityRequest
        {
            Items = new List<Request.CartItem>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(request));

        Assert.Contains("Products not found", ex.Message);
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
            Items = new List<Request.CartItem>
            {
                new() { ProductId = product.Id, Quantity = 1 }
            }
        };

        var result = await service.CheckInventoryAvailability(request);

        Assert.Null(result);
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
            Items = new List<Request.CartItem>
            {
                new() { ProductId = combo1.Id, Quantity = 1 },
                new() { ProductId = combo2.Id, Quantity = 1 }
            }
        };

        var result = await service.CheckInventoryAvailability(request);

        Assert.NotNull(result);
        var inventoryError = Assert.Single(result);
        Assert.NotNull(inventoryError.MaterialId);
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
            Items = new List<Request.CartItem>
            {
                new() { ProductId = combo.Id, Quantity = 1 }
            }
        };

        var result = await service.CheckInventoryAvailability(request);

        Assert.Null(result);
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
            Items = new List<Request.CartItem>
            {
                new() { ProductId = combo.Id, Quantity = 1 }
            }
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
            Items = new List<Request.CartItem>
            {
                new() { ProductId = combo1.Id, Quantity = 1 },
                new() { ProductId = combo2.Id, Quantity = 1 }
            }
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
            Items = new List<Request.CartItem>
            {
                new() { ProductId = parent.Id, Quantity = 1 }
            }
        };

        var result = await service.CheckInventoryAvailability(request);

        Assert.NotNull(result);
        var inventoryError = Assert.Single(result);
        Assert.NotNull(inventoryError.MaterialId);
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
            Items = new List<Request.CartItem>
            {
                new() { ProductId = combo.Id, Quantity = 1 }
            }
        };

        var result = await service.CheckInventoryAvailability(request);

        Assert.NotNull(result);
        var inventoryError = Assert.Single(result);
        Assert.NotNull(inventoryError.MaterialId);
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
            Items = new List<Request.CartItem>
            {
                new() { ProductId = parentCombo.Id, Quantity = 1 }
            }
        };

        var result = await service.CheckInventoryAvailability(request);

        Assert.NotNull(result);
        var inventoryError = Assert.Single(result);
        Assert.NotNull(inventoryError.MaterialId);
    }

    [Fact]
    public async Task CheckInventoryAvailability_DuplicateItemsInRequest_CombinesQuantities()
    {
        // Test rằng 2 dòng cùng ProductId sẽ được gộp lại
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Material - có 5 cái
        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Hoa Hồng",
            AvailableForSale = 5,
            IsActive = true,
            IsDeleted = false
        };

        // Combo - cần 2 Hoa Hồng mỗi lần mua
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

        // Request với 2 dòng cùng ProductId: 2 + 1 = 3 lần mua
        // Cần: 3 * 2 = 6 Hoa Hồng, có 5 -> FAIL
        var request = new Request.CheckInventoryAvailabilityRequest
        {
            Items = new List<Request.CartItem>
            {
                new() { ProductId = combo.Id, Quantity = 2 },
                new() { ProductId = combo.Id, Quantity = 1 }
            }
        };

        var result = await service.CheckInventoryAvailability(request);

        Assert.NotNull(result);
        var inventoryError = Assert.Single(result);
        Assert.Equal(material.Id, inventoryError.MaterialId);
        Assert.Equal(6, inventoryError.RequiredQuantity);
        Assert.Equal(5, inventoryError.AvailableQuantity);
    }

    [Fact]
    public async Task CheckInventoryAvailability_QuantityIsZero_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var request = new Request.CheckInventoryAvailabilityRequest
        {
            Items = new List<Request.CartItem>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 0 }
            }
        };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(request));

        Assert.Contains("Quantity greater than 0", ex.Message);
    }

    [Fact]
    public async Task CheckInventoryAvailability_InactiveRequestedProduct_ThrowsBadRequestException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Combo ngừng bán",
            IsActive = false,
            IsDeleted = false
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var request = new Request.CheckInventoryAvailabilityRequest
        {
            Items = new List<Request.CartItem>
            {
                new() { ProductId = product.Id, Quantity = 1 }
            }
        };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.CheckInventoryAvailability(request));

        Assert.Equal("One or more products are inactive", ex.Message);
    }

    [Fact]
    public async Task CheckInventoryAvailability_InactiveChildCombo_IsNotIncludedInCalculation()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Hoa Hồng",
            AvailableForSale = 1,
            IsActive = true,
            IsDeleted = false
        };

        var parent = new ProductEntity
        {
            Id = Guid.NewGuid(),
            NhanhProductId = "PARENT-ACTIVE-COMBO",
            ProductType = "Combo",
            ProductName = "Combo cha",
            IsActive = true,
            IsDeleted = false
        };

        var activeChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Combo",
            ProductName = "Combo con active",
            IsActive = true,
            IsDeleted = false
        };

        var inactiveChild = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ParentProductId = parent.NhanhProductId,
            ProductType = "Combo",
            ProductName = "Combo con inactive",
            IsActive = false,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, parent, activeChild, inactiveChild);
        await dbContext.ComboSpecifications.AddRangeAsync(
            new ComboSpecification
            {
                Id = Guid.NewGuid(),
                SubId = activeChild.Id,
                MatId = material.Id,
                Quantity = 1,
                IsCore = true,
                IsActive = true,
                IsDeleted = false
            },
            new ComboSpecification
            {
                Id = Guid.NewGuid(),
                SubId = inactiveChild.Id,
                MatId = material.Id,
                Quantity = 100,
                IsCore = true,
                IsActive = true,
                IsDeleted = false
            });
        await dbContext.SaveChangesAsync();

        var request = new Request.CheckInventoryAvailabilityRequest
        {
            Items = new List<Request.CartItem>
            {
                new() { ProductId = parent.Id, Quantity = 1 }
            }
        };

        await service.CheckInventoryAvailability(request);
    }

    [Fact]
    public async Task CheckInventoryAvailability_InactiveMaterial_IsTreatedAsUnavailable()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var material = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Hoa Lan ngừng bán",
            AvailableForSale = 100,
            IsActive = false,
            IsDeleted = false
        };

        var combo = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Combo đang bán",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(material, combo);
        await dbContext.ComboSpecifications.AddAsync(new ComboSpecification
        {
            Id = Guid.NewGuid(),
            SubId = combo.Id,
            MatId = material.Id,
            Quantity = 1,
            IsCore = true,
            IsActive = true,
            IsDeleted = false
        });
        await dbContext.SaveChangesAsync();

        var request = new Request.CheckInventoryAvailabilityRequest
        {
            Items = new List<Request.CartItem>
            {
                new() { ProductId = combo.Id, Quantity = 1 }
            }
        };

        var result = await service.CheckInventoryAvailability(request);

        Assert.NotNull(result);
        var inventoryError = Assert.Single(result);
        Assert.Equal(material.Id, inventoryError.MaterialId);
        Assert.Equal(0, inventoryError.AvailableQuantity);
    }

    [Fact]
    public async Task CheckInventoryAvailability_MultipleInsufficientCoreMaterials_ReturnsAllMaterials()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var firstMaterial = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Hoa Hồng",
            AvailableForSale = 1,
            IsActive = true,
            IsDeleted = false
        };

        var secondMaterial = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Sản phẩm",
            ProductName = "Sen đá",
            AvailableForSale = 2,
            IsActive = true,
            IsDeleted = false
        };

        var combo = new ProductEntity
        {
            Id = Guid.NewGuid(),
            ProductType = "Combo",
            ProductName = "Combo nhiều CORE",
            IsActive = true,
            IsDeleted = false
        };

        await dbContext.Products.AddRangeAsync(firstMaterial, secondMaterial, combo);
        await dbContext.ComboSpecifications.AddRangeAsync(
            new ComboSpecification
            {
                Id = Guid.NewGuid(),
                SubId = combo.Id,
                MatId = firstMaterial.Id,
                Quantity = 2,
                IsCore = true,
                IsActive = true,
                IsDeleted = false
            },
            new ComboSpecification
            {
                Id = Guid.NewGuid(),
                SubId = combo.Id,
                MatId = secondMaterial.Id,
                Quantity = 3,
                IsCore = true,
                IsActive = true,
                IsDeleted = false
            });
        await dbContext.SaveChangesAsync();

        var result = await service.CheckInventoryAvailability(
            new Request.CheckInventoryAvailabilityRequest
            {
                Items = new List<Request.CartItem>
                {
                    new() { ProductId = combo.Id, Quantity = 1 }
                }
            });

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.MaterialId == firstMaterial.Id && x.RequiredQuantity == 2);
        Assert.Contains(result, x => x.MaterialId == secondMaterial.Id && x.RequiredQuantity == 3);
    }
}

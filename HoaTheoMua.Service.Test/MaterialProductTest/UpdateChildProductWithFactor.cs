using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ComboSpecEntity = HoaTheoMua.Repository.Entity.ComboSpecification;
using MaterialRequest = HoaTheoMua.Service.Material.Request;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;

namespace HoaTheoMua.Service.Test.MaterialProductTest;

public class UpdateChildProductWithFactor
{
    private static readonly Guid ComboId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ChildLargeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid MatCoreId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid MatSupportId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private const string ParentNhanhProductId = "nhanh-combo-a";
    private const string MaterialLabel = "Thành phần";

    [Fact]
    public async Task UpdateChildProductWithFactor_ValidFactor_UpdatesChildQuantitiesFromParentRecipe()
    {
        await using var dbContext = CreateDbContext();
        SeedParentAndChildRecipe(dbContext);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.UpdateMaterialChildRequest
        {
            ProductId = ChildLargeId,
            Factor = 1.5m
        };

        var result = await service.UpdateChildProductWithFactor(request);

        Assert.Equal("Cập nhật hệ số và định lượng sản phẩm con thành công", result);

        var childProduct = await dbContext.Products.FirstAsync(x => x.Id == ChildLargeId);
        var childCore = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildLargeId && x.MatId == MatCoreId);
        var childSupport = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildLargeId && x.MatId == MatSupportId);

        Assert.Equal(1.5m, childProduct.Factor);
        Assert.Equal(7, childCore.Quantity);
        Assert.Equal(3, childSupport.Quantity);
        Assert.NotNull(childCore.UpdatedAt);
        Assert.NotNull(childSupport.UpdatedAt);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_NullFactor_ClearsChildQuantities()
    {
        await using var dbContext = CreateDbContext();
        SeedParentAndChildRecipe(dbContext);
        await dbContext.SaveChangesAsync();

        var result = await CreateService(dbContext).UpdateChildProductWithFactor(
            new MaterialRequest.UpdateMaterialChildRequest
            {
                ProductId = ChildLargeId,
                Factor = null
            });

        Assert.Equal("Cập nhật hệ số và định lượng sản phẩm con thành công", result);

        var childProduct = await dbContext.Products.FirstAsync(x => x.Id == ChildLargeId);
        var childSpecifications = await dbContext.ComboSpecifications
            .Where(x => x.SubId == ChildLargeId && !x.IsDeleted)
            .ToListAsync();

        Assert.Null(childProduct.Factor);
        // AvailableForSale is not recalculated by UpdateChildProductWithFactor.
        Assert.Equal(2, childSpecifications.Count);
        Assert.All(childSpecifications, x => Assert.Null(x.Quantity));
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_AppliesUnitAndAuxiliaryRules()
    {
        await using var dbContext = CreateDbContext();
        var bunchId = Guid.NewGuid();
        var auxiliaryId = Guid.NewGuid();

        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId, unit: "cành"),
            CreateMaterial(bunchId, unit: " BÓ "),
            CreateMaterial(auxiliaryId, productLabel: " Phụ liệu ", unit: "cành"));
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ComboId, bunchId, quantity: 2, isCore: false),
            CreateSpecification(ComboId, auxiliaryId, quantity: 3, isCore: false));
        await dbContext.SaveChangesAsync();

        var result = await CreateService(dbContext).UpdateChildProductWithFactor(new MaterialRequest.UpdateMaterialChildRequest
        {
            ProductId = ChildLargeId,
            Factor = 1.25m
        });

        Assert.Equal("Cập nhật hệ số và định lượng sản phẩm con thành công", result);
        Assert.Equal(6m, (await dbContext.ComboSpecifications.FirstAsync(x => x.SubId == ChildLargeId && x.MatId == MatCoreId)).Quantity);
        Assert.Equal(2.5m, (await dbContext.ComboSpecifications.FirstAsync(x => x.SubId == ChildLargeId && x.MatId == bunchId)).Quantity);
        Assert.Equal(3m, (await dbContext.ComboSpecifications.FirstAsync(x => x.SubId == ChildLargeId && x.MatId == auxiliaryId)).Quantity);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_SynchronizesRecipe()
    {
        await using var dbContext = CreateDbContext();
        var accessoryId = Guid.NewGuid();
        var orphanId = Guid.NewGuid();
        var coreMaterial = CreateMaterial(MatCoreId, unit: "cành");
        coreMaterial.AvailableForSale = 100m;

        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            coreMaterial,
            CreateMaterial(accessoryId, productLabel: "Phụ liệu", unit: "cành"),
            CreateMaterial(orphanId));
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ComboId, accessoryId, quantity: 3, isCore: false),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildLargeId, accessoryId, quantity: 3, isCore: false),
            CreateSpecification(ChildLargeId, orphanId, quantity: 1, isCore: false));
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).UpdateChildProductWithFactor(new MaterialRequest.UpdateMaterialChildRequest
        {
            ProductId = ChildLargeId,
            Factor = 2m
        });

        var childProduct = await dbContext.Products.FirstAsync(x => x.Id == ChildLargeId);
        var childCore = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildLargeId && x.MatId == MatCoreId);
        var childAccessory = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildLargeId && x.MatId == accessoryId);
        var orphan = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildLargeId && x.MatId == orphanId);

        Assert.Equal(10m, childCore.Quantity);
        Assert.Equal(3m, childAccessory.Quantity);
        Assert.True(orphan.IsDeleted);
        // AvailableForSale is not recalculated by UpdateChildProductWithFactor.
        Assert.NotNull(childProduct.UpdatedAt);
    }

    [Fact]
    //factor phải > 0 mới hợp lệ
    public async Task UpdateChildProductWithFactor_InvalidFactor_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.UpdateMaterialChildRequest
        {
            ProductId = ChildLargeId,
            Factor = 0m
        };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateChildProductWithFactor(request));

        Assert.Equal("Hệ số phải lớn hơn 0", ex.Message);
        Assert.Empty(dbContext.Products);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_ChildProductNotFound_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(dbContext).UpdateChildProductWithFactor(
                new MaterialRequest.UpdateMaterialChildRequest
                {
                    ProductId = ChildLargeId,
                    Factor = 1m
                }));

        Assert.Equal("Không tìm thấy sản phẩm", exception.Message);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_ParentComboProduct_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(CreateParentCombo());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.UpdateMaterialChildRequest
        {
            ProductId = ComboId,
            Factor = 1.2m
        };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateChildProductWithFactor(request));

        Assert.Equal("Sản phẩm không phải là combo con (không có sản phẩm cha)", ex.Message);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_ParentProductNotFound_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(CreateChildProduct(ChildLargeId, "combo-a-l"));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(dbContext).UpdateChildProductWithFactor(
                new MaterialRequest.UpdateMaterialChildRequest
                {
                    ProductId = ChildLargeId,
                    Factor = 1m
                }));

        Assert.Equal("Không tìm thấy sản phẩm cha", exception.Message);
    }

    [Fact]
    //xem lại
    public async Task UpdateChildProductWithFactor_ParentRecipeMissing_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId));
        dbContext.ComboSpecifications.Add(
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.UpdateMaterialChildRequest
        {
            ProductId = ChildLargeId,
            Factor = 1.5m
        };

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateChildProductWithFactor(request));

        Assert.Equal("Không tìm thấy công thức combo cha", ex.Message);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_InvalidParentQuantity_ThrowsBadRequestWithoutChangingChild()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId));
        dbContext.ComboSpecifications.AddRange(
            new ComboSpecEntity
            {
                Id = Guid.NewGuid(),
                SubId = ComboId,
                MatId = MatCoreId,
                Quantity = null,
                IsCore = true,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            },
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).UpdateChildProductWithFactor(
                new MaterialRequest.UpdateMaterialChildRequest
                {
                    ProductId = ChildLargeId,
                    Factor = 2m
                }));

        Assert.Equal(
            $"Công thức của combo cha có số lượng không hợp lệ: Material {MatCoreId:N}",
            exception.Message);
        Assert.Null((await dbContext.Products.FindAsync(ChildLargeId))!.Factor);
        Assert.Equal(
            5m,
            (await dbContext.ComboSpecifications.SingleAsync(x => x.SubId == ChildLargeId)).Quantity);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_ParentRecipeWithoutActiveCore_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId));
        dbContext.ComboSpecifications.Add(CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: false));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).UpdateChildProductWithFactor(
                new MaterialRequest.UpdateMaterialChildRequest
                {
                    ProductId = ChildLargeId,
                    Factor = 2m
                }));

        Assert.Equal("Công thức của combo cha không còn vật liệu chính (core) đang hoạt động", exception.Message);
        Assert.Null((await dbContext.Products.FindAsync(ChildLargeId))!.Factor);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_ChildMissingParentMaterial_CreatesMissingChildSpecification()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId),
            CreateMaterial(MatSupportId));
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ComboId, MatSupportId, quantity: 2, isCore: false),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true)); 
        //con đang thiếu support, rào trước cho trường hợp người dùng admin vô tình xóa lộn dòng nào trong DB thì ném lỗi này
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.UpdateMaterialChildRequest
        {
            ProductId = ChildLargeId,
            Factor = 1.5m
        };

        var result = await service.UpdateChildProductWithFactor(request);

        Assert.Equal("Cập nhật hệ số và định lượng sản phẩm con thành công", result);

        var childCore = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildLargeId && x.MatId == MatCoreId);
        var childSupport = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildLargeId && x.MatId == MatSupportId);

        Assert.Equal(7, childCore.Quantity);
        Assert.Equal(3, childSupport.Quantity);
        Assert.True(childCore.IsCore);
        Assert.False(childSupport.IsCore);
        Assert.True(childSupport.IsActive);
    }

    [Fact]
    public async Task UpdateChildProductWithFactor_SoftDeletedChildSpecification_CreatesNewActiveRow()
    {
        await using var dbContext = CreateDbContext();
        SeedParentAndChildRecipe(dbContext);
        await dbContext.SaveChangesAsync();

        var deletedSpecification = await dbContext.ComboSpecifications.SingleAsync(
            x => x.SubId == ChildLargeId && x.MatId == MatCoreId);
        deletedSpecification.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).UpdateChildProductWithFactor(
            new MaterialRequest.UpdateMaterialChildRequest
            {
                ProductId = ChildLargeId,
                Factor = 2m
            });

        var specifications = await dbContext.ComboSpecifications
            .Where(x => x.SubId == ChildLargeId && x.MatId == MatCoreId)
            .ToListAsync();
        Assert.Equal(2, specifications.Count);
        Assert.True(specifications.Single(x => x.Id == deletedSpecification.Id).IsDeleted);
        var activeSpecification = specifications.Single(x => !x.IsDeleted);
        Assert.NotEqual(deletedSpecification.Id, activeSpecification.Id);
        Assert.Equal(10m, activeSpecification.Quantity);
    }

    [Fact]
    //confirm => Không cho cập nhật quantity về 0 VD: 1 * 0.5 = 0.5 -> floor -> 0 -> INVALID_QUANTITY
    public async Task UpdateChildProductWithFactor_FactorMakesQuantityZero_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId));
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 1, isCore: true),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 1, isCore: true));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.UpdateMaterialChildRequest
        {
            ProductId = ChildLargeId,
            Factor = 0.5m
        };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateChildProductWithFactor(request));

        Assert.Equal($"Số lượng vật liệu sau khi nhân hệ số bị giảm về 0, vui lòng kiểm tra lại hệ số: Material {MatCoreId:N}", ex.Message);
    }

    private static void SeedParentAndChildRecipe(AppDbContext dbContext)
    {
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId),
            CreateMaterial(MatSupportId));

        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ComboId, MatSupportId, quantity: 2, isCore: false),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildLargeId, MatSupportId, quantity: 2, isCore: false));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static HoaTheoMua.Service.Material.Service CreateService(AppDbContext dbContext)
    {
        return new HoaTheoMua.Service.Material.Service(dbContext);
    }

    private static ProductEntity CreateParentCombo()
    {
        return new ProductEntity
        {
            Id = ComboId,
            NhanhProductId = ParentNhanhProductId,
            ParentProductId = null,
            ProductName = "Combo A",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static ProductEntity CreateChildProduct(Guid id, string nhanhProductId)
    {
        return new ProductEntity
        {
            Id = id,
            NhanhProductId = nhanhProductId,
            ParentProductId = ParentNhanhProductId,
            ProductName = $"Combo child {nhanhProductId}",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static ProductEntity CreateMaterial(Guid id, string productLabel = MaterialLabel, string? unit = null)
    {
        return new ProductEntity
        {
            Id = id,
            NhanhProductId = $"mat-{id:N}",
            ProductLabel = new List<string> { productLabel },
            Unit = unit,
            ProductName = $"Material {id:N}",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static ComboSpecEntity CreateSpecification(Guid subId, Guid matId, int quantity, bool isCore)
    {
        return new ComboSpecEntity
        {
            Id = Guid.NewGuid(),
            SubId = subId,
            MatId = matId,
            Quantity = quantity,
            IsCore = isCore,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}


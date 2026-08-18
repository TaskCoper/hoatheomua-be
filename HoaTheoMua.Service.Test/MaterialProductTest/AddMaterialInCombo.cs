using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ComboSpecEntity = HoaTheoMua.Repository.Entity.ComboSpecification;
using MaterialRequest = HoaTheoMua.Service.Material.Request;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;

namespace HoaTheoMua.Service.Test.MaterialProductTest;

public class AddMaterialInCombo
{
    private static readonly Guid ComboId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ChildSmallId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ChildLargeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid MatCoreId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid MatSupportId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private const string ParentNhanhProductId = "nhanh-combo-a";
    private const string MaterialLabel = "Thành phần";

    [Fact]
    public async Task AddMaterialInCombo_ValidMaterial_AddsSpecificationForParentAndChildren()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.MaterialComboRequest
        {
            Items =
            [
                new MaterialRequest.ComboSpecification
                {
                    MatId = MatCoreId,
                    Quantity = 10,
                    IsCore = true,
                    IsActive = true
                }
            ]
        };

        var result = await service.AddMaterialInCombo(ComboId, request);

        Assert.Equal("Lưu công thức thành công.", result);

        var specifications = await dbContext.ComboSpecifications
            .Where(x => x.MatId == MatCoreId)
            .ToListAsync();

        Assert.Equal(3, specifications.Count);
        Assert.Contains(specifications, x => x.SubId == ComboId);
        Assert.Contains(specifications, x => x.SubId == ChildSmallId);
        Assert.Contains(specifications, x => x.SubId == ChildLargeId);
        Assert.All(specifications, x =>
        {
            Assert.Equal(10, x.Quantity);
            Assert.True(x.IsCore);
            Assert.True(x.IsActive);
        });
    }

    [Fact]
    public async Task AddMaterialInCombo_BranchUnit_FloorsParentAndChildQuantity()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateMaterial(MatCoreId, unit: "cành"));
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).AddMaterialInCombo(
            ComboId,
            CreateRequest(MatCoreId, quantity: 1.5m, isCore: true));

        var parentSpecification = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ComboId && x.MatId == MatCoreId);
        var childSpecification = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildSmallId && x.MatId == MatCoreId);

        Assert.Equal(1m, parentSpecification.Quantity);
        Assert.Equal(1m, childSpecification.Quantity);
    }
    [Fact]
    public async Task AddMaterialInCombo_ChildWithFactor_MultipliesChildQuantity()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s", factor: 1.5m),
            CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatCoreId, quantity: 10, isCore: true);

        await service.AddMaterialInCombo(ComboId, request);

        var parentSpecification = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ComboId && x.MatId == MatCoreId);
        var childSpecification = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildSmallId && x.MatId == MatCoreId);

        Assert.Equal(10, parentSpecification.Quantity);
        Assert.Equal(15, childSpecification.Quantity);
    }

    [Fact]
    public async Task AddMaterialInCombo_AuxiliaryMaterial_KeepsParentQuantityForChildWithFactor()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s", factor: 1.5m),
            CreateMaterial(MatSupportId, productLabel: "Phụ liệu"));
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).AddMaterialInCombo(
            ComboId,
            CreateRequest(MatSupportId, quantity: 2, isCore: true));

        var childSpecification = await dbContext.ComboSpecifications
            .FirstAsync(x => x.SubId == ChildSmallId && x.MatId == MatSupportId);

        Assert.Equal(2m, childSpecification.Quantity);
    }
    [Fact]
    public async Task AddMaterialInCombo_ComboNotFound_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatCoreId, quantity: 10, isCore: true);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.AddMaterialInCombo(ComboId, request));

        Assert.Equal("COMBO_NOT_FOUND", ex.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_InvalidQuantity_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(CreateParentCombo(), CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatCoreId, quantity: 0, isCore: true);

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.AddMaterialInCombo(ComboId, request));

        Assert.Equal("INVALID_QUANTITY", ex.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_MaterialNotFoundOrWrongLabel_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateMaterial(MatCoreId, productLabel: "Combo"));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatCoreId, quantity: 2, isCore: true);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.AddMaterialInCombo(ComboId, request));

        Assert.Equal("MATERIAL_NOT_FOUND", ex.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    //Thêm 1 vật liệu vào công thức, nếu hiện tại combo ko có core, thì thêm vào vật liệu bắt buộc phải là core
    public async Task AddMaterialInCombo_NoActiveCoreInRequestOrExistingRecipe_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(CreateParentCombo(), CreateMaterial(MatSupportId));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatSupportId, quantity: 2, isCore: false, isActive: true);

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.AddMaterialInCombo(ComboId, request));

        Assert.Equal("CORE_MATERIAL_REQUIRED", ex.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_ExistingActiveCore_AllowsAddingSupportMaterial()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateMaterial(MatCoreId),
            CreateMaterial(MatSupportId));
        dbContext.ComboSpecifications.Add(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatSupportId, quantity: 2, isCore: false, isActive: true);

        var result = await service.AddMaterialInCombo(ComboId, request);

        Assert.Equal("Lưu công thức thành công.", result);
        Assert.Contains(
            dbContext.ComboSpecifications,
            x => x.SubId == ComboId &&
                 x.MatId == MatSupportId &&
                 x.Quantity == 2 &&
                 !x.IsCore &&
                 x.IsActive);
    }

    private static MaterialRequest.MaterialComboRequest CreateRequest(
        Guid matId,
        decimal quantity,
        bool isCore,
        bool isActive = true)
    {
        return new MaterialRequest.MaterialComboRequest
        {
            Items =
            [
                new MaterialRequest.ComboSpecification
                {
                    MatId = matId,
                    Quantity = quantity,
                    IsCore = isCore,
                    IsActive = isActive
                }
            ]
        };
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

    private static ProductEntity CreateChildProduct(Guid id, string nhanhProductId, decimal? factor = null)
    {
        return new ProductEntity
        {
            Id = id,
            NhanhProductId = nhanhProductId,
            ParentProductId = ParentNhanhProductId,
            Factor = factor,
            ProductName = $"Combo child {nhanhProductId}",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static ProductEntity CreateMaterial(Guid id, string? productLabel = MaterialLabel, string? unit = null)
    {
        return new ProductEntity
        {
            Id = id,
            NhanhProductId = $"mat-{id:N}",
            ProductLabel = productLabel,
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






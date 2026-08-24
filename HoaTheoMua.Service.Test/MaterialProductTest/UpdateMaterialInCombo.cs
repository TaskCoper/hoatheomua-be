using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ComboSpecEntity = HoaTheoMua.Repository.Entity.ComboSpecification;
using MaterialRequest = HoaTheoMua.Service.Material.Request;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;

namespace HoaTheoMua.Service.Test.MaterialProductTest;

public class UpdateMaterialInCombo
{
    private static readonly Guid ComboId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ChildSmallId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ChildLargeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid MatCoreId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid MatSupportId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid MissingMatId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    private const string ParentNhanhProductId = "nhanh-combo-a";
    private const string MaterialLabel = "Thành phần";

    [Fact]
    public async Task UpdateMaterialInCombo_ValidMaterial_UpdatesParentAndChildren()
    {
        await using var dbContext = CreateDbContext();
        SeedParentChildrenAndMaterials(dbContext);
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: false),
            CreateSpecification(ChildSmallId, MatCoreId, quantity: 5, isCore: false),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: false));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatCoreId, quantity: 10, isCore: true, isActive: true);

        var result = await service.UpdateMaterialInCombo(ComboId, request);

        Assert.Equal("Cập nhật vật liệu trong công thức thành công.", result);

        var specifications = await dbContext.ComboSpecifications
            .Where(x => x.MatId == MatCoreId)
            .ToListAsync();

        Assert.Equal(3, specifications.Count);
        Assert.Equal(10m, specifications.Single(x => x.SubId == ComboId).Quantity);
        Assert.Null(specifications.Single(x => x.SubId == ChildSmallId).Quantity);
        Assert.Null(specifications.Single(x => x.SubId == ChildLargeId).Quantity);
        Assert.All(specifications, x =>
        {
            Assert.True(x.IsCore);
            Assert.True(x.IsActive);
        });
        Assert.All(specifications.Where(x => x.SubId != ComboId), x => Assert.NotNull(x.UpdatedAt));
    }

    [Fact]
    public async Task UpdateMaterialInCombo_EmptyItems_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(CreateParentCombo());
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.MaterialComboRequest { Items = [] };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateMaterialInCombo(ComboId, request));

        Assert.Equal("INVALID_INPUT", ex.Message);
    }

    [Fact]
    //cập 1 số lượng sản phẩm thành 0
    public async Task UpdateMaterialInCombo_InvalidQuantity_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        SeedParentChildrenAndMaterials(dbContext);
        dbContext.ComboSpecifications.Add(CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatCoreId, quantity: 0, isCore: true);

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateMaterialInCombo(ComboId, request));

        Assert.Equal("INVALID_QUANTITY", ex.Message);
    }

    [Fact]
    public async Task UpdateMaterialInCombo_MaterialNotInParentRecipe_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext();
        SeedParentChildrenAndMaterials(dbContext);
        dbContext.ComboSpecifications.Add(CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MissingMatId, quantity: 10, isCore: true);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateMaterialInCombo(ComboId, request));

        Assert.Equal("MATERIAL_NOT_FOUND", ex.Message);
    }

    [Fact]
    public async Task UpdateMaterialInCombo_ChildMissingMaterial_CreatesMissingChildSpecification()
    {
        await using var dbContext = CreateDbContext();
        SeedParentChildrenAndMaterials(dbContext);
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: false),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: false),
            CreateSpecification(ComboId, MatSupportId, quantity: 3, isCore: true), 
            //thiếu thằng ChildSmallId và MatSupportId vì có 1 thằng vô tình bị xóa do 1 trường hợp vô tình nào đó
            CreateSpecification(ChildSmallId, MatSupportId, quantity: 3, isCore: true),
            CreateSpecification(ChildLargeId, MatSupportId, quantity: 3, isCore: true));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatCoreId, quantity: 10, isCore: true);

        var result = await service.UpdateMaterialInCombo(ComboId, request);

        Assert.Equal("Cập nhật vật liệu trong công thức thành công.", result);

        var childSpecification = await dbContext.ComboSpecifications
            .SingleAsync(x => x.SubId == ChildSmallId && x.MatId == MatCoreId && !x.IsDeleted);

        Assert.Null(childSpecification.Quantity);
        Assert.True(childSpecification.IsCore);
        Assert.True(childSpecification.IsActive);
    }

    [Fact]
    public async Task UpdateMaterialInCombo_ChildWithFactor_UpdatesChildQuantityFromParent()
    {
        await using var dbContext = CreateDbContext();
        SeedParentChildrenAndMaterials(dbContext);
        dbContext.Products.Local.Single(x => x.Id == ChildSmallId).Factor = 1.5m;
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildSmallId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).UpdateMaterialInCombo(
            ComboId,
            CreateRequest(MatCoreId, quantity: 10, isCore: true));

        Assert.Equal(
            10m,
            (await dbContext.ComboSpecifications.SingleAsync(x => x.SubId == ComboId)).Quantity);
        Assert.Equal(
            15m,
            (await dbContext.ComboSpecifications.SingleAsync(x => x.SubId == ChildSmallId)).Quantity);
        Assert.Null(
            (await dbContext.ComboSpecifications.SingleAsync(x => x.SubId == ChildLargeId)).Quantity);
    }

    [Fact]
    public async Task UpdateMaterialInCombo_NoActiveCoreAfterUpdate_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        SeedParentChildrenAndMaterials(dbContext);
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildSmallId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatCoreId, quantity: 5, isCore: false, isActive: true);

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.UpdateMaterialInCombo(ComboId, request));

        Assert.Equal("CORE_MATERIAL_REQUIRED", ex.Message);

        var specifications = await dbContext.ComboSpecifications
            .Where(x => x.MatId == MatCoreId && !x.IsDeleted)
            .ToListAsync();

        Assert.Equal(3, specifications.Count);
        Assert.All(specifications, x =>
        {
            Assert.True(x.IsCore);
            Assert.True(x.IsActive);
            Assert.Null(x.UpdatedAt);
        });
    }

    private static void SeedParentChildrenAndMaterials(AppDbContext dbContext)
    {
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId),
            CreateMaterial(MatSupportId));
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

    private static ProductEntity CreateMaterial(Guid id)
    {
        return new ProductEntity
        {
            Id = id,
            NhanhProductId = $"mat-{id:N}",
            ProductLabel = MaterialLabel,
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


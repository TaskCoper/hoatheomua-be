using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ComboSpecEntity = HoaTheoMua.Repository.Entity.ComboSpecification;
using MaterialRequest = HoaTheoMua.Service.Material.Request;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;

namespace HoaTheoMua.Service.Test.MaterialProductTest;

public class DeleteMaterialInCombo
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
    public async Task DeleteMaterialInCombo_ValidSupportMaterial_SoftDeletesParentAndChildrenSpecifications()
    {
        await using var dbContext = CreateDbContext();
        SeedComboRecipe(dbContext);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.DeleteMaterial
        {
            Materials = [MatSupportId]
        };

        var result = await service.DeleteMaterialInCombo(ComboId, request);

        Assert.Equal("Xóa vật liệu khỏi công thức thành công", result);

        var deletedSpecifications = await dbContext.ComboSpecifications
            .Where(x => x.MatId == MatSupportId)
            .ToListAsync();

        Assert.Equal(3, deletedSpecifications.Count);
        Assert.All(deletedSpecifications, x => Assert.True(x.IsDeleted));
        Assert.All(
            dbContext.ComboSpecifications.Where(x => x.MatId == MatCoreId),
            x => Assert.False(x.IsDeleted));
    }

    [Fact]
    public async Task DeleteMaterialInCombo_EmptyMaterials_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        SeedComboRecipe(dbContext);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.DeleteMaterial { Materials = [] };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.DeleteMaterialInCombo(ComboId, request));

        Assert.Equal("INVALID_INPUT", ex.Message);
        Assert.All(dbContext.ComboSpecifications, x => Assert.False(x.IsDeleted));
    }

    [Fact]
    public async Task DeleteMaterialInCombo_MaterialNotInParentRecipe_ThrowsNotFound()
    {
        await using var dbContext = CreateDbContext();
        SeedComboRecipe(dbContext);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.DeleteMaterial
        {
            Materials = [MissingMatId]
        };

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteMaterialInCombo(ComboId, request));

        Assert.Equal("MATERIAL_NOT_FOUND_IN_RECIPE", ex.Message);
        Assert.All(dbContext.ComboSpecifications, x => Assert.False(x.IsDeleted));
    }

    [Fact]
    public async Task DeleteMaterialInCombo_LastCoreMaterial_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId));
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildSmallId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = new MaterialRequest.DeleteMaterial
        {
            Materials = [MatCoreId]
        };

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.DeleteMaterialInCombo(ComboId, request));

        Assert.Equal("CORE_MATERIAL_REQUIRED", ex.Message);
        Assert.All(dbContext.ComboSpecifications, x => Assert.False(x.IsDeleted));
    }

    [Fact]
    public async Task DeleteMaterialInCombo_NoSizeCombo_DeletesMaterialFromParentOnly()
    {
        await using var dbContext = CreateDbContext();
        var parentCombo = CreateParentCombo();
        parentCombo.ProductType = "Combo";

        dbContext.Products.AddRange(
            parentCombo,
            CreateMaterial(MatCoreId),
            CreateMaterial(MatSupportId));
        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ComboId, MatSupportId, quantity: 2, isCore: false));
        await dbContext.SaveChangesAsync();

        var result = await CreateService(dbContext).DeleteMaterialInCombo(
            ComboId,
            new MaterialRequest.DeleteMaterial { Materials = [MatSupportId] });

        Assert.Equal("Xóa vật liệu khỏi công thức thành công", result);

        var specifications = await dbContext.ComboSpecifications.ToListAsync();
        Assert.Equal(2, specifications.Count);
        Assert.True(specifications.Single(x => x.MatId == MatSupportId).IsDeleted);
        Assert.False(specifications.Single(x => x.MatId == MatCoreId).IsDeleted);
    }

    [Fact]
    public async Task DeleteMaterialInCombo_NoSizeCombo_LastCoreMaterial_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        var parentCombo = CreateParentCombo();
        parentCombo.ProductType = "Combo";

        dbContext.Products.AddRange(
            parentCombo,
            CreateMaterial(MatCoreId));
        dbContext.ComboSpecifications.Add(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).DeleteMaterialInCombo(
                ComboId,
                new MaterialRequest.DeleteMaterial { Materials = [MatCoreId] }));

        Assert.Equal("CORE_MATERIAL_REQUIRED", exception.Message);
        Assert.False((await dbContext.ComboSpecifications.SingleAsync()).IsDeleted);
    }

    private static void SeedComboRecipe(AppDbContext dbContext)
    {
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateChildProduct(ChildLargeId, "combo-a-l"),
            CreateMaterial(MatCoreId),
            CreateMaterial(MatSupportId));

        dbContext.ComboSpecifications.AddRange(
            CreateSpecification(ComboId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildSmallId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ChildLargeId, MatCoreId, quantity: 5, isCore: true),
            CreateSpecification(ComboId, MatSupportId, quantity: 2, isCore: false),
            CreateSpecification(ChildSmallId, MatSupportId, quantity: 2, isCore: false),
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

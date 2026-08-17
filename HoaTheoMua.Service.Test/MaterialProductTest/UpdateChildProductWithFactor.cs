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

        Assert.Equal("INVALID_FACTOR", ex.Message);
        Assert.Empty(dbContext.Products);
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

        Assert.Equal("PARENT_PRODUCT_REQUIRED", ex.Message);
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

        Assert.Equal("PARENT_COMBO_SPECIFICATION_NOT_FOUND", ex.Message);
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

        Assert.Equal("INVALID_QUANTITY", ex.Message);
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

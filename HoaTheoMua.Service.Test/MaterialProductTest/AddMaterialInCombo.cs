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
    private const string MaterialLabel = "Vật liệu";

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
        Assert.Equal(10m, specifications.Single(x => x.SubId == ComboId).Quantity);
        Assert.Null(specifications.Single(x => x.SubId == ChildSmallId).Quantity);
        Assert.Null(specifications.Single(x => x.SubId == ChildLargeId).Quantity);
        Assert.All(specifications, x =>
        {
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
        Assert.Null(childSpecification.Quantity);
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
    public async Task AddMaterialInCombo_ChildWithFactor_FloorsNonDecimalQuantity()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s", factor: 1.5m),
            CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).AddMaterialInCombo(
            ComboId,
            CreateRequest(MatCoreId, quantity: 3, isCore: true));

        Assert.Equal(
            3m,
            (await dbContext.ComboSpecifications.SingleAsync(x => x.SubId == ComboId)).Quantity);
        Assert.Equal(
            4m,
            (await dbContext.ComboSpecifications.SingleAsync(x => x.SubId == ChildSmallId)).Quantity);
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

        Assert.Equal("Không tìm thấy combo", ex.Message);
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

        Assert.Equal("Số lượng vật liệu phải lớn hơn 0", ex.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_EmptyItems_ThrowsBadRequestWithoutWriting()
    {
        await using var dbContext = CreateDbContext();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ComboId,
                new MaterialRequest.MaterialComboRequest { Items = [] }));

        Assert.Equal("Danh sách vật liệu không được để trống", exception.Message);
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

        Assert.Equal("Vật liệu không tồn tại hoặc không hợp lệ", ex.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_SupportOnlyRecipeWithoutCore_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateMaterial(MatSupportId));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var request = CreateRequest(MatSupportId, quantity: 2, isCore: false, isActive: true);

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.AddMaterialInCombo(ComboId, request));

        Assert.Equal("Công thức phải có ít nhất một vật liệu chính (core) đang hoạt động", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_ExistingActiveCore_AllowsAddingSupportMaterial()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
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

    [Fact]
    public async Task AddMaterialInCombo_ComboWithoutChildren_ThrowsBadRequestWithoutWriting()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(CreateParentCombo(), CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ComboId,
                CreateRequest(MatCoreId, quantity: 3.5m, isCore: true)));

        Assert.Equal("Combo phải có ít nhất một biến thể (combo con)", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_NoSizeCombo_AddsParentSpecificationOnly()
    {
        await using var dbContext = CreateDbContext();
        var parentCombo = CreateParentCombo();
        parentCombo.ProductType = " Combo ";
        dbContext.Products.AddRange(parentCombo, CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var result = await CreateService(dbContext).AddMaterialInCombo(
            ComboId,
            CreateRequest(MatCoreId, quantity: 3, isCore: true));

        Assert.Equal("Lưu công thức thành công.", result);
        var specification = await dbContext.ComboSpecifications.SingleAsync();
        Assert.Equal(ComboId, specification.SubId);
        Assert.Equal(MatCoreId, specification.MatId);
        Assert.Equal(3m, specification.Quantity);
        Assert.True(specification.IsCore);
        Assert.True(specification.IsActive);
    }

    [Theory]
    [InlineData("bó")]
    [InlineData("giỏ")]
    [InlineData("chậu")]
    public async Task AddMaterialInCombo_DecimalUnit_PreservesDecimalQuantity(string unit)
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s", factor: 1.5m),
            CreateMaterial(MatCoreId, unit: unit));
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).AddMaterialInCombo(
            ComboId,
            CreateRequest(MatCoreId, quantity: 1.5m, isCore: true));

        Assert.Equal(
            1.5m,
            (await dbContext.ComboSpecifications.SingleAsync(x => x.SubId == ComboId)).Quantity);
        Assert.Equal(
            2.25m,
            (await dbContext.ComboSpecifications.SingleAsync(x => x.SubId == ChildSmallId)).Quantity);
    }

    [Fact]
    public async Task AddMaterialInCombo_DuplicateMaterialInRequest_ThrowsConflictWithoutWriting()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(CreateParentCombo(), CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var request = new MaterialRequest.MaterialComboRequest
        {
            Items =
            [
                new MaterialRequest.ComboSpecification { MatId = MatCoreId, Quantity = 1 },
                new MaterialRequest.ComboSpecification { MatId = MatCoreId, Quantity = 2 }
            ]
        };

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService(dbContext).AddMaterialInCombo(ComboId, request));

        Assert.Equal("Danh sách vật liệu bị trùng lặp", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_ActiveParentSpecificationAlreadyExists_ThrowsConflictWithoutUpdating()
    {
        await using var dbContext = CreateDbContext();
        var existing = CreateSpecification(
            ComboId,
            MatCoreId,
            quantity: 7,
            isCore: false,
            isActive: false);
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateMaterial(MatCoreId));
        dbContext.ComboSpecifications.Add(existing);
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ComboId,
                CreateRequest(MatCoreId, quantity: 10, isCore: true)));

        Assert.Equal("Vật liệu đã có trong công thức của combo", exception.Message);
        Assert.Single(dbContext.ComboSpecifications);
        Assert.Equal(7, existing.Quantity);
        Assert.False(existing.IsCore);
        Assert.False(existing.IsActive);
    }

    [Fact]
    public async Task AddMaterialInCombo_SoftDeletedSpecifications_CreatesNewActiveRows()
    {
        await using var dbContext = CreateDbContext();
        var deletedParent = CreateSpecification(
            ComboId,
            MatCoreId,
            quantity: 99,
            isCore: false,
            isActive: false,
            isDeleted: true);
        var deletedChild = CreateSpecification(
            ChildSmallId,
            MatCoreId,
            quantity: 99,
            isCore: false,
            isActive: false,
            isDeleted: true);
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s", factor: 2m),
            CreateMaterial(MatCoreId));
        dbContext.ComboSpecifications.AddRange(deletedParent, deletedChild);
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).AddMaterialInCombo(
            ComboId,
            CreateRequest(MatCoreId, quantity: 3, isCore: true));

        var specifications = await dbContext.ComboSpecifications
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SubId)
            .ToListAsync();
        Assert.Equal(2, specifications.Count);
        Assert.DoesNotContain(specifications, x => x.Id == deletedParent.Id || x.Id == deletedChild.Id);
        Assert.Equal(4, await dbContext.ComboSpecifications.CountAsync());
        Assert.All(
            await dbContext.ComboSpecifications.Where(x => x.IsDeleted).ToListAsync(),
            x => Assert.Contains(x.Id, new[] { deletedParent.Id, deletedChild.Id }));
        Assert.All(specifications, x =>
        {
            Assert.False(x.IsDeleted);
            Assert.True(x.IsCore);
            Assert.True(x.IsActive);
        });
        Assert.Equal(3m, specifications.Single(x => x.SubId == ComboId).Quantity);
        Assert.Equal(6m, specifications.Single(x => x.SubId == ChildSmallId).Quantity);
    }

    [Fact]
    public async Task AddMaterialInCombo_MaterialStatusDoesNotAffectAdding()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateMaterial(MatCoreId, status: "2"));
        await dbContext.SaveChangesAsync();

        var result = await CreateService(dbContext).AddMaterialInCombo(
            ComboId,
            CreateRequest(MatCoreId, quantity: 1, isCore: true));

        Assert.Equal("Lưu công thức thành công.", result);
        Assert.Equal(2, await dbContext.ComboSpecifications.CountAsync());
    }

    [Fact]
    public async Task AddMaterialInCombo_DeletedMaterial_ThrowsNotFoundWithoutWriting()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateMaterial(MatCoreId, isDeleted: true));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ComboId,
                CreateRequest(MatCoreId, quantity: 1, isCore: true)));

        Assert.Equal("Vật liệu không tồn tại hoặc không hợp lệ", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_ChildComboId_ThrowsComboNotFound()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ChildSmallId,
                CreateRequest(MatCoreId, quantity: 1, isCore: true)));

        Assert.Equal("Không tìm thấy combo", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_UnsyncedParentCombo_ThrowsBadRequest()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(nhanhProductId: null),
            CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ComboId,
                CreateRequest(MatCoreId, quantity: 1, isCore: true)));

        Assert.Equal("Combo chưa đồng bộ", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_NonPositiveChildFactor_ThrowsBeforeWriting()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s", factor: 0),
            CreateMaterial(MatCoreId));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ComboId,
                CreateRequest(MatCoreId, quantity: 1, isCore: true)));

        Assert.Equal($"Hệ số của combo con không hợp lệ: {ChildSmallId}", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_QuantityRoundsToZero_ThrowsBeforeWriting()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s"),
            CreateMaterial(MatCoreId, unit: "cành"));
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ComboId,
                CreateRequest(MatCoreId, quantity: 0.5m, isCore: true)));

        Assert.Equal($"Số lượng vật liệu sau khi tính toán không hợp lệ: {MatCoreId}", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
    }

    [Fact]
    public async Task AddMaterialInCombo_AddingNewMaterial_DoesNotChangeExistingRecipeRows()
    {
        await using var dbContext = CreateDbContext();
        var existingParent = CreateSpecification(
            ComboId,
            MatCoreId,
            quantity: 7,
            isCore: true,
            isActive: true);
        var existingChild = CreateSpecification(
            ChildSmallId,
            MatCoreId,
            quantity: 11,
            isCore: false,
            isActive: true);

        dbContext.Products.AddRange(
            CreateParentCombo(),
            CreateChildProduct(ChildSmallId, "combo-a-s", factor: 2m),
            CreateMaterial(MatCoreId),
            CreateMaterial(MatSupportId, productLabel: "Phụ liệu"));
        dbContext.ComboSpecifications.AddRange(existingParent, existingChild);
        await dbContext.SaveChangesAsync();

        await CreateService(dbContext).AddMaterialInCombo(
            ComboId,
            CreateRequest(MatSupportId, quantity: 4, isCore: false));

        Assert.Equal(7m, existingParent.Quantity);
        Assert.True(existingParent.IsCore);
        Assert.True(existingParent.IsActive);
        Assert.Equal(11m, existingChild.Quantity);
        Assert.False(existingChild.IsCore);
        Assert.True(existingChild.IsActive);
        Assert.Equal(4, await dbContext.ComboSpecifications.CountAsync());
    }

    [Fact]
    public async Task AddMaterialInCombo_EmptyMaterialId_ThrowsBadRequestWithoutWriting()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(CreateParentCombo());
        await dbContext.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => CreateService(dbContext).AddMaterialInCombo(
                ComboId,
                CreateRequest(Guid.Empty, quantity: 1, isCore: true)));

        Assert.Equal("Mã vật liệu không được để trống", exception.Message);
        Assert.Empty(dbContext.ComboSpecifications);
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

    private static ProductEntity CreateParentCombo(string? nhanhProductId = ParentNhanhProductId)
    {
        return new ProductEntity
        {
            Id = ComboId,
            NhanhProductId = nhanhProductId,
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

    private static ProductEntity CreateMaterial(
        Guid id,
        string? productLabel = MaterialLabel,
        string? unit = null,
        string? status = null,
        bool isDeleted = false)
    {
        return new ProductEntity
        {
            Id = id,
            NhanhProductId = $"mat-{id:N}",
            ProductLabel = productLabel,
            Unit = unit,
            Status = status,
            IsDeleted = isDeleted,
            ProductName = $"Material {id:N}",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static ComboSpecEntity CreateSpecification(
        Guid subId,
        Guid matId,
        decimal quantity,
        bool isCore,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new ComboSpecEntity
        {
            Id = Guid.NewGuid(),
            SubId = subId,
            MatId = matId,
            Quantity = quantity,
            IsCore = isCore,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}






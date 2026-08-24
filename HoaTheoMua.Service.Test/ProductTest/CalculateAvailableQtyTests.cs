using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using HoaTheoMua.Service.Product;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using HoaTheoMua.Service.Test.TestSupport;
using Xunit;
using ProductService = HoaTheoMua.Service.Product.Service;

namespace HoaTheoMua.Service.Test.ProductTest;

/// <summary>
/// Unit tests for CalculateAvailableQty method (UT-005 suite).
/// Tests SellableQuantity calculation logic with EXC-01 through EXC-04 handling.
/// </summary>
public class CalculateAvailableQtyTests
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

    #region UT-005: CalculateAvailableQty Method Tests

    /// <summary>
    /// UT-005-01: Combo không biến thể có SellableQuantity đúng.
    /// MIN(floor(stock_i / Quantity_i)) = MIN(100/2, 99/3) = MIN(50, 33) = 33
    ///
    /// **Rationale:** Xác nhận combo tính đúng MIN(CORE ratio) với nhiều materials
    /// </summary>
    [Fact]
    public async Task UT_005_01_StandaloneCombo_CalculatesCorrectlyBasedOnMinCoreComponentRatio()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };
        var mat2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 99, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat1.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat2.Id, Quantity = 3, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(33, result[combo.Id].SellableQuantity);
        Assert.Equal("Còn hàng", result[combo.Id].StockStatus);
        Assert.Null(result[combo.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-02: Cha combo lấy MIN SellableQuantity của các biến thể con.
    /// variant1: qty = floor(100/2) = 50
    /// variant2: qty = floor(100/5) = 20
    /// → parent = MIN(50, 20) = 20
    ///
    /// NOTE: Phải truyền cả parent.Id VÀ variant IDs để code tìm được variants.
    ///
    /// **Rationale:** Xác nhận parent lấy MIN từ các variant con
    /// </summary>
    [Fact]
    public async Task UT_005_02_ComboParent_CalculatesMinOfChildVariants()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };

        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        // Include both parent AND variant IDs so code can find variants
        var result = await service.CalculateAvailableQty(new List<Guid> { parent.Id, variant1.Id, variant2.Id });

        Assert.True(result.ContainsKey(parent.Id));
        Assert.Equal(20, result[parent.Id].SellableQuantity);
        Assert.Equal("Còn hàng", result[parent.Id].StockStatus);
        Assert.Null(result[parent.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-03: Sản phẩm thường lấy trực tiếp Stock.
    ///
    /// **Rationale:** Xác nhận sản phẩm thường dùng trực tiếp Stock
    /// </summary>
    [Fact]
    public async Task UT_005_03_NormalProduct_ReturnsStockDirectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 15, IsDeleted = false };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { product.Id });

        Assert.True(result.ContainsKey(product.Id));
        Assert.Equal(15, result[product.Id].SellableQuantity);
        Assert.Equal("Còn hàng", result[product.Id].StockStatus);
        Assert.Null(result[product.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-04: EXC-02 - Combo có specs nhưng không có dòng CORE nào.
    /// SellableQuantity = 0, StockStatus = null, UnavailableReason = "no_core"
    ///
    /// **Rationale:** Xác nhận combo có spec nhưng không CORE trả no_core
    /// </summary>
    [Fact]
    public async Task UT_005_04_EXC_02_ComboWithNoCoreComponents_ReturnsNoCore()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };
        // spec có IsCore = false → không có CORE
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = false, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(0, result[combo.Id].SellableQuantity);
        Assert.Null(result[combo.Id].StockStatus); // EXC-02 → null
        Assert.Equal("no_core", result[combo.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-05: Combo có CORE đủ hàng → qty > 0, "Còn hàng"
    ///
    /// **Rationale:** Xác nhận combo 1 material đủ hàng tính đúng
    /// </summary>
    [Fact]
    public async Task UT_005_05_ComboWithSufficientStock_ReturnsCorrectQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 50, IsDeleted = false };

        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(25, result[combo.Id].SellableQuantity); // floor(50/2)
        Assert.Equal("Còn hàng", result[combo.Id].StockStatus);
        Assert.Null(result[combo.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-06: Quantity <= 0 được skip, không gây divide by zero.
    ///
    /// **Rationale:** Xác nhận spec có Quantity<=0 được skip không divide by zero
    /// </summary>
    [Fact]
    public async Task UT_005_06_RequiredQuantityZeroOrNegative_AvoidsDivideByZeroAndSkips()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };
        var mat2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 50, IsDeleted = false };

        // spec1 có Quantity = 0 → skip
        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat1.Id, Quantity = 0, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat2.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(25, result[combo.Id].SellableQuantity); // chỉ mat2: floor(50/2)
        Assert.Equal("Còn hàng", result[combo.Id].StockStatus);
    }

    /// <summary>
    /// UT-005-07: Specs bị IsDeleted hoặc IsActive=false được filter ra.
    ///
    /// **Rationale:** Xác nhận chỉ specs IsActive=true và IsDeleted=false được tính
    /// </summary>
    [Fact]
    public async Task UT_005_07_SoftDeletedOrInactiveSpecifications_AreFilteredOut()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };
        var mat2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 50, IsDeleted = false };

        // spec1 bị xóa mềm → không tính
        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat1.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = true };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat2.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat1, mat2);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(50, result[combo.Id].SellableQuantity); // chỉ mat2: floor(50/1)
    }

    /// <summary>
    /// UT-005-08: Empty input list → empty dictionary.
    ///
    /// **Rationale:** Xác nhận input rỗng trả {} không lỗi
    /// </summary>
    [Fact]
    public async Task UT_005_08_EmptyInputList_ReturnsEmptyDictionary()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var result = await service.CalculateAvailableQty(new List<Guid>());

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// UT-005-09: StockStatus 2 mức đúng - "Còn hàng" (qty>0) vs null (qty=0 có CORE).
    ///
    /// **Rationale:** Xác nhận combo có CORE nhưng stock=0 trả qty=0, status=null
    /// </summary>
    [Fact]
    public async Task UT_005_09_StockStatusDeterminism_CorrectlyMapsStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var combo2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 1, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo1.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };
        // combo2: cần 2 nhưng stock chỉ có 1 → qty = 0, có CORE nên StockStatus = null
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo2.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo1, combo2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo1.Id, combo2.Id });

        // combo1: qty = 1, có hàng
        Assert.Equal("Còn hàng", result[combo1.Id].StockStatus);
        Assert.Equal(1, result[combo1.Id].SellableQuantity);
        Assert.Null(result[combo1.Id].UnavailableReason);

        // combo2: qty = 0, có CORE nhưng hết nguyên liệu → StockStatus = null (không phải "Hết hàng")
        Assert.Equal(0, result[combo2.Id].SellableQuantity);
        Assert.Null(result[combo2.Id].StockStatus); // "Hết hàng" chỉ dành cho sản phẩm thường hoặc combo có CORE
        Assert.Null(result[combo2.Id].UnavailableReason); // Không có lỗi, chỉ là hết hàng
    }

    /// <summary>
    /// UT-005-10: Material không tồn tại trong inventory → treated as 0 stock.
    ///
    /// **Rationale:** Xác nhận material không tồn tại được coi là stock=0
    /// </summary>
    [Fact]
    public async Task UT_005_10_ComponentMissingFromInventory_TreatedAsZeroStock()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        // matId không tham chiếu đến product nào trong database
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = Guid.NewGuid(), Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddAsync(combo);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(0, result[combo.Id].SellableQuantity);
        // Có CORE nhưng stock = 0 → StockStatus = null
        Assert.Null(result[combo.Id].StockStatus);
        Assert.Null(result[combo.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-11: EXC-03 - Stock âm được clamp về 0.
    ///
    /// **Rationale:** Xác nhận stock âm được clamp về 0
    /// </summary>
    [Fact]
    public async Task UT_005_11_EXC_03_NegativeStock_ClampedToZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = -5, IsDeleted = false };

        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        // floor(clamp(-5, 0) / 2) = floor(0 / 2) = 0, không phải -2
        Assert.Equal(0, result[combo.Id].SellableQuantity);
        Assert.Null(result[combo.Id].StockStatus); // Có CORE, qty = 0 → null
        Assert.Null(result[combo.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-12: EXC-01 - Combo không có công thức (không có ComboSpecification).
    /// SellableQuantity = 0, StockStatus = null, UnavailableReason = "no_formula"
    ///
    /// **Rationale:** Xác nhận combo không có spec trả no_formula
    /// </summary>
    [Fact]
    public async Task UT_005_12_EXC_01_ComboWithoutFormula_ReturnsNoFormula()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        // Combo không có ComboSpecification nào
        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };

        await dbContext.Products.AddAsync(combo);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.True(result.ContainsKey(combo.Id));
        Assert.Equal(0, result[combo.Id].SellableQuantity);
        Assert.Null(result[combo.Id].StockStatus);
        Assert.Equal("no_formula", result[combo.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-13: Combo parent với mixed reasons trong variants → UnavailableReason = null.
    ///
    /// **Rationale:** Xác nhận parent với variants có lý do khác nhau trả null
    /// </summary>
    [Fact]
    public async Task UT_005_13_ComboParentWithMixedVariantReasons_HasNullUnavailableReason()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };

        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 50, IsDeleted = false };

        // variant1: có đủ nguyên liệu → qty = 50
        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };
        // variant2: không có công thức → EXC-01
        // variant2: thêm một material không tồn tại

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddAsync(spec1);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { parent.Id });

        Assert.True(result.ContainsKey(parent.Id));
        // variant1: qty=50, variant2: qty=0 ("no_formula")
        Assert.Equal(0, result[parent.Id].SellableQuantity); // MIN(50, 0)
        // Mixed reasons → UnavailableReason = null
        Assert.Null(result[parent.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-14: Parent với tất cả variants đều hết hàng → StockStatus = null.
    ///
    /// **Rationale:** Xác nhận parent với variants hết material trả status=null
    /// </summary>
    [Fact]
    public async Task UT_005_14_ComboParentWithAllVariantsOutOfStock_ReturnsNullStockStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 0, IsDeleted = false };

        // Cả 2 variants đều có CORE nhưng hết nguyên liệu
        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { parent.Id });

        Assert.True(result.ContainsKey(parent.Id));
        Assert.Equal(0, result[parent.Id].SellableQuantity);
        // Cả 2 variants đều có cùng reason (null) → lấy null
        Assert.Null(result[parent.Id].UnavailableReason);
        Assert.Null(result[parent.Id].StockStatus);
    }

    #endregion

    #region UT-004: GetLocalProductsV2 Integration Tests

    /// <summary>
    /// UT-004-01: Combo không biến thể có SellableQuantity.
    ///
    /// **Rationale:** Xác nhận combo độc lập tính đúng MIN(CORE ratio)
    /// </summary>
    [Fact]
    public async Task UT_004_01_GetLocalProductsV2_StandaloneCombo_HasSellableQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "Combo 1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedCombo = items.FirstOrDefault(i => i.Id == combo.Id);

        Assert.NotNull(mappedCombo);
        Assert.Equal(100, mappedCombo.SellableQuantity);
        Assert.Equal("Còn hàng", mappedCombo.StockStatus);
        Assert.Null(mappedCombo.UnavailableReason);
    }

    /// <summary>
    /// UT-004-02: Cha combo có biến thể lấy MIN variant.
    ///
    /// **Rationale:** Xác nhận parent combo lấy MIN từ các variant con
    /// </summary>
    [Fact]
    public async Task UT_004_02_GetLocalProductsV2_ComboParent_HasSellableQuantityAsMinOfVariants()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Parent", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedParent = items.FirstOrDefault(i => i.Id == parent.Id);

        Assert.NotNull(mappedParent);
        Assert.Equal(20, mappedParent.SellableQuantity);
        Assert.Equal("Còn hàng", mappedParent.StockStatus);
        Assert.Null(mappedParent.UnavailableReason);
    }

    /// <summary>
    /// UT-004-03: Mixed products - sản phẩm thường + combo cùng trang.
    ///
    /// **Rationale:** Xác nhận cả 2 loại product trả về đúng SellableQuantity
    /// </summary>
    [Fact]
    public async Task UT_004_03_GetLocalProductsV2_MixedProducts()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Normal 1", Stock = 10, IsDeleted = false };
        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "Combo 1", IsDeleted = false };
        var product2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Normal 2", Stock = 0, IsDeleted = false };

        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 30, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(product1, combo, product2, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var p1 = items.FirstOrDefault(i => i.Id == product1.Id);
        var c = items.FirstOrDefault(i => i.Id == combo.Id);
        var p2 = items.FirstOrDefault(i => i.Id == product2.Id);

        Assert.NotNull(p1);
        Assert.Equal(10, p1.SellableQuantity);
        Assert.Equal("Còn hàng", p1.StockStatus);

        Assert.NotNull(c);
        Assert.Equal(30, c.SellableQuantity);
        Assert.Equal("Còn hàng", c.StockStatus);

        // Sản phẩm thường Stock=0 → "Hết hàng"
        Assert.NotNull(p2);
        Assert.Equal(0, p2.SellableQuantity);
        Assert.Equal("Hết hàng", p2.StockStatus);
    }

    /// <summary>
    /// UT-004-04: Danh sách rỗng - early return.
    ///
    /// **Rationale:** Xác nhận early return khi không có dữ liệu
    /// </summary>
    [Fact]
    public async Task UT_004_04_GetLocalProductsV2_EmptyDatabase_ReturnsEmptyList()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        Assert.Empty(result.Value.Items);
    }

    /// <summary>
    /// UT-004-05: EXC-01 - Combo không có công thức.
    ///
    /// **Rationale:** Xác nhận combo không công thức trả về no_formula
    /// </summary>
    [Fact]
    public async Task UT_004_05_EXC_01_GetLocalProductsV2_ComboWithoutFormula_ReturnsNoFormula()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "No Formula", IsDeleted = false };
        await dbContext.Products.AddAsync(combo);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedCombo = items.FirstOrDefault(i => i.Id == combo.Id);

        Assert.NotNull(mappedCombo);
        Assert.Equal(0, mappedCombo.SellableQuantity);
        Assert.Null(mappedCombo.StockStatus);
        Assert.Equal("no_formula", mappedCombo.UnavailableReason);
    }

    /// <summary>
    /// UT-004-06: EXC-02 - Combo không có dòng CORE.
    ///
    /// **Rationale:** Xác nhận combo không CORE trả về no_core
    /// </summary>
    [Fact]
    public async Task UT_004_06_EXC_02_GetLocalProductsV2_ComboWithNoCore_ReturnsNoCore()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "No Core", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };
        // IsCore = false
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = false, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedCombo = items.FirstOrDefault(i => i.Id == combo.Id);

        Assert.NotNull(mappedCombo);
        Assert.Equal(0, mappedCombo.SellableQuantity);
        Assert.Null(mappedCombo.StockStatus);
        Assert.Equal("no_core", mappedCombo.UnavailableReason);
    }

    /// <summary>
    /// UT-004-07: EXC-03 - Stock âm clamp về 0.
    ///
    /// **Rationale:** Xác nhận stock âm được clamp về 0
    /// </summary>
    [Fact]
    public async Task UT_004_07_EXC_03_GetLocalProductsV2_NegativeStock_ClampedToZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "Negative Stock", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = -5, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedCombo = items.FirstOrDefault(i => i.Id == combo.Id);

        Assert.NotNull(mappedCombo);
        // floor(clamp(-5, 0) / 2) = 0
        Assert.Equal(0, mappedCombo.SellableQuantity);
        Assert.Null(mappedCombo.StockStatus); // Có CORE, qty = 0 → null
    }

    /// <summary>
    /// UT-004-08: StockStatus 2 mức đúng - "Còn hàng" khi qty > 0.
    ///
    /// **Rationale:** Xác nhận StockStatus mapping đúng khi qty > 0
    /// </summary>
    [Fact]
    public async Task UT_004_08_GetLocalProductsV2_StockStatus_CònHàng_WhenQuantityPositive()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "Combo In Stock", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 10, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var mappedCombo = items.FirstOrDefault(i => i.Id == combo.Id);

        Assert.NotNull(mappedCombo);
        Assert.Equal(5, mappedCombo.SellableQuantity); // floor(10/2)
        Assert.Equal("Còn hàng", mappedCombo.StockStatus);
        Assert.Null(mappedCombo.UnavailableReason);
    }

    /// <summary>
    /// UT-004-09: EXC-04 - Lỗi tính 1 combo không ảnh hưởng combo khác.
    ///
    /// **Rationale:** Xác nhận lỗi 1 combo không ảnh hưởng combo khác
    /// </summary>
    [Fact]
    public async Task UT_004_09_EXC_04_GetLocalProductsV2_CalculationError_OneComboOnly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "OK Combo", IsDeleted = false };
        var combo2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ProductName = "Error Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo1.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };
        // combo2 có spec trỏ đến mat không tồn tại trong inventory
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo2.Id, MatId = Guid.NewGuid(), Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo1, combo2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest());

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        var c1 = items.FirstOrDefault(i => i.Id == combo1.Id);
        var c2 = items.FirstOrDefault(i => i.Id == combo2.Id);

        // combo1: OK
        Assert.NotNull(c1);
        Assert.Equal(100, c1.SellableQuantity);
        Assert.Equal("Còn hàng", c1.StockStatus);
        Assert.Null(c1.UnavailableReason);

        // combo2: stock không tìm thấy trong inventory (vì matId không phải là product entity)
        Assert.NotNull(c2);
        Assert.Equal(0, c2.SellableQuantity);
        Assert.Null(c2.StockStatus);
        Assert.Null(c2.UnavailableReason);
    }

    /// <summary>
    /// UT-004-10: AC-010 - soldOutOnly=true filter đúng.
    ///
    /// **Rationale:** Xác nhận soldOutOnly=true chỉ trả items qty=0
    /// </summary>
    [Fact]
    public async Task UT_004_10_AC_010_GetLocalProductsV2_SoldOutOnly_ReturnsOnlyOutOfStockItems()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var item1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 1", Stock = 0, IsDeleted = false };
        var item2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 2", Stock = 3, IsDeleted = false };
        var item3 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 3", Stock = 12, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(item1, item2, item3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest { soldOutOnly = true };
        var result = await service.GetLocalProductsV2(request);

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Single(items);
        Assert.Equal(0, items[0].SellableQuantity);
        Assert.Equal("Hết hàng", items[0].StockStatus);
    }

    /// <summary>
    /// UT-004-11: AC-011 - sort sellableQuantity tăng dần.
    ///
    /// **Rationale:** Xác nhận sort asc theo SellableQuantity đúng
    /// </summary>
    [Fact]
    public async Task UT_004_11_AC_011_GetLocalProductsV2_SortBySellableQuantityAscending()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var item1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 1", Stock = 12, IsDeleted = false };
        var item2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 2", Stock = 0, IsDeleted = false };
        var item3 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 3", Stock = 3, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(item1, item2, item3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest { sortBy = new List<string> { "+sellableQuantity" } };
        var result = await service.GetLocalProductsV2(request);

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(3, items.Count);
        // Order: qty=0, qty=3, qty=12
        Assert.Equal(0, items[0].SellableQuantity);
        Assert.Equal(3, items[1].SellableQuantity);
        Assert.Equal(12, items[2].SellableQuantity);
    }

    /// <summary>
    /// UT-004-12: inStockOnly=true filter đúng.
    ///
    /// **Rationale:** Xác nhận inStockOnly=true chỉ trả items qty>0
    /// </summary>
    [Fact]
    public async Task UT_004_12_GetLocalProductsV2_InStockOnly_ReturnsOnlyInStockItems()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var item1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 1", Stock = 0, IsDeleted = false };
        var item2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 2", Stock = 3, IsDeleted = false };
        var item3 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 3", Stock = 12, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(item1, item2, item3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest { inStockOnly = true };
        var result = await service.GetLocalProductsV2(request);

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.True(i.SellableQuantity > 0));
    }

    /// <summary>
    /// UT-004-13: soldOutOnly + sortBy kết hợp.
    ///
    /// **Rationale:** Xác nhận filter và sort kết hợp đúng
    /// </summary>
    [Fact]
    public async Task UT_004_13_GetLocalProductsV2_SoldOutOnlyWithSort_CombinesCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var item1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 1", Stock = 5, IsDeleted = false };
        var item2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 2", Stock = 0, IsDeleted = false };
        var item3 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 3", Stock = 0, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(item1, item2, item3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest
        {
            soldOutOnly = true,
            sortBy = new List<string> { "+sellableQuantity" }
        };
        var result = await service.GetLocalProductsV2(request);

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.Equal(0, i.SellableQuantity));
    }

    /// <summary>
    /// UT-004-14: sortBy=-sellableQuantity giảm dần.
    ///
    /// **Rationale:** Xác nhận sort desc theo SellableQuantity đúng
    /// </summary>
    [Fact]
    public async Task UT_004_14_GetLocalProductsV2_SortBySellableQuantityDescending()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var item1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 1", Stock = 0, IsDeleted = false };
        var item2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 2", Stock = 3, IsDeleted = false };
        var item3 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Item 3", Stock = 12, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(item1, item2, item3);
        await dbContext.SaveChangesAsync();

        var request = new Request.LocalProductQueryRequest { sortBy = new List<string> { "-sellableQuantity" } };
        var result = await service.GetLocalProductsV2(request);

        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();
        Assert.Equal(3, items.Count);
        // Order: qty=12, qty=3, qty=0
        Assert.Equal(12, items[0].SellableQuantity);
        Assert.Equal(3, items[1].SellableQuantity);
        Assert.Equal(0, items[2].SellableQuantity);
    }

    #endregion

    #region UT-013: GetLocalProductDetail Integration Tests

    /// <summary>
    /// UT-013-01: Sản phẩm thường lấy Stock.
    ///
    /// **Rationale:** Xác nhận sản phẩm thường dùng trực tiếp Stock
    /// </summary>
    [Fact]
    public async Task UT_013_01_GetLocalProductDetail_NormalProduct_UsesStockForSellableQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 15, IsDeleted = false };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(product.Id);

        Assert.Equal(product.Id, result.Id);
        Assert.Equal(15, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
        Assert.Null(result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-02: Combo không biến thể có SellableQuantity.
    ///
    /// **Rationale:** Xác nhận combo độc lập tính đúng MIN(CORE)
    /// </summary>
    [Fact]
    public async Task UT_013_02_GetLocalProductDetail_StandaloneCombo_HasSellableQuantity()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(combo.Id);

        Assert.Equal(combo.Id, result.Id);
        Assert.Equal(100, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
        Assert.Null(result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-03: Cha combo lấy MIN variant.
    ///
    /// **Rationale:** Xác nhận parent lấy MIN từ các variant con
    /// </summary>
    [Fact]
    public async Task UT_013_03_GetLocalProductDetail_ComboParent_HasSellableQuantityAsMinOfVariants()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(parent.Id);

        Assert.Equal(parent.Id, result.Id);
        Assert.Equal(20, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
        Assert.Null(result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-04: Không tìm thấy sản phẩm thì ném NotFoundException.
    ///
    /// **Rationale:** Xác nhận sản phẩm không tồn tại ném NotFoundException
    /// </summary>
    [Fact]
    public async Task UT_013_04_GetLocalProductDetail_NonExistentProduct_ThrowsNotFoundException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var nonExistentId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.GetLocalProductDetail(nonExistentId));
        Assert.Equal("Không tìm thấy sản phẩm.", ex.Message);
    }

    /// <summary>
    /// UT-013-05: Combo hết nguyên liệu → qty=0, StockStatus=null.
    ///
    /// **Rationale:** Xác nhận combo hết material trả qty=0, status=null
    /// </summary>
    [Fact]
    public async Task UT_013_05_GetLocalProductDetail_ComboOutOfStock_HasZeroQtyAndNullStockStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 0, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(combo.Id);

        Assert.Equal(combo.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity);
        Assert.Null(result.StockStatus); // Có CORE, qty = 0 → null
        Assert.Null(result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-06: EXC-01 - Combo không có công thức.
    ///
    /// **Rationale:** Xác nhận combo không có spec trả no_formula
    /// </summary>
    [Fact]
    public async Task UT_013_06_EXC_01_GetLocalProductDetail_ComboWithoutFormula_ReturnsNoFormula()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        await dbContext.Products.AddAsync(combo);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(combo.Id);

        Assert.Equal(combo.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity);
        Assert.Null(result.StockStatus);
        Assert.Equal("no_formula", result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-07: Sản phẩm bị xóa mềm thì ném NotFoundException.
    ///
    /// **Rationale:** Xác nhận sản phẩm IsDeleted=true ném NotFoundException
    /// </summary>
    [Fact]
    public async Task UT_013_07_GetLocalProductDetail_SoftDeletedProduct_ThrowsNotFoundException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", IsDeleted = true };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.GetLocalProductDetail(product.Id));
        Assert.Equal("Không tìm thấy sản phẩm.", ex.Message);
    }

    /// <summary>
    /// UT-013-08: Parent với tất cả variants đều hết hàng.
    ///
    /// **Rationale:** Xác nhận parent với variants hết material trả status=null
    /// </summary>
    [Fact]
    public async Task UT_013_08_GetLocalProductDetail_ComboParentWithAllVariantsOutOfStock_ReturnsZeroAndNullStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 0, IsDeleted = false };

        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = mat.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, mat);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(parent.Id);

        Assert.Equal(parent.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity);
        Assert.Null(result.StockStatus);
        Assert.Null(result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-09: Child variant combo query trực tiếp → trả về qty riêng.
    ///
    /// **Rationale:** Xác nhận variant combo query riêng trả qty độc lập
    /// </summary>
    [Fact]
    public async Task UT_013_09_GetLocalProductDetail_ChildVariantComboDirectQuery_ReturnsIndividualQty()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 50, IsDeleted = false };

        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant.Id, MatId = mat.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(parent, variant, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(variant.Id);

        Assert.Equal(variant.Id, result.Id);
        Assert.Equal(50, result.SellableQuantity);
        Assert.Equal("Còn hàng", result.StockStatus);
        Assert.Null(result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-10: EXC-02 - Combo có specs nhưng không có dòng CORE.
    ///
    /// **Rationale:** Xác nhận combo có spec nhưng không CORE trả no_core
    /// </summary>
    [Fact]
    public async Task UT_013_10_EXC_02_GetLocalProductDetail_ComboWithNoCore_ReturnsNoCore()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 100, IsDeleted = false };
        // IsCore = false
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 1, IsCore = false, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(combo.Id);

        Assert.Equal(combo.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity);
        Assert.Null(result.StockStatus);
        Assert.Equal("no_core", result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-11: Sản phẩm thường Stock = 0 → "Hết hàng".
    ///
    /// **Rationale:** Xác nhận sản phẩm thường stock=0 trả "Hết hàng"
    /// </summary>
    [Fact]
    public async Task UT_013_11_GetLocalProductDetail_NormalProduct_OutOfStock_ReturnsHếtHàng()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 0, IsDeleted = false };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(product.Id);

        Assert.Equal(product.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity);
        Assert.Equal("Hết hàng", result.StockStatus);
        Assert.Null(result.UnavailableReason);
    }

    /// <summary>
    /// UT-013-12: EXC-03 - Material stock âm clamp về 0.
    ///
    /// **Rationale:** Xác nhận material stock âm được clamp về 0
    /// </summary>
    [Fact]
    public async Task UT_013_12_EXC_03_GetLocalProductDetail_NegativeMaterialStock_ClampedToZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var mat = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = -10, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = mat.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, mat);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(combo.Id);

        Assert.Equal(combo.Id, result.Id);
        Assert.Equal(0, result.SellableQuantity); // clamp(-10, 0) / 2 = 0
        Assert.Null(result.StockStatus);
        Assert.Null(result.UnavailableReason);
    }

    /// <summary>
    /// UT-005-15: Input null được xử lý như input rỗng.
    ///
    /// **Rationale:** Xác nhận input null được xử lý như rỗng
    /// </summary>
    [Fact]
    public async Task UT_005_15_NullInput_ReturnsEmptyDictionary()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var result = await service.CalculateAvailableQty(null!);

        Assert.Empty(result);
    }

    /// <summary>
    /// UT-005-16: Spec inactive hoặc xóa mềm không được coi là công thức.
    ///
    /// **Rationale:** Xác nhận combo chỉ có spec inactive/deleted trả no_formula
    /// </summary>
    [Fact]
    public async Task UT_005_16_OnlyInactiveOrDeletedSpecifications_ReturnsNoFormula()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var material = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 10, IsDeleted = false };
        var inactiveSpec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = material.Id, Quantity = 1, IsCore = true, IsActive = false, IsDeleted = false };
        var deletedSpec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = material.Id, Quantity = 1, IsCore = true, IsActive = true, IsDeleted = true };

        await dbContext.Products.AddRangeAsync(combo, material);
        await dbContext.ComboSpecifications.AddRangeAsync(inactiveSpec, deletedSpec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.Equal(0, result[combo.Id].SellableQuantity);
        Assert.Null(result[combo.Id].StockStatus);
        Assert.Equal("no_formula", result[combo.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-17: Công thức có toàn bộ dòng CORE với Quantity không dương thì không có CORE khả dụng.
    ///
    /// **Rationale:** Xác nhận combo có CORE nhưng Quantity<=0 trả no_core
    /// </summary>
    [Fact]
    public async Task UT_005_17_AllCoreQuantitiesNonPositive_ReturnsNoCore()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var combo = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", IsDeleted = false };
        var material = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 10, IsDeleted = false };
        var zeroQuantitySpec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = material.Id, Quantity = 0, IsCore = true, IsActive = true, IsDeleted = false };
        var negativeQuantitySpec = new ComboSpecification { Id = Guid.NewGuid(), SubId = combo.Id, MatId = material.Id, Quantity = -1, IsCore = true, IsActive = true, IsDeleted = false };

        await dbContext.Products.AddRangeAsync(combo, material);
        await dbContext.ComboSpecifications.AddRangeAsync(zeroQuantitySpec, negativeQuantitySpec);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { combo.Id });

        Assert.Equal(0, result[combo.Id].SellableQuantity);
        Assert.Null(result[combo.Id].StockStatus);
        Assert.Equal("no_core", result[combo.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-005-18: Stock âm của sản phẩm thường được clamp về 0.
    ///
    /// **Rationale:** Xác nhận sản phẩm thường stock âm được clamp về 0
    /// </summary>
    [Fact]
    public async Task UT_005_18_NormalProductNegativeStock_IsClampedToZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = -1, IsDeleted = false };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.CalculateAvailableQty(new List<Guid> { product.Id });

        Assert.Equal(0, result[product.Id].SellableQuantity);
        Assert.Equal("Hết hàng", result[product.Id].StockStatus);
        Assert.Null(result[product.Id].UnavailableReason);
    }

    /// <summary>
    /// UT-004-15: Search được trim và không phân biệt hoa thường trên các trường hỗ trợ.
    ///
    /// **Rationale:** Xác nhận search trim và không phân biệt hoa thường
    /// </summary>
    [Fact]
    public async Task UT_004_15_GetLocalProductsV2_SearchesBarcodeCaseInsensitivelyAfterTrim()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var matched = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Matched", Barcode = "BC-ABC-123", Stock = 1, IsDeleted = false };
        var unmatched = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Unmatched", Barcode = "BC-XYZ-999", Stock = 1, IsDeleted = false };
        await dbContext.Products.AddRangeAsync(matched, unmatched);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest { search = "  abc-123  " });
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();

        Assert.Single(items);
        Assert.Equal(matched.Id, items[0].Id);
    }

    /// <summary>
    /// UT-004-16: Lọc tồn kho xảy ra trước phân trang nên trang chỉ chứa item phù hợp.
    ///
    /// **Rationale:** Xác nhận filter được áp dụng TRƯỚC phân trang
    /// </summary>
    [Fact]
    public async Task UT_004_16_GetLocalProductsV2_SoldOutFilterIsAppliedBeforePagination()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var soldOut1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Sold out 1", Stock = 0, Position = 1, IsDeleted = false };
        var inStock = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "In stock", Stock = 5, Position = 2, IsDeleted = false };
        var soldOut2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Sold out 2", Stock = 0, Position = 3, IsDeleted = false };
        var soldOut3 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", ProductName = "Sold out 3", Stock = 0, Position = 4, IsDeleted = false };
        await dbContext.Products.AddRangeAsync(soldOut1, inStock, soldOut2, soldOut3);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            soldOutOnly = true,
            pageIndex = 2,
            pageSize = 2
        });
        var items = result.Value.Items.Cast<Response.ProductDetailResponse>().ToList();

        Assert.Single(items);
        Assert.Equal(soldOut3.Id, items[0].Id);
        Assert.Equal(3, result.Value.TotalCount);
    }

    /// <summary>
    /// UT-004-17: Khi cả hai cờ tồn kho cùng true, hai filter tuần tự cho kết quả rỗng.
    ///
    /// **Rationale:** Xác nhận soldOutOnly + inStockOnly loại trừ nhau
    /// </summary>
    [Fact]
    public async Task UT_004_17_GetLocalProductsV2_BothStockFilters_ReturnsNoItems()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        await dbContext.Products.AddRangeAsync(
            new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 0, IsDeleted = false },
            new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 1, IsDeleted = false });
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductsV2(new Request.LocalProductQueryRequest
        {
            soldOutOnly = true,
            inStockOnly = true
        });

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    /// <summary>
    /// UT-013-13: Biến thể con resolve đúng ParentId và không có children riêng.
    ///
    /// **Rationale:** Xác nhận variant combo resolve đúng ParentId
    /// </summary>
    [Fact]
    public async Task UT_013_13_GetLocalProductDetail_ChildVariant_ResolvesParentId()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var material = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 10, IsDeleted = false };
        var spec = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant.Id, MatId = material.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        await dbContext.Products.AddRangeAsync(parent, variant, material);
        await dbContext.ComboSpecifications.AddAsync(spec);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(variant.Id);

        Assert.Equal(parent.Id, result.ParentId);
        Assert.Empty(result.Children);
        Assert.Equal(5, result.SellableQuantity);
    }

    /// <summary>
    /// UT-013-14: Parent trả children active cùng số lượng được tính độc lập.
    ///
    /// **Rationale:** Xác nhận parent trả đúng children active với qty tính độc lập
    /// </summary>
    [Fact]
    public async Task UT_013_14_GetLocalProductDetail_Parent_ReturnsCalculatedActiveChildren()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", NhanhProductId = "parent-1", IsDeleted = false };
        var variant1 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var variant2 = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = false };
        var deletedVariant = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Combo", ParentProductId = "parent-1", IsDeleted = true };
        var material = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = 20, IsDeleted = false };
        var spec1 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant1.Id, MatId = material.Id, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false };
        var spec2 = new ComboSpecification { Id = Guid.NewGuid(), SubId = variant2.Id, MatId = material.Id, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false };
        await dbContext.Products.AddRangeAsync(parent, variant1, variant2, deletedVariant, material);
        await dbContext.ComboSpecifications.AddRangeAsync(spec1, spec2);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(parent.Id);

        Assert.Equal(4, result.SellableQuantity);
        Assert.Equal(2, result.Children.Count);
        Assert.Equal(10, result.Children.Single(x => x.Id == variant1.Id).SellableQuantity);
        Assert.Equal(4, result.Children.Single(x => x.Id == variant2.Id).SellableQuantity);
    }

    /// <summary>
    /// UT-013-15: Stock âm của sản phẩm thường cũng được clamp trong response detail.
    ///
    /// **Rationale:** Xác nhận sản phẩm thường stock âm clamp về 0 trong detail
    /// </summary>
    [Fact]
    public async Task UT_013_15_GetLocalProductDetail_NormalProductNegativeStock_IsClampedToZero()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var product = new ProductEntity { Id = Guid.NewGuid(), ProductType = "Sản phẩm", Stock = -1, IsDeleted = false };
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var result = await service.GetLocalProductDetail(product.Id);

        Assert.Equal(0, result.SellableQuantity);
        Assert.Equal("Hết hàng", result.StockStatus);
        Assert.Null(result.UnavailableReason);
    }

    #endregion
}

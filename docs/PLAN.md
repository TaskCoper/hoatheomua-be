# Plan: Implement CalculateAvailableQty cho Product API

## Context
API `GetLocalProductsV2` và `GetLocalProductDetail` hiện tại trả về danh sách/thông tin sản phẩm nhưng thiếu thông tin về số lượng khả dụng của **Combo** (ProductType="Combo"). Cần thêm:

- **SellableQuantity**: Số lượng có thể bán của Combo, được tính dựa trên thành phần Core và tồn kho
- **StockStatus**: Trạng thái tồn kho ("Còn hàng" / "Hết hàng")

Logic tính toán được tách riêng thành hàm bổ trợ `CalculateAvailableQty` trong `ProductService`, có thể tái sử dụng bởi nhiều API khác nhau.

---

## Cấu trúc Entity (Đã có sẵn)

### ComboSpecification
```csharp
public class ComboSpecification : BaseEntity<Guid>, IAuditableEntity
{
    public Guid SubId { get; set; }      // Combo (sản phẩm combo)
    public Product Sub { get; set; }

    public Guid MatId { get; set; }       // Material (thành phần)
    public Product Mat { get; set; }

    public int Quantity { get; set; }     // Số lượng cần dùng
    public bool IsCore { get; set; }     // Là thành phần chính
    public bool IsActive { get; set; }   // Đang hoạt động
}
```

### Product
```csharp
// Đã có navigation properties:
public ICollection<ComboSpecification> SubIds { get; set; }  // Combo này là Combo
public ICollection<ComboSpecification> MatIds { get; set; }  // Combo này là Material
```

---

## Files cần sửa

```
HoaTheoMua.Service/Product/Response.cs      (thêm SellableQuantity, StockStatus)
HoaTheoMua.Service/Product/Service.cs      (thêm CalculateAvailableQty + dùng trong 2 API)
HoaTheoMua.Service.Test/ProductTest/CalculateAvailableQtyTests.cs  (Unit tests)
```

---

## Step 1: Cập nhật Response

### File: `HoaTheoMua.Service/Product/Response.cs`

Thêm vào `ProductDetailResponse`:

```csharp
public class ProductDetailResponse
{
    // ... các field hiện tại ...

    // Số lượng khả dụng (chỉ có cho Combo)
    public int? SellableQuantity { get; set; }

    // Trạng thái tồn kho: "Còn hàng" / "Hết hàng" (chỉ có cho Combo)
    public string? StockStatus { get; set; }
}
```

---

## Step 2: Thêm CalculateAvailableQty vào ProductService

### File: `HoaTheoMua.Service/Product/Service.cs`

Thêm method `CalculateAvailableQty`:

```csharp
/// <summary>
/// Tính số lượng khả dụng của Combo dựa trên công thức và tồn kho thành phần.
/// Chỉ tính với sản phẩm có ProductParent = null.
/// </summary>
/// <param name="productIds">Danh sách Id sản phẩm cần tính</param>
/// <returns>Dictionary với Key = ProductId, Value = (SellableQuantity, StockStatus)</returns>
public async Task<Dictionary<Guid, (int SellableQuantity, string StockStatus)>> CalculateAvailableQty(List<Guid> productIds)
{
    if (productIds == null || !productIds.Any())
        return new Dictionary<Guid, (int, string)>();

    // === GIAI ĐOẠN 1: Xác định combo cần tính ===

    // Lấy thông tin sản phẩm để phân loại
    var products = await _dbContext.Products
        .AsNoTracking()
        .Where(p => productIds.Contains(p.Id))
        .ToListAsync();

    // 1.1: Combo không biến thể (ProductType = "Combo", ProductParent = null)
    var standaloneComboIds = products
        .Where(p => p.ProductType == "Combo" && string.IsNullOrEmpty(p.ParentProductId))
        .Select(p => p.Id)
        .ToList();

    // 1.2: Cha combo có biến thể (ProductType = "Sản Phẩm", ProductParent = null, có con Combo)
    var parentProductIds = products
        .Where(p => p.ProductType == "Sản Phẩm" && string.IsNullOrEmpty(p.ParentProductId))
        .Select(p => p.Id.ToString())
        .ToList();

    // Lấy các biến thể Combo con
    var comboVariants = await _dbContext.Products
        .AsNoTracking()
        .Where(p => p.ProductType == "Combo" && parentProductIds.Contains(p.ParentProductId))
        .ToListAsync();

    // Xác định cha nào thực sự là cha combo (có ít nhất 1 biến thể Combo)
    var parentIdsWithVariants = comboVariants
        .Select(v => v.ParentProductId)
        .Where(pid => !string.IsNullOrEmpty(pid))
        .Distinct()
        .ToList();

    var comboVariantIds = comboVariants.Select(v => v.Id).ToList();

    // Danh sách mã cần lấy công thức (chỉ ProductType = Combo)
    var comboIdsToCalculate = standaloneComboIds.Concat(comboVariantIds).Distinct().ToList();

    // === GIAI ĐOẠN 2: Batch load dữ liệu ===

    if (!comboIdsToCalculate.Any())
        return new Dictionary<Guid, (int, string)>();

    // Load công thức đang hoạt động (IsActive = true, IsDeleted = false)
    var specifications = await _dbContext.ComboSpecifications
        .AsNoTracking()
        .Include(cs => cs.Mat)
        .Where(cs => !cs.IsDeleted &&
                     cs.IsActive &&
                     comboIdsToCalculate.Contains(cs.SubId))
        .ToListAsync();

    // Gộp mã thành phần và load tồn kho
    var materialIds = specifications.Select(s => s.MatId).Distinct().ToList();
    var inventories = await _dbContext.Products
        .AsNoTracking()
        .Where(p => materialIds.Contains(p.Id))
        .Select(p => new { p.Id, Stock = (int)(p.Stock ?? 0) })
        .ToDictionaryAsync(p => p.Id, p => p.Stock);

    // === GIAI ĐOẠN 3: Tính SellableQuantity cho từng Combo ===

    var comboQuantities = new Dictionary<Guid, int>();

    foreach (var comboId in comboIdsToCalculate)
    {
        var comboSpecs = specifications.Where(cs => cs.SubId == comboId).ToList();

        // Lọc chỉ Core (IsCore = true)
        var coreSpecs = comboSpecs.Where(cs => cs.IsCore).ToList();

        if (!coreSpecs.Any())
        {
            // Không có Core nào → SellableQuantity = 0
            comboQuantities[comboId] = 0;
            continue;
        }

        // Tính min(floor(Stock / Quantity)) cho từng Core
        int minQty = int.MaxValue;
        bool hasValidCore = false;

        foreach (var spec in coreSpecs)
        {
            // Guard: Quantity <= 0 → bỏ qua
            if (spec.Quantity <= 0)
                continue;

            // Lấy tồn kho (coi như 0 nếu không tìm thấy)
            var stock = inventories.TryGetValue(spec.MatId, out var s) ? s : 0;

            int availableCombos = stock / spec.Quantity;
            minQty = Math.Min(minQty, availableCombos);
            hasValidCore = true;
        }

        comboQuantities[comboId] = hasValidCore ? minQty : 0;
    }

    // === GIAI ĐOẠN 4: Tổng hợp kết quả ===

    var result = new Dictionary<Guid, (int SellableQuantity, string StockStatus)>();

    // 4.1: Combo không biến thể
    foreach (var comboId in standaloneComboIds)
    {
        var qty = comboQuantities.TryGetValue(comboId, out var q) ? q : 0;
        result[comboId] = (qty, qty > 0 ? "Còn hàng" : "Hết hàng");
    }

    // 4.2: Cha combo - lấy min của các biến thể con
    foreach (var parentIdStr in parentIdsWithVariants)
    {
        if (!Guid.TryParse(parentIdStr, out var parentId))
            continue;

        var variants = comboVariants.Where(v => v.ParentProductId == parentIdStr).ToList();
        if (!variants.Any())
            continue;

        var minQty = variants.Min(v => comboQuantities.TryGetValue(v.Id, out var q) ? q : 0);
        result[parentId] = (minQty, minQty > 0 ? "Còn hàng" : "Hết hàng");
    }

    return result;
}
```

---

## Step 3: Cập nhật GetLocalProductsV2

### File: `HoaTheoMua.Service/Product/Service.cs`

Sửa method `GetLocalProductsV2` - sau khi lấy danh sách sản phẩm, gọi `CalculateAvailableQty`:

```csharp
public async Task<BasePaginationResponse> GetLocalProductsV2(Request.LocalProductQueryRequest request)
{
    // ... code hiện tại giữ nguyên ...

    var items = await orderedQuery
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    // === THAY ĐỔI: Tính SellableQuantity cho Combo ===

    var result = new List<Response.ProductDetailResponse>();

    if (items.Any())
    {
        // Lấy danh sách Id để tính combo
        var productIds = items.Select(p => p.Id).ToList();

        // Gọi hàm bổ trợ tính số lượng khả dụng
        var comboQuantities = await CalculateAvailableQty(productIds);

        // Map với SellableQuantity nếu có
        foreach (var product in items)
        {
            var response = MapToProductDetailResponse(product);

            if (comboQuantities.TryGetValue(product.Id, out var comboInfo))
            {
                response.SellableQuantity = comboInfo.SellableQuantity;
                response.StockStatus = comboInfo.StockStatus;
            }

            result.Add(response);
        }
    }

    return ApiResponseFactory.BasePagination(result, pageIndex, pageSize, totalCount);
}
```

---

## Step 4: Cập nhật GetLocalProductDetail

### File: `HoaTheoMua.Service/Product/Service.cs`

Sửa method `GetLocalProductDetail`:

```csharp
public async Task<Response.ProductDetailResponse> GetLocalProductDetail(Guid id)
{
    var product = await _dbContext.Products
        .Include(p => p.CategoryProductDetails)
        .ThenInclude(cpd => cpd.CategoryProduct)
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

    if (product == null)
    {
        throw new NotFoundException("PRODUCT_NOT_FOUND");
    }

    var response = MapToProductDetailResponse(product);

    // === THÊM MỚI: Tính SellableQuantity cho Combo ===

    // Kiểm tra có phải là Combo không (không biến thể hoặc có biến thể)
    var isCombo = product.ProductType == "Combo" ||
                  (product.ProductType == "Sản Phẩm" &&
                   !string.IsNullOrEmpty(product.ParentProductId) == false &&
                   await _dbContext.Products.AnyAsync(p =>
                       p.ParentProductId == product.Id.ToString() &&
                       p.ProductType == "Combo" &&
                       !p.IsDeleted));

    if (isCombo)
    {
        var comboQuantities = await CalculateAvailableQty(new List<Guid> { id });

        if (comboQuantities.TryGetValue(id, out var comboInfo))
        {
            response.SellableQuantity = comboInfo.SellableQuantity;
            response.StockStatus = comboInfo.StockStatus;
        }
    }

    return response;
}
```

---

## Step 5: Tạo Unit Test

### File: `HoaTheoMua.Service.Test/ProductTest/CalculateAvailableQtyTests.cs`

Tham khảo pattern trong `HoaTheoMua.Service.Test/ProductTest/GetListProducts.cs`

Test cases (10 tests):
1. UT-005-01: Combo không biến thể - tính đúng
2. UT-005-02: Cha combo - lấy min biến thể
3. UT-005-03: Sản phẩm thường - bỏ qua
4. UT-005-04: Không có Core → qty = 0
5. UT-005-05: Hết hàng → qty = 0
6. UT-005-06: Quantity = 0 - tránh divide by zero
7. UT-005-07: IsDeleted/IsActive = false được lọc
8. UT-005-08: Input rỗng - early return
9. UT-005-09: StockStatus logic
10. UT-005-10: Material không tồn tại trong tồn kho

---

## Verification

### 1. Build
```bash
dotnet build HoaTheoMua.sln -c Release
```

### 2. Run Tests
```bash
dotnet test HoaTheoMua.Service.Test --filter "FullyQualifiedName~CalculateAvailableQtyTests"
```

### 3. Manual Test (Swagger)
- `GET /api/v2/products` → Kiểm tra response có `SellableQuantity` và `StockStatus`
- `GET /api/v1/products/{id}` → Kiểm tra response có `SellableQuantity` và `StockStatus`

---

## Thứ tự thực hiện

1. **Step 1**: Cập nhật Response (thêm SellableQuantity, StockStatus)
2. **Step 2**: Thêm `CalculateAvailableQty` vào ProductService
3. **Step 3**: Cập nhật `GetLocalProductsV2`
4. **Step 4**: Cập nhật `GetLocalProductDetail`
5. **Step 5**: Tạo Unit Tests
6. **Build & Verify**

---

## Lưu ý

- Entity `ComboSpecification` đã có sẵn trong codebase
- Logic lọc: `IsDeleted = false` AND `IsActive = true`
- Navigation: `Sub` = Combo, `Mat` = Material (thành phần)
- Test cases reference: `docs/UT-005-CalculateAvailableQty.md` và `docs/UT-013-GetLocalProductDetail.md`

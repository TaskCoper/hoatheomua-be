# GetLocalProducts — Phân tích & Hướng dẫn sửa đổi

> File: `HoaTheoMua.Service/Product/Service.cs` (dòng ~207)
> Method: `Task<BasePaginationResponse> GetLocalProducts(Request.LocalProductQueryRequest request)`
>
> **Tài liệu này là hướng dẫn tự sửa — chưa có thay đổi code nào được thực hiện.**

---

## 1. Hàm hiện tại hoạt động như thế nào

### Request hiện tại (`Request.LocalProductQueryRequest` — `Request.cs` dòng ~87)

| Param | Kiểu | Mặc định | Ý nghĩa |
|---|---|---|---|
| `pageIndex` | `int` | 1 | Trang hiện tại (`<=0` → ép về 1) |
| `pageSize` | `int` | 10 | Kích thước trang (`<=0` → 10, tối đa 100) |
| `priceFrom` | `decimal?` | null | Lọc giá `>=` |
| `priceTo` | `decimal?` | null | Lọc giá `<=` |
| `sortBy` | `List<string>?` | null | `price` / `name`, tiền tố `-` = giảm dần, field khác fallback `CreatedAt` |
| `categoryIds` | `List<Guid>?` | null | Lọc theo danh mục (qua bảng nối `CategoryProductDetails`) |

### Luồng xử lý

1. Chuẩn hóa `pageIndex` / `pageSize`, distinct `categoryIds`.
2. Query `Products` với `AsNoTracking()` + `Include(CategoryProductDetails).ThenInclude(CategoryProduct)`.
3. Lọc soft-delete: `!x.IsDeleted` (dòng ~221).
4. Lọc `priceFrom` / `priceTo` nếu có (dòng ~223-230).
5. Lọc theo `categoryIds` — product có ít nhất 1 `CategoryProductDetail` chưa xóa thuộc danh mục (dòng ~232-237).
6. **Sắp xếp** (dòng ~239-285):
   - Nếu đúng **1** categoryId → order theo `IsPin` / `Position` **của bản ghi nối trong danh mục đó** (pin theo từng danh mục).
   - Ngược lại → order theo `IsPin` / `Position` **global trên Product**.
   - Sau đó áp `sortBy`; nếu không có → `ThenByDescending(CreatedAt)`.
7. `CountAsync()` tổng → `Skip/Take` phân trang → map từng item qua `MapToProductDetailResponse` (dòng ~313) → trả `ApiResponseFactory.BasePagination(...)`.

### Response hiện tại (`Response.ProductDetailResponse` — `Response.cs`)

- Chứa toàn bộ field sản phẩm (giá, tồn kho, kích thước, …).
- **Đã có** `Categories` (`List<CategoryProductResponse>`) được map từ `CategoryProductDetails` → `CategoryProduct` trong `MapToProductDetailResponse` (Service.cs dòng ~383-401). Tức là thông tin danh mục đã trả ra sẵn dưới key JSON `categories`.
- **Chưa có** field `isPinned`.

---

## 2. Hướng dẫn sửa — Yêu cầu 1: JSON trả thêm `categoryProduct`

Response đã map sẵn danh mục vào property `Categories`. Chỉ cần đổi tên key JSON thành `categoryProduct`.

**File:** `HoaTheoMua.Service/Product/Response.cs`

**Bước 1** — thêm using ở đầu file (nếu chưa có):

```csharp
using System.Text.Json.Serialization;
```

**Bước 2** — sửa dòng 78:

```csharp
// TRƯỚC:
public List<CategoryProductResponse> Categories { get; set; } = new List<CategoryProductResponse>();

// SAU:
[JsonPropertyName("categoryProduct")]
public List<CategoryProductResponse> Categories { get; set; } = new List<CategoryProductResponse>();
```

> ⚠️ Lưu ý: `MapToProductDetailResponse` dùng chung cho cả `GetLocalProductDetail` — đổi tên JSON sẽ ảnh hưởng cả endpoint chi tiết sản phẩm. Nếu FE đang đọc key `categories` ở nơi khác thì cân nhắc giữ nguyên tên và chỉ báo FE dùng key `categories`.

---

## 3. Hướng dẫn sửa — Yêu cầu 2: Thêm field `isPinned` vào response

### Bước 1 — Thêm property vào Response

**File:** `HoaTheoMua.Service/Product/Response.cs` — thêm vào class `ProductDetailResponse` (ví dụ ngay sau `UpdatedAt`, dòng ~76):

```csharp
public bool IsPinned { get; set; }
```

### Bước 2 — Set giá trị trong `GetLocalProducts`

`MapToProductDetailResponse` là hàm `static` dùng chung, nên **không** set `IsPin` cứng bên trong mapper. Thay vào đó set **sau khi map**, theo đúng ngữ cảnh danh mục (nhất quán với logic sắp xếp pin hiện tại).

**File:** `HoaTheoMua.Service/Product/Service.cs` — sửa dòng ~293:

```csharp
// TRƯỚC:
var mappedItems = items.Select(MapToProductDetailResponse).ToList();

// SAU:
var mappedItems = items.Select(product =>
{
    var mapped = MapToProductDetailResponse(product);

    // Filter đúng 1 danh mục → pin theo danh mục đó; ngược lại → pin global trên Product
    if (categoryIds.Count == 1)
    {
        mapped.IsPinned = product.CategoryProductDetails?
            .Where(cpd => !cpd.IsDeleted && cpd.CategoryProductId == categoryIds[0])
            .Select(cpd => cpd.IsPin)
            .FirstOrDefault() ?? false;
    }
    else
    {
        mapped.IsPinned = product.IsPin;
    }

    return mapped;
}).ToList();
```

> Ghi chú: nếu `Product.IsPin` là `bool?` thì đổi `mapped.IsPinned = product.IsPin;` thành `mapped.IsPinned = product.IsPin ?? false;`. Tương tự, nếu `cpd.IsPin` là `bool` (không nullable) thì bỏ `?? false` ở nhánh trên (compiler sẽ báo).

---

## 4. Hướng dẫn sửa — Yêu cầu 3: Param `search` theo 3 cột (tên, mã vạch, nhanhProductId)

### Bước 1 — Thêm param vào Request

**File:** `HoaTheoMua.Service/Product/Request.cs` — thêm vào class `LocalProductQueryRequest` (sau `categoryIds`, dòng ~94):

```csharp
// Tìm kiếm theo tên, mã vạch, nhanhProductId
public string? search { get; set; }
```

### Bước 2 — Thêm filter trong `GetLocalProducts`

**File:** `HoaTheoMua.Service/Product/Service.cs` — chèn **ngay sau** dòng `query = query.Where(x => !x.IsDeleted);` (dòng ~221):

```csharp
if (!string.IsNullOrWhiteSpace(request.search))
{
    var keyword = request.search.Trim();
    query = query.Where(x =>
        (x.ProductName != null && EF.Functions.ILike(x.ProductName, $"%{keyword}%")) ||
        (x.Barcode != null && EF.Functions.ILike(x.Barcode, $"%{keyword}%")) ||
        (x.NhanhProductId != null && EF.Functions.ILike(x.NhanhProductId, $"%{keyword}%")));
}
```

- `EF.Functions.ILike` = search không phân biệt hoa thường, chỉ chạy trên PostgreSQL (Npgsql) — DB của project là Postgres nên OK.
- ⚠️ **Test project** (`LocalProductSearchTest.cs`) dùng MockQueryable/InMemory → `ILike` sẽ throw khi chạy test. Nếu test fail, dùng phương án tương thích cả hai:

```csharp
if (!string.IsNullOrWhiteSpace(request.search))
{
    var keyword = request.search.Trim().ToLower();
    query = query.Where(x =>
        (x.ProductName != null && x.ProductName.ToLower().Contains(keyword)) ||
        (x.Barcode != null && x.Barcode.ToLower().Contains(keyword)) ||
        (x.NhanhProductId != null && x.NhanhProductId.ToLower().Contains(keyword)));
}
```

> `ToLower().Contains` dịch được sang SQL (`LOWER(...) LIKE ...`) và chạy được trên cả InMemory — an toàn hơn nếu muốn giữ test xanh. Khuyến nghị dùng phương án này.

### Bước 3 — Kiểm tra Controller

**File:** `HoaTheoMua.API/Controllers/ProductController.cs` — nếu endpoint nhận `LocalProductQueryRequest` qua `[FromQuery]` hoặc `[FromBody]` nguyên class thì **không cần sửa gì** — param `search` tự bind. Chỉ cần sửa nếu controller đang liệt kê từng param thủ công.

---

## 5. Tổng hợp file cần sửa

| # | File | Thay đổi |
|---|---|---|
| 1 | `Product/Response.cs` | Thêm `IsPinned`; gắn `[JsonPropertyName("categoryProduct")]` lên `Categories` |
| 2 | `Product/Request.cs` | Thêm `search` vào `LocalProductQueryRequest` |
| 3 | `Product/Service.cs` | Thêm filter search 3 cột (sau dòng ~221); set `IsPinned` khi map (dòng ~293) |
| 4 | `HoaTheoMua.Service.Test/ProductTest/LocalProductSearchTest.cs` | Bổ sung test: search theo tên / barcode / nhanhProductId, và assert `isPinned` |

Sau khi sửa, chạy:

```bash
dotnet build HoaTheoMua.sln -c Release
dotnet test HoaTheoMua.Service.Test --filter "FullyQualifiedName~LocalProductSearchTest"
```

(Git hook và CI **không** chạy test — nhớ tự chạy trước khi push.)

---

## 6. Response mẫu sau thay đổi

```json
{
  "data": [
    {
      "id": "guid",
      "nhanhProductId": "12345",
      "barcode": "8938505970012",
      "productName": "Hoa hướng dương",
      "price": 250000,
      "isPinned": true,
      "categoryProduct": [
        { "categoryId": "guid", "name": "Hoa bó", "type": 1, "isActive": true }
      ]
    }
  ],
  "pageIndex": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

## 7. Request mẫu sau thay đổi

```
GET /api/v1/products/local?pageIndex=1&pageSize=10&search=hướng dương
GET /api/v1/products/local?search=8938505970012          → khớp barcode
GET /api/v1/products/local?search=12345                  → khớp nhanhProductId
GET /api/v1/products/local?search=hoa&categoryIds=<guid> → kết hợp filter danh mục
```

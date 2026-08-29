# Đề xuất kiểm tra material support có khả năng bị thay thế

## Nghiệp vụ

- Phần validate `Items`, `ProductId` và `Number` đã được hàm bên ngoài xử lý nên helper này không kiểm tra lại.
- Đơn hàng combo vẫn được phép đặt khi các material `core` đáp ứng đủ.
- Material `support` là material có `ComboSpecification.IsCore == false` và có thể được admin thay thế.
- Chỉ xét material đang hoạt động và chưa bị xóa:
  - `Mat.IsActive == true`.
  - `Mat.IsDeleted == false`.
- Material support có khả năng bị thay thế khi:

```text
Mat.AvailableForSale < ComboSpecification.Quantity * Number
```

- Response chỉ cần:
  - Tổng số material support có khả năng bị thay thế.
  - Tên material.
  - Định lượng material lấy từ `ComboSpecification.Quantity`.

## Request (`Product/Request.cs`)

```csharp
public class GetProductByIdsItem
{
    public Guid ProductId { get; set; }
    public int Number { get; set; }
}

public class GetProductByIds
{
    public List<GetProductByIdsItem> Items { get; set; } = new();
}
```

## Response (`Product/Response.cs`)

```csharp
public class SupportMaterialReplacementResponse
{
    public int ReplaceableMaterialCount { get; set; }
    public List<ReplaceableMaterialResponse> ReplaceableMaterials { get; set; } = new();
}

public class ReplaceableMaterialResponse
{
    public string? MaterialName { get; set; }

    // Định lượng của material trong một combo.
    // Giá trị lấy trực tiếp từ ComboSpecification.Quantity.
    public decimal Quantity { get; set; }
}
```

Nếu cần gắn kết quả này vào response hiện tại của `GetProductByIds`, có thể tạo response bao ngoài:

```csharp
public class GetProductByIdsResult
{
    public List<GetProductByIdsResponse> Products { get; set; } = new();
    public int ReplaceableMaterialCount { get; set; }
    public List<ReplaceableMaterialResponse> ReplaceableMaterials { get; set; } = new();
}
```

## Helper mới trong `Product/Service.cs`

```csharp
private async Task<Response.SupportMaterialReplacementResponse>
    GetReplaceableSupportMaterialsAsync(
        IReadOnlyDictionary<Guid, int> numberByProductId)
{
    var comboIds = numberByProductId.Keys.ToList();

    var supportMaterials = await _dbContext.ComboSpecifications
        .AsNoTracking()
        .Where(specification =>
            comboIds.Contains(specification.SubId) &&
            !specification.IsCore &&
            specification.IsActive &&
            !specification.IsDeleted &&
            specification.Sub.ProductType == "Combo" &&
            specification.Sub.IsActive &&
            !specification.Sub.IsDeleted &&
            specification.Mat.IsActive &&
            !specification.Mat.IsDeleted &&
            specification.Quantity.HasValue &&
            specification.Quantity.Value > 0)
        .Select(specification => new
        {
            ComboId = specification.SubId,
            MaterialName = specification.Mat.ProductName,
            Quantity = specification.Quantity!.Value,
            AvailableForSale = specification.Mat.AvailableForSale ?? 0m
        })
        .ToListAsync();

    var replaceableMaterials = supportMaterials
        .Where(material =>
            material.AvailableForSale <
            material.Quantity * numberByProductId[material.ComboId])
        .Select(material => new Response.ReplaceableMaterialResponse
        {
            MaterialName = material.MaterialName,
            Quantity = material.Quantity
        })
        .ToList();

    return new Response.SupportMaterialReplacementResponse
    {
        ReplaceableMaterialCount = replaceableMaterials.Count,
        ReplaceableMaterials = replaceableMaterials
    };
}
```

Helper chỉ thực hiện một query xuống database và không query trong vòng lặp.

## Cách gọi từ `GetProductByIds`

Sau khi hàm ngoài đã validate request, gom các `ProductId` bị trùng và cộng `Number`:

```csharp
var numberByProductId = request.Items
    .GroupBy(item => item.ProductId)
    .ToDictionary(
        group => group.Key,
        group => group.Sum(item => item.Number));

var supportMaterialResult = await GetReplaceableSupportMaterialsAsync(
    numberByProductId);
```

Khi trả response:

```csharp
return new Response.GetProductByIdsResult
{
    Products = productsResult,
    ReplaceableMaterialCount = supportMaterialResult.ReplaceableMaterialCount,
    ReplaceableMaterials = supportMaterialResult.ReplaceableMaterials
};
```

## Ví dụ response

```json
{
  "products": [],
  "replaceableMaterialCount": 2,
  "replaceableMaterials": [
    {
      "materialName": "Hồng Julibee",
      "quantity": 5
    },
    {
      "materialName": "Tana",
      "quantity": 0.3
    }
  ]
}
```

`quantity` trong response là định lượng của material trong một combo, không phải tổng số lượng sau khi nhân với `Number`.

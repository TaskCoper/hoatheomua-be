# Đề xuất kiểm tra vật liệu support của combo

## Mục tiêu

Không nhét thêm logic vào `GetProductByIds`. Tạo một API riêng nhận danh sách sản phẩm và số lượng FE muốn mua, sau đó:

- Chỉ xử lý sản phẩm có `ProductType == "Combo"`.
- Chỉ kiểm tra `ComboSpecification` đang hoạt động, chưa bị xóa và có `IsCore == false` (material có vai trò support).
- Lấy tồn có thể bán từ `Products.AvailableForSale` của material.
- Nếu material không đủ cho tổng số combo khách mua thì đưa material đó vào danh sách có thể cần thay thế.
- Trả về số material thiếu và danh sách chi tiết material thiếu.
- Không gọi Nhanh.vn và không tạo truy vấn bên trong vòng lặp.

## Công thức đúng

Với từng dòng `ComboSpecification`:

```text
requiredQuantity = ComboSpecification.Quantity * Number FE gửi
```

Material thiếu khi:

```text
AvailableForSale < requiredQuantity
```

Nếu muốn biết material đó tự nó làm được tối đa bao nhiêu combo thì mới dùng phép chia:

```text
maxComboQuantity = floor(AvailableForSale / ComboSpecification.Quantity)
```

Không chia `AvailableForSale` cho `Quantity * Number`, vì kết quả đó không phải số combo tối đa có thể bán.

## 1. Request đề xuất (`Product/Request.cs`)

```csharp
public class CheckSupportMaterialAvailabilityRequest
{
    public List<ComboItemRequest> Items { get; set; } = new();
}

public class ComboItemRequest
{
    public Guid ProductId { get; set; }
    public int Number { get; set; }
}
```

Ví dụ request từ FE:

```json
{
  "items": [
    {
      "productId": "00000000-0000-0000-0000-000000000001",
      "number": 2
    },
    {
      "productId": "00000000-0000-0000-0000-000000000002",
      "number": 1
    }
  ]
}
```

Nếu FE gửi trùng `ProductId`, service sẽ cộng các giá trị `Number` trước khi tính tồn kho.

## 2. Response đề xuất (`Product/Response.cs`)

```csharp
public class CheckSupportMaterialAvailabilityResponse
{
    public int ReplaceableSupportMaterialCount { get; set; }
    public List<ReplaceableSupportMaterialResponse> ReplaceableSupportMaterials { get; set; } = new();
}

public class ReplaceableSupportMaterialResponse
{
    public Guid MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public decimal AvailableForSale { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal MissingQuantity { get; set; }

    // Giữ được Quantity gốc trong ComboSpecification và combo nào đang dùng material.
    public List<SupportMaterialUsageResponse> Usages { get; set; } = new();
}

public class SupportMaterialUsageResponse
{
    public Guid ComboId { get; set; }
    public string? ComboName { get; set; }

    // Số material cần cho một combo, lấy từ ComboSpecification.Quantity.
    public decimal Quantity { get; set; }

    // Số combo FE muốn mua.
    public int Number { get; set; }

    // Quantity * Number.
    public decimal RequiredQuantity { get; set; }
}
```

`ReplaceableSupportMaterialCount` đếm số material thiếu duy nhất theo `MatId`, không đếm lặp khi cùng một material được sử dụng trong nhiều combo.

## 3. Khai báo service (`Product/IService.cs`)

```csharp
Task<Response.CheckSupportMaterialAvailabilityResponse> CheckSupportMaterialAvailability(
    Request.CheckSupportMaterialAvailabilityRequest request);
```

## 4. Hàm service đề xuất (`Product/Service.cs`)

```csharp
public async Task<Response.CheckSupportMaterialAvailabilityResponse> CheckSupportMaterialAvailability(
    Request.CheckSupportMaterialAvailabilityRequest request)
{
    if (request?.Items == null || request.Items.Count == 0)
    {
        throw new BadRequestException("Items are required");
    }

    if (request.Items.Any(x => x.ProductId == Guid.Empty || x.Number <= 0))
    {
        throw new BadRequestException(
            "Each item must have a valid ProductId and Number greater than 0");
    }

    // FE có thể gửi nhiều dòng trùng ProductId, vì vậy phải cộng Number trước.
    var requestedNumberByComboId = request.Items
        .GroupBy(x => x.ProductId)
        .ToDictionary(
            group => group.Key,
            group => group.Sum(item => item.Number));

    var requestedProductIds = requestedNumberByComboId.Keys.ToList();

    // Chỉ một query DB:
    // - Lọc combo trong request.
    // - Lọc specification support.
    // - Lấy luôn tên combo, tên material và AvailableForSale của material.
    //
    // Không lọc Mat.IsActive/Mat.IsDeleted khỏi query, vì material không hoạt động
    // hoặc đã xóa vẫn phải được xem là tồn bằng 0 và báo cần thay thế.
    var supportSpecifications = await _dbContext.ComboSpecifications
        .AsNoTracking()
        .Where(specification =>
            requestedProductIds.Contains(specification.SubId) &&
            !specification.IsDeleted &&
            specification.IsActive &&
            !specification.IsCore &&
            specification.Quantity.HasValue &&
            specification.Quantity.Value > 0 &&
            !specification.Sub.IsDeleted &&
            specification.Sub.IsActive &&
            specification.Sub.ProductType == "Combo")
        .Select(specification => new
        {
            ComboId = specification.SubId,
            ComboName = specification.Sub.ProductName,
            MaterialId = specification.MatId,
            MaterialName = specification.Mat.ProductName,
            QuantityPerCombo = specification.Quantity!.Value,
            MaterialIsUsable = !specification.Mat.IsDeleted && specification.Mat.IsActive,
            MaterialAvailableForSale = specification.Mat.AvailableForSale ?? 0m
        })
        .ToListAsync();

    // Nếu dữ liệu có nhiều specification trùng ComboId + MatId thì cộng Quantity,
    // tránh trả trùng material và tránh tính thiếu nhu cầu thực tế.
    var requirementsByComboAndMaterial = supportSpecifications
        .GroupBy(x => new { x.ComboId, x.MaterialId })
        .Select(group =>
        {
            var first = group.First();
            var quantityPerCombo = group.Sum(x => x.QuantityPerCombo);
            var requestedNumber = requestedNumberByComboId[group.Key.ComboId];

            return new
            {
                group.Key.ComboId,
                first.ComboName,
                group.Key.MaterialId,
                first.MaterialName,
                QuantityPerCombo = quantityPerCombo,
                Number = requestedNumber,
                RequiredQuantity = quantityPerCombo * requestedNumber,
                AvailableForSale = first.MaterialIsUsable
                    ? Math.Max(0m, first.MaterialAvailableForSale)
                    : 0m
            };
        })
        .ToList();

    // Quan trọng: gom tiếp theo MatId trên toàn bộ request.
    // Một material có thể được nhiều combo cùng sử dụng; không được dùng lại cùng
    // một AvailableForSale để kết luận riêng từng combo rằng tất cả đều đủ.
    var replaceableMaterials = requirementsByComboAndMaterial
        .GroupBy(x => x.MaterialId)
        .Select(group =>
        {
            var first = group.First();
            var requiredQuantity = group.Sum(x => x.RequiredQuantity);
            var availableForSale = first.AvailableForSale;

            return new Response.ReplaceableSupportMaterialResponse
            {
                MaterialId = group.Key,
                MaterialName = first.MaterialName,
                AvailableForSale = availableForSale,
                RequiredQuantity = requiredQuantity,
                MissingQuantity = Math.Max(0m, requiredQuantity - availableForSale),
                Usages = group
                    .OrderBy(x => x.ComboName)
                    .Select(x => new Response.SupportMaterialUsageResponse
                    {
                        ComboId = x.ComboId,
                        ComboName = x.ComboName,
                        Quantity = x.QuantityPerCombo,
                        Number = x.Number,
                        RequiredQuantity = x.RequiredQuantity
                    })
                    .ToList()
            };
        })
        .Where(x => x.MissingQuantity > 0)
        .OrderBy(x => x.MaterialName)
        .ToList();

    return new Response.CheckSupportMaterialAvailabilityResponse
    {
        ReplaceableSupportMaterialCount = replaceableMaterials.Count,
        ReplaceableSupportMaterials = replaceableMaterials
    };
}
```

## 5. Endpoint đề xuất (`ProductController.cs`)

```csharp
[HttpPost("/api/v1/products/check-support-materials")]
public async Task<IActionResult> CheckSupportMaterialAvailability(
    [FromBody] Request.CheckSupportMaterialAvailabilityRequest request)
{
    var result = await _productService.CheckSupportMaterialAvailability(request);
    return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
}
```

## 6. Ví dụ response

```json
{
  "replaceableSupportMaterialCount": 1,
  "replaceableSupportMaterials": [
    {
      "materialId": "00000000-0000-0000-0000-000000000010",
      "materialName": "Hồng Julibee",
      "availableForSale": 8,
      "requiredQuantity": 10,
      "missingQuantity": 2,
      "usages": [
        {
          "comboId": "00000000-0000-0000-0000-000000000001",
          "comboName": "Bó Coral Pink L.2026",
          "quantity": 5,
          "number": 2,
          "requiredQuantity": 10
        }
      ]
    }
  ]
}
```

Nếu tất cả material support đều đủ, response luôn ổn định cho FE:

```json
{
  "replaceableSupportMaterialCount": 0,
  "replaceableSupportMaterials": []
}
```

## 7. Lưu ý trước khi apply

- Proposal này không sửa `GetProductByIds`; FE gọi endpoint mới bằng danh sách `{ productId, number }`.
- `IsCore == false` được hiểu là vai trò support, dựa trên quy ước hiện có của `CheckInventoryAvailability`, nơi `IsCore == true` dùng cho vật liệu bắt buộc.
- Sản phẩm thường trong request được bỏ qua; chỉ những ID thực sự là combo và có support specification mới được tính.
- Material inactive hoặc deleted vẫn xuất hiện trong kết quả thiếu với `AvailableForSale = 0`, thay vì bị lọc mất.
- Số lượng thiếu được gom theo `MatId` trên toàn bộ giỏ hàng để không dùng trùng cùng một lượng tồn cho nhiều combo.
- Nếu nghiệp vụ yêu cầu sản phẩm cha phải dùng specification của các combo con như `CheckInventoryAvailability` hiện tại, cần bổ sung bước ánh xạ `ParentProductId -> child combo Id` trước query. Với yêu cầu hiện tại, code dùng trực tiếp `ComboSpecification.SubId == ProductId` FE gửi.

# US-038 — Khởi tạo Checkout từ mẫu hoa AI

## I. Các API cần làm

US-038 chỉ kiểm tra một **mẫu hoa AI** có còn được phép đi vào flow Checkout cũ hay không.

Không tạo Checkout mới, không tạo Order, không lưu dữ liệu tạm và không xử lý thanh toán ở US này.

Flow Checkout hiện có vẫn giữ nguyên 5 bước:

```text
Nhập thông tin cá nhân
→ Chọn thiệp (có thể bỏ qua)
→ Tính tổng tiền
→ Xem lại toàn bộ thông tin
→ Thanh toán
```

Khác biệt chỉ nằm ở lúc customer bấm **Đặt hàng ngay** từ ảnh hoa AI:

```text
Customer chọn ảnh hoa AI
→ FE gọi API kiểm tra mẫu hoa
→ API tìm Combo nguồn của ảnh hoa
→ Combo nguồn còn bán được
→ FE dùng Combo đó để đi vào flow Checkout cũ
```

Chỉ cần 1 API:

```http
GET /api/v1/generated-flowers/{generatedFlowerId}/checkout-availability
```

- FE gọi khi customer bấm **Đặt hàng ngay** trên một mẫu hoa AI.
- API trả `rootComboId` và trạng thái vật liệu của Combo nguồn.
- Nếu Core còn đủ hàng, FE dùng `rootComboId` để tiếp tục Checkout/order flow hiện có như khi customer chọn Combo bình thường.
- Nếu Support thiếu hàng, vẫn cho đi tiếp và trả vật liệu cần thay thế để FE thông báo.
- Nếu Combo không còn bán được hoặc Core không đủ hàng, không cho đi vào Checkout.

## II. Code từng API

### API 1 — Kiểm tra mẫu hoa AI trước Checkout

**Vai trò:** xác định Combo nguồn của ảnh hoa AI và kiểm tra Combo đó còn có thể bán được hay không.

### IService

```csharp
namespace HoaTheoMua.Service.GeneratedFlowerCheckout;

public interface IService
{
    Task<Response.CheckoutAvailabilityResponse> GetCheckoutAvailability(
        Guid generatedFlowerId);
}
```

### Request

```http
GET /api/v1/generated-flowers/f1000000-0000-0000-0000-000000000001/checkout-availability
Authorization: Bearer {customer-token}
```

`generatedFlowerId` là route parameter nên không cần Request DTO.

### Response

```csharp
namespace HoaTheoMua.Service.GeneratedFlowerCheckout;

public class Response
{
    public class CheckoutAvailabilityResponse
    {
        public Guid GeneratedFlowerId { get; set; }
        public Guid RootComboId { get; set; }
        public decimal SellableQuantityCombo { get; set; }
        public List<MaterialResponse> Materials { get; set; } = new();
    }

    public class MaterialResponse
    {
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsCore { get; set; }
        public bool NeedsReplacement { get; set; }
    }
}
```

Ví dụ response khi Core còn hàng nhưng có một Support cần thay:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "generatedFlowerId": "f1000000-0000-0000-0000-000000000001",
    "rootComboId": "c1000000-0000-0000-0000-000000000001",
    "sellableQuantityCombo": 2,
    "materials": [
      {
        "materialId": "m1000000-0000-0000-0000-000000000001",
        "quantity": 3,
        "isCore": true,
        "needsReplacement": false
      },
      {
        "materialId": "m2000000-0000-0000-0000-000000000001",
        "quantity": 1,
        "isCore": false,
        "needsReplacement": true
      }
    ]
  }
}
```

### Service

```csharp
public async Task<Response.CheckoutAvailabilityResponse> GetCheckoutAvailability(
    Guid generatedFlowerId)
{
    var userId = _currentUser.GetRequiredUserId();

    var flower = await _dbContext.GeneratedFlowers
        .AsNoTracking()
        .Where(x => x.Id == generatedFlowerId && x.UserId == userId)
        .Select(x => new { x.Id })
        .FirstOrDefaultAsync();

    if (flower == null)
    {
        throw new NotFoundException("Không tìm thấy mẫu hoa AI");
    }

    var rootComboId = await _dbContext.ClientHistories
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.Type == "flower" &&
            x.OutputId == generatedFlowerId)
        .Select(x => x.Root)
        .FirstOrDefaultAsync();

    if (rootComboId == Guid.Empty)
    {
        throw new BadRequestException(
            "Không xác định được Combo nguồn của mẫu hoa AI");
    }

    var limit = await _orderService.GetLimitCoreProduct(rootComboId);

    if (limit.SellableQuantityCombo < 1)
    {
        throw new BadRequestException(
            "Combo hiện không còn đủ thành phần chính để đặt hàng. Vui lòng chọn mẫu hoa khác.");
    }

    return new Response.CheckoutAvailabilityResponse
    {
        GeneratedFlowerId = flower.Id,
        RootComboId = rootComboId,
        SellableQuantityCombo = limit.SellableQuantityCombo,
        Materials = limit.Materials.Select(x => new Response.MaterialResponse
        {
            MaterialId = x.MatId,
            Quantity = x.Quantity,
            IsCore = x.IsCore,
            NeedsReplacement = x.NeedsReplacement
        }).ToList()
    };
}
```

Ý chính của hàm:

```text
1. Kiểm tra ảnh hoa AI có thuộc customer hiện tại.
2. Từ Flower History của ảnh đó, lấy root = Combo nguồn.
3. Gọi lại GetLimitCoreProduct(rootComboId).
4. Core đủ hàng thì trả rootComboId để FE mở flow Checkout cũ.
5. Support thiếu hàng không chặn; trả NeedsReplacement để FE báo trước.
```

`GetLimitCoreProduct(rootComboId)` là hàm dùng chung cho cả hai trường hợp:

```text
Customer chọn Combo bình thường
→ flow Order/Checkout cũ gọi GetLimitCoreProduct(comboId)

Customer chọn ảnh hoa AI
→ API US-038 resolve rootComboId
→ gọi GetLimitCoreProduct(rootComboId)
→ quay về flow Order/Checkout cũ
```

## III. DB còn thiếu — chỉ phần cần đồng bộ

US-038 không cần bảng Checkout mới.

Theo docs, `client_histories.root` lưu Product/Combo nguồn của Flower AI. Nhưng entity `ClientHistory` hiện tại chưa có property `Root`, nên cần bổ sung property/mapping này để API lấy đúng Combo nguồn:

```csharp
public Guid Root { get; set; }
```

Không dùng `generated_flowers.input_snapshot` để tìm Combo nguồn. Docs đã chốt `root` là nguồn chuẩn cho flow này.


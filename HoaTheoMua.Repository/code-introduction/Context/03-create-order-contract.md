# 03 — Contract CreateOrder

## 1. Ý tưởng chính

CreateOrder giữ flow Order cũ. Khác biệt chỉ là mỗi item có thể:

- đặt Product/Combo bình thường;
- đặt Combo theo một `GeneratedFlowerId` đã chọn;
- có 0, 1 hoặc nhiều lựa chọn thiệp.

```text
Items[]
→ mỗi item tạo một OrderDetails
→ item.Cards[] tự thuộc OrderDetails của item đó
```

Do `Cards` đã nằm trong `Items`, Card **không có** `itemId` riêng để map sang dòng hoa.

## 1.1. Hai nguồn checkout loại trừ nhau

Ở giao diện, customer chỉ được bắt đầu Checkout theo một trong hai cách:

```text
A. Chọn Product/Combo đang bán bình thường
B. Chọn một GeneratedFlower AI đã tồn tại
```

Không có flow vừa chọn Combo trực tiếp, vừa tạo/chọn Flower AI ngay trong Checkout đó. Tạo Flower AI là flow riêng diễn ra trước; chỉ sau khi đã có ảnh thì customer mới có thể bắt đầu Checkout từ ảnh đó.

Về mặt request, `productId` vẫn luôn có vì Order cuối cùng phải mua Product/Combo thật:

```text
GeneratedFlowerId = null
→ nguồn hiển thị/lựa chọn là Product hoặc Combo ở productId.

GeneratedFlowerId có giá trị
→ nguồn hiển thị/lựa chọn là ảnh Flower AI;
→ productId chỉ là Combo gốc để BE đối chiếu và tạo OrderDetails.
```

Vì vậy cùng một item có `productId` và `generatedFlowerId` không có nghĩa customer đã chọn hai nguồn. Nó biểu diễn một lựa chọn là Flower AI và Combo gốc bắt buộc để fulfillment.

## 2. Request FE gửi

Phần thông tin giao hàng/thanh toán hiện có giữ nguyên. Phần bổ sung quan trọng:

```csharp
public class Items
{
    public int ItemId { get; set; }             // Item hiện có trong flow giỏ hàng
    public Guid ProductId { get; set; }         // Product/Combo khách mua
    public int Number { get; set; }             // số lượng Product/Combo
    public Guid? GeneratedFlowerId { get; set; }
    public List<CardSelectionRequest> Cards { get; set; } = [];
}

public class CardSelectionRequest
{
    public required string CardMode { get; set; } // handmade | ai | history
    public required Guid CardTemplateId { get; set; }
    public string? SizeName { get; set; }         // ai/history
    public string? FormType { get; set; }         // go_may | calligraphy; handmade BE tự cố định
    public string? MessageContent { get; set; }
    public string? AttachedImageUrl { get; set; }
    public int Quantity { get; set; }
    public Guid? RegeneratedFromCardId { get; set; }
}
```

Ví dụ item checkout từ ảnh Flower AI và chọn hai thiệp:

```json
{
  "itemId": 1833667,
  "productId": "440e8400-e29b-41d4-a716-446655440001",
  "number": 2,
  "generatedFlowerId": "fa000000-0000-0000-0000-000000000001",
  "cards": [
    {
      "cardMode": "ai",
      "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
      "sizeName": "L",
      "formType": "calligraphy",
      "messageContent": "Chúc mẹ luôn mạnh khỏe",
      "attachedImageUrl": null,
      "quantity": 1,
      "regeneratedFromCardId": null
    },
    {
      "cardMode": "history",
      "cardTemplateId": "550e8400-e29b-41d4-a716-446655440002",
      "sizeName": "L",
      "formType": "go_may",
      "messageContent": "Chúc em luôn hạnh phúc",
      "attachedImageUrl": null,
      "quantity": 1,
      "regeneratedFromCardId": "ca000000-0000-0000-0000-000000000001"
    }
  ]
}
```

## 3. Ba ID phải hiểu đúng

| Field | Là gì | Có dùng để kiểm tra Combo nguồn? |
|---|---|---|
| `itemId` | Mã dòng item/giỏ hàng hiện có. | Không. |
| `productId` | Product/Combo khách thực sự mua. | Có. Đây là Combo ID cần đối chiếu. |
| `generatedFlowerId` | Ảnh Flower AI khách đã chọn. | Có, để lấy History và RootID. |

Khi checkout từ ảnh hoa AI, FE phải gửi **đủ cả ba**. Khi đặt bình thường, `generatedFlowerId` là `null`/không gửi.

## 4. Validate item khi có GeneratedFlowerId

```text
GeneratedFlowerId có giá trị
→ ProductId bắt buộc là ProductType = Combo
→ Flower phải thuộc customer hiện tại
→ Flower phải có ClientHistory Type = flower hợp lệ
→ History.RootID phải bằng ProductId
→ Combo/Core phải còn có thể bán tại thời điểm tạo Order
→ lưu GeneratedFlowerId vào OrderDetails.GeneratedFlowerId
```

Không được hiểu `itemId` là Combo ID. Cũng không lấy Combo chỉ từ FE gửi `generatedFlowerId`; BE phải đối chiếu `History.RootID` với `productId`.

## 5. Validate Card khi tạo Order

Mỗi `CardSelectionRequest` nằm trong `item.Cards`:

```text
→ kiểm tra Template active/chưa xóa và đúng loại Card
→ kiểm tra mode, Size/Form/Message/Quantity theo mode
→ nếu RegeneratedFromCardId có giá trị:
     Card nguồn phải là Card AI, thuộc customer và có Card History hợp lệ
     HandMade không được dùng làm nguồn regenerate AI
→ BE tự tính giá
→ BE tự dựng snapshot
```

`regeneratedFromCardId` là `GeneratedCard.Id` cũ FE lấy được từ màn hình History:

```text
null        → AI Card mới từ Product/GeneratedFlower của item
có giá trị  → AI Card mới dựa trên Card AI cũ đã chọn
```

Khách có thể xem nội dung Card cũ rồi sửa Template, Size, Form hoặc lời chúc trước khi đặt. Giá được tính lại theo lựa chọn mới; không bê giá Card cũ sang.

## 6. Field FE tuyệt đối không gửi

```text
source
cardConfig
wordCount
basePrice
extraPrice
totalPrice
systemPrompt
validatedAt
SnapshotCard
GeneratedCardIds
```

BE tự lấy hoặc tự tính những field này để ngăn FE sửa giá, Prompt, nguồn hoặc dữ liệu đã chốt.

Tài liệu request đầy đủ hơn: [../US-039/requestOrder.md](../US-039/requestOrder.md).

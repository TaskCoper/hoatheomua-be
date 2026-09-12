# requestOrder

## Mục đích

Tài liệu này chốt phần request của `CreateOrder` cho flow đặt hoa có thể kèm ảnh hoa AI và thiệp.

Nguyên tắc đơn giản:

```text
FE chỉ gửi lựa chọn của khách.
BE tự validate, tự tính giá, tự tạo SnapshotCard.
```

FE không được gửi giá, Prompt hoặc SnapshotCard đã tự dựng.

---

## Các field hiện có của CreateOrder

`CreateOrderRequest` hiện kế thừa thông tin giao hàng:

| Field | Ý nghĩa |
|---|---|
| `latitude`, `longitude` | Toạ độ giao hàng. |
| `ward`, `city` | Khu vực giao hàng. |
| `address`, `shortAddress` | Địa chỉ giao chi tiết và địa chỉ rút gọn. |
| `fullName`, `phone` | Thông tin người đặt theo flow hiện tại. |
| `fullNameReceiver`, `phoneReceiver` | Thông tin người nhận. |
| `remarks` | Ghi chú giao hàng. |
| `orderTime` | Thời gian giao hàng. |
| `gateway`, `paymentGateway`, `paymentMethod` | Phương thức thanh toán. |
| `shippingGateway` | Đơn vị giao hàng. |
| `isExportInvoice` và các field hoá đơn | Chỉ dùng khi khách yêu cầu xuất hoá đơn. |
| `items` | Danh sách Product/Combo khách đặt. |

Các field này giữ nguyên. Phần cần bổ sung là `generatedFlowerId` và `cards` ở mỗi item.

---

## 1. Bổ sung vào Item đặt hoa

Mỗi item hiện tạo ra một `OrderDetails`. Vì vậy ảnh hoa AI được chọn phải nằm ở chính item đó.

```csharp
public class Items
{
    public int ItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Number { get; set; }

    // null: khách đặt Product/Combo bình thường.
    // Có giá trị: khách đặt Combo theo ảnh hoa AI đã chọn.
    public Guid? GeneratedFlowerId { get; set; }

    // Có thể rỗng: thiệp là lựa chọn không bắt buộc.
    // Các thiệp trong đây mặc định thuộc OrderDetails tạo từ item này.
    public List<CardSelectionRequest> Cards { get; set; } = [];
}
```

| Field | FE gửi khi nào | BE dùng để làm gì |
|---|---|---|
| `itemId` | Luôn có | Mã Item hiện có trong flow đặt hàng. Các thiệp đã nằm trực tiếp trong item này nên không cần dùng mã này để ghép thiệp với OrderDetails. |
| `productId` | Luôn có | Product/Combo khách thực sự mua. |
| `number` | Luôn có | Số lượng Product/Combo. Đây là số bó hoa, không phải số thiệp. |
| `generatedFlowerId` | Chỉ khi checkout từ ảnh hoa AI | Lưu vào `OrderDetails.GeneratedFlowerId`. BE kiểm tra ảnh thuộc customer, có Flower History và `RootID` trùng Combo `productId` được đặt. |

Rule quan trọng:

```text
GeneratedFlowerId chỉ được dùng khi ProductId là Combo.
```

---

## 2. Bổ sung danh sách thiệp vào từng Item

```csharp
public class CardSelectionRequest
{
    // handmade | ai | history.
    // BE vẫn phải đối chiếu với Template/Card nguồn, không tin riêng field này.
    public required string CardMode { get; set; }

    // Template khách chọn hoặc Template muốn dùng lại khi tạo từ History.
    public required Guid CardTemplateId { get; set; }

    // Bắt buộc với ai/history; null với handmade.
    public string? SizeName { get; set; }

    // go_may | calligraphy với ai/history.
    // handmade không cần gửi: BE cố định là calligraphy.
    public string? FormType { get; set; }

    public string? MessageContent { get; set; }

    // Chỉ áp dụng cho Card AI/history theo rule hiện tại.
    public string? AttachedImageUrl { get; set; }

    // Số bản in của đúng thiết kế thiệp này.
    public int Quantity { get; set; }

    // null: tạo Card AI mới.
    // Có giá trị: khách chọn GeneratedCard AI cũ từ History để làm lại.
    public Guid? RegeneratedFromCardId { get; set; }
}
```

### Ý nghĩa từng field Card

| Field | Ý nghĩa dễ hiểu |
|---|---|
| `cardMode` | FE đang ở lựa chọn thiệp thường, thiệp AI mới hay thiệp từ lịch sử. Nó chỉ giúp phân nhánh; BE vẫn kiểm tra dữ liệu thật. |
| `cardTemplateId` | Template khách nhìn thấy và chọn. BE lấy tên, loại, ảnh Template từ DB để snapshot. |
| `sizeName` | Size khách chọn cho thiệp AI hoặc Card từ History. Thiệp HandMade không dùng Size theo rule hiện tại. |
| `formType` | `go_may` hoặc `calligraphy`. Gõ máy chỉ tính giá Size; viết tay tính thêm theo số từ. |
| `messageContent` | Lời chúc. BE tự đếm từ, chọn bậc giá và đưa vào SnapshotCard. |
| `attachedImageUrl` | Ảnh khách muốn đính kèm vào Card AI, nếu có. HandMade không dùng field này. |
| `quantity` | Số bản in của thiết kế thiệp này. Không dùng `Items[].number` vì số bó hoa và số thiệp có thể khác nhau. |
| `regeneratedFromCardId` | ID Card AI cũ khách chọn từ lịch sử. `null` là tạo mới; có giá trị là tạo lại dựa trên Card đó. |

Không còn `senderName` và `receiverName` trong request Card.

---

## 3. Request FE gửi thực tế

Ví dụ khách mua hai bó từ ảnh hoa AI; một thiệp AI mới gửi mẹ và một thiệp tạo lại từ Card cũ gửi vợ.

```json
{
  "gateway": "Sepay",
  "paymentGateway": "Sepay",
  "shippingGateway": "Ahamove",
  "paymentMethod": "VA",

  "latitude": 10.7769,
  "longitude": 106.7009,
  "ward": 26734,
  "city": 79,
  "address": "12 Nguyễn Huệ, Quận 1, TP.HCM",
  "shortAddress": "12 Nguyễn Huệ, Quận 1",
  "fullName": "Nguyễn An",
  "phone": "0900000000",
  "remarks": "Giao giờ hành chính",
  "orderTime": 0,

  "fullNameReceiver": "Nguyễn An",
  "phoneReceiver": "0900000000",
  "isExportInvoice": false,

  "items": [
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
  ]
}
```

---

## 4. CreateOrder xử lý request này như thế nào

```text
1. Lấy Items.
   → Mỗi item tạo một OrderDetails.

2. Nếu item có GeneratedFlowerId.
   → Kiểm tra Flower thuộc customer.
   → Kiểm tra Flower History.
   → Lấy RootID và xác nhận nó khớp ProductId/Combo khách đang mua.
   → Lưu ID này vào column OrderDetails.GeneratedFlowerId.

3. Lấy item.Cards.
   → Đây là các thiệp thuộc OrderDetails vừa tạo từ chính item đó.

4. Với từng Card.
   → Kiểm tra Template.
   → Nếu RegeneratedFromCardId có giá trị: kiểm tra Card AI cũ thuộc customer và có History hợp lệ.
   → Xác định source từ item hoặc từ Card History cũ.
   → Gọi CalculateCardPrice để BE tự tính giá.
   → Lấy System Prompt hiện hành.
   → Tạo một object SnapshotCard đầy đủ, gồm cả Quantity của thiết kế thiệp đó.

5. Gom các object Card thành mảng JSON.
   → Lưu vào OrderDetails.SnapshotCard.
   → Snapshot chưa có GeneratedCardId.
   → Mỗi snapshot có Quantity để Card/đóng gói biết số bản cần tạo/in.

6. Cộng tổng giá thiệp vào Order.TotalFee.
   → Khách thanh toán theo tổng tiền này.

7. Sau khi payment thành công.
   → Card AI Service đọc trọn mảng SnapshotCard của OrderDetails.
   → Lần lượt tạo GeneratedCard/History cho từng snapshot AI.
   → Service nội bộ thêm toàn bộ ID Card tạo thành công vào OrderDetails.GeneratedCardIds.
```

---

## 5. Field FE không được gửi

Các field sau luôn do BE tự lấy, tự tính hoặc tự lưu:

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

Lý do: nếu FE gửi các field này thì khách có thể sửa giá, Prompt hoặc làm sai dữ liệu đã chốt.

---

## 6. Những rule cần nhớ

```text
GeneratedFlowerId khác null
→ ProductId bắt buộc là Combo.

RegeneratedFromCardId = null
→ tạo Card AI mới từ Product/GeneratedFlower của item.

RegeneratedFromCardId có giá trị
→ Card AI mới được tạo dựa trên Card cũ khách đã chọn từ History.

Thiệp HandMade
→ tạo trước thanh toán, không gọi AI, luôn calligraphy, không dùng Size.

Thiệp AI mới và thiệp từ History
→ tạo sau thanh toán, dùng SnapshotCard đã chốt giá.

GeneratedCardIds
→ không do FE gửi; chỉ có sau khi Card tạo thành công.
```

Theo docs hiện tại, HandMade có thể xuất hiện trong lịch sử, nhưng không được dùng làm source cho flow regenerate AI. Nếu sau này muốn khách dùng lại HandMade từ History để tạo một Card mới, cần có rule riêng cho flow đó.

# 04 — Giá thiệp, SnapshotCard và tạo Card sau Order

## 1. Ba loại lựa chọn thiệp

| Mode | Bản chất | Khi tạo GeneratedCard |
|---|---|---|
| `handmade` | Thiệp thường/có sẵn, không gọi AI. | Ngay trong `CreateOrder`, khi Order còn Pending. |
| `ai` | Thiệp AI tạo mới. | Sau payment thành công. |
| `history` | Một Card AI mới dựa trên Card AI cũ customer chọn từ History. | Sau payment thành công. |

`history` không sửa Card cũ. Nó tạo một `GeneratedCard` và một History mới.

## 2. Giá thiệp đã chốt

| Mode | Size | Form | Giá một bản |
|---|---|---|---|
| `handmade` | Không dùng | luôn `calligraphy` | chỉ theo bậc giá số từ. |
| `ai` | Bắt buộc | `go_may` | `size.base_price`. |
| `ai` | Bắt buộc | `calligraphy` | `size.base_price + cardConfig.extra_price`. |
| `history` | Bắt buộc | `go_may` | `size.base_price`. |
| `history` | Bắt buộc | `calligraphy` | `size.base_price + cardConfig.extra_price`. |

```text
lineTotal = unitPrice × card.quantity
```

BE có một hàm tính giá dùng chung cho FE báo giá và `CreateOrder` chốt giá. `CreateOrder` phải gọi lại hàm này, không tin tổng tiền FE gửi.

Với `go_may`, `cardConfig = null`. Với `calligraphy`, BE đếm số từ, kiểm tra giới hạn Size (nếu có Size) và chọn rule giá đúng.

Tài liệu code minh họa hàm: [../US-039/CalculateCardPrice.md](../US-039/CalculateCardPrice.md).

## 3. SnapshotCard trong OrderDetails

`OrderDetails.SnapshotCard` là **một cột JSON riêng**, có giá trị là mảng object. Nó không chứa `GeneratedCardId` và không được FE gửi.

Mỗi object trong mảng giữ đủ dữ liệu đã chốt để Card AI có thể tạo đúng thiết kế và đúng giá sau payment:

```json
{
  "source": {
    "type": "product | generatedFlower",
    "id": "440e8400-e29b-41d4-a716-446655440001",
    "imageUrl": "https://storage.example.com/products/flower.png"
  },
  "regeneratedFromCardId": null,
  "cardTemplate": {
    "templateId": "550e8400-e29b-41d4-a716-446655440001",
    "templateName": "Thiệp hoa cổ điển",
    "templateType": "ai",
    "templateImageUrl": "https://storage.example.com/templates/card-ai.png"
  },
  "cardSize": {
    "name": "Lớn",
    "width": 15,
    "height": 25,
    "base_price": 15000,
    "max_words": 100
  },
  "cardConfig": {
    "min_words": 36,
    "max_words": 70,
    "extra_price": 39000
  },
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "formType": "calligraphy",
  "attachedImageUrl": null,
  "quantity": 1,
  "wordCount": 5,
  "basePrice": 15000,
  "extraPrice": 0,
  "totalPrice": 15000,
  "systemPrompt": {
    "type": "card",
    "content": "Nội dung prompt đầy đủ"
  },
  "validatedAt": "2026-09-11T08:00:00Z"
}
```

Rule của object này:

- Không có `senderName`, `receiverName`.
- Không có `generatedCardId`.
- Không có `rawImage`; lúc chốt Order chưa gọi AI.
- `source.type` là `product` hoặc `generatedFlower`.
- Nếu `formType = go_may`, `cardConfig` phải là `null`.
- `regeneratedFromCardId` là `null` khi tạo AI mới; có ID Card AI cũ khi khách chọn từ History.
- `quantity` là số bản in của **một thiết kế snapshot**. `basePrice` và `extraPrice` là giá một bản; `totalPrice = (basePrice + extraPrice) × quantity`.

Snapshot là giá và dữ liệu đã chốt. Sau payment, Card service dùng snapshot này, không query/tính lại giá từ Config hiện hành.

## 4. CreateOrder và Card lifecycle

### A. Không có Card

```text
CreateOrder tạo OrderDetails
→ GeneratedCardIds = []
```

### B. HandMade

```text
CreateOrder
→ tạo Order và OrderDetails Pending
→ tạo GeneratedCard HandMade
→ tạo ClientHistory Type = handmade_card, OutputId = GeneratedCard.Id
→ thêm GeneratedCard.Id vào OrderDetails.GeneratedCardIds
→ trả kết quả tạo Order
```

Các bước trên nên chung transaction: lỗi ở đâu thì không để lại Order/Card/History dở dang.

### C. AI mới hoặc từ History

```text
CreateOrder
→ validate, tính giá và lưu SnapshotCard
→ GeneratedCardIds = [] vì chưa có Card thật
→ customer thanh toán
→ webhook xác nhận Order Paid
→ BE gọi Card AI service theo từng OrderDetails/SnapshotCard
→ Card service lần lượt xử lý từng object trong mảng SnapshotCard
→ mỗi object AI tạo một GeneratedCard + ClientHistory
→ service nội bộ thêm toàn bộ ID tạo thành công vào OrderDetails.GeneratedCardIds
```

Đây là BE gọi BE. FE không gọi `UpdateOrderById` để thêm `GeneratedCardIds`, vì FE không được quyền quyết định ID Card nào thuộc Order.

Nếu Card AI thất bại sau payment: Order vẫn đã Paid; snapshot giữ nguyên để retry tạo lại cùng dữ liệu/giá, không tính lại giá và không tạo History/GeneratedCard giả.

Vì SnapshotCard không có `generatedCardId` hay mã item nội bộ, cách đúng là xử lý **trọn mảng snapshot của một OrderDetails trong một lần job/service**. Không tách request theo từng object rồi cố map ngược từ FE.

Nếu một snapshot có `quantity = 2`, AI tạo một `GeneratedCard` cho thiết kế đó và `GeneratedCardIds` chỉ thêm ID một lần. `quantity` là thông tin để in/đóng gói hai bản, không phải lý do tạo hai record Card giống hệt nhau.

## 5. Nơi tạo History

- HandMade thành công: tạo `GeneratedCard` + `ClientHistory(type = handmade_card)` ngay trong CreateOrder.
- AI mới thành công sau payment: Card service tạo `GeneratedCard` + `ClientHistory(type = card)`.
- AI từ History thành công sau payment: cũng tạo Card/History mới; History/metadata biết parent qua `regeneratedFromCardId`.

`GeneratedCardIds` nên là `[]` khi chưa có Card thay vì `null`, vì `[]` phân biệt rõ “chưa tạo/không có Card” với lỗi dữ liệu null.

# 06 — History Card, chọn tại Checkout và contract Admin

## 1. Lịch sử Card của customer

Card History là dữ liệu đọc từ `ClientHistory` join `GeneratedCards`:

```text
ClientHistory.OutputId = GeneratedCard.Id
Type = card hoặc handmade_card
UserId = customer từ JWT
```

Thiệp HandMade có thể xuất hiện trong History. Tuy nhiên, chỉ Card AI được dùng làm nguồn cho luồng tạo lại AI; HandMade không đi vào regenerate AI.

### API danh sách và chi tiết

```http
GET /api/v1/card-histories?pageIndex=1&pageSize=20
GET /api/v1/card-histories/{generatedCardId}
```

Danh sách:

- sort mới nhất trước;
- filter tùy chọn theo `formType`: gõ máy (`go_may`) hoặc viết tay (`calligraphy`);
- không có filter thì lấy tất cả Card hợp lệ;
- không join OrderDetails ngay trong API list để tránh nhân bản một Card khi nó liên quan nhiều Order.

Chi tiết:

- kiểm tra ownership trước;
- không tìm thấy/không thuộc customer trả `404`;
- sau khi tìm thấy mới lấy các Order liên quan;
- Card không có Order vẫn trả thành công với `orders: []`.

Order liên quan nên trả bốn field:

```json
{
  "orderId": "uuid",
  "status": "Paid",
  "createdAt": "2026-09-05T11:00:00Z",
  "totalFee": 520000
}
```

## 2. US-042 — Chọn thiệp từ History cho Checkout

US-042 và US-045 có thể dùng chung một API list History nếu thêm filter `comboId`.

Ý nghĩa:

```text
US-045: customer xem toàn bộ thiệp đã tạo
US-042: customer chọn thiệp trong History phù hợp Combo đang checkout
```

Filter `comboId` phải dùng lineage `RootID`, không dùng cách đoán từ Card hiện tại:

```text
History.RootID = comboId FE truyền
```

Nhờ vậy cả Card được tạo trực tiếp từ Combo và Card được tạo từ Flower AI của cùng Combo đều có thể được tìm đúng theo nguồn gốc.

Response item phục vụ Checkout có thể gồm:

```json
{
  "generatedCardId": "uuid",
  "imageUrl": "https://...",
  "formType": "calligraphy",
  "sizeName": "10 x 15",
  "messageContent": "Chúc mừng sinh nhật",
  "snapshotPrice": 30000,
  "generatedAt": "2026-09-05T11:00:00Z"
}
```

Khi customer chọn một Card AI cũ trong màn hình này, FE gửi `generatedCardId` đó qua `regeneratedFromCardId` trong `Items[].Cards[]` của CreateOrder. BE xác minh lại ownership/type/history, rồi tạo Card mới sau payment; không tái sử dụng record Card cũ.

## 3. Admin — danh sách/chi tiết thiệp

### List Card AI

Response list không trả `sizeKey`. Mỗi item cần có thông tin customer để Admin liên hệ:

```json
{
  "nameCustomer": "Tấn Hùng",
  "phoneCustomer": "0XXX",
  "addressCustomer": "AHC"
}
```

Các thông tin thiệp giữ lại gồm Template, Size (`sizeName`, width, height, base price, max words), nội dung, hình thức chữ, giá và ảnh.

Search list Card AI theo:

```text
tên khách hàng + tên mẫu thiệp
```

Filter theo Size thiệp. Không giữ các hướng search cũ như Card ID/Order ID nếu contract mới chỉ yêu cầu hai điều kiện trên.

### Detail Card AI

- Bỏ `sizeKey`.
- Thêm `nameCustomer`, `phoneCustomer`, `addressCustomer`.
- `orders` của Card trả `orderId`, `status`, `createdAt`, `totalFee`.
- Tên Template/thiết kế phải lấy từ snapshot khi cần giữ dữ liệu lịch sử, không phụ thuộc hoàn toàn vào Template live đã có thể đổi/xóa.

## 4. Admin — danh sách/chi tiết Flower AI

### List Flower AI

- Bỏ `customerId` khỏi response; thay bằng `nameCustomer`, `phoneCustomer`, `addressCustomer`.
- Filter chỉ theo mảng Combo nguồn (`comboIds`).
- Search chỉ theo tên customer.
- `comboId`, `comboName`, `comboImageUrl` lấy từ metadata của `ClientHistory`, theo quy ước đã chốt; không tự join live Product nếu mục tiêu là hiển thị snapshot lịch sử.

### Detail Flower AI

Response nên có:

```text
- customer: tên, điện thoại, địa chỉ; không trả customerId
- combo nguồn: id, name/code, description nếu có, imageUrl
- ảnh Flower AI: imageUrl, createdAt
- mockup: id, name, imageUrl
- materials: một mảng duy nhất
  mỗi object có name, code, quantity, unit, isCore
- orders: orderId, status, createdAt, totalFee
```

Không trả `fileStatus` hay `userInput` trong response detail Admin theo quyết định cuối. Core và Support không tách thành hai mảng; dùng `isCore` để phân biệt.

## 5. Response wrapper dùng chung

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {}
}
```

Danh sách phân trang đặt item tại `value.items` cùng `pageIndex`, `pageSize`, `totalCount`, `hasNextPage`, `hasPreviousPage`.


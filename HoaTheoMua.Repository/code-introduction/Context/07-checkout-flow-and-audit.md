# 07 — Flow Checkout 5 bước và kết quả rà soát

## 1. Hai cách bắt đầu Checkout, không trộn vào nhau

```text
Cách 1 — Product/Combo bình thường
Customer chọn một Product đang bán: vật liệu, bình/lọ hoặc Combo.

Cách 2 — Ảnh Flower AI đã có
Customer đã tạo Flower AI ở flow trước đó
→ chọn chính ảnh đó từ History/Order
→ BE kiểm tra Combo nguồn còn bán được
→ đi vào Checkout bằng Combo gốc.
```

Không có flow này:

```text
Chọn Combo bình thường
→ tạo Flower AI ngay trong Checkout
→ tiếp tục Checkout bằng cả Combo và Flower AI.
```

Tạo Flower AI và Checkout là hai flow độc lập. Khi order từ Flower AI, `GeneratedFlowerId` chỉ giúp Order giữ đúng thiết kế ảnh đã chọn; Product/Combo gốc vẫn là thứ được bán và giao.

## 2. Năm bước Checkout dùng chung

```text
1. Nhập thông tin cá nhân/giao hàng
   - người đặt, người nhận, địa chỉ, ngày giao, khung giờ, ghi chú,
     phương thức giao và thanh toán.

2. Chọn thiệp (không bắt buộc)
   - thiệp thường/HandMade;
   - thiệp AI tạo mới;
   - thiệp từ History.

3. Tính tổng tiền
   - BE báo giá hoa + ship + thuế (nếu có) + các dòng Card.
   - CreateOrder luôn tự tính lại giá Card để chốt tiền thật.

4. Hiển thị lại toàn bộ lựa chọn
   - nguồn hoa, thông tin giao, các Card, số lượng và tổng tiền.

5. Thanh toán và hoàn tất
   - HandMade đã tạo trong CreateOrder.
   - Card AI mới/Card từ History chỉ sinh sau khi payment thành công.
```

Hai loại nguồn Checkout đều đi qua đủ năm bước trên. Khác biệt chỉ ở cách chọn bó hoa tại bước bắt đầu.

## 3. Ba loại Card trong bước 2

| Lựa chọn | FE gửi | BE làm |
|---|---|---|
| HandMade/thiệp thường | `cardMode = handmade` và thông tin khách chọn | Tính giá, tạo Card/History ngay trong transaction CreateOrder. |
| Thiệp AI mới | `cardMode = ai`, `regeneratedFromCardId = null` | Tính giá, tạo snapshot; sau Paid mới tạo Card/History. |
| Thiệp từ History | `cardMode = history`, `regeneratedFromCardId = GeneratedCard.Id` | Validate Card AI cũ, tính giá lại từ lựa chọn hiện tại, snapshot; sau Paid mới tạo Card/History mới. |

Card từ History là **tạo Card mới**, không sao chép hoặc sửa record Card cũ. Giá cũng là giá mới tại lúc đặt đơn.

## 4. Kết quả rà soát đã sửa

| Hạng mục | Kết quả |
|---|---|
| Hai nguồn checkout | Đã bổ sung rule loại trừ nhau, nhưng vẫn giải thích vì request Flower AI có cả `productId` và `generatedFlowerId`. |
| `GeneratedFlowerId` | Đã nêu đủ validate ownership, History RootID khớp `productId`, Combo và Core. |
| Snapshot nhiều Card | Đã làm rõ: một OrderDetails xử lý cả mảng snapshot, rồi BE append tất cả ID tạo được. |
| Gõ máy | Đã sửa code thiết kế `CalculateCardPrice`: không kiểm tra/định giá theo số từ ở nhánh `go_may`; `cardConfig = null`. |
| HandMade | Đã giữ tạo trước payment và tự ghi Card ID/History trong CreateOrder. |
| Update GeneratedCardIds | Đã chốt là BE/webhook/service nội bộ làm, không phải FE gọi UpdateOrder. |
| Số bản in | Đã chốt `quantity` nằm trong từng object SnapshotCard. Một thiết kế tạo một GeneratedCard; quantity dùng cho giá và in/đóng gói. |

## 5. Rule đã xác nhận sau rà soát

```text
HandMade không dùng Size.

Cả ba lựa chọn Card ở Checkout (HandMade, AI mới, History)
đều không có senderName và receiverName.

Từng object SnapshotCard có quantity.
Một snapshot đại diện một thiết kế; AI tạo một GeneratedCard cho thiết kế đó,
còn quantity quyết định tổng tiền và số bản in.
```

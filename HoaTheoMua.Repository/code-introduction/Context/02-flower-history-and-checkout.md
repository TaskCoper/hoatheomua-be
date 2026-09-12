# 02 — Flower AI, History và Checkout từ ảnh hoa

## 1. Tạo và tạo lại Flower AI

```text
Customer chọn một Combo
→ nhập yêu cầu/bối cảnh và Mockup
→ BE kiểm tra Combo, Mockup, quyền và quota
→ gọi AI, lưu ảnh thành công
→ tạo GeneratedFlower
→ tạo ClientHistory Type = flower
```

Không tạo draft/record Flower dở dang. AI thất bại thì không có `GeneratedFlower`/History hoàn tất.

Generate lần đầu lấy `RootID = BaseId = ComboId`. Regenerate giữ nguyên `RootID` và dùng `BaseId = GeneratedFlowerId` vừa được chọn làm nguồn trực tiếp.

## 2. Ba API xem History Flower của customer

| API | Mục đích |
|---|---|
| `GET /api/v1/flower-histories` | Lấy tất cả ảnh Flower của customer, mới nhất trước. |
| `GET /api/v1/flower-histories/{generatedFlowerId}/root-group` | Lấy tất cả ảnh chung Combo nguồn. Mỗi ảnh mang `orders` đã dùng chính ảnh đó. |
| `GET /api/v1/flower-histories/{generatedFlowerId}/session` | Lấy các version trong đúng một chuỗi generate/regenerate. |

Phân biệt hai khái niệm:

```text
Root group: mọi kết quả chung Combo CA
  CA → GF1 → GF2 → GF3
  CA → GF4

Session: chỉ một chuỗi liên tục
  GF1 → GF2 → GF3
```

Khi trả `orders` cho mỗi Flower trong root group, truy vấn bằng:

```text
OrderDetails.GeneratedFlowerId = GeneratedFlower.Id
```

Không trả một danh sách Order chung ở cấp Root vì FE cần biết chính ảnh nào đã được đặt trong Order nào.

## 3. Checkout từ một GeneratedFlower

API:

```http
GET /api/v1/generated-flowers/{generatedFlowerId}/checkout-availability
```

Nó **không** tạo Checkout, Order hay dữ liệu tạm. Nó chỉ trả lời: ảnh hoa này có còn được dùng để mở Checkout cũ không?

Flow:

```text
Customer bấm Đặt hàng ngay trên ảnh hoa AI
→ FE gửi generatedFlowerId
→ BE kiểm tra Flower thuộc customer hiện tại và có Flower History
→ từ History lấy RootID = Combo nguồn
→ gọi GetLimitCoreProduct(rootComboId)
→ Core còn đủ hàng: trả rootComboId + tình trạng vật liệu
→ FE mở flow Checkout cũ như khách vừa chọn Combo đó
```

Nếu API này trả thành công, FE vẫn phải gửi lại `productId = rootComboId` cùng `generatedFlowerId` khi gọi `CreateOrder`. API availability không thay thế validation lúc đặt đơn.

## 4. GetLimitCoreProduct — rule tồn kho dùng chung

Hàm nhận `comboId` và được dùng cho cả chọn Combo bình thường lẫn checkout từ ảnh hoa AI.

```text
1. Tìm Product theo Id = comboId.
   - chưa xóa
   - active
   - ProductType = Combo
   Sai một điều kiện thì chặn.

2. Lấy toàn bộ ComboSpecifications chưa xóa,
   bao gồm Core/Support và trạng thái Material.

3. Với Core:
   - phải có ít nhất một Core;
   - specification active;
   - material active, chưa xóa;
   - quantity > 0;
   - AvailableForSale đủ dùng.
   Sai một điều kiện thì không bán được Combo.

4. Với Support:
   - không chặn checkout;
   - trả NeedsReplacement/trạng thái để FE hoặc bước xử lý sau biết cần thay thế.

5. Trả SellableQuantityCombo và snapshot vật liệu cần thiết.
```

```text
SellableQuantityCombo
= min(floor(AvailableForSale / Quantity)) của toàn bộ Core.
```

Kiểm tra availability chỉ là kiểm tra trước. Khi tạo Order/giữ tồn thực tế vẫn phải kiểm tra và reserve theo transaction riêng; không dựa vào kết quả cũ vì tồn kho có thể đổi trong lúc customer thao tác.

## 5. Error cho checkout availability

| HTTP | Khi nào |
|---:|---|
| `401` | Customer chưa đăng nhập/token không hợp lệ. |
| `404` | Không có ảnh Flower thuộc customer hoặc không có Flower History hợp lệ. |
| `400` | Không resolve được `RootID`, Combo không còn bán được hoặc Core không đủ điều kiện. |

Không trả `rootComboId` khi lỗi. Order cũ và Flower History cũ không bị đổi bởi API kiểm tra này.


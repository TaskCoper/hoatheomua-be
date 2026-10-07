# Kiểm tra giới hạn slot trong CreateOrder

## API bị ảnh hưởng

```http
POST /api/v1/orders
```

Request bổ sung:

```json
{
  "isUrgent": false,
  "orderTime": 1791171653,
  "tagIds": [238]
}
```

## Flow FE

- Giao gấp: gửi `isUrgent = true`, không gửi tag khung giờ.
- Giao theo giờ: gửi `isUrgent = false` và đúng một tag khung giờ đã chọn.
- Nếu BE trả `DELIVERY_SLOT_FULL`, thông báo slot đã đầy và gọi lại API availability để tải lại danh sách.

Response khi đầy:

```json
{
  "code": "DELIVERY_SLOT_FULL",
  "tagId": 238,
  "limit": 20,
  "occupied": 20
}
```

## Validate BE

- `IsUrgent = true`: đặt `OrderTime = 0`, bỏ qua kiểm tra capacity. `Fulfillment` tự thêm tag giao gấp khi gửi Nhanh.vn.
- `IsUrgent = false`: `TagIds` phải chứa đúng một tag tồn tại trong `nhanhvn.order_tags.value` và tag đó phải có limit.
- Tag phụ như thuế không được dùng để xác định slot.

## Flow atomic cho đơn không gấp

1. Hoàn tất các validation và tính tiền hiện có.
2. Lấy ngày tạo đơn hiện tại theo múi giờ Việt Nam và tag khung giờ đã chọn.
3. Mở database transaction và khóa theo cặp `ngày + tagId`. Với PostgreSQL có thể dùng transaction advisory lock.
4. Trong transaction, gọi lại `CalculateSlotAvailability(date, tagId)`.
5. Nếu `occupied >= limit`, rollback và trả `DELIVERY_SLOT_FULL`.
6. Nếu còn chỗ, lưu `Order` với `Status = "Pending"`, `IsUrgent = false` và commit transaction.
7. Sau khi đã lưu Pending, tiếp tục flow tạo thanh toán hiện có. Nếu khởi tạo thanh toán thất bại thì chuyển đơn sang `Cancelled` để nhả chỗ.

Việc khóa chỉ bao quanh bước kiểm tra và lưu Pending; không giữ database lock trong lúc gọi API bên ngoài.

## Phần giữ nguyên

- Chuẩn bị sản phẩm và snapshot.
- Tính giá sản phẩm, phí giao hàng và VAT.
- Tạo URL/QR thanh toán.
- Các flow fulfillment sau khi thanh toán.

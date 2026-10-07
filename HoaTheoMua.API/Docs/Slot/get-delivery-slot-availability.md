# Lấy số lượng đơn theo khung giờ

## API

```http
GET /api/v1/delivery-slots/availability?date=2026-10-05
GET /api/v1/delivery-slots/availability?date=2026-10-05&tagId=238
```

- `date` bắt buộc, định dạng `yyyy-MM-dd`.
- `tagId` không bắt buộc. Không truyền thì trả tất cả khung giờ; có truyền thì trả đúng khung giờ đó.

## Flow FE

- Gọi API khi mở danh sách chọn giờ.
- Dùng `label` để hiển thị và `tagId` làm giá trị lựa chọn.
- FE tự tính `available = max(0, limit - occupied)`.
- Disable khung giờ khi `limit` chưa được cấu hình hoặc `occupied >= limit`.

Response:

```json
{
  "date": "2026-10-05",
  "slots": [
    {
      "tagId": 238,
      "label": "17h - 19h",
      "limit": 20,
      "occupied": 12
    },
    {
      "tagId": 273,
      "label": "18h - 20h",
      "limit": 15,
      "occupied": 15
    }
  ]
}
```

## Flow BE

1. Đọc config `nhanhvn.order_tags`:

- `value`: các `tagId` khung giờ được sử dụng.
- `options`: lấy `label` theo `tagId`.
- `slotLimits`: lấy `limit` theo `tagId`.

2. Chuyển `date` thành khoảng `[đầu ngày, đầu ngày kế tiếp)` theo múi giờ Việt Nam, sau đó đổi sang UTC để query `DateTimeOffset`.

3. Với từng `tagId`, đếm `OrderNhanhVn`:

```text
IsDeleted = false
ThoiGianDonHang nằm trong ngày cần kiểm tra
LabelIds chứa tagId
TrangThaiDonHang khác "63" và "Khách hủy"
```

`ThoiGianDonHang` được map từ webhook:

```text
payload.info.createdAt
→ DateTimeOffset.FromUnixTimeSeconds(info.CreatedAt)
→ OrderNhanhVn.ThoiGianDonHang
```

4. Đếm `Order` đang giữ chỗ:

```text
Status = "Pending"
IsUrgent = false
CreatedAt nằm trong ngày cần kiểm tra
TagIds chứa tagId
```

5. Tính:

```text
occupied = orderNhanhVnCount + pendingOrderCount
```

6. Trả `tagId`, `label`, `limit`, `occupied`. Nếu có `tagId` thì chỉ xử lý và trả slot tương ứng.

## Hàm dùng chung

Tách phần đọc config và đếm đơn thành một hàm service dùng chung:

```csharp
CalculateSlotAvailability(DateOnly date, long? tagId)
```

- API GET gọi hàm với `tagId = null` để lấy tất cả slot hoặc truyền `tagId` để lấy một slot.
- `CreateOrder` gọi lại cùng hàm với tag khung giờ đã chọn ngay trước khi lưu đơn `Pending`.
- Không gọi HTTP từ `CreateOrder`; hai flow dùng chung service để kết quả và điều kiện đếm luôn đồng nhất.

## Validate trong CreateOrder

- Thêm bool IsUrgent vào trong request của createOrder
- `IsUrgent = true`: không yêu cầu tag khung giờ, đặt `OrderTime = 0` và bỏ qua kiểm tra slot capacity. `Fulfillment` sẽ tự thêm tag giao gấp khi gửi đơn sang Nhanh.vn.
- `IsUrgent = false`: `request.TagIds` phải chứa đúng một `tagId` tồn tại trong `nhanhvn.order_tags.value`.
- Các tag phụ như thuế không được dùng để xác định khung giờ.
- Với đơn không gấp, dùng tag khung giờ để lấy `limit`, tính `occupied` và kiểm tra còn chỗ trước khi lưu đơn `Pending`.
- Nếu đơn không gấp nhưng thiếu hoặc có nhiều hơn một tag khung giờ thì từ chối tạo đơn.

## Lỗi chính

- `400`: Sai định dạng ngày hoặc `tagId` không thuộc danh sách khung giờ.
- `404`: Không tìm thấy config `nhanhvn.order_tags`.

Flow của hàm là:

1. Lấy các tagId khung giờ và limit tương ứng từ config.
2. Với từng tagId:
    - Đếm OrderNhanhVn trong ngày có LabelIds chứa tagId, loại "63" và "Khách hủy" (field Trạng thái đơn hàng)
    - Đếm Order trong ngày đang Pending có TagIds chứa tagId.

3. Cộng lại:

occupied = orderNhanhVnCount + pendingOrderCount

4. Trả limit và occupied; FE tính:

available = max(0, limit - occupied)

 ý cần phải hoàn thành:
1. Validate request CreateOrder
[//]: # (2. Trong order có check lại tag đó một lần nữa)

[//]: # (3. )

# Job hủy đơn Pending hết hạn

## Mục tiêu

Định kỳ chuyển đơn thanh toán quá hạn:

```text
Pending → Cancelled
```

Không tạo bảng giữ slot và không thêm field hết hạn. Khi đơn không còn `Pending`, API tính `occupied` sẽ tự ngừng tính đơn đó nên slot được giải phóng.

## Cấu hình

Đọc thời gian chờ thanh toán từ config, đơn vị phút:

```text
pendingTimeoutMinutes
```

Thời điểm hết hạn:

```text
expiresAt = Order.CreatedAt + pendingTimeoutMinutes
```

Toàn bộ phép so sánh thời gian dùng UTC.

## Điều kiện xử lý

Job chỉ chọn `Order` thỏa tất cả điều kiện:

```text
IsDeleted = false
Status = "Pending"
CreatedAt <= UtcNow - pendingTimeoutMinutes
```

## Flow job

1. Job chạy định kỳ, đề xuất mỗi `1 phút`.
2. Đọc `pendingTimeoutMinutes`; nếu thiếu hoặc không hợp lệ thì ghi log và không cập nhật dữ liệu.
3. Cập nhật theo batch các đơn hết hạn:

```text
Status = "Cancelled"
UpdatedAt = UtcNow
```

4. Khi update phải kèm điều kiện `Status = "Pending"` để tránh job ghi đè đơn vừa được payment webhook chuyển trạng thái.
5. Ghi log số đơn đã chuyển sang `Cancelled`.

## Lưu ý triển khai

- Dùng `ExecuteUpdateAsync` hoặc batch update; không cần load toàn bộ entity.
- Job chạy lại nhiều lần vẫn an toàn vì chỉ xử lý đơn còn `Pending`.
- Không gọi API Nhanh.vn và không cần thao tác nhả slot riêng.
- Không hủy đơn đã thanh toán hoặc đã `Cancelled`.

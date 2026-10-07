# Đồng bộ nhãn đơn hàng Nhanh.vn

## 1. Bổ sung field

Thêm field vào `OrderNhanhVn`:

```csharp
public List<long>? LabelIds { get; set; }
```

và tạo migration tương ứng.

## 2. Webhook orderAdd

Trong `WebhookNhanhVnOrderAdd`, lấy `labelIds` từ payload và lưu vào `OrderNhanhVn.LabelIds` khi tạo đơn.

## 3. Webhook orderUpdate

Trong `WebhookNhanhVnOrderUpdate`, nếu payload có `labelIds` thì cập nhật lại `OrderNhanhVn.LabelIds`.

Mục đích của field này là xác định nhãn khung giờ giao hàng khi tính giới hạn slot.

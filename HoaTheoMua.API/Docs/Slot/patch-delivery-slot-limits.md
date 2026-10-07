# Cập nhật giới hạn khung giờ giao hàng

## API

```http
PATCH /api/v1/delivery-slots/limits
```

Chỉ tài khoản admin được sử dụng.

## Flow FE

- Hiển thị các khung giờ đang được chọn trong `nhanhvn.order_tags.value`.
- Lấy tên khung giờ từ `options` theo `tagId`.
- Mỗi khung giờ có một ô nhập `limit` là số đơn tối đa.
- Khi lưu, gửi các khung giờ được thay đổi xuống API.

Request:

```json
{
  "slots": [
    {
      "tagId": 229,
      "limit": 20
    },
    {
      "tagId": 234,
      "limit": 15
    }
  ]
}
```

Điều kiện:

- `slots` không được rỗng.
- `tagId` không được trùng trong request.
- `limit` là số nguyên và phải lớn hơn hoặc bằng `0`.

Response:

```json
{
  "slots": [
    {
      "tagId": 229,
      "label": "8h -10h",
      "limit": 20
    },
    {
      "tagId": 234,
      "label": "10h - 12h",
      "limit": 15
    }
  ],
  "updatedAt": "2026-10-05T10:00:00Z"
}
```

## Flow BE

1. Query một bản ghi `Config` có:

```text
Key = "nhanhvn.order_tags"
IsDeleted = false
```

2. Deserialize `Config.Value` và lấy:

```text
value       // Các tagId được chọn làm khung giờ
options     // Dùng để lấy label theo tagId
slotLimits  // Limit theo tagId
```

3. Kiểm tra mỗi `tagId` trong request:

- Phải tồn tại trong `value`.
- Phải có option tương ứng.
- Nếu không hợp lệ, từ chối toàn bộ request.

4. Cập nhật đúng key tương ứng:

```text
slotLimits[tagId] = limit
```

5. Giữ nguyên `group`, `label`, `value`, `options`, `required`, `inputType` và `validations`.

6. Serialize lại toàn bộ JSON vào `Config.Value`, cập nhật `UpdatedAt` và gọi `SaveChangesAsync()` một lần.

7. Trả về các slot vừa cập nhật kèm label và limit mới.

## Lỗi chính

- `404`: Không tìm thấy config `nhanhvn.order_tags`.
- `400`: Request rỗng, trùng `tagId`, limit âm hoặc tag không thuộc danh sách khung giờ.
- `403`: Người dùng không có quyền admin.

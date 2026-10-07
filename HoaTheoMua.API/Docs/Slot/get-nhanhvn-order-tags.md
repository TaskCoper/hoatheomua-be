# Đọc cấu hình nhãn và giới hạn khung giờ

## API hiện có

```http
GET /api/v1/configs/order-tags
```

Không tạo API mới. Chỉ mở rộng parser của `GetNhanhVnOrderTag()` để đọc thêm `slotLimits` từ config có:

```text
Key = "nhanhvn.order_tags"
IsDeleted = false
```

## DTO cần bổ sung

Thêm vào `ConfigItem`:

```csharp
[JsonPropertyName("slotLimits")]
public Dictionary<int, int?> SlotLimits { get; set; } = new();
```

- Key: `tagId` của khung giờ.
- Value: số đơn tối đa; `null` là admin chưa cấu hình.

## Flow BE

1. Query chính xác config có key `nhanhvn.order_tags`.
2. Deserialize `Value` thành `ConfigItem`, gồm `value`, `options` và `slotLimits`.
3. Chỉ giữ các `options` có `tagId` nằm trong `value`.
4. Chỉ trả `slotLimits` của các `tagId` nằm trong `value`; tag chưa có limit được trả `null`.
5. Giữ nguyên các field và hành vi cũ của API.

## Response data

```json
{
  "group": "nhanhvn",
  "label": "Giờ giao hàng",
  "value": [238, 273],
  "options": [
    { "label": "17h - 19h", "value": 238 },
    { "label": "18h - 20h", "value": 273 }
  ],
  "slotLimits": {
    "238": 20,
    "273": null
  },
  "required": true,
  "inputType": "multiSelect",
  "validations": []
}
```

FE admin dùng dữ liệu này để hiển thị cấu hình limit. Trang đặt hàng tiếp tục dùng API `delivery-slots/availability`.

## Lỗi chính

- Không tìm thấy config: trả lỗi hiện có của `GetNhanhVnOrderTag()`.
- JSON không hợp lệ hoặc thiếu `value`/`options`: trả lỗi `Cannot convert value`.
- Thiếu `slotLimits`: coi như dictionary rỗng và trả `null` cho các slot, bảo đảm tương thích config cũ.

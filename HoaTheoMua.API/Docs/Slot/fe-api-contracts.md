# API contract cho FE - Delivery Slot

## 1. Lấy cấu hình khung giờ (API sửa)

```http
GET /api/v1/configs/order-tags
```

Request body: không có.

Response:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "trace-id",
  "timestampUtc": "2026-10-07T03:00:00Z",
  "value": {
    "group": "nhanhvn",
    "label": "Giờ giao hàng",
    "value": [229, 234, 236, 237, 238, 246, 270, 273],
    "slotLimits": {
      "229": 20,
      "234": 15,
      "236": 20,
      "237": 20,
      "238": 15,
      "246": 10,
      "270": 10,
      "273": null
    },
    "options": [
      { "label": "8h - 10h", "value": 229 },
      { "label": "10h - 12h", "value": 234 },
      { "label": "18h - 20h", "value": 273 }
    ],
    "required": true,
    "inputType": "multiSelect",
    "validations": []
  }
}
```

`slotLimits[tagId] = null` nghĩa là admin chưa cấu hình limit.

## 2. Cập nhật giới hạn khung giờ (API mới - Admin)

```http
PATCH /api/v1/delivery-slots/limits
```

Request:

```json
{
  "slots": [
    { "tagId": 229, "limit": 20 },
    { "tagId": 234, "limit": 15 }
  ]
}
```

Response:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "trace-id",
  "timestampUtc": "2026-10-07T03:00:00Z",
  "value": {
    "slots": [
      { "tagId": 229, "label": "8h - 10h", "limit": 20 },
      { "tagId": 234, "label": "10h - 12h", "limit": 15 }
    ],
    "updatedAt": "2026-10-07T03:00:00Z"
  }
}
```

Rule: `limit` là số nguyên `>= 0`; `tagId` phải thuộc danh sách `value`.

## 3. Lấy số lượng slot đã sử dụng (API mới)

Lấy tất cả:

```http
GET /api/v1/delivery-slots/availability?date=2026-10-07
```

Lọc một khung giờ:

```http
GET /api/v1/delivery-slots/availability?date=2026-10-07&tagId=238
```

Request body: không có. `date` bắt buộc, định dạng `yyyy-MM-dd`; `tagId` không bắt buộc.

Response:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "trace-id",
  "timestampUtc": "2026-10-07T03:00:00Z",
  "value": {
    "date": "2026-10-07",
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
        "limit": null,
        "occupied": 3
      }
    ]
  }
}
```

FE tính:

```text
available = max(0, limit - occupied)
```

- Disable slot khi `limit = null` hoặc `occupied >= limit`.
- Khi tạo đơn, FE phải gửi ngày và đúng một `tagId` thuộc danh sách `value`.

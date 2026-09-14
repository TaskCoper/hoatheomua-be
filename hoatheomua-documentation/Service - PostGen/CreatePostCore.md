# CreatePostCore

Kiểm chứng ngày 2026-09-12 bằng HTTP cục bộ, JWT kiểm thử và PostgreSQL 16 riêng.

#### Request

`POST /api/v1/posts`

Policy: `StaffOrAdminPolicy`.

```json
{
  "title": "Hoa mùa thu",
  "content": "Nội dung gốc",
  "hashtag": "#hoa",
  "images": [
    {
      "imageUrl": "https://example.com/2.jpg",
      "sortOrder": 2
    },
    {
      "imageUrl": "https://example.com/1.jpg",
      "sortOrder": 1
    },
    {
      "imageUrl": "https://example.com/3.jpg",
      "sortOrder": 3
    }
  ]
}
```

#### Response

HTTP 200; payload và traceId ghi nhận từ lần chạy thực tế.

```json
{
  "value": {
    "baseImages": [
      {
        "id": "2ae66a44-fcd1-4b3e-a99e-aeb3301d2f63",
        "imageUrl": "https://example.com/1.jpg",
        "sortOrder": 1,
        "width": null,
        "height": null
      },
      {
        "id": "53a3d3f2-c1e3-4880-a867-2c5406735805",
        "imageUrl": "https://example.com/2.jpg",
        "sortOrder": 2,
        "width": null,
        "height": null
      },
      {
        "id": "7bb95a8d-bff0-411a-88d0-225f9eb34d96",
        "imageUrl": "https://example.com/3.jpg",
        "sortOrder": 3,
        "width": null,
        "height": null
      }
    ],
    "images": [],
    "platforms": [],
    "basePostId": "8a99eb32-6df5-458e-aa2a-27e1bb45087a",
    "title": "Hoa mùa thu",
    "content": "Nội dung gốc",
    "hashtag": "#hoa",
    "createdAt": "2026-09-12T07:10:47.090101+00:00",
    "updatedAt": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNOGJJRTDS8B:00000004",
  "timestampUtc": "2026-09-12T07:10:47.944942Z"
}
```

#### Flow diagram

```mermaid
flowchart TD
    S0["Xác thực Staff/Admin"]
    S1["validate đầu vào"]
    S0 --> S1
    S2["tạo bài dùng chung"]
    S1 --> S2
    S3["lưu bài và ảnh trong một SaveChanges"]
    S2 --> S3
    S4["trả detail"]
    S3 --> S4
```

#### Test case

- Tạo bài có 3 ảnh: 200, ảnh trả đúng thứ tự.
- Không có ảnh: 422; URL không phải HTTP(S), trùng thứ tự, title/content/hashtag quá dài: validator từ chối.
- BasePost không lưu UserId; không ghi PostHistory/ClientHistory.

Test tự động: `HoaTheoMua.Service.Test/PostGenTest/CreatePostCore.cs`; kiểm DB thật tại `PostGenPostgres.cs`.

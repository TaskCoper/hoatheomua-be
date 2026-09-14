# UpdatePostCore

Kiểm chứng ngày 2026-09-12 bằng HTTP cục bộ, JWT kiểm thử và PostgreSQL 16 riêng.

#### Request

`PUT /api/v1/posts/8a99eb32-6df5-458e-aa2a-27e1bb45087a`

Policy: `StaffOrAdminPolicy`.

```json
{
  "title": "Title đã sửa",
  "content": "Nội dung gốc",
  "hashtag": "#hoa",
  "images": [
    {
      "id": "7bb95a8d-bff0-411a-88d0-225f9eb34d96",
      "imageUrl": "https://example.com/3.jpg",
      "sortOrder": 1
    },
    {
      "id": "53a3d3f2-c1e3-4880-a867-2c5406735805",
      "imageUrl": "https://example.com/2.jpg",
      "sortOrder": 2
    },
    {
      "id": "2ae66a44-fcd1-4b3e-a99e-aeb3301d2f63",
      "imageUrl": "https://example.com/1.jpg",
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
        "id": "160b023f-da2c-44a7-b032-fa2d2896d53e",
        "imageUrl": "https://example.com/3.jpg",
        "sortOrder": 1,
        "width": null,
        "height": null
      },
      {
        "id": "660b4417-732a-42cb-a5a1-cf7cd51be5ce",
        "imageUrl": "https://example.com/2.jpg",
        "sortOrder": 2,
        "width": null,
        "height": null
      },
      {
        "id": "faa6efc9-5bcc-4f1d-81b5-2e391f2d3f91",
        "imageUrl": "https://example.com/1.jpg",
        "sortOrder": 3,
        "width": null,
        "height": null
      }
    ],
    "platforms": [],
    "basePostId": "e8e5bc6d-d39a-4588-bb19-5fa4ad426665",
    "title": "Title đã sửa",
    "content": "Nội dung gốc",
    "hashtag": "#hoa",
    "createdAt": "2026-09-14T08:10:57.971964+00:00",
    "updatedAt": "2026-09-14T08:10:58.435467+00:00"
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNOI6UQF96NF:0000000B",
  "timestampUtc": "2026-09-14T08:10:58.451686Z"
}
```

#### Flow diagram

```mermaid
flowchart TD
    S0["Validate"]
    S1["khóa bài dùng chung"]
    S0 --> S1
    S2["kiểm ảnh và trạng thái"]
    S1 --> S2
    S3["nhả thứ tự ảnh trong transaction"]
    S2 --> S3
    S4["lưu tập cuối"]
    S3 --> S4
    S5["trả detail"]
    S4 --> S5
```

#### Test case

- Đảo thứ tự 3 ảnh không vi phạm unique index PostgreSQL.
- Ảnh không thuộc bài: 400. Đổi URL của ID ảnh cũ: 400.
- Ảnh bỏ khỏi request được soft delete; thêm ảnh không id sinh ID mới.
- Staff/Admin khác cũng sửa được bài; nền tảng đang Generating: 409.

Test tự động: `HoaTheoMua.Service.Test/PostGenTest/UpdatePostCore.cs`; kiểm DB thật tại `PostGenPostgres.cs`.

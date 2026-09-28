# CreatePostCore

Kiểm chứng ngày 2026-09-12 bằng HTTP cục bộ, JWT kiểm thử và PostgreSQL 16 riêng.

#### Request

`POST /api/v1/posts`

Policy: `StaffOrAdminPolicy`.

Cập nhật 2026-09-14: ảnh nguồn không bắt buộc khi tạo bài. Có thể bỏ `images`, gửi `null` hoặc `[]`; response trả `baseImages: []` và `platforms: []`. Title/content vẫn bắt buộc. Nếu gửi ảnh: tối đa 20, từng ảnh vẫn phải có URL HTTP(S), thứ tự hợp lệ và không trùng.

Ví dụ tạo bài trước, bổ sung ảnh sau bằng `PUT /api/v1/posts/{id}`:

```json
{ "title": "Hoa mùa thu", "content": "Nội dung gốc" }
```

Có thể gọi generate với `mode: "content"` ngay khi chưa có ảnh. Chỉ chọn sinh ảnh sau khi đã bổ sung ảnh nguồn.

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
        "id": "faa6efc9-5bcc-4f1d-81b5-2e391f2d3f91",
        "imageUrl": "https://example.com/1.jpg",
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
        "id": "160b023f-da2c-44a7-b032-fa2d2896d53e",
        "imageUrl": "https://example.com/3.jpg",
        "sortOrder": 3,
        "width": null,
        "height": null
      }
    ],
    "platforms": [],
    "basePostId": "e8e5bc6d-d39a-4588-bb19-5fa4ad426665",
    "title": "Hoa mùa thu",
    "content": "Nội dung gốc",
    "hashtag": "#hoa",
    "createdAt": "2026-09-14T08:10:57.971964+00:00",
    "updatedAt": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNOI6UQF96NF:00000004",
  "timestampUtc": "2026-09-14T08:10:58.230164Z"
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
- Bỏ images / null / []: tạo thành công, đọc lại có baseImages rỗng; có thể sửa nội dung trước và bổ sung ảnh qua PUT sau.
- Hơn 20 ảnh, URL không phải HTTP(S), phần tử null, trùng thứ tự, title/content/hashtag quá dài: validator từ chối.
- BasePost không lưu UserId; không ghi PostHistory/ClientHistory.

Test tự động: `HoaTheoMua.Service.Test/PostGenTest/CreatePostCore.cs`; kiểm DB thật tại `PostGenPostgres.cs`.

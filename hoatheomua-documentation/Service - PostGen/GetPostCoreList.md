# GetPostCoreList

Kiểm chứng ngày 2026-09-12 bằng HTTP cục bộ, JWT kiểm thử và PostgreSQL 16 riêng.

#### Request

`GET /api/v1/posts?pageSize=200&keyword=hoa`

Policy: `StaffOrAdminPolicy`.

#### Response

HTTP 200; payload và traceId ghi nhận từ lần chạy thực tế.

```json
{
  "value": {
    "items": [
      {
        "basePostId": "8a99eb32-6df5-458e-aa2a-27e1bb45087a",
        "title": "Hoa mùa thu",
        "content": "Nội dung gốc",
        "hashtag": "#hoa",
        "createdAt": "2026-09-12T07:10:47.090101+00:00",
        "updatedAt": null
      }
    ],
    "pageIndex": 1,
    "pageSize": 100,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNOGJJRTDS8B:00000005",
  "timestampUtc": "0001-01-01T00:00:00"
}
```

#### Flow diagram

```mermaid
flowchart TD
    S0["Xác thực Staff/Admin"]
    S1["lọc bài chưa xóa"]
    S0 --> S1
    S2["lọc keyword/platform"]
    S1 --> S2
    S3["đếm và phân trang"]
    S2 --> S3
    S4["trả envelope"]
    S3 --> S4
```

#### Test case

- Mọi Staff/Admin thấy cùng danh sách bài; bài soft delete không xuất hiện.
- Keyword không phân biệt hoa thường; có lọc platform.
- PageIndex 1-based; pageSize clamp 100.

Test tự động: `HoaTheoMua.Service.Test/PostGenTest/GetPostCoreList.cs`; kiểm DB thật tại `PostGenPostgres.cs`.

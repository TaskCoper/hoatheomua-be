# API: Lấy danh sách Recruitment Request của người dùng hiện tại

## URL

```http
GET /api/v1/recruitment-requests/me?pageIndex=1&pageSize=10
Authorization: Bearer {accessToken}
```

API này chỉ cần `[Authorize]`, không yêu cầu role Admin/Staff. User nào gọi API thì chỉ nhận được danh sách recruitment request của chính user đó dựa theo `UserId` trong access token.

## Request Body

Không có request body.

## Query Parameters

| Field | Type | Required | Default | Rule |
| --- | --- | --- | --- | --- |
| `pageIndex` | `int` | No | `1` | Nếu `<= 0` thì service tự set về `1` |
| `pageSize` | `int` | No | `10` | Nếu `<= 0` thì service tự set về `10`; tối đa `100` |

Ví dụ:

```http
GET /api/v1/recruitment-requests/me?pageIndex=1&pageSize=10
```

## Response Body

### Success - `200 OK`

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HN...",
  "timestampUtc": "2026-06-20T10:30:00Z",
  "value": {
    "items": [
      {
        "id": "7c3a7c5b-0e3a-4f94-98e6-8f5df1db0f0a",
        "title": "Ứng tuyển vị trí Backend Developer",
        "coverLetter": "Tôi muốn ứng tuyển vị trí này...",
        "attachments": [
          "https://example.com/cv.pdf"
        ],
        "isRead": false,
        "createdAt": "2026-06-20T10:00:00Z",
        "jobDescription": {
          "id": "2a3e4c7d-90c2-4c3d-9db7-017b3eaf5b99",
          "title": "Backend Developer"
        }
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

### Item Fields

| Field | Type | Ý nghĩa |
| --- | --- | --- |
| `id` | `Guid` | Id của recruitment request |
| `title` | `string` | Tiêu đề đơn ứng tuyển |
| `coverLetter` | `string` | Nội dung thư ứng tuyển |
| `attachments` | `string[]` | Danh sách file đính kèm/CV |
| `isRead` | `bool` | Admin đã đọc đơn hay chưa |
| `createdAt` | `DateTimeOffset` | Thời gian tạo đơn |
| `jobDescription.id` | `Guid` | Id job đã ứng tuyển |
| `jobDescription.title` | `string` | Tiêu đề job đã ứng tuyển |

> Gợi ý: API này là API cho chính user đang đăng nhập, nên response không cần trả object `user` như API admin `GET /api/v1/recruitment-requests`.

## Business Rule

1. Endpoint chỉ yêu cầu user đã đăng nhập:

```csharp
[Authorize]
```

Không dùng:

```csharp
[Authorize(Policy = JwtExtensions.AdminPolicy)]
```

2. Service lấy `UserId` từ access token:

```csharp
_httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value
```

3. Nếu không lấy được `UserId` hoặc `UserId` không hợp lệ thì trả lỗi authentication/user not found.

4. Query chỉ lấy recruitment request của chính user hiện tại:

```csharp
_context.RecruitmentRequests
    .Where(x => x.UserId == currentUserId && !x.IsDeleted)
```

5. Không cho truyền `userId` từ query/body để tránh user xem recruitment request của người khác.

6. Sắp xếp danh sách theo thời gian tạo mới nhất trước:

```csharp
.OrderByDescending(x => x.CreatedAt)
```

7. Có phân trang:

```csharp
.Skip((pageIndex - 1) * pageSize)
.Take(pageSize)
```

8. `totalCount` là tổng số recruitment request của user hiện tại sau khi filter `UserId` và `IsDeleted`.

9. `pageSize` tối đa `100` để tránh query quá nhiều dữ liệu một lần.

10. API này chỉ đọc dữ liệu, không thay đổi trạng thái `isRead`.

## Các lỗi có thể xuất hiện

### `401 Unauthorized` - Thiếu hoặc sai access token

Xảy ra khi:

- Không truyền header `Authorization`
- Access token sai format
- Access token hết hạn
- Access token không validate được

Response mẫu:

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication is required.",
  "messageCode": "UNAUTHORIZED",
  "errors": null,
  "traceId": "0HN...",
  "timestampUtc": "2026-06-20T10:30:00Z"
}
```

### `404 Not Found` - Không tìm thấy user từ token

Xảy ra khi token có `UserId` nhưng user không còn tồn tại hoặc đã bị xóa trong hệ thống.

Response mẫu:

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "User not found.",
  "messageCode": "USER_NOT_FOUND",
  "errors": null,
  "traceId": "0HN...",
  "timestampUtc": "2026-06-20T10:30:00Z"
}
```

### `400 Bad Request` - Query phân trang không hợp lệ

Với logic hiện tại của project, `pageIndex <= 0` và `pageSize <= 0` có thể tự normalize về default. Nếu muốn strict validation thì có thể trả lỗi:

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid pagination parameters.",
  "messageCode": "INVALID_PAGINATION",
  "errors": {
    "pageIndex": ["pageIndex must be greater than 0"],
    "pageSize": ["pageSize must be greater than 0"]
  },
  "traceId": "0HN...",
  "timestampUtc": "2026-06-20T10:30:00Z"
}
```

### `500 Internal Server Error` - Lỗi không mong muốn

Xảy ra khi có lỗi database hoặc exception chưa được handle.

Response mẫu:

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred.",
  "messageCode": "INTERNAL_SERVER_ERROR",
  "errors": null,
  "traceId": "0HN...",
  "timestampUtc": "2026-06-20T10:30:00Z"
}
```

## Gợi ý tên method khi implement

Controller:

```csharp
[HttpGet("me")]
[Authorize]
public async Task<IActionResult> GetMyRecruitmentRequests(int pageIndex = 1, int pageSize = 10)
```

Service interface:

```csharp
Task<BasePaginationResponse> GetMyRecruitmentRequestsAsync(int pageIndex, int pageSize);
```

Service implementation:

```csharp
public async Task<BasePaginationResponse> GetMyRecruitmentRequestsAsync(int pageIndex, int pageSize)
```

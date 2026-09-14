# Bàn giao API cho Frontend — PostCore

Ngày đối chiếu: **14/09/2026**. Nội dung dựa trên controller, DTO, service và validator hiện tại trong repository `hoatheomua-be`.

## 1. Phạm vi và trạng thái

| Method | Endpoint | Chức năng / thay đổi | Quyền | Trạng thái code |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/posts` | Tạo bài viết gốc và ảnh nguồn | Staff hoặc Admin | Đã triển khai, commit/push trên feature/post-gen |
| GET | `/api/v1/posts` | Danh sách bài viết, phân trang và lọc | Staff hoặc Admin | Đã triển khai, commit/push trên feature/post-gen |
| GET | `/api/v1/posts/{id}` | Chi tiết bài viết, ảnh và dữ liệu nền tảng | Staff hoặc Admin | Đã triển khai, commit/push trên feature/post-gen |
| PUT | `/api/v1/posts/{id}` | Cập nhật bài viết và thay toàn bộ danh sách ảnh nguồn | Staff hoặc Admin | Đã triển khai, commit/push trên feature/post-gen |
| DELETE | `/api/v1/posts/{id}` | Xóa mềm bài viết | Staff hoặc Admin | Đã triển khai, commit/push trên feature/post-gen |

**Tổng: 5 endpoint PostCore đã triển khai, commit/push trên feature/post-gen.** Trạng thái triển khai lên server chưa được xác minh trong lần tổng hợp này; FE cần dùng môi trường đã có code và migration tương ứng.

Mốc 3 đã thêm API generate nội dung trong workspace; xem [bàn giao PostGen mốc 3](FE-API-Handoff-PostGen-Milestone3.md). API regenerate theo feedback, chỉnh/chọn version, sinh ảnh, official, publish và schedule vẫn chưa triển khai. Tài liệu này tập trung vào 5 API PostCore. Cấu hình PostGen dùng API Config chung hiện có, không thuộc 5 endpoint trong tài liệu này.

## 2. Quy ước chung

- Ghép đường dẫn với API origin của môi trường FE đang dùng; tài liệu không cố định domain.
- Gửi `Authorization: Bearer <access_token>` cho tất cả endpoint trong bảng.
- POST và PUT nhận JSON với `Content-Type: application/json`; không nhận file upload trực tiếp. FE truyền URL ảnh đã upload.
- Tên thuộc tính JSON dùng `camelCase`. ID là chuỗi GUID; thời gian là chuỗi ISO 8601.
- Cả 5 API PostCore trả **HTTP 200** khi thành công, kể cả tạo và xóa.
- `{id}` trong route PostCore chính là `basePostId` từ response, phải là GUID hợp lệ.

### Response thành công

```ts
type Guid = string;
type DateTimeString = string;

interface ApiResponse<T> {
  value: T;
  isSuccess: boolean;
  isFailed: boolean;
  error: unknown | null;
  traceId: string | null;
  timestampUtc: DateTimeString;
}

interface PaginationValue<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

`isSuccess = true`, `isFailed = false`, `error = null` khi thành công. Trong code hiện tại, factory phân trang chưa gán `timestampUtc`, nên response danh sách có thể trả thời gian mặc định `0001-01-01T00:00:00`. Dùng `createdAt` / `updatedAt` của từng item để hiển thị thời gian. GET danh sách PostCore có gán `traceId`.

## 3. POST `/api/v1/posts` — Tạo bài viết

### Request

```json
{
  "title": "Bó hoa chúc mừng sinh nhật",
  "content": "Gửi lời chúc yêu thương cùng một bó hoa tươi.",
  "hashtag": "#hoatuoi #sinhnhat",
  "images": [
    {
      "imageUrl": "https://cdn.example.com/flowers/birthday.jpg",
      "sortOrder": 1,
      "width": 1080,
      "height": 1080
    }
  ]
}
```

| Field | Kiểu | Quy tắc |
| --- | --- | --- |
| `title` | string | Bắt buộc, không rỗng/toàn khoảng trắng, tối đa 500 ký tự |
| `content` | string | Bắt buộc, không rỗng/toàn khoảng trắng, tối đa 20.000 ký tự |
| `hashtag` | string hoặc null | Không bắt buộc, tối đa 1.000 ký tự; là một chuỗi, không phải mảng |
| `images` | array | Bắt buộc, từ 1 đến 20 phần tử; không được chứa phần tử null |
| `images[].id` | GUID hoặc null | Khi tạo mới phải bỏ field này hoặc gửi null |
| `images[].imageUrl` | string | Bắt buộc, URL tuyệt đối HTTP/HTTPS, tối đa 2.048 ký tự |
| `images[].sortOrder` | integer | Từ 1 trở lên, không trùng trong cùng request; không bắt buộc liên tiếp |
| `images[].width` | integer hoặc null | Không bắt buộc; nếu có phải lớn hơn 0 |
| `images[].height` | integer hoặc null | Không bắt buộc; nếu có phải lớn hơn 0 |

### Response 200

Kiểu: `ApiResponse<PostCoreDetail>`. Tạo bài viết chỉ lưu nội dung và ảnh nguồn, chưa sinh AI hoặc tạo bài cho nền tảng.

```json
{
  "value": {
    "basePostId": "11111111-1111-4111-8111-111111111111",
    "title": "Bó hoa chúc mừng sinh nhật",
    "content": "Gửi lời chúc yêu thương cùng một bó hoa tươi.",
    "hashtag": "#hoatuoi #sinhnhat",
    "createdAt": "2026-09-14T08:00:00+00:00",
    "updatedAt": null,
    "baseImages": [
      {
        "id": "22222222-2222-4222-8222-222222222222",
        "imageUrl": "https://cdn.example.com/flowers/birthday.jpg",
        "sortOrder": 1,
        "width": 1080,
        "height": 1080
      }
    ],
    "platforms": []
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "example-trace-id",
  "timestampUtc": "2026-09-14T08:00:00Z"
}
```

Các ID, URL và thời gian trong ví dụ là dữ liệu minh họa. Khi sửa/xóa, dùng ID thực từ response.

## 4. GET `/api/v1/posts` — Danh sách bài viết

Ví dụ:

```http
GET /api/v1/posts?pageIndex=1&pageSize=10&keyword=hoa
GET /api/v1/posts?pageIndex=1&pageSize=10&platform=facebook
```

| Query | Kiểu | Mặc định / hành vi |
| --- | --- | --- |
| `pageIndex` | integer | Mặc định 1; nhỏ hơn 1 được đưa về 1 |
| `pageSize` | integer | Mặc định 10; nhỏ hơn hoặc bằng 0 được đưa về 10; tối đa 100 |
| `platform` | string | Không bắt buộc; FE dùng `facebook`, `instagram`, `zalo` |
| `keyword` | string | Không bắt buộc; trim rồi tìm chứa trong `title` hoặc `content`, không phân biệt hoa thường |

Chỉ trả bài chưa xóa. Sắp xếp `createdAt` giảm dần, sau đó ID tăng dần. Bộ lọc `platform` chỉ lấy bài đã có bản ghi nền tảng tương ứng chưa xóa; bài vừa tạo có `platforms: []` sẽ không xuất hiện khi dùng bộ lọc này.

Response 200: `ApiResponse<PaginationValue<PostCoreSummary>>`. Đọc danh sách ở **`response.value.items`**; item danh sách không có `baseImages`, `images`, `platforms`.

```json
{
  "value": {
    "items": [
      {
        "basePostId": "11111111-1111-4111-8111-111111111111",
        "title": "Bó hoa chúc mừng sinh nhật",
        "content": "Gửi lời chúc yêu thương cùng một bó hoa tươi.",
        "hashtag": "#hoatuoi #sinhnhat",
        "createdAt": "2026-09-14T08:00:00+00:00",
        "updatedAt": null
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "example-trace-id",
  "timestampUtc": "0001-01-01T00:00:00"
}
```

Danh sách rỗng hoặc trang vượt dữ liệu trả HTTP 200 với `items: []`.

## 5. GET `/api/v1/posts/{id}` — Chi tiết bài viết

Không có body. Response 200: `ApiResponse<PostCoreDetail>`, cùng shape với response tạo/cập nhật.

```ts
interface PostCoreSummary {
  basePostId: Guid;
  title: string;
  content: string;
  hashtag: string | null;
  createdAt: DateTimeString;
  updatedAt: DateTimeString | null;
}

interface PostCoreDetail extends PostCoreSummary {
  baseImages: BaseImage[];
  platforms: PlatformDetail[];
}

interface BaseImage {
  id: Guid;
  imageUrl: string;
  sortOrder: number;
  width: number | null;
  height: number | null;
}

type RatioCode =
  | "Square1x1" | "Portrait4x5" | "Portrait3x4"
  | "Story9x16" | "Landscape16x9";

type PlatformStatus =
  | "Draft" | "Generating" | "Generated" | "Failed" | "Ready"
  | "Scheduled" | "Publishing" | "Published" | "PublishFailed" | "Partial";

interface PlatformDetail {
  platformPostId: Guid;
  platform: "facebook" | "instagram" | "zalo";
  status: PlatformStatus;
  lastError: string | null;
  content: ContentVersion | null;
  images: OfficialImage[];
}

interface ContentVersion {
  generationId: Guid;
  parentGeneratedPostId: Guid | null;
  versionNo: number;
  source: "Ai" | "Manual";
  title: string | null;
  content: string;
  hashtag: string | null;
  createdAt: DateTimeString;
}

interface OfficialImage {
  sourceBasePostImageId: Guid;
  ratioCode: RatioCode;
  selectedGeneratedPostImageId: Guid;
  imageUrl: string;
  pixelWidth: number;
  pixelHeight: number;
  sortOrder: number;
}
```

Lưu ý cho màn hình chi tiết:

- `baseImages`: ảnh nguồn còn hiệu lực, sắp xếp theo `sortOrder`; dùng để điền form sửa bài.
- Không còn `value.images`; lịch sử ảnh vẫn ở database, API đọc lịch sử riêng dự kiến ở mốc 6.
- `platforms[].content`: một object nội dung chính thức được chọn hoặc null. Không có mảng versions trong response chi tiết; API lịch sử riêng dự kiến ở mốc 5.
- `platforms[].images`: ảnh đã chốt thành công theo `sortOrder`, có `selectedGeneratedPostImageId`, `imageUrl` và kích thước; không trả ô chưa chọn hoặc ảnh lỗi.
- `versionNo` được tính lúc đọc. Dùng `id` / `generationId` để định danh version.
- `Partial` được suy ra khi trạng thái lưu là Generated/Ready và có lỗi ảnh một phần (`lastError`), hoặc có cả ô đã chọn ảnh lẫn ô chưa chọn ảnh.
- Detail chỉ trả nội dung và ảnh chốt; lịch sử nội dung/ảnh không trả kèm detail. Các trạng thái và DTO phục vụ đọc dữ liệu; không có nghĩa các thao tác sinh AI/đăng bài đã được triển khai.

Bài không tồn tại hoặc đã xóa trả HTTP 404, `detail: "Post not found"`.

## 6. PUT `/api/v1/posts/{id}` — Cập nhật bài viết

Body có các field và giới hạn giống POST. **PUT thay toàn bộ nội dung form và tập ảnh nguồn**, vì vậy FE phải gửi đủ `title`, `content`, `images` muốn giữ sau khi lưu. Bỏ `hashtag` hoặc gửi null sẽ xóa hashtag hiện tại.

```json
{
  "title": "Bó hoa sinh nhật — phiên bản cập nhật",
  "content": "Nội dung cập nhật cho bài viết.",
  "hashtag": "#hoatuoi",
  "images": [
    {
      "id": "22222222-2222-4222-8222-222222222222",
      "imageUrl": "https://cdn.example.com/flowers/birthday.jpg",
      "sortOrder": 2,
      "width": 1080,
      "height": 1080
    },
    {
      "imageUrl": "https://cdn.example.com/flowers/new.jpg",
      "sortOrder": 1,
      "width": 1080,
      "height": 1350
    }
  ]
}
```

| Thao tác của FE | Cách gửi |
| --- | --- |
| Giữ ảnh nguồn | Gửi `id` ảnh đang có trong `baseImages`, giữ nguyên `imageUrl` |
| Đổi thứ tự | Giữ `id` và URL, đổi `sortOrder` |
| Thêm ảnh | Bỏ `id` hoặc gửi null, gửi URL ảnh mới |
| Xóa ảnh nguồn | Bỏ ảnh đó khỏi mảng `images` gửi lên |
| Thay nguồn ảnh | Bỏ phần tử có ID cũ, thêm phần tử không ID với URL mới |

ID ảnh phải thuộc bài đang sửa và còn hiệu lực; không được là GUID rỗng hoặc trùng trong request. Không được đổi URL của ảnh có ID cũ. `width`/`height` của ảnh giữ lại cũng được cập nhật theo request; bỏ field sẽ đưa về null. Sau khi sửa vẫn phải còn 1–20 ảnh.

Response 200: `ApiResponse<PostCoreDetail>`. FE nên dùng response để cập nhật lại ID ảnh mới, thứ tự và `updatedAt`.

Nếu bất kỳ bản ghi nền tảng còn hiệu lực của bài đang `Generating`, trả HTTP 409. Thay đổi nội dung gốc không tự chạy lại AI hoặc cập nhật các version đã sinh.

## 7. DELETE `/api/v1/posts/{id}` — Xóa bài viết

Không có body. Xóa mềm bài gốc, ảnh nguồn và các bản ghi nền tảng liên quan đang còn hiệu lực; giữ lịch sử sinh trong dữ liệu.

Response 200: `ApiResponse<{ basePostId: Guid; isDeleted: boolean }>`, với `value`:

```json
{
  "basePostId": "11111111-1111-4111-8111-111111111111",
  "isDeleted": true
}
```

Sau thành công, FE bỏ bài khỏi danh sách hoặc tải lại trang danh sách. Gọi GET/PUT/DELETE trên bài đã xóa trả 404. Nếu có nền tảng đang `Generating`, DELETE trả 409 và bài chưa bị xóa.

## 8. Lỗi và cách xử lý

Lỗi nghiệp vụ/validation do middleware trả có cấu trúc riêng, không nằm trong `value`:

```ts
interface ApiError {
  title: string;
  status: number;
  detail: string;
  messageCode: string;
  errors: unknown | null;
  traceId: string | null;
  timestampUtc: DateTimeString;
}

interface ValidationError {
  field: string;
  message: string;
}
```

Ví dụ lỗi validation:

```json
{
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "",
  "messageCode": "VALIDATION_ERROR",
  "errors": [
    { "field": "images", "message": "Duplicate image order" }
  ],
  "traceId": "example-trace-id",
  "timestampUtc": "2026-09-14T08:00:00Z"
}
```

| HTTP | Trường hợp | Giá trị / xử lý FE |
| --- | --- | --- |
| 400 | Ảnh không thuộc bài hoặc không còn hiệu lực | `messageCode: BAD_REQUEST`, `detail: Image does not belong to this post` |
| 400 | Đổi URL trên ID ảnh cũ | `detail: Existing image URL cannot be changed; add a new image instead` |
| 400 | JSON/query không bind được, sai kiểu dữ liệu | Có thể là ProblemDetails của ASP.NET Core, không dùng envelope lỗi nghiệp vụ trên |
| 401 | Thiếu, sai hoặc hết hạn token | Xử lý theo luồng xác thực hiện có |
| 403 | Không đúng role | Hiển thị không đủ quyền; không giả định luôn có JSON body |
| 404 | Bài không tồn tại hoặc đã xóa | `messageCode: NOT_FOUND`, `detail: Post not found` |
| 409 | Bài có nền tảng đang sinh | `messageCode: CONFLICT`, `detail: Platform post is generating`; tải lại trạng thái trước khi cho sửa/xóa tiếp |
| 422 | Vi phạm validator | `messageCode: VALIDATION_ERROR`; map `errors[].field` vào form, ví dụ `images[0].imageUrl` |
| 500 | Lỗi hệ thống | Hiển thị lỗi chung; lưu `traceId` nếu có để tra cứu |

FE cần kiểm tra HTTP status trước, đọc body theo shape thực tế và có fallback khi body rỗng. GUID sai định dạng trên route `{id:guid}` không khớp route, có thể trả 404 không có error envelope nghiệp vụ.

## 9. Nguồn đối chiếu trong repository

- `HoaTheoMua.API/Controllers/PostCoreController.cs`
- `HoaTheoMua.Service/PostCore/Request.cs`, `Response.cs`, `Service.cs`
- `HoaTheoMua.Service/PostCore/Validate/SavePostCoreValidator.cs`
- `HoaTheoMua.Service/Models/ApiResponse.cs`
- `HoaTheoMua.API/Filters/ValidationActionFilter.cs`
- `HoaTheoMua.API/Middleware/GlobalExceptionHandlerMiddleware.cs`
- `HoaTheoMua.API/Extentions/JwtExtensions.cs`

Tài liệu được đối chiếu tĩnh với source code. Lần tổng hợp này không chạy lại API hoặc xác nhận deploy.

Sinh ảnh trong generate đã triển khai tại [bàn giao FE mốc 4](FE-API-Handoff-PostGen-Milestone4.md). Kích thước pixel của ảnh chốt là kích thước mục tiêu gửi AI, chưa đo từ file ảnh.

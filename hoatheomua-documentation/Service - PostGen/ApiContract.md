# PostGen — Hợp đồng API

Cập nhật 2026-09-14: đã bỏ bảng PlatformStandard và cả API chuẩn nền tảng/preset riêng. Admin dùng [Config chuẩn nền tảng](PlatformConfig.md) và [Config ratio preset](RatioPresetConfig.md).

## Hợp đồng API

Các endpoint PostGen dưới đây dùng `[Authorize(Policy = JwtExtensions.StaffOrAdminPolicy)]` (quyết định #7). **Ba** endpoint gọi AI — `generate`, `content/regenerate`, `images/regenerate` — thêm `[EnableRateLimiting(RateLimitExtensions.HeavyPolicy)]`; đó là ba chỗ duy nhất tiêu tiền.

`{platform}` trong route là tên enum viết thường: `facebook` | `instagram` | `zalo`.

| Method | Route | Service method | Ghi chú |
| --- | --- | --- | --- |
| POST | `/api/v1/posts` | `CreatePostCore` | Title, Content, Hashtag, danh sách ảnh |
| GET | `/api/v1/posts` | `GetPostCoreList` | phân trang + lọc `platform`, `keyword` |
| GET | `/api/v1/posts/{id:guid}` | `GetPostCoreDetail` | kèm bài nền tảng + version |
| PUT | `/api/v1/posts/{id:guid}` | `UpdatePostCore` | |
| DELETE | `/api/v1/posts/{id:guid}` | `DeletePostCore` | soft delete |
| POST | `/api/v1/posts/{id:guid}/generate` | `GeneratePlatformPosts` | **heavy**, chọn nền tảng + tỷ lệ + map ảnh nguồn |
| POST | `/api/v1/posts/{id:guid}/platforms/{platform}/content/regenerate` | `RegenerateContent` | **heavy**, `sourceGenerationId` + `feedback` — chỉ nội dung |
| PUT | `/api/v1/posts/{id:guid}/platforms/{platform}/content` | `EditPlatformContent` | sửa tay nội dung → tạo version `Manual` và chốt luôn |
| GET | `/api/v1/posts/{id:guid}/platforms/{platform}/content/versions` | `GetContentVersions` | danh sách version nội dung |
| PUT | `/api/v1/posts/{id:guid}/platforms/{platform}/content/official` | `SelectOfficialContent` | chọn version nội dung chính thức |
| POST | `/api/v1/posts/{id:guid}/images/regenerate` | `RegenerateImage` | **heavy**, paraphase lại **đúng một cặp** `(sourceBasePostImageId, ratioCode)`. Chỉ thêm ảnh vào kho, không đụng ô của nền tảng nào |
| GET | `/api/v1/posts/{id:guid}/images` | `GetImageVersions` | mọi version ảnh của bài gốc, gom theo cặp (ảnh × tỷ lệ) |
| PUT | `/api/v1/posts/{id:guid}/platforms/{platform}/images` | `UpdatePlatformImages` | ghi đè toàn bộ ô ảnh của nền tảng: ảnh nào, tỷ lệ nào, version nào, thứ tự nào |
| GET | `/api/v1/posts/{id:guid}/platforms/{platform}/official` | `GetOfficialVersion` | payload sẽ đem đi đăng |

Chừa sẵn, **ghi trong tài liệu nhưng không triển khai**: `POST .../platforms/{platform}/publish`, `POST .../platforms/{platform}/schedule`, `DELETE .../platforms/{platform}/schedule`, `GET /api/v1/post-gen/platform-accounts`.

### DTO chính

`POST /api/v1/posts/{id}/generate`

```json
{ "platforms": [
    { "platform": "facebook", "usePreset": false,
      "ratios": [ { "ratioCode": "Square1x1", "basePostImageIds": ["…A", "…B"] } ] },
    { "platform": "instagram", "usePreset": true }
] }
```

`usePreset` và `ratios` loại trừ nhau trên **mỗi nền tảng** — một nền tảng dùng preset, nền tảng khác cấu hình tay trong cùng một request là hợp lệ. Khi `usePreset = true`, server đọc preset dùng chung từ `Config` group `postgen` rồi khai triển theo `imageSelectionMode`.

Server gom các cặp `(sourceBasePostImageId, ratioCode)` của **tất cả** nền tảng trong request lại, loại trùng, rồi mới gọi AI. Trong ví dụ trên nếu Instagram cũng chọn `Square1x1` với cùng ảnh A thì cặp đó chỉ sinh một lần và cả hai nền tảng trỏ vào chung một tấm.

```json
{ "value": {
    "basePostId": "…",
    "images": [
      { "id": "…", "sourceBasePostImageId": "…A", "ratioCode": "Square1x1",
        "versionNo": 1, "pixelWidth": 1080, "pixelHeight": 1080,
        "imageUrl": "https://…", "status": "Succeeded" },
      { "id": "…", "sourceBasePostImageId": "…B", "ratioCode": "Landscape16x9",
        "versionNo": 1, "imageUrl": null,
        "status": "Failed", "errorMessage": "AI service timed out" }
    ],
    "platforms": [
      { "platform": "facebook", "platformPostId": "…", "status": "Partial",
        "content": { "generationId": "…", "versionNo": 1,
                     "title": "…", "content": "…", "hashtag": "…" },
        "images": [ { "sourceBasePostImageId": "…A", "ratioCode": "Square1x1",
                      "selectedGeneratedPostImageId": "…", "sortOrder": 1 } ] }
    ] },
  "isSuccess": true, "isFailed": false, "error": null,
  "traceId": "…", "timestampUtc": "…" }
```

Mảng `images` là kho ảnh dùng chung, mảng `platforms` là các ô ảnh trỏ vào kho đó. Trường `versionNo` **được tính lúc đọc**, không phải cột trong DB — mọi tham chiếu tới một version cụ thể đều dùng `id`. `GET /api/v1/posts/{id}` trả về đúng shape này, nên chuyển sang bất đồng bộ sau này không phải sửa DTO.

`POST .../platforms/{platform}/content/regenerate`

```json
{ "sourceGenerationId": "…v1…", "feedback": "trẻ trung hơn" }
```

`sourceGenerationId = null` nghĩa là làm lại từ `BasePost.Content`; truyền id của một version bất kỳ thuộc bài nền tảng này thì AI làm tiếp từ nội dung của version đó. Giá trị này được ghi thẳng vào `ParentGeneratedPostId` của version mới.

`POST /api/v1/posts/{id}/images/regenerate` — đúng **một** ô ảnh

```json
{ "sourceBasePostImageId": "…A", "ratioCode": "Portrait4x5",
  "sourceGeneratedPostImageId": "…v1…",
  "feedback": "nền sáng hơn" }
```

`sourceGeneratedPostImageId = null` → dựng lại từ ảnh gốc; truyền id một version → dựng tiếp trên ảnh của version đó. Giá trị này ghi thẳng vào `ParentGeneratedPostImageId`.

Endpoint này **chỉ thêm ảnh vào kho**, không đụng tới `PlatformPostImage` của bất kỳ nền tảng nào. Muốn dùng version mới thì chốt ở `PUT .../platforms/{platform}/images`.

`PUT .../platforms/{platform}/images` — ghi đè toàn bộ ô ảnh của một nền tảng

```json
{ "images": [
    { "sourceBasePostImageId": "…A", "ratioCode": "Square1x1",
      "selectedGeneratedPostImageId": "…", "sortOrder": 1 },
    { "sourceBasePostImageId": "…B", "ratioCode": "Landscape16x9",
      "selectedGeneratedPostImageId": "…", "sortOrder": 2 } ] }
```

Dùng cho cả việc đổi thứ tự, bỏ bớt ảnh, lẫn việc chọn version khác cho một ô.

`PUT .../platforms/{platform}/content` — sửa tay

```json
{ "title": "…", "content": "…", "hashtag": "#hoa #tet" }
```

Tạo version `Source = Manual` (hoặc ghi đè version `Manual` cuối nếu nó chưa bị tham chiếu), rồi trỏ `OfficialGenerationId` vào đó.

`PUT .../platforms/{platform}/content/official`

```json
{ "generationId": "…" }
```

### Lỗi

Response luôn bọc bằng `ApiResponseFactory.Base(...)` / `BasePagination(...)`. Lỗi **ném** ra từ service:

| Tình huống | Exception | Message |
| --- | --- | --- |
| Không tìm thấy Post Core hoặc bài đã bị xóa | `NotFoundException` | `Post not found` |
| Chưa sinh bài cho nền tảng đó | `NotFoundException` | `Platform post not found` |
| Nền tảng đó đang sinh dở | `ConflictException` | `Platform post is generating` |
| Vừa gửi preset vừa gửi `ratios` | `BadRequestException` | `Preset and ratios cannot be used together` |
| `basePostImageIds` chứa ảnh của bài khác | `BadRequestException` | `Image does not belong to this post` |
| `sourceGenerationId` không phải version của bài nền tảng này | `BadRequestException` | `Source generation does not belong to this platform post` |
| `sourceGeneratedPostImageId` không thuộc đúng cặp (ảnh gốc, tỷ lệ) đang paraphase | `BadRequestException` | `Source image does not match the requested ratio` |
| `sourceGeneratedPostImageId` trỏ vào version `Failed` | `BadRequestException` | `Cannot regenerate from a failed image` |
| Chọn version ảnh thuộc bài gốc khác | `BadRequestException` | `Selected image does not belong to this post` |
| Version ảnh được chọn không đúng cặp (ảnh gốc, tỷ lệ) của ô đó | `BadRequestException` | `Selected image does not match the slot` |
| Ảnh được chọn có `Status != Succeeded` | `BadRequestException` | `Selected image is not available` |
| `sortOrder` trùng nhau | `BadRequestException` | `Duplicate image order` |
| Chưa cấu hình System Prompt `Post` hoặc Config chuẩn của nền tảng | `BadRequestException` | `System prompt for platform {platform} is not configured` |
| AI trả 2xx nhưng JSON hỏng / thiếu field bắt buộc (`content`, `imageUrl`) | `BadGatewayException` | `AI service returned an invalid response` |
| AI không gọi được / 5xx / timeout lời gọi nội dung | `BadGatewayException` | `AI service is unavailable` |
| AI trả **400/422** (payload của ta sai) | `BadRequestException` | `AI service rejected the request` — log mã lỗi của provider, không dội nguyên văn ra client |
| Cờ `postgen.enabled = false` | `BadRequestException` | `PostGen is disabled` |
| Sai shape / vượt giới hạn độ dài, số ảnh, số tỷ lệ | FluentValidation → **422** qua `ValidationActionFilter` | |


## Phần đã hiện thực

Mốc 1–2: 5 API Post Core; chuẩn nền tảng và preset dùng API Config chung. Các API sinh AI, version và official trong bảng trên là contract đã thiết kế, chưa có route thực thi.

- GET danh sách: `pageIndex` mặc định 1, `pageSize` mặc định 10, clamp tối đa 100; `platform=facebook|instagram|zalo`, `keyword` tìm trong title/content, không phân biệt hoa thường.
- POST/PUT Post Core: `{title, content, hashtag, images: [{id?, imageUrl, sortOrder, width?, height?}]}`. `sortOrder` bắt đầu từ 1, không trùng. POST không nhận id ảnh có sẵn. PUT thay toàn bộ tập; id ảnh phải thuộc bài đang sửa, URL của id cũ không thay đổi. Bỏ id cũ và thêm ảnh không id để thay nguồn.
- Detail dùng `basePostId`, `title/content/hashtag`, `createdAt/updatedAt`, `baseImages` (ảnh nguồn còn hiệu lực), `images` (mọi version ảnh), `platforms` (nội dung chính thức, `versions`, ô ảnh). Kho version không phân trang và vẫn gồm ảnh nguồn đã soft delete để không đứt lịch sử.
- Preset quản lý qua API Config chung với `AdminPolicy`; payload nằm trong `value`, đọc settings trả danh sách Config có `id`. Chi tiết tạo/đọc/sửa/xóa, quyền và giới hạn validation tại [RatioPresetConfig.md](RatioPresetConfig.md).
- Bài không tồn tại hoặc đã xóa trả 404 `Post not found`; sửa URL trên ID ảnh cũ trả 400 `Existing image URL cannot be changed; add a new image instead`. Các lỗi FluentValidation trả 422. Error envelope theo middleware hiện có là `{title,status,detail,messageCode,errors,traceId,timestampUtc}`; success dùng `{value,isSuccess,isFailed,error,traceId,timestampUtc}`.

Payload và traceId kiểm thử thật nằm trong tài liệu từng endpoint.

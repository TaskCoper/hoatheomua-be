# PostGen — Hợp đồng API

Cập nhật 2026-09-14: đã bỏ bảng PlatformStandard và cả API chuẩn nền tảng/preset riêng. Đã bỏ key `postgen.platform.*`; Admin dùng [Config prompt theo nền tảng](PlatformConfig.md) và [Config ratio preset](RatioPresetConfig.md).

## Hợp đồng API

Các endpoint PostGen dưới đây dùng `[Authorize(Policy = JwtExtensions.StaffOrAdminPolicy)]` (quyết định #7). **Ba** endpoint gọi AI — `generate`, `content/regenerate`, `images/regenerate` — thêm `[EnableRateLimiting(RateLimitExtensions.HeavyPolicy)]`; đó là ba chỗ duy nhất tiêu tiền.

`{platform}` trong route là tên enum viết thường: `facebook` | `instagram` | `zalo`.

| Method | Route | Service method | Ghi chú |
| --- | --- | --- | --- |
| POST | `/api/v1/posts` | `CreatePostCore` | Title, Content, Hashtag, danh sách ảnh |
| GET | `/api/v1/posts` | `GetPostCoreList` | phân trang + lọc `platform`, `keyword` |
| GET | `/api/v1/posts/{id:guid}` | `GetPostCoreDetail` | kèm nội dung và ảnh chốt của nền tảng |
| PUT | `/api/v1/posts/{id:guid}` | `UpdatePostCore` | |
| DELETE | `/api/v1/posts/{id:guid}` | `DeletePostCore` | soft delete |
| POST | `/api/v1/posts/{id:guid}/generate` | `GeneratePlatformPosts` | **heavy**, sinh nội dung và ảnh theo lựa chọn/preset |
| POST | `/api/v1/posts/{id:guid}/platforms/{platform}/content/regenerate` | `RegenerateContent` | **heavy**, `sourceGenerationId` + `feedback` — chỉ nội dung |
| PUT | `/api/v1/posts/{id:guid}/platforms/{platform}/content` | `EditPlatformContent` | sửa tay nội dung → tạo version `Manual` và chốt luôn |
| GET | `/api/v1/posts/{id:guid}/platforms/{platform}/content/versions` | `GetContentVersions` | danh sách version nội dung |
| PUT | `/api/v1/posts/{id:guid}/platforms/{platform}/content/official` | `SelectOfficialContent` | chọn version nội dung chính thức |
| POST | `/api/v1/posts/{id:guid}/images/regenerate` | `RegenerateImage` | **heavy**, paraphase lại **đúng một cặp** `(sourceBasePostImageId, ratioCode)`. Chỉ thêm ảnh vào kho, không đụng ô của nền tảng nào |
| GET | `/api/v1/posts/{id:guid}/images` | `GetImageVersions` | mọi version ảnh của bài gốc, gom theo cặp (ảnh × tỷ lệ) |
| PUT | `/api/v1/posts/{id:guid}/platforms/{platform}/images` | `UpdatePlatformImages` | ghi đè toàn bộ ô ảnh của nền tảng: ảnh nào, tỷ lệ nào, version nào, thứ tự nào |
| GET | `/api/v1/posts/{id:guid}/platforms/{platform}/official` | `GetOfficialVersion` | payload sẽ đem đi đăng |

Chừa sẵn, **ghi trong tài liệu nhưng không triển khai**: `POST .../platforms/{platform}/publish`, `POST .../platforms/{platform}/schedule`, `DELETE .../platforms/{platform}/schedule`.

### Tài khoản nền tảng — mốc 11

Nhóm `/api/v1/post-gen/platform-accounts` đã triển khai: GET list, POST `/{platform}/connect`, GET `/{platform}/callback`, POST `/{id}/reconnect`, POST `/{id}/refresh`, DELETE `/{id}`. **AdminPolicy** cho quản trị; callback AllowAnonymous nhưng bắt buộc OAuth state và cookie trình duyệt của phiên hợp lệ. Không áp dụng StaffOrAdminPolicy của nhóm bài cho nhóm account.

Chi tiết DTO, lỗi, OAuth và cấu hình ở [PlatformAccounts.md](PlatformAccounts.md); bàn giao [FE mốc 11](../FE-API-Handoff-PostGen-Milestone11.md). Không có API trả token hoặc đăng bài trong nhóm này.

### DTO chính

**Mốc 4 đã triển khai:** request chỉ có platform vẫn sinh nội dung. Thêm usePreset=true hoặc ratios để sinh ảnh, không dùng đồng thời cả hai. Response giữ shape PostCoreDetail. Xem [GeneratePlatformPosts.md](GeneratePlatformPosts.md).

**Payload mốc 4:**

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
{
  "value": {
    "basePostId": "…",
    "platforms": [
      {
        "platform": "facebook",
        "platformPostId": "…",
        "status": "Partial",
        "content": {
          "generationId": "…",
          "versionNo": 1,
          "title": "…",
          "content": "…",
          "hashtag": "…"
        },
        "images": [
          {
            "sourceBasePostImageId": "…A",
            "ratioCode": "Square1x1",
            "selectedGeneratedPostImageId": "…",
            "sortOrder": 1,
            "imageUrl": "https://cdn.example.com/flowers/generated-square.jpg",
            "pixelWidth": 1080,
            "pixelHeight": 1080
          }
        ]
      }
    ]
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "…",
  "timestampUtc": "…"
}
```

Response không có `value.images` hoặc `platforms[].versions`. `baseImages` chứa ảnh nguồn; mỗi nền tảng có một object `content` chốt (hoặc null) và `images` chỉ gồm ảnh đã chốt thành công, kèm `imageUrl`, `pixelWidth`, `pixelHeight`, theo `sortOrder`. Ô chưa chọn ảnh hoặc ảnh lỗi không xuất hiện trong danh sách này. Kho lịch sử vẫn lưu trong database; API lịch sử nội dung/ảnh riêng đã triển khai ở mốc 5/6. `versionNo` của nội dung được tính lúc đọc, định danh bằng `generationId`. Ví dụ trên rút gọn các trường bài gốc của `PostCoreDetail`.

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
| Thiếu/rỗng Config system prompt của nền tảng | `BadRequestException` | `System prompt for platform {platform} is not configured` |
| Config prompt có isActive=false | `BadRequestException` | `Platform {platform} is disabled` |
| Config prompt thiếu/sai kiểu isActive | `BadRequestException` | `System prompt for platform {platform} is not configured` |
| AI trả 2xx nhưng JSON hỏng / thiếu field bắt buộc (`content`, `imageUrl`) | `BadGatewayException` | `AI service returned an invalid response` |
| AI không gọi được / 5xx / timeout lời gọi nội dung | `BadGatewayException` | `AI service is unavailable` |
| AI trả **400/422** (payload của ta sai) | `BadRequestException` | `AI service rejected the request` — log mã lỗi của provider, không dội nguyên văn ra client |
| Cờ `postgen.enabled = false` | `BadRequestException` | `PostGen is disabled` |
| Sai shape / vượt giới hạn độ dài, số ảnh, số tỷ lệ | FluentValidation → **422** qua `ValidationActionFilter` | |


## Phần đã hiện thực

Mốc 1–2: 5 API Post Core; prompt/bật tắt nền tảng và preset dùng API Config chung; không còn key standard riêng. Mốc 3 đã có POST generate nội dung, AiService proxy và snapshot prompt Config. Sinh ảnh trong generate đã triển khai ở mốc 4; Bốn API version nội dung đã triển khai ở mốc 5; ba API version ảnh đã triển khai trên nhánh `feature/post-gen` ở mốc 6; payload official đã triển khai trên nhánh `feature/post-gen` ở mốc 7.

- GET danh sách: `pageIndex` mặc định 1, `pageSize` mặc định 10, clamp tối đa 100; `platform=facebook|instagram|zalo`, `keyword` tìm trong title/content, không phân biệt hoa thường.
- POST/PUT Post Core: `{title, content, hashtag, images: [{id?, imageUrl, sortOrder, width?, height?}]}`. POST không bắt buộc ảnh: bỏ images/null/[] hợp lệ. PUT yêu cầu mảng images đầy đủ, được phép rỗng; tối đa 20 ảnh. Bài chưa có ảnh trả baseImages=[] và vẫn sinh nội dung bằng mode content được. `sortOrder` bắt đầu từ 1, không trùng. POST không nhận id ảnh có sẵn. PUT thay toàn bộ tập; id ảnh phải thuộc bài đang sửa, URL của id cũ không thay đổi. Bỏ id cũ và thêm ảnh không id để thay nguồn.
- Detail dùng `basePostId`, `title/content/hashtag`, `createdAt/updatedAt`, `baseImages` (ảnh nguồn còn hiệu lực), `platforms` (một object `content` chính thức hoặc null, `images` chỉ gồm ảnh đã chốt thành công kèm URL/kích thước). Không có `value.images` hoặc `platforms[].versions`. Lịch sử vẫn lưu trong database; API lịch sử nội dung/ảnh riêng ở mốc 5/6.
- Preset quản lý qua API Config chung với `AdminPolicy`; payload nằm trong `value`, đọc settings trả danh sách Config có `id`. Chi tiết tạo/đọc/sửa/xóa, quyền và giới hạn validation tại [RatioPresetConfig.md](RatioPresetConfig.md).
- Bài không tồn tại hoặc đã xóa trả 404 `Post not found`; sửa URL trên ID ảnh cũ trả 400 `Existing image URL cannot be changed; add a new image instead`. Các lỗi FluentValidation trả 422. Error envelope theo middleware hiện có là `{title,status,detail,messageCode,errors,traceId,timestampUtc}`; success dùng `{value,isSuccess,isFailed,error,traceId,timestampUtc}`.

Payload và traceId kiểm thử thật nằm trong tài liệu từng endpoint.

## Mốc 4 đã triển khai (2026-09-14)

Generate nhận ratios hoặc usePreset, tối đa 12 cặp ảnh/tỷ lệ sau gom trùng. Bỏ trống/null/[] basePostImageIds lấy mọi ảnh nguồn còn hiệu lực. Mỗi lần thành công chỉ tự điền ô chưa chọn; giữ ảnh chốt và thứ tự cũ. 3 lời gọi ảnh đồng thời, ngân sách AI 180 giây, lưu DB tối đa 10 giây riêng. Xem [contract FE mốc 4](../FE-API-Handoff-PostGen-Milestone4.md) cho validation, partial/all failure và giới hạn kích thước mục tiêu.

## Chọn phần cần sinh bằng `mode`

Cùng endpoint `POST /api/v1/posts/{id}/generate`, `mode` là chuỗi `content`, `images` hoặc `all` (viết thường). Bỏ field thì mặc định `all` để tương thích request cũ; gửi null hoặc giá trị khác trả 422.

| Mode | Xử lý |
| --- | --- |
| `content` | Chỉ gọi AI sinh nội dung, tạo version và chốt nội dung mới. Giữ nguyên ảnh/slot và thứ tự đã chốt. Không gửi ratios/usePreset=true; nếu gửi trả 422. Không cần Config chỉ dẫn ảnh/preset. |
| `images` | Chỉ gọi AI sinh ảnh. Không thêm GeneratedPost, không đổi nội dung, OfficialGenerationId hoặc SelectedAt. Mỗi nền tảng phải có ratios hoặc usePreset=true; thiếu trả 422. |
| `all` (mặc định) | Sinh nội dung trước, rồi ảnh theo lựa chọn. Không có lựa chọn ảnh thì chỉ sinh nội dung như request cũ. |

Hai lần gọi tách rời để tránh sinh nội dung hai lần:

```json
{
  "mode": "content",
  "platforms": [{ "platform": "facebook" }]
}
```

```json
{
  "mode": "images",
  "platforms": [{
    "platform": "facebook",
    "ratios": [{ "ratioCode": "Story9x16", "basePostImageIds": [] }]
  }]
}
```

Images mode vẫn kiểm postgen.enabled và isActive trong Config nền tảng, cùng chỉ dẫn ảnh/preset khi dùng; không yêu cầu hoặc gửi value.content của prompt nội dung. Có thể sinh ảnh trước khi có nội dung: platform.content vẫn null, chưa sẵn sàng đăng. Thành công/lỗi ảnh chỉ cập nhật trạng thái xử lý, ảnh và lastError; nội dung chốt giữ nguyên kể cả khi mọi ảnh lỗi. Hủy request hoặc ghi DB lỗi vẫn giải phóng claim và giữ dữ liệu đã chốt trước.

Mode áp dụng cho toàn bộ request. Các mode cùng dùng khóa/claim của nền tảng để chặn yêu cầu sinh trùng; không gọi content và images đồng thời cho cùng nền tảng. Không tự retry POST.

## Mốc 5 đã triển khai (2026-09-14)

Bốn route content/regenerate, content (PUT), content/versions (GET), content/official (PUT) đã chạy. POST regenerate dùng heavy rate limit; ba route còn lại không gọi AI. Source/official phải thuộc cùng PlatformPost, mọi mutation dùng khóa BasePost và chặn Generating hoặc các trạng thái xuất bản. Regenerate tự chốt bản mới; manual/select giữ ảnh và trạng thái hiện tại. Manual chỉ ghi đè tại chỗ khi đang official, mới nhất và chưa có child.

GET versions trả ContentHistory gồm platformPostId/platform/officialGenerationId và versions (ContentVersion + feedback/isOfficial); mutation trả PostCoreDetail, không gắn lịch sử vào platforms. Lịch sử chưa phân trang. Body và lỗi chi tiết tại [bàn giao FE mốc 5](../FE-API-Handoff-PostGen-Milestone5.md).

## Contract mốc 6 đã triển khai

- POST `/api/v1/posts/{id}/images/regenerate`: body sourceBasePostImageId/ratioCode/sourceGeneratedPostImageId?/feedback?; trả ApiResponse<ImageVersion>. HTTP 200 kể cả attempt Failed sau khi gọi provider; FE kiểm value.status/errorMessage. Không thay content, trạng thái hoặc bất kỳ ảnh chốt nào.
- GET `/api/v1/posts/{id}/images`: ApiResponse<ImageHistory> với basePostId và groups; mỗi nhóm có sourceBasePostImageId/originalImageUrl/sourceIsDeleted/ratioCode/versions. Version gồm id, parent, versionNo, source/result URL, target pixels, status/error/feedback/createdAt/selectedPlatforms. Chưa phân trang.
- PUT `/api/v1/posts/{id}/platforms/{platform}/images`: mảng images bắt buộc, 0–100 ô; [] bỏ hết, selectedGeneratedPostImageId null giữ ô chưa chốt. Cặp và sortOrder không trùng (400). Chọn sai bài/cặp hoặc Failed trả 400. Thành công trả PostCoreDetail; tính Ready từ nội dung chốt và ít nhất một ảnh chốt, không đổi nội dung.

Chỉ POST regenerate dùng HeavyPolicy và AI; PUT/history không đọc Config/AI. Body, ví dụ response, xử lý lỗi và kịch bản FE tại [bàn giao mốc 6](../FE-API-Handoff-PostGen-Milestone6.md). Không thay shape detail hoặc generate; không thêm value.images/platforms[].versions. Mốc 7 official riêng đã triển khai trên nhánh `feature/post-gen`.

## Contract mốc 7 đã triển khai

GET `/api/v1/posts/{id}/platforms/{platform}/official` trả ApiResponse<OfficialVersion>: basePostId/platformPostId/platform, generationId/title/content/hashtag/selectedAt, images chốt (shape như detail), status/isReady/notReadyReasons. Chỉ đọc, không heavy/AI/Config, không sửa trạng thái. Scope content đúng PlatformPost, ảnh đúng bài/cặp và Succeeded có URL. Không có lịch sử hoặc ảnh nguồn trong response.

Có content không rỗng và ít nhất một ảnh hợp lệ → Ready; thiếu trả 200 với MissingContent/MissingImages. Generating hoặc các trạng thái publish có lý do riêng, isReady=false. Cờ Ready lưu trong DB không thay thế kiểm dữ liệu thật; status của official phản ánh payload, có thể khác trạng thái xử lý trong detail. Tất cả truy vấn cùng snapshot PostgreSQL Repeatable Read. Ví dụ và cách FE xử lý tại [bàn giao mốc 7](../FE-API-Handoff-PostGen-Milestone7.md).

# Bàn giao Frontend — PostGen mốc 3: sinh nội dung theo nền tảng

> Tài liệu này mô tả phạm vi mốc 3. Mốc 4 đã bổ sung sinh ảnh trên cùng endpoint; quy tắc từ chối yêu cầu ảnh bên dưới chỉ còn là lịch sử. Dùng [bàn giao mốc 4](FE-API-Handoff-PostGen-Milestone4.md) cho contract hiện tại.

Đối chiếu code ngày **14/09/2026**, gồm thay đổi bỏ `postgen.platform.*`.

## 1. FE cần triển khai gì

Trên màn hình chi tiết bài gốc đã lưu:

1. Cho chọn một hoặc nhiều nền tảng: Facebook, Instagram, Zalo.
2. Nút **Tạo nội dung** gọi backend để sinh nội dung cho các nền tảng đã chọn.
3. Hiển thị kết quả riêng từng nền tảng: tiêu đề, nội dung, hashtag và số phiên bản.
4. Hiển thị trạng thái đang tạo/thành công/thất bại, giữ nội dung cũ khi một lượt tạo mới lỗi.
5. Mỗi nền tảng chỉ hiển thị một nội dung chốt từ `platforms[].content`; chưa tích hợp màn hình lịch sử ở mốc này.

Mốc này chưa có sinh ảnh, sinh lại theo feedback/version nguồn, sửa tay, chọn lại phiên bản chính thức, publish hoặc schedule. Không hiển thị các nút đó như chức năng đã dùng được. Ảnh gốc vẫn được hiển thị như PostCore.

**Trạng thái backend:** code mốc 3 đã triển khai và kiểm thử local, nằm trong nhánh `feature/post-gen`; chưa deploy. FE dùng backend đã có code mới và toàn bộ migration, gồm `20260914073414_ConsolidatePostGenPlatformConfigs`. Không giả định server hiện tại đã có endpoint.

## 2. Endpoint và xác thực

| Method | Route | Mục đích |
| --- | --- | --- |
| POST | `/api/v1/posts/{id}/generate` | API mới: sinh nội dung cho nền tảng đã chọn |
| GET | `/api/v1/posts/{id}` | API có sẵn: đọc lại bài, trạng thái và nội dung chính thức |

- `{id}` là `basePostId` dạng GUID, lấy từ PostCore.
- Ghép route với **API origin backend hoatheomua-be** của môi trường FE.
- Gửi JWT đăng nhập: `Authorization: Bearer <access_token>`. API dùng policy StaffOrAdmin; tài khoản Admin hiện tại gọi được.
- POST gửi `Content-Type: application/json`.
- FE chỉ gọi backend này. Backend tự đọc prompt, gọi AI proxy và lưu kết quả; FE không cần AI key hoặc gọi proxy trực tiếp.
- PostCore CRUD và DTO chi tiết bài: [bàn giao PostCore](FE-API-Handoff-PostCore.md).

## 3. Request generate

```http
POST /api/v1/posts/{basePostId}/generate
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "platforms": [
    { "platform": "facebook" },
    { "platform": "instagram" }
  ]
}
```

```ts
type PostGenPlatform = "facebook" | "instagram" | "zalo";

interface GeneratePlatformPostsRequest {
  platforms: Array<{ platform: PostGenPlatform }>;
}
```

| Quy tắc | FE xử lý |
| --- | --- |
| Từ 1 đến 3 nền tảng, không trùng | Dùng checkbox/multi-select; chưa chọn thì khóa nút tạo |
| Bài gốc phải tồn tại và chưa xóa | Lưu PostCore trước; lưu xong mới gọi generate |
| AI dùng nội dung bài gốc đã lưu | Nếu có chỉnh sửa chưa lưu, yêu cầu lưu trước khi tạo |
| Prompt nền tảng phải hợp lệ và đang bật | Xem phần Config bên dưới |
| Mốc 3 chỉ text | Không gửi `ratios` hoặc `usePreset=true`; backend trả 422 cho yêu cầu ảnh |

Không gửi title/content/hashtag, prompt, model, image IDs, feedback hoặc sourceGenerationId vào request này. Backend lấy bài nguồn theo basePostId. Nếu dùng chung DTO cũ, có thể bỏ `usePreset` hoặc gửi false; nên bỏ hoàn toàn `ratios`.

## 4. Response thành công

**HTTP 200**, kiểu `ApiResponse<PostCoreDetail>`, cùng cấu trúc với GET chi tiết PostCore. FE tái sử dụng DTO/cache của API chi tiết bài.

Ví dụ minh họa một nền tảng:

```json
{
  "value": {
    "basePostId": "11111111-1111-4111-8111-111111111111",
    "title": "Bó hoa Cảm Ơn",
    "content": "Nội dung bài gốc đã lưu",
    "hashtag": "#HoaTheoMua",
    "createdAt": "2026-09-14T07:00:00Z",
    "updatedAt": null,
    "baseImages": [
      {
        "id": "22222222-2222-4222-8222-222222222222",
        "imageUrl": "https://example.com/flower.jpg",
        "sortOrder": 1,
        "width": null,
        "height": null
      }
    ],
    "platforms": [
      {
        "platformPostId": "33333333-3333-4333-8333-333333333333",
        "platform": "facebook",
        "status": "Generated",
        "lastError": null,
        "content": {
          "generationId": "44444444-4444-4444-8444-444444444444",
          "parentGeneratedPostId": null,
          "versionNo": 1,
          "source": "Ai",
          "title": "Gửi lời cảm ơn bằng một bó hoa",
          "content": "Nội dung được AI viết lại cho Facebook.\n\nĐoạn tiếp theo.",
          "hashtag": "#HoaTheoMua #CamOn",
          "createdAt": "2026-09-14T07:01:00Z"
        },
        "images": []
      }
    ]
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "example-trace-id",
  "timestampUtc": "2026-09-14T07:01:00Z"
}
```

| Dữ liệu hiển thị | Lấy từ |
| --- | --- |
| Bài gốc | `value.title`, `value.content`, `value.hashtag` |
| Ảnh gốc | `value.baseImages` |
| Tab nền tảng | `value.platforms[]`, định danh bằng `platform` |
| Nội dung chính thức đang được chọn | `platforms[].content` — có thể null |
| Tiêu đề AI / thân bài / hashtag | `platforms[].content.title` / `.content` / `.hashtag` |
| Số phiên bản | `platforms[].content.versionNo` |
| Trạng thái / lỗi lượt gần nhất | `platforms[].status` / `.lastError` |

Lưu ý khi render:

- `platforms[].content` là một object; `.content.content` mới là chuỗi thân bài.
- `hashtag` là chuỗi, có thể rỗng/null; không phải mảng. Title có thể null theo DTO.
- Hiển thị xuống dòng của nội dung bằng text, không render trực tiếp thành HTML.
- Response là **toàn bộ chi tiết bài**, có thể gồm cả nền tảng không nằm trong request. Map theo `platform`, không dựa vào vị trí mảng.
- Mỗi platform chỉ có một object `content` hoặc null; response không có trường `versions`. Lịch sử vẫn lưu ở backend và sẽ được đọc qua API riêng `GET /api/v1/posts/{id}/platforms/{platform}/content/versions` ở mốc 5; route này chưa triển khai.
- `generationId` là định danh phiên bản. `versionNo` chỉ để hiển thị và đã được backend tính; FE không tự tăng số.
- Mốc 3 không sinh ảnh. `value.images` đã bỏ; `baseImages` vẫn là ảnh nguồn. `platforms[].images` chỉ chứa ảnh đã chốt thành công, kèm `imageUrl`, `pixelWidth`, `pixelHeight`, theo `sortOrder`; rỗng nếu chưa chốt ảnh. FE không cần tra kho ảnh chung.
- PromptSnapshot, ID Config prompt và metadata AI được lưu ở backend, không có trong response chi tiết cho FE.

## 5. Hành vi và trạng thái UI

**Request đồng bộ:** POST chờ AI và lưu database rồi trả 200; không trả job ID/202, không có SSE hoặc phần trăm tiến độ.

| Tình huống | Hành vi FE |
| --- | --- |
| Chuẩn bị gọi | Giữ nội dung đang hiển thị, bật trạng thái “Đang tạo nội dung…” |
| Request đang chờ | Chặn bấm tạo trùng cho nền tảng đó; khóa lưu/xóa bài gốc |
| Response 200 | Thay cache chi tiết bài bằng `response.value`, hiển thị kết quả mới; làm mới danh sách bài nếu cần |
| GET thấy `Generating` | Hiển thị đang tạo; khóa lưu/xóa bài và tạo lại cùng nền tảng |
| GET thấy `Generated` | Hiển thị `content` hiện tại; đây chưa phải bài đã xuất bản |
| GET thấy `Failed` và `content` khác null | Hiển thị lỗi lượt mới, vẫn giữ nội dung chính thức cũ |
| GET thấy `Failed` và `content` null | Chưa có kết quả; cho người dùng chủ động thử lại |

Mỗi lần POST thành công tạo **một phiên bản mới cho từng nền tảng được chọn**, từ bài gốc, và tự chọn phiên bản đó làm official. Có thể gọi lại từ bài gốc; chưa hỗ trợ feedback hoặc chọn phiên bản nguồn.

Nếu chọn hai nền tảng mà một lời gọi AI lỗi, backend **không thêm phiên bản mới cho cả lượt**. Các nền tảng đã bắt đầu sinh chuyển Failed, bản official cũ vẫn giữ. Nếu lỗi validation/config trước khi bắt đầu, trạng thái cũ không đổi. Nền tảng ngoài request không đổi.

### Chờ lâu, mất kết nối và refresh trang

- Backend có timeout từng lời gọi AI mặc định 120 giây, tổng ngân sách lượt sinh 180 giây. Cấu hình HTTP client FE cho phép chờ hơn 180 giây; đây không phải cam kết thời gian đáp ứng của proxy triển khai.
- Không tự retry POST generate ở HTTP client hoặc thư viện mutation. Request trước có thể đã tạo version dù FE không nhận được response.
- Sau lỗi mạng/timeout, GET chi tiết trước để đối chiếu phiên bản và trạng thái. Nếu đang Generating, có thể đọc lại GET mỗi 3–5 giây với số lần/thời gian chờ hữu hạn; dừng khi rời trang. Chỉ retry POST khi người dùng chủ động bấm lại và bài không còn Generating.
- Refresh hoặc đóng tab không bảo đảm hủy được xử lý phía server. Hiện không có API cancel job.
- Nếu Generating kéo dài bất thường, cung cấp thao tác đọc lại trạng thái và báo lỗi để kiểm tra backend; không tự suy diễn là đã thất bại hoặc cho phép sửa/xóa bài. Backend chưa có cơ chế phục hồi tự động khi process bị dừng đột ngột.

## 6. Xử lý lỗi

| HTTP | Trường hợp | FE xử lý |
| --- | --- | --- |
| 400 | Prompt thiếu/sai, nền tảng bị tắt, PostGen bị tắt, AI từ chối request hoặc JSON request không hợp lệ | Hiển thị thông báo phù hợp, kiểm cấu hình/đầu vào; không tự retry |
| 401 | Thiếu/hết hạn JWT | Theo luồng đăng nhập/refresh token của ứng dụng; không tự phát lại generate một cách mù quáng |
| 403 | Không đủ quyền | Hiển thị không có quyền |
| 404 | Bài không tồn tại/đã xóa | Quay lại danh sách hoặc làm mới chi tiết |
| 409 | Nền tảng đang sinh hoặc trạng thái không cho phép | GET chi tiết và cập nhật trạng thái UI |
| 422 | Không chọn nền tảng, trùng nền tảng, gửi usePreset=true/ratios | Hiển thị lỗi validation theo field |
| 429 | Vượt rate limit | Đọc `Retry-After` (giây), thông báo và tạm khóa nút tạo |
| 502 | AI lỗi kết nối/timeout, trả JSON sai, hỏi lại hoặc nội dung rỗng | Thông báo không tạo được nội dung, GET chi tiết; giữ bản cũ nếu có |
| 500 / lỗi mạng | Lỗi hệ thống hoặc mất kết nối | Hiển thị lỗi chung, GET lại trước khi người dùng thử lại |

Envelope AppException thường có dạng:

```json
{
  "title": "Bad Gateway",
  "status": 502,
  "detail": "An unexpected error occurred.",
  "messageCode": "BAD_GATEWAY",
  "errors": null,
  "traceId": "example-trace-id",
  "timestampUtc": "2026-09-14T07:01:00Z"
}
```

Middleware hiện che detail cấp ngoài của lỗi 5xx; ở môi trường Development, errors có thể là object `{detail: ...}` thay vì null. FE xử lý theo status/messageCode, không phụ thuộc các câu lỗi AI nội bộ. Chỉ coi errors là lỗi theo field khi đó là một danh sách. Với 422, `errors` là danh sách `{field, message}`, ví dụ field `platforms` hoặc `platforms[0].usePreset`.

Không giả định mọi lỗi đều là JSON envelope: 429 có thể là plain text kèm Retry-After; 401/403 có thể không có body; lỗi model binding có thể là ProblemDetails. HTTP layer cần fallback khi parse JSON thất bại. Giữ traceId nếu có để hỗ trợ đối chiếu lỗi.

## 7. Config nền tảng — dành cho màn hình Admin

Chỉ có ba key:

| Nền tảng | Key |
| --- | --- |
| Facebook | `postgen.system-prompt.facebook` |
| Instagram | `postgen.system-prompt.instagram` |
| Zalo | `postgen.system-prompt.zalo` |

Ví dụ body POST/PUT Config:

```json
{
  "group": "postgen",
  "key": "postgen.system-prompt.facebook",
  "kind": "Setting",
  "isPublic": false,
  "value": {
    "isActive": true,
    "content": "<system prompt hoàn chỉnh>"
  }
}
```

- Đọc GET `/api/v1/configs/settings?groupName=postgen&key=postgen.system-prompt.`. Response.value là danh sách Config. Bộ lọc tìm chuỗi: FE chọn đúng group/key và Kind=Setting, không lấy phần tử đầu một cách mặc định.
- Nếu cần điều chỉnh lựa chọn nền tảng, Admin có thể dùng value.isActive để vô hiệu hóa lựa chọn đã tắt. Backend vẫn là nơi kiểm quyết định cuối cùng.
- POST `/api/v1/configs` khi chưa có; PUT `/api/v1/configs/{id}` khi đã có. PUT thay toàn bộ value, phải giữ cả content và isActive.
- Xóa mềm bằng DELETE `/api/v1/configs/{id}`; tắt tạm thời bằng PUT isActive=false.
- Đây là API Admin; việc quản trị prompt không phải một field gửi vào request generate.
- Bỏ phần quản lý platform standard và mọi lời gọi/key `postgen.platform.*`. Không gọi `/api/v1/platform-standards`.
- Ratio preset vẫn riêng, không dùng trong generate của mốc 3.

Payload đầy đủ cho từng prompt và hướng dẫn Config: [PlatformConfig.md](Service%20-%20PostGen/PlatformConfig.md), [ParaphrasePromptData.md](Service%20-%20PostGen/ParaphrasePromptData.md).

## 8. Checklist nghiệm thu FE

- Tạo/lưu bài gốc, chọn Facebook + Instagram, nhận và hiển thị hai nội dung riêng.
- Không chọn hoặc chọn trùng nền tảng được chặn trước khi gửi; payload không có trường sinh ảnh.
- Bấm nhanh nhiều lần chỉ gửi một POST; trong lúc sinh khóa lưu/xóa bài.
- Gọi tạo lại từ bài gốc: số version tăng, `content` đổi sang bản mới.
- Lượt tạo mới lỗi: vẫn thấy bản cũ; không xóa kết quả chỉ vì status=Failed.
- Refresh trang hoặc gặp 409: GET chi tiết khôi phục đúng trạng thái/nội dung.
- Xử lý 422 theo field, 429 plain text + Retry-After, 502 generic, lỗi mạng không auto retry POST.
- Admin tắt một nền tảng qua Config prompt: lựa chọn phản ánh trạng thái; backend từ chối sinh nếu vẫn gửi.
- Không xuất hiện chức năng sinh ảnh, sửa/chọn version, feedback hoặc đăng bài chưa có API.

Backend đã kiểm **108 test Config/PostGen**, **48 request HTTP / 89 kiểm tra PASS**. HTTP probe dùng response AI giả lập, còn API/DB thật; một lời gọi proxy thật ở bước trước xác nhận contract text. Không coi đây là bằng chứng môi trường deployed đã sẵn sàng.

Nguồn đối chiếu: [GeneratePlatformPosts.md](Service%20-%20PostGen/GeneratePlatformPosts.md), [Plan.md](Service%20-%20PostGen/Plan.md).

## Bổ sung mode cho FE

Khi chỉ cần sinh nội dung, gửi `mode: "content"`. Khi đã có nội dung và muốn sinh ảnh sau, gửi `mode: "images"` cùng ratios/usePreset; backend giữ nguyên nội dung chốt. Bỏ mode mặc định all nên request có chọn ảnh sẽ sinh lại cả nội dung. Xem [contract mode và ví dụ](FE-API-Handoff-PostGen-Milestone4.md).

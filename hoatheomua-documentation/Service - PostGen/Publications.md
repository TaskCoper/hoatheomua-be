# Mốc 12 — Tác vụ đăng và snapshot

Triển khai local ngày **2026-09-15**. Mốc 12 cung cấp API, snapshot, job/attempt/review, outbox, worker và recovery. **Cập nhật mốc 13:** đã đăng ký adapter Facebook/Instagram gọi Graph API theo [PublishingAdapters.md](PublishingAdapters.md). Flag vẫn mặc định false; chưa smoke account thật hoặc deploy. Kiểm thử mốc 12 dưới đây là bằng fake adapter tại thời điểm nghiệm thu mốc 12.

## Quy tắc đã hiện thực

- Chỉ Admin được preview, tạo job, xem lịch sử và xử lý tác vụ. Post Core vẫn dùng Staff/Admin như trước.
- Một job đang hoạt động trên mỗi PlatformPost. `Queued`, `Scheduled`, `Publishing`, `NeedsReview` là trạng thái hoạt động.
- Snapshot cố định chứa title/content/hashtag, text đã format, version formatter/API, ID nền tảng/account đích và danh sách version ảnh/URL/thứ tự. Không giữ token trong snapshot/message/DTO. Không chỉ giữ FK đến Manual vì dòng đó có thể được sửa tại chỗ.
- Snapshot hiện lưu JSONB trong `PostPublication`, không cần bảng asset hoặc kho ảnh mới. Trigger PostgreSQL chặn sửa snapshot/identity. URL vẫn có thể hết hạn hoặc thay đổi file bên ngoài; không bảo đảm bytes bất biến.
- Preview revision là SHA-256 của dữ liệu snapshot, account revision và thời điểm chọn chính thức. Gửi revision cũ sau thay đổi → HTTP 409.
- Idempotency key duy nhất theo PlatformPost: cùng key/cùng account và expectedRevision → job cũ; khác request → 409. Retry HTTP của publish không gửi provider thêm lần nữa.
- Job thành công hoặc đã đóng với kết quả chưa rõ chặn tạo bài đăng mới trên cùng PlatformPost. Luồng đăng lại bài đã đăng nằm ngoài phạm vi này.
- Chỉ thao tác publish/retry tạo message qua bus outbox trong cùng transaction với job. Generate, edit, official và preview không đăng bài hoặc tạo container.

## Trạng thái job

| Status | Ý nghĩa và thao tác |
| --- | --- |
| `Queued` | Đã nhận tác vụ, worker chưa claim; có thể cancel với revision đúng |
| `Publishing` | Worker đã bắt đầu, có thể đang chuẩn bị media hoặc chờ poll; không cho cancel |
| `Succeeded` | Có bằng chứng thành công và external post ID; đọc link lỗi không được gửi lại bài |
| `Failed` | Đã xác định thất bại trước khi đăng hoặc có bằng chứng chưa đăng; chỉ retry nếu `canRetry=true` |
| `NeedsReview` | Có thể đã đăng nhưng kết quả chưa rõ; khóa đăng lại, đối soát có giới hạn hoặc Admin yêu cầu kiểm tra |
| `Cancelled` | Hủy thành công trước khi worker bắt đầu; message cũ bị bỏ qua |
| `Closed` | Admin dừng xử lý, vẫn giữ `publishMayHaveOccurred=true`; không có nghĩa bài bên ngoài đã bị hủy/xóa |
| `Scheduled` | Đã có API đặt/đổi/hủy lịch ở mốc 14; xem Scheduling.md |

`PlatformPost.Status` là trạng thái tổng hợp tương thích enum cũ: Queued/Publishing/NeedsReview ánh xạ `Publishing`, Succeeded → `Published`, Failed/Closed → `PublishFailed`. FE xem job để phân biệt đầy đủ, không suy ra trạng thái job chỉ từ detail bài.

## Khóa sửa/xóa

| Thao tác | Khi có job hoạt động |
| --- | --- |
| Sửa/xóa bài gốc, thay tập ảnh nguồn qua PUT | Chặn nếu bất kỳ nền tảng của bài còn job hoạt động |
| Generate cho nền tảng đang có job | Chặn vì generate thay bản chốt |
| Regenerate nội dung, sửa Manual, chọn nội dung chính thức | Chặn trên nền tảng có job |
| Thay ảnh chốt/thứ tự ảnh | Chặn trên nền tảng có job |
| Regenerate một ảnh vào kho chung | Cho phép; chỉ thêm version, không thay ảnh chốt |
| Sinh/chỉnh nền tảng khác của cùng bài | Cho phép nếu nền tảng đó không bị khóa |
| GET bài, official, lịch sử | Cho phép, không tạo tác vụ |
| Disconnect tài khoản đang dùng bởi job hoạt động | HTTP 409 |

BasePost là khóa đầu tiên để serialize với Post Core/generation. Thứ tự ghi của publication: BasePost → PlatformPost → PlatformAccount → PostPublication. Refresh/reconnect có thể thay credential của cùng định danh account, nhưng snapshot không đổi đích đăng. Worker kiểm account ở mỗi bước; kết quả phiên cũ không ghi đè credential mới.

Muốn thay nội dung của job chưa chạy: cancel → chỉnh/chốt → preview → publish với key mới. Retry job Failed giữ **snapshot cũ**, không tự lấy bản chốt mới. Với lịch tương lai, quy trình hủy/chỉnh/tạo lại lịch được triển khai ở mốc 14.

## API

Tất cả endpoint dùng `AdminPolicy`, response theo `ApiResponseFactory`.

| Method | Route | Kết quả |
| --- | --- | --- |
| GET | `/api/v1/posts/{id}/platforms/{platform}/publication-preview?accountId={uuid}` | 200: revision, snapshot, canPublish, reasons, mediaVerification |
| POST | `/api/v1/posts/{id}/platforms/{platform}/publish` | 202: job; Location đến detail |
| GET | `/api/v1/posts/{id}/platforms/{platform}/publications?pageIndex=1&pageSize=20` | Phân trang, 1-based, pageSize tối đa 100 |
| GET | `/api/v1/post-gen/publications/{id}` | Job, snapshot, attempts, reviews; vẫn đọc được sau soft delete bài gốc |
| POST | `/api/v1/post-gen/publications/{id}/retry` | 202: cùng job/snapshot, revision mới |
| POST | `/api/v1/post-gen/publications/{id}/cancel` | 200 hoặc 409 nếu worker đã claim |
| POST | `/api/v1/post-gen/publications/{id}/recheck` | Đối soát, trả trạng thái hiện tại; không publish |
| POST | `/api/v1/post-gen/publications/{id}/resolve` | Yêu cầu xác minh bằng chứng hoặc đóng xử lý |

Publish:

```json
{
  "accountId": "00000000-0000-0000-0000-000000000001",
  "idempotencyKey": "request-uuid-do-not-change-on-http-retry",
  "expectedRevision": "<64 ký tự revision từ preview>"
}
```

Retry/cancel/recheck: `{"expectedRevision": 4}` — dùng revision số của **job**, khác revision chuỗi của preview.

Resolve:

```json
{
  "expectedRevision": 7,
  "action": "ConfirmPublished",
  "externalPostId": "provider-post-id",
  "note": "Đã tìm thấy bài tương ứng trên tài khoản đích."
}
```

Action: `ConfirmPublished`, `ConfirmNotPublished`, `Close`. Ghi chú bắt buộc; ConfirmPublished cần external ID. Hiện nhận ID, chưa nhận URL rồi tự parse. Admin không được ép thành công/chưa đăng chỉ bằng lời xác nhận: adapter phải xác minh. Nếu không có bằng chứng đủ chắc chắn, vẫn `NeedsReview`. Close chỉ đóng xử lý và giữ lịch sử; không xóa bài ngoài provider. Mỗi thao tác ghi actor/time/note vào review append-only.

Job trả: `publicationId`, `status`, `revision`, `requestedBy`, `createdAt`, `updatedAt`, `completedAt`, `attemptCount`, `canRetry`, `externalPostId`, `externalPostUrl`, `errorCode`, `publishMayHaveOccurred`.

Lỗi tiêu biểu: `PUBLICATION_PREVIEW_CHANGED`, `PUBLICATION_CHANGED`, `IDEMPOTENCY_CONFLICT`, `PUBLICATION_EXISTS`, `PUBLICATION_LOCKED`, `PUBLICATION_RETRY_NOT_ALLOWED`, `PUBLICATION_CANCEL_NOT_ALLOWED`, `PUBLICATION_REVIEW_NOT_ALLOWED`, `PUBLISHING_DISABLED`, `PUBLISHING_ADAPTER_UNAVAILABLE`. Lỗi 409 yêu cầu tải lại trạng thái, không tự tạo key mới để vượt qua conflict.

## Worker, outbox và recovery

Adapter contract nội bộ là `IPublishingAdapter`: Validate, Preflight và Execute theo operation Prepare/Publish/Reconcile. Mỗi Execute chỉ làm **một bước**, trả checkpoint với media IDs/container ID/readiness; không trả response provider raw hoặc tự gửi nhiều bước mà chưa lưu checkpoint.

1. Claim bằng transaction ngắn: kiểm revision/job/account, tạo lease, ghi attempt intent và commit.
2. Đóng transaction rồi mới gọi preflight/provider với timeout. Trước operation Publish, đặt `PublishMayHaveOccurred=true` trong DB.
3. Transaction ngắn ghi checkpoint/kết quả nếu lease vẫn thuộc worker đó. Stale response không ghi đè worker đã recovery.
4. Prepare/Pending có `NextRunAt` để recovery phát message tiếp; không giữ transaction hoặc thread chờ poll dài.
5. Lease hết hạn: có publish intent → NeedsReview; chưa có intent → Failed có thể retry. Không tự gửi lại POST sau mất response/crash.
6. Reconcile chỉ chuyển Failed/retry khi provider trả `ConfirmedNotPublished=true`. Không tìm thấy trên feed không đủ bằng chứng. Có external ID thành công thì giữ thành công ngay cả khi chưa lấy được link.

Queue production `postgen-publication` được loại khỏi callback retry và EF consumer outbox chung; nó không giữ inbox transaction qua HTTP. API/recovery dùng **bus outbox** hiện có. Scanner đọc DB mỗi 15 giây, batch tối đa 50 bằng `FOR UPDATE SKIP LOCKED`; message bị mất/chưa xử lý có thể phát lại sau 60 giây. Revision/lease/trạng thái DB bảo vệ khỏi message trùng, không phụ thuộc cửa sổ dedup của inbox. [MassTransit transactional outbox](https://masstransit.io/documentation/patterns/transactional-outbox).

## Cấu hình và giới hạn

`PublishingOptions`: FacebookEnabled=false, InstagramEnabled=false, OperationTimeoutSeconds=30, LeaseSeconds=120, MaxSteps=80, MaxAttempts=200, RecoveryIntervalSeconds=15. Lease phải dài hơn timeout ít nhất 30 giây. Giới hạn steps áp dụng cho một lượt chạy; retry được phép reset step count, tổng attempts vẫn bị chặn. Đối soát tự động dừng khi hết budget; dữ liệu và trạng thái chưa rõ được giữ lại.

Mốc 13 đã đăng ký adapter thật; bật flag sau khi đủ cấu hình sẽ cho phép worker gửi Graph API. Xem PublishingAdapters.md về HTTP error mapping, preflight account/media và giới hạn provider. Preview mốc 12 kiểm bản chốt/account local/validator adapter, **không gọi provider**, luôn ghi `mediaVerification=Unverified`; canPublish nghĩa là có thể nhận job, không cam kết provider chấp nhận media. Worker mới kiểm điều kiện bên ngoài qua adapter.

Không thêm storage/import/đo ảnh/chuyển định dạng của mốc 10. Không đặt lịch của mốc 14. Chưa có API trả toàn bộ job theo trạng thái cho màn hình vận hành; danh sách hiện theo bài/nền tảng.

## Migration và vận hành

`20260915120000_AddPostGenPublications` chỉ thêm bảng/index/trigger, không đổi version nội dung/ảnh cũ. EF scaffold vẫn gặp NullReferenceException ở comparer snapshot enum hiện hữu; migration và phần snapshot mới được viết tường minh, kiểm bằng PostgreSQL thật.

Down chỉ cho phép khi PostPublication rỗng. Nếu đã có lịch sử, rollback phải tắt nhận job mới/adapter và giữ schema để đối soát, không xóa bằng chứng. Trigger bảo vệ snapshot và review. Khi tắt flag giữa lượt chạy, tác vụ chưa gửi sẽ Failed, tác vụ có khả năng đã gửi giữ NeedsReview.

Theo dõi job NeedsReview, lease quá hạn, tuổi job Queued, lỗi recovery và attempts. `errorCode` là mã ổn định, provider code/request ID chỉ nhận qua contract adapter đã lọc; không log exception/raw payload chứa token. Cấp lại quyền/token qua account workflow, sau đó recheck hoặc retry tùy trạng thái. Không UPDATE status trực tiếp trong DB để ép gửi lại.

## Kiểm chứng

- Bộ test `PostPublishingTest`: idempotency, snapshot Manual, preview conflict, guard, cancellation, timeout/response thiếu ID, review có bằng chứng, Closed, lease/crash, PostgreSQL constraint/migration và rollback job/outbox.
- HTTP probe: JWT anonymous/Staff/Admin, 8 endpoint publication, quyền sửa bản chốt, RabbitMQ → consumer → timeout → NeedsReview → xác minh thành công; xác nhận chỉ một lệnh publish và không gọi AI.
- Lệnh/probe tại [PublicationVerification](PublicationVerification/Program.cs); evidence tại [publication-api-evidence.json](PublicationVerification/results/publication-api-evidence.json). Toàn bộ provider là fake; chưa deploy hoặc gọi Meta thật trong mốc này.


## Cập nhật mốc 14

Đã thêm scheduledAt nullable vào response job, API POST/PUT/DELETE schedule và scanner cho Scheduled. Contract hiện hành: [Scheduling.md](Scheduling.md). Snapshot mốc 12 giữ nguyên; giờ lịch nằm ngoài snapshot và có audit/trigger riêng.

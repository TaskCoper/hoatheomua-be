# AI Flower background jobs — implementation và rollout

Ngày: 2026-09-22. Trạng thái: đã triển khai trong source local; chưa deploy.

## Hành vi

FE tạo hoa và regenerate gửi request ngắn để tạo job. API trả 202, worker xử lý qua RabbitMQ/MassTransit, FE polling trạng thái. Render không dùng RequestAborted của POST. Đóng/reload trang không hủy job.

Màn hình studio, session và lịch sử tài khoản có danh sách thiết kế gần đây. Mỗi thẻ có tên thiết kế, ảnh sản phẩm/mockup, loại thao tác, thời gian và trạng thái. Chọn thẻ để theo dõi/hủy hoặc mở kết quả. Danh sách gồm cả terminal jobs để tìm lại kết quả hoàn thành khi người dùng vắng mặt.

## API contract

Tất cả endpoint ở dưới prefix `/api/v1` tại BE, dùng CustomerPolicy và chỉ truy cập dữ liệu người dùng hiện tại.

| Method/path | Contract |
| --- | --- |
| POST /ai-flower-jobs | Body CreateFlowerRequest như API tạo hoa cũ; bắt buộc header Idempotency-Key; trả 202 + envelope.value JobResponse |
| POST /ai-flowers/{id}/regeneration-jobs | Body RegenerateFlowerRequest; cùng quy tắc key và response |
| GET /ai-flower-jobs/{id} | Envelope.value JobResponse; no-store; 404 nếu không tồn tại/không sở hữu |
| GET /ai-flower-jobs?active=true&pageIndex=1&pageSize=20 | Envelope pagination hiện có; active mặc định true; active=false trả mọi trạng thái; pageSize 1–50 |
| POST /ai-flower-jobs/{id}/cancel | Trả JobResponse; Queued hủy ngay; Running ghi yêu cầu hủy, worker đối chiếu; terminal không đổi |

JobResponse: `jobId`, `type`, `status`, `stage`, `displayName`, `productId`, `productImageUrl`, `mockupImageUrl`, `sourceFlowerId`, `createdAt`, `updatedAt`, `completedAt`, `cancelRequested`, `errorCode`, `result`.

- Type: Create, Regenerate.
- Status: Queued, Running, Succeeded, Failed, Canceled.
- Stage: Queued, GeneratingFlower, ApplyingMockup, Branding, Saving, Completed. Khi lỗi, giữ stage cuối để chẩn đoán.
- Result khi Succeeded có cùng shape GeneratedFlowerResponse cũ, gồm sessionId để mở thiết kế.
- Không trả snapshot prompt, checkpoint nội bộ hoặc thông báo exception của provider.
- Idempotency-Key: 1–128 ký tự ASCII chữ/số/dấu `-`/`_`; duy nhất theo user. Cùng key và cùng JSON input trả job cũ; đổi input với key cũ trả 409. Một lượt tạo mới có chủ đích dùng key mới.
- FE giữ key trong sessionStorage khi POST mất phản hồi; không tự retry POST bằng key mới. GET polling dùng request timeout thông thường.

## Độ bền và giới hạn

Bảng `FlowerGenerationJob` lưu snapshot sau validation, checkpoint, result, lease và revision. Job và message được lưu cùng transaction qua MassTransit bus outbox. Advisory lock PostgreSQL theo user bảo vệ quota và idempotency concurrent. Unique index giữ ràng buộc key ở database.

Consumer queue `flower-generation` không dùng middleware bao transaction/retry quanh toàn bộ render. Mỗi stage ghi checkpoint trong transaction ngắn; không giữ transaction qua lời gọi AI. Claim/lease và kiểm tra trạng thái chống duplicate delivery. Flower + history + Succeeded được commit nguyên tử.

Heartbeat mỗi 3 giây kiểm tra yêu cầu hủy và gia hạn lease; mất lease không được ghi kết quả. Recovery phát lại job Queued bị bỏ sót; job Running hết lease chuyển Failed với `FLOWER_JOB_WORKER_INTERRUPTED` (hoặc Canceled nếu đã yêu cầu hủy). Không tự gọi lại provider khi chưa rõ kết quả, tránh phí trùng. Checkpoint được giữ để chẩn đoán; phiên bản này chưa có API tiếp tục từ checkpoint.

Hủy kết nối tới provider không bảo đảm provider ngừng xử lý hoặc hoàn phí. Cancel và success được quyết định dưới row lock; job terminal không chuyển ngược trạng thái.

Mã lỗi worker: `FLOWER_JOB_INTERRUPTED`, `FLOWER_JOB_GENERATION_FAILED`, `FLOWER_JOB_SOURCE_UNAVAILABLE`, `FLOWER_JOB_WORKER_INTERRUPTED`. Quota trả `FLOWER_JOB_LIMIT`, tái sử dụng key sai input trả `FLOWER_JOB_KEY_REUSED`.

Job và checkpoint được giữ cùng lịch sử, chưa tự động purge. Không tự xóa object ảnh để tránh xóa nhầm output đang được tham chiếu; cần bổ sung chính sách retention/cleanup nếu lượng sử dụng tăng. Kết quả provider chưa nhận được trước khi process chết không thể checkpoint.

## Cấu hình và triển khai

Migration mới: `20260922093143_AddFlowerGenerationJobs`. API hiện tự chạy migrations lúc startup theo cơ chế sẵn có.

Cấu hình section `FlowerJobs` (environment dùng dấu `__`):

| Key | Mặc định | Giới hạn |
| --- | --- | --- |
| MaxActivePerUser | 3 | 1–10 |
| Concurrency | 2 | 1–16; trên mỗi consumer instance |
| BudgetSeconds | 600 | 30–1800 |
| LeaseSeconds | 60 | 30–300 |
| RecoveryIntervalSeconds | 15 | 5–60 |

Timeout mỗi lời gọi AI vẫn theo AiOption.TimeoutSeconds; branding vẫn theo FlowerImageOptions.TimeoutSeconds. Giới hạn các bước độc lập với timeout HTTP phía người dùng. Worker và recovery chạy cùng process API hiện tại. Nếu scale nhiều API replica, tổng concurrency tăng theo số replica; phải tính quota provider trước khi scale.

Thứ tự rollout:

1. Deploy BE có migration, worker và kết nối RabbitMQ hoạt động. Kiểm tra queue `flower-generation`, outbox, recovery và log theo jobId.
2. Kiểm tra POST trả 202 nhanh, GET chỉ thấy job của chủ sở hữu, và worker hoàn thành một job ở dev.
3. Deploy FE mới. Không cần đổi gateway route vì wildcard `/ecommerce-api/*` hiện tại bao phủ các endpoint mới.
4. Theo dõi Queued lâu, Failed, lease hết hạn và thời lượng từng stage.

API render đồng bộ cũ vẫn còn để tương thích giai đoạn rollout; FE mới không gọi chúng. Rollback FE không xóa job. Giữ worker phiên bản mới chạy/drain các job đã nhận; không downgrade bảng có dữ liệu đang xử lý.

## Bằng chứng kiểm thử local

- BE build thành công; migration snapshot khớp model, không có model drift.
- 55/55 test Flower thành công, gồm các test mới cho idempotency, quyền sở hữu, quota, snapshot, cancellation, checkpoint, duplicate delivery, recovery và regenerate lineage.
- Test HTTP dùng controller thật, JWT, PostgreSQL và production MassTransit/RabbitMQ registration. Giả lập mockup 130 giây: hai POST concurrent cùng key nhận cùng job trước 10 giây; polling nhận Succeeded sau render, chỉ một kết quả/AI call; cancel Running được xử lý. Toàn bộ suite mất khoảng 2 phút 20 giây. Không gọi AI tính phí.
- FE TypeScript, lint các file thay đổi và i18n lint qua tại thời điểm kiểm tra.
- Browser fixture: thẻ nhận diện, chọn tiến trình, POST có idempotency key, reload không POST lại, mở kết quả, cancel và viewport 390px không overflow; không có page error. Fixture dùng API giả lập và route tạm, đã gỡ. Middleware i18n local có redirect loop nên fixture bỏ qua middleware; đây không phải xác nhận full-site deployment.
- Production build FE đang bị chặn bởi trang có sẵn `app/dashboard/page.tsx` không có root layout; chưa thay đổi trang ngoài phạm vi này.
- Chưa test trên Cloudflare/deployment thật. Test 130 giây xác nhận kiến trúc dùng request ngắn ở local, không phải chứng nhận cấu hình server đã deploy.

Chạy integration test với PostgreSQL/RabbitMQ riêng trên localhost (RabbitMQ test port 5679):

```sh
FLOWER_JOBS_INTEGRATION=1 \
FLOWER_JOB_TEST_DELAY_SECONDS=130 \
POSTGEN_TEST_CONNECTION_STRING='Host=127.0.0.1;Port=55439;Database=postgres;Username=postgres;Password=<test-password>' \
dotnet test HoaTheoMua.Service.Test/HoaTheoMua.Service.Test.csproj --filter FullyQualifiedName~FlowerTest
```

Integration test tự tạo/xóa database có prefix `flower_jobs_`; không chạy với connection string của dữ liệu thật. Không bật biến FLOWER_JOBS_INTEGRATION thì integration test được đánh dấu skip.

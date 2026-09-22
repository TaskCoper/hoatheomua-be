# Kế hoạch chuyển AI Flower sang background job

Ngày lập: 2026-09-22. Trạng thái: đã triển khai luồng chính trong source local, chưa deploy. Contract, giới hạn và bằng chứng kiểm thử tại [Implementation](AI-Flower-Background-Jobs-Implementation.md).

## Bối cảnh và mục tiêu

Luồng `POST /api/v1/ai-flowers` và regenerate hiện chờ toàn bộ pipeline tạo hoa → ghép mockup → ghép logo/upload → lưu DB trong cùng HTTP request. RequestAborted được truyền xuống lời gọi AI. Log sự cố ghi nhận request bị hủy sau 125020 ms tại bước ghép mockup (`AiService/FlowerImageGeneration.cs:49`), cùng lỗi Cloudflare 524 phía client.

Source hiện đặt timeout FE và gateway 300 giây, nhưng không khắc phục giới hạn chờ của Cloudflare. Chưa xác minh cấu hình runtime trên server. Mỗi lời gọi AI mặc định được chờ 120 giây; branding có ngân sách mặc định 60 giây.

Mục tiêu: người dùng vẫn bấm tạo, chờ và nhận ảnh trên giao diện; tác vụ render chạy độc lập với kết nối HTTP, không bị hủy chỉ vì reload, đóng trang hoặc Cloudflare ngắt request.

Phạm vi: tạo hoa và regenerate. Tận dụng RabbitMQ + MassTransit hiện có; PostgreSQL lưu trạng thái bền vững. Chưa mở rộng sang PostGeneration.

## Luồng đề xuất

1. FE gửi yêu cầu với Idempotency-Key.
2. API xác thực, kiểm tra quyền và dữ liệu, chụp snapshot đầu vào/prompt; lưu job Queued và outbox trong cùng transaction.
3. API trả nhanh HTTP 202 cùng jobId và URL đọc trạng thái.
4. Outbox chuyển message vào RabbitMQ; worker nhận job và claim quyền xử lý.
5. Worker tạo hoa, ghép mockup, ghép logo/upload; lưu checkpoint sau mỗi bước.
6. Worker ghi flower, history và trạng thái Succeeded trong cùng transaction.
7. FE polling trạng thái bằng request ngắn và hiển thị kết quả khi hoàn thành.

## Mốc 1 — Contract và database

Thiết kế bảng FlowerGenerationJob với các nhóm dữ liệu:

- Job ID, user ID, loại Create/Regenerate, source flower ID nếu có.
- Snapshot input, dữ liệu tham chiếu và prompt đã kiểm tra.
- Status: Queued, Running, Succeeded, Failed, Canceled.
- Stage: tạo hoa, ghép mockup, branding/upload, persistence.
- Kết quả/checkpoint từng bước, provider request ID, generatedFlowerId.
- Mã lỗi và thông báo an toàn; chi tiết chẩn đoán chỉ dành cho log nội bộ.
- Idempotency key, input hash, attempt count, timestamps.
- Lease owner/expiry, concurrency token và cancel-request timestamp.

Ràng buộc idempotency theo người dùng và thao tác. Cùng key/cùng input trả lại job hiện có; cùng key/khác input trả conflict. Xác định thời gian lưu job, snapshot, checkpoint và dọn ảnh mồ côi trước khi triển khai.

API đề xuất, cần chốt contract chi tiết trước khi code:

| API | Chức năng |
| --- | --- |
| POST /v1/ai-flower-jobs | Tạo job Create, trả 202 + jobId |
| POST /v1/ai-flowers/{id}/regeneration-jobs | Tạo job Regenerate |
| GET /v1/ai-flower-jobs/{jobId} | Trạng thái, stage, lỗi an toàn hoặc kết quả |
| GET /v1/ai-flower-jobs?active=true | Khôi phục job đang chạy khi mở lại trang |
| POST /v1/ai-flower-jobs/{jobId}/cancel | Ghi nhận yêu cầu hủy |

Mọi API phải kiểm tra quyền sở hữu. GET trạng thái không được gọi lại provider hoặc tạo tác dụng phụ. Chuẩn hóa response theo envelope hiện có của dự án.

## Mốc 2 — Queue, worker và recovery

- Tách pipeline render khỏi HttpContext; worker dùng token của job và timeout từng bước/tổng budget, không dùng RequestAborted của POST.
- Kiểm tra và cấu hình transactional outbox với MassTransit: job và message phải được ghi nguyên tử.
- Claim job bằng concurrency control/lease; chống duplicate delivery và không tạo trùng flower/history.
- Lưu checkpoint sau từng bước; mockup thất bại vẫn giữ kết quả tạo hoa để có thể tiếp tục khi phù hợp.
- Giới hạn concurrency và số job đang chờ/chạy theo user; cấu hình timeout và giới hạn retry rõ ràng.
- Có heartbeat/lease renewal và recovery cho worker restart hoặc job treo. Worker cũ mất lease không được commit kết quả.
- Không mặc định retry lời gọi AI khi timeout/mất phản hồi: provider có thể đã thực hiện và tính phí. Nếu provider hỗ trợ idempotency/status lookup thì tận dụng; nếu không, ghi nhận kết quả không xác định và yêu cầu retry có chủ đích.
- Phân biệt retry an toàn cho DB/message với retry tạo ảnh. Không hứa exactly-once với provider nếu chưa có cơ chế hỗ trợ.
- Hủy là yêu cầu bất đồng bộ; kiểm tra ở các ranh giới bước và trong lời gọi hỗ trợ cancellation. Hủy HTTP tới provider không bảo đảm provider ngừng xử lý/tính phí.
- Quy định race giữa cancel và success bằng transition nguyên tử: job đã terminal không bị chuyển ngược trạng thái.
- Xác định chính sách nếu nguồn bị xóa/thay đổi trong lúc chờ: snapshot phục vụ tái lập, nhưng kiểm tra lại quyền và điều kiện cần thiết trước khi lưu kết quả.

## Mốc 3 — FE theo dõi job

- POST nhận jobId rồi polling khoảng 3 giây/lần, giãn nhịp khi chờ lâu; không giữ request render dài.
- Hiển thị stage thực từ BE: xếp hàng → tạo hoa → ghép mockup → ghép logo → hoàn thành. Không hiển thị phần trăm giả.
- Dừng polling khi job terminal hoặc unmount; mở lại trang thì khôi phục từ API job đang chạy.
- Ngăn gửi lặp, giữ cùng idempotency key khi gửi lại cùng yêu cầu do mất mạng. Một lần render mới do người dùng chủ động phải có key mới.
- Lỗi polling là mất kết nối tạm thời, không đồng nghĩa job Failed; tiếp tục đối chiếu trạng thái khi kết nối phục hồi.
- Đóng trang không hủy job. Chỉ gửi cancel khi người dùng chủ động yêu cầu.
- Khi thành công, cập nhật/invalidate gallery và lịch sử liên quan; giữ input và giải thích lỗi khi thất bại.

## Mốc 4 — Gateway, kiểm thử và rollout

Gateway chuyển tiếp request ngắn; không dùng tăng timeout làm giải pháp chính. Log theo jobId, stage, duration và provider request ID; không log secrets hoặc dữ liệu nhạy cảm. Phân loại cancellation riêng thay vì ghi tất cả thành Unhandled.

Theo dõi queue depth, thời gian chờ, thời lượng từng bước, tỷ lệ thất bại và số job hết lease.

### Tiêu chí nghiệm thu

| Tình huống | Kết quả cần đạt |
| --- | --- |
| Render vượt 125 giây qua Cloudflare | FE vẫn nhận được kết quả bằng polling, không phụ thuộc request dài |
| Reload/đóng rồi mở trang | Khôi phục đúng job và kết quả |
| POST mất response rồi gửi lại | Cùng idempotency key không tạo job mới |
| RabbitMQ giao message trùng | Không tạo trùng kết quả; claim bảo vệ xử lý |
| Broker tạm gián đoạn | Outbox giữ message và chuyển tiếp sau phục hồi |
| Worker restart giữa bước | Recovery có giới hạn, không treo mãi hoặc retry AI mù quáng |
| Mockup lỗi | Checkpoint tạo hoa đã hoàn thành vẫn tồn tại |
| Provider timeout không rõ kết quả | Ghi nhận đúng nguyên nhân, không tự gọi lặp vô hạn |
| User khác đọc/hủy job | Bị từ chối |
| Cancel sát lúc hoàn thành | Trạng thái terminal và dữ liệu nhất quán |
| Nhiều job đồng thời | Concurrency/rate limit hoạt động, không vượt giới hạn đã cấu hình |

Ưu tiên test với provider giả lập độ trễ/lỗi cho các nhánh lỗi; kiểm chứng một luồng tích hợp qua Cloudflare trong môi trường dev khi triển khai.

Rollout: migration + API/worker → kiểm thử → FE dùng API job mới → theo dõi → ngừng endpoint render đồng bộ. Giữ endpoint cũ trong giai đoạn chuyển đổi. Rollback FE không được xóa job đang chạy; worker cần tiếp tục xử lý hoặc drain có kiểm soát.

## Nguồn đối chiếu

- FE: `hoatheomua-fe/lib/axios.ts`, `features/studio/services.ts`, `app/api/[...path]/route.ts`.
- Gateway: `hoatheomua-gateway/.docker/compose.yaml`, `.docker/docker-compose.yml`.
- BE: `HoaTheoMua.API/Controllers/FlowerController.cs`, `HoaTheoMua.Service/Flower/Service.cs`, `Flower/Generation.cs`, `AiService/FlowerImageGeneration.cs`, `AiService/Service.cs`.
- Hạ tầng sẵn có: MassTransit/RabbitMQ trong `HoaTheoMua.Service/DependencyInjection/Extensions/ServiceCollectionExtensions.cs`; job đăng bài trong `HoaTheoMua.Service/PostPublishing`.
- Cloudflare 524: https://developers.cloudflare.com/support/troubleshooting/http-status-codes/cloudflare-5xx-errors/error-524/

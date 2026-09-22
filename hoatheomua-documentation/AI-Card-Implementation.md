# Tạo thiệp AI và background jobs

Ngày: 2026-09-22. Đã triển khai trong source local, chưa deploy hoặc chạy migration trên database triển khai.

## Luồng xử lý

API chuẩn bị và kiểm tra dữ liệu, lưu job cùng message qua MassTransit bus outbox rồi trả HTTP 202. Worker queue `card-generation` gọi AI client chung với luồng hoa: `AiOption`, Chat Completions, timeout, parser Markdown. Thiệp có phương thức `GenerateCardImage` riêng để cho phép lời chúc và tỷ lệ thiệp, không dùng policy ảnh sản phẩm cấm chữ hoặc policy hoa vuông 1:1.

Một lời gọi AI tạo ảnh thiệp hoàn chỉnh. System message giữ nguyên prompt, thêm policy ứng dụng. User message chứa lời chúc, hình thức chữ, chiều rộng/cao và các ảnh `image_url` được gắn nhãn: bó hoa, template, rawImage và ảnh đính kèm nếu có. Tỷ lệ yêu cầu lấy từ size, không cố định hình vuông. Backend từ chối kết quả thiếu ảnh, nhiều ảnh hoặc trả lại đúng URL tham chiếu. Chưa kiểm pixel, chính tả trong ảnh hoặc chất lượng thẩm mỹ bằng AI thật.

`RawImage` giữ URL nền được cung cấp khi Create; nếu không có, dùng cùng URL với ảnh kết quả. Regenerate không dùng ảnh thiệp cũ làm ảnh bó hoa. Phiên bản này không sinh riêng nền không chữ, không ghép chữ bằng code, không thêm bước mockup/logo của hoa cho thiệp.

## Hai nguồn bó hoa

- Bó thường: ảnh Product (`CoverImage` trong entity hiện tại).
- Bó AI: `ImageUrl` của GeneratedFlower khách chọn; không lấy bản gần nhất, không phân loại theo tăng hoa hay số lần regenerate.

Order đóng ảnh vào `snapShotCard.source.imageUrl`, thêm `source.imageSourceType = product | generated_flower`. `source.id` tiếp tục giữ Product gốc cho lineage. `productMetadata.generatedFlower` giữ ID kết quả hoa khách chọn.

Snapshot/History cũ chưa có `imageSourceType` và có GeneratedFlower liên kết: đọc đúng GeneratedFlower thuộc user một lần khi chuẩn bị thiệp, chụp ảnh vào job và History kết quả. Snapshot mới đã có marker thì giữ ảnh đã chốt, không đọc đè bằng dữ liệu live. Regenerate tiếp tục kế thừa ảnh từ History thiệp nguồn.

## Config system prompt

- Key: `imagegen.card_prompt`.
- Group: `imagegen`.
- Kind: `Setting` (Group/Kind được trim và so sánh không phân biệt hoa thường).
- Value là JSON object chứa chuỗi `value` không rỗng, ví dụ `{"value":"Nội dung system prompt tạo thiệp"}`.
- Không yêu cầu `IsPublic`, không seed hoặc ghi đè prompt đang có.

Order đọc config và đóng vào snapshot. Create dùng prompt snapshot đã thanh toán. Regenerate ưu tiên config hiện hành; chỉ fallback History.SystemForm khi không có config. Config tồn tại nhưng rỗng, sai Kind/Group, trùng key hoặc JSON sai trả `SYSTEM_PROMPT_INVALID`, không âm thầm fallback.

## API

Tất cả route dưới `/api/v1`, quyền Customer và kiểm owner.

| Method/path | Hành vi |
|---|---|
| POST `/ai-card-jobs` | Nhận Create hoặc Regenerate, header `Idempotency-Key` bắt buộc, trả 202 + `value` là JobResponse |
| POST `/ai-cards` | Alias của job submission, cùng contract 202 và header; không còn trả trực tiếp ảnh đồng bộ |
| GET `/ai-card-jobs/{id}` | Trạng thái, stage và kết quả; `Cache-Control: no-store` |
| GET `/ai-card-jobs` | Phân trang; `active=true` mặc định; có filter `orderDetailId`, `virtualId`; `active=false` lấy cả job terminal |
| POST `/ai-card-jobs/{id}/cancel` | Queued hủy ngay; Running ghi yêu cầu hủy; terminal giữ nguyên |
| PUT `/final-generated-card` | Chọn ID thuộc `idResult`, khóa dòng OrderDetails trong transaction ngắn |

Create body chỉ cần `orderDetailId`, `virtualId`. Không gửi `cardTemplateId` hoặc `messageContent`, kể cả null. Create có rawImage vẫn là `operation=create`; snapshot đã done không thể tạo lần đầu lần nữa.

Regenerate body có `sourceCardId`, tùy chọn `cardTemplateId`, `messageContent`. Không gửi `orderDetailId`, `virtualId`, kể cả null. Bỏ/null/blank lời chúc dùng nguồn. Template bỏ/null/trùng ID dùng snapshot; ID mới phải hợp lệ và active. UUID rỗng bị từ chối.

JobResponse: `jobId`, `type`, `status`, `stage`, `displayName`, `orderDetailId`, `virtualId`, `bouquetImageUrl`, `templateImageUrl`, `sourceCardId`, thời gian, `cancelRequested`, `errorCode`, `result`. `result` khi Succeeded có shape `CreateCardResponse` hiện có. Không trả prompt, checkpoint hoặc provider exception. Job chứa Order IDs để FE tìm lại tiến trình; response kết quả thiệp vẫn phẳng.

FE đã chuyển sang `/ai-card-jobs`. Client khác dùng `/ai-cards` phải chuyển sang đọc JobResponse và polling; triển khai BE và FE đồng bộ.

## Lưu trữ và đồng thời

Migration `20260922124238_AddCardGenerationJobs` tạo bảng job, JSON snapshot/checkpoint/result, lease/revision và các index. Unique `(UserId, IdempotencyKey)` chống submit trùng; partial unique `(OrderDetailId, VirtualId)` cho Create đang Queued/Running bảo vệ một lượt tạo đầu tiên. Khóa PostgreSQL theo user bảo vệ quota và cùng key. Cùng key + cùng request trả job cũ; thay request trả 409.

Worker dùng transaction ngắn để claim/checkpoint/commit. Không giữ transaction khi chờ AI. Lưu GeneratedCard, ClientHistory, append `idResult`, status snapshot và job Succeeded trong cùng transaction. Khóa và đọc lại OrderDetails trước khi cập nhật JSON, giữ kết quả và lựa chọn final của request khác. Regenerate không khóa theo sourceCardId; hai job hợp lệ cùng nguồn vẫn được chạy.

Heartbeat kiểm tra hủy và lease mỗi 3 giây. Job Running hết lease chuyển Failed thay vì tự gọi AI lại; job Queued bị bỏ sót được dispatch lại. Duplicate delivery không render lại job đã xử lý. Lỗi provider/timeout không tự retry; người dùng có thể chủ động tạo job mới với key mới. Hủy kết nối không bảo đảm provider dừng xử lý hoặc hoàn phí. Ảnh provider có thể tồn tại nếu hủy hoặc commit lỗi; chưa tích hợp dọn ảnh mồ côi khi hủy hoặc commit lỗi.

Các state: Queued, Running, Succeeded, Failed, Canceled. Stage: Queued, GeneratingCard, Saving, Completed. Giới hạn mặc định dưới section `CardJobs`: MaxActivePerUser=3, Concurrency=2, BudgetSeconds=240, LeaseSeconds=60, RecoveryIntervalSeconds=15. Timeout mỗi lời gọi theo `AiOption.TimeoutSeconds` như hoa. Recovery và worker chạy trong host API cùng RabbitMQ hiện có.

## Giao diện

Studio thiệp gửi request ngắn, dùng idempotency key, polling và tìm lại job theo dòng Order/virtualId khi mở lại. Key của POST mất phản hồi được giữ trong sessionStorage để lần gửi lại không tạo lượt mới. Tiến trình hiển thị stage từ backend, có hủy và thông báo mất kết nối/lỗi. Đóng màn hình không hủy job. Sau thành công, tải lại danh sách phiên bản. Ảnh thiệp AI đã có chữ nên không đè thêm lời chúc lên ảnh kết quả.

## Kiểm chứng local

- 93 regression tests pass cho Card, Flower, AI image client và CreateOrder; 3 integration tests opt-in được skip trong lượt regression.
- Test tích hợp Card được bật riêng: HTTP/JWT thật, PostgreSQL 16 tạm, RabbitMQ 4, production MassTransit outbox/consumer registration. Pass submit concurrent cùng key, owner checks, polling tới thành công, duplicate delivery, concurrent regenerate không mất ID, giữ final selection, hủy Running. Kiểm model không lệch migration.
- Test AI dùng HttpMessageHandler giả để kiểm exact prompt, JSON, ảnh có nhãn, tỷ lệ, lỗi output, cancellation và không retry. Không dùng kết quả này để kết luận chất lượng ảnh AI thật.
- TypeScript, lint các file thay đổi và i18n lint pass. Browser fixture dùng hook và component thật với API giả lập: reload không POST lại, theo dõi job, nhận Succeeded/Canceled, màn hình 390px không overflow, không console error. Fixture đã được gỡ.
- Production build FE bị chặn bởi lỗi có sẵn `app/dashboard/page.tsx` thiếu root layout. Chưa gọi AI tính phí hoặc kiểm toàn bộ journey trên deployment thật.

Chạy integration test bằng PostgreSQL/RabbitMQ riêng ở localhost (RabbitMQ cổng 5679):

```sh
CARD_JOBS_INTEGRATION=1 \
POSTGEN_TEST_CONNECTION_STRING='Host=127.0.0.1;Port=55440;Database=postgres;Username=postgres;Password=<test-password>' \
dotnet test HoaTheoMua.Service.Test/HoaTheoMua.Service.Test.csproj --filter FullyQualifiedName~CardTest
```

Test chỉ chấp nhận host localhost/127.0.0.1, tạo/xóa database riêng prefix `card_jobs_`. Có thể đặt `CARD_JOB_TEST_DELAY_SECONDS` để kéo dài provider giả lập. Trước deploy cần config prompt hợp lệ, AiOption, RabbitMQ và migration bảng job. Cơ chế API startup hiện có tự chạy migration.

## Logo thiệp AI

Cả Create, Regenerate và caller đồng bộ đều chèn `public/logo.png` sau khi AI render, dùng chung transport và storage session với Hoa AI. `ImageUrl` là PNG đã gắn logo trên storage; `RawImage` giữ ảnh đầu vào nếu có, nếu không giữ URL render chưa gắn logo. Không ghi ảnh branded vào nền tái sử dụng. Thiệp handmade không đổi.

Logo ở góc dưới phải, giữ tỉ lệ logo và kích thước canvas, không crop ảnh. Mặc định cả chiều rộng và chiều cao logo bị giới hạn trong 16% cạnh ngắn của ảnh thực tế sau AutoOrient; lề bằng 3% cạnh ngắn. Quy tắc áp dụng cho ngang, dọc, vuông và tỉ lệ dài. Prompt yêu cầu chừa vùng vuông 22% cạnh ngắn ở góc dưới phải, không đặt lời chúc/chủ thể quan trọng vào đó; đây là chỉ dẫn cho AI, chưa có nhận diện chữ để bảo đảm vùng trống.

Section `CardImage` kế thừa cấu hình ảnh từ `FlowerImage`, mặc định riêng `ObjectPrefix=general/ai-cards`, `LogoWidthRatio=0.16`; có thể override `LogoPath`, `MarginRatio`, `PublicBaseUrl` và giới hạn dung lượng/timeout. Xác thực storage vẫn dùng session chung từ FlowerImage/TASKCOPER. Nếu tăng kích thước logo hoặc lề cần đồng bộ vùng trống trong prompt.

Job có stage `Branding` giữa `GeneratingCard` và `Saving`. Checkpoint lưu ảnh provider trước download/composite/upload, sau đó lưu thông tin branding (object key, SHA-256 logo, kích thước, tọa độ). History lưu cả generation và branding. Chỉ publish kết quả sau khi chèn/upload thành công; lỗi branding không commit thiệp hoặc tự gọi AI lại. Không cần migration.

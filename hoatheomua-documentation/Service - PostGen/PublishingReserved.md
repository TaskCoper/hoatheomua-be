# PostGen — Kế hoạch đăng bài và lập lịch

> **Phạm vi chốt 2026-09-15:** chỉ triển khai cấu hình tài khoản, đăng tải và lập lịch Facebook/Instagram trong đợt này. **13C Zalo tạm hoãn**; chưa cần tài khoản/app/OA hay smoke test Zalo. Giữ thiết kế Zalo bên dưới cho lần tiếp tục; không coi Zalo là điều kiện nghiệm thu mốc 11–15 đợt này.

Chưa có API đăng bài, đặt lịch, OAuth account hoặc background job trong mốc 0–7.

**Cập nhật 2026-09-14:** kế hoạch tiếp nối nằm tại [Plan.md — mốc 8–15](Plan.md#kế-hoạch-tiếp-nối--mốc-815). Đã chốt backend gọi trực tiếp API chính thức Facebook/Instagram/Zalo. Mốc 11 đã triển khai local ngày 2026-09-15 ([PlatformAccounts.md](PlatformAccounts.md)); mốc 12 đã có [tác vụ xuất bản](Publications.md) và kiểm chứng local; adapter thật đã có ở mốc 13, lịch đã có ở mốc 14. Chi tiết đề xuất cũ bên dưới được thay thế bởi Publications.md ở phần mốc 12.

| Mốc | Phạm vi |
| --- | --- |
| 8 | Chốt contract, loại bài, quyền, formatter và API cho FE |
| 9 | **Tạm hoãn** — chuẩn bị bật API key AI bằng cấu hình sẵn có |
| 10 | **Tạm hoãn** — lưu ảnh bền vững, đo metadata thực và quản lý asset |
| 11 | **Đã triển khai local** — tài khoản, OAuth, mã hóa/refresh token và ngắt kết nối local |
| 12 | **Đã triển khai local** — job/snapshot, idempotency, outbox/worker và đối soát; xem Publications.md |
| 13 | Adapter Facebook, Instagram theo contract từng kênh; **13C Zalo tạm hoãn** |
| 14 | Lập lịch, đổi/hủy lịch, phục hồi sau restart |
| 15 | Nghiệm thu tích hợp, tài liệu FE và rollout theo kênh |

**Thứ tự hiện tại:** mốc 8 đã chốt backend gọi API chính thức trực tiếp; mốc 11 đã triển khai, mốc 12 đã triển khai local, tiếp tục **13 → 14 → 15**. Người dùng yêu cầu tạm bỏ qua mốc 9 và 10, giữ lại thiết kế để làm sau. Giữ cách gọi AI hiện tại; publishing dùng version ID/URL/thứ tự ảnh đã chốt, chưa yêu cầu kho ảnh, import hoặc bảng asset. URL hết hạn/không truy cập được hoặc media bị provider từ chối phải hiện lỗi, không tự sinh lại ảnh. Snapshot giữ nguyên tham chiếu URL nhưng chưa bảo đảm file bên ngoài bất biến hoặc tồn tại lâu dài.

## Các phần đã dành sẵn

`PlatformPost` có `PlatformAccountId`, `ScheduledAt`, `PublishedAt`, `ExternalPostId`, `PublishError`, `PublishAttemptCount`. Mốc 11 đã thêm bảng PlatformAccount và FK Restrict từ PlatformAccountId; API publish mốc 12 nhận accountId và gắn account khi tạo job. Index `(Status, ScheduledAt)` chỉ chứa trạng thái Scheduled. Enum có `Scheduled`, `Publishing`, `Published`, `PublishFailed`; job mốc 12 cập nhật Publishing/Published/PublishFailed; Scheduled chờ mốc 14.

Route dự kiến: `POST .../platforms/{platform}/publish`, `POST/PUT/DELETE .../platforms/{platform}/schedule`, `GET .../platforms/{platform}/publications`, `GET /api/v1/post-gen/publications/{id}`. Nhóm `/api/v1/post-gen/platform-accounts` đã có API riêng tại [PlatformAccounts.md](PlatformAccounts.md), gồm list/connect/callback/reconnect/refresh/disconnect; publish/publications đã triển khai ở mốc 12; schedule vẫn chờ mốc 14.

## Thông tin cần lấy trước khi implement

- Contract và quyền đăng Facebook Page, Instagram chuyên nghiệp, Zalo OA; loại account hỗ trợ, quyền ứng dụng, token/refresh và API version.
- Giới hạn nội dung, hashtag, ảnh và tỷ lệ tại thời điểm tích hợp. Các con số seed chỉ là tham khảo từ kế hoạch, chưa dùng để validate việc đăng.
- Cách ánh xạ title/content/hashtag theo loại bài và contract từng provider; không mặc định mọi kênh đều ghép title vào content.
- Timezone cho lịch, xử lý giờ địa phương, retry/backoff, idempotency, reconciliation khi mất phản hồi provider.
- Độ bền URL ảnh chưa được xác nhận. `GeneratedPostImage.ImageExpiresAt` và `StorageProvider` đã dành sẵn, mốc 10 đang tạm hoãn. Thiết kế asset/migration cho object key, checksum và kích thước thực chỉ tiếp tục khi có yêu cầu; không chặn mốc 11–15 bằng yêu cầu phải có kho ảnh riêng.
- Hạ tầng queue/scheduler, guard để không đăng trùng, nơi lưu token và quyền quản lý account.

Mỗi lần đăng phải do hành động đăng ngay hoặc đặt lịch rõ ràng của người dùng.

Đề xuất các thao tác xuất bản và quản lý account mới chỉ dành cho Admin. **Đã chốt: backend gọi trực tiếp API chính thức của Facebook, Instagram và Zalo**, qua adapter riêng cho từng nền tảng. Mốc 8 đối chiếu tài liệu chính thức, loại bài và quyền ứng dụng để hoàn thiện contract. Không thêm lại `PlatformStandard` hoặc `postgen.platform.*` để phục vụ publishing.

Mốc 7 đã có GET official chỉ trả payload đang chốt và readiness nội bộ; không phải API publish, không gọi mạng xã hội hoặc chuyển trạng thái trong database. Title/content/hashtag giữ riêng. Xem [GetOfficialVersion.md](GetOfficialVersion.md).

Job đăng phải lưu bản sao cố định của text đã định dạng và danh sách media/order, không chỉ giữ `OfficialGenerationId`: bản Manual hiện tại có thể sửa tại chỗ. Publish/schedule không gọi AI. Dùng lại MassTransit/outbox hiện có, nhưng không coi dedup của queue là bảo đảm không đăng trùng bên ngoài; timeout sau khi gửi cần đối soát trước khi retry. Xem tiêu chí transaction, snapshot, retry và lịch ở mốc 12–14 trong Plan.


## Cập nhật mốc 13 — 2026-09-15

Đã triển khai adapter Facebook/Instagram gọi Graph API, giữ API mốc 12. Kiểm chứng local qua fake HTTP Meta/media, PostgreSQL/RabbitMQ thật; HTTP 49 request / 157 checks PASS. Chưa deploy/smoke account thật; flag mặc định false. Contract hiện hành và giới hạn media/đối soát: [PublishingAdapters.md](PublishingAdapters.md); [FE handoff](../FE-API-Handoff-PostGen-Milestone13.md). Không thêm migration; checkpoint JSON có preparedAt và metadata/ready của media.


## Cập nhật mốc 14 — 2026-09-15

Đã triển khai đặt/đổi/hủy lịch và phục hồi đến hạn theo [Scheduling.md](Scheduling.md). Các phần thiết kế dự phòng lịch cũ được thay bằng contract này. Chưa deploy/smoke Meta thật.

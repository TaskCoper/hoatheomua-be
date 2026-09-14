# FE handoff — PostGen mốc 13

Ngày 2026-09-15. Backend đã có adapter Facebook/Instagram, kiểm chứng local. **Chưa deploy hoặc smoke trên account thật.** Hai flag publishing mặc định false.

## API

Giữ nguyên tám endpoint/DTO của [mốc 12](FE-API-Handoff-PostGen-Milestone12.md). Không thêm endpoint đăng trực tiếp, không đổi generate thành publish.

Luồng FE: lấy publication-preview với account → hiển thị `snapshot.text` và ảnh đúng order → Admin xác nhận → POST publish với accountId/idempotencyKey/expectedRevision → nhận 202 và theo dõi job/detail. HTTP retry dùng lại cùng key và request. Revision conflict phải lấy preview mới.

`canPublish=true` là đủ điều kiện tạo job. `mediaVerification=Unverified` nghĩa là file/quyền/quota còn được kiểm khi worker chạy; không hiển thị như đã xác minh khả năng đăng thành công.

## Phạm vi ảnh

- Facebook Page: 1–10 ảnh; JPEG/PNG/GIF/BMP/TIFF, tối đa 10 MiB/ảnh.
- Instagram professional liên kết Page: JPEG tối đa 8 MiB, width 320–1440, ratio 4:5–1,91:1. Một ảnh có thể portrait/landscape; carousel 2–10 ảnh chỉ nhận ảnh vuông trong đợt này.
- Caption Instagram là text sau ghép title/content/hashtag: tối đa 2.200 Unicode scalar, 30 hashtag, 20 mention. FE hiển thị lỗi vượt giới hạn; không tự cắt text hoặc bỏ ảnh.
- Worker kiểm file thực, không tin kích thước mục tiêu trong dữ liệu sinh ảnh. URL redirect/hỏng/hết hạn, PNG cho Instagram hoặc tỷ lệ sai sẽ bị từ chối. Không có bước tự chuyển định dạng/crop/upload kho ảnh.

## Hiển thị kết quả và xử lý lỗi

| Job/lỗi | FE xử lý |
| --- | --- |
| `Succeeded` | Hiển thị externalPostId và link nếu có; externalPostUrl có thể null khi đọc permalink lỗi |
| `Failed`, mã ảnh có hậu tố `_N` | N là order ảnh; hướng dẫn sửa/chọn ảnh, lấy preview mới rồi tạo job mới. Snapshot job cũ giữ nguyên |
| `META_TOKEN_INVALID` / `META_PERMISSION_DENIED` | Hiển thị cần kết nối lại tài khoản; chỉ bật Retry khi API trả canRetry=true |
| `META_RATE_LIMITED` | Hạn mức provider; Retry quá sớm trả 429. Không vòng lặp retry liên tục |
| `PUBLICATION_MEDIA_EXPIRED` | Retry nếu canRetry=true; backend chuẩn bị lại media với snapshot cũ |
| `NeedsReview` | Không bật đăng lại/Retry. Cho Recheck hoặc review theo mốc 12; có thể Close với ghi chú |
| `PUBLISHING_DISABLED` | Kênh chưa được bật trong môi trường này |

Facebook candidate ID phải qua kiểm chứng Page/text/photo IDs đã lưu. Instagram `INSTAGRAM_PUBLISHED_ID_UNRESOLVED` nghĩa là đã có dấu hiệu đăng nhưng thiếu media ID; nhập candidate ID chưa đủ để backend xác nhận. Không thông báo “chưa đăng” chỉ vì recheck chưa tìm được kết quả. Close không xóa bài ngoài nền tảng và không mở quyền đăng lại cùng PlatformPost.

Contract chi tiết/mã lỗi: [PublishingAdapters.md](Service%20-%20PostGen/PublishingAdapters.md). Evidence: [HTTP 49 request / 157 checks](Service%20-%20PostGen/PublicationVerification/results/api-integration-tester.json), dùng phản hồi Meta giả lập.

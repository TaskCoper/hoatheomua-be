# PostGen — Phần đăng bài dành cho phase sau

Chưa có API đăng bài, đặt lịch, OAuth account hoặc background job trong mốc 0–2.

## Các phần đã dành sẵn

`PlatformPost` có `PlatformAccountId`, `ScheduledAt`, `PublishedAt`, `ExternalPostId`, `PublishError`, `PublishAttemptCount`. Chưa có FK từ `PlatformAccountId`; bảng account chưa tồn tại. Index `(Status, ScheduledAt)` chỉ chứa trạng thái Scheduled. Enum có `Scheduled`, `Publishing`, `Published`, `PublishFailed`; các endpoint hiện tại không chuyển vào các trạng thái này.

Route dự kiến: `POST .../platforms/{platform}/publish`, `POST .../platforms/{platform}/schedule`, `DELETE .../platforms/{platform}/schedule`, `GET /api/v1/post-gen/platform-accounts`.

## Thông tin cần lấy trước khi implement

- Contract và quyền đăng Facebook Page, Instagram chuyên nghiệp, Zalo OA; loại account hỗ trợ, quyền ứng dụng, token/refresh và API version.
- Giới hạn nội dung, hashtag, ảnh và tỷ lệ tại thời điểm tích hợp. Các con số seed chỉ là tham khảo từ kế hoạch, chưa dùng để validate việc đăng.
- Quy tắc ghép title vào đầu content, số dòng trống và vị trí hashtag; chưa có quyết định cuối.
- Timezone cho lịch, xử lý giờ địa phương, retry/backoff, idempotency, reconciliation khi mất phản hồi provider.
- Độ bền URL ảnh. `GeneratedPostImage.ImageExpiresAt` và `StorageProvider` dành cho việc quản lý link hết hạn hoặc chuyển sang kho ảnh riêng.
- Hạ tầng queue/scheduler, guard để không đăng trùng, nơi lưu token và quyền quản lý account.

Mỗi lần đăng phải do hành động đăng ngay hoặc đặt lịch rõ ràng của người dùng.

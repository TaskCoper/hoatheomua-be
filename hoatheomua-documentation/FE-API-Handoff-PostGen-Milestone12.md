# PostGen — Bàn giao FE mốc 12

Backend đã có tác vụ xuất bản và snapshot; **adapter Meta thật chờ mốc 13**, publishing mặc định tắt. Contract đầy đủ: [Publications.md](Service%20-%20PostGen/Publications.md).

## Luồng màn hình

1. Admin chọn tài khoản đích, gọi GET `.../platforms/{platform}/publication-preview?accountId=...`.
2. Hiển thị snapshot text và ảnh theo order. `canPublish=false`: hiển thị reasons. `mediaVerification=Unverified` nghĩa file/điều kiện provider chưa được kiểm trực tiếp.
3. Khi Admin bấm đăng, tạo một idempotency key và POST `.../publish` với accountId + expectedRevision chuỗi từ preview. Nếu retry HTTP, dùng lại key và body cũ.
4. HTTP 202 trả `publicationId`, chưa phải đăng thành công. Poll GET `/api/v1/post-gen/publications/{id}` có backoff và dừng khi rời màn hình.
5. Chi tiết job chứa status/revision/snapshot/attempts/reviews. Lịch sử phân trang tại GET `.../platforms/{platform}/publications`.

## Trạng thái và nút thao tác

| Trạng thái | Hiển thị / thao tác |
| --- | --- |
| Queued | Đang chờ; có nút Hủy trước khi chạy |
| Publishing | Đang xử lý; khóa sửa bản chốt và hủy |
| Succeeded | Thành công; mở externalPostUrl nếu có, không có nút đăng lại |
| Failed | Hiển thị errorCode; Thử lại chỉ khi canRetry=true, nhắc rằng giữ snapshot cũ |
| NeedsReview | Cần kiểm tra kết quả; Kiểm tra lại / Nhập ID bài để xác minh / Đóng xử lý |
| Cancelled | Đã hủy trước khi chạy; có thể chỉnh/chốt và tạo yêu cầu mới |
| Closed | Đã đóng xử lý, kết quả bên ngoài vẫn chưa rõ; không bật đăng lại |

Retry/cancel/recheck/resolve dùng expectedRevision **số của job**, không dùng revision chuỗi của preview. HTTP 409 → tải lại; không tự lặp thao tác ghi hoặc đổi key để vượt conflict.

Resolve gửi action `ConfirmPublished`/`ConfirmNotPublished`/`Close`, note bắt buộc; ConfirmPublished cần externalPostId. Xác nhận của Admin vẫn được backend đối chiếu bằng adapter; response có thể tiếp tục NeedsReview. Không báo thành công chỉ vì request resolve trả 200.

## Khóa chỉnh sửa

Khi job Queued/Publishing/Scheduled/NeedsReview: khóa sửa/chốt nội dung và ảnh của nền tảng liên quan; khóa sửa/xóa bài gốc. Vẫn xem lịch sử hoặc thêm version ảnh vào kho, không tự chọn nó cho bài đang đăng. Nền tảng khác không bị khóa nếu không có job riêng. Backend kiểm lại để xử lý nhiều tab; nút disable trên FE không thay thế guard.

Mốc 12 có hủy job Queued; đặt lịch/đổi lịch/hủy lịch tương lai là mốc 14. Không hiển thị Scheduled như một tính năng đã dùng được.

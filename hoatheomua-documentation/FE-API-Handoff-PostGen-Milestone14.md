# FE handoff — Lịch đăng PostGen mốc 14

Đã triển khai local ngày 2026-09-15; chưa deploy/đăng thử Meta thật. Bổ sung ba API Admin và field `scheduledAt` nullable trong mọi response job.

## Luồng UI

1. Chọn tài khoản, đọc `publication-preview`, hiển thị đúng text/ảnh chốt.
2. Admin chọn ngày/giờ; UI mặc định hiển thị Asia/Ho_Chi_Minh. Chuyển giờ lựa chọn thành ISO có `+07:00` hoặc `Z`; không gửi `datetime-local` thiếu offset trực tiếp.
3. Gửi POST schedule với accountId, idempotencyKey, hash expectedRevision của preview và scheduledAt. Backend trả 202, publicationId, status Scheduled và scheduledAt UTC.
4. Hiển thị lịch từ job, đổi về múi giờ UI. Đọc list/detail publications để theo dõi. Không coi 202 là đã đăng.
5. Đổi/hủy gửi publicationId cùng **revision số của job**. Chỉ hiển thị khi Scheduled; backend vẫn quyết định nếu worker vừa claim.

## Request

Prefix `/api/v1/posts/{id}/platforms/{platform}`.

| Thao tác | API | Thành công |
| --- | --- | --- |
| Đặt lịch | POST `.../schedule` | 202 + job |
| Đổi giờ | PUT `.../schedule` | 200 + job/revision mới |
| Hủy lịch | DELETE `.../schedule?publicationId=...&expectedRevision=...` | 200 + Cancelled |

POST body:

```json
{
  "accountId": "<uuid>",
  "idempotencyKey": "<unique key cho lần xác nhận>",
  "expectedRevision": "<hash từ preview>",
  "scheduledAt": "2026-09-20T09:00:00+07:00"
}
```

PUT body:

```json
{
  "publicationId": "<uuid>",
  "expectedRevision": 2,
  "scheduledAt": "2026-09-20T10:00:00+07:00"
}
```

DELETE dùng query, không body. Khi retry POST vì mất mạng, giữ nguyên key và payload. Đổi/hủy gặp 409 thì tải lại job; không tăng revision phía client hoặc tự lặp lệnh.

## Quy tắc cần thể hiện

- Mặc định đặt trước ít nhất 60 giây, tối đa 30 ngày. Chọn dư thời gian gửi request; backend có thể cấu hình ngưỡng khác và trả lỗi nêu giới hạn.
- Đổi giờ không đổi text/ảnh/account. Muốn sửa nội dung: hủy lịch, chỉnh/chốt, preview lại rồi tạo lịch mới.
- Đến hạn hệ thống mới bắt đầu kiểm token/media và tạo container; có thể có độ trễ xử lý Meta. Preview vẫn `mediaVerification=Unverified` theo mốc 13.
- Job Scheduled có thể đổi/hủy trước claim. Publishing không được đổi/hủy lịch, kể cả chưa thấy bài ngoài Meta.
- Trễ hơn 15 phút mà chưa bắt đầu: Failed + `PUBLICATION_SCHEDULE_MISSED`, không tự đăng bù; hướng dẫn tạo lịch/yêu cầu đăng mới. Retry tác vụ Failed/canRetry khác là **chạy lại ngay**, không lập lịch mới.
- `scheduledAt` trong job đã Cancelled/Succeeded/Failed là giờ lịch sử, không có nghĩa còn lịch chờ chạy.
- NeedsReview giữ thao tác đối soát mốc 12–13; không cho đăng lại mù.

Chi tiết lỗi, trạng thái, cấu hình và migration: [Scheduling.md](Service%20-%20PostGen/Scheduling.md). Account, snapshot, idempotency và lỗi media tiếp tục theo bàn giao mốc 11–13.

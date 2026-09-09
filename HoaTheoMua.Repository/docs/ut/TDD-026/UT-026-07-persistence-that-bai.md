# UT-026-07

## Thông tin tài liệu

- **Tiêu đề:** Rollback khi lưu trạng thái xóa template thất bại
- **Ghi chú:** Kiểm tra lỗi database trong lúc lưu isDeleted và updatedAt.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-026-07
- **Phiên bản:** 0.2
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** ADMIN - Card Template
- **Unit under test:** `CardTemplateService.DeleteAsync`
- **Loại:** Negative / Transaction
- **Mục tiêu:** Không để entity trong trạng thái đã xóa một phần khi persistence không thành công.
- **Precondition / Mock setup**:
  - Người dùng đã xác thực với role Admin.
  - Template tồn tại với isDeleted = false và updatedAt = old-time.
  - Repository save hoặc transaction commit được mock ném lỗi.
- **Dữ liệu đại diện**:
  - Lỗi save là dữ liệu chính; lỗi commit đại diện cùng kết quả persistence không hoàn tất.
  - Mỗi điểm lỗi được chạy riêng để xác nhận rollback cả cờ isDeleted lẫn updatedAt.
- **Input**:
  - Admin gọi DELETE template hợp lệ.
- **Các bước thực hiện**:
  - Cho service tải template và chuẩn bị thay đổi trạng thái.
  - Làm save/commit thất bại.
  - Đọc lại trạng thái bền vững sau rollback.
- **Expected output (bắt buộc)**:
  - HTTP 500 với mã INTERNAL_SERVER_ERROR.
  - Trong database, isDeleted vẫn false và updatedAt vẫn là old-time; không có thay đổi một phần.
  - Không gọi File Storage, không xóa ảnh và không thay đổi GeneratedCard/History.
  - Lỗi kỹ thuật gốc được log nhưng không lộ chi tiết nội bộ trong response.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Client chỉ được nhận thành công khi trạng thái xóa đã được lưu bền vững; rollback bảo toàn tính nhất quán khi database lỗi.

## TEST_LINKS

- [TDD-026 - Xóa Card Template](../../tdd/TDD-026/TDD-026-xoa-card-template.md)
- BR-022-10

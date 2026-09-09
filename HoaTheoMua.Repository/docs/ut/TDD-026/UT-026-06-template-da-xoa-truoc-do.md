# UT-026-06

## Thông tin tài liệu

- **Tiêu đề:** Từ chối Card Template đã được xóa trước đó
- **Ghi chú:** Delete lần hai không được coi là một lần cập nhật thành công mới.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-026-06
- **Phiên bản:** 0.2
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** ADMIN - Card Template
- **Unit under test:** `CardTemplateService.DeleteAsync`
- **Loại:** Negative / Idempotency
- **Mục tiêu:** Báo rõ trạng thái Gone và không thay đổi dấu thời gian của lần xóa đầu.
- **Precondition / Mock setup**:
  - Người dùng đã xác thực với role Admin.
  - Template tồn tại với isDeleted = true và một updatedAt đã biết.
- **Dữ liệu đại diện**:
  - Template đã xóa nhưng active là dữ liệu chính; template đã xóa và inactive đại diện cùng nhánh vì isDeleted được kiểm tra độc lập với isActive.
  - Hai biến thể đều không được save lần nữa.
- **Input**:
  - Admin gọi DELETE ID của template đã xóa.
- **Các bước thực hiện**:
  - Cho service tải template.
  - Phát hiện isDeleted đã là true.
  - Kiểm tra response, save count và updatedAt.
- **Expected output (bắt buộc)**:
  - HTTP 410 với mã CARD_TEMPLATE_ALREADY_DELETED.
  - Không gọi save; isDeleted vẫn true và updatedAt giữ nguyên giá trị của lần xóa trước.
  - Không gọi File Storage và không ảnh hưởng GeneratedCard/History.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Không cập nhật lại updatedAt giúp giữ chính xác thời điểm thay đổi cuối và tránh che khuất lần xóa ban đầu.

## TEST_LINKS

- [TDD-026 - Xóa Card Template](../../tdd/TDD-026/TDD-026-xoa-card-template.md)
- BR-022-03, BR-022-04


# UT-026-01

## Thông tin tài liệu

- **Tiêu đề:** Xóa mềm Card Template thành công
- **Ghi chú:** Kiểm tra Admin xóa một template bằng cách đổi cờ trạng thái, không xóa bản ghi hay file ảnh.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-026-01
- **Phiên bản:** 0.2
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** ADMIN - Card Template
- **Unit under test:** `CardTemplateService.DeleteAsync`
- **Loại:** Positive
- **Mục tiêu:** Xác nhận thao tác delete chỉ đánh dấu template đã xóa và giữ nguyên toàn bộ dữ liệu đã tham chiếu.
- **Precondition / Mock setup**:
  - Người dùng admin-01 đã xác thực với role Admin.
  - Template template-01 tồn tại, isDeleted = false, có image URL và đã được một GeneratedCard/History cũ tham chiếu.
  - Repository save thành công; đồng hồ hệ thống được cố định để kiểm tra updatedAt.
- **Dữ liệu đại diện**:
  - Template active là dữ liệu chính; template inactive nhưng chưa xóa đại diện cùng nhánh vì delete không phụ thuộc isActive.
  - Template loại ai và handmade đều là biến thể đại diện: loại template không thay đổi cơ chế xóa mềm.
  - Các biến thể cần chạy riêng để chứng minh trạng thái/loại không làm mất dữ liệu liên quan.
- **Input**:
  - Gọi DELETE với id = template-01 và danh tính admin-01.
- **Các bước thực hiện**:
  - Xác thực và kiểm tra role Admin.
  - Tải template theo ID, kiểm tra chưa bị xóa rồi thực hiện save.
  - Đọc lại template và các bản ghi liên quan sau thao tác.
- **Expected output (bắt buộc)**:
  - HTTP 200 và response xác nhận xóa thành công.
  - isDeleted đổi từ false thành true; updatedAt được cập nhật bằng thời gian UTC hiện tại.
  - Template vẫn còn trong database; image URL, loại template và các trường nội dung khác không bị sửa.
  - GeneratedCard và History cũ vẫn tồn tại, vẫn đọc được snapshot; không gọi File Storage để xóa ảnh.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Xóa mềm phải ngăn dùng template cho dữ liệu mới nhưng vẫn bảo toàn lịch sử, khả năng audit và ảnh mà dữ liệu cũ đang tham chiếu.

## TEST_LINKS

- [TDD-026 - Xóa Card Template](../../tdd/TDD-026/TDD-026-xoa-card-template.md)
- BR-022-01, BR-022-02, BR-022-05 đến BR-022-07

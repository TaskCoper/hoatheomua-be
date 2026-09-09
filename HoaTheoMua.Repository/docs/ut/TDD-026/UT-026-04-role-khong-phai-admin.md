# UT-026-04

## Thông tin tài liệu

- **Tiêu đề:** Từ chối người dùng không có role Admin
- **Ghi chú:** Người dùng đã đăng nhập nhưng không có quyền quản trị template.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-026-04
- **Phiên bản:** 0.2
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** ADMIN - Card Template
- **Unit under test:** `CardTemplateService.DeleteAsync`
- **Loại:** Security / Authorization
- **Mục tiêu:** Chỉ role Admin được phép thực hiện xóa mềm Card Template.
- **Precondition / Mock setup**:
  - Người dùng customer-01 đã xác thực với role Customer.
  - Template template-01 tồn tại nhưng không được truy vấn trong nhánh bị cấm.
- **Dữ liệu đại diện**:
  - Role Customer là dữ liệu chính; mọi role đã xác thực nhưng không phải Admin đại diện cùng nhánh forbidden.
  - Trạng thái hoặc loại template không liên quan vì quyền phải được kiểm tra trước khi tải tài nguyên.
- **Input**:
  - Customer gọi DELETE template-01 với token hợp lệ.
- **Các bước thực hiện**:
  - Xác thực danh tính thành công.
  - Cho authorization kiểm tra role.
  - Theo dõi service/repository.
- **Expected output (bắt buộc)**:
  - HTTP 403 với mã FORBIDDEN.
  - Không gọi nghiệp vụ xóa, không truy vấn hoặc cập nhật template, không gọi File Storage.
  - Response không tiết lộ trạng thái tồn tại của template.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Tách 403 khỏi 401 giúp thể hiện đúng rằng danh tính đã biết nhưng không đủ quyền, đồng thời giữ thứ tự bảo mật.

## TEST_LINKS

- [TDD-026 - Xóa Card Template](../../tdd/TDD-026/TDD-026-xoa-card-template.md)
- BR-022-01


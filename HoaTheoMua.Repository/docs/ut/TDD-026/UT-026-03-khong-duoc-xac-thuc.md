# UT-026-03

## Thông tin tài liệu

- **Tiêu đề:** Từ chối xóa template khi chưa xác thực
- **Ghi chú:** Endpoint quản trị phải xác thực người gọi trước khi kiểm tra dữ liệu template.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-026-03
- **Phiên bản:** 0.2
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** ADMIN - Card Template
- **Unit under test:** `CardTemplateService.DeleteAsync`
- **Loại:** Security
- **Mục tiêu:** Không cho request thiếu danh tính truy cập chức năng xóa.
- **Precondition / Mock setup**:
  - Template template-01 tồn tại nhưng repository không được phép bị gọi.
  - Request không có access token hợp lệ.
- **Dữ liệu đại diện**:
  - Không gửi token là dữ liệu chính; token sai chữ ký, hết hạn hoặc thuộc tài khoản không còn hoạt động đại diện cùng trạng thái chưa xác thực.
  - Các biến thể đều dừng trước kiểm tra role và trước truy vấn template.
- **Input**:
  - Gọi DELETE template-01 mà không có Authorization hợp lệ.
- **Các bước thực hiện**:
  - Gửi request tới endpoint.
  - Quan sát middleware xác thực và các dependency phía sau.
- **Expected output (bắt buộc)**:
  - HTTP 401 với mã UNAUTHORIZED.
  - Không kiểm tra role, không gọi service/repository, không đổi isDeleted và không gọi File Storage.
  - Response không tiết lộ template-01 có tồn tại hay không.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Xác thực trước truy vấn tài nguyên ngăn truy cập trái phép và tránh để người ngoài dò ID template.

## TEST_LINKS

- [TDD-026 - Xóa Card Template](../../tdd/TDD-026/TDD-026-xoa-card-template.md)
- BR-022-01


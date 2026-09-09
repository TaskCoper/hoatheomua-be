# UT-027-10

## Thông tin tài liệu

- **Tiêu đề:** Từ chối field bị khóa
- **Ghi chú:** Template, Size và nguồn hoa phải lấy từ Card/History nguồn, không nhận giá trị thay thế từ client.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-10
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** Request validator của recreate-content
- **Loại:** Negative / Validation
- **Mục tiêu:** Đảm bảo client không thể thay Template, Size hoặc nguồn của Card recreate.
- **Precondition / Mock setup**:
  - Người dùng đã đăng nhập; theo dõi service/repository để xác nhận validation dừng sớm.
- **Dữ liệu đại diện**:
  - Chạy riêng từng field `card_template_id`, `size`, `product_id`, `generated_flower_id`; mỗi request chỉ thêm một field bị khóa.
- **Input**:
  - Route hợp lệ; body chứa một field bị khóa.
- **Các bước thực hiện**:
  - Gọi endpoint cho từng biến thể.
- **Expected output (bắt buộc)**:
  - HTTP 400 với `messageCode=RECREATE_LOCKED_FIELD_NOT_ALLOWED`.
  - Lỗi xác định đúng field bị khóa.
  - Không tải Card/History, không gọi AIModule và không ghi database.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Cho phép thay các field này sẽ phá mục tiêu giữ nguyên thiết kế và lineage nguồn.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-07

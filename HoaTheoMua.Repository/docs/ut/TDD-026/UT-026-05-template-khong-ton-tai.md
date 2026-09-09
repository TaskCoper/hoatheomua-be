# UT-026-05

## Thông tin tài liệu

- **Tiêu đề:** Không tìm thấy Card Template cần xóa
- **Ghi chú:** ID hợp lệ về cú pháp nhưng không có bản ghi tương ứng.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-026-05
- **Phiên bản:** 0.2
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** ADMIN - Card Template
- **Unit under test:** `CardTemplateService.DeleteAsync`
- **Loại:** Negative
- **Mục tiêu:** Trả 404 và không thực hiện bất kỳ ghi dữ liệu nào.
- **Precondition / Mock setup**:
  - Người dùng đã xác thực với role Admin.
  - Repository trả về null khi tìm template-missing.
- **Dữ liệu đại diện**:
  - Một UUID không tồn tại đại diện cho mọi ID hợp lệ về định dạng nhưng không ánh xạ tới Card Template nào.
- **Input**:
  - Gọi DELETE với id = template-missing.
- **Các bước thực hiện**:
  - Cho request đi qua xác thực và phân quyền.
  - Cho service truy vấn template đúng một lần.
  - Kiểm tra response và số lần save.
- **Expected output (bắt buộc)**:
  - HTTP 404 với mã CARD_TEMPLATE_NOT_FOUND.
  - Repository chỉ thực hiện truy vấn cần thiết và không gọi save.
  - Không gọi File Storage, không thay đổi GeneratedCard/History.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** 404 phân biệt tài nguyên không tồn tại với tài nguyên đã xóa và bảo đảm service không tự tạo hoặc cập nhật một bản ghi thay thế.

## TEST_LINKS

- [TDD-026 - Xóa Card Template](../../tdd/TDD-026/TDD-026-xoa-card-template.md)
- BR-022-03


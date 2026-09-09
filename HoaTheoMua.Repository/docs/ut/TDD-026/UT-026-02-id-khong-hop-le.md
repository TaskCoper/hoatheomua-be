# UT-026-02

## Thông tin tài liệu

- **Tiêu đề:** Từ chối ID Card Template sai định dạng
- **Ghi chú:** Kiểm tra validation tham số đường dẫn trước khi gọi service hoặc database.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-026-02
- **Phiên bản:** 0.2
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** ADMIN - Card Template
- **Unit under test:** `CardTemplateService.DeleteAsync`
- **Loại:** Negative / Validation
- **Mục tiêu:** Trả lỗi rõ ràng khi ID không phải UUID hợp lệ.
- **Precondition / Mock setup**:
  - Người dùng đã xác thực với role Admin.
  - Mock service/repository được theo dõi số lần gọi.
- **Dữ liệu đại diện**:
  - id = abc là dữ liệu chính; chuỗi rỗng, UUID thiếu ký tự hoặc chứa ký tự ngoài hệ UUID là các biến thể cùng nhánh sai định dạng.
  - Mỗi biến thể được chạy riêng nhưng đều phải bị chặn ở tầng validation.
- **Input**:
  - Gọi DELETE với id = abc.
- **Các bước thực hiện**:
  - Gửi request tới endpoint xóa template.
  - Quan sát validation, service và repository.
- **Expected output (bắt buộc)**:
  - HTTP 400 với mã VALIDATION_ERROR; thông báo chỉ rõ ID không đúng định dạng UUID.
  - Không gọi CardTemplateService xử lý xóa, không truy vấn repository và không ghi database.
  - Không gọi File Storage và không thay đổi GeneratedCard/History.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** ID sai cú pháp không thể ánh xạ tới tài nguyên; chặn sớm tạo hợp đồng lỗi ổn định và tránh truy vấn vô ích.

## TEST_LINKS

- [TDD-026 - Xóa Card Template](../../tdd/TDD-026/TDD-026-xoa-card-template.md)
- API Contract nội bộ - Mã lỗi `VALIDATION_ERROR/400`

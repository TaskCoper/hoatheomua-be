# UT-027-12

## Thông tin tài liệu

- **Tiêu đề:** Từ chối form type không hợp lệ
- **Ghi chú:** Chỉ chấp nhận gõ máy hoặc Calligraphy khi client chủ động đổi hình thức.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-12
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
- **Mục tiêu:** Chặn form type ngoài hai giá trị được hỗ trợ.
- **Precondition / Mock setup**:
  - Người dùng đã đăng nhập; theo dõi service/repository.
- **Dữ liệu đại diện**:
  - `handwriting` đại diện cho chuỗi không thuộc `go_may|calligraphy`.
- **Input**:
  - `form_type=handwriting`.
- **Các bước thực hiện**:
  - Gọi recreate-content.
- **Expected output (bắt buộc)**:
  - HTTP 400 với `messageCode=VALIDATION_ERROR` và lỗi field `form_type`.
  - Không đọc Price Config/prompt, không gọi AIModule và không ghi dữ liệu.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Form type quyết định cách render và tính giá nên không được nhận giá trị tự do.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-09

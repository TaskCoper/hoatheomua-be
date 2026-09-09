# UT-027-13

## Thông tin tài liệu

- **Tiêu đề:** Từ chối ảnh đính kèm không hợp lệ
- **Ghi chú:** Ảnh mới phải qua cùng validation với Card Create trước khi gọi AI.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-13
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Validation
- **Mục tiêu:** Không đưa URL/file ảnh không dùng được vào snapshot hoặc AIModule.
- **Precondition / Mock setup**:
  - Card/History nguồn hợp lệ.
  - Bộ kiểm tra ảnh trả kết quả không hợp lệ.
- **Dữ liệu đại diện**:
  - Chạy riêng URL sai định dạng, định dạng file không hỗ trợ, file quá dung lượng và tải/upload thất bại ở bước admission.
- **Input**:
  - `attached_image_url` chứa dữ liệu không hợp lệ tương ứng.
- **Các bước thực hiện**:
  - Gọi recreate-content cho từng biến thể.
- **Expected output (bắt buộc)**:
  - HTTP 400 với `messageCode=VALIDATION_ERROR`, lỗi field `attached_image_url`.
  - Không gọi AIModule và không tạo Card/History mới.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Chặn sớm ảnh lỗi giúp tránh request AI không xác định và dữ liệu snapshot hỏng.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-10

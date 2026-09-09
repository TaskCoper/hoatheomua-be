# UT-027-19

## Thông tin tài liệu

- **Tiêu đề:** System Prompt hiện hành không hợp lệ
- **Ghi chú:** Recreate-content không fallback sang prompt snapshot của Card nguồn.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-19
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Dependency
- **Mục tiêu:** Chặn request khi prompt `card` hiện hành không dùng được.
- **Precondition / Mock setup**:
  - Source, input và pricing hợp lệ.
  - History nguồn có prompt snapshot hợp lệ để xác nhận service không dùng fallback.
- **Dữ liệu đại diện**:
  - Chạy riêng prompt hiện hành không tồn tại, content null, rỗng, blank hoặc sai cấu trúc bắt buộc.
- **Input**:
  - Request recreate-content hợp lệ.
- **Các bước thực hiện**:
  - Cho service tải prompt hiện hành và kiểm tra kết quả.
- **Expected output (bắt buộc)**:
  - HTTP 409 với `messageCode=SYSTEM_PROMPT_INVALID`.
  - Không dùng `system_form` nguồn thay thế, không gọi AIModule và không ghi dữ liệu.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Flow này yêu cầu prompt hiện hành để áp dụng cách tạo nội dung mới thống nhất.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-13

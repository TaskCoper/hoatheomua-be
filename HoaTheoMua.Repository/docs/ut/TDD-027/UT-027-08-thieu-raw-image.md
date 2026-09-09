# UT-027-08

## Thông tin tài liệu

- **Tiêu đề:** Card AI nguồn thiếu ảnh gốc
- **Ghi chú:** Không được dùng ảnh hoàn chỉnh đã chèn chữ thay cho `raw_image`.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-08
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Data integrity
- **Mục tiêu:** Chặn Card nguồn không có ảnh gốc dùng được.
- **Precondition / Mock setup**:
  - Card AI và History thuộc đúng người dùng; các snapshot khác hợp lệ.
  - `image_url` nguồn vẫn tồn tại để xác nhận service không dùng nó thay thế.
- **Dữ liệu đại diện**:
  - Chạy riêng `raw_image=null`, rỗng, blank và URL không dùng được; cùng phải trả một lỗi.
- **Input**:
  - Gọi recreate-content với body `{}`.
- **Các bước thực hiện**:
  - Cho service tải nguồn và kiểm tra `raw_image`.
- **Expected output (bắt buộc)**:
  - HTTP 409 với `messageCode=CARD_SOURCE_RAW_IMAGE_MISSING`.
  - Không fallback sang `image_url`, không gọi AIModule và không ghi dữ liệu mới.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Dùng ảnh đã có chữ làm nguồn có thể làm lặp chữ và sai thiết kế.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-01, BR-027-05

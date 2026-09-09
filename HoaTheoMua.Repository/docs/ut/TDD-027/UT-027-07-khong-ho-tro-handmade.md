# UT-027-07

## Thông tin tài liệu

- **Tiêu đề:** Không cho recreate từ Card HandMade
- **Ghi chú:** Flow này chỉ dùng ảnh gốc do AI sinh nên Card HandMade không hợp lệ làm nguồn.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-07
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Business rule
- **Mục tiêu:** Chặn source không phải Card AI bằng mã lỗi chuyên biệt.
- **Precondition / Mock setup**:
  - Người dùng sở hữu Card nguồn có `card_type=handmade` và History `type=handmade_card`.
- **Dữ liệu đại diện**:
  - Một Card HandMade hợp lệ đại diện cho mọi source không có thiết kế AI gốc.
- **Input**:
  - Gọi recreate-content với ID Card HandMade; body `{}`.
- **Các bước thực hiện**:
  - Cho service tải Card/History và kiểm tra loại Card.
- **Expected output (bắt buộc)**:
  - HTTP 409 với `messageCode=HANDMADE_CARD_RECREATE_NOT_SUPPORTED`.
  - Không đọc prompt/Price Config, không gọi AIModule và không ghi dữ liệu.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Tránh gửi ảnh template HandMade vào flow image-to-image dành cho Card AI.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-05

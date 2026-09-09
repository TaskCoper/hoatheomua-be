# UT-027-05

## Thông tin tài liệu

- **Tiêu đề:** Card nguồn không tồn tại
- **Ghi chú:** Kiểm tra ID đúng định dạng nhưng không trỏ đến Generated Card nào.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-05
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Not found
- **Mục tiêu:** Trả lỗi rõ ràng khi không có Card nguồn.
- **Precondition / Mock setup**:
  - Người dùng đã đăng nhập.
  - Repository không tìm thấy Card theo `source_card_id`.
- **Dữ liệu đại diện**:
  - Một UUID không tồn tại đại diện cho mọi ID hợp lệ về cú pháp nhưng không có record.
- **Input**:
  - `source_card_id` không tồn tại; body `{}`.
- **Các bước thực hiện**:
  - Gọi recreate-content.
- **Expected output (bắt buộc)**:
  - HTTP 404 với `messageCode=CARD_NOT_FOUND`.
  - Không tìm Price Config/prompt, không gọi AIModule và không ghi database.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Phân biệt Card không tồn tại với Card không thuộc quyền sở hữu giúp xử lý đúng contract.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-03, BR-027-04

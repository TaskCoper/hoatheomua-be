# UT-027-11

## Thông tin tài liệu

- **Tiêu đề:** Từ chối nội dung hiệu lực vượt giới hạn
- **Ghi chú:** Kiểm tra giới hạn từ sau khi hệ thống đã resolve dữ liệu mới hoặc fallback từ nguồn.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-11
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
- **Mục tiêu:** Đảm bảo sender, receiver và message hiệu lực tuân theo giới hạn của Card Create và Size snapshot.
- **Precondition / Mock setup**:
  - Card/History nguồn hợp lệ; Size snapshot có `size_max_words=100`.
  - Theo dõi Price Config, prompt, AIModule và persistence.
- **Dữ liệu đại diện**:
  - Chạy riêng sender trên 20 từ, receiver trên 20 từ và message trên 100 từ hoặc vượt `size_max_words`.
- **Input**:
  - Mỗi request chỉ có một text field vượt giới hạn; các field còn lại hợp lệ hoặc dùng fallback.
- **Các bước thực hiện**:
  - Gọi recreate-content và kiểm tra lỗi theo field.
- **Expected output (bắt buộc)**:
  - HTTP 400 với `messageCode=VALIDATION_ERROR`.
  - `errors` chỉ rõ text field không hợp lệ.
  - Không tải dependency phía sau validation, không gọi AIModule và không ghi dữ liệu.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Nội dung vượt giới hạn có thể không vừa thiết kế đã khóa.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-08

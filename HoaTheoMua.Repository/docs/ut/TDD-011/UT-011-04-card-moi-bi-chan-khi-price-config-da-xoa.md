# UT-011-04

## Thông tin tài liệu

- **Tiêu đề:** Chặn Card mới khi Price Config đã bị xóa
- **Ghi chú:** Flow có calligraphy cần Price Config live để chọn bậc giá theo số từ.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-011-04
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CARD - Admission theo Card Config
- **Unit under test:** `CardService.CreateAsync`
- **Loại:** Error
- **Mục tiêu:** Trả lỗi Gone trước AI hoặc persistence khi singleton giá calligraphy đã bị xóa.
- **Precondition / Mock setup**:
  - Người dùng đã đăng nhập; request Card calligraphy hợp lệ đến bước tải bảng giá.
  - Exact Config card_config.prices tồn tại với IsDeleted=true.
  - Lời chúc có 20 từ và Size Config đang khả dụng để cô lập lỗi Price Config.
- **Dữ liệu đại diện**:
  - Card calligraphy AI tại TDD-006 là dữ liệu chính; HandMade tại TDD-021 và các flow khác cần tính phí theo từ đại diện cùng dependency.
  - Lời chúc 20 từ đại diện mọi wordCount vốn có thể khớp rule nếu config còn live; số từ không quyết định mã deleted.
  - Mỗi flow đại diện phải chạy riêng và đều dừng trước khi chọn bậc giá.
- **Input**:
  - POST endpoint tạo Card calligraphy với message 20 từ và các trường khác hợp lệ.
- **Các bước thực hiện**:
  - Gọi flow tạo Card.
  - Cho service tải Price Config và phát hiện IsDeleted=true.
  - Kiểm tra response cùng các dependency AI/persistence.
- **Expected output (bắt buộc)**:
  - HTTP 410 với messageCode=CALLIGRAPHY_CONFIG_DELETED.
  - Không chuyển thành lỗi rule giá hoặc schema vì trạng thái deleted được kiểm tra trước.
  - Không gọi AI, không trừ quota và không persist Card/History.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Bài test bảo vệ tính đúng đắn của giá và trả nguyên nhân rõ ràng khi dependency quản trị đã bị xóa.

## TEST_LINKS

- [TDD-011 - Xóa Config có hỗ trợ Card Config](../../tdd/TDD-011/TDD-011-xoa-card-config.md)
- BR-011-07
- Đối chiếu TDD-006 và TDD-021


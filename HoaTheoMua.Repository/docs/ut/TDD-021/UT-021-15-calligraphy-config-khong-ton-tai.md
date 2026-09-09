# UT-021-15: Báo lỗi khi chưa có bảng giá viết tay

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi chưa có bảng giá viết tay
- **Ghi chú**: Phí viết chữ phải lấy từ bảng giá đã được quản trị, không được tự đặt.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-15
- **Phiên bản**: v1.1
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp mới)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Nguồn hoa, mẫu thiệp và kích thước chung đều hợp lệ.
  - Khi tìm `card_config.prices`, nơi lưu dữ liệu báo không có bản ghi.
- **Các trường hợp cần kiểm tra**:
  - Dùng lời chúc 8 từ nhưng không có bảng giá để xác định phụ phí.
- **Input**:
  ```text
  messageContent có 8 từ
  card_config.prices = không có
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 404 với mã CALLIGRAPHY_CONFIG_NOT_FOUND.
  Hệ thống không tự đặt giá mặc định và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Tự đoán giá có thể làm khách bị thu sai tiền. Bài kiểm tra buộc hệ thống dừng cho đến khi bảng giá chính thức được thiết lập.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-12
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

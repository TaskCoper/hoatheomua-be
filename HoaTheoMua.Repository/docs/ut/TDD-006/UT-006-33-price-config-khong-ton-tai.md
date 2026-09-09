# UT-006-33: Báo lỗi khi chưa có bảng giá viết tay

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi chưa có bảng giá viết tay
- **Ghi chú**: Thiệp dùng chữ viết tay cần bảng giá để xác định phụ phí.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-33
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: AI CARD - Tạo thiệp thiết kế AI
- **Unit under test (bắt buộc)**: `AICardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp AI mới)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Khách hàng đã đăng nhập; sản phẩm nguồn, mẫu thiệp AI và kích thước chung đều hợp lệ.
  - Hình thức chữ là `calligraphy` (viết tay), nên hệ thống bắt buộc phải đọc bảng giá viết tay.
  - Hướng dẫn dành cho AI đang dùng được. Phần tạo ảnh và việc lưu dữ liệu được theo dõi.
  - Không có bản ghi group=card_config, key=card_config.prices.
- **Các trường hợp cần kiểm tra**:
  - Không có bảng giá viết tay nào được thiết lập.
- **Input**:
  ```text
  POST /api/ai-cards
  formType=calligraphy
  card_config.prices=không có
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 404 với mã CALLIGRAPHY_CONFIG_NOT_FOUND.
  Hệ thống không tự đặt giá, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Tự đoán phụ phí có thể làm khách bị thu sai tiền. Hệ thống phải chờ bảng giá chính thức.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-09
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

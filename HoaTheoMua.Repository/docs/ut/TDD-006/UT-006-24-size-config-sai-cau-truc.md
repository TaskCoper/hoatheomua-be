# UT-006-24: Từ chối cấu hình kích thước có cấu trúc sai

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối cấu hình kích thước có cấu trúc sai
- **Ghi chú**: Cấu hình tồn tại nhưng thiếu trường cần thiết để tạo bản chụp kích thước đáng tin cậy.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-24
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
  - Khách hàng đã đăng nhập; sản phẩm nguồn và mẫu thiệp AI đều hợp lệ.
  - Bảng giá viết tay và hướng dẫn dành cho AI đang dùng được.
  - Phần tạo ảnh và việc lưu dữ liệu được theo dõi để xác nhận chúng không diễn ra khi cấu hình kích thước bị từ chối.
  - card_config.sizes đang hoạt động nhưng phần tử kích thước thiếu max_words.
- **Các trường hợp cần kiểm tra**:
  - Chỉ kiểm tra trường hợp thiếu giới hạn số từ.
- **Input**:
  ```text
  POST /api/ai-cards
  Size item={name, width, height, base_price}; thiếu max_words
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã CARD_SIZE_CONFIG_INVALID.
  Thông báo xác định lỗi nằm ở cấu hình hệ thống; hệ thống không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiếu giới hạn từ khiến hệ thống không biết lời chúc có vừa không. Dừng lại an toàn hơn việc tự đặt một giới hạn.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-07
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

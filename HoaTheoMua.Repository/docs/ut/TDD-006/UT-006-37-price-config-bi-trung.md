# UT-006-37: Từ chối khi có nhiều bản bảng giá viết tay

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối khi có nhiều bản bảng giá viết tay
- **Ghi chú**: Có nhiều bản ghi cùng loại khiến hệ thống không biết bảng nào là nguồn chính thức.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-37
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
  - Có hai bản chưa bị xóa cùng group=card_config và key=card_config.prices.
- **Các trường hợp cần kiểm tra**:
  - Một bản được bật và một bản bị tắt vẫn được xem là trùng vì cả hai chưa bị xóa.
- **Input**:
  ```text
  POST /api/ai-cards
  Số bản card_config.prices chưa bị xóa=2
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã CARD_CONFIG_AMBIGUOUS.
  Hệ thống không chọn bảng giá, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Hai bảng có thể cho hai số tiền khác nhau. Từ chối giúp cùng một lời chúc không bị tính giá thất thường.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-09, BR-006-13
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

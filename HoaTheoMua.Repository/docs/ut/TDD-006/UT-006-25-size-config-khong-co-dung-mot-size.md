# UT-006-25: Từ chối cấu hình không có đúng một kích thước chung

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối cấu hình không có đúng một kích thước chung
- **Ghi chú**: Hệ thống không cho khách chọn kích thước nên cấu hình bắt buộc chỉ có một lựa chọn chung.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-25
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
  - card_config.sizes đang hoạt động nhưng items chứa hai kích thước.
- **Các trường hợp cần kiểm tra**:
  - Kiểm tra danh sách có 2 phần tử, thay vì đúng 1 phần tử.
- **Input**:
  ```text
  POST /api/ai-cards
  card_config.sizes.items.count=2
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã CARD_SIZE_CONFIG_INVALID.
  Hệ thống không tự chọn phần tử đầu tiên, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu có hai kích thước, hệ thống không có quy tắc để chọn. Từ chối giúp tránh kết quả thay đổi theo thứ tự dữ liệu.

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

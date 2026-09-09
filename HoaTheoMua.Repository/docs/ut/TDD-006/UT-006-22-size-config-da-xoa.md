# UT-006-22: Từ chối cấu hình kích thước đã được đánh dấu là đã xóa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối cấu hình kích thước đã được đánh dấu là đã xóa
- **Ghi chú**: Cấu hình cũ còn để tra cứu nhưng không còn được phép sử dụng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-22
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
  - card_config.sizes có isDeleted=true và isPublic=false.
- **Các trường hợp cần kiểm tra**:
  - Cấu hình đồng thời đã xóa và bị tắt để xác nhận lỗi đã xóa được ưu tiên.
- **Input**:
  ```text
  POST /api/ai-cards
  card_config.sizes: isDeleted=true; isPublic=false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 410 với mã CARD_SIZE_CONFIG_DELETED.
  Hệ thống không báo lỗi tạm ngưng, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Dùng cấu hình đã xóa có thể áp dụng kích thước hoặc giá mà quản trị viên đã loại bỏ.

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

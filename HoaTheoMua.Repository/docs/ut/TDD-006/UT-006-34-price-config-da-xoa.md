# UT-006-34: Từ chối bảng giá viết tay đã được đánh dấu là đã xóa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối bảng giá viết tay đã được đánh dấu là đã xóa
- **Ghi chú**: Bảng giá cũ còn để tra cứu nhưng không còn hiệu lực cho thiệp mới.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-34
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
  - card_config.prices có isDeleted=true và isPublic=false.
- **Các trường hợp cần kiểm tra**:
  - Bảng giá đồng thời đã xóa và bị tắt để xác nhận lỗi đã xóa được ưu tiên.
- **Input**:
  ```text
  POST /api/ai-cards
  card_config.prices: isDeleted=true; isPublic=false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 410 với mã CALLIGRAPHY_CONFIG_DELETED.
  Hệ thống không báo lỗi tạm ngưng, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Dùng lại bảng giá đã xóa có thể thu một mức tiền mà quản trị viên đã loại bỏ.

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

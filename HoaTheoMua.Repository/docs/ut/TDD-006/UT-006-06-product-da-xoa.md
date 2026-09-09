# UT-006-06: Từ chối sản phẩm hoa đã được đánh dấu là đã xóa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối sản phẩm hoa đã được đánh dấu là đã xóa
- **Ghi chú**: Sản phẩm vẫn còn để tra cứu nhưng không còn được phép dùng cho thiệp mới.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-06
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
  - Khách hàng `customer-01` đã đăng nhập và tài khoản đang hoạt động.
  - Mẫu thiệp AI, kích thước chung, bảng giá viết tay và hướng dẫn dành cho AI đều đang dùng được.
  - Phần tạo ảnh và việc lưu dữ liệu được theo dõi để xác nhận chúng không diễn ra khi sản phẩm bị từ chối.
  - product-001 có isDeleted=true và isActive=false.
- **Các trường hợp cần kiểm tra**:
  - Sản phẩm đồng thời đã xóa và tạm ngưng để xác nhận lỗi đã xóa được ưu tiên.
- **Input**:
  ```text
  POST /api/ai-cards
  productId=product-001
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 410 với mã PRODUCT_DELETED, không báo PRODUCT_INACTIVE.
  Hệ thống không kiểm tra số lượng còn lại, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Đã xóa khác với tạm ngưng. Báo đúng tình trạng giúp tránh hiểu nhầm rằng chỉ cần bật sản phẩm trở lại.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-03
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

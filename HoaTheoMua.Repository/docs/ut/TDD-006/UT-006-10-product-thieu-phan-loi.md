# UT-006-10: Từ chối sản phẩm hoa thiếu phần lõi cần thiết

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối sản phẩm hoa thiếu phần lõi cần thiết
- **Ghi chú**: Sản phẩm có công thức nhưng công thức không chứa phần lõi bắt buộc.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-10
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
  - product-001 tồn tại, chưa xóa, đang hoạt động nhưng có trạng thái no_core.
- **Các trường hợp cần kiểm tra**:
  - Chỉ kiểm tra trường hợp công thức không có phần lõi.
- **Input**:
  ```text
  POST /api/ai-cards
  productId=product-001
  availabilityReason=no_core
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 422 với mã PRODUCT_NOT_SELLABLE.
  Hệ thống không báo nhầm là hết hàng, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiếu phần lõi khiến sản phẩm chưa đủ điều kiện bán dù đã có công thức. Tách riêng giúp xác định đúng nguyên nhân này.

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

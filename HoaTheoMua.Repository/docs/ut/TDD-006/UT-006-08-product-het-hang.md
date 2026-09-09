# UT-006-08: Từ chối sản phẩm hoa đã hết hàng

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối sản phẩm hoa đã hết hàng
- **Ghi chú**: Kiểm tra ranh giới khi số lượng có thể bán bằng đúng 0.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-08
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: AI CARD - Tạo thiệp thiết kế AI
- **Unit under test (bắt buộc)**: `AICardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp AI mới)
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Khách hàng `customer-01` đã đăng nhập và tài khoản đang hoạt động.
  - Mẫu thiệp AI, kích thước chung, bảng giá viết tay và hướng dẫn dành cho AI đều đang dùng được.
  - Phần tạo ảnh và việc lưu dữ liệu được theo dõi để xác nhận chúng không diễn ra khi sản phẩm bị từ chối.
  - product-001 tồn tại, chưa xóa, đang hoạt động và có đủ thành phần để bán.
  - Phần tính số lượng có thể bán trả 0.
- **Các trường hợp cần kiểm tra**:
  - Dùng đúng giá trị 0 vì đây là điểm đầu tiên không còn sản phẩm nào để cung cấp.
- **Input**:
  ```text
  POST /api/ai-cards
  productId=product-001
  sellableQuantity=0
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã PRODUCT_OUT_OF_STOCK.
  Hệ thống không gọi AI và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu vẫn tạo thiệp cho sản phẩm hết hàng, khách có thể chuẩn bị một thiệp không thể đi cùng sản phẩm đã chọn.

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

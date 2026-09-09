# UT-006-05: Báo lỗi khi không tìm thấy sản phẩm hoa nguồn

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi không tìm thấy sản phẩm hoa nguồn
- **Ghi chú**: Mã sản phẩm có hình thức hợp lệ nhưng không khớp với sản phẩm nào đang lưu.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-05
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
  - Không có sản phẩm mang mã product-missing.
- **Các trường hợp cần kiểm tra**:
  - Chỉ gửi mã sản phẩm không tồn tại.
- **Input**:
  ```text
  POST /api/ai-cards
  productId=product-missing
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 404 (không tìm thấy dữ liệu) với mã PRODUCT_NOT_FOUND.
  Hệ thống không gọi AI và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiệp phải dựa trên một sản phẩm có thật. Nếu vẫn cho tạo, lịch sử sẽ chứa một mã nguồn không thể đối chiếu.

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

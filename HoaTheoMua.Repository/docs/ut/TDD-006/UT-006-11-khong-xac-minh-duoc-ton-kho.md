# UT-006-11: Báo lỗi khi không xác minh được số lượng có thể bán

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi không xác minh được số lượng có thể bán
- **Ghi chú**: Phần tính số lượng còn lại không thể đưa ra kết quả đáng tin cậy.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-11
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
  - product-001 tồn tại, chưa xóa, đang hoạt động và có đủ thành phần để bán.
  - Phần tính số lượng dừng vì quá thời gian chờ.
- **Các trường hợp cần kiểm tra**:
  - Không có kết quả số lượng, khác với kết quả hợp lệ bằng 0.
- **Input**:
  ```text
  POST /api/ai-cards
  productId=product-001
  Kiểm tra số lượng=thất bại
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 503 (tạm thời chưa phục vụ được) với mã PRODUCT_AVAILABILITY_UNAVAILABLE.
  Hệ thống không tự cho rằng sản phẩm còn hàng, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Tiếp tục khi chưa biết chắc còn hàng là dựa trên phỏng đoán. Dừng lại giúp tránh nhận yêu cầu cho sản phẩm có thể không cung cấp được.

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

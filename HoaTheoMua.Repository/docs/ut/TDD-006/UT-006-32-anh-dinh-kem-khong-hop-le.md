# UT-006-32: Từ chối địa chỉ ảnh đính kèm không hợp lệ

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối địa chỉ ảnh đính kèm không hợp lệ
- **Ghi chú**: Ảnh đính kèm là tùy chọn, nhưng nếu có thì địa chỉ phải dùng được theo quy định của Card.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-32
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
  - Sản phẩm `product-001` tồn tại, chưa bị xóa, đang bán, có công thức hoa, có phần lõi cần thiết và còn 10 sản phẩm có thể bán.
  - Mẫu thiệp `template-001` thuộc loại AI, chưa bị xóa và đang hoạt động.
  - Cấu hình kích thước có đúng một kích thước chung: rộng 15, cao 25, giá 15.000 đồng và tối đa 100 từ.
  - Bảng giá viết tay có ba mức: 0-35 từ thêm 0 đồng, 36-70 từ thêm 39.000 đồng, 71-100 từ thêm 69.000 đồng.
  - Hướng dẫn dành cho AI loại `card` có nội dung dùng được. Phần tạo ảnh trả `rawImage=raw.png` và `imageUrl=final.png`; việc lưu ảnh và dữ liệu đều thành công.
- **Các trường hợp cần kiểm tra**:
  - Gửi attachedImageUrl là một chuỗi không có hình thức địa chỉ mạng.
- **Input**:
  ```text
  POST /api/ai-cards
  attachedImageUrl=not-a-valid-url
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 400 với mã VALIDATION_ERROR và chỉ rõ attachedImageUrl.
  Hệ thống không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Địa chỉ sai khiến AI không thể lấy ảnh tham chiếu. Dừng trước khi gọi AI giúp khách sửa đúng thông tin gây lỗi.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-10
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

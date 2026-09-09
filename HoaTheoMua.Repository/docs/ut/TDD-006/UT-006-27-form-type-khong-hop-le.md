# UT-006-27: Từ chối hình thức chữ không được hỗ trợ

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối hình thức chữ không được hỗ trợ
- **Ghi chú**: Thiệp AI chỉ cho phép chữ gõ máy hoặc chữ viết tay.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-27
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
  - Gửi formType=handwriting, không phải go_may hoặc calligraphy.
- **Input**:
  ```text
  POST /api/ai-cards
  formType=handwriting
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 400 với mã VALIDATION_ERROR và chỉ rõ trường formType.
  Hệ thống không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Một giá trị không được hỗ trợ khiến hệ thống không biết phải tính phụ phí và tạo chữ theo cách nào.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-08
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

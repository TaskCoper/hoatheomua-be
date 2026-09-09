# UT-006-31: Từ chối lời chúc vượt giới hạn của kích thước chung

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối lời chúc vượt giới hạn của kích thước chung
- **Ghi chú**: Lời chúc có thể chưa vượt 100 từ nhưng vẫn quá dài đối với kích thước đang dùng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-31
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
  - Sản phẩm `product-001` tồn tại, chưa bị xóa, đang bán, có công thức hoa, có phần lõi cần thiết và còn 10 sản phẩm có thể bán.
  - Mẫu thiệp `template-001` thuộc loại AI, chưa bị xóa và đang hoạt động.
  - Cấu hình kích thước có đúng một kích thước chung: rộng 15, cao 25, giá 15.000 đồng và tối đa 100 từ.
  - Bảng giá viết tay có ba mức: 0-35 từ thêm 0 đồng, 36-70 từ thêm 39.000 đồng, 71-100 từ thêm 69.000 đồng.
  - Hướng dẫn dành cho AI loại `card` có nội dung dùng được. Phần tạo ảnh trả `rawImage=raw.png` và `imageUrl=final.png`; việc lưu ảnh và dữ liệu đều thành công.
  - Thay kích thước chung thành max_words=50.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc có 51 từ: dưới 100 nhưng vượt giới hạn kích thước một từ.
- **Input**:
  ```text
  POST /api/ai-cards
  messageContent có 51 từ
  size.max_words=50
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 400 với mã VALIDATION_ERROR và chỉ rõ lời chúc vượt max_words=50.
  Hệ thống không tự chọn kích thước khác, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Bài kiểm tra bảo đảm giới hạn thực tế của kích thước được tôn trọng, không chỉ kiểm tra mốc chung 100 từ.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-07, BR-006-10
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

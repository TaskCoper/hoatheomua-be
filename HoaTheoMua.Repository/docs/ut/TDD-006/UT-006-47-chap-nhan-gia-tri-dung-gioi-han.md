# UT-006-47: Chấp nhận nội dung có số từ đúng bằng giới hạn

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Chấp nhận nội dung có số từ đúng bằng giới hạn
- **Ghi chú**: Giá trị đúng bằng giới hạn vẫn hợp lệ; chỉ giá trị lớn hơn mới bị từ chối.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-47
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
  - Kích thước chung có max_words=100.
  - Tên người gửi và người nhận đều có đúng 20 từ; lời chúc có đúng 100 từ.
  - Bảng giá viết tay có mức 71-100 từ với phụ phí 69.000 đồng.
- **Các trường hợp cần kiểm tra**:
  - Dùng đúng các mốc tối đa, không vượt thêm từ nào.
- **Input**:
  ```text
  POST /api/ai-cards
  senderName=20 từ
  receiverName=20 từ
  messageContent=100 từ
  formType=calligraphy
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo thành công, không báo VALIDATION_ERROR.
  wordCount=100; basePrice=15000; extraPrice=69000; totalPrice=84000.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Kiểm tra đúng tại mốc tối đa giúp phát hiện lỗi so sánh làm hệ thống từ chối nhầm giá trị vẫn được phép.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-09, BR-006-10, BR-006-11
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

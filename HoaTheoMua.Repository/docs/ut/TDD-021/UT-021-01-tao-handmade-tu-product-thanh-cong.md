# UT-021-01: Tạo thiệp HandMade từ sản phẩm hoa thành công

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Tạo thiệp HandMade từ sản phẩm hoa thành công
- **Ghi chú**: Kiểm tra đường đi phổ biến nhất: khách chọn trực tiếp một sản phẩm hoa còn bán được và một mẫu thiệp viết tay có sẵn.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-01
- **Phiên bản**: v1.1
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp mới)
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Khách hàng `customer-01` đã đăng nhập và tài khoản đang hoạt động.
  - Sản phẩm `product-rose-01` tồn tại, đang bán, có đủ thông tin để bán và còn hàng.
  - Mẫu thiệp `template-hm-01` là mẫu HandMade đang dùng được, có ảnh `https://cdn.example.com/templates/hm-01.png`.
  - Hệ thống có một kích thước chung: tối đa 50 từ, giá ghi nhận 20.000 đồng. Bảng giá quy định lời chúc từ 0 đến 50 từ có phí 39.000 đồng.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc có 8 từ để xác nhận hệ thống chọn đúng mức phí 39.000 đồng.
  - Chỉ gửi `productId`, không gửi `generatedFlowerId`, để nguồn trực tiếp của thiệp là sản phẩm khách đã chọn.
- **Input**:
  ```text
  productId = product-rose-01
  cardTemplateId = template-hm-01
  senderName = An; receiverName = Bình
  messageContent = Chúc bạn luôn vui và hạnh phúc
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo thành công và tạo đúng một thiệp HandMade.
  Thiệp dùng ảnh của mẫu có sẵn; loại chữ là calligraphy; không có ảnh thô hoặc ảnh đính kèm.
  Giá của kích thước chỉ được ghi lại để đối chiếu. Số tiền phải trả là 39.000 đồng: basePrice = 0, extraPrice = 39000, totalPrice = 39000.
  Hệ thống tạo đúng một lịch sử: base_id và root cùng bằng product-rose-01; output_id là mã thiệp mới; system_form để trống.
  Phản hồi cho khách không chứa root. Hệ thống không gọi AI, không dùng hạn mức AI và không gửi ảnh sang nơi lưu file.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu đường đi này sai, khách có thể bị tính sai tiền hoặc thiệp không còn gắn đúng sản phẩm hoa ban đầu. Đây là trường hợp cần xác nhận trước tiên.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-02, BR-021-07, BR-021-11 đến BR-021-22, BR-021-24
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

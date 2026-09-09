# UT-021-02: Tạo thiệp HandMade từ mẫu hoa đã tạo thành công

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Tạo thiệp HandMade từ mẫu hoa đã tạo thành công
- **Ghi chú**: Kiểm tra trường hợp khách dùng một mẫu hoa đã tạo trước đó và hệ thống vẫn tìm đúng sản phẩm hoa gốc.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-02
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
  - Khách hàng `customer-01` đã đăng nhập và sở hữu mẫu hoa `flower-01`.
  - Mẫu hoa có ảnh dùng được. Lịch sử của mẫu hoa có type = flower, output_id = flower-01, base_id = root = product-rose-01.
  - Mẫu thiệp HandMade, kích thước chung và bảng giá đều đang dùng được. Mức giá cho lời chúc 0 từ là 0 đồng.
- **Các trường hợp cần kiểm tra**:
  - Dùng lời chúc rỗng để xác nhận hệ thống tính 0 từ và không thu phí viết chữ.
  - Chỉ gửi `generatedFlowerId`, không gửi `productId`, để kiểm tra việc tìm ngược về sản phẩm hoa gốc.
- **Input**:
  ```text
  generatedFlowerId = flower-01
  cardTemplateId = template-hm-01
  senderName = An; receiverName = Bình
  messageContent = chuỗi rỗng
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo thành công và tạo đúng một thiệp HandMade.
  wordCount = 0; basePrice = 0; extraPrice = 0; totalPrice = 0.
  Lịch sử mới có base_id = flower-01 và root = product-rose-01. Điều này cho biết nguồn trực tiếp là mẫu hoa, còn gốc cuối cùng là sản phẩm.
  root không xuất hiện trong phản hồi cho khách. Hệ thống không gọi AI, không dùng hạn mức AI và không gửi ảnh sang nơi lưu file.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiệp tạo từ mẫu hoa vẫn phải truy được về đúng sản phẩm gốc. Nếu thiếu liên kết này, các bước đối chiếu và kiểm tra hàng về sau có thể dùng nhầm sản phẩm.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-03, BR-021-08, BR-021-09, BR-021-11 đến BR-021-22, BR-021-24
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

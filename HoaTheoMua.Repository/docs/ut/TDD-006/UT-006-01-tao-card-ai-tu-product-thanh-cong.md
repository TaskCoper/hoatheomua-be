# UT-006-01: Tạo thiệp AI từ sản phẩm hoa thành công

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Tạo thiệp AI từ sản phẩm hoa thành công
- **Ghi chú**: Kiểm tra đường đi phổ biến khi khách chọn trực tiếp một sản phẩm và dùng hình thức chữ gõ máy.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-01
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: AI CARD - Tạo thiệp thiết kế AI
- **Unit under test (bắt buộc)**: `AICardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp AI mới)
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Khách hàng `customer-01` đã đăng nhập và tài khoản đang hoạt động.
  - Sản phẩm `product-001` tồn tại, chưa bị xóa, đang bán, có công thức hoa, có phần lõi cần thiết và còn 10 sản phẩm có thể bán.
  - Mẫu thiệp `template-001` thuộc loại AI, chưa bị xóa và đang hoạt động.
  - Cấu hình kích thước có đúng một kích thước chung: rộng 15, cao 25, giá 15.000 đồng và tối đa 100 từ.
  - Bảng giá viết tay có ba mức: 0-35 từ thêm 0 đồng, 36-70 từ thêm 39.000 đồng, 71-100 từ thêm 69.000 đồng.
  - Hướng dẫn dành cho AI loại `card` có nội dung dùng được. Phần tạo ảnh trả `rawImage=raw.png` và `imageUrl=final.png`; việc lưu ảnh và dữ liệu đều thành công.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc có 5 từ và không có ảnh đính kèm.
  - Chỉ gửi productId để sản phẩm là nguồn trực tiếp của thiệp.
- **Input**:
  ```text
  POST /api/ai-cards
  productId=product-001; generatedFlowerId=không có
  formType=go_may
  messageContent=Chúc bạn sinh nhật vui vẻ
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo thành công và tạo đúng một thiệp có cardType=ai.
  Hệ thống gọi phần tạo ảnh AI đúng một lần bằng dữ liệu đã được chụp lại trước đó.
  Thiệp giữ đúng người gửi, người nhận, lời chúc và formType=go_may từ yêu cầu.
  wordCount=5; basePrice=15000; extraPrice=0; totalPrice=15000; wordConfigSnapshot=null.
  Thông tin kích thước được lưu: rộng 15, cao 25, giá tại thời điểm tạo là 15.000 đồng và giới hạn 100 từ.
  Thiệp lưu rawImage=raw.png và imageUrl=final.png. Lịch sử có type=card, base_id=root=product-001 và output_id là mã thiệp mới.
  system_form của lịch sử lưu type=card và nội dung hướng dẫn đã dùng, nhưng không lưu mã của hướng dẫn. metadata chỉ lưu nguồn và mã nhận diện cần thiết, không chép lại nội dung hoặc giá đã có trên thiệp.
  Phản hồi cho khách không chứa root. Hệ thống không tự gắn thiệp vào giỏ hàng hoặc đơn hàng.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Đây là đường đi chính. Nếu sai, khách có thể nhận sai ảnh, sai giá hoặc một lịch sử không còn gắn đúng sản phẩm ban đầu.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-01, BR-006-02, BR-006-03, BR-006-07, BR-006-08, BR-006-11, BR-006-14, BR-006-15, BR-006-16, BR-006-17, BR-006-18, BR-006-19, BR-006-20
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

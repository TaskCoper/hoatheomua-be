# UT-006-02: Tạo thiệp AI từ mẫu hoa đã tạo và tính phí viết tay thành công

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Tạo thiệp AI từ mẫu hoa đã tạo và tính phí viết tay thành công
- **Ghi chú**: Kiểm tra hệ thống tìm đúng sản phẩm gốc của mẫu hoa và cộng đúng phí viết tay theo số từ.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-02
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
  - Mẫu hoa `flower-001` thuộc khách hàng này và có ảnh `flower.png` dùng được.
  - Lịch sử mẫu hoa có type=flower, output_id=flower-001, base_id=root=product-001.
  - Mẫu thiệp AI, kích thước chung, bảng giá viết tay và hướng dẫn dành cho AI đều đang dùng được.
  - Phần tạo ảnh trả `rawImage=raw.png` và `imageUrl=final.png`; việc lưu ảnh và dữ liệu đều thành công.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc có 45 từ nên nằm trong mức 36-70 từ, phụ phí 39.000 đồng.
  - Chỉ gửi generatedFlowerId để mẫu hoa là nguồn trực tiếp, còn product-001 là nguồn gốc cuối cùng.
- **Input**:
  ```text
  POST /api/ai-cards
  generatedFlowerId=flower-001; productId=không có
  formType=calligraphy
  wordCount=45
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo thành công và tạo đúng một thiệp AI.
  Hệ thống gọi phần tạo ảnh AI đúng một lần. Thiệp giữ đúng người gửi, người nhận, lời chúc và formType=calligraphy.
  basePrice=15000; extraPrice=39000; totalPrice=54000. Bản chụp mức giá lưu min_words=36, max_words=70, extra_price=39000.
  Lịch sử mới có base_id=flower-001, root=product-001 và output_id là mã thiệp mới. root không xuất hiện trong phản hồi cho khách.
  Hệ thống dùng product-001 làm nguồn gốc theo lịch sử đã có, không kiểm tra lại trạng thái bán hiện tại của product-001.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiệp dựa trên mẫu hoa vẫn phải truy được về sản phẩm gốc và áp dụng đúng mức phí. Hai thông tin này cần cho việc đối chiếu giá và nguồn về sau.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-04, BR-006-05, BR-006-09, BR-006-11, BR-006-17, BR-006-18, BR-006-19
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

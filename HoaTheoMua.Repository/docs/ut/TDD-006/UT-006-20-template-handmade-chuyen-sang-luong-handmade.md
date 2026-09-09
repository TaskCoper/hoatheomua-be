# UT-006-20: Chuyển mẫu HandMade sang đúng phần tạo thiệp HandMade

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Chuyển mẫu HandMade sang đúng phần tạo thiệp HandMade
- **Ghi chú**: Cùng một đường dẫn tiếp nhận hai loại thiệp; loại mẫu quyết định phần chương trình tiếp tục.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-20
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: AI CARD - Tạo thiệp thiết kế AI
- **Unit under test (bắt buộc)**: `AICardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp AI mới)
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Khách hàng `customer-01` đã đăng nhập và chọn sản phẩm `product-001` đang bán được.
  - Kích thước chung, bảng giá viết tay và hướng dẫn dành cho AI đều đang dùng được.
  - Phần tạo ảnh và việc lưu dữ liệu được theo dõi để xác nhận chúng không diễn ra khi mẫu thiệp bị từ chối.
  - template-001 tồn tại, đang hoạt động, chưa xóa và có templateType=handmade.
  - Phần tạo thiệp HandMade theo TDD-021 được thay bằng bộ phận giả để theo dõi việc chuyển tiếp.
- **Các trường hợp cần kiểm tra**:
  - Chọn mẫu loại HandMade thay vì mẫu AI.
- **Input**:
  ```text
  POST /api/ai-cards
  cardTemplateId=template-001
  templateType=handmade
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống chuyển yêu cầu sang phần tạo thiệp HandMade theo TDD-021.
  Phần tạo thiệp AI không đọc cấu hình AI, không gọi AI và không tạo cardType=ai.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu chọn sai phần xử lý, thiệp HandMade có thể bị tạo như thiệp AI và mang ảnh hoặc giá sai.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-01
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

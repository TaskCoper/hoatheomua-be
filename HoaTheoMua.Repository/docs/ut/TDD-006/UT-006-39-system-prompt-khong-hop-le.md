# UT-006-39: Từ chối hướng dẫn dành cho AI không có nội dung

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối hướng dẫn dành cho AI không có nội dung
- **Ghi chú**: Bản ghi hướng dẫn tồn tại nhưng nội dung rỗng nên AI không có chỉ dẫn đáng tin cậy để tạo thiệp.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-39
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
  - Khách hàng đã đăng nhập; sản phẩm nguồn, mẫu thiệp AI, kích thước chung và dữ liệu người dùng đều hợp lệ.
  - Phần tạo ảnh và việc lưu dữ liệu được theo dõi để xác nhận chúng không diễn ra khi thiếu hướng dẫn dành cho AI.
  - Có `SystemPrompt` (bản hướng dẫn dành cho AI) với type=card nhưng content chỉ chứa khoảng trắng.
- **Các trường hợp cần kiểm tra**:
  - Chỉ kiểm tra trường hợp nội dung hướng dẫn rỗng sau khi bỏ khoảng trắng thừa.
- **Input**:
  ```text
  POST /api/ai-cards
  SystemPrompt.type=card
  SystemPrompt.content=chuỗi chỉ có khoảng trắng
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã SYSTEM_PROMPT_INVALID.
  Hệ thống không gọi AI và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Gọi AI khi không có hướng dẫn có thể tạo kết quả sai mục đích. Dừng lại giúp lỗi cấu hình được sửa trước.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-12
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

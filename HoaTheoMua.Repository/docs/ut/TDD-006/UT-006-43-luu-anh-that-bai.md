# UT-006-43: Không tạo dữ liệu khi việc lưu ảnh thất bại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không tạo dữ liệu khi việc lưu ảnh thất bại
- **Ghi chú**: AI đã tạo đủ hai ảnh nhưng hệ thống không thể lưu một ảnh để dùng lâu dài.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-43
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
  - Khách hàng đã đăng nhập và toàn bộ thông tin nguồn, mẫu thiệp, kích thước, giá, lời chúc cùng hướng dẫn dành cho AI đều hợp lệ.
  - Hệ thống đã tạo một bản chụp đầy đủ về dữ liệu sẽ dùng để tạo thiệp.
  - Theo dõi hai bảng `generated_cards` và `client_histories` để xác nhận không có dữ liệu thiếu được giữ lại.
  - AIModule trả đủ hai ảnh. Việc lưu rawImage được thiết lập để báo lỗi.
- **Các trường hợp cần kiểm tra**:
  - Lỗi xảy ra trước khi thiệp và lịch sử được ghi nhận hoàn chỉnh.
- **Input**:
  ```text
  POST /api/ai-cards
  AI result=đủ hai ảnh
  Lưu rawImage=thất bại
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 500 với mã INTERNAL_SERVER_ERROR.
  Không có thiệp hoặc lịch sử mới được giữ lại.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Một địa chỉ ảnh chưa được lưu thành công có thể mất hoặc không truy cập được. Không tạo thiệp giúp tránh kết quả hỏng.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-16
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

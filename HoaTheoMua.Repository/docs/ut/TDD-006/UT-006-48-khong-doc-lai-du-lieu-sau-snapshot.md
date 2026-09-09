# UT-006-48: Giữ nguyên dữ liệu đã xác nhận trong suốt một lần tạo thiệp

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Giữ nguyên dữ liệu đã xác nhận trong suốt một lần tạo thiệp
- **Ghi chú**: Sau khi hệ thống chụp lại nguồn, mẫu, kích thước, giá và hướng dẫn, thay đổi bên ngoài không được làm yêu cầu đang chạy dùng lẫn dữ liệu mới.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-48
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: AI CARD - Tạo thiệp thiết kế AI
- **Unit under test (bắt buộc)**: `AICardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp AI mới)
- **Loại**: Determinism
- **Precondition / Mock setup**:
  - Khách hàng đã đăng nhập và toàn bộ thông tin nguồn, mẫu thiệp, kích thước, giá, lời chúc cùng hướng dẫn dành cho AI đều hợp lệ.
  - Hệ thống đã tạo một bản chụp đầy đủ về dữ liệu sẽ dùng để tạo thiệp.
  - Theo dõi hai bảng `generated_cards` và `client_histories` để xác nhận không có dữ liệu thiếu được giữ lại.
  - Bản chụp đã ghi basePrice=15000 và mẫu template-001.
  - Ngay sau khi có bản chụp, giá trong cơ sở dữ liệu đổi thành 20000 và mẫu bị tạm ngưng.
- **Các trường hợp cần kiểm tra**:
  - Thay đổi dữ liệu quản trị trong khi AI đang tạo ảnh.
- **Input**:
  ```text
  POST /api/ai-cards
  Snapshot.basePrice=15000
  Dữ liệu hiện tại đổi thành: basePrice=20000; mẫu thiệp bị tạm ngưng
  ```
- **Expected output (bắt buộc)**:
  ```text
  Lần tạo đang chạy tiếp tục dùng bản chụp có basePrice=15000 và không đọc lại mẫu hoặc giá.
  Nếu AI trả đủ ảnh và việc lưu thành công, thiệp có basePrice=15000. Yêu cầu mới về sau mới nhìn thấy thay đổi 20000 hoặc mẫu tạm ngưng.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu đọc lại giữa chừng, một thiệp có thể dùng mẫu cũ nhưng giá mới. Giữ nguyên bản chụp giúp toàn bộ kết quả của một lần tạo nhất quán.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-14
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

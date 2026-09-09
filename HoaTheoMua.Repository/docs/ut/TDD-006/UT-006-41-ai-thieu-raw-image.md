# UT-006-41: Không tạo dữ liệu khi AI thiếu ảnh gốc chưa chèn chữ

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không tạo dữ liệu khi AI thiếu ảnh gốc chưa chèn chữ
- **Ghi chú**: Phần tạo ảnh trả ảnh hoàn chỉnh nhưng thiếu rawImage, là ảnh cần cho việc thay nội dung về sau.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-41
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
  - AIModule trả rawImage=null và imageUrl=final.png.
- **Các trường hợp cần kiểm tra**:
  - Chỉ thiếu rawImage; imageUrl vẫn có giá trị.
- **Input**:
  ```text
  POST /api/ai-cards
  AI result: rawImage=null; imageUrl=final.png
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 500 với mã INTERNAL_SERVER_ERROR.
  Hệ thống không lưu thiệp hoặc lịch sử dù đã có ảnh hoàn chỉnh.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiếu ảnh gốc khiến thiệp không đáp ứng đủ dữ liệu đã cam kết và không thể phục vụ chức năng thay nội dung về sau.

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

# UT-006-40: Không tạo dữ liệu khi phần tạo ảnh AI báo lỗi

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không tạo dữ liệu khi phần tạo ảnh AI báo lỗi
- **Ghi chú**: Mọi thông tin đã hợp lệ nhưng phần tạo ảnh không thể hoàn thành yêu cầu.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-40
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
  - AIModule (phần tạo ảnh bằng trí tuệ nhân tạo) dừng và báo lỗi.
- **Các trường hợp cần kiểm tra**:
  - Kiểm tra một lần gọi AI thất bại trước khi có ảnh kết quả.
- **Input**:
  ```text
  POST /api/ai-cards
  AIModule.Generate(snapshot)=lỗi
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 500 với mã INTERNAL_SERVER_ERROR.
  Không có generated_cards hoặc client_histories mới. Chi tiết nội bộ được ghi cho người vận hành nhưng không hiển thị cho khách.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Lỗi tạo ảnh không phải do khách gây ra. Không lưu dữ liệu giúp khách không nhìn thấy một thiệp dở dang.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-15, BR-006-16
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

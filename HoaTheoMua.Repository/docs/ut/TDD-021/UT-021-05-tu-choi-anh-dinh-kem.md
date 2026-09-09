# UT-021-05: Từ chối ảnh đính kèm trong yêu cầu tạo thiệp HandMade

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối ảnh đính kèm trong yêu cầu tạo thiệp HandMade
- **Ghi chú**: Ảnh hiển thị của thiệp HandMade phải lấy từ mẫu có sẵn, không lấy từ ảnh do khách gửi.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-05
- **Phiên bản**: v1.1
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp mới)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Khách hàng đã đăng nhập và các thông tin tạo thiệp khác đều hợp lệ.
  - Theo dõi việc đọc, lưu ảnh và ghi dữ liệu để xác nhận không có bước nào tiếp tục.
- **Các trường hợp cần kiểm tra**:
  - Gửi trường `attachedImageUrl` với một địa chỉ ảnh. Trường này cũng phải bị từ chối nếu được gửi với giá trị rỗng hoặc null.
- **Input**:
  ```text
  attachedImageUrl = https://client.example.com/upload.png
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 400 với mã HANDMADE_CARD_ATTACHMENT_NOT_ALLOWED.
  Hệ thống không tải hoặc lưu ảnh, không đọc cấu hình, không tạo thiệp, không tạo lịch sử và không gọi AI.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Bài kiểm tra ngăn ảnh bên ngoài thay thế ảnh mẫu đã được quản lý, nhờ đó hình ảnh của thiệp luôn có nguồn rõ ràng.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-06
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

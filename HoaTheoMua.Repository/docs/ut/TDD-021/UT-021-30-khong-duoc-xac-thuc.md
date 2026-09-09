# UT-021-30: Từ chối người chưa đăng nhập hoặc tài khoản không còn hoạt động

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối người chưa đăng nhập hoặc tài khoản không còn hoạt động
- **Ghi chú**: Kiểm tra quyền tạo thiệp trước khi đọc bất kỳ dữ liệu kinh doanh nào.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-30
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
  - Thông tin tạo thiệp có thể hợp lệ, nhưng không có danh tính khách hàng được chấp nhận.
  - Theo dõi phần tạo thiệp để xác nhận nó không được gọi.
- **Các trường hợp cần kiểm tra**:
  - Không gửi thông tin đăng nhập; thông tin hết hạn hoặc không hợp lệ; tài khoản đã bị tắt.
- **Input**:
  ```text
  Danh tính khách hàng = không hợp lệ
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 401 với mã UNAUTHORIZED.
  Hệ thống không đọc mẫu thiệp, nguồn hoa hoặc cấu hình; không tạo dữ liệu mới và không gọi AI.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Dừng trước khi đọc dữ liệu vừa bảo vệ thông tin của khách hàng khác, vừa tránh tốn công xử lý cho người không có quyền sử dụng chức năng.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-01
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

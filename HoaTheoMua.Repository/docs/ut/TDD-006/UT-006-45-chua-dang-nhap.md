# UT-006-45: Từ chối người chưa đăng nhập hoặc tài khoản không hoạt động

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối người chưa đăng nhập hoặc tài khoản không hoạt động
- **Ghi chú**: Kiểm tra quyền tạo thiệp trước khi đọc dữ liệu sản phẩm và mẫu thiệp.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-45
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
  - Các thông tin tạo thiệp trong yêu cầu đều hợp lệ.
  - Không có khách hàng đã đăng nhập hoặc tài khoản hiện tại không còn hoạt động.
  - Theo dõi phần tạo thiệp để xác nhận nó không được gọi.
- **Các trường hợp cần kiểm tra**:
  - Không có thông tin đăng nhập hợp lệ.
- **Input**:
  ```text
  POST /api/ai-cards
  Danh tính khách hàng=không hợp lệ
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 401 (chưa được phép sử dụng) với mã UNAUTHORIZED.
  Hệ thống không đọc dữ liệu kinh doanh, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Dừng trước khi đọc dữ liệu vừa bảo vệ thông tin khách hàng, vừa tránh xử lý không cần thiết cho người không có quyền.

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

# UT-021-04: Từ chối hình thức chữ do phía người dùng tự gửi

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối hình thức chữ do phía người dùng tự gửi
- **Ghi chú**: Thiệp HandMade luôn dùng kiểu chữ viết tay do hệ thống quy định; phía người dùng không được tự thay đổi.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-04
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
  - Theo dõi việc đọc dữ liệu để xác nhận yêu cầu bị dừng trước khi tạo thiệp.
- **Các trường hợp cần kiểm tra**:
  - Gửi thêm trường `formType`. Kể cả giá trị là `calligraphy`, trường này vẫn không được phép xuất hiện.
- **Input**:
  ```text
  formType = calligraphy
  Các thông tin còn lại hợp lệ
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 400 với mã HANDMADE_CARD_FORM_TYPE_NOT_ALLOWED.
  Thông báo cho biết hình thức chữ của thiệp HandMade do hệ thống tự đặt.
  Hệ thống không đọc nguồn, cấu hình hoặc bảng giá; không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu cho phép phía người dùng gửi trường này, cùng một loại thiệp có thể mang nhiều hình thức chữ trái với quy định. Bài kiểm tra giữ một cách quyết định duy nhất.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-05
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

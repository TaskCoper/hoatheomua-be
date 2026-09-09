# UT-021-08: Từ chối mẫu thiệp đang tạm ngưng

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối mẫu thiệp đang tạm ngưng
- **Ghi chú**: Kiểm tra mẫu chưa bị xóa nhưng hiện không được quản trị viên cho phép tạo thiệp mới.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-08
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
  - Mẫu thiệp tồn tại với isDeleted = false và isActive = false.
  - Các thông tin còn lại đều hợp lệ.
- **Các trường hợp cần kiểm tra**:
  - Dùng mã của mẫu đang tạm ngưng.
- **Input**:
  ```text
  cardTemplateId trỏ tới mẫu có isDeleted = false, isActive = false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã CARD_TEMPLATE_INACTIVE.
  Hệ thống không đọc nguồn hoặc cấu hình; không tạo thiệp và không tạo lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Một mẫu còn được lưu không có nghĩa là đang được phép dùng. Bài kiểm tra bảo đảm quyết định tạm ngưng của quản trị viên được tôn trọng.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-10
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

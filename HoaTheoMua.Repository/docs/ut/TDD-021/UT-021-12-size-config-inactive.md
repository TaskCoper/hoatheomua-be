# UT-021-12: Từ chối cấu hình kích thước đang tạm ngưng

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối cấu hình kích thước đang tạm ngưng
- **Ghi chú**: Kiểm tra cấu hình còn tồn tại nhưng hiện chưa được phép sử dụng công khai.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-12
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
  - Nguồn hoa và mẫu thiệp hợp lệ.
  - Cấu hình kích thước có isDeleted = false và isPublic = false.
- **Các trường hợp cần kiểm tra**:
  - Dùng cấu hình chưa xóa nhưng đang bị tắt.
- **Input**:
  ```text
  card_config.sizes có isDeleted = false, isPublic = false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã CARD_SIZE_CONFIG_INACTIVE.
  Hệ thống không đọc nội dung kích thước, không đọc bảng giá và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Quản trị viên có thể tạm ngưng cấu hình khi đang điều chỉnh. Bài kiểm tra bảo đảm thiệp mới không dùng thông tin chưa sẵn sàng.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-11
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

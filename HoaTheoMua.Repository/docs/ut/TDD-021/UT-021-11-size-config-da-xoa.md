# UT-021-11: Từ chối cấu hình kích thước đã được đánh dấu là đã xóa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối cấu hình kích thước đã được đánh dấu là đã xóa
- **Ghi chú**: Kiểm tra cấu hình cũ vẫn còn để tra cứu nhưng không còn được phép dùng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-11
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
  - Cấu hình kích thước có isDeleted = true và isPublic = false.
- **Các trường hợp cần kiểm tra**:
  - Cấu hình đồng thời đã bị đánh dấu xóa và không được công khai để xác nhận lỗi đã xóa được ưu tiên.
- **Input**:
  ```text
  card_config.sizes có isDeleted = true, isPublic = false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 410 với mã CARD_SIZE_CONFIG_DELETED.
  Hệ thống không báo lỗi tạm ngưng, không đọc bảng giá và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Cấu hình đã xóa không còn giá trị sử dụng. Phân biệt rõ với tạm ngưng giúp quản trị viên hiểu đúng việc cần khôi phục hay tạo cấu hình mới.

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

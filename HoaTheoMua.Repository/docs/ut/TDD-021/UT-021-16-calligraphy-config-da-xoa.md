# UT-021-16: Từ chối bảng giá viết tay đã được đánh dấu là đã xóa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối bảng giá viết tay đã được đánh dấu là đã xóa
- **Ghi chú**: Kiểm tra bảng giá cũ không còn được dùng để tính tiền cho thiệp mới.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-16
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
  - Nguồn hoa, mẫu thiệp và kích thước chung đều hợp lệ.
  - Bảng giá có isDeleted = true và isPublic = false.
- **Các trường hợp cần kiểm tra**:
  - Bảng giá đồng thời đã bị đánh dấu xóa và bị tắt để xác nhận lỗi đã xóa được ưu tiên.
- **Input**:
  ```text
  card_config.prices có isDeleted = true, isPublic = false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 410 với mã CALLIGRAPHY_CONFIG_DELETED.
  Hệ thống không báo lỗi tạm ngưng, không đọc các mức giá và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Bảng giá đã xóa không còn hiệu lực. Dùng lại bảng giá này có thể khiến thiệp mới áp dụng mức tiền mà quản trị viên đã loại bỏ.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-12
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

# UT-021-17: Từ chối bảng giá viết tay đang tạm ngưng

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối bảng giá viết tay đang tạm ngưng
- **Ghi chú**: Kiểm tra bảng giá còn tồn tại nhưng chưa được phép dùng để tính tiền.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-17
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
  - Bảng giá có isDeleted = false và isPublic = false.
- **Các trường hợp cần kiểm tra**:
  - Dùng bảng giá chưa xóa nhưng đang bị tắt.
- **Input**:
  ```text
  card_config.prices có isDeleted = false, isPublic = false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã CALLIGRAPHY_CONFIG_INACTIVE.
  Hệ thống không đọc các mức giá và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Khi quản trị viên tạm tắt bảng giá, hệ thống phải dừng nhận thiệp mới để tránh báo một mức phí chưa được duyệt.

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

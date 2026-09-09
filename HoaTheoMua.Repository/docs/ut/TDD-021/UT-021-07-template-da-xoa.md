# UT-021-07: Từ chối mẫu thiệp đã được đánh dấu là đã xóa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối mẫu thiệp đã được đánh dấu là đã xóa
- **Ghi chú**: Kiểm tra mẫu vẫn còn bản ghi để tra cứu nhưng đã được quản trị viên loại khỏi sử dụng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-07
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
  - Mẫu thiệp tồn tại với isDeleted = true và isActive = false.
  - Các thông tin còn lại đều hợp lệ.
- **Các trường hợp cần kiểm tra**:
  - Mẫu đồng thời đã bị đánh dấu xóa và đang tạm ngưng để xác nhận lỗi đã xóa được ưu tiên.
- **Input**:
  ```text
  cardTemplateId trỏ tới mẫu có isDeleted = true, isActive = false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 410 với mã CARD_TEMPLATE_DELETED, không báo CARD_TEMPLATE_INACTIVE.
  Hệ thống không đọc nguồn hoặc cấu hình và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Đã xóa và tạm ngưng là hai tình trạng khác nhau. Giữ đúng thứ tự báo lỗi giúp phía sử dụng biết mẫu không còn được phép dùng, thay vì nghĩ rằng chỉ cần bật lại.

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

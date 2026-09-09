# UT-021-22: Từ chối sản phẩm hoa đã được đánh dấu là đã xóa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối sản phẩm hoa đã được đánh dấu là đã xóa
- **Ghi chú**: Kiểm tra sản phẩm cũ vẫn còn để tra cứu nhưng không được dùng cho thiệp mới.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-22
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
  - Sản phẩm tồn tại với isDeleted = true và isActive = false.
  - Các thông tin còn lại đều hợp lệ.
- **Các trường hợp cần kiểm tra**:
  - Sản phẩm đồng thời đã bị đánh dấu xóa và đang tạm ngưng để xác nhận lỗi đã xóa được ưu tiên.
- **Input**:
  ```text
  productId trỏ tới sản phẩm có isDeleted = true, isActive = false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 410 với mã PRODUCT_DELETED, không báo PRODUCT_INACTIVE.
  Hệ thống không kiểm tra số lượng còn lại và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Sản phẩm đã xóa không còn được phép tham gia giao dịch mới. Báo đúng tình trạng giúp tránh hiểu nhầm rằng chỉ cần bật sản phẩm lại.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-07
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

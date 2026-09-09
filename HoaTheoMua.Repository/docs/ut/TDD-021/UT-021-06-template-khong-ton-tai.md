# UT-021-06: Báo lỗi khi không tìm thấy mẫu thiệp

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi không tìm thấy mẫu thiệp
- **Ghi chú**: Kiểm tra mã mẫu có hình thức hợp lệ nhưng không khớp với mẫu nào đang lưu.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-06
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
  - Khách hàng đã đăng nhập và yêu cầu có đủ thông tin.
  - Khi tìm `template-missing`, nơi lưu dữ liệu báo không có mẫu thiệp tương ứng.
- **Các trường hợp cần kiểm tra**:
  - Dùng một mã mẫu không tồn tại, trong khi nguồn hoa và nội dung đều hợp lệ.
- **Input**:
  ```text
  cardTemplateId = template-missing
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 404 với mã CARD_TEMPLATE_NOT_FOUND.
  Hệ thống dừng trước khi đọc nguồn hoa, kích thước và bảng giá; không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Hệ thống phải tìm được mẫu trước để biết yêu cầu thuộc loại AI hay HandMade. Báo đúng lỗi giúp người dùng chọn lại mẫu thay vì nhận một lỗi chung chung.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-02, BR-021-10
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

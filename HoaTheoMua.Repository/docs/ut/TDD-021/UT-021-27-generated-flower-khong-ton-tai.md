# UT-021-27: Báo lỗi khi không tìm thấy mẫu hoa đã tạo

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi không tìm thấy mẫu hoa đã tạo
- **Ghi chú**: Kiểm tra mã mẫu hoa không tồn tại trong dữ liệu thuộc khách hàng hiện tại.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-27
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
  - Người gọi là `customer-01`.
  - Không có mẫu hoa mang mã `flower-missing` thuộc khách hàng này.
- **Các trường hợp cần kiểm tra**:
  - Chỉ gửi mã mẫu hoa không tồn tại, không gửi mã sản phẩm.
- **Input**:
  ```text
  generatedFlowerId = flower-missing
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 404 với mã GENERATED_FLOWER_NOT_FOUND.
  Hệ thống không đọc ảnh hoặc lịch sử nguồn gốc và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiệp không thể dựa trên một mẫu hoa không có thật. Báo một mã rõ ràng giúp phía sử dụng yêu cầu khách chọn lại nguồn.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-08
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

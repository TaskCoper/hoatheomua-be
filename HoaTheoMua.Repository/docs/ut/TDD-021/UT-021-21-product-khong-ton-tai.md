# UT-021-21: Báo lỗi khi không tìm thấy sản phẩm hoa nguồn

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi không tìm thấy sản phẩm hoa nguồn
- **Ghi chú**: Kiểm tra trường hợp khách chọn trực tiếp một mã sản phẩm không có trong hệ thống.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-21
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
  - Khách hàng và mẫu thiệp đều hợp lệ.
  - Khi tìm `product-missing`, nơi lưu dữ liệu báo không có sản phẩm tương ứng.
- **Các trường hợp cần kiểm tra**:
  - Chỉ gửi mã sản phẩm không tồn tại, không gửi mã mẫu hoa đã tạo.
- **Input**:
  ```text
  productId = product-missing
  generatedFlowerId = không có
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 404 với mã PRODUCT_NOT_FOUND.
  Hệ thống không tạo thiệp hoặc lịch sử và không gọi AI.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiệp phải gắn với một sản phẩm có thật. Nếu vẫn cho tạo, lịch sử sẽ chứa một mã nguồn không thể đối chiếu.

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

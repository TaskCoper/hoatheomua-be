# UT-021-25: Từ chối sản phẩm hoa thiếu thông tin để bán

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối sản phẩm hoa thiếu thông tin để bán
- **Ghi chú**: Một sản phẩm có thể đang hoạt động nhưng vẫn không bán được nếu thiếu công thức hoa hoặc phần lõi cần thiết.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-25
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
  - Sản phẩm tồn tại, đang hoạt động và chưa bị xóa.
  - Lần kiểm tra thứ nhất sản phẩm thiếu công thức hoa. Lần thứ hai sản phẩm thiếu phần lõi cần thiết.
- **Các trường hợp cần kiểm tra**:
  - Chạy riêng trường hợp `no_formula` và `no_core`; cả hai đều có nghĩa sản phẩm chưa đủ thành phần để bán.
- **Input**:
  ```text
  Trường hợp 1: productState = no_formula
  Trường hợp 2: productState = no_core
  ```
- **Expected output (bắt buộc)**:
  ```text
  Mỗi trường hợp đều báo HTTP 422 với mã PRODUCT_NOT_SELLABLE.
  Hệ thống không báo hết hàng và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Chỉ kiểm tra trạng thái hoạt động và số lượng là chưa đủ. Bài kiểm tra ngăn tạo thiệp cho một sản phẩm chưa có đủ thông tin để trở thành món hàng hoàn chỉnh.

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

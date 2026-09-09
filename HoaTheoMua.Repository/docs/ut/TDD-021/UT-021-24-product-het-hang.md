# UT-021-24: Từ chối sản phẩm hoa đã hết hàng

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối sản phẩm hoa đã hết hàng
- **Ghi chú**: Kiểm tra ranh giới khi số lượng có thể bán bằng đúng 0.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-24
- **Phiên bản**: v1.1
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp mới)
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Sản phẩm tồn tại, đang hoạt động, chưa bị xóa và có đủ thông tin để bán.
  - Phần tính số lượng có thể bán trả về 0.
- **Các trường hợp cần kiểm tra**:
  - Dùng đúng giá trị 0 vì đây là điểm đầu tiên sản phẩm không còn đơn vị nào để bán.
- **Input**:
  ```text
  sellableQuantity = 0
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã PRODUCT_OUT_OF_STOCK.
  Hệ thống không nhầm tình trạng hết hàng với lỗi không kiểm tra được số lượng và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu vẫn tạo thiệp cho sản phẩm hết hàng, khách có thể chuẩn bị một thiệp không thể đi cùng sản phẩm đã chọn. Kiểm tra tại mốc 0 giúp bảo vệ đúng ranh giới.

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

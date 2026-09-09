# UT-021-23: Từ chối sản phẩm hoa đang tạm ngưng bán

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối sản phẩm hoa đang tạm ngưng bán
- **Ghi chú**: Kiểm tra sản phẩm vẫn tồn tại nhưng hiện không được phép bán.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-23
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
  - Sản phẩm có isDeleted = false và isActive = false.
  - Các thông tin còn lại đều hợp lệ.
- **Các trường hợp cần kiểm tra**:
  - Dùng mã sản phẩm đang tạm ngưng bán.
- **Input**:
  ```text
  productId trỏ tới sản phẩm có isDeleted = false, isActive = false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã PRODUCT_INACTIVE.
  Hệ thống không kiểm tra số lượng còn lại và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Sản phẩm còn trong hệ thống không đồng nghĩa đang được bán. Bài kiểm tra bảo đảm quyết định tạm ngưng được áp dụng cho mọi thiệp mới.

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

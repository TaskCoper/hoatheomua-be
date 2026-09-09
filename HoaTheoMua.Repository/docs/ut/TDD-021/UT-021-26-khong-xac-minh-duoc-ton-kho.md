# UT-021-26: Báo lỗi khi không xác minh được số lượng có thể bán

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi không xác minh được số lượng có thể bán
- **Ghi chú**: Kiểm tra trường hợp phần tính số lượng còn lại không thể đưa ra kết quả đáng tin cậy.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-26
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
  - Sản phẩm tồn tại, đang hoạt động, chưa bị xóa và có đủ thông tin để bán.
  - Phần tính số lượng được thiết lập để không trả được kết quả do quá thời gian chờ.
- **Các trường hợp cần kiểm tra**:
  - Quá thời gian chờ là trường hợp chính. Mất kết nối hoặc nhận dữ liệu không đọc được cũng phải cho cùng kết quả bên ngoài.
- **Input**:
  ```text
  Kiểm tra số lượng có thể bán = thất bại
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 503 với mã PRODUCT_AVAILABILITY_UNAVAILABLE.
  Hệ thống không tự cho rằng sản phẩm còn hàng, không báo nhầm là hết hàng và không tạo dữ liệu mới.
  Chi tiết lỗi được ghi lại cho người vận hành nhưng không hiển thị cho khách.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Khi chưa biết chắc còn hàng, tiếp tục tạo thiệp là dựa trên phỏng đoán. Dừng lại giúp tránh nhận yêu cầu cho sản phẩm có thể không cung cấp được.

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

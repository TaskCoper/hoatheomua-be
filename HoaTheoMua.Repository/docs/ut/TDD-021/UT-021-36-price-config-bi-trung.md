# UT-021-36: Từ chối khi có nhiều bảng giá viết tay cùng loại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối khi có nhiều bảng giá viết tay cùng loại
- **Ghi chú**: Kiểm tra hệ thống không tự chọn một bảng giá khi tìm thấy nhiều bản chưa bị xóa.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-36
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
  - Khách hàng, nguồn hoa, mẫu thiệp và kích thước chung đều hợp lệ.
  - Có hai bản ghi cùng group = card_config và key = card_config.prices; cả hai chưa bị xóa.
- **Các trường hợp cần kiểm tra**:
  - Một bảng giá được bật và một bảng bị tắt vẫn được xem là trùng, vì cả hai đều chưa bị xóa.
- **Input**:
  ```text
  Số bản ghi card_config.prices chưa bị xóa = 2
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã CARD_CONFIG_AMBIGUOUS.
  Hệ thống không chọn bảng giá theo thứ tự bất kỳ, không tính totalPrice và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Hai bảng có thể cho hai số tiền khác nhau. Từ chối rõ ràng giúp tránh việc cùng một lời chúc bị tính giá khác nhau giữa các lần tạo.

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

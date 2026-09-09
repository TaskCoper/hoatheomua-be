# UT-021-20: Từ chối khi có nhiều cấu hình kích thước cùng loại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối khi có nhiều cấu hình kích thước cùng loại
- **Ghi chú**: Kiểm tra hệ thống không tự chọn một cấu hình khi tìm thấy nhiều bản chưa bị xóa.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-20
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
  - Nguồn hoa và mẫu thiệp đều hợp lệ.
  - Có hai bản ghi cùng group = card_config và key = card_config.sizes; cả hai chưa bị xóa.
- **Các trường hợp cần kiểm tra**:
  - Một bản được bật và một bản bị tắt vẫn được xem là trùng, vì cả hai đều chưa bị xóa.
- **Input**:
  ```text
  Số bản ghi card_config.sizes chưa bị xóa = 2
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã CARD_CONFIG_AMBIGUOUS.
  Hệ thống không tự chọn bản đầu tiên, không tính giá và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Hai cấu hình có thể chứa giới hạn khác nhau. Nếu chọn theo thứ tự bất kỳ, cùng một yêu cầu có thể cho kết quả khác nhau giữa các lần chạy.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-11
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

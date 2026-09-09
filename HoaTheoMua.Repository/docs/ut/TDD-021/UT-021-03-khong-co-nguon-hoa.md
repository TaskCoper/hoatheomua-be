# UT-021-03: Từ chối yêu cầu không có nguồn hoa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối yêu cầu không có nguồn hoa
- **Ghi chú**: Kiểm tra yêu cầu thiếu cả sản phẩm hoa lẫn mẫu hoa đã tạo.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-03
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
  - Khách hàng đã đăng nhập.
  - Các thông tin về mẫu thiệp, người gửi, người nhận và lời chúc đều hợp lệ.
  - Theo dõi các phần đọc và ghi dữ liệu để xác nhận hệ thống dừng sớm.
- **Các trường hợp cần kiểm tra**:
  - Không gửi `productId` và cũng không gửi `generatedFlowerId`.
- **Input**:
  ```text
  productId = không có
  generatedFlowerId = không có
  Các thông tin còn lại hợp lệ
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 400 với mã VALIDATION_ERROR và giải thích rằng phải chọn đúng một nguồn hoa.
  Hệ thống không đọc mẫu thiệp, nguồn hoa hoặc bảng giá; không tạo thiệp và không tạo lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Không có nguồn hoa thì hệ thống không biết thiệp thuộc bó hoa nào. Dừng ngay từ đầu giúp tránh tạo một thiệp không thể truy lại nguồn.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-03
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

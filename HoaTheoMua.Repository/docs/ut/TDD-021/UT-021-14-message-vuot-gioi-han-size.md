# UT-021-14: Từ chối lời chúc vượt giới hạn của kích thước chung

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối lời chúc vượt giới hạn của kích thước chung
- **Ghi chú**: Khách không chọn kích thước. Hệ thống tự dùng kích thước duy nhất và phải giữ đúng giới hạn số từ của kích thước đó.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-14
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
  - Khách hàng, nguồn hoa và mẫu thiệp đều hợp lệ.
  - Kích thước chung cho phép tối đa 5 từ. Bảng giá viết tay hợp lệ.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc có đúng 6 từ, tức nhiều hơn giới hạn một từ.
- **Input**:
  ```text
  messageContent = một hai ba bốn năm sáu
  max_words của kích thước chung = 5
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 400 với mã VALIDATION_ERROR và nêu lời chúc có 6 từ, vượt giới hạn 5 từ.
  Hệ thống không tự tìm kích thước khác, không đọc bảng giá và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Kiểm tra ngay sát giới hạn giúp phát hiện lỗi đếm từ hoặc so sánh sai. Nếu bỏ qua, cửa hàng có thể nhận nội dung dài hơn phần thiệp có thể viết.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-04, BR-021-11, BR-021-14
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

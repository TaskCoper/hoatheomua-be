# UT-021-19: Báo lỗi khi số từ không thuộc mức giá nào

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi số từ không thuộc mức giá nào
- **Ghi chú**: Kiểm tra hệ thống không tự chọn mức giá gần nhất nếu bảng giá để hở một khoảng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-19
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
  - Kích thước chung cho phép ít nhất 100 từ.
  - Bảng giá có hai khoảng 0 đến 35 từ và 51 đến 100 từ.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc có đúng 40 từ, nằm trong khoảng chưa có giá từ 36 đến 50.
- **Input**:
  ```text
  wordCount = 40
  Các mức giá = 0..35 và 51..100
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 422 với mã CALLIGRAPHY_PRICE_RULE_NOT_FOUND.
  Hệ thống không làm tròn số từ, không chọn mức liền trước hoặc liền sau và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Chọn giá gần nhất là một quyết định không có trong quy định và có thể làm khách bị thu sai. Bài kiểm tra buộc người quản trị sửa khoảng giá bị thiếu.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-13
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

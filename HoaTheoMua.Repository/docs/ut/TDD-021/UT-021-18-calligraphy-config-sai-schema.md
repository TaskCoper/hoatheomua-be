# UT-021-18: Từ chối bảng giá viết tay có cấu trúc sai

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối bảng giá viết tay có cấu trúc sai
- **Ghi chú**: Kiểm tra bảng giá tồn tại nhưng không thể dùng để xác định duy nhất một mức phí.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-18
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
  - Nguồn hoa, mẫu thiệp, kích thước chung và trạng thái bảng giá đều hợp lệ.
  - Nội dung bảng giá được thay lần lượt bằng từng trường hợp sai được nêu bên dưới.
- **Các trường hợp cần kiểm tra**:
  - Nội dung không đọc được; thiếu số từ bắt đầu, số từ kết thúc hoặc số tiền; có số âm; điểm bắt đầu lớn hơn điểm kết thúc.
  - Hai khoảng giá bị chồng lên nhau hoặc không nối tiếp nhau cũng được xem là cấu hình sai.
- **Input**:
  ```text
  card_config.prices.value = nội dung sai cấu trúc
  Các thông tin tạo thiệp khác hợp lệ
  ```
- **Expected output (bắt buộc)**:
  ```text
  Với từng trường hợp, hệ thống báo HTTP 409 với mã CALLIGRAPHY_CONFIG_INVALID.
  Hệ thống không chọn mức giá, không tính tiền và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Bảng giá mơ hồ có thể khiến cùng một lời chúc nhận hai mức phí hoặc không có mức phí. Kiểm tra cấu trúc trước khi tính tiền giúp kết quả luôn rõ ràng.

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

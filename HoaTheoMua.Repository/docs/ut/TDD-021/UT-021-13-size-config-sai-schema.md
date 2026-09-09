# UT-021-13: Từ chối cấu hình kích thước có cấu trúc sai

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối cấu hình kích thước có cấu trúc sai
- **Ghi chú**: Kiểm tra cấu hình tồn tại nhưng nội dung không đủ rõ để lấy ra một kích thước chung an toàn.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-13
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
  - Nguồn hoa, mẫu thiệp và trạng thái của cấu hình kích thước đều hợp lệ.
  - Nội dung cấu hình được thay lần lượt bằng từng trường hợp sai được nêu bên dưới.
- **Các trường hợp cần kiểm tra**:
  - Nội dung không đọc được; danh sách kích thước không phải danh sách; hoặc kích thước thiếu tên, chiều rộng, chiều cao, giá ghi nhận hay giới hạn từ.
  - Danh sách không có kích thước hoặc có nhiều hơn một kích thước cũng phải nhận cùng mã lỗi.
- **Input**:
  ```text
  card_config.sizes.value = nội dung sai cấu trúc
  Các thông tin tạo thiệp khác hợp lệ
  ```
- **Expected output (bắt buộc)**:
  ```text
  Với từng trường hợp, hệ thống báo HTTP 409 với mã CARD_SIZE_CONFIG_INVALID.
  Thông báo xác định lỗi nằm ở cấu hình hệ thống, không đổ lỗi cho khách hàng.
  Hệ thống không đọc bảng giá và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Một cấu hình hỏng có thể làm sai kích thước hoặc cho phép lời chúc quá dài. Kiểm tra đầy đủ giúp hệ thống chỉ tạo thiệp khi có đúng một kích thước đáng tin cậy.

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

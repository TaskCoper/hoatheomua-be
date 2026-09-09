# UT-021-10: Báo lỗi khi chưa có cấu hình kích thước chung

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi chưa có cấu hình kích thước chung
- **Ghi chú**: Thiệp cần một kích thước chung để biết giới hạn lời chúc và lưu thông tin kích thước.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-10
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
  - Khách hàng, nguồn hoa và mẫu thiệp HandMade đều hợp lệ.
  - Khi tìm cấu hình `card_config.sizes`, nơi lưu dữ liệu báo không có bản ghi.
- **Các trường hợp cần kiểm tra**:
  - Không có cấu hình kích thước chung nào được thiết lập.
- **Input**:
  ```text
  Yêu cầu tạo thiệp hợp lệ; hệ thống không tìm thấy card_config.sizes
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 404 với mã CARD_SIZE_CONFIG_NOT_FOUND.
  Hệ thống không đọc bảng giá viết tay và không tạo thiệp hoặc lịch sử.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Không có kích thước thì hệ thống không thể biết lời chúc có vừa hay không. Dừng lại an toàn hơn việc tự đoán một kích thước hoặc giới hạn.

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

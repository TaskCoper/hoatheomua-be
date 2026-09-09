# UT-021-34: Không tiết lộ mẫu hoa thuộc khách hàng khác

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không tiết lộ mẫu hoa thuộc khách hàng khác
- **Ghi chú**: Mẫu hoa có tồn tại nhưng không thuộc người đang gửi yêu cầu; phản hồi phải giống như khi không tìm thấy.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-34
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
  - Người gọi là `customer-01`.
  - Mẫu hoa `flower-02` tồn tại nhưng thuộc `customer-02`.
- **Các trường hợp cần kiểm tra**:
  - Người gọi gửi mã mẫu hoa của một tài khoản khác.
- **Input**:
  ```text
  currentCustomer = customer-01
  flower-02.owner = customer-02
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 404 với mã GENERATED_FLOWER_NOT_FOUND.
  Phản hồi không cho biết flower-02 thật sự tồn tại.
  Hệ thống không đọc ảnh hoặc lịch sử của flower-02 và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu trả một lỗi khác, người xấu có thể thử nhiều mã để biết mẫu hoa nào thuộc tài khoản khác. Phản hồi giống nhau giúp bảo vệ thông tin riêng tư.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-08
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

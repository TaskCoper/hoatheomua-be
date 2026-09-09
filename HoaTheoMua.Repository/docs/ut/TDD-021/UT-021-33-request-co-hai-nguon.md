# UT-021-33: Từ chối yêu cầu có đồng thời hai nguồn hoa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối yêu cầu có đồng thời hai nguồn hoa
- **Ghi chú**: Mỗi thiệp chỉ được có một nguồn trực tiếp để lịch sử không mang hai cách hiểu.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-33
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
  - Sản phẩm `product-01` và mẫu hoa `flower-01` đều tồn tại.
  - Theo dõi việc đọc dữ liệu để xác nhận hệ thống dừng sớm.
- **Các trường hợp cần kiểm tra**:
  - Gửi cùng lúc `productId` và `generatedFlowerId`.
- **Input**:
  ```text
  productId = product-01
  generatedFlowerId = flower-01
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 400 với mã VALIDATION_ERROR và giải thích rằng chỉ được chọn một nguồn.
  Hệ thống không tự chọn một trong hai nguồn, không đọc mẫu hoặc cấu hình và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu hệ thống tự chọn một nguồn, base_id có thể thay đổi theo cách người dùng không biết. Bài kiểm tra giữ lịch sử nguồn gốc rõ ràng và duy nhất.

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

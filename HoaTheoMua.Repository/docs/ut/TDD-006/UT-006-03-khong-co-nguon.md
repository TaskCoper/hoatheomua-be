# UT-006-03: Từ chối yêu cầu không có nguồn hoa

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối yêu cầu không có nguồn hoa
- **Ghi chú**: Kiểm tra yêu cầu thiếu cả sản phẩm lẫn mẫu hoa đã tạo.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-03
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: AI CARD - Tạo thiệp thiết kế AI
- **Unit under test (bắt buộc)**: `AICardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp AI mới)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Khách hàng `customer-01` đã đăng nhập và chọn sản phẩm `product-001` đang bán được.
  - Kích thước chung, bảng giá viết tay và hướng dẫn dành cho AI đều đang dùng được.
  - Phần tạo ảnh và việc lưu dữ liệu được theo dõi để xác nhận chúng không diễn ra khi mẫu thiệp bị từ chối.
- **Các trường hợp cần kiểm tra**:
  - Không gửi productId và cũng không gửi generatedFlowerId.
- **Input**:
  ```text
  POST /api/ai-cards
  productId=không có
  generatedFlowerId=không có
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 400 với mã VALIDATION_ERROR và giải thích rằng phải chọn đúng một nguồn hoa.
  Hệ thống không đọc nguồn hoặc cấu hình, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Không có nguồn thì hệ thống không biết thiệp thuộc bó hoa nào. Dừng sớm giúp tránh tạo một thiệp không thể truy lại nguồn.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-02
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

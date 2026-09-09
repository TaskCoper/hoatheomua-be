# UT-006-14: Từ chối mẫu hoa đã tạo nhưng thiếu ảnh dùng được

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối mẫu hoa đã tạo nhưng thiếu ảnh dùng được
- **Ghi chú**: Mẫu hoa thuộc đúng khách hàng nhưng chưa có hình ảnh hoàn chỉnh để dùng làm nguồn.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-14
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
  - Khách hàng `customer-01` đã đăng nhập và tài khoản đang hoạt động.
  - Mẫu thiệp AI, kích thước chung, bảng giá viết tay và hướng dẫn dành cho AI đều đang dùng được.
  - Phần tạo ảnh và việc lưu dữ liệu được theo dõi để xác nhận chúng không diễn ra khi mẫu hoa bị từ chối.
  - flower-001 thuộc customer-01 nhưng imageUrl=null.
- **Các trường hợp cần kiểm tra**:
  - Địa chỉ ảnh null là trường hợp cụ thể được kiểm tra.
- **Input**:
  ```text
  POST /api/ai-cards
  generatedFlowerId=flower-001
  flower-001.imageUrl=null
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã GENERATED_FLOWER_INPUT_INVALID.
  Hệ thống không đọc lịch sử nguồn gốc, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Có bản ghi chưa có nghĩa là mẫu hoa đã hoàn chỉnh. Ảnh dùng được là thông tin tối thiểu để tạo thiệp từ nguồn này.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-04
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

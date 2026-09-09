# UT-006-15: Từ chối mẫu hoa không có lịch sử nguồn gốc

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối mẫu hoa không có lịch sử nguồn gốc
- **Ghi chú**: Mẫu hoa hợp lệ nhưng không có bản ghi cho biết nó bắt đầu từ sản phẩm nào.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-15
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
  - flower-001 thuộc customer-01 và có ảnh dùng được.
  - Không có lịch sử type=flower với output_id=flower-001.
- **Các trường hợp cần kiểm tra**:
  - Chỉ kiểm tra trường hợp thiếu hoàn toàn lịch sử nguồn gốc.
- **Input**:
  ```text
  POST /api/ai-cards
  generatedFlowerId=flower-001
  Flower History=không có
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã SOURCE_PRODUCT_PROVENANCE_INVALID.
  Hệ thống không đoán sản phẩm từ metadata, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Không có lịch sử thì hệ thống không thể chứng minh sản phẩm gốc. Đoán bù có thể gắn thiệp với sai sản phẩm.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-04, BR-006-05
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

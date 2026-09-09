# UT-006-16: Từ chối khi thông tin sản phẩm gốc của mẫu hoa mâu thuẫn

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối khi thông tin sản phẩm gốc của mẫu hoa mâu thuẫn
- **Ghi chú**: Lịch sử tồn tại nhưng base_id và root không cùng chỉ về một sản phẩm gốc.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-16
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
  - Lịch sử có type=flower, output_id=flower-001, base_id=product-001 nhưng root=product-002.
- **Các trường hợp cần kiểm tra**:
  - Kiểm tra một lịch sử có hai mã sản phẩm gốc khác nhau.
- **Input**:
  ```text
  POST /api/ai-cards
  generatedFlowerId=flower-001
  base_id=product-001; root=product-002
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã SOURCE_PRODUCT_PROVENANCE_INVALID.
  Hệ thống không chọn một trong hai mã, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Hai mã gốc khác nhau khiến lịch sử không còn đáng tin cậy. Dừng lại tránh gắn thiệp với sản phẩm được chọn tùy ý.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-05
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

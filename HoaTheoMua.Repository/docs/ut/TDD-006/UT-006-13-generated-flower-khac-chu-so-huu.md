# UT-006-13: Không tiết lộ mẫu hoa thuộc khách hàng khác

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không tiết lộ mẫu hoa thuộc khách hàng khác
- **Ghi chú**: Mẫu hoa có tồn tại nhưng không thuộc người đang gửi yêu cầu.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-13
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
  - Người gọi là customer-01; flower-002 tồn tại nhưng thuộc customer-02.
- **Các trường hợp cần kiểm tra**:
  - customer-01 gửi mã flower-002.
- **Input**:
  ```text
  POST /api/ai-cards
  currentCustomer=customer-01
  flower-002.owner=customer-02
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 404 với mã GENERATED_FLOWER_NOT_FOUND, giống trường hợp mẫu hoa không tồn tại.
  Phản hồi không tiết lộ flower-002 tồn tại; hệ thống không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Phản hồi giống nhau ngăn người xấu thử nhiều mã để biết mẫu hoa nào thuộc tài khoản khác.

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

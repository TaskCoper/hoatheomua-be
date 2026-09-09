# UT-006-21: Báo lỗi khi chưa có cấu hình kích thước chung

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi chưa có cấu hình kích thước chung
- **Ghi chú**: Thiệp cần kích thước chung để biết giới hạn lời chúc và giá cơ bản.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-21
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
  - Khách hàng đã đăng nhập; sản phẩm nguồn và mẫu thiệp AI đều hợp lệ.
  - Bảng giá viết tay và hướng dẫn dành cho AI đang dùng được.
  - Phần tạo ảnh và việc lưu dữ liệu được theo dõi để xác nhận chúng không diễn ra khi cấu hình kích thước bị từ chối.
  - Không có bản ghi group=card_config, key=card_config.sizes.
- **Các trường hợp cần kiểm tra**:
  - Không có cấu hình kích thước nào được thiết lập.
- **Input**:
  ```text
  POST /api/ai-cards
  card_config.sizes=không có
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 404 với mã CARD_SIZE_CONFIG_NOT_FOUND.
  Hệ thống không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Không có kích thước thì hệ thống không thể biết lời chúc có vừa hay tính giá cơ bản bao nhiêu.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-07
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

# UT-006-35: Từ chối bảng giá viết tay đang tạm ngưng

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối bảng giá viết tay đang tạm ngưng
- **Ghi chú**: Bảng giá còn tồn tại nhưng hiện chưa được phép dùng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-35
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
  - Khách hàng đã đăng nhập; sản phẩm nguồn, mẫu thiệp AI và kích thước chung đều hợp lệ.
  - Hình thức chữ là `calligraphy` (viết tay), nên hệ thống bắt buộc phải đọc bảng giá viết tay.
  - Hướng dẫn dành cho AI đang dùng được. Phần tạo ảnh và việc lưu dữ liệu được theo dõi.
  - card_config.prices có isDeleted=false và isPublic=false.
- **Các trường hợp cần kiểm tra**:
  - Dùng bảng giá chưa xóa nhưng đang bị tắt.
- **Input**:
  ```text
  POST /api/ai-cards
  card_config.prices: isDeleted=false; isPublic=false
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã CALLIGRAPHY_CONFIG_INACTIVE.
  Hệ thống không đọc các mức giá, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Khi bảng giá đang được điều chỉnh, hệ thống phải dừng nhận thiệp viết tay để tránh báo mức phí chưa được duyệt.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-09
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

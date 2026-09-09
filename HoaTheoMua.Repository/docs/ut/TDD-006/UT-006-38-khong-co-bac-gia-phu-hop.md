# UT-006-38: Báo lỗi khi số từ không thuộc mức giá nào

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Báo lỗi khi số từ không thuộc mức giá nào
- **Ghi chú**: Bảng giá không có khoảng chứa số từ của lời chúc.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-38
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
  - Bảng giá có hai khoảng 0-35 từ và 51-100 từ.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc có 40 từ, nằm trong khoảng chưa có giá từ 36 đến 50.
- **Input**:
  ```text
  POST /api/ai-cards
  wordCount=40
  price ranges=0..35 và 51..100
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 422 với mã CALLIGRAPHY_PRICE_RULE_NOT_FOUND.
  Hệ thống không chọn mức gần nhất, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Chọn giá gần nhất là quyết định không có trong quy định và có thể thu sai tiền. Bài kiểm tra buộc người quản trị sửa khoảng giá bị thiếu.

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

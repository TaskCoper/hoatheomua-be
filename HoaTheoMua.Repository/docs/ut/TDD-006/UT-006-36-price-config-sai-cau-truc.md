# UT-006-36: Từ chối bảng giá viết tay có các khoảng chồng nhau

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối bảng giá viết tay có các khoảng chồng nhau
- **Ghi chú**: Hai khoảng cùng chứa một số từ khiến hệ thống không biết phải dùng mức tiền nào.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-36
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
  - Bảng giá đang hoạt động và có hai khoảng 0-40 từ và 36-70 từ.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc có 38 từ nên đồng thời nằm trong cả hai khoảng.
- **Input**:
  ```text
  POST /api/ai-cards
  price ranges=0..40 và 36..70
  wordCount=38
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 409 với mã CALLIGRAPHY_CONFIG_INVALID.
  Hệ thống không chọn một mức giá theo thứ tự bất kỳ, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Hai mức giá cùng áp dụng có thể làm số tiền thay đổi theo thứ tự đọc. Từ chối buộc bảng giá phải rõ ràng.

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

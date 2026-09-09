# UT-006-44: Xóa bỏ toàn bộ kết quả tạm khi lưu thiệp và lịch sử thất bại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Xóa bỏ toàn bộ kết quả tạm khi lưu thiệp và lịch sử thất bại
- **Ghi chú**: Kiểm tra không để lại thiệp không có lịch sử nếu lỗi xảy ra giữa lần lưu cuối cùng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-44
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
  - Khách hàng đã đăng nhập và toàn bộ bước kiểm tra trước khi tạo ảnh đều thành công.
  - Phần tạo ảnh trả đủ `rawImage=raw.png` và `imageUrl=final.png`.
  - Thiệp và lịch sử phải được lưu trong cùng một lần: hoặc giữ cả hai, hoặc không giữ bản nào.
  - Thiệp được ghi tạm nhưng bước ghi lịch sử được thiết lập để báo lỗi.
- **Các trường hợp cần kiểm tra**:
  - Lỗi ở lịch sử được chọn vì nó xảy ra sau khi thiệp đã được ghi tạm.
- **Input**:
  ```text
  POST /api/ai-cards
  Lưu generated_cards=thành công tạm thời
  Lưu client_histories=thất bại
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo lỗi HTTP 500 với mã INTERNAL_SERVER_ERROR.
  Khi lần lưu kết thúc, không còn thiệp mới và cũng không có lịch sử mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Thiệp và lịch sử là một cặp không thể tách rời. Chỉ giữ một bên sẽ làm mất nguồn gốc hoặc khiến dữ liệu khách nhìn thấy không đầy đủ.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-16, BR-006-17
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

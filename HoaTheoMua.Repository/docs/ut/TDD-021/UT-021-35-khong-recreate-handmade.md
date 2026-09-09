# UT-021-35: Không cho thay nội dung bằng luồng AI đối với thiệp HandMade

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không cho thay nội dung bằng luồng AI đối với thiệp HandMade
- **Ghi chú**: Chức năng thay nội dung bằng AI chỉ dành cho thiệp AI; thiệp HandMade không có ảnh thiết kế gốc để dùng cho việc này.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-35
- **Phiên bản**: v1.1
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.RecreateContentAsync` (phần chương trình tạo lại nội dung của một thiệp đã có)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Khách hàng sở hữu thiệp có card_type = handmade và raw_image = null.
  - Lịch sử tương ứng có type = handmade_card và output_id bằng mã thiệp.
- **Các trường hợp cần kiểm tra**:
  - Gửi yêu cầu thay nội dung cho một thiệp HandMade hợp lệ.
- **Input**:
  ```text
  POST /api/ai-cards/card-handmade-01/recreate-content
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã HANDMADE_CARD_RECREATE_NOT_SUPPORTED.
  Hệ thống nhận ra loại HandMade trước khi tìm ảnh thiết kế gốc.
  Hệ thống không đọc cấu hình dành cho AI, không gọi AI và không tạo dữ liệu mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Chặn theo loại thiệp giúp người dùng nhận đúng lý do, thay vì một lỗi gây hiểu lầm rằng thiệp chỉ đang thiếu ảnh. Đồng thời nó ngăn HandMade đi vào chức năng chỉ dành cho AI.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-23
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-037
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

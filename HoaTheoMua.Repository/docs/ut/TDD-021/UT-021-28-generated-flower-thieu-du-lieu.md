# UT-021-28: Từ chối mẫu hoa đã tạo nhưng không có ảnh dùng được

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối mẫu hoa đã tạo nhưng không có ảnh dùng được
- **Ghi chú**: Mẫu hoa thuộc đúng khách hàng nhưng thiếu ảnh cần thiết để xác nhận đây là một kết quả hoàn chỉnh.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-28
- **Phiên bản**: v1.1
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp mới)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Mẫu hoa tồn tại và thuộc khách hàng hiện tại.
  - Địa chỉ ảnh của mẫu hoa lần lượt là null, chuỗi rỗng, chỉ có khoảng trắng hoặc sai hình thức địa chỉ.
- **Các trường hợp cần kiểm tra**:
  - Chạy riêng từng dạng địa chỉ ảnh không dùng được; tất cả đều thể hiện mẫu hoa chưa hoàn chỉnh.
- **Input**:
  ```text
  generatedFlowerId trỏ tới mẫu hoa có imageUrl không dùng được
  ```
- **Expected output (bắt buộc)**:
  ```text
  Mỗi trường hợp đều báo HTTP 409 với mã GENERATED_FLOWER_INPUT_INVALID.
  Hệ thống không đọc tiếp lịch sử nguồn gốc và không tạo thiệp hoặc lịch sử mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Có một bản ghi chưa có nghĩa là mẫu hoa đã sẵn sàng. Ảnh dùng được là bằng chứng tối thiểu để nguồn hoa có thể được hiển thị và đối chiếu.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-08
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

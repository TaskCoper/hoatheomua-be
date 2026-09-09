# UT-021-32: Không cho tạo lại thiết kế đối với thiệp HandMade

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không cho tạo lại thiết kế đối với thiệp HandMade
- **Ghi chú**: Thiệp HandMade dùng ảnh mẫu có sẵn, không có thiết kế do AI tạo để sinh lại.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-32
- **Phiên bản**: v1.1
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.RegenerateAsync` (phần chương trình tạo lại thiết kế của một thiệp đã có)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Khách hàng đã đăng nhập và sở hữu thiệp nguồn.
  - Thiệp nguồn có cardType = handmade.
- **Các trường hợp cần kiểm tra**:
  - Thiệp có thể được tạo từ sản phẩm hoặc từ mẫu hoa; cả hai nguồn đều không thay đổi quy tắc này.
- **Input**:
  ```text
  Yêu cầu tạo lại thiết kế với mã của thiệp HandMade
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo HTTP 409 với mã HANDMADE_CARD_REGENERATE_NOT_SUPPORTED.
  Hệ thống không gọi AI, không dùng hạn mức AI và không tạo thiệp hoặc lịch sử mới.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Tạo lại thiết kế chỉ có ý nghĩa với kết quả do AI sinh ra. Chặn rõ ràng giúp tránh tạo một bản sao HandMade không có quy định về giá và nguồn.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-23
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-036
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

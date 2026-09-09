# UT-021-09: Chuyển mẫu thiệp AI sang đúng phần tạo thiệp AI

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Chuyển mẫu thiệp AI sang đúng phần tạo thiệp AI
- **Ghi chú**: Cùng một đường dẫn tiếp nhận cả thiệp AI và HandMade; loại của mẫu quyết định phần chương trình nào tiếp tục.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-09
- **Phiên bản**: v1.1
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.CreateAsync` (phần chương trình tiếp nhận và chọn cách tạo thiệp)
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Khách hàng đã đăng nhập và yêu cầu có đủ thông tin chung.
  - Mẫu thiệp tồn tại, đang dùng được và có template_type = ai.
  - Phần tạo thiệp AI theo TDD-006 được thay bằng một bộ phận giả để theo dõi việc chuyển tiếp.
- **Các trường hợp cần kiểm tra**:
  - Chọn một mẫu loại AI thay vì mẫu loại HandMade.
- **Input**:
  ```text
  cardTemplateId trỏ tới mẫu có template_type = ai
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống chuyển yêu cầu sang phần tạo thiệp AI theo TDD-006.
  Hệ thống không áp dụng cách tính giá viết tay và không tạo thiệp có cardType = handmade.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Nếu chọn sai nhánh, thiệp AI có thể bị tạo như thiệp viết tay và mang giá, ảnh hoặc lịch sử sai. Đây là ranh giới quan trọng giữa hai loại thiệp.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-02
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: Story
- **Mã**: STORY-035
- **Section**: Luồng tạo hoặc tạo lại thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.

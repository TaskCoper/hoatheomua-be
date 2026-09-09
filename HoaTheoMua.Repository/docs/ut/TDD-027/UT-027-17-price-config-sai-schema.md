# UT-027-17

## Thông tin tài liệu

- **Tiêu đề:** Price Config sai cấu trúc
- **Ghi chú:** Chỉ tính giá khi wrapper, JSON và các khoảng từ đều hợp lệ.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-17
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Configuration validation
- **Mục tiêu:** Không tự sửa hoặc đoán cấu hình giá bị hỏng.
- **Precondition / Mock setup**:
  - Source/input hợp lệ; Price Config tồn tại, active và chưa xóa.
- **Dữ liệu đại diện**:
  - Chạy riêng wrapper thiếu field, chuỗi JSON không parse được, item thiếu/sai kiểu, khoảng chồng lấn, sai thứ tự và có khoảng hở.
- **Input**:
  - Request Calligraphy hợp lệ cho từng payload Config sai.
- **Các bước thực hiện**:
  - Gọi recreate-content và cho service validate Price Config.
- **Expected output (bắt buộc)**:
  - HTTP 409 với `messageCode=CALLIGRAPHY_CONFIG_INVALID` cho từng biến thể.
  - Không chọn rule thay thế, không gọi AIModule và không ghi dữ liệu.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Cấu hình sai có thể tạo giá không nhất quán giữa các đơn hàng.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-12

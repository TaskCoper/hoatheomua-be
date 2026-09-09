# UT-027-16

## Thông tin tài liệu

- **Tiêu đề:** Price Config không hoạt động
- **Ghi chú:** Cấu hình tồn tại nhưng không public thì không được dùng cho request mới.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-16
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Dependency
- **Mục tiêu:** Chặn tính giá Calligraphy bằng cấu hình đã tắt.
- **Precondition / Mock setup**:
  - Source/input hợp lệ và form hiệu lực là Calligraphy.
  - Price Config chưa xóa nhưng có `isPublic=false`.
- **Dữ liệu đại diện**:
  - Một singleton inactive với payload hợp lệ để chứng minh lỗi đến từ trạng thái.
- **Input**:
  - Request recreate-content hợp lệ.
- **Các bước thực hiện**:
  - Cho service tải Price Config inactive.
- **Expected output (bắt buộc)**:
  - HTTP 409 với `messageCode=CALLIGRAPHY_CONFIG_INACTIVE`.
  - Không chọn rule, không gọi AIModule và không ghi dữ liệu.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Request mới chỉ được dùng cấu hình quản trị đang hoạt động.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-12

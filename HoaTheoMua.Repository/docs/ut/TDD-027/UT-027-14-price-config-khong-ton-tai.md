# UT-027-14

## Thông tin tài liệu

- **Tiêu đề:** Không tìm thấy Price Config cho Calligraphy
- **Ghi chú:** Chỉ nhánh Calligraphy cần singleton `card_config.prices` hiện hành.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-14
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
- **Mục tiêu:** Trả đúng lỗi khi không có cấu hình giá viết chữ.
- **Precondition / Mock setup**:
  - Source và input hiệu lực hợp lệ; `form_type=calligraphy`.
  - Repository không có singleton `card_config.prices`.
- **Dữ liệu đại diện**:
  - Không có record đại diện cho trạng thái not-found, tách biệt với deleted và inactive.
- **Input**:
  - Gọi recreate-content với Calligraphy.
- **Các bước thực hiện**:
  - Cho service resolve input rồi tìm Price Config.
- **Expected output (bắt buộc)**:
  - HTTP 404 với `messageCode=CALLIGRAPHY_CONFIG_NOT_FOUND`.
  - Không đọc prompt, không gọi AIModule và không ghi dữ liệu mới.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Không có cấu hình thì hệ thống không thể tính giá xác định.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-12

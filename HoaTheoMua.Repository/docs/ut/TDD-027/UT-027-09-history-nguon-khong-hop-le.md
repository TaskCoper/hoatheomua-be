# UT-027-09

## Thông tin tài liệu

- **Tiêu đề:** History nguồn không đủ dữ liệu
- **Ghi chú:** Kiểm tra các thành phần bắt buộc để dựng snapshot recreate-content.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-09
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Data integrity
- **Mục tiêu:** Không tạo Card mới khi lineage hoặc snapshot nguồn không đủ dùng.
- **Precondition / Mock setup**:
  - Card AI nguồn tồn tại, thuộc user và có `raw_image` hợp lệ.
  - Theo dõi AIModule và transaction persistence.
- **Dữ liệu đại diện**:
  - Chạy riêng các biến thể: thiếu History; sai `type/output_id`; thiếu hoặc `Guid.Empty` ở `base_id/root`; thiếu Template snapshot; thiếu một field Size snapshot; thiếu dữ liệu nội dung bắt buộc.
- **Input**:
  - Gọi recreate-content với body `{}` cho từng biến thể.
- **Các bước thực hiện**:
  - Cho service tải Card/History rồi xác thực dữ liệu nguồn.
- **Expected output (bắt buộc)**:
  - Mỗi biến thể trả HTTP 409 với `messageCode=CARD_SOURCE_HISTORY_INVALID`.
  - Không tính lại `root` từ metadata và không đoán dữ liệu snapshot bị thiếu.
  - Không gọi AIModule và không ghi Card/History mới.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Snapshot và Product lineage phải đủ tin cậy trước khi dùng lại dữ liệu lịch sử.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-04, BR-027-11, BR-027-17

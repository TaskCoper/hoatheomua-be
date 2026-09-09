# UT-027-15

## Thông tin tài liệu

- **Tiêu đề:** Price Config đã bị xóa mềm
- **Ghi chú:** Phân biệt cấu hình đã xóa với cấu hình chưa từng tồn tại.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-15
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
- **Mục tiêu:** Chặn Calligraphy khi Price Config đã soft-delete.
- **Precondition / Mock setup**:
  - Source/input hợp lệ và form hiệu lực là Calligraphy.
  - Singleton `card_config.prices` có `isDeleted=true`.
- **Dữ liệu đại diện**:
  - Một record đã xóa đại diện cho mọi payload bên trong vì trạng thái deleted phải được kiểm tra trước parse.
- **Input**:
  - Request recreate-content hợp lệ.
- **Các bước thực hiện**:
  - Cho service tải Price Config đã xóa.
- **Expected output (bắt buộc)**:
  - HTTP 410 với `messageCode=CALLIGRAPHY_CONFIG_DELETED`.
  - Không parse payload, không gọi AIModule và không ghi dữ liệu.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Trạng thái xóa mềm là quyết định quản trị rõ ràng và phải được tôn trọng.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-12

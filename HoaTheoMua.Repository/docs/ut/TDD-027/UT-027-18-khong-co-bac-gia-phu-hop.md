# UT-027-18

## Thông tin tài liệu

- **Tiêu đề:** Không có bậc giá phù hợp với số từ
- **Ghi chú:** Price Config hợp lệ nhưng không phủ số từ hiệu lực thì request phải dừng.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-18
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Pricing
- **Mục tiêu:** Không tính phụ phí khi không có rule chứa `word_count`.
- **Precondition / Mock setup**:
  - Source/input hợp lệ; form hiệu lực là Calligraphy.
  - Price Config hợp lệ nhưng các rule không chứa số từ của message hiệu lực.
- **Dữ liệu đại diện**:
  - `word_count` lớn hơn cận trên cuối cùng đại diện cho mọi giá trị ngoài vùng được cấu hình.
- **Input**:
  - Request có message hợp lệ theo Size nhưng không thuộc Price rule nào.
- **Các bước thực hiện**:
  - Gọi recreate-content và quan sát bước chọn rule.
- **Expected output (bắt buộc)**:
  - HTTP 422 với `messageCode=CALLIGRAPHY_PRICE_RULE_NOT_FOUND`.
  - Không dùng rule gần nhất, không gọi AIModule và không ghi dữ liệu.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Không được tự suy diễn giá khi cấu hình không bao phủ dữ liệu thực tế.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-12

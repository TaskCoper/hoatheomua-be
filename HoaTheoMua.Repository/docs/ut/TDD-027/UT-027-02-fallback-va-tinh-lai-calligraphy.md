# UT-027-02

## Thông tin tài liệu

- **Tiêu đề:** Dùng dữ liệu nguồn và tính lại giá Calligraphy
- **Ghi chú:** Kiểm tra fallback nội dung cùng việc chọn lại bậc giá theo số từ hiệu lực.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-02
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Positive / Business rule
- **Mục tiêu:** Xác nhận text bị bỏ, null, rỗng hoặc blank đều lấy từ nguồn và giá Calligraphy được tính lại thay vì sao chép giá cũ.
- **Precondition / Mock setup**:
  - Card/History nguồn hợp lệ, có message 45 từ và snapshot Size cho phép ít nhất 45 từ.
  - Price Config hiện hành có rule 36-70 từ với `extra_price=39000`.
  - Prompt, AIModule và persistence đều thành công.
- **Dữ liệu đại diện**:
  - Chạy riêng bốn biến thể cho từng text field: bỏ field, `null`, chuỗi rỗng và chuỗi chỉ có khoảng trắng.
  - Mỗi lần chỉ đổi một biến thể để xác định đúng hành vi fallback.
- **Input**:
  - `form_type=calligraphy`; các text field dùng các biến thể trên; không truyền `attached_image_url`.
- **Các bước thực hiện**:
  - Gọi recreate-content cho từng bộ dữ liệu.
  - Kiểm tra input hiệu lực, số từ, rule giá và snapshot Card mới.
- **Expected output (bắt buộc)**:
  - HTTP 200 cho mọi biến thể hợp lệ.
  - Sender, receiver và message hiệu lực lấy đúng giá trị nguồn khi field tương ứng bị bỏ/null/rỗng/blank.
  - Ảnh đính kèm nguồn được giữ vì request không truyền field này.
  - `word_count=45`, `word_config_snapshot` là rule 36-70, `extra_price=39000`; `base_price=size_base_price` và `total_price=base_price+39000`.
  - Không sao chép `word_count`, rule hoặc giá Calligraphy cũ nếu chúng khác kết quả tính hiện hành.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Fallback và tính lại giá là khác biệt chính giữa recreate-content và regenerate thông thường.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-08, BR-027-09, BR-027-10, BR-027-12

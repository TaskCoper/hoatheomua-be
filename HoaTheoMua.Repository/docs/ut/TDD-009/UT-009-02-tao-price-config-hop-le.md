# UT-009-02

## Thông tin tài liệu

- **Tiêu đề:** Tạo Price Config hợp lệ
- **Ghi chú:** Price Config được tách khỏi Size Config vì inner items sử dụng các khoảng số từ và quy tắc liên tục riêng.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-009-02
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Tạo Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.CreateConfig`
- **Loại:** Happy
- **Mục tiêu:** Xác nhận tập bậc giá hợp lệ được chấp nhận và lưu thành singleton public.
- **Precondition / Mock setup**:
  - Admin admin-01 đã đăng nhập; chưa có record chưa xóa mang key card_config.prices.
  - Wrapper có đúng bảy field; inner value parse thành object chỉ có items.
- **Dữ liệu đại diện**:
  - Rule 0-35 có extra_price=0 và rule 36-70 có extra_price=39000.
  - Hai rule đại diện tập khoảng hợp lệ: tăng dần, không chồng lấn và rule sau bắt đầu ngay tại max_words trước + 1.
  - Giá 0 ở rule đầu là biên hợp lệ vì extra_price được phép bằng 0.
- **Input**:
  - POST /api/v1/configs.
  - Body có group=card_config, key=card_config.prices, kind=Setting; wrapper chuẩn và inner items gồm hai rule 0-35, 36-70.
- **Các bước thực hiện**:
  - Gọi endpoint tạo Config.
  - Cho service kiểm tra wrapper, từng field số và quan hệ giữa hai khoảng.
  - Lưu record rồi kiểm tra response.
- **Expected output (bắt buộc)**:
  - HTTP 200 và trả ConfigResponse của Price Config mới.
  - Record có isPublic=true, isDeleted=false, CreatedAt UTC, UpdatedAt=null; wrapper được giữ nguyên.
  - Không có lỗi khoảng hở/chồng lấn; revalidation được gọi sau SaveChanges.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Bài test chứng minh validator chấp nhận đúng chuỗi bậc giá liên tục và không từ chối giá phụ thu bằng 0.

## TEST_LINKS

- [TDD-009 - Tạo Config có hỗ trợ Card Config](../../tdd/TDD-009/TDD-009-tao-card-config.md)
- BR-009-07, BR-009-08, BR-009-10 đến BR-009-12


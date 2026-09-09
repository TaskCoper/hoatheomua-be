# UT-009-08

## Thông tin tài liệu

- **Tiêu đề:** Từ chối khi Card Config singleton đã tồn tại
- **Ghi chú:** Mỗi key Card chỉ có một record chưa xóa, kể cả record hiện đang private/inactive.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-009-08
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Tạo Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.CreateConfig`
- **Loại:** Error
- **Mục tiêu:** Ngăn tạo singleton thứ hai có cùng exact key.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập.
  - Database đã có size-01 với key=card_config.sizes, group=card_config, kind=Setting, isDeleted=false.
  - Request Size mới vượt qua toàn bộ validation identity và schema.
- **Dữ liệu đại diện**:
  - size-01 public là dữ liệu chính; size-01 isPublic=false vẫn chặn vì uniqueness không phụ thuộc trạng thái public.
  - Price singleton chưa xóa đại diện cùng quy tắc đối với key card_config.prices.
  - Record cùng key nhưng isDeleted=true không thuộc nhánh lỗi và không chặn tạo lại; đây là đối chứng cần nêu rõ.
- **Input**:
  - POST /api/v1/configs với Size Config hợp lệ cùng key card_config.sizes.
- **Các bước thực hiện**:
  - Gọi endpoint với singleton public tồn tại.
  - Lặp lại với singleton inactive và với Price Config.
  - Kiểm tra database và revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 409 với messageCode=CARD_CONFIG_ALREADY_EXISTS cho mọi record cùng key chưa xóa.
  - Không tạo record thứ hai, không SaveChanges và không gọi revalidation.
  - Đối chứng soft-deleted không làm phát sinh lỗi singleton vì truy vấn chỉ xét record chưa xóa.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Bài test giữ tính duy nhất của cấu hình live và giải thích rõ vì sao inactive vẫn chặn nhưng soft-deleted thì không.

## TEST_LINKS

- [TDD-009 - Tạo Config có hỗ trợ Card Config](../../tdd/TDD-009/TDD-009-tao-card-config.md)
- BR-009-06


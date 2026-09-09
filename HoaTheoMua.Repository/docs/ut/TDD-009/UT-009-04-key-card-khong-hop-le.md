# UT-009-04

## Thông tin tài liệu

- **Tiêu đề:** Từ chối key Card Config ngoài allowlist
- **Ghi chú:** Khi group đã là card_config, mọi key không thuộc sizes hoặc prices đều là lỗi Card.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-009-04
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
- **Mục tiêu:** Chỉ cho phép tạo hai singleton có key canonical.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập; group=card_config, kind=Setting.
  - Không có Config trùng và wrapper được giữ hợp lệ để cô lập lỗi key.
- **Dữ liệu đại diện**:
  - key=card_config.unknown là dữ liệu chính; key rỗng, null, sai casing hoặc key khác hoàn toàn là các biến thể ngoài allowlist.
  - Các biến thể đều thuộc Card mode do group canonical, nên không được xử lý như Config thường.
- **Input**:
  - POST /api/v1/configs với group=card_config và key=card_config.unknown.
- **Các bước thực hiện**:
  - Gọi endpoint cho từng key đại diện.
  - Kiểm tra lỗi trước schema/singleton và các side effect.
- **Expected output (bắt buộc)**:
  - HTTP 400 với messageCode=CARD_CONFIG_KEY_INVALID cho từng biến thể.
  - Không insert, không SaveChanges và không gọi revalidation.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Allowlist bảo đảm hệ thống không sinh thêm loại Card Config ngoài hai dependency đã thiết kế.

## TEST_LINKS

- [TDD-009 - Tạo Config có hỗ trợ Card Config](../../tdd/TDD-009/TDD-009-tao-card-config.md)
- BR-009-02, BR-009-04


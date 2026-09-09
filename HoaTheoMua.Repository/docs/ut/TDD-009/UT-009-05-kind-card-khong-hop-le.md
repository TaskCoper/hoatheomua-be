# UT-009-05

## Thông tin tài liệu

- **Tiêu đề:** Từ chối kind Card Config không phải Setting
- **Ghi chú:** Frontend thường gửi Setting, nhưng backend vẫn phải bảo vệ invariant của bảng Config.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-009-05
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
- **Mục tiêu:** Yêu cầu kind khớp chính xác giá trị Setting.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập; group và key Card canonical.
  - Wrapper Size hợp lệ và chưa có singleton cùng key.
- **Dữ liệu đại diện**:
  - kind=Content là dữ liệu chính; kind rỗng, null hoặc sai casing như setting đại diện mọi giá trị không exact Setting.
  - Mỗi biến thể chỉ thay kind để mã lỗi không bị che bởi lỗi group, key hoặc schema.
- **Input**:
  - POST /api/v1/configs với group=card_config, key=card_config.sizes, kind=Content.
- **Các bước thực hiện**:
  - Gọi endpoint cho dữ liệu chính và từng biến thể.
  - Theo dõi persistence và revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 400 với messageCode=CARD_CONFIG_KIND_INVALID.
  - Không insert Config, không SaveChanges và không gọi revalidation.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Kiểm tra phòng vệ này giữ Card Config trong đúng nhóm Setting mà các API đọc đang lọc.

## TEST_LINKS

- [TDD-009 - Tạo Config có hỗ trợ Card Config](../../tdd/TDD-009/TDD-009-tao-card-config.md)
- BR-009-05


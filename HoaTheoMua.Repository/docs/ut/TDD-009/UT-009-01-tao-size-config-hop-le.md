# UT-009-01

## Thông tin tài liệu

- **Tiêu đề:** Tạo Size Config hợp lệ
- **Ghi chú:** Kiểm tra Admin tạo singleton kích thước thiệp bằng endpoint Config hiện có.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-009-01
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
- **Mục tiêu:** Xác nhận wrapper và Size items hợp lệ được lưu với trạng thái mặc định đúng, sau đó mới gọi revalidation.
- **Precondition / Mock setup**:
  - Admin admin-01 đã đăng nhập; chưa có record chưa xóa mang key card_config.sizes.
  - Đồng hồ được cố định tại T1 để kiểm tra CreatedAt.
  - Wrapper có đúng bảy field bắt buộc và inner value là chuỗi JSON parse được.
- **Dữ liệu đại diện**:
  - Item S có width=10, height=20, base_price=10000, max_words=100; đại diện một kích thước hợp lệ ở các biên dương thông thường.
  - Có thể thêm item M với name khác để chứng minh name duy nhất; mọi item đều phải thỏa cùng schema.
  - Không truyền isPublic vì Create request cũ không có field này; server phải tự gán public.
- **Input**:
  - POST /api/v1/configs.
  - Body: group=card_config, key=card_config.sizes, kind=Setting; wrapper group=card_config, label='Kích thước thiệp', options=[], required=false, inputType=json, validations=[]; inner items chứa S.
- **Các bước thực hiện**:
  - Gọi endpoint tạo Config.
  - Cho service nhận diện Card mode, kiểm tra identity, wrapper, inner JSON và Size item.
  - Xác nhận singleton chưa tồn tại, lưu database rồi gọi revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 200 và response dùng ConfigResponse hiện có.
  - Record lưu nguyên wrapper; isPublic=true, isDeleted=false, CreatedAt=T1 UTC, UpdatedAt=null và userId=admin-01.
  - Revalidation chỉ được gọi sau SaveChanges với entity=config, group=card_config, key=card_config.sizes.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Luồng này xác nhận toàn bộ hợp đồng tạo Size Config từ validation đến trạng thái mặc định và side effect sau commit.

## TEST_LINKS

- [TDD-009 - Tạo Config có hỗ trợ Card Config](../../tdd/TDD-009/TDD-009-tao-card-config.md)
- BR-009-01, BR-009-02, BR-009-07 đến BR-009-09, BR-009-11, BR-009-12


# UT-008-02

## Thông tin tài liệu

- **Tiêu đề:** Lọc Card Config theo key chính xác
- **Ghi chú:** Card mode không được dùng phép Contains của Config Setting cũ vì có thể trả nhầm key chỉ chứa chuỗi gần giống.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-008-02
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Danh sách Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.GetConfigSetting`
- **Loại:** Branch
- **Mục tiêu:** Xác nhận key Card được so khớp exact cùng group canonical.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập.
  - Config size-01 có group=card_config, key=card_config.sizes, kind=Setting, isPublic=true, isDeleted=false.
  - Config similar-01 có group=card_config, key=prefix.card_config.sizes.suffix và cùng trạng thái khả dụng.
  - Config wrong-group-01 có key=card_config.sizes nhưng group=other.
- **Dữ liệu đại diện**:
  - similar-01 đại diện mọi key có chứa nhưng không bằng key chuẩn, chẳng hạn card_config.sizes.backup.
  - wrong-group-01 chứng minh exact key vẫn chưa đủ nếu group không bằng card_config.
  - Khoảng trắng bao quanh query được trim trước nhận diện; sau đó dữ liệu trong database vẫn phải khớp giá trị canonical.
- **Input**:
  - GET /api/v1/configs/settings?key=card_config.sizes
- **Các bước thực hiện**:
  - Gọi endpoint với key chuẩn.
  - Cho service nhận diện Card mode và áp dụng exact group/key.
  - Kiểm tra từng ID trong value.
- **Expected output (bắt buộc)**:
  - HTTP 200.
  - value chỉ chứa size-01.
  - similar-01 bị loại vì key không exact; wrong-group-01 bị loại vì group sai.
  - Không áp dụng phép Contains của nhánh Setting thông thường.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** So khớp exact ngăn trả nhầm cấu hình có tên gần giống và bảo đảm Card chỉ đọc đúng singleton chuẩn.

## TEST_LINKS

- [TDD-008 - Lấy danh sách Config Setting và Card Config](../../tdd/TDD-008/TDD-008-lay-danh-sach-card-configs.md)
- BR-008-03, BR-008-04


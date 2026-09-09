# UT-008-01

## Thông tin tài liệu

- **Tiêu đề:** Lấy hai Card Config đang hoạt động
- **Ghi chú:** Kiểm tra Card mode khi Admin lấy danh sách cấu hình kích thước và bậc giá đang được phép sử dụng.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-008-01
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Danh sách Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.GetConfigSetting`
- **Loại:** Happy
- **Mục tiêu:** Xác nhận endpoint cũ chỉ trả đúng hai singleton Card Config thuộc allowlist và giữ nguyên cấu trúc ConfigResponse.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập và thỏa AdminPolicy.
  - Config size-01 có group=card_config, key=card_config.sizes, kind=Setting, isPublic=true, isDeleted=false.
  - Config price-01 có group=card_config, key=card_config.prices, kind=Setting, isPublic=true, isDeleted=false.
  - Config unknown-01 có cùng group/trạng thái nhưng key=card_config.unknown.
- **Dữ liệu đại diện**:
  - Hai record sizes và prices đại diện đầy đủ cho allowlist Card Config được phép công khai.
  - Record unknown-01 chứng minh cùng namespace và đang public vẫn không đủ điều kiện nếu key nằm ngoài allowlist.
  - Giá trị bên trong value không được parse trong API đọc nên có thể dùng JSON hợp lệ tối thiểu; nội dung schema không quyết định nhánh lọc này.
- **Input**:
  - GET /api/v1/configs/settings?groupName=card_config
- **Các bước thực hiện**:
  - Gọi endpoint với groupName nhận diện Card mode.
  - Cho service lọc Kind=Setting, chưa xóa, public, exact group và key thuộc allowlist.
  - Đối chiếu danh sách và các field response.
- **Expected output (bắt buộc)**:
  - HTTP 200; isSuccess=true và isFailed=false.
  - value gồm đúng size-01 và price-01; unknown-01 không xuất hiện vì key ngoài allowlist.
  - Mỗi item có id, key, group, isPublic, value, kind, userId; không có pagination, totalCount, isDeleted, createdAt hoặc updatedAt.
  - Không khẳng định thứ tự sizes/prices vì API không có quy tắc sort.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Đây là luồng đọc Card Config chính, xác nhận đồng thời điều kiện public, allowlist và contract response không đổi.

## TEST_LINKS

- [TDD-008 - Lấy danh sách Config Setting và Card Config](../../tdd/TDD-008/TDD-008-lay-danh-sach-card-configs.md)
- BR-008-01, BR-008-02, BR-008-04, BR-008-06, BR-008-07


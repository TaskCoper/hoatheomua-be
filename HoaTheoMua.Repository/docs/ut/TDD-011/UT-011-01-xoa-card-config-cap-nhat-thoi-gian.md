# UT-011-01

## Thông tin tài liệu

- **Tiêu đề:** Xóa mềm Card Config và cập nhật thời gian
- **Ghi chú:** DELETE giữ contract cũ nhưng phải ghi thời điểm xóa và bảo toàn snapshot của Card đã tạo.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-011-01
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Xóa Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.DeleteConfig`
- **Loại:** Happy
- **Mục tiêu:** Xác nhận Config được đánh dấu IsDeleted thay vì xóa vật lý, sau đó revalidation nhận đúng group/key.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập và thỏa AdminPolicy.
  - Size Config size-01 tồn tại với group=card_config, key=card_config.sizes, isPublic=true, isDeleted=false, UpdatedAt=T0.
  - Đồng hồ hệ thống là T1>T0; Card old-card-01 đã lưu snapshot từ size-01.
- **Dữ liệu đại diện**:
  - Size Config public là dữ liệu chính; Price Config và Config isPublic=false đại diện cùng thao tác soft-delete vì DELETE không phụ thuộc key Card hay trạng thái public.
  - Mỗi biến thể được chạy riêng để chứng minh response trả đúng group/key của record thực tế.
  - Card cũ đại diện mọi GeneratedCard/History đã snapshot cấu hình trước thời điểm xóa.
- **Input**:
  - DELETE /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
- **Các bước thực hiện**:
  - Gọi endpoint với ID size-01.
  - Cho service tìm record chưa xóa, set IsDeleted và UpdatedAt rồi SaveChanges.
  - Kiểm tra response, revalidation và Card cũ.
- **Expected output (bắt buộc)**:
  - HTTP 200; response value={group:'card_config', key:'card_config.sizes'}.
  - size-01 vẫn tồn tại trong database, IsDeleted=true và UpdatedAt=T1 UTC; các field khác giữ nguyên.
  - old-card-01 và snapshot của nó không thay đổi.
  - Revalidation được gọi sau SaveChanges với entity=config cùng group/key vừa xóa.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Luồng chính xác nhận xóa mềm có dấu thời gian mà không làm mất contract hoặc dữ liệu Card lịch sử.

## TEST_LINKS

- [TDD-011 - Xóa Config có hỗ trợ Card Config](../../tdd/TDD-011/TDD-011-xoa-card-config.md)
- BR-011-01, BR-011-03 đến BR-011-05, BR-011-08, BR-011-09


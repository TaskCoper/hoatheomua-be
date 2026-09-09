# UT-010-04

## Thông tin tài liệu

- **Tiêu đề:** Không cho thay đổi identity của Card Config
- **Ghi chú:** Group, key và kind xác định vai trò singleton nên PUT chỉ được gửi lại đúng giá trị hiện tại.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-010-04
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Cập nhật Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.UpdateConfig`
- **Loại:** Error
- **Mục tiêu:** Từ chối mọi yêu cầu biến Size Config thành Price Config hoặc Config loại khác.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập.
  - Target size-01 tồn tại, chưa xóa, group=card_config, key=card_config.sizes, kind=Setting, value/isPublic/UpdatedAt đã biết.
  - Phần value trong request hợp lệ.
- **Dữ liệu đại diện**:
  - Đổi key thành card_config.prices là dữ liệu chính.
  - Đổi group, đổi kind, để trống hoặc thiếu một trong ba identity là các biến thể cùng vi phạm bất biến.
  - Mỗi biến thể chỉ thay một field để xác định chính xác identity nào gây lỗi.
- **Input**:
  - PUT size-01 với request key=card_config.prices; group=card_config, kind=Setting và các field khác hợp lệ.
- **Các bước thực hiện**:
  - Gọi endpoint với key bị đổi.
  - Lặp lại riêng cho group và kind.
  - Kiểm tra response và toàn bộ field của target.
- **Expected output (bắt buộc)**:
  - HTTP 409 với messageCode=CARD_CONFIG_IDENTITY_IMMUTABLE cho từng biến thể.
  - Group/key/kind, value, isPublic và UpdatedAt đều giữ nguyên.
  - Không SaveChanges và không gọi revalidation.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Khóa identity ngăn một singleton đổi vai trò, điều có thể làm hệ thống thiếu Size và đồng thời có hai Price Config.

## TEST_LINKS

- [TDD-010 - Cập nhật Config có hỗ trợ Card Config](../../tdd/TDD-010/TDD-010-cap-nhat-card-config.md)
- BR-010-03, BR-010-04


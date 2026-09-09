# UT-010-02

## Thông tin tài liệu

- **Tiêu đề:** Cập nhật Price Config hợp lệ
- **Ghi chú:** Price Config dùng quy tắc khoảng số từ riêng nên được kiểm tra độc lập với Size.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-010-02
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Cập nhật Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.UpdateConfig`
- **Loại:** Happy
- **Mục tiêu:** Xác nhận bảng giá liên tục hợp lệ được lưu mà không làm đổi identity của singleton.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập.
  - Price Config price-01 tồn tại, chưa xóa, identity canonical và UpdatedAt=T0.
  - Value cũ có bậc 0-35 giá 0 và 36-70 giá 39000.
  - Request mới giữ hai khoảng liên tục nhưng đổi extra_price của bậc 36-70 thành 49000; đồng hồ là T1>T0.
- **Dữ liệu đại diện**:
  - Đổi 39000 thành 49000 đại diện cập nhật giá hợp lệ; extra_price vẫn không âm.
  - Hai khoảng 0-35 và 36-70 chứng minh quy tắc tăng dần, không chồng lấn và không có khoảng hở.
  - Thêm nhiều khoảng hợp lệ dùng cùng thuật toán; test này chọn hai khoảng để dễ kiểm tra bằng mắt.
- **Input**:
  - PUT /api/v1/configs/550e8400-e29b-41d4-a716-446655440002 với identity Price giữ nguyên, isPublic=true và wrapper mới.
- **Các bước thực hiện**:
  - Gọi endpoint cập nhật price-01.
  - Cho service kiểm tra identity, wrapper, từng rule và quan hệ giữa các khoảng.
  - Lưu rồi kiểm tra response và record.
- **Expected output (bắt buộc)**:
  - HTTP 200; value='Cập nhật thành công'.
  - Wrapper mới được lưu với extra_price=49000 và UpdatedAt=T1.
  - Group=card_config, key=card_config.prices, kind=Setting cùng các field bất biến khác giữ nguyên.
  - Revalidation được gọi sau SaveChanges.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Bài test xác nhận Update tái sử dụng đúng Price validator và chỉ thay dữ liệu được phép.

## TEST_LINKS

- [TDD-010 - Cập nhật Config có hỗ trợ Card Config](../../tdd/TDD-010/TDD-010-cap-nhat-card-config.md)
- BR-010-07 đến BR-010-10


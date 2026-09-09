# UT-009-07

## Thông tin tài liệu

- **Tiêu đề:** Từ chối Price Config sai schema
- **Ghi chú:** Price validator phải kiểm tra cả từng rule và quan hệ liên tục giữa các rule.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-009-07
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Tạo Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.CreateConfig`
- **Loại:** Boundary
- **Mục tiêu:** Không lưu cấu hình giá có thể khiến một số lượng từ không có giá hoặc khớp nhiều giá.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập; group/key/kind canonical và chưa có Price singleton.
  - Request chính có wrapper chuẩn, hai rule 0-35 và 37-70.
- **Dữ liệu đại diện**:
  - Khoảng hở tại 36 là dữ liệu chính: rule sau phải bắt đầu từ 36 chứ không phải 37.
  - Các biến thể wrapper/inner gồm thiếu-thừa-sai field, JSON lỗi, items rỗng hoặc sai kiểu.
  - Các biến thể rule gồm số âm, min lớn hơn max, sai thứ tự, chồng lấn 0-35 và 35-70, hoặc khoảng không liên tục.
  - Mỗi biến thể được chạy riêng nhưng đều làm bảng giá không xác định duy nhất giá cho toàn miền đã khai báo.
- **Input**:
  - POST Price Config với ranges 0-35/extra_price=0 và 37-70/extra_price=39000.
- **Các bước thực hiện**:
  - Gọi endpoint với khoảng hở.
  - Lặp từng biến thể, giữ identity Card hợp lệ.
  - Kiểm tra response, database và revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 400 với messageCode=CALLIGRAPHY_CONFIG_INVALID cho từng biến thể.
  - Không insert Config, không SaveChanges và không gọi revalidation.
  - Không tự sửa khoảng, sắp xếp lại rule hoặc chọn giá thay cho Admin.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Giá calligraphy ảnh hưởng trực tiếp số tiền; validator phải từ chối mọi cấu hình mơ hồ thay vì tự suy đoán.

## TEST_LINKS

- [TDD-009 - Tạo Config có hỗ trợ Card Config](../../tdd/TDD-009/TDD-009-tao-card-config.md)
- BR-009-07, BR-009-08, BR-009-10


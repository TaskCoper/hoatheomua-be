# UT-010-03

## Thông tin tài liệu

- **Tiêu đề:** Card Config không tồn tại hoặc đã bị xóa
- **Ghi chú:** Service chỉ tìm record chưa xóa, nên ID không tồn tại và record soft-deleted có cùng phản hồi công khai.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-010-03
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
- **Mục tiêu:** Dừng update trước Card validation khi target không còn khả dụng.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập.
  - Lần chính có Size Config size-deleted-01 với isDeleted=true và UpdatedAt=T0.
  - Full request cập nhật hợp lệ để lỗi duy nhất đến từ target.
- **Dữ liệu đại diện**:
  - Record đã xóa là dữ liệu chính; UUID không có bản ghi và Guid.Empty không ánh xạ bản ghi đại diện cùng kết quả truy vấn không tìm thấy.
  - Mỗi biến thể phải chạy riêng để xác nhận cả filter isDeleted và nhánh thiếu bản ghi.
  - ID sai định dạng không thuộc UT này vì bị route/model validation xử lý trước service.
- **Input**:
  - PUT /api/v1/configs/{id} với id của size-deleted-01 và full Size request hợp lệ.
- **Các bước thực hiện**:
  - Gọi endpoint với record đã xóa.
  - Lặp lại bằng UUID không tồn tại và Guid.Empty.
  - Kiểm tra response, trạng thái record và revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 404 với messageCode=NOT_FOUND cho từng biến thể.
  - Không validate/ghi value mới, không đổi isPublic hoặc UpdatedAt=T0.
  - Không SaveChanges và không gọi revalidation.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Phản hồi thống nhất thể hiện target không thể cập nhật mà không làm lộ khác biệt giữa thiếu và đã soft-delete.

## TEST_LINKS

- [TDD-010 - Cập nhật Config có hỗ trợ Card Config](../../tdd/TDD-010/TDD-010-cap-nhat-card-config.md)
- BR-010-02


# UT-011-02

## Thông tin tài liệu

- **Tiêu đề:** Card Config không tồn tại hoặc đã bị xóa
- **Ghi chú:** Service chỉ tìm record chưa xóa nên cả hai trạng thái đều trả NOT_FOUND.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-011-02
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Xóa Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.DeleteConfig`
- **Loại:** Error
- **Mục tiêu:** Không ghi lại hoặc cập nhật timestamp khi DELETE target không khả dụng.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập.
  - Lần chính có size-deleted-01 với IsDeleted=true và UpdatedAt=T0.
- **Dữ liệu đại diện**:
  - Record đã soft-delete là dữ liệu chính; UUID không tồn tại và Guid.Empty không ánh xạ bản ghi đại diện cùng kết quả.
  - Các biến thể phải chạy riêng để kiểm tra filter IsDeleted và trường hợp repository trả null.
  - ID sai định dạng không thuộc case này vì route/model validation xử lý trước service.
- **Input**:
  - DELETE /api/v1/configs/{id} với id của size-deleted-01.
- **Các bước thực hiện**:
  - Gọi endpoint với record đã xóa.
  - Lặp lại bằng UUID không tồn tại và Guid.Empty.
  - Kiểm tra response, SaveChanges và revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 404 với messageCode=NOT_FOUND cho từng biến thể.
  - IsDeleted và UpdatedAt=T0 của record đã xóa giữ nguyên; không SaveChanges.
  - Không gọi revalidation vì không có kết quả xóa thành công.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Bài test bảo đảm DELETE lần nữa không làm mới timestamp và dùng cùng hợp đồng not-found cho target vắng mặt.

## TEST_LINKS

- [TDD-011 - Xóa Config có hỗ trợ Card Config](../../tdd/TDD-011/TDD-011-xoa-card-config.md)
- BR-011-02


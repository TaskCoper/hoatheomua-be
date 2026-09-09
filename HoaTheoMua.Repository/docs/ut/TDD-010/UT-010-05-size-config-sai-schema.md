# UT-010-05

## Thông tin tài liệu

- **Tiêu đề:** Từ chối cập nhật Size Config sai schema
- **Ghi chú:** Update phải dùng đầy đủ validator Size giống Create và không được làm hỏng record hiện hành.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-010-05
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Cập nhật Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.UpdateConfig`
- **Loại:** Boundary
- **Mục tiêu:** Giữ nguyên Config cũ khi wrapper, inner JSON hoặc Size item mới không hợp lệ.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập; target size-01 tồn tại, chưa xóa, identity canonical, UpdatedAt=T0.
  - Request giữ identity nhưng có item S với max_words=0; các field khác hợp lệ.
- **Dữ liệu đại diện**:
  - max_words=0 là biên chính vì trường này phải lớn hơn 0.
  - Các biến thể wrapper thiếu/thừa/sai constant, inner JSON lỗi, items rỗng/sai kiểu và item thiếu/thừa/sai tên đều cùng mã lỗi.
  - name rỗng/trùng, width/height không dương và base_price âm là các biến thể constraint; từng biến thể được chạy riêng.
- **Input**:
  - PUT size-01 với item name=S, width=10, height=20, base_price=10000, max_words=0.
- **Các bước thực hiện**:
  - Gọi endpoint với dữ liệu chính.
  - Lặp lại từng biến thể trong nhóm đại diện, mỗi lần chỉ tạo một lỗi.
  - Đọc lại target và theo dõi revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 400 với messageCode=CARD_SIZE_CONFIG_INVALID cho từng biến thể.
  - Value, isPublic và UpdatedAt=T0 của size-01 giữ nguyên; không có cập nhật một phần.
  - Không SaveChanges và không gọi revalidation.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Từ chối trước persistence bảo đảm một lần chỉnh sửa sai không phá singleton đang phục vụ các luồng Card.

## TEST_LINKS

- [TDD-010 - Cập nhật Config có hỗ trợ Card Config](../../tdd/TDD-010/TDD-010-cap-nhat-card-config.md)
- BR-010-06
- Đối chiếu BR-009-07 đến BR-009-09


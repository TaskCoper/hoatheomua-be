# UT-010-06

## Thông tin tài liệu

- **Tiêu đề:** Từ chối cập nhật Price Config sai schema
- **Ghi chú:** Bảng giá mới phải hợp lệ ở cả từng rule và quan hệ giữa các rule.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-010-06
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
- **Mục tiêu:** Không ghi Price Config có khoảng chồng lấn, khoảng hở hoặc giá trị không hợp lệ.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập; target price-01 tồn tại, chưa xóa, identity canonical, UpdatedAt=T0.
  - Request chính có hai khoảng 0-35 và 35-70, tạo chồng lấn tại 35.
- **Dữ liệu đại diện**:
  - Chồng lấn tại 35 là dữ liệu chính; khoảng hở 0-35 và 37-70, sai thứ tự, min lớn hơn max hoặc số âm là các biến thể rule.
  - Wrapper/inner JSON sai, items rỗng, field thiếu/thừa/sai kiểu cũng thuộc cùng mã CALLIGRAPHY_CONFIG_INVALID.
  - Mỗi biến thể được chạy riêng để ghi rõ loại sai, không coi một khoảng chồng lấn thay thế toàn bộ schema.
- **Input**:
  - PUT price-01 với ranges 0-35 và 35-70.
- **Các bước thực hiện**:
  - Gọi endpoint với hai khoảng chồng lấn.
  - Lặp từng biến thể đại diện trong khi giữ identity hợp lệ.
  - Kiểm tra response, record cũ và revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 400 với messageCode=CALLIGRAPHY_CONFIG_INVALID cho từng biến thể.
  - Value và UpdatedAt=T0 giữ nguyên; không tự sắp xếp hoặc sửa các khoảng.
  - Không SaveChanges và không gọi revalidation.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Bảng giá mơ hồ có thể thu sai tiền nên Update phải thất bại toàn bộ thay vì giữ một phần payload.

## TEST_LINKS

- [TDD-010 - Cập nhật Config có hỗ trợ Card Config](../../tdd/TDD-010/TDD-010-cap-nhat-card-config.md)
- BR-010-07
- Đối chiếu BR-009-07, BR-009-08, BR-009-10


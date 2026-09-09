# UT-027-04

## Thông tin tài liệu

- **Tiêu đề:** Từ chối người dùng chưa đăng nhập
- **Ghi chú:** Kiểm tra hàng rào xác thực trước khi đọc dữ liệu Card.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-04
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** Endpoint recreate-content và lớp xác thực
- **Loại:** Negative / Security
- **Mục tiêu:** Đảm bảo request không có phiên đăng nhập không thể đọc hoặc tạo dữ liệu Card.
- **Precondition / Mock setup**:
  - Request không có token hoặc phiên xác thực hợp lệ.
  - Theo dõi repository, AIModule và persistence.
- **Dữ liệu đại diện**:
  - Không có token đại diện cho mọi trường hợp middleware không tạo được danh tính người dùng.
- **Input**:
  - Route có UUID đúng định dạng; body `{}`.
- **Các bước thực hiện**:
  - Gọi endpoint recreate-content.
- **Expected output (bắt buộc)**:
  - HTTP 401 với `messageCode=UNAUTHORIZED`.
  - Không đọc Card/History, không gọi AIModule và không ghi database.

## Phân loại và trách nhiệm

- **Suite:** SECURITY
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Card và History là dữ liệu riêng của khách hàng.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-03

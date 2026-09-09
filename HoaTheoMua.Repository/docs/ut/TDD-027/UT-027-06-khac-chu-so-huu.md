# UT-027-06

## Thông tin tài liệu

- **Tiêu đề:** Từ chối Card hoặc History thuộc người dùng khác
- **Ghi chú:** Kiểm tra quyền sở hữu của cả Card và History nguồn.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-06
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Authorization
- **Mục tiêu:** Không cho người dùng tạo lại Card từ dữ liệu của người khác.
- **Precondition / Mock setup**:
  - Người dùng `customer-a` đã đăng nhập.
  - Card hoặc History nguồn thuộc `customer-b`.
- **Dữ liệu đại diện**:
  - Chạy riêng hai biến thể: Card khác owner và History khác owner; mỗi biến thể phải bị chặn.
- **Input**:
  - Route trỏ đến Card tồn tại; body `{}`.
- **Các bước thực hiện**:
  - Gọi recreate-content cho từng biến thể ownership.
- **Expected output (bắt buộc)**:
  - HTTP 403 với `messageCode=ACCESS_DENIED`.
  - Không dựng snapshot, không gọi AIModule và không ghi Card/History mới.

## Phân loại và trách nhiệm

- **Suite:** SECURITY
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Cả Card và History đều có thể chứa dữ liệu cá nhân hóa của khách hàng.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-04

# UT-023-05: Từ chối role ngoài Admin và Customer xem chi tiết

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối role ngoài Admin và Customer xem chi tiết
- **Ghi chú**: Role Staff đại diện cho mọi role đã xác thực nhưng không nằm trong allowlist Admin/Customer. Vì danh tính hợp lệ nhưng quyền không đủ, response phải là 403.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-05
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Chi tiết
- **Unit under test (bắt buộc)**: `CardTemplateService.GetByIdAsync`
- **Loại**: Error
- **Mục đích kiểm thử**: Ngăn role không được thiết kế truy cập dữ liệu chi tiết Template.

- **Precondition / Mock setup**:

1. Token hợp lệ tạo principal active với role=Staff.
2. Principal không có role Admin hoặc Customer.
3. Repository query count ban đầu bằng 0.

- **Dữ liệu đại diện**:

- Staff đại diện cho Partner hoặc role khác không có quyền.
- Điều kiện quyết định là không thuộc allowlist, không phải tên Staff.

- **Input**:

```http
GET /api/card-templates/550e8400-e29b-41d4-a716-446655440023
Authorization: Bearer <token-staff-hop-le>
```

- **Các bước thực hiện**:

1. Authentication thành công.
2. Authorization kiểm tra role và từ chối Staff.
3. Service không được gọi.

- **Expected output (bắt buộc)**:

```text
HTTP 403 Forbidden
response.messageCode = FORBIDDEN
Repository query count = 0
```

Giải thích kết quả:

- Người gọi có danh tính nhưng thiếu quyền nên dùng 403, không dùng 401.
- Không truy vấn để tránh lộ trạng thái Template.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận chỉ Admin và Customer được phép xem detail.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BR-019-01
- **Ghi chú**: Kiểm tra allowlist role.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BƯỚC 3 — API / Mã lỗi FORBIDDEN
- **Ghi chú**: Kiểm tra HTTP 403.

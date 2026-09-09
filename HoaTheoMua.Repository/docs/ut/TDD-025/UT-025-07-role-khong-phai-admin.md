# UT-025-07: Từ chối Update khi người gọi không phải Admin

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối Update khi người gọi không phải Admin
- **Ghi chú**: Customer đại diện cho mọi role đã xác thực nhưng không phải Admin. Danh tính hợp lệ nhưng quyền không đủ nên trả 403.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-07
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Cập nhật
- **Unit under test (bắt buộc)**: `CardTemplateService.UpdateAsync`
- **Loại**: Error
- **Mục đích kiểm thử**: Ngăn người dùng thường thay đổi nội dung và trạng thái Template.

- **Precondition / Mock setup**:

1. Token hợp lệ của tài khoản active role=Customer.
2. Body PATCH hợp lệ.
3. Repository query và Save count bằng 0.

- **Dữ liệu đại diện**:

- Customer đại diện cho Staff, Partner và role khác thiếu quyền Admin.
- Điểm quyết định là thiếu role Admin.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Authorization: Bearer <token-customer-hop-le>
Content-Type: application/json

{"name":"Tên mới"}
```

- **Các bước thực hiện**:

1. Authentication thành công.
2. Authorization từ chối role Customer.
3. Service không được gọi.

- **Expected output (bắt buộc)**:

```text
HTTP 403 Forbidden
response.messageCode = FORBIDDEN
Repository query count = 0; Save count = 0
```

Giải thích kết quả:

- Người gọi đã xác thực nên dùng 403 thay vì 401.
- Không query để tránh lộ hoặc sửa dữ liệu quản trị.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận chỉ Admin có quyền Update.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-01
- **Ghi chú**: Kiểm tra authorization Admin.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BƯỚC 3 — API / Mã lỗi FORBIDDEN
- **Ghi chú**: Kiểm tra HTTP 403.

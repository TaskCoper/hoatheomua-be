# UT-024-07: Từ chối Create khi người gọi không phải Admin

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối Create khi người gọi không phải Admin
- **Ghi chú**: Customer được dùng làm dữ liệu đại diện cho mọi role đã xác thực nhưng không phải Admin. Vì danh tính hợp lệ mà thiếu quyền, kết quả phải là 403.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024-07
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Tạo mới
- **Unit under test (bắt buộc)**: `CardTemplateService.CreateAsync`
- **Loại**: Error
- **Mục đích kiểm thử**: Bảo đảm chỉ Admin mới quản trị việc tạo Card Template.

- **Precondition / Mock setup**:

1. Token hợp lệ cho tài khoản active role=Customer.
2. Body Create hợp lệ.
3. Repository insert count=0.

- **Dữ liệu đại diện**:

- Customer đại diện cho Staff, Partner hoặc role khác không có quyền Admin.
- Điểm quyết định là thiếu role Admin.

- **Input**:

```http
POST /api/card-templates
Authorization: Bearer <token-customer-hop-le>
Content-Type: application/json

{"name":"Thiệp mới","templateType":"ai","imageUrl":"https://storage.example.com/new.png"}
```

- **Các bước thực hiện**:

1. Authentication thành công.
2. Authorization phát hiện role không phải Admin.
3. Request dừng trước service.

- **Expected output (bắt buộc)**:

```text
HTTP 403 Forbidden
response.messageCode = FORBIDDEN
Repository insert count = 0
```

Giải thích kết quả:

- Người gọi đã xác thực nên không dùng 401.
- Không query/insert để tránh role thường thay đổi dữ liệu quản trị.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận quyền Create chỉ thuộc Admin.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-01
- **Ghi chú**: Kiểm tra authorization Admin.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BƯỚC 3 — API / Mã lỗi FORBIDDEN
- **Ghi chú**: Kiểm tra HTTP 403.

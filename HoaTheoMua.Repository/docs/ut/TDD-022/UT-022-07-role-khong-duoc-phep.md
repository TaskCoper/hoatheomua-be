# UT-022-07: Từ chối role ngoài Admin và Customer

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối role ngoài Admin và Customer
- **Ghi chú**: Principal role Staff đại diện cho mọi role đã xác thực nhưng không nằm trong allowlist Admin/Customer. Người gọi đã đăng nhập nên kết quả đúng là 403, không phải 401.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-022-07
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Danh sách
- **Unit under test (bắt buộc)**: `CardTemplateService.GetListAsync`
- **Loại**: Error
- **Mục đích kiểm thử**: Xác nhận chỉ hai nhóm người dùng được thiết kế mới có thể xem danh sách Template.

- **Precondition / Mock setup**:

1. Token hợp lệ tạo principal đang hoạt động với role=Staff.
2. Principal không có role Admin hoặc Customer.
3. Repository query count ban đầu bằng 0.

- **Dữ liệu đại diện**:

- Staff đại diện cho Partner hoặc các role khác không được TDD-022 cho phép.
- Điều kiện quyết định là không thuộc allowlist Admin/Customer.

- **Input**:

```http
GET /api/card-templates?templateType=ai
Authorization: Bearer <token-staff-hop-le>
```

- **Các bước thực hiện**:

1. Authentication thành công và xác định người gọi là Staff.
2. Authorization kiểm tra allowlist role.
3. Request bị từ chối trước truy vấn.

- **Expected output (bắt buộc)**:

```text
HTTP 403 Forbidden
response.messageCode = FORBIDDEN
Repository query count = 0
```

Giải thích kết quả:

- Người gọi có danh tính hợp lệ nhưng không đủ quyền nên dùng 403.
- Không đọc database để tránh lộ dữ liệu cho role không được phép.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận whitelist role Admin/Customer được áp dụng trước service truy vấn.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-01
- **Ghi chú**: Kiểm tra authorization theo role.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BƯỚC 3 — API / Mã lỗi FORBIDDEN
- **Ghi chú**: Kiểm tra HTTP 403.

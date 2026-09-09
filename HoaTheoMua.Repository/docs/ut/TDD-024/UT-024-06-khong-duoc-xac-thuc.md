# UT-024-06: Từ chối Create khi Admin chưa được xác thực

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối Create khi Admin chưa được xác thực
- **Ghi chú**: Không token đại diện cho token sai, token hết hạn và tài khoản inactive. Hệ thống không hình thành principal Admin hoạt động nên trả 401 trước validation body và repository.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024-06
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
- **Mục đích kiểm thử**: Ngăn người chưa đăng nhập tạo Card Template.

- **Precondition / Mock setup**:

1. Request không có Authorization header.
2. Body Create hợp lệ để lỗi duy nhất là authentication.
3. Repository insert count=0.

- **Dữ liệu đại diện**:

- Không token, token sai, token hết hạn và tài khoản inactive cùng trả UNAUTHORIZED.
- Role Customer hợp lệ thuộc nhánh 403, không thuộc nhóm này.

- **Input**:

```http
POST /api/card-templates
Content-Type: application/json

{"name":"Thiệp mới","templateType":"ai","imageUrl":"https://storage.example.com/new.png"}
```

- **Các bước thực hiện**:

1. Authentication pipeline không xác lập được Admin hoạt động.
2. Request dừng trước service.
3. API trả 401.

- **Expected output (bắt buộc)**:

```text
HTTP 401 Unauthorized
response.messageCode = UNAUTHORIZED
Repository insert count = 0
```

Giải thích kết quả:

- Chưa xác thực được danh tính nên không được phép tạo dữ liệu quản trị.
- Body không cần được xử lý tiếp.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận Create bắt buộc phiên Admin hợp lệ.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-01
- **Ghi chú**: Kiểm tra authentication của Create.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BƯỚC 3 — API / Mã lỗi UNAUTHORIZED
- **Ghi chú**: Kiểm tra HTTP 401.

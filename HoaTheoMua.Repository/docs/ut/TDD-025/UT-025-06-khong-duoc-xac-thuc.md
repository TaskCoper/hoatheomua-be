# UT-025-06: Từ chối Update khi Admin chưa được xác thực

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối Update khi Admin chưa được xác thực
- **Ghi chú**: Không token đại diện cho token sai, token hết hạn và tài khoản inactive. Các trường hợp đều không tạo principal Admin hoạt động và trả 401 trước query/update.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-06
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
- **Mục đích kiểm thử**: Bảo vệ chức năng cập nhật khỏi request không có phiên đăng nhập hợp lệ.

- **Precondition / Mock setup**:

1. Request không có Authorization header.
2. Body PATCH hợp lệ.
3. Repository query và Save count đều bằng 0.

- **Dữ liệu đại diện**:

- Không token, token sai, token hết hạn và tài khoản inactive cùng trả UNAUTHORIZED.
- Role sai đã xác thực thuộc nhánh 403.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Content-Type: application/json

{"name":"Tên mới"}
```

- **Các bước thực hiện**:

1. Authentication không xác lập được principal hoạt động.
2. Request dừng trước service.
3. API trả 401.

- **Expected output (bắt buộc)**:

```text
HTTP 401 Unauthorized
response.messageCode = UNAUTHORIZED
Repository query count = 0; Save count = 0
```

Giải thích kết quả:

- Chưa xác thực được danh tính nên không được đọc hoặc sửa Template.
- Không tiết lộ trạng thái record.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận authentication bảo vệ Update.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-01
- **Ghi chú**: Kiểm tra Admin đã đăng nhập và active.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BƯỚC 3 — API / Mã lỗi UNAUTHORIZED
- **Ghi chú**: Kiểm tra HTTP 401.

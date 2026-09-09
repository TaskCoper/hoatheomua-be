# UT-025-08: Trả 404 khi Template cần cập nhật không tồn tại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Trả 404 khi Template cần cập nhật không tồn tại
- **Ghi chú**: UUID hợp lệ nhưng repository trả null. Đây là bước đầu của taxonomy record; Update không được tự biến thành Create.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-08
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
- **Mục đích kiểm thử**: Phân biệt missing record và bảo đảm PATCH không tạo tài nguyên mới.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. ID 550e8400-e29b-41d4-a716-446655440099 đúng UUID nhưng không có trong database.
3. Repository Save count=0.

- **Dữ liệu đại diện**:

- ID này đại diện cho mọi UUID hợp lệ không có record.
- Record isDeleted=true vẫn tồn tại và thuộc nhánh 410.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440099
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"Tên mới"}
```

- **Các bước thực hiện**:

1. Service query theo ID.
2. Repository trả null.
3. Service trả CARD_TEMPLATE_NOT_FOUND và không Save.

- **Expected output (bắt buộc)**:

```text
HTTP 404 Not Found
response.messageCode = CARD_TEMPLATE_NOT_FOUND
Không tạo record mới
Repository Save count = 0
```

Giải thích kết quả:

- PATCH chỉ sửa tài nguyên hiện có, không có upsert.
- Không thể kiểm tra field thay đổi khi target không tồn tại.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận Update không biến missing target thành Create.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-02
- **Ghi chú**: Kiểm tra not-found trả 404.

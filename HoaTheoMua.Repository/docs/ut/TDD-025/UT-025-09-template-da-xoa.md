# UT-025-09: Từ chối cập nhật Template đã xóa mềm

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối cập nhật Template đã xóa mềm
- **Ghi chú**: Bất kỳ mutable field nào và trạng thái Active/Inactive hiện tại đều không thay đổi kết quả khi isDeleted=true. Request đổi name được dùng làm dữ liệu đại diện.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-09
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Cập nhật
- **Unit under test (bắt buộc)**: `CardTemplateService.UpdateAsync`
- **Loại**: Branch
- **Mục đích kiểm thử**: Ngăn Update trở thành cách khôi phục hoặc sửa một Template đã xóa.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. Template tồn tại với isDeleted=true, name=Tên cũ và updatedAt=T1.
3. Repository Save count=0.

- **Dữ liệu đại diện**:

- Đổi name đại diện cho đổi description, imageUrl hoặc isActive trên record deleted.
- isActive hiện là true hay false đều không quan trọng sau khi isDeleted=true.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"Tên mới"}
```

- **Các bước thực hiện**:

1. Repository tìm thấy record.
2. Service kiểm tra isDeleted trước khi áp dụng patch.
3. Service trả CARD_TEMPLATE_DELETED và không Save.

- **Expected output (bắt buộc)**:

```text
HTTP 410 Gone
response.messageCode = CARD_TEMPLATE_DELETED
Persisted name = Tên cũ
Persisted isDeleted = true; updatedAt = T1
Repository Save count = 0
```

Giải thích kết quả:

- Record đã xóa không được phép sửa hoặc restore qua PATCH.
- updatedAt không đổi vì request bị từ chối.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận soft-deleted Template là bất biến đối với Update.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-02
- **Ghi chú**: Kiểm tra deleted trả 410.

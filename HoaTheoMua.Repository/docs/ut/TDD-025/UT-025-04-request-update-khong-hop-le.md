# UT-025-04: Từ chối request Update không hợp lệ

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối request Update không hợp lệ
- **Ghi chú**: Body rỗng là dữ liệu chính và đại diện cho ID sai, không có mutable field, name null/rỗng/blank/sai kiểu, imageUrl null/rỗng/sai URL/quá 2048 ký tự, isActive null/sai kiểu, isDeleted hoặc field lạ. Các lỗi chung trả VALIDATION_ERROR; templateType có mã riêng tại UT-025-05.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-04
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
- **Mục đích kiểm thử**: Bảo đảm PATCH phải thể hiện ít nhất một thay đổi hợp lệ và không chứa field bị cấm.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. Template tồn tại và chưa xóa.
3. Repository Save count=0.

- **Dữ liệu đại diện**:

- Body rỗng đại diện cho nhóm lỗi request chung có cùng response 400.
- templateType không thuộc nhóm này vì chỉ cần xuất hiện đã trả CARD_TEMPLATE_TYPE_IMMUTABLE/409.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{}
```

- **Các bước thực hiện**:

1. Validation kiểm tra body có field mutable hay không.
2. Không có field name, description, imageUrl hoặc isActive.
3. Request bị từ chối trước Save.

- **Expected output (bắt buộc)**:

```text
HTTP 400 Bad Request
response.messageCode = VALIDATION_ERROR
response.errors giải thích cần ít nhất một field cập nhật
Repository Save count = 0
```

Giải thích kết quả:

- PATCH rỗng không thể hiện ý định cập nhật.
- Không đổi record hoặc updatedAt khi request sai.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận lớp validation chung chặn request Update sai trước persistence.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-03, BR-021-05, BR-021-06, BR-021-12
- **Ghi chú**: Kiểm tra request shape và mutable field.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BƯỚC 3 — API / Mã lỗi VALIDATION_ERROR
- **Ghi chú**: Kiểm tra HTTP 400.

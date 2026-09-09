# UT-024-02: Chuẩn hóa description rỗng thành null

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Chuẩn hóa description rỗng thành null
- **Ghi chú**: Chuỗi chỉ có khoảng trắng được dùng làm dữ liệu chính và đại diện cho bốn cách không cung cấp mô tả hữu ích: bỏ field, null, chuỗi rỗng hoặc blank. Tất cả phải tạo thành công với description=null.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024-02
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Tạo mới
- **Unit under test (bắt buộc)**: `CardTemplateService.CreateAsync`
- **Loại**: Boundary
- **Mục đích kiểm thử**: Xác nhận field mô tả tùy chọn được chuẩn hóa nhất quán, không lưu chuỗi vô nghĩa.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. Các field name, templateType và imageUrl hợp lệ.
3. Repository insert thành công.

- **Dữ liệu đại diện**:

- description blank đại diện cho field bị bỏ, null và chuỗi rỗng.
- Giá trị có ký tự thực sau trim không thuộc nhóm này và phải được lưu sau khi trim.

- **Input**:

```http
POST /api/card-templates
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"Thiệp cảm ơn","description":"   ","templateType":"ai","imageUrl":"https://storage.example.com/thanks.png"}
```

- **Các bước thực hiện**:

1. Service nhận biết description có mặt nhưng chỉ chứa khoảng trắng.
2. Service chuẩn hóa description thành null.
3. Service tạo và lưu Template.

- **Expected output (bắt buộc)**:

```text
HTTP 201 Created
value.description = null
Record persisted description = null
Các field còn lại đúng request và default state
```

Giải thích kết quả:

- description là tùy chọn nên blank không làm request thất bại.
- Lưu null giúp database phân biệt rõ không có mô tả với mô tả có nội dung.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận mọi dạng mô tả không có nội dung được lưu thống nhất thành null.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-03
- **Ghi chú**: Kiểm tra normalization của description tùy chọn.

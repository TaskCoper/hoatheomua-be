# UT-025-01: Cập nhật các field được phép thành công

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Cập nhật các field được phép thành công
- **Ghi chú**: Một request cập nhật name, imageUrl và isActive để kiểm tra nhiều field mutable trong cùng lần PATCH. Cập nhật riêng từng field dùng cùng cơ chế. Description cố ý không xuất hiện để chứng minh field bị bỏ khỏi request phải giữ nguyên.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-01
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Cập nhật
- **Unit under test (bắt buộc)**: `CardTemplateService.UpdateAsync`
- **Loại**: Happy
- **Mục đích kiểm thử**: Xác nhận PATCH chỉ thay field được gửi, chuẩn hóa dữ liệu và bảo toàn field bị bỏ cùng các field bất biến.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. Template ID 550e8400-e29b-41d4-a716-446655440025 có name=Tên cũ, description=Mô tả cũ, imageUrl=https://storage.example.com/old.png, templateType=handmade, isActive=true, isDeleted=false.
3. createdAt=T0, updatedAt=T1; đồng hồ hiện tại T2>T1.
4. Repository Save thành công; File Storage call count=0.

- **Dữ liệu đại diện**:

- Request nhiều field đại diện cho update từng mutable field name, imageUrl hoặc isActive.
- Description không truyền khác với description=null: không truyền nghĩa là giữ nguyên, null nghĩa là xóa mô tả.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"  Tên mới  ","imageUrl":"https://storage.example.com/new.png","isActive":false}
```

- **Các bước thực hiện**:

1. Service tải Template và xác nhận chưa xóa.
2. Service xác nhận request không chứa templateType/isDeleted.
3. Service trim name, giữ URL mới và áp dụng isActive=false.
4. Service giữ description cũ, đặt updatedAt=T2 và Save.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
value.name = Tên mới
value.description = Mô tả cũ
value.imageUrl = https://storage.example.com/new.png
value.isActive = false
value.templateType = handmade; value.isDeleted = false
value.createdAt = T0; value.updatedAt = T2
```

Giải thích kết quả:

- Description giữ nguyên vì key không có trong JSON.
- Template inactive không còn xuất hiện cho Customer hoặc dùng cho Create mới; Card/History cũ không đổi.
- File Storage không được gọi vì API chỉ thay URL trong database.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận partial update đúng field hiện diện và bảo toàn dữ liệu ngoài phạm vi request.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-03, BR-021-05 đến BR-021-07
- **Ghi chú**: Kiểm tra mutable field, trim và cập nhật URL.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-09, BR-021-10
- **Ghi chú**: Kiểm tra inactive/updatedAt và dữ liệu lịch sử không đổi.

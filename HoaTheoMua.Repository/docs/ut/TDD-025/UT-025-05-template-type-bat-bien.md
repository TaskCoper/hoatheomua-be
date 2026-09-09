# UT-025-05: Từ chối mọi request có templateType

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối mọi request có templateType
- **Ghi chú**: Request truyền đúng templateType=handmade hiện tại là dữ liệu đại diện mạnh nhất: ngay cả khi giá trị không đổi, chỉ cần key xuất hiện đã bị cấm. Giá trị khác, null hoặc sai kiểu cũng trả CARD_TEMPLATE_TYPE_IMMUTABLE.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-05
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Cập nhật
- **Unit under test (bắt buộc)**: `CardTemplateService.UpdateAsync`
- **Loại**: Quirk
- **Mục đích kiểm thử**: Giữ loại AI/HandMade bất biến suốt vòng đời Template.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. Template tồn tại, chưa xóa và templateType=handmade.
3. Repository Save count=0.

- **Dữ liệu đại diện**:

- Cùng giá trị hiện tại, giá trị khác, null và sai kiểu đều bị chặn theo sự hiện diện của key.
- Client phải bỏ hẳn templateType khỏi PATCH.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"templateType":"handmade"}
```

- **Các bước thực hiện**:

1. Service/binder phát hiện request chứa key templateType.
2. Service dừng trước so sánh giá trị và trước Save.
3. API trả mã immutable.

- **Expected output (bắt buộc)**:

```text
HTTP 409 Conflict
response.messageCode = CARD_TEMPLATE_TYPE_IMMUTABLE
Persisted templateType = handmade
Repository Save count = 0
```

Giải thích kết quả:

- Loại Template quyết định flow AI/HandMade; cho phép sửa sẽ làm lịch sử và hành vi sai.
- Truyền cùng giá trị vẫn bị cấm để contract rõ ràng và không tạo ngoại lệ.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận discriminator bất biến dựa trên sự hiện diện của field, không dựa trên giá trị mới.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-04
- **Ghi chú**: Kiểm tra templateType không thuộc Update request.

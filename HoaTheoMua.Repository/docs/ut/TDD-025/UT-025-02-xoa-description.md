# UT-025-02: Xóa description bằng giá trị rỗng

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Xóa description bằng giá trị rỗng
- **Ghi chú**: Request dùng description=null làm dữ liệu chính và đại diện cho chuỗi rỗng hoặc chỉ khoảng trắng. Các giá trị này đều yêu cầu lưu null. Trường hợp không gửi key description không thuộc nhóm này vì phải giữ mô tả cũ.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-02
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Cập nhật
- **Unit under test (bắt buộc)**: `CardTemplateService.UpdateAsync`
- **Loại**: Boundary
- **Mục đích kiểm thử**: Xác nhận API phân biệt rõ yêu cầu xóa mô tả với việc không cập nhật mô tả.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. Template tồn tại, chưa xóa và có description=Mô tả cũ.
3. updatedAt=T1; đồng hồ hiện tại T2>T1; Save thành công.

- **Dữ liệu đại diện**:

- null, chuỗi rỗng và blank đều đại diện cho yêu cầu xóa description.
- Key không xuất hiện nghĩa là giữ nguyên, không được tự hiểu thành null.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"description":null}
```

- **Các bước thực hiện**:

1. Binder ghi nhận key description có xuất hiện.
2. Service chuẩn hóa giá trị null thành description=null.
3. Service nhận biết có thay đổi hiệu lực, đặt updatedAt=T2 và Save.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
value.description = null
Persisted description = null
value.updatedAt = T2
Các field khác giữ nguyên
```

Giải thích kết quả:

- Việc key xuất hiện thể hiện ý định xóa mô tả.
- updatedAt thay đổi vì dữ liệu persisted thực sự thay đổi.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận presence semantics của description để không vô tình giữ hoặc xóa sai dữ liệu.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-05, BR-021-12
- **Ghi chú**: Kiểm tra normalization và phân biệt field vắng mặt với explicit null.

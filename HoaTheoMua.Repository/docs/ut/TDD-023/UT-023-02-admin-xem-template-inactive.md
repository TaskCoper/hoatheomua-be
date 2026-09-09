# UT-023-02: Admin xem chi tiết Template Inactive thành công

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Admin xem chi tiết Template Inactive thành công
- **Ghi chú**: Kịch bản tách riêng khỏi Customer vì Admin được phép xem Template inactive. Template AI và HandMade dùng cùng nhánh quyền này.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-02
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Chi tiết
- **Unit under test (bắt buộc)**: `CardTemplateService.GetByIdAsync`
- **Loại**: Branch
- **Mục đích kiểm thử**: Xác nhận trạng thái inactive không chặn Admin quản lý Template chưa bị xóa.

- **Precondition / Mock setup**:

1. Admin đã đăng nhập và tài khoản đang hoạt động.
2. Template ID 550e8400-e29b-41d4-a716-446655440024 tồn tại, templateType=ai, isActive=false, isDeleted=false.
3. Template có name, description, imageUrl, createdAt và updatedAt đầy đủ.

- **Dữ liệu đại diện**:

- Template AI inactive đại diện cho Template HandMade inactive vì quyền Admin không phân nhánh theo loại.
- Record deleted không thuộc nhóm này và phải trả 410.

- **Input**:

```http
GET /api/card-templates/550e8400-e29b-41d4-a716-446655440024
Authorization: Bearer <token-admin-hop-le>
```

- **Các bước thực hiện**:

1. Service xác nhận role Admin.
2. Repository trả record chưa xóa nhưng inactive.
3. Service không áp dụng điều kiện isActive=true dành cho Customer.
4. Service trả resource đầy đủ.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
value.id = 550e8400-e29b-41d4-a716-446655440024
value.templateType = ai
value.isActive = false
value.isDeleted = false
Các field name, description, imageUrl, createdAt, updatedAt khớp record
```

Giải thích kết quả:

- Admin cần xem Template inactive để quản lý hoặc bật lại.
- Không trả CARD_TEMPLATE_INACTIVE vì mã đó chỉ áp dụng khi Customer xem.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận khác biệt visibility giữa Admin và Customer trên cùng một record inactive.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BR-019-04
- **Ghi chú**: Kiểm tra Admin xem được Template Active hoặc Inactive chưa xóa.

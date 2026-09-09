# UT-023-06: Trả 404 khi Template không tồn tại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Trả 404 khi Template không tồn tại
- **Ghi chú**: UUID hợp lệ nhưng repository không có record. Đây là bước đầu của thứ tự kiểm tra not-found → deleted → inactive.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-06
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Chi tiết
- **Unit under test (bắt buộc)**: `CardTemplateService.GetByIdAsync`
- **Loại**: Error
- **Mục đích kiểm thử**: Phân biệt record không tồn tại với record đã xóa hoặc inactive.

- **Precondition / Mock setup**:

1. Người gọi là Customer hợp lệ.
2. ID 550e8400-e29b-41d4-a716-446655440099 đúng định dạng UUID.
3. Repository query ID trên trả null.

- **Dữ liệu đại diện**:

- UUID này đại diện cho mọi UUID hợp lệ nhưng không có trong card_templates.
- Record isDeleted=true vẫn tồn tại và phải trả 410, không thuộc nhóm not-found.

- **Input**:

```http
GET /api/card-templates/550e8400-e29b-41d4-a716-446655440099
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Service nhận UUID hợp lệ.
2. Repository tìm đúng ID một lần và không có kết quả.
3. Service trả CARD_TEMPLATE_NOT_FOUND.

- **Expected output (bắt buộc)**:

```text
HTTP 404 Not Found
response.messageCode = CARD_TEMPLATE_NOT_FOUND
Không có value resource
```

Giải thích kết quả:

- Không có record để trả về.
- Không kiểm tra deleted/inactive khi đối tượng không tồn tại.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận taxonomy not-found và short-circuit đúng bước đầu.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BR-019-02
- **Ghi chú**: Kiểm tra Template không tồn tại trả 404.

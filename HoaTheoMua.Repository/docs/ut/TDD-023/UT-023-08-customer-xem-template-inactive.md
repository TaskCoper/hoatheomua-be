# UT-023-08: Từ chối Customer xem Template Inactive

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối Customer xem Template Inactive
- **Ghi chú**: Template phải chưa xóa để cô lập nhánh inactive. Admin xem cùng record được phép và đã được UT-023-02 đại diện.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-08
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
- **Mục đích kiểm thử**: Xác nhận Customer không thể xem trước một Template đang tạm ngừng hoạt động.

- **Precondition / Mock setup**:

1. Customer đã đăng nhập và tài khoản active.
2. Template tồn tại với isDeleted=false và isActive=false.
3. Repository trả đúng record.

- **Dữ liệu đại diện**:

- Template AI và HandMade inactive cho cùng kết quả đối với Customer.
- Record deleted không thuộc nhóm này vì phải trả 410 trước.

- **Input**:

```http
GET /api/card-templates/550e8400-e29b-41d4-a716-446655440024
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Repository tìm thấy Template.
2. Service xác nhận record chưa xóa.
3. Service thấy caller là Customer và isActive=false.
4. Service trả CARD_TEMPLATE_INACTIVE.

- **Expected output (bắt buộc)**:

```text
HTTP 409 Conflict
response.messageCode = CARD_TEMPLATE_INACTIVE
Không trả value Template
```

Giải thích kết quả:

- Inactive là trạng thái có thể thay đổi nên dùng 409, không dùng 410.
- Customer bị chặn nhưng Admin vẫn có thể quản lý record.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận visibility Customer không bao gồm Template inactive.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BR-019-05
- **Ghi chú**: Kiểm tra Customer chỉ xem được Template active.

# UT-023-07: Trả 410 khi Template đã xóa mềm

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Trả 410 khi Template đã xóa mềm
- **Ghi chú**: Record được đặt đồng thời isDeleted=true và isActive=false để chứng minh deleted có ưu tiên trước inactive. Admin được dùng làm caller đại diện cho cả Admin và Customer vì Template đã xóa chặn mọi role.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-07
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
- **Mục đích kiểm thử**: Xác nhận record đã xóa không được trả detail và lỗi deleted có thứ tự ưu tiên đúng.

- **Precondition / Mock setup**:

1. Admin đã đăng nhập và có quyền.
2. Template ID hợp lệ tồn tại với isDeleted=true và isActive=false.
3. Repository trả đúng record.

- **Dữ liệu đại diện**:

- Admin đại diện cho Customer trong nhánh deleted vì BR-019-03 áp dụng cho mọi role.
- Hai cờ cùng bất lợi chứng minh service kiểm tra isDeleted trước isActive.

- **Input**:

```http
GET /api/card-templates/550e8400-e29b-41d4-a716-446655440023
Authorization: Bearer <token-admin-hop-le>
```

- **Các bước thực hiện**:

1. Repository tìm thấy Template.
2. Service kiểm tra isDeleted và phát hiện true.
3. Service dừng trước nhánh quyền xem inactive.

- **Expected output (bắt buộc)**:

```text
HTTP 410 Gone
response.messageCode = CARD_TEMPLATE_DELETED
Không trả value Template
```

Giải thích kết quả:

- Record vẫn có trong DB nhưng đã bị loại khỏi sử dụng nên dùng 410.
- Không trả 200 cho Admin và không trả CARD_TEMPLATE_INACTIVE vì deleted ưu tiên.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận soft-delete chặn mọi role và được đánh giá trước trạng thái active.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BR-019-03
- **Ghi chú**: Kiểm tra deleted trả 410 cho mọi role.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BƯỚC 2 — Activity Diagram
- **Ghi chú**: Kiểm tra thứ tự not-found → deleted → inactive.

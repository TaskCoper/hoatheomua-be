# UT-023-03: Từ chối ID Template không đúng định dạng UUID

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối ID Template không đúng định dạng UUID
- **Ghi chú**: Chuỗi not-a-uuid đại diện cho mọi path ID không thể bind hoặc parse thành UUID, gồm UUID thiếu ký tự, sai cấu trúc hoặc có ký tự không hợp lệ.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-03
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
- **Mục đích kiểm thử**: Dừng request sai định dạng trước khi truy vấn database.

- **Precondition / Mock setup**:

1. Người gọi đã xác thực và có role Customer.
2. Path ID là not-a-uuid.
3. Service/repository query count ban đầu bằng 0.

- **Dữ liệu đại diện**:

- not-a-uuid đại diện cho toàn bộ dữ liệu không thể chuyển thành Guid.
- Trường hợp UUID hợp lệ nhưng không có record phải đi tới repository và trả 404, không thuộc UT này.

- **Input**:

```http
GET /api/card-templates/not-a-uuid
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Framework cố parse path ID thành UUID.
2. Parse thất bại tại lớp validation.
3. Request kết thúc trước service.

- **Expected output (bắt buộc)**:

```text
HTTP 400 Bad Request
response.messageCode = VALIDATION_ERROR
response.errors chỉ rõ field id
Repository query count = 0
```

Giải thích kết quả:

- ID chưa hợp lệ nên không có khóa để tìm database.
- Không trả CARD_TEMPLATE_NOT_FOUND vì chưa thực hiện truy vấn.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận validation path parameter không để input sai đi vào tầng dữ liệu.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BƯỚC 3 — API / Mã lỗi VALIDATION_ERROR
- **Ghi chú**: Kiểm tra ID sai định dạng trả HTTP 400.

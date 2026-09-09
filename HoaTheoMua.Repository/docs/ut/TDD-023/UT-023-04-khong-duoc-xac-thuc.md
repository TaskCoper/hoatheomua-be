# UT-023-04: Từ chối xem chi tiết khi chưa được xác thực

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối xem chi tiết khi chưa được xác thực
- **Ghi chú**: Không token đại diện cho token sai, token hết hạn và tài khoản inactive. Các trường hợp đều không hình thành principal hoạt động nên trả 401 trước khi đọc Template.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-04
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
- **Mục đích kiểm thử**: Bảo vệ endpoint chi tiết khỏi người gọi không có phiên đăng nhập hợp lệ.

- **Precondition / Mock setup**:

1. Request không có Authorization header.
2. Không có authenticated principal.
3. Repository query count ban đầu bằng 0.

- **Dữ liệu đại diện**:

- Không token, token sai, token hết hạn và tài khoản inactive cùng cho kết quả UNAUTHORIZED.
- Role sai không thuộc nhóm này vì role sai đã xác thực và phải trả 403.

- **Input**:

```http
GET /api/card-templates/550e8400-e29b-41d4-a716-446655440023
```

- **Các bước thực hiện**:

1. Authentication pipeline không xác lập được người dùng hoạt động.
2. Request bị dừng trước Controller/Service.
3. API trả 401.

- **Expected output (bắt buộc)**:

```text
HTTP 401 Unauthorized
response.messageCode = UNAUTHORIZED
Repository query count = 0
```

Giải thích kết quả:

- Chưa biết người gọi hợp lệ là ai nên hệ thống không được đọc Template.
- Không tiết lộ Template tồn tại, deleted hay inactive.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận authentication là hàng rào đầu tiên của API detail.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BR-019-01
- **Ghi chú**: Kiểm tra endpoint bắt buộc đăng nhập.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BƯỚC 3 — API / Mã lỗi UNAUTHORIZED
- **Ghi chú**: Kiểm tra HTTP 401.

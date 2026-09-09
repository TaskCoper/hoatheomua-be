# UT-024-08: Trả lỗi và không để lại record khi Insert thất bại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Trả lỗi và không để lại record khi Insert thất bại
- **Ghi chú**: Repository ném exception khi insert/save đại diện cho lỗi database, timeout hoặc lỗi hệ thống sau validation. Vì API chỉ lưu URL có sẵn, failure không được gọi File Storage để cleanup.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024-08
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Tạo mới
- **Unit under test (bắt buộc)**: `CardTemplateService.CreateAsync`
- **Loại**: Error
- **Mục đích kiểm thử**: Xác nhận Create có tính nguyên tử và không báo thành công khi database không lưu được record.

- **Precondition / Mock setup**:

1. Admin và toàn bộ body hợp lệ.
2. Repository được cấu hình ném exception khi insert hoặc Save.
3. Database ban đầu không có Template với ID dự kiến.
4. File Storage call count=0.

- **Dữ liệu đại diện**:

- Exception repository đại diện cho lỗi kết nối, timeout và database từ chối ghi.
- Tất cả cùng yêu cầu không có record persisted và trả INTERNAL_SERVER_ERROR.

- **Input**:

```http
POST /api/card-templates
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"Thiệp mới","templateType":"handmade","imageUrl":"https://storage.example.com/new.png"}
```

- **Các bước thực hiện**:

1. Service validation và normalization thành công.
2. Service tạo entity trong bộ nhớ.
3. Repository insert/Save ném exception.
4. Transaction rollback và service trả 500.

- **Expected output (bắt buộc)**:

```text
HTTP 500 Internal Server Error
response.messageCode = INTERNAL_SERVER_ERROR
Không có Card Template mới trong database
File Storage call count = 0
```

Giải thích kết quả:

- Entity trong bộ nhớ không được xem là đã tạo nếu Save thất bại.
- Không cleanup ảnh vì endpoint không upload hoặc sở hữu file tại URL.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận lỗi persistence không để lại dữ liệu dở dang hoặc side effect file.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-06, BR-020-09
- **Ghi chú**: Kiểm tra lưu URL trực tiếp và failure không cleanup file.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BƯỚC 3 — API / Mã lỗi INTERNAL_SERVER_ERROR
- **Ghi chú**: Kiểm tra HTTP 500.

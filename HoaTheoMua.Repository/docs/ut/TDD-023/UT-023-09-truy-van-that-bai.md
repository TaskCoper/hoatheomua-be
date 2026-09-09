# UT-023-09: Trả lỗi hệ thống khi truy vấn chi tiết thất bại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Trả lỗi hệ thống khi truy vấn chi tiết thất bại
- **Ghi chú**: Repository exception đại diện cho lỗi EF Core, timeout, mất kết nối hoặc lỗi database không xác định. Lỗi này không được biến thành 404 vì hệ thống chưa xác định được record có tồn tại hay không.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-09
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
- **Mục đích kiểm thử**: Chuẩn hóa lỗi tầng dữ liệu và không trả resource một phần.

- **Precondition / Mock setup**:

1. Customer, ID và request đều hợp lệ.
2. Repository ném exception khi tìm Template theo ID.
3. Không có record hoàn chỉnh để ánh xạ.

- **Dữ liệu đại diện**:

- Exception repository đại diện cho các lỗi kỹ thuật khi đọc database.
- Khác not-found: not-found là query thành công và trả null; UT này là query không hoàn tất.

- **Input**:

```http
GET /api/card-templates/550e8400-e29b-41d4-a716-446655440023
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Service validation thành công.
2. Service gọi repository.
3. Repository ném exception.
4. Service trả INTERNAL_SERVER_ERROR.

- **Expected output (bắt buộc)**:

```text
HTTP 500 Internal Server Error
response.messageCode = INTERNAL_SERVER_ERROR
Không trả value hoặc field Template một phần
```

Giải thích kết quả:

- Hệ thống không biết trạng thái thật của record khi query thất bại.
- Trả 500 để client có thể thử lại thay vì hiểu nhầm Template không tồn tại.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận lỗi database được phản ánh trung thực và không làm lộ dữ liệu không hoàn chỉnh.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BƯỚC 3 — API / Mã lỗi INTERNAL_SERVER_ERROR
- **Ghi chú**: Kiểm tra repository failure trả HTTP 500.

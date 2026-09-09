# UT-022-08: Trả lỗi hệ thống khi truy vấn danh sách thất bại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Trả lỗi hệ thống khi truy vấn danh sách thất bại
- **Ghi chú**: Repository ném exception khi query đại diện cho lỗi EF Core, mất kết nối, timeout hoặc lỗi database không xác định. Các lỗi này xảy ra sau khi request hợp lệ và không được trả danh sách một phần.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-022-08
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Danh sách
- **Unit under test (bắt buộc)**: `CardTemplateService.GetListAsync`
- **Loại**: Error
- **Mục đích kiểm thử**: Xác nhận lỗi tầng dữ liệu được chuẩn hóa thành HTTP 500 và không tạo response có dữ liệu thiếu.

- **Precondition / Mock setup**:

1. Customer đã đăng nhập, templateType=ai và pagination đều hợp lệ.
2. Repository được cấu hình ném exception khi thực hiện query hoặc count.
3. Không có kết quả query hoàn chỉnh để ánh xạ.

- **Dữ liệu đại diện**:

- Exception repository đại diện cho lỗi kết nối, timeout và lỗi thực thi câu lệnh database.
- Tất cả cùng là lỗi nội bộ; không được ánh xạ thành danh sách rỗng.

- **Input**:

```http
GET /api/card-templates?templateType=ai&page=1&pageSize=10
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Service validation request thành công.
2. Service gọi repository để count và lấy items.
3. Repository ném exception.
4. Service chuẩn hóa exception thành INTERNAL_SERVER_ERROR.

- **Expected output (bắt buộc)**:

```text
HTTP 500 Internal Server Error
response.messageCode = INTERNAL_SERVER_ERROR
response không chứa value.items hoặc dữ liệu một phần
```

Giải thích kết quả:

- Danh sách rỗng chỉ đúng khi query thành công nhưng không có record; ở đây query đã thất bại.
- Không trả dữ liệu một phần vì client không thể biết phần nào đáng tin cậy.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận lỗi truy vấn không bị che thành kết quả rỗng và response lỗi giữ đúng contract.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BƯỚC 3 — API / Mã lỗi INTERNAL_SERVER_ERROR
- **Ghi chú**: Kiểm tra lỗi repository được trả HTTP 500.

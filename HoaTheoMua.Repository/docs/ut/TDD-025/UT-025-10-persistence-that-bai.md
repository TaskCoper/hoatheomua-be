# UT-025-10: Rollback khi lưu Update thất bại

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Rollback khi lưu Update thất bại
- **Ghi chú**: Request đổi imageUrl và repository Save ném exception, đại diện cho lỗi database ở bất kỳ mutable field. URL cũ và toàn bộ record persisted phải giữ nguyên; không gọi File Storage để cleanup.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-10
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Cập nhật
- **Unit under test (bắt buộc)**: `CardTemplateService.UpdateAsync`
- **Loại**: Error
- **Mục đích kiểm thử**: Xác nhận lỗi persistence không để lại bản cập nhật dở dang.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. Template có imageUrl=https://storage.example.com/old.png và updatedAt=T1.
3. Request URL mới hợp lệ; repository ném exception khi Save.
4. File Storage call count=0.

- **Dữ liệu đại diện**:

- Lỗi khi đổi imageUrl đại diện cho lỗi Save khi đổi name, description hoặc isActive.
- Exception đại diện cho timeout, mất kết nối hoặc database từ chối ghi.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"imageUrl":"https://storage.example.com/new.png"}
```

- **Các bước thực hiện**:

1. Service validation thành công và tạm gán URL mới.
2. Repository Save ném exception.
3. Transaction rollback; service trả 500.

- **Expected output (bắt buộc)**:

```text
HTTP 500 Internal Server Error
response.messageCode = INTERNAL_SERVER_ERROR
Persisted imageUrl = https://storage.example.com/old.png
Persisted updatedAt = T1
File Storage call count = 0
```

Giải thích kết quả:

- Chỉ trạng thái persisted được coi là kết quả; entity tạm trong bộ nhớ không được để lọt sang lần Save sau.
- Không cleanup URL mới vì API không upload hoặc sở hữu file.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận Update có tính nguyên tử và bảo toàn dữ liệu cũ khi Save thất bại.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-07, BR-021-08
- **Ghi chú**: Kiểm tra lưu URL trực tiếp và rollback khi persistence lỗi.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BƯỚC 3 — API / Mã lỗi INTERNAL_SERVER_ERROR
- **Ghi chú**: Kiểm tra HTTP 500.

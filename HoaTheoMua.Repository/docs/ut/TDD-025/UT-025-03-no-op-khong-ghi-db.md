# UT-025-03: Không ghi database khi Update không tạo thay đổi

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không ghi database khi Update không tạo thay đổi
- **Ghi chú**: Request gửi name có khoảng trắng hai đầu và imageUrl bằng giá trị hiện tại. Sau chuẩn hóa, không field nào thay đổi. Dữ liệu này đại diện cho mọi tổ hợp name, description, imageUrl hoặc isActive có giá trị hiệu lực giống record.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-03
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Cập nhật
- **Unit under test (bắt buộc)**: `CardTemplateService.UpdateAsync`
- **Loại**: Determinism
- **Mục đích kiểm thử**: Tránh phát sinh write và timestamp giả khi nội dung thực tế không đổi.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. Template có name=Thiệp sinh nhật, imageUrl=https://storage.example.com/current.png, updatedAt=T1.
3. Repository Save count ban đầu bằng 0.

- **Dữ liệu đại diện**:

- Name sau trim và URL giống hiện tại đại diện cho các request no-op.
- isActive hoặc description được gửi bằng đúng giá trị hiện tại cũng phải có cùng hành vi.

- **Input**:

```http
PATCH /api/card-templates/550e8400-e29b-41d4-a716-446655440025
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"  Thiệp sinh nhật  ","imageUrl":"https://storage.example.com/current.png"}
```

- **Các bước thực hiện**:

1. Service tải record và chuẩn hóa name.
2. Service so sánh từng field hiệu lực với dữ liệu hiện tại.
3. Service xác định không có thay đổi và trả resource mà không Save.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
Resource trả về giống record hiện tại
Repository Save count = 0
value.updatedAt = T1
```

Giải thích kết quả:

- Khoảng trắng bị trim nên name hiệu lực không đổi.
- Không cập nhật updatedAt vì không có dữ liệu persisted nào thay đổi.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận no-op ổn định và không tạo thao tác ghi không cần thiết.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-11
- **Ghi chú**: Kiểm tra no-op không Save và giữ updatedAt.

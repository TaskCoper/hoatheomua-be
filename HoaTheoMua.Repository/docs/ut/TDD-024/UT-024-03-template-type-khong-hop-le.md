# UT-024-03: Từ chối templateType không đúng lowercase allowlist

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối templateType không đúng lowercase allowlist
- **Ghi chú**: Giá trị AI viết hoa là dữ liệu chính, đại diện cho thiếu field, null, rỗng, sai kiểu và mọi chuỗi ngoài đúng ai hoặc handmade viết thường. Các trường hợp đều trả mã chuyên biệt CARD_TEMPLATE_TYPE_INVALID.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024-03
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
- **Mục đích kiểm thử**: Ngăn lưu discriminator không hợp lệ làm Template bị phân nhánh sai giữa AI và HandMade.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. name và imageUrl hợp lệ.
3. Repository insert count ban đầu bằng 0.

- **Dữ liệu đại diện**:

- AI đại diện cho Ai, HANDMADE, giá trị khác allowlist, thiếu và null vì tất cả không đúng contract lowercase.
- Chỉ chính xác ai hoặc handmade mới được chấp nhận.

- **Input**:

```http
POST /api/card-templates
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"Thiệp AI","templateType":"AI","imageUrl":"https://storage.example.com/ai.png"}
```

- **Các bước thực hiện**:

1. Service đọc templateType.
2. Validation so sánh với allowlist ai và handmade.
3. Giá trị AI không khớp nên request dừng trước insert.

- **Expected output (bắt buộc)**:

```text
HTTP 400 Bad Request
response.messageCode = CARD_TEMPLATE_TYPE_INVALID
Repository insert count = 0
```

Giải thích kết quả:

- Phân biệt chữ hoa/thường là chủ ý của contract và database chỉ lưu lowercase.
- Không tự sửa AI thành ai vì client phải gửi đúng discriminator.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận loại Template luôn có giá trị chuẩn để các flow Card phân nhánh an toàn.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-04
- **Ghi chú**: Kiểm tra templateType bắt buộc và lowercase allowlist.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BƯỚC 3 — API / Mã lỗi CARD_TEMPLATE_TYPE_INVALID
- **Ghi chú**: Kiểm tra HTTP 400 chuyên biệt.

# UT-024-04: Từ chối Create khi thiếu imageUrl

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối Create khi thiếu imageUrl
- **Ghi chú**: Request không có key imageUrl đại diện cho imageUrl=null, chuỗi rỗng hoặc chỉ khoảng trắng. Đây là nhóm không cung cấp ảnh và phải trả CARD_TEMPLATE_IMAGE_REQUIRED/400.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024-04
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
- **Mục đích kiểm thử**: Bảo đảm mọi Card Template mới đều có URL ảnh bắt buộc.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. name và templateType hợp lệ.
3. Request không có imageUrl.
4. Repository insert count=0 và File Storage call count=0.

- **Dữ liệu đại diện**:

- Không truyền, null, rỗng và blank đều có nghĩa là không có URL ảnh.
- URL có mặt nhưng sai định dạng/quá 2048 ký tự thuộc VALIDATION_ERROR, không phải lỗi required.

- **Input**:

```http
POST /api/card-templates
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"Thiệp thiếu ảnh","templateType":"handmade"}
```

- **Các bước thực hiện**:

1. Service kiểm tra các field bắt buộc.
2. Service phát hiện không có imageUrl dùng được.
3. Request dừng trước insert.

- **Expected output (bắt buộc)**:

```text
HTTP 400 Bad Request
response.messageCode = CARD_TEMPLATE_IMAGE_REQUIRED
Repository insert count = 0
File Storage call count = 0
```

Giải thích kết quả:

- Template không có ảnh không thể phục vụ preview hoặc tạo Card.
- API không upload hộ client nên không có bước tìm ảnh thay thế.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận imageUrl là dependency bắt buộc và lỗi được báo bằng mã riêng.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-05
- **Ghi chú**: Kiểm tra imageUrl bắt buộc.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BƯỚC 3 — API / Mã lỗi CARD_TEMPLATE_IMAGE_REQUIRED
- **Ghi chú**: Kiểm tra HTTP 400.

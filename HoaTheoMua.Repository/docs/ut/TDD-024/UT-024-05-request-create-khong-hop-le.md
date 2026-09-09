# UT-024-05: Từ chối request Create có field chung không hợp lệ

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối request Create có field chung không hợp lệ
- **Ghi chú**: name chỉ có khoảng trắng là dữ liệu chính. Nó đại diện cho name thiếu/null/rỗng/sai kiểu, imageUrl sai URL/quá 2048/sai kiểu, client truyền isActive/isDeleted hoặc field không được phép; các lỗi chung trả VALIDATION_ERROR. Lỗi templateType và thiếu imageUrl đã có mã riêng ở UT-024-03/04.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024-05
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
- **Mục đích kiểm thử**: Dừng mọi request không đúng shape hoặc validation chung trước khi ghi dữ liệu.

- **Precondition / Mock setup**:

1. Admin hợp lệ.
2. templateType và imageUrl hợp lệ để cô lập lỗi name.
3. name chỉ có khoảng trắng.
4. Repository insert count=0.

- **Dữ liệu đại diện**:

- name blank đại diện cho nhóm validation chung có cùng messageCode.
- Không đại diện cho templateType sai hoặc imageUrl bị thiếu vì hai trường hợp đó có mã lỗi chuyên biệt.

- **Input**:

```http
POST /api/card-templates
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"   ","templateType":"ai","imageUrl":"https://storage.example.com/ai.png"}
```

- **Các bước thực hiện**:

1. Service trim name.
2. Giá trị sau trim là chuỗi rỗng.
3. Validation trả lỗi trước khi tạo entity.

- **Expected output (bắt buộc)**:

```text
HTTP 400 Bad Request
response.messageCode = VALIDATION_ERROR
response.errors chỉ rõ name
Repository insert count = 0
File Storage call count = 0
```

Giải thích kết quả:

- name là field bắt buộc dùng để người dùng nhận biết Template.
- Không lưu record có tên rỗng và không có side effect file.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận lớp validation chung bảo vệ database khỏi dữ liệu Create sai.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-02, BR-020-05, BR-020-07
- **Ghi chú**: Kiểm tra validation name, URL và field trạng thái không được phép.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BƯỚC 3 — API / Mã lỗi VALIDATION_ERROR
- **Ghi chú**: Kiểm tra lỗi chung HTTP 400.

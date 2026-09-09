# UT-022-06: Từ chối lấy danh sách khi chưa được xác thực

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối lấy danh sách khi chưa được xác thực
- **Ghi chú**: Request không có token đại diện cho token sai, token hết hạn và tài khoản inactive. Điểm chung là hệ thống không hình thành được một principal hoạt động, nên phải trả 401 trước khi kiểm tra filter hoặc query database.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-022-06
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
- **Mục đích kiểm thử**: Bảo vệ danh sách Card Template khỏi request không có phiên đăng nhập hợp lệ.

- **Precondition / Mock setup**:

1. Request không có Authorization header.
2. Không có authenticated principal.
3. Repository query count ban đầu bằng 0.

- **Dữ liệu đại diện**:

- Không token, token sai, token hết hạn và tài khoản inactive cùng thuộc nhóm UNAUTHORIZED theo TDD-022.
- Những trường hợp này khác role sai: role sai có danh tính hợp lệ và trả 403.

- **Input**:

```http
GET /api/card-templates?templateType=ai
```

- **Các bước thực hiện**:

1. Authentication pipeline không tìm được phiên người dùng hợp lệ.
2. Request bị dừng trước Controller/Service.
3. API trả lỗi 401.

- **Expected output (bắt buộc)**:

```text
HTTP 401 Unauthorized
response.messageCode = UNAUTHORIZED
Repository query count = 0
```

Giải thích kết quả:

- Chưa xác thực được người gọi nên hệ thống không được đọc dữ liệu nghiệp vụ.
- Không trả 403 vì chưa đến bước kiểm tra role.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận endpoint danh sách bắt buộc người dùng đã đăng nhập và tài khoản hoạt động.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-01
- **Ghi chú**: Kiểm tra điều kiện authentication của endpoint.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BƯỚC 3 — API / Mã lỗi UNAUTHORIZED
- **Ghi chú**: Kiểm tra HTTP 401 và không query DB.

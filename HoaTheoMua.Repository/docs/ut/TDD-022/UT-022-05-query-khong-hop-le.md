# UT-022-05: Từ chối query danh sách không hợp lệ

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối query danh sách không hợp lệ
- **Ghi chú**: Request Customer thiếu templateType là dữ liệu chính. Nó đại diện cho templateType khác lowercase ai/handmade, page nhỏ hơn 1, pageSize ngoài 5/10/20/30/40/50 và filter sai kiểu; các lỗi đều thuộc validation query và phải dừng trước repository.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-022-05
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
- **Mục đích kiểm thử**: Bảo đảm input sai được báo rõ là lỗi phía client và không tạo truy vấn database.

- **Precondition / Mock setup**:

1. Người gọi là Customer đã đăng nhập và tài khoản đang hoạt động.
2. Request không có templateType.
3. Repository được theo dõi và phải có số lần query bằng 0.

- **Dữ liệu đại diện**:

- Thiếu templateType đại diện cho mọi lỗi query cùng trả VALIDATION_ERROR/400.
- Giá trị AI viết hoa cũng không hợp lệ vì contract chỉ nhận lowercase ai hoặc handmade.
- page=0 hoặc pageSize=15 không hợp lệ vì nằm ngoài giới hạn TDD.

- **Input**:

```http
GET /api/card-templates?page=1&pageSize=10
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Service xác định người gọi là Customer.
2. Service kiểm tra query và phát hiện thiếu templateType bắt buộc.
3. Service trả validation error trước khi tạo câu query dữ liệu.

- **Expected output (bắt buộc)**:

```text
HTTP 400 Bad Request
response.messageCode = VALIDATION_ERROR
response.errors chỉ rõ templateType là field bắt buộc với Customer
Repository query count = 0
```

Giải thích kết quả:

- Customer phải chỉ rõ màn hình AI hay HandMade để không trộn hai loại Template.
- Không query database vì lỗi đã có thể xác định hoàn toàn từ request.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận query sai bị chặn sớm và trả một contract lỗi ổn định.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-03, BR-018-05
- **Ghi chú**: Kiểm tra templateType bắt buộc và allowlist lowercase.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-07
- **Ghi chú**: Kiểm tra lớp validation pagination.

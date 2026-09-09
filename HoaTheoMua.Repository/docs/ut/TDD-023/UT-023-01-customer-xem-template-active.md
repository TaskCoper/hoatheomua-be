# UT-023-01: Customer xem chi tiết Template Active thành công

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Customer xem chi tiết Template Active thành công
- **Ghi chú**: Template HandMade được dùng làm dữ liệu đại diện cho cả hai loại handmade và ai vì response detail có cùng field và cùng quy tắc visibility. Kịch bản kiểm tra đầy đủ dữ liệu trả về, không chỉ kiểm tra HTTP 200.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023-01
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Chi tiết
- **Unit under test (bắt buộc)**: `CardTemplateService.GetByIdAsync`
- **Loại**: Happy
- **Mục đích kiểm thử**: Xác nhận Customer xem được một Template đang hoạt động, chưa xóa và nhận đủ thông tin để hiển thị.

- **Precondition / Mock setup**:

1. Customer đã đăng nhập và tài khoản đang hoạt động.
2. Repository có Template ID 550e8400-e29b-41d4-a716-446655440023, name=Thiệp sinh nhật viết tay, description=Mẫu có sẵn, imageUrl=https://storage.example.com/hm.png.
3. Template có templateType=handmade, isActive=true, isDeleted=false, createdAt=T0 và updatedAt=T1.
4. Repository query thành công; File Storage mock có call count=0.

- **Dữ liệu đại diện**:

- handmade đại diện cho ai vì cả hai loại được trả cùng schema; chỉ giá trị templateType khác.
- Điều kiện quan trọng với Customer là isActive=true và isDeleted=false.

- **Input**:

```http
GET /api/card-templates/550e8400-e29b-41d4-a716-446655440023
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Service xác nhận Customer là role được phép truy cập.
2. Repository tìm Template theo ID.
3. Service kiểm tra not-found, deleted rồi inactive theo đúng thứ tự.
4. Service ánh xạ toàn bộ record sang response.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
value.id = 550e8400-e29b-41d4-a716-446655440023
value.name = Thiệp sinh nhật viết tay
value.description = Mẫu có sẵn
value.imageUrl = https://storage.example.com/hm.png
value.templateType = handmade
value.isActive = true
value.isDeleted = false
value.createdAt = T0; value.updatedAt = T1
```

Giải thích kết quả:

- Customer được xem vì Template vừa active vừa chưa xóa.
- API chỉ trả URL đã lưu; File Storage không được gọi để tải nội dung ảnh.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận happy path và mapping đầy đủ của API chi tiết dành cho Customer.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BR-019-05
- **Ghi chú**: Kiểm tra visibility Customer.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BR-019-06, BR-019-07
- **Ghi chú**: Kiểm tra đầy đủ field và templateType lowercase.

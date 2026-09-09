# UT-024-01: Admin tạo Card Template thành công

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Admin tạo Card Template thành công
- **Ghi chú**: Request dùng templateType=handmade đại diện cho cả handmade và ai vì quy trình tạo giống nhau, chỉ khác discriminator được lưu. Database đã có Template trùng tên để chứng minh name không unique; name và description có khoảng trắng hai đầu để kiểm tra trim.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024-01
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Tạo mới
- **Unit under test (bắt buộc)**: `CardTemplateService.CreateAsync`
- **Loại**: Happy
- **Mục đích kiểm thử**: Xác nhận request hợp lệ tạo một record mới với dữ liệu chuẩn hóa và trạng thái mặc định.

- **Precondition / Mock setup**:

1. Admin đã đăng nhập, tài khoản đang hoạt động và có quyền Create.
2. Database đã có Template khác tên hiển thị Thiệp sinh nhật nhưng có ID khác.
3. Repository insert thành công; đồng hồ UTC cố định T1=2026-09-08T08:00:00Z.
4. File Storage mock có call count=0.

- **Dữ liệu đại diện**:

- templateType=handmade đại diện cho ai vì cùng validation/persistence; response chỉ đổi templateType.
- Template trùng tên chứng minh hệ thống không dùng name làm khóa duy nhất.

- **Input**:

```http
POST /api/card-templates
Authorization: Bearer <token-admin-hop-le>
Content-Type: application/json

{"name":"  Thiệp sinh nhật  ","description":"  Mẫu do Admin thiết kế  ","templateType":"handmade","imageUrl":"https://storage.example.com/hm.png"}
```

- **Các bước thực hiện**:

1. Service kiểm tra name, description, templateType và imageUrl.
2. Service trim name/description nhưng giữ nguyên URL hợp lệ.
3. Service tạo record mới với isActive=true và isDeleted=false.
4. Repository insert và Save thành công.

- **Expected output (bắt buộc)**:

```text
HTTP 201 Created
value.id là UUID mới
value.name = Thiệp sinh nhật
value.description = Mẫu do Admin thiết kế
value.templateType = handmade
value.imageUrl = https://storage.example.com/hm.png
value.isActive = true; value.isDeleted = false
value.createdAt = T1
```

Giải thích kết quả:

- Trùng tên vẫn tạo được vì TDD không quy định unique name.
- Không gọi File Storage vì API nhận và lưu trực tiếp imageUrl.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận happy path, normalization, discriminator và trạng thái mặc định của Template mới.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-02 đến BR-020-08
- **Ghi chú**: Kiểm tra validation, trim, URL, type và default state.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BR-020-10
- **Ghi chú**: Kiểm tra tên trùng vẫn có ID riêng.

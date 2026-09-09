# UT-022-01: Customer lấy Card Template đúng phạm vi được phép xem

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Customer lấy Card Template đúng phạm vi được phép xem
- **Ghi chú**: Request dùng templateType=handmade và cố truyền isActive=false. Dữ liệu này đại diện cho cả ba cách Customer truyền isActive: bỏ trống, true hoặc false; service luôn ép điều kiện isActive=true. Hai loại ai và handmade dùng cùng cơ chế lọc, chỉ khác giá trị templateType.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-022-01
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Danh sách
- **Unit under test (bắt buộc)**: `CardTemplateService.GetListAsync`
- **Loại**: Happy
- **Mục đích kiểm thử**: Chứng minh Customer chỉ nhìn thấy Template đang hoạt động, chưa xóa và đúng loại của màn hình đang sử dụng.

- **Precondition / Mock setup**:

1. Người gọi là Customer đã đăng nhập và tài khoản đang hoạt động.
2. Repository có HM-01: templateType=handmade, isActive=true, isDeleted=false.
3. Repository có HM-02: templateType=handmade, isActive=false, isDeleted=false.
4. Repository có HM-03: templateType=handmade, isActive=true, isDeleted=true.
5. Repository có AI-01: templateType=ai, isActive=true, isDeleted=false.
6. page=1 và pageSize=10; repository query thành công.

- **Dữ liệu đại diện**:

- isActive=false trong request chứng minh Customer không thể dùng filter để xem Template inactive; server vẫn bắt buộc lọc active.
- templateType=handmade đại diện cho templateType=ai vì cả hai dùng cùng điều kiện visibility; kết quả chỉ thay đổi loại Template được trả về.

- **Input**:

```http
GET /api/card-templates?templateType=handmade&isActive=false&page=1&pageSize=10
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Service kiểm tra role Customer và xác nhận templateType là handmade.
2. Service luôn thêm isDeleted=false, isActive=true và templateType=handmade vào query.
3. Service đếm tổng số record sau khi lọc, sau đó sắp xếp và phân trang.
4. Service ánh xạ record hợp lệ sang response.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
value.items chỉ có HM-01
value.page = 1
value.pageSize = 10
value.totalCount = 1
```

Giải thích kết quả:

- HM-02 không xuất hiện vì inactive; HM-03 không xuất hiện vì đã xóa; AI-01 không xuất hiện vì sai templateType.
- Không có mã lỗi vì danh sách một phần tử là kết quả hợp lệ.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P0
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận filter do Customer gửi không thể nới lỏng quy tắc bảo vệ Template inactive/deleted.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-02, BR-018-03
- **Ghi chú**: Kiểm tra visibility bắt buộc của Customer.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-10
- **Ghi chú**: Kiểm tra response không trộn Template khác loại.

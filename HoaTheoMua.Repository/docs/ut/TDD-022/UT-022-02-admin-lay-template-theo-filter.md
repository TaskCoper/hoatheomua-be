# UT-022-02: Admin lọc danh sách Template AI đang Inactive

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Admin lọc danh sách Template AI đang Inactive
- **Ghi chú**: Bộ lọc templateType=ai và isActive=false đại diện cho các tổ hợp lọc loại/trạng thái khác của Admin. Khi bỏ hai filter, Admin được xem cả AI, HandMade, Active và Inactive nhưng vẫn không bao giờ xem record đã xóa.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-022-02
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
- **Mục đích kiểm thử**: Xác nhận Admin có thể quản lý Template inactive và các filter chỉ trả đúng tập dữ liệu được yêu cầu.

- **Precondition / Mock setup**:

1. Người gọi là Admin đã đăng nhập và tài khoản đang hoạt động.
2. Có AI-01 active/chưa xóa, AI-02 inactive/chưa xóa, AI-03 inactive/đã xóa và HM-01 inactive/chưa xóa.
3. page=1, pageSize=20; repository query thành công.

- **Dữ liệu đại diện**:

- Tổ hợp ai + inactive được chọn vì đồng thời kiểm tra cả templateType và isActive.
- Các tổ hợp ai + active, handmade + active và handmade + inactive dùng cùng thuật toán; khi không truyền filter, chỉ điều kiện isDeleted=false còn bắt buộc.

- **Input**:

```http
GET /api/card-templates?templateType=ai&isActive=false&page=1&pageSize=20
Authorization: Bearer <token-admin-hop-le>
```

- **Các bước thực hiện**:

1. Service xác nhận người gọi là Admin.
2. Service áp dụng isDeleted=false, templateType=ai và isActive=false.
3. Service tính totalCount trên tập đã lọc rồi phân trang.
4. Service trả danh sách cho Admin.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
value.items chỉ có AI-02
value.page = 1
value.pageSize = 20
value.totalCount = 1
```

Giải thích kết quả:

- AI-01 bị loại vì active; AI-03 bị loại vì đã xóa; HM-01 bị loại vì templateType=handmade.
- Admin được nhận AI-02 dù record inactive, vì quyền Admin khác Customer.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận Admin xem được Template inactive nhưng filter loại và soft-delete vẫn được áp dụng chính xác.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-02, BR-018-04
- **Ghi chú**: Kiểm tra quyền xem inactive của Admin và loại record deleted.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-05
- **Ghi chú**: Kiểm tra filter templateType của Admin.

# UT-022-04: Trả danh sách rỗng khi không có Template phù hợp

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Trả danh sách rỗng khi không có Template phù hợp
- **Ghi chú**: Dữ liệu có Template nhưng tất cả đều bị loại bởi visibility/filter, đại diện cho cả trường hợp bảng hoàn toàn không có dữ liệu. Hai tình huống đều tạo tập kết quả rỗng và không phải lỗi hệ thống.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-022-04
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Danh sách
- **Unit under test (bắt buộc)**: `CardTemplateService.GetListAsync`
- **Loại**: Boundary
- **Mục đích kiểm thử**: Xác nhận empty result được trả bằng HTTP 200 với cấu trúc phân trang đầy đủ.

- **Precondition / Mock setup**:

1. Customer đã đăng nhập và yêu cầu Template handmade.
2. Có một Template handmade inactive, một Template handmade đã xóa và một Template AI active.
3. Không có Template handmade nào đồng thời active và chưa xóa.

- **Dữ liệu đại diện**:

- Tập dữ liệu bị lọc hết đại diện cho database rỗng vì kết quả cuối sau query đều không có item.
- Không được đổi empty result thành 404; endpoint đang lấy danh sách chứ không tìm một ID cụ thể.

- **Input**:

```http
GET /api/card-templates?templateType=handmade
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Service áp dụng giá trị mặc định page=1 và pageSize=10.
2. Service lọc đúng loại handmade, isActive=true và isDeleted=false.
3. Không record nào thỏa tất cả điều kiện nên service trả paged result rỗng.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
value.items = []
value.page = 1
value.pageSize = 10
value.totalCount = 0
```

Giải thích kết quả:

- Template inactive, deleted và sai loại đều không được trả về.
- Danh sách rỗng là một kết quả hợp lệ nên không có messageCode lỗi.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận client luôn nhận được cấu trúc danh sách nhất quán ngay cả khi không có dữ liệu phù hợp.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-07, BR-018-09
- **Ghi chú**: Kiểm tra default paging và empty result HTTP 200.

# UT-022-03: Tìm kiếm, sắp xếp và phân trang danh sách ổn định

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Tìm kiếm, sắp xếp và phân trang danh sách ổn định
- **Ghi chú**: Một bộ dữ liệu kiểm tra đồng thời việc trim từ khóa, tìm không phân biệt chữ hoa/thường, sắp theo createdAt DESC rồi id DESC, tính totalCount trước phân trang và lấy đúng pageSize. pageSize=5 đại diện cho các giá trị hợp lệ 10, 20, 30, 40 và 50 vì chỉ khác số phần tử cắt ra.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-022-03
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Danh sách
- **Unit under test (bắt buộc)**: `CardTemplateService.GetListAsync`
- **Loại**: Determinism
- **Mục đích kiểm thử**: Bảo đảm cùng dữ liệu và cùng query luôn tạo ra cùng thứ tự và thông tin phân trang.

- **Precondition / Mock setup**:

1. Customer hợp lệ và truyền templateType=handmade.
2. Có 7 Template active/chưa xóa, tên đều chứa cụm Sinh Nhật với cách viết hoa/thường khác nhau.
3. HM-07 mới nhất; HM-06 và HM-05 có cùng createdAt nhưng ID của HM-06 lớn hơn HM-05; các record còn lại cũ dần.
4. Không có record ngoài tập trên vượt qua search và visibility.

- **Dữ liệu đại diện**:

- Từ khóa có khoảng trắng hai đầu và chữ hoa/thường hỗn hợp đại diện cho các cách nhập tương đương của người dùng.
- pageSize=5 đại diện cho toàn bộ allowlist 5/10/20/30/40/50; thuật toán sort và count không đổi.

- **Input**:

```http
GET /api/card-templates?templateType=handmade&search=%20sInH%20NhẬt%20&page=1&pageSize=5
Authorization: Bearer <token-customer-hop-le>
```

- **Các bước thực hiện**:

1. Service trim search thành sinh nhật và thực hiện so khớp tên không phân biệt hoa/thường.
2. Service thu được 7 record rồi tính totalCount=7.
3. Service sắp createdAt giảm dần; nếu bằng nhau tiếp tục sắp id giảm dần.
4. Service lấy 5 record đầu cho trang 1.

- **Expected output (bắt buộc)**:

```text
HTTP 200 OK
value.totalCount = 7
value.items.Count = 5
Thứ tự bắt đầu: HM-07, HM-06, HM-05, sau đó hai record mới nhất tiếp theo
value.page = 1
value.pageSize = 5
```

Giải thích kết quả:

- totalCount bằng 7 vì được tính trước khi cắt trang.
- HM-06 đứng trước HM-05 vì hai record cùng createdAt và ID giảm dần là tiêu chí phụ.
- Search vẫn khớp dù request có khoảng trắng thừa và khác chữ hoa/thường.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận kết quả danh sách có thể lặp lại, không đổi thứ tự ngẫu nhiên giữa các lần gọi.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-06
- **Ghi chú**: Kiểm tra trim và tìm kiếm không phân biệt hoa/thường.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-022
- **Section**: BR-018-07, BR-018-08
- **Ghi chú**: Kiểm tra pagination và stable sort.

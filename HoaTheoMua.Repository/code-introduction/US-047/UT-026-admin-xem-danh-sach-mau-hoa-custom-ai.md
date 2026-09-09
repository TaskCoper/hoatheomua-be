# UT-026: Unit Tests - Admin xem danh sách mẫu hoa Custom AI

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API Admin xem danh sách mẫu hoa Custom AI.
- **Ghi chú**: Các kịch bản bám theo TDD-026, AC/BR và system test của STORY-047.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-026
- **Phiên bản**: v0.1
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-026-01: Danh sách mặc định đúng item và thứ tự

## Thông tin tài liệu

- **Tiêu đề**: Trả danh sách mẫu hoa hợp lệ mới nhất trước.
- **Ghi chú**: Mỗi History flower có ảnh hợp lệ tạo đúng một item.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetFlowers`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Có hai cặp History/Generated Flower hợp lệ, `type = flower`, chưa xóa, cùng `user_id`, có `image_url`.
  - History mới có `created_at` lớn hơn History cũ.
  - Metadata có `product.name = "Combo sinh nhật"` và `user_input.name = "Nguyễn Văn A"`.
- **Input**:

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10
```

- **Expected output**:

```text
HTTP 200.
response.isSuccess = true.
Danh sách nằm ở response.value.items.
Có đúng 2 item.
Item mới nhất đứng trước.
Mỗi item có historyId, generatedFlowerId, customerId, comboId, comboName, customerName, imageUrl, createdAt.
Không có field giá hoặc bill.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận danh sách cơ bản, dữ liệu hiển thị và thứ tự mới nhất.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: AC-001, AC-008
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: BƯỚC 3 - Ví dụ 1
- **Ghi chú**:

---

## UT-026-02: Tìm theo mã mẫu hoa

## Thông tin tài liệu

- **Tiêu đề**: Keyword là Guid của Generated Flower chỉ trả đúng mẫu hoa.
- **Ghi chú**: Mã thiệp/mẫu hoa trong hệ thống là Guid thật.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetFlowers`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Có hai Generated Flower hợp lệ với ID khác nhau.
  - Keyword parse được thành ID của Flower thứ nhất.
- **Input**:

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10&keyword=fa000000-0000-0000-0000-000000000001
```

- **Expected output**:

```text
HTTP 200.
value.items chỉ có Flower có id = fa000000-0000-0000-0000-000000000001.
totalCount = 1.
Không trả Flower khác.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận tìm kiếm theo mã mẫu hoa.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: Business Rules - Tìm kiếm
- **Ghi chú**:

---

## UT-026-03: Tìm theo mã Order

## Thông tin tài liệu

- **Tiêu đề**: Keyword là Guid của Order chỉ trả mẫu hoa có liên kết Order đó.
- **Ghi chú**: Order được kiểm tra qua `OrderDetails.GeneratedFlowerIds`, không join vào item projection.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetFlowers`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Có Flower A được lưu trong `OrderDetails.GeneratedFlowerIds` của Order O1.
  - Có Flower B không nằm trong O1.
  - Order và OrderDetail chưa bị xóa.
- **Input**:

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10&keyword=0a000000-0000-0000-0000-000000000001
```

- **Expected output**:

```text
HTTP 200.
value.items chỉ có Flower A.
Flower A không bị nhân bản dù có nhiều OrderDetail tham chiếu.
Flower B không xuất hiện.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận tìm kiếm Order đúng và không tạo duplicate item.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: AC-002, AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: Kiến trúc - Không nhân bản theo Order
- **Ghi chú**:

---

## UT-026-04: Tìm theo tên người nhập

## Thông tin tài liệu

- **Tiêu đề**: Tìm theo tên người nhập trong snapshot/input.
- **Ghi chú**: Tên này là tên gửi vào form tạo hoa, không lấy từ User Service.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetFlowers`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - History có snapshot/input chứa tên `Nguyễn Văn A`.
  - Flower khác có tên người nhập khác.
- **Input**:

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10&keyword=Nguyễn Văn A
```

- **Expected output**:

```text
HTTP 200.
Chỉ item có tên người nhập phù hợp xuất hiện.
customerName = "Nguyễn Văn A".
Không gọi User Service để tìm tên account.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận tìm kiếm và mapping customerName theo snapshot.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: Response item - customerName
- **Ghi chú**:

---

## UT-026-05: Lọc theo nhiều Combo

## Thông tin tài liệu

- **Tiêu đề**: Lọc được nhiều Combo và loại ID rỗng/trùng.
- **Ghi chú**: Filter dùng `client_histories.base_id`.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetFlowers`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Có Flower từ Combo C1, C2 và C3.
  - Request chứa C1, C1, C2 và `Guid.Empty`.
- **Input**:

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10&comboIds=c1000000-0000-0000-0000-000000000001&comboIds=c1000000-0000-0000-0000-000000000001&comboIds=00000000-0000-0000-0000-000000000000&comboIds=c1000000-0000-0000-0000-000000000002
```

- **Expected output**:

```text
HTTP 200.
Chỉ trả Flower có base_id = C1 hoặc C2.
Flower của C3 không xuất hiện.
ID trùng và Guid.Empty không làm sai kết quả.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận bộ lọc một/nhiều Combo theo base_id.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: AC-004
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: Request query - comboIds
- **Ghi chú**:

---

## UT-026-06: Phân trang và pageSize không hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: Phân trang đúng và từ chối pageSize ngoài danh sách cho phép.
- **Ghi chú**: Bao phủ pageSize hợp lệ và nhánh lỗi.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetFlowers`
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Có 11 Flower hợp lệ, sắp xếp theo created_at.
- **Input**:

```text
Case A: pageIndex=2, pageSize=10.
Case B: pageIndex=1, pageSize=15.
```

- **Expected output**:

```text
Case A: HTTP 200, value.items có 1 item, pageIndex=2, pageSize=10, totalCount=11, hasPreviousPage=true, hasNextPage=false.
Case B: HTTP 400, message code tương ứng INVALID_PAGE_SIZE, không trả dữ liệu danh sách.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận phân trang phía server và giới hạn pageSize.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: Mã lỗi - INVALID_PAGE_SIZE
- **Ghi chú**:

---

## UT-026-07: Empty state

## Thông tin tài liệu

- **Tiêu đề**: Không có kết quả trả danh sách rỗng.
- **Ghi chú**: Không trả dữ liệu của điều kiện cũ.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetFlowers`
- **Loại**: Branch
- **Precondition / Mock setup**: Không có bản ghi Flower phù hợp keyword/filter.
- **Input**:

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10&keyword=khong-ton-tai
```

- **Expected output**:

```text
HTTP 200.
value.items = [].
totalCount = 0.
hasNextPage = false.
Không trả item cũ hoặc item không thỏa điều kiện.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận empty/no-result state của API.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: EXC-01
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: BƯỚC 3 - Ví dụ 3
- **Ghi chú**:

---

## UT-026-08: Chặn tài khoản không có quyền

## Thông tin tài liệu

- **Tiêu đề**: Người dùng không có quyền không nhận được danh sách.
- **Ghi chú**: Kiểm tra ở Controller/policy.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlowerController.GetFlowers`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Case 1: không có token.
  - Case 2: có token nhưng không có quyền AI Custom.
  - Có dữ liệu Flower hợp lệ để chắc chắn request bị chặn bởi quyền.
- **Input**:

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10
```

- **Expected output**:

```text
Chưa đăng nhập: HTTP 401.
Không có quyền: HTTP 403.
Service không được gọi.
Không trả ảnh, khách hàng, Combo hoặc metadata.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận API danh sách luôn kiểm tra quyền ở Backend.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: AC-009, BR-132
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: Mã lỗi - UNAUTHORIZED, FORBIDDEN
- **Ghi chú**:

---

## UT-026-09: Kết quả có nhiều Order không bị lặp

## Thông tin tài liệu

- **Tiêu đề**: Một mẫu hoa vẫn chỉ có một item dù liên kết nhiều Order.
- **Ghi chú**: Order chỉ phục vụ điều kiện tìm kiếm, không làm tăng số dòng danh sách.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetFlowers`
- **Loại**: Quirk
- **Precondition / Mock setup**:
  - Một History/Generated Flower hợp lệ được tham chiếu bởi hai Order khác nhau.
  - Không truyền keyword Order.
- **Input**:

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10
```

- **Expected output**:

```text
HTTP 200.
Flower chỉ xuất hiện 1 lần trong value.items.
totalCount tăng 1 theo kết quả Flower, không tăng theo số Order.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Khóa rule không duplicate theo Order liên kết.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-047
- **Section**: AC-002, BR-144
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-026
- **Section**: Business Rules - Một item theo kết quả AI
- **Ghi chú**:


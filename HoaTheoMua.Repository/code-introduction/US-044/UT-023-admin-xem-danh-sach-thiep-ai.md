# UT-023: Unit Tests - Admin xem danh sách thiệp AI

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API Admin xem danh sách thiệp AI.
- **Ghi chú**: Mỗi UT bên dưới kiểm tra một nhánh xử lý chính của TDD-023.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-023
- **Phiên bản**: v0.1
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-023-01: Lấy danh sách thiệp AI thành công

## Thông tin tài liệu

- **Tiêu đề**: Lấy danh sách thiệp AI thành công.
- **Ghi chú**: Kiểm tra luồng chính khi Admin mở màn hình danh sách.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Có 3 `client_histories` có `type = card`, `is_deleted = false`.
  - Mỗi history có `output_id` trỏ đến một `generated_cards.id`.
  - 3 card đều có `image_url`, `is_deleted = false`.
  - 3 history có ngày tạo khác nhau.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10
```

- **Expected output**:

```text
HTTP 200
Trả 3 item.
Danh sách sắp xếp mới nhất trước.
Mỗi item có đủ thông tin hiển thị trên list: mã history, mã thiệp, khách hàng, ảnh, template, size, hình thức, ngày tạo.
Response có đầy đủ field của generated card, bao gồm basePrice, extraPrice, totalPrice.
Response không có bill snapshot của Order.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận Admin xem được danh sách thiệp AI theo luồng chính.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-001, AC-008
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: BƯỚC 3 - API
- **Ghi chú**:

---

## UT-023-02: Chỉ lấy thiệp AI hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: Chỉ lấy history type card và card có ảnh.
- **Ghi chú**: Kiểm tra API không trả nhầm flower, card bị xóa hoặc card chưa có ảnh.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - 1 history type `card`, card có `image_url`.
  - 1 history type `flower`.
  - 1 history type `card`, card `image_url = null`.
  - 1 history type `card`, card `image_url = ""`.
  - 1 history/card có `is_deleted = true`.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10
```

- **Expected output**:

```text
HTTP 200
Chỉ trả item của history type card có card hợp lệ và có image_url.
Không trả history/card đã xóa.
Không gọi AI tạo lại ảnh.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận danh sách chỉ gồm thiệp AI có thể hiển thị.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-001, AC-007
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: Business Rules
- **Ghi chú**:

---

## UT-023-03: Không duplicate khi thiệp liên kết nhiều Order

## Thông tin tài liệu

- **Tiêu đề**: Một thiệp liên kết nhiều Order vẫn chỉ hiển thị một dòng.
- **Ghi chú**: Kiểm tra rule chống nhân bản item.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Có 1 generated card hợp lệ.
  - Có 1 history trỏ đến generated card đó.
  - Card này nằm trong `order_details.generated_card_ids` của 3 Order khác nhau.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10
```

- **Expected output**:

```text
HTTP 200
Card chỉ xuất hiện 1 item.
totalCount = 1.
Số Order liên kết không làm tăng số dòng trong danh sách.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận list được tính theo history/card, không tính theo Order.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-002
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: Business Rules
- **Ghi chú**:

---

## UT-023-04: Tìm kiếm theo mã thiệp

## Thông tin tài liệu

- **Tiêu đề**: Admin tìm kiếm bằng generatedCardId.
- **Ghi chú**: Mã thiệp trong US này là GUID của `generated_cards.id`.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Có nhiều generated card hợp lệ.
  - Keyword là GUID của 1 `generated_cards.id`.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10&keyword=22222222-2222-2222-2222-222222222222
```

- **Expected output**:

```text
HTTP 200
Chỉ trả card có generated_cards.id đúng bằng keyword.
Không trả các card khác.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận search theo mã thiệp hoạt động đúng.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: API - Ví dụ tìm kiếm theo mã thiệp
- **Ghi chú**:

---

## UT-023-05: Tìm kiếm theo tên người gửi

## Thông tin tài liệu

- **Tiêu đề**: Admin tìm kiếm bằng tên người gửi.
- **Ghi chú**: Tên hiển thị ở list lấy từ `generated_cards.sender_name`.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Card A có `sender_name = "Nguyễn Văn A"`.
  - Card B có `sender_name = "Trần Văn B"`.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10&keyword=Nguyễn
```

- **Expected output**:

```text
HTTP 200
Trả Card A.
Không trả Card B.
Search tên dùng field generated_cards.sender_name.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận search theo tên không lấy nhầm từ bảng Order/User.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: Business Rules
- **Ghi chú**:

---

## UT-023-06: Tìm kiếm theo mã Order

## Thông tin tài liệu

- **Tiêu đề**: Admin tìm kiếm thiệp bằng OrderId.
- **Ghi chú**: Mã Order trong US này là GUID của `orders.id`.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Order X có `orders.id = orderId`.
  - `order_details.generated_card_ids` của Order X chứa `generatedCardId`.
  - Có card khác không nằm trong Order X.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10&keyword=44444444-4444-4444-4444-444444444444
```

- **Expected output**:

```text
HTTP 200
Trả card nằm trong Order X.
Không trả card không liên quan Order X.
Chỉ search Order khi keyword là GUID hợp lệ.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận Admin có thể tìm thiệp đã gắn vào một Order cụ thể.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: API - Search theo keyword
- **Ghi chú**:

---

## UT-023-07: Lọc theo size và hình thức

## Thông tin tài liệu

- **Tiêu đề**: Admin lọc danh sách theo size và hình thức thiệp.
- **Ghi chú**: Khi chọn cả 2 filter, card phải thỏa cả 2 điều kiện.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Card A: `size_key = "10x15"`, `form_type = Calligraphy`.
  - Card B: `size_key = "10x15"`, `form_type = Printed`.
  - Card C: `size_key = "15x20"`, `form_type = Calligraphy`.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10&sizeKey=10x15&formType=Calligraphy
```

- **Expected output**:

```text
HTTP 200
Chỉ trả Card A.
Card B bị loại vì sai formType.
Card C bị loại vì sai sizeKey.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận filter được áp dụng đồng thời.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-004
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: API - Ví dụ lọc theo size và hình thức
- **Ghi chú**:

---

## UT-023-08: Phân trang danh sách

## Thông tin tài liệu

- **Tiêu đề**: API trả đúng trang dữ liệu được yêu cầu.
- **Ghi chú**: Kiểm tra pageIndex/pageSize và totalCount.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Có 12 card hợp lệ.
  - Các card có ngày tạo khác nhau.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=2&pageSize=5
```

- **Expected output**:

```text
HTTP 200
Trả 5 item của trang 2.
pageIndex = 2.
pageSize = 5.
totalCount = 12.
Danh sách vẫn giữ thứ tự mới nhất trước.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận API phân trang đúng để FE render bảng ổn định.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: API - Phân trang
- **Ghi chú**:

---

## UT-023-09: PageSize không hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: API báo lỗi khi pageSize không nằm trong danh sách cho phép.
- **Ghi chú**: Page size hợp lệ gồm `5, 10, 20, 30, 40, 50`.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Không cần dữ liệu đặc biệt.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=99
```

- **Expected output**:

```text
HTTP 400
Trả lỗi INVALID_PAGE_SIZE.
Không tiếp tục query danh sách.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận BE chặn pageSize không đúng rule.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: Mã lỗi
- **Ghi chú**:

---

## UT-023-10: Đọc tên template từ snapshot

## Thông tin tài liệu

- **Tiêu đề**: API lấy templateName từ metadata của history.
- **Ghi chú**: Template là snapshot tại thời điểm tạo thiệp, không đọc trực tiếp từ template hiện tại.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetCards`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - `client_histories.metadata` có snapshot:

```json
{
  "card_template": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "name": "Modern Pink"
  }
}
```

- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10
```

- **Expected output**:

```text
HTTP 200
Response item có templateName = "Modern Pink".
Nếu metadata rỗng hoặc sai format thì templateName = null.
API không lỗi toàn bộ danh sách vì một metadata sai.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận tên template hiển thị theo snapshot của lần tạo thiệp.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-001
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: Mô hình dữ liệu
- **Ghi chú**:

---

## UT-023-11: Chặn tài khoản không có quyền

## Thông tin tài liệu

- **Tiêu đề**: Tài khoản không có quyền không được xem danh sách thiệp AI.
- **Ghi chú**: Kiểm tra ở tầng controller/policy.

## Đơn vị kiểm thử

- **Module**: Admin - Danh sách thiệp AI
- **Unit under test**: `AdminGeneratedCardController.GetCards`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Trường hợp 1: request không có token.
  - Trường hợp 2: request có token nhưng không có quyền Admin.
- **Input**:

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10
```

- **Expected output**:

```text
Trường hợp chưa đăng nhập: HTTP 401.
Trường hợp không có quyền: HTTP 403.
Service không được gọi.
Không trả dữ liệu thiệp.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận dữ liệu thiệp AI chỉ được trả cho tài khoản có quyền.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-044
- **Section**: AC-009
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-023
- **Section**: Mã lỗi
- **Ghi chú**:

# UT-030: Unit Tests - Customer xem lịch sử thiệp đã tạo

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API danh sách và chi tiết History thiệp.
- **Ghi chú**: Các kịch bản bám theo TDD-030, STORY-045 và các quy tắc ownership/order liên quan.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-030
- **Phiên bản**: v0.1
- **Author (bắt buộc)**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-030-01: Lấy danh sách thiệp của customer

## Thông tin tài liệu

- **Tiêu đề**: Danh sách trả đúng các thiệp đã tạo của customer.
- **Ghi chú**: Luồng chính của Endpoint #1.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử thiệp
- **Unit under test (bắt buộc)**: `CardHistory.Service.GetAll`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - H01 có `type = card`, `output_id = card-001`, chưa xóa.
  - Generated Card `card-001` cùng user, chưa xóa, có `image_url`, `form_type`, size và `created_at`.
  - Có một History của customer-002 để kiểm tra ownership.
- **Input**:

```http
GET /api/v1/card-histories?pageIndex=1&pageSize=20
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200, response.isSuccess = true.
response.value.items chỉ có card-001 của customer-001.
Item có generatedCardId, imageUrl, formType, sizeName, sizeWidth, sizeHeight, generatedAt.
value.totalCount = 1; items nằm tại response.value.items.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer xem được danh sách thiệp đúng ownership và đúng wrapper phân trang.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-045
- **Section**: AC-001
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-030
- **Section**: Endpoint #1 - Response
- **Ghi chú**:

---

## UT-030-02: Lọc loại History và bản ghi bị xóa

## Thông tin tài liệu

- **Tiêu đề**: Chỉ trả History `type = card` còn hiệu lực.
- **Ghi chú**: Kiểm tra các nhánh lọc trong danh sách.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử thiệp
- **Unit under test (bắt buộc)**: `CardHistory.Service.GetAll`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - H01 hợp lệ: `type = card`, chưa xóa, Card C01 hợp lệ.
  - H02 cùng user nhưng `type = handmade_card`.
  - H03 cùng user nhưng History đã xóa.
  - H04 là History card hợp lệ nhưng Generated Card đã xóa.
  - H05 thuộc customer-002.
- **Input**:

```http
GET /api/v1/card-histories?pageIndex=1&pageSize=20
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
response.value.items chỉ chứa C01.
Không trả H02, H03, H04 hoặc H05.
totalCount = 1.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận không lộ Card History khác loại, đã xóa hoặc thuộc tài khoản khác.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-045
- **Section**: AC-001, AC-002, AC-007
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-030
- **Section**: Business Rules - type, ownership và isDeleted
- **Ghi chú**:

---

## UT-030-03: Sắp xếp và phân trang danh sách

## Thông tin tài liệu

- **Tiêu đề**: Thiệp mới hơn đứng trước và phân trang ổn định.
- **Ghi chú**: Khóa thứ tự khi hai Card có cùng thời điểm tạo.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử thiệp
- **Unit under test (bắt buộc)**: `CardHistory.Service.GetAll`
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Customer có 5 Generated Card hợp lệ.
  - C04 và C05 có cùng `created_at`; C05 có ID lớn hơn C04.
- **Input**:

```http
GET /api/v1/card-histories?pageIndex=2&pageSize=2
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
value.items có đúng hai phần tử thứ 3 và thứ 4 theo createdAt giảm dần,
sau đó generatedCardId giảm dần khi thời điểm bằng nhau.
value.pageIndex = 2, value.pageSize = 2, value.totalCount = 5.
hasPreviousPage = true và hasNextPage = true.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận FE phân trang đúng và thứ tự không thay đổi giữa các lần gọi.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-045
- **Section**: AC-001
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-030
- **Section**: Business Rules - thứ tự và phân trang
- **Ghi chú**:

---

## UT-030-04: Xem chi tiết thiệp đầy đủ

## Thông tin tài liệu

- **Tiêu đề**: Chi tiết trả toàn bộ field Card và snapshot History.
- **Ghi chú**: Luồng chính của Endpoint #2.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Chi tiết lịch sử thiệp
- **Unit under test (bắt buộc)**: `CardHistory.Service.GetById`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - H01 thuộc customer-001, `type = card`, `base_id = source-001`, `output_id = card-001`.
  - Card C01 có đủ `content`, `image_url`, sender/receiver/message, size snapshot, word config snapshot và giá snapshot.
  - Có một OrderDetail chưa xóa chứa `GeneratedCardIds = [card-001]`, Order cùng user và chưa xóa.
- **Input**:

```http
GET /api/v1/card-histories/card-001
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200, response.isSuccess = true.
value.generatedCardId = card-001; value.sourceId = H01.base_id.
value.content, imageUrl, formType, sizeKey, senderName, receiverName,
messageContent, attachedImageUrl, size snapshot, word count, pricing snapshot
và generatedAt lấy từ Generated Card.
value.historyMetadata lấy từ History.metadata.
value.orders chứa Order liên kết.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận màn chi tiết có đủ dữ liệu Card và Order để FE render.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-045
- **Section**: AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-030
- **Section**: Endpoint #2 - Response
- **Ghi chú**:

---

## UT-030-05: Thiệp chưa có Order

## Thông tin tài liệu

- **Tiêu đề**: Chi tiết thiệp chưa sử dụng cho Order vẫn được trả.
- **Ghi chú**: `orders` là mảng rỗng, không phải lỗi.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Chi tiết lịch sử thiệp
- **Unit under test (bắt buộc)**: `CardHistory.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - History và Generated Card hợp lệ của customer-001.
  - Không có OrderDetails nào chứa `GeneratedCardIds` của Card.
- **Input**:

```http
GET /api/v1/card-histories/card-unlinked
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
value chứa đầy đủ dữ liệu Card.
value.orders = [].
Không trả 404 và không tạo Order liên kết mới.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận thiệp chưa dùng cho Order vẫn xuất hiện và xem được bình thường.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-045
- **Section**: AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-030
- **Section**: Endpoint #2 - `orders: []`
- **Ghi chú**:

---

## UT-030-06: Một thiệp liên kết nhiều Order

## Thông tin tài liệu

- **Tiêu đề**: Không nhân bản History và không lặp Order.
- **Ghi chú**: Kiểm tra mảng ID trên OrderDetails.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Chi tiết lịch sử thiệp
- **Unit under test (bắt buộc)**: `CardHistory.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Một Card C01 có History duy nhất.
  - Order A và Order B có OrderDetails chứa C01.
  - Một OrderDetail lặp lại C01 để kiểm tra `Distinct`.
- **Input**:

```http
GET /api/v1/card-histories/card-001
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
Endpoint danh sách chỉ trả C01 một lần.
Endpoint chi tiết trả value.orders đúng hai Order khác nhau.
Order mới hơn đứng trước; không lặp Order.
Không tạo History mới.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận tái sử dụng thiệp không nhân bản History và chi tiết hiển thị đủ Order.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-045
- **Section**: AC-004, BR-135
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-030
- **Section**: Mô hình dữ liệu - OrderDetails.GeneratedCardIds
- **Ghi chú**:

---

## UT-030-07: Không xem thiệp của customer khác

## Thông tin tài liệu

- **Tiêu đề**: Ownership được kiểm tra cho danh sách và chi tiết.
- **Ghi chú**: Không làm lộ Card, metadata hoặc Order liên quan.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử thiệp
- **Unit under test (bắt buộc)**: `CardHistory.Service.GetAll`, `CardHistory.Service.GetById`
- **Loại**: Error
- **Precondition / Mock setup**:
  - C02 và History H02 thuộc customer-002.
  - JWT hiện tại là customer-001.
- **Input**:

```text
Case A: GET /api/v1/card-histories?pageIndex=1&pageSize=20
Case B: GET /api/v1/card-histories/card-002
```

- **Expected output (bắt buộc)**:

```text
Case A: danh sách không chứa C02.
Case B: HTTP 404 với mã CARD_HISTORY_NOT_FOUND.
Không trả imageUrl, historyMetadata hoặc orders của C02.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer chỉ xem được History thuộc tài khoản của mình.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-045
- **Section**: AC-007
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-030
- **Section**: Mã lỗi - CARD_HISTORY_NOT_FOUND
- **Ghi chú**:

---

## UT-030-08: Input không hợp lệ và empty state

## Thông tin tài liệu

- **Tiêu đề**: Chặn tham số sai và trả danh sách rỗng đúng chuẩn.
- **Ghi chú**: Kiểm tra boundary của hai endpoint.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử thiệp
- **Unit under test (bắt buộc)**: `CardHistory.Service.GetAll`, `CardHistory.Service.GetById`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Case A: `pageIndex = 0` hoặc `pageSize = 101`.
  - Case B: customer hợp lệ nhưng không có Card History.
  - Case C: `generatedCardId = Guid.Empty`.
- **Input**:

```text
Case A: GET /api/v1/card-histories?pageIndex=0&pageSize=101
Case B: GET /api/v1/card-histories?pageIndex=1&pageSize=20
Case C: GET /api/v1/card-histories/00000000-0000-0000-0000-000000000000
```

- **Expected output (bắt buộc)**:

```text
Case A: HTTP 400, mã INVALID_PAGE; không query DB.
Case B: HTTP 200, value.items = [], totalCount = 0.
Case C: HTTP 400, mã INVALID_GENERATED_CARD_ID; không query History/Card.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận input sai và trạng thái chưa có dữ liệu được xử lý khác nhau.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-045
- **Section**: EXC-01, AC-006
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-030
- **Section**: Mã lỗi - INVALID_PAGE, INVALID_GENERATED_CARD_ID
- **Ghi chú**:


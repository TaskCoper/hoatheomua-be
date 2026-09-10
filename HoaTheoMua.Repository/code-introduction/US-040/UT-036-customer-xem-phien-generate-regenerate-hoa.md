# UT-036: Unit Tests - Customer xem phiên generate/regenerate hoa

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API lấy các version trong một phiên generate/regenerate.
- **Ghi chú**: Các kịch bản kiểm tra Endpoint #1 của TDD-036.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-036
- **Phiên bản**: v0.1
- **Author (bắt buộc)**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-10

---

## UT-036-01: Chọn version giữa chuỗi trả đủ phiên

## Thông tin tài liệu

- **Tiêu đề**: Customer chọn GF2 và nhận GF1, GF2, GF3.
- **Ghi chú**: Kiểm tra API đi ngược lấy version trước và đi xuôi lấy version sau.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetSession`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - Cùng Combo nguồn `CA` có chuỗi hợp lệ: `GF1(root=CA, baseId=CA)`, `GF2(root=CA, baseId=GF1)`, `GF3(root=CA, baseId=GF2)`.
  - Có `GF4(root=CA, baseId=CA)` tạo mới trực tiếp từ CA, là phiên khác.
  - Tất cả Flower History/Generated Flower trên thuộc customer-001, chưa bị xóa và có `imageUrl`.
- **Input**:

```http
GET /api/v1/flower-histories/GF2/session
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200, isSuccess = true.
rootComboId = CA.
sessionStartGeneratedFlowerId = GF1.
items có đúng thứ tự GF1, GF2, GF3.
GF4 không xuất hiện vì GF4 có baseId = CA, là phiên tạo mới khác.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer xem đúng toàn bộ version thuộc phiên đang chọn, không lẫn phiên khác cùng Combo.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-006, AC-008, BR-095
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-036
- **Section**: Business Rules - chuỗi tuyến tính; Endpoint #1 - Ví dụ 1
- **Ghi chú**:

---

## UT-036-02: Phiên chỉ có một lần generate đầu tiên

## Thông tin tài liệu

- **Tiêu đề**: Customer chọn GF4 không có regenerate và chỉ nhận GF4.
- **Ghi chú**: Kiểm tra trường hợp không có version trước hay sau ảnh FE chọn.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetSession`
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - `GF4(root=CA, baseId=CA)` thuộc customer-001, chưa bị xóa.
  - Không có History hợp lệ có `baseId = GF4`.
- **Input**:

```http
GET /api/v1/flower-histories/GF4/session
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
sessionStartGeneratedFlowerId = GF4.
items chỉ có một phần tử GF4; không có version trước hoặc sau.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận ảnh generate lần đầu vẫn là một phiên hợp lệ dù chưa regenerate.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-003, AC-006
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-036
- **Section**: Endpoint #1 - Ví dụ 2
- **Ghi chú**:

---

## UT-036-03: Chuỗi bị đứt không được tự ghép

## Thông tin tài liệu

- **Tiêu đề**: Customer chọn GF2 nhưng không có History của GF1.
- **Ghi chú**: Kiểm tra dữ liệu không đủ để xác định phiên một cách chắc chắn.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetSession`
- **Loại**: Error
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - `GF2(root=CA, baseId=GF1)` có Flower History và Generated Flower hợp lệ.
  - Không tồn tại Flower History hợp lệ có `outputId = GF1` của customer-001.
- **Input**:

```http
GET /api/v1/flower-histories/GF2/session
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 400, messageCode = BAD_REQUEST.
detail = "Chuỗi tạo mẫu hoa không hợp lệ".
Không trả danh sách version vì không thể xác định chắc chắn ảnh đầu phiên.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận hệ thống không tự đoán quan hệ generate/regenerate khi History bị đứt.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-036
- **Section**: Business Rules - chuỗi bị đứt; Mã lỗi - BAD_REQUEST
- **Ghi chú**:

---

## UT-036-04: Chọn version cuối vẫn trả đúng thứ tự từ đầu phiên

## Thông tin tài liệu

- **Tiêu đề**: Customer chọn GF3 và nhận GF1, GF2, GF3 theo thứ tự cũ đến mới.
- **Ghi chú**: Kiểm tra phần đi ngược nhiều hơn một bước và đảo lại danh sách version trước ảnh được chọn.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetSession`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - Chuỗi hợp lệ: `GF1(root=CA, baseId=CA)`, `GF2(root=CA, baseId=GF1)`, `GF3(root=CA, baseId=GF2)`.
  - Không có History hợp lệ có `baseId = GF3`.
- **Input**:

```http
GET /api/v1/flower-histories/GF3/session
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
sessionStartGeneratedFlowerId = GF1.
items có đúng thứ tự GF1, GF2, GF3; không bị đảo thành GF2, GF1, GF3.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API vẫn trả thứ tự đúng khi FE chọn version cuối chuỗi.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-006, BR-098
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-036
- **Section**: Business Rules - đi ngược và thứ tự cũ đến mới
- **Ghi chú**:

---

## UT-036-05: Chọn version đầu lấy các version regenerate sau nó

## Thông tin tài liệu

- **Tiêu đề**: Customer chọn GF1 và nhận toàn bộ các version đi xuôi.
- **Ghi chú**: Kiểm tra phiên bắt đầu ngay tại ảnh FE chọn.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetSession`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - Chuỗi hợp lệ: `GF1(root=CA, baseId=CA)`, `GF2(root=CA, baseId=GF1)`, `GF3(root=CA, baseId=GF2)`.
- **Input**:

```http
GET /api/v1/flower-histories/GF1/session
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
sessionStartGeneratedFlowerId = GF1.
items có đúng GF1, GF2, GF3 theo thứ tự tạo; API không bỏ GF2 hoặc GF3.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API lấy được toàn bộ version sau ảnh đầu phiên.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-006
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-036
- **Section**: Business Rules - đi xuôi từ ảnh FE chọn
- **Ghi chú**:

---

## UT-036-06: Không xem được phiên của customer khác

## Thông tin tài liệu

- **Tiêu đề**: Customer không thể gửi ID ảnh của customer khác để xem phiên.
- **Ghi chú**: Selection query kiểm tra ownership của cả History và Generated Flower.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetSession`
- **Loại**: Error
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - `GF9` có Flower History và Generated Flower hợp lệ nhưng `userId = customer-002`.
- **Input**:

```http
GET /api/v1/flower-histories/GF9/session
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 404, messageCode = NOT_FOUND.
Không trả rootComboId, sessionStartGeneratedFlowerId, imageUrl hoặc items của customer-002.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận endpoint phiên không làm lộ ảnh hay quan hệ generate của customer khác.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-008, BR-100
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-036
- **Section**: Business Rules - ownership; Mã lỗi - NOT_FOUND
- **Ghi chú**:

---

## UT-036-07: Customer chưa đăng nhập

## Thông tin tài liệu

- **Tiêu đề**: Endpoint phiên yêu cầu xác thực.
- **Ghi chú**: Kiểm tra tại Controller/middleware trước khi Service đọc dữ liệu.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistoryController.GetSession`
- **Loại**: Error
- **Precondition / Mock setup**: Không có Bearer token hoặc token không hợp lệ; DB có chuỗi Flower History hợp lệ.
- **Input**:

```http
GET /api/v1/flower-histories/GF2/session
```

- **Expected output (bắt buộc)**:

```text
HTTP 401.
Không gọi FlowerHistory.Service và không trả version, rootComboId hay metadata ảnh.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer phải xác thực trước khi xem phiên generate/regenerate.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-008, BR-100
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-036
- **Section**: Business Rules - customer phải đăng nhập; Mã lỗi - UNAUTHORIZED
- **Ghi chú**:

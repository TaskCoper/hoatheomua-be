# UT-035: Unit Tests - Customer xem nhóm mẫu hoa cùng Combo nguồn

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API lấy các ảnh hoa AI cùng Combo nguồn.
- **Ghi chú**: Các kịch bản kiểm tra Endpoint #1 của TDD-035.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-035
- **Phiên bản**: v0.1
- **Author (bắt buộc)**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-10

---

## UT-035-01: Trả mọi ảnh cùng Combo nguồn của customer

## Thông tin tài liệu

- **Tiêu đề**: Customer chọn GF2 và nhận toàn bộ ảnh cùng root Combo.
- **Ghi chú**: Bao gồm một chuỗi regenerate và một phiên generate mới từ cùng Combo.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetRootGroup`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - Combo nguồn `CA`.
  - History/Flower hợp lệ của customer-001: `GF1(root=CA, baseId=CA)`, `GF2(root=CA, baseId=GF1)`, `GF3(root=CA, baseId=GF2)`, `GF4(root=CA, baseId=CA)`.
  - `GF4` có thời điểm tạo mới nhất; tất cả có `imageUrl`, chưa bị xóa.
- **Input**:

```http
GET /api/v1/flower-histories/GF2/root-group
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200, isSuccess = true.
rootComboId = CA.
items có đúng GF4, GF3, GF2, GF1 theo generatedAt giảm dần.
GF4 vẫn xuất hiện vì cùng root CA, dù GF4 là một phiên generate khác.
Mỗi item có generatedFlowerId, baseId, imageUrl và generatedAt.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API nhóm theo Combo nguồn, không chỉ theo một chuỗi regenerate.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-006, BR-095, BR-098
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-035
- **Section**: Business Rules - nhóm theo root; Endpoint #1 - Ví dụ 1
- **Ghi chú**:

---

## UT-035-02: Không trả nhóm ảnh của customer khác

## Thông tin tài liệu

- **Tiêu đề**: Customer không thể dùng ID ảnh của customer khác để xem nhóm.
- **Ghi chú**: Kiểm tra ownership trước khi lấy `root`.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetRootGroup`
- **Loại**: Error
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - `GF9` có Flower History và Generated Flower hợp lệ nhưng thuộc `customer-002`.
  - Customer-001 không có Flower History với `outputId = GF9`.
- **Input**:

```http
GET /api/v1/flower-histories/GF9/root-group
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 404, messageCode = NOT_FOUND.
Không trả rootComboId, imageUrl, baseId hoặc bất kỳ item nào của customer-002.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer chỉ xem được nhóm History thuộc chính mình.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-008, BR-100
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-035
- **Section**: Business Rules - ownership; Mã lỗi - NOT_FOUND
- **Ghi chú**:

---

## UT-035-03: Chỉ trả History flower hợp lệ cùng root

## Thông tin tài liệu

- **Tiêu đề**: Nhóm ảnh loại trừ dữ liệu sai loại, khác root hoặc đã xóa.
- **Ghi chú**: Kiểm tra toàn bộ điều kiện lọc của danh sách nhóm.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetRootGroup`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`; ảnh được chọn `GF1(root=CA, baseId=CA)` hợp lệ.
  - `GF2(root=CA, baseId=GF1)` hợp lệ và thuộc customer-001.
  - Có các record không được trả: Card History cùng customer; Flower History `root=CB`; Flower History đã xóa; Flower có `is_deleted=true`; Flower thuộc customer-002.
- **Input**:

```http
GET /api/v1/flower-histories/GF1/root-group
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
rootComboId = CA.
items chỉ có GF1 và GF2.
Không trả Card History, item root=CB, History/Flower đã xóa hoặc Flower của customer-002.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận nhóm chỉ chứa ảnh hoa hợp lệ của đúng customer và đúng Combo nguồn.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-008, BR-100
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-035
- **Section**: Business Rules - type, root, ownership và isDeleted
- **Ghi chú**:

---

## UT-035-04: URL ảnh rỗng vẫn giữ History trong nhóm

## Thông tin tài liệu

- **Tiêu đề**: Một ảnh mất URL không làm mất các item History khác.
- **Ghi chú**: API chỉ trả metadata; FE quyết định placeholder ảnh.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetRootGroup`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - `GF1(root=CA, baseId=CA)` có `imageUrl` hợp lệ.
  - `GF2(root=CA, baseId=GF1)` hợp lệ nhưng `GeneratedFlower.ImageUrl = null`.
  - Cả hai chưa bị xóa và cùng thuộc customer-001.
- **Input**:

```http
GET /api/v1/flower-histories/GF1/root-group
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
items vẫn gồm GF1 và GF2.
GF2 có imageUrl = null; request không chuyển thành lỗi và GF1 không bị mất khỏi nhóm.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận History được bảo toàn khi file ảnh không còn khả dụng.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-007, BR-099
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-035
- **Section**: Business Rules - URL ảnh rỗng
- **Ghi chú**:

---

## UT-035-05: History nguồn không có root hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: Không tạo được nhóm khi ảnh nguồn có root rỗng.
- **Ghi chú**: `root` là điều kiện bắt buộc để xác định Combo nguồn.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetRootGroup`
- **Loại**: Error
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - Có Flower History H01 của customer-001 với `outputId=GF1`, `type=flower`, chưa xóa, nhưng `root = Guid.Empty`.
- **Input**:

```http
GET /api/v1/flower-histories/GF1/root-group
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 404, messageCode = NOT_FOUND.
Không trả rootComboId hay items vì không xác định được Combo nguồn.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API không tự đoán Combo nguồn khi dữ liệu History thiếu root.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-035
- **Section**: Business Rules - root hợp lệ; Mã lỗi - NOT_FOUND
- **Ghi chú**:

---

## UT-035-06: Customer chưa đăng nhập

## Thông tin tài liệu

- **Tiêu đề**: Endpoint nhóm ảnh yêu cầu xác thực.
- **Ghi chú**: Kiểm tra tại Controller/middleware trước khi Service đọc History.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistoryController.GetRootGroup`
- **Loại**: Error
- **Precondition / Mock setup**: Không có Bearer token hoặc token không hợp lệ; DB có dữ liệu Flower History hợp lệ để tránh false positive.
- **Input**:

```http
GET /api/v1/flower-histories/GF1/root-group
```

- **Expected output (bắt buộc)**:

```text
HTTP 401.
Không gọi FlowerHistory.Service và không trả rootComboId, items hay metadata ảnh.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận endpoint không làm lộ nhóm History khi customer chưa xác thực.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-008, BR-100
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-035
- **Section**: Business Rules - customer phải đăng nhập; Mã lỗi - UNAUTHORIZED
- **Ghi chú**:

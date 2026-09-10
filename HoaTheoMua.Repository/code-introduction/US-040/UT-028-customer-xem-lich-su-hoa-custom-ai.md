# UT-028: Unit Tests - Customer lấy toàn bộ lịch sử mẫu hoa Custom AI

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API lấy toàn bộ lịch sử hoa AI có phân trang.
- **Ghi chú**: UT-028 chỉ kiểm tra Endpoint #1 của TDD-028: `GET /api/v1/flower-histories?pageIndex&pageSize`.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-028
- **Phiên bản**: v0.1
- **Author (bắt buộc)**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-028-01: Lấy danh sách lịch sử của customer

## Thông tin tài liệu

- **Tiêu đề**: Customer nhận các kết quả hoa AI của chính mình theo trang.
- **Ghi chú**: Luồng chính của API danh sách.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetAll`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - `client_histories`: H01 có `root = combo-001`, `base_id = combo-001`, `output_id = F01`; H02 là lần regenerate có `root = combo-001`, `base_id = F01`, `output_id = F02`.
  - `generated_flowers`: F01 và F02 cùng user, chưa xóa, có `image_url`.
  - Có H03 của customer-002 để kiểm tra ownership.
- **Input**:

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200, response.isSuccess = true.
response.value.items chỉ có F01 và F02 của customer-001.
Mỗi item có generatedFlowerId, comboId, imageUrl, generatedAt. Cả F01 và F02 đều trả comboId = combo-001; F02 không được trả comboId = F01.
value.pageIndex = 1, value.pageSize = 20, value.totalCount = 2.
value.hasNextPage = false và value.hasPreviousPage = false.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API danh sách trả đúng các kết quả hoa AI thuộc customer trong wrapper phân trang.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-003, AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-028
- **Section**: Endpoint #1 - Response
- **Ghi chú**:

---

## UT-028-02: Phân trang và thứ tự mới nhất trước

## Thông tin tài liệu

- **Tiêu đề**: API áp dụng đúng `pageIndex`, `pageSize` và thứ tự ổn định.
- **Ghi chú**: Kiểm tra dữ liệu lớn hơn một trang.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetAll`
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Customer có 5 kết quả hợp lệ F01...F05.
  - F04 và F05 có cùng `created_at`; F05 có `id` lớn hơn F04.
- **Input**:

```http
GET /api/v1/flower-histories?pageIndex=2&pageSize=2
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
value.items có đúng phần tử thứ 3 và thứ 4 theo createdAt giảm dần,
sau đó generatedFlowerId giảm dần khi thời điểm bằng nhau.
value.pageIndex = 2, value.pageSize = 2, value.totalCount = 5.
value.hasPreviousPage = true, value.hasNextPage = true.
Không lặp hoặc bỏ sai phần tử giữa các trang.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận FE nhận danh sách ổn định khi có nhiều kết quả và có thể phân trang.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-005, AC-006
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-028
- **Section**: Business Rules - thứ tự và phân trang
- **Ghi chú**:

---

## UT-028-03: Lọc đúng loại History và ownership

## Thông tin tài liệu

- **Tiêu đề**: Không trả Card History, bản ghi bị xóa hoặc dữ liệu của customer khác.
- **Ghi chú**: Kiểm tra các điều kiện lọc trong query.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetAll`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - H01 hợp lệ: `type = flower`, cùng user, chưa xóa, output F01 hợp lệ.
  - H02 cùng user nhưng `type = card`.
  - H03 cùng user nhưng `is_deleted = true`.
  - H04 là `type = flower` nhưng thuộc customer-002.
  - H05 là flower của customer-001 nhưng Generated Flower đã bị xóa.
- **Input**:

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
value.items chỉ chứa H01/F01.
Không trả H02, H03, H04 hoặc H05.
totalCount = 1.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận danh sách không lộ dữ liệu khác loại, đã xóa hoặc không thuộc customer.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-008, BR-100
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-028
- **Section**: Business Rules - type, ownership và isDeleted
- **Ghi chú**:

---

## UT-028-04: Danh sách rỗng

## Thông tin tài liệu

- **Tiêu đề**: Customer chưa có kết quả hoa AI nhận empty state hợp lệ.
- **Ghi chú**: Không xem danh sách rỗng là lỗi hệ thống.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetAll`
- **Loại**: Boundary
- **Precondition / Mock setup**: JWT hợp lệ nhưng customer không có History flower nào liên kết Generated Flower hợp lệ.
- **Input**:

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
response.isSuccess = true.
response.value.items = [].
response.value.totalCount = 0.
Không trả 404 và không lấy dữ liệu của customer khác.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận FE có thể hiển thị empty state khi customer chưa tạo hoa AI.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-010
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-028
- **Section**: Endpoint #1 - Không có dữ liệu
- **Ghi chú**:

---

## UT-028-05: Ảnh không còn khả dụng nhưng vẫn giữ record

## Thông tin tài liệu

- **Tiêu đề**: URL ảnh rỗng không làm mất lịch sử hoặc lỗi toàn bộ danh sách.
- **Ghi chú**: FE hiển thị placeholder cho item đó.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetAll`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - H01 có `image_url` hợp lệ.
  - H02 cùng customer, cùng `type = flower`, chưa xóa, nhưng `generated_flowers.image_url` null/rỗng.
- **Input**:

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200.
H01 và H02 đều vẫn xuất hiện trong value.items.
H02 có imageUrl null/rỗng; H01 vẫn có URL bình thường.
Lỗi một URL không làm mất item khác hoặc làm request thất bại.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận bản ghi lịch sử được bảo toàn dù file ảnh không còn khả dụng.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-007, BR-099
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-028
- **Section**: Business Rules - bảo toàn record
- **Ghi chú**:

---

## UT-028-06: Tham số phân trang không hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: Chặn `pageIndex` và `pageSize` ngoài giới hạn.
- **Ghi chú**: Validation xảy ra trước khi truy vấn dữ liệu.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistory.Service.GetAll`
- **Loại**: Error
- **Precondition / Mock setup**: Không cần dữ liệu lịch sử.
- **Input**:

```text
Case A: GET /api/v1/flower-histories?pageIndex=0&pageSize=20
Case B: GET /api/v1/flower-histories?pageIndex=1&pageSize=0
Case C: GET /api/v1/flower-histories?pageIndex=1&pageSize=101
```

- **Expected output (bắt buộc)**:

```text
Cả ba case trả HTTP 400 với mã INVALID_PAGE.
Không truy vấn client_histories hoặc generated_flowers sau khi validation thất bại.
Không trả danh sách một trang mặc định cho input sai.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận giới hạn phân trang được kiểm soát trước khi truy vấn DB.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-040
- **Section**: AC-011
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-028
- **Section**: Mã lỗi - INVALID_PAGE
- **Ghi chú**:

---

## UT-028-07: Khách hàng chưa đăng nhập

## Thông tin tài liệu

- **Tiêu đề**: API không lấy danh sách khi thiếu user hiện tại.
- **Ghi chú**: Kiểm tra authentication trước Service.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Lịch sử hoa Custom AI
- **Unit under test (bắt buộc)**: `FlowerHistoryController.GetAll`
- **Loại**: Error
- **Precondition / Mock setup**: Không có Bearer token hoặc token không hợp lệ; DB có dữ liệu để tránh false positive.
- **Input**:

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
```

- **Expected output (bắt buộc)**:

```text
HTTP 401.
Không gọi FlowerHistory.Service.
Không trả items, comboId hoặc imageUrl.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API danh sách yêu cầu đăng nhập và không làm lộ dữ liệu lịch sử.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-028
- **Section**: Business Rules - ownership
- **Ghi chú**:


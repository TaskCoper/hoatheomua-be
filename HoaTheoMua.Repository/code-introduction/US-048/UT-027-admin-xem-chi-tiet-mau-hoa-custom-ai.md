# UT-027: Unit Tests - Admin xem chi tiết mẫu hoa Custom AI

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API Admin xem chi tiết mẫu hoa Custom AI.
- **Ghi chú**: Các kịch bản bám theo TDD-027, AC/BR và system test của STORY-048.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-027
- **Phiên bản**: v0.1
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-027-01: Chi tiết đầy đủ từ snapshot

## Thông tin tài liệu

- **Tiêu đề**: Admin xem đủ ảnh, Combo, User Input, Mockup, Core, Support và Order.
- **Ghi chú**: Kiểm tra luồng chính với snapshot hợp lệ.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetById`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Có History `type = flower`, chưa xóa, `output_id = flower-001`, `base_id = combo-001`.
  - Có Generated Flower `flower-001`, cùng `user_id`, chưa xóa, có `image_url` và `input_snapshot` hợp lệ.
  - Snapshot có product, user_input, mockup, core_materials và support_materials.
  - Có một OrderDetail chứa `flower-001` và Order chưa xóa.
- **Input**:

```http
GET /api/v1/admin/generated-flowers/fa000000-0000-0000-0000-000000000001
```

- **Expected output**:

```text
HTTP 200.
response.isSuccess = true.
Chi tiết nằm ở response.value.
fileStatus = available.
value.comboId = History.base_id.
value.comboName, customerName, userInput, mockup, coreMaterials, supportMaterials lấy từ snapshot.
value.orders có Order liên kết.
Không có field giá hoặc bill.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận API chi tiết trả đủ dữ liệu mà Admin cần xem.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-048
- **Section**: AC-003, AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-027
- **Section**: BƯỚC 3 - Ví dụ 1
- **Ghi chú**:

---

## UT-027-02: Có nhiều Order liên kết

## Thông tin tài liệu

- **Tiêu đề**: Chi tiết trả đủ Order liên kết và không bị lặp.
- **Ghi chú**: Giá/bill không nằm trong response mẫu hoa.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Một Flower hợp lệ.
  - Hai Order khác nhau cùng tham chiếu Flower qua OrderDetails.GeneratedFlowerIds.
  - Có thêm một OrderDetail trùng tham chiếu để kiểm tra `Distinct`.
- **Input**:

```http
GET /api/v1/admin/generated-flowers/fa000000-0000-0000-0000-000000000002
```

- **Expected output**:

```text
HTTP 200.
value.orders có đúng 2 Order khác nhau.
Mỗi Order có orderId, nhanhOrderId, status, createdAt.
Order mới hơn đứng trước.
Không có totalFee, price hoặc bill snapshot.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận Order liên kết đầy đủ và không duplicate.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-048
- **Section**: AC-001, AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-027
- **Section**: Mô hình dữ liệu - Orders
- **Ghi chú**:

---

## UT-027-03: Ưu tiên input_snapshot và fallback metadata

## Thông tin tài liệu

- **Tiêu đề**: Service đọc snapshot đúng thứ tự.
- **Ghi chú**: `generated_flowers.input_snapshot` được ưu tiên; metadata dùng khi input snapshot trống hoặc JSON không đọc được.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Case A: InputSnapshot và Metadata đều hợp lệ nhưng tên Combo khác nhau.
  - Case B: InputSnapshot null/JSON lỗi, Metadata hợp lệ.
- **Input**:

```text
Gọi GetById với cùng generatedFlowerId cho từng case.
```

- **Expected output**:

```text
Case A: comboName, userInput, mockup, materials lấy từ input_snapshot.
Case B: các field snapshot lấy từ metadata.
API không ném lỗi chỉ vì InputSnapshot bị thiếu hoặc JSON lỗi.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Khóa thứ tự đọc snapshot để dữ liệu lịch sử không bị mất khi một nguồn snapshot lỗi.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-048
- **Section**: AC-003, BR-147, BR-148
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-027
- **Section**: Business Rules - Snapshot
- **Ghi chú**:

---

## UT-027-04: Không đọc trạng thái Combo/Mockup live

## Thông tin tài liệu

- **Tiêu đề**: Snapshot vẫn hiển thị khi nguồn hiện tại inactive hoặc bị xóa logic.
- **Ghi chú**: Chi tiết lịch sử không bị thay đổi theo trạng thái hiện tại.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetById`
- **Loại**: Quirk
- **Precondition / Mock setup**:
  - History/Generated Flower hợp lệ và có snapshot Combo/Mockup.
  - Product hoặc Mockup hiện tại bị inactive/soft-delete.
- **Input**:

```http
GET /api/v1/admin/generated-flowers/fa000000-0000-0000-0000-000000000001
```

- **Expected output**:

```text
HTTP 200.
API vẫn trả comboName, comboImageUrl, mockup và các material từ snapshot.
Không trả lỗi chỉ vì nguồn hiện tại inactive/đã xóa.
Không đánh giá tồn kho hoặc trạng thái live trong API này.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận dữ liệu lịch sử được giữ nguyên sau thay đổi nguồn.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-048
- **Section**: AC-003, BR-148, BR-149
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-027
- **Section**: Ngoài phạm vi - Không kiểm tra live
- **Ghi chú**:

---

## UT-027-05: Ảnh không khả dụng nhưng metadata vẫn trả

## Thông tin tài liệu

- **Tiêu đề**: Không có imageUrl vẫn hiển thị được chi tiết snapshot.
- **Ghi chú**: FE chỉ báo lỗi tại vùng ảnh.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - History/Generated Flower hợp lệ, snapshot đầy đủ.
  - `generated_flowers.image_url = null` hoặc rỗng.
- **Input**:

```http
GET /api/v1/admin/generated-flowers/fa000000-0000-0000-0000-000000000003
```

- **Expected output**:

```text
HTTP 200.
value.imageUrl = null hoặc rỗng.
value.fileStatus = unavailable.
Combo, User Input, Mockup, Core, Support và Orders vẫn được trả theo dữ liệu có sẵn.
Không gọi AI và không xóa History.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận lỗi ảnh không làm mất chi tiết mẫu hoa.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-048
- **Section**: AC-004, BR-151
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-027
- **Section**: BƯỚC 3 - Ví dụ 3
- **Ghi chú**:

---

## UT-027-06: generatedFlowerId không hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: API chặn Guid.Empty trước khi truy vấn.
- **Ghi chú**: Không được query hoặc trả dữ liệu của mẫu hoa khác.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetById`
- **Loại**: Error
- **Precondition / Mock setup**: Không cần dữ liệu đặc biệt.
- **Input**:

```http
GET /api/v1/admin/generated-flowers/00000000-0000-0000-0000-000000000000
```

- **Expected output**:

```text
HTTP 400.
Mã lỗi INVALID_GENERATED_FLOWER_ID.
Không query History/Generated Flower.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận kiểm tra route ID trước khi đọc dữ liệu.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-048
- **Section**: EXC-03
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-027
- **Section**: Mã lỗi - INVALID_GENERATED_FLOWER_ID
- **Ghi chú**:

---

## UT-027-07: Không tìm thấy mẫu hoa hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: API trả 404 khi mẫu hoa không tồn tại hoặc không truy cập được.
- **Ghi chú**: Bao gồm sai type, bị xóa logic, không có cặp output hoặc sai ownership.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlower.Service.GetById`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Chạy riêng các case: không có Flower; History không phải type flower; History/Flower bị xóa; hoặc user_id không khớp.
- **Input**:

```http
GET /api/v1/admin/generated-flowers/fa000000-0000-0000-0000-ffffffffffff
```

- **Expected output**:

```text
HTTP 404.
Mã lỗi GENERATED_FLOWER_NOT_FOUND.
Không trả ảnh, snapshot, Combo, materials hoặc Orders.
Không chuyển sang mẫu hoa khác.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận tài nguyên không hợp lệ không làm lộ dữ liệu khác.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-048
- **Section**: EXC-03
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-027
- **Section**: Mã lỗi - GENERATED_FLOWER_NOT_FOUND
- **Ghi chú**:

---

## UT-027-08: Chặn tài khoản không có quyền

## Thông tin tài liệu

- **Tiêu đề**: Người dùng không có quyền không nhận được chi tiết mẫu hoa.
- **Ghi chú**: Kiểm tra ở Controller/policy.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết mẫu hoa Custom AI
- **Unit under test**: `AdminGeneratedFlowerController.GetById`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Case 1: không có token.
  - Case 2: có token nhưng không có quyền AI Custom.
  - Có mẫu hoa hợp lệ để chắc chắn request bị chặn bởi quyền.
- **Input**:

```http
GET /api/v1/admin/generated-flowers/fa000000-0000-0000-0000-000000000001
```

- **Expected output**:

```text
Chưa đăng nhập: HTTP 401.
Không có quyền: HTTP 403.
Service không được gọi.
Không trả ảnh, khách hàng, Combo, materials, snapshot hoặc Order.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận API chi tiết kiểm tra quyền ở Backend.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-048
- **Section**: AC-006, BR-132
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-027
- **Section**: Mã lỗi - UNAUTHORIZED, FORBIDDEN
- **Ghi chú**:


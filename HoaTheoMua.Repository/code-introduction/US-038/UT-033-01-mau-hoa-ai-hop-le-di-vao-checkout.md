# UT-033-01: Mẫu hoa AI hợp lệ đi vào Checkout cũ

## Thông tin tài liệu

- **Tiêu đề**: Mẫu hoa AI hợp lệ đi vào Checkout cũ
- **Ghi chú**: Xác nhận API tìm đúng Combo nguồn từ Flower History và cho customer đi tiếp khi Core còn hàng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-033-01
- **Phiên bản**: v0.1
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-10

## Đơn vị kiểm thử

- **Module**: GENERATED FLOWER CHECKOUT - Kiểm tra mẫu hoa AI trước Checkout
- **Unit under test**: `GeneratedFlowerCheckout.Service.GetCheckoutAvailability`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Customer đang đăng nhập là `customer-001`.
  - `generatedFlowerId = flower-001` thuộc `customer-001`.
  - Flower History của `customer-001` có `Type = flower`, `OutputId = flower-001`, `Root = combo-001`.
  - `combo-001` là Product active, chưa xóa, `ProductType = Combo`.
  - Core `material-core-001` có `Quantity = 3`, `AvailableForSale = 9`, specification và material đều active/chưa xóa.
  - Support `material-support-001` có `Quantity = 1`, `AvailableForSale = 0`.
- **Input**:
  ```http
  GET /api/v1/generated-flowers/flower-001/checkout-availability
  Authorization: Bearer <customer-001-token>
  ```
- **Expected output**:
  ```text
  HTTP 200.
  generatedFlowerId = flower-001.
  rootComboId = combo-001 vì Root của Flower History là Combo nguồn.
  sellableQuantityCombo = 3 vì Core có 9 / 3 = 3.
  Support material-support-001 có NeedsReplacement = true nhưng API vẫn trả thành công,
  để FE đi vào flow Checkout cũ với combo-001.
  Không tạo Checkout mới, Order hoặc thay đổi History.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận luồng chính: ảnh hoa AI hợp lệ được quy về đúng Combo nguồn và vẫn đặt được khi chỉ Support thiếu.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: Main Flow bước 1-6; AC-002
- **Ghi chú**: Mẫu hoa được dùng để đi vào Checkout với đúng một nguồn.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: ALT-02; AC-006
- **Ghi chú**: Support hết không chặn Checkout.

**Link 3**
- **Loại**: System Test
- **Mã**: ST-038-01-02
- **Section**: Smoke: Mở Checkout từ mẫu hoa hợp lệ
- **Ghi chú**:

**Link 4**
- **Loại**: TDD
- **Mã**: TDD-033
- **Section**: Business Rules - Combo nguồn và Support
- **Ghi chú**:

---

# UT-033-02: Ảnh hoa AI không thuộc customer hiện tại

## Thông tin tài liệu

- **Tiêu đề**: Ảnh hoa AI không thuộc customer hiện tại
- **Ghi chú**: Customer không được dùng ảnh hoa AI của người khác để đi vào Checkout.

## Đơn vị kiểm thử

- **Module**: GENERATED FLOWER CHECKOUT - Kiểm tra mẫu hoa AI trước Checkout
- **Unit under test**: `GeneratedFlowerCheckout.Service.GetCheckoutAvailability`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Customer đang đăng nhập là `customer-001`.
  - `flower-001` tồn tại nhưng có `UserId = customer-002`.
- **Input**:
  ```http
  GET /api/v1/generated-flowers/flower-001/checkout-availability
  Authorization: Bearer <customer-001-token>
  ```
- **Expected output**:
  ```text
  HTTP 404 với messageCode = NOT_FOUND.
  Không trả ảnh hoa, rootComboId hoặc materials.
  Customer không được đi tiếp Checkout và không có dữ liệu nào bị thay đổi.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Bảo vệ ownership của ảnh hoa AI.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: EXC-03
- **Ghi chú**: Không có quyền truy cập mẫu hoa AI của customer khác.

**Link 2**
- **Loại**: System Test
- **Mã**: ST-038-08-01
- **Section**: Bảo vệ ownership
- **Ghi chú**:

**Link 3**
- **Loại**: TDD
- **Mã**: TDD-033
- **Section**: Business Rules - Ownership
- **Ghi chú**:

---

# UT-033-03: Ảnh hoa AI thiếu Flower History hoặc root

## Thông tin tài liệu

- **Tiêu đề**: Ảnh hoa AI thiếu Flower History hoặc root
- **Ghi chú**: Ảnh hoa không xác định được Combo nguồn thì không thể dùng để đặt hàng.

## Đơn vị kiểm thử

- **Module**: GENERATED FLOWER CHECKOUT - Kiểm tra mẫu hoa AI trước Checkout
- **Unit under test**: `GeneratedFlowerCheckout.Service.GetCheckoutAvailability`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Customer `customer-001` sở hữu `flower-001`.
  - Không có Flower History `Type = flower`, `OutputId = flower-001`; hoặc History có `Root = Guid.Empty`.
- **Input**:
  ```http
  GET /api/v1/generated-flowers/flower-001/checkout-availability
  Authorization: Bearer <customer-001-token>
  ```
- **Expected output**:
  ```text
  HTTP 400 với messageCode = BAD_REQUEST.
  Detail: Không xác định được Combo nguồn của mẫu hoa AI.
  Không trả rootComboId và không đi vào Checkout.
  ```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Không được suy đoán Combo nguồn từ ảnh hay từ dữ liệu FE gửi lên.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: Main Flow bước 2-3
- **Ghi chú**: Backend phải xác định được nguồn của mẫu hoa trước Checkout.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-033
- **Section**: Business Rules - Flower History và root
- **Ghi chú**:

---

# UT-033-04: Combo nguồn không còn khả dụng

## Thông tin tài liệu

- **Tiêu đề**: Combo nguồn không còn khả dụng
- **Ghi chú**: Lịch sử ảnh vẫn giữ nguyên, nhưng không được dùng để mở Checkout khi Combo nguồn đã bị xóa hoặc inactive.

## Đơn vị kiểm thử

- **Module**: GENERATED FLOWER CHECKOUT - Kiểm tra mẫu hoa AI trước Checkout
- **Unit under test**: `GeneratedFlowerCheckout.Service.GetCheckoutAvailability`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Customer `customer-001` sở hữu `flower-001` và Flower History có `Root = combo-001`.
  - `combo-001` không tồn tại, đã xóa, inactive hoặc không có `ProductType = Combo`.
- **Input**:
  ```http
  GET /api/v1/generated-flowers/flower-001/checkout-availability
  Authorization: Bearer <customer-001-token>
  ```
- **Expected output**:
  ```text
  HTTP 400 với messageCode = BAD_REQUEST.
  Detail: Combo nguồn không tồn tại hoặc không còn khả dụng.
  Không trả rootComboId cho FE đi vào Checkout.
  Flower History và các Order cũ không bị thay đổi.
  ```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Ảnh hoa AI không bảo đảm Combo nguồn còn bán được ở thời điểm customer đặt hàng.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: EXC-02; AC-008
- **Ghi chú**: Combo nguồn không còn khả dụng.

**Link 2**
- **Loại**: System Test
- **Mã**: ST-038-07-01
- **Section**: Combo nguồn không còn khả dụng: không mở được Checkout
- **Ghi chú**:

**Link 3**
- **Loại**: TDD
- **Mã**: TDD-033
- **Section**: Ví dụ 3 — Combo nguồn không còn bán được
- **Ghi chú**:

---

# UT-033-05: Core không đủ hàng tại thời điểm mở Checkout

## Thông tin tài liệu

- **Tiêu đề**: Core không đủ hàng tại thời điểm mở Checkout
- **Ghi chú**: Combo nguồn vẫn tồn tại nhưng không đủ vật liệu chính để đặt.

## Đơn vị kiểm thử

- **Module**: GENERATED FLOWER CHECKOUT - Kiểm tra mẫu hoa AI trước Checkout
- **Unit under test**: `GeneratedFlowerCheckout.Service.GetCheckoutAvailability`
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Customer `customer-001` sở hữu `flower-001`; Flower History có `Root = combo-001`.
  - `combo-001` hợp lệ, nhưng Core có `Quantity = 3` và `AvailableForSale = 2`.
  - `GetLimitCoreProduct(combo-001)` trả `SellableQuantityCombo = 0`.
- **Input**:
  ```http
  GET /api/v1/generated-flowers/flower-001/checkout-availability
  Authorization: Bearer <customer-001-token>
  ```
- **Expected output**:
  ```text
  HTTP 400 với messageCode = BAD_REQUEST.
  Detail: Combo hiện không còn đủ thành phần chính để đặt hàng. Vui lòng chọn mẫu hoa khác.
  Không mở Checkout và không tạo Order.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Core không đủ phải chặn ngay khi customer bấm Đặt hàng ngay.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: EXC-01; AC-005; BR-084
- **Ghi chú**: Core hết hàng tại lúc mở Checkout.

**Link 2**
- **Loại**: System Test
- **Mã**: ST-038-04-01
- **Section**: Tình huống A — Core hết khi mở Checkout
- **Ghi chú**:

**Link 3**
- **Loại**: TDD
- **Mã**: TDD-033
- **Section**: Ví dụ 3 — Core hết hàng
- **Ghi chú**:

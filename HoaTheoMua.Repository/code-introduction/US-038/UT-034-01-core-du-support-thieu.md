# UT-034-01: Core đủ nhưng Support thiếu

## Thông tin tài liệu

- **Tiêu đề**: Core đủ nhưng Support thiếu
- **Ghi chú**: Xác nhận hàm dùng chung vẫn cho Combo đi tiếp khi chỉ vật liệu Support cần thay thế.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-034-01
- **Phiên bản**: v0.1
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-10

## Đơn vị kiểm thử

- **Module**: ORDER - Kiểm tra khả năng bán của Combo
- **Unit under test**: `Order.Service.GetLimitCoreProduct`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - `combo-001` là Product active, chưa xóa, `ProductType = Combo`.
  - Core `material-core-001`: specification active, material active/chưa xóa, `Quantity = 3`, `AvailableForSale = 10`.
  - Core `material-core-002`: specification active, material active/chưa xóa, `Quantity = 2`, `AvailableForSale = 9`.
  - Support `material-support-001`: specification active, material active/chưa xóa, `Quantity = 1`, `AvailableForSale = 0`.
- **Input**:
  ```text
  GetLimitCoreProduct(combo-001)
  ```
- **Expected output**:
  ```text
  Hàm trả SellableQuantityCombo = 3 vì Core thứ nhất bán được floor(10 / 3) = 3,
  Core thứ hai bán được floor(9 / 2) = 4, nên lấy giá trị nhỏ nhất là 3.

  material-support-001 vẫn xuất hiện trong Materials và NeedsReplacement = true vì tồn kho 0 < Quantity 1.
  Hàm không throw lỗi và không reserve/trừ tồn kho, vì Support thiếu không được phép chặn Checkout.
  ```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận đúng quy tắc quan trọng nhất của US-038: Core quyết định có thể đặt, còn Support chỉ cần được cảnh báo để thay thế sau.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: ALT-02; AC-006; BR-084
- **Ghi chú**: Support thiếu vẫn được tiếp tục Checkout.

**Link 2**
- **Loại**: System Test
- **Mã**: ST-038-04-01
- **Section**: Tình huống B — Support hết, Core còn
- **Ghi chú**:

**Link 3**
- **Loại**: TDD
- **Mã**: TDD-034
- **Section**: Business Rules - Core và Support
- **Ghi chú**:

---

# UT-034-02: Core không đủ hàng

## Thông tin tài liệu

- **Tiêu đề**: Core không đủ hàng
- **Ghi chú**: Core quyết định Combo có thể đi vào Checkout hay không.

## Đơn vị kiểm thử

- **Module**: ORDER - Kiểm tra khả năng bán của Combo
- **Unit under test**: `Order.Service.GetLimitCoreProduct`
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - `combo-001` là Product active, chưa xóa, `ProductType = Combo`.
  - Core `material-core-001`: specification và material đều hợp lệ, `Quantity = 3`, `AvailableForSale = 2`.
- **Input**:
  ```text
  GetLimitCoreProduct(combo-001)
  ```
- **Expected output**:
  ```text
  Hàm trả SellableQuantityCombo = 0 vì floor(2 / 3) = 0.
  Caller không được mở Checkout hoặc tạo Order với Combo này.
  Hàm không reserve/trừ tồn kho.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận Core hết hàng chặn việc đặt Combo.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: EXC-01; AC-005; BR-084
- **Ghi chú**: Core không đủ tại lúc mở Checkout.

**Link 2**
- **Loại**: System Test
- **Mã**: ST-038-04-01
- **Section**: Tình huống A — Core hết khi mở Checkout
- **Ghi chú**:

**Link 3**
- **Loại**: TDD
- **Mã**: TDD-034
- **Section**: Ví dụ 2 — Core không còn đủ tồn kho
- **Ghi chú**:

---

# UT-034-03: Combo nguồn không còn khả dụng

## Thông tin tài liệu

- **Tiêu đề**: Combo nguồn không còn khả dụng
- **Ghi chú**: Không cho dùng một Product bị xóa, inactive hoặc không phải Combo để đi vào Checkout.

## Đơn vị kiểm thử

- **Module**: ORDER - Kiểm tra khả năng bán của Combo
- **Unit under test**: `Order.Service.GetLimitCoreProduct`
- **Loại**: Error
- **Precondition / Mock setup**:
  - `combo-001` có thể không tồn tại, đã bị xóa, inactive hoặc có `ProductType` khác `Combo`.
- **Input**:
  ```text
  GetLimitCoreProduct(combo-001)
  ```
- **Expected output**:
  ```text
  Hàm throw BadRequestException: Combo nguồn không tồn tại hoặc không còn khả dụng.
  Không trả số lượng bán được, không mở Checkout và không tạo Order.
  ```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận ảnh hoa AI không thể được dùng để đặt hàng nếu Combo nguồn hiện tại không còn bán được.

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
- **Mã**: TDD-034
- **Section**: Business Rules - Combo phải còn khả dụng
- **Ghi chú**:

---

# UT-034-04: Core có cấu hình không hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: Core có cấu hình không hợp lệ
- **Ghi chú**: Không được bỏ qua Core bị inactive, material đã xóa hoặc số lượng bằng 0.

## Đơn vị kiểm thử

- **Module**: ORDER - Kiểm tra khả năng bán của Combo
- **Unit under test**: `Order.Service.GetLimitCoreProduct`
- **Loại**: Error
- **Precondition / Mock setup**:
  - `combo-001` là Combo active và chưa xóa.
  - Có một Core, nhưng Core đó có một trong các trạng thái: specification inactive, material inactive, material đã xóa hoặc `Quantity = 0`.
- **Input**:
  ```text
  GetLimitCoreProduct(combo-001)
  ```
- **Expected output**:
  ```text
  Hàm throw BadRequestException vì Combo không có vật liệu Core hợp lệ để đặt hàng.
  Không trả SellableQuantityCombo và không cho flow tiếp tục.
  ```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận dữ liệu Core sai phải chặn thay vì bị lọc mất hoặc bị coi là Support.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-038
- **Section**: AC-005; BR-084
- **Ghi chú**: Combo cần có Core hợp lệ mới được đặt.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-034
- **Section**: Business Rules - Core hợp lệ
- **Ghi chú**:

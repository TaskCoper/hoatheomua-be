# UT-005: CalculateAvailableQty - Tính số lượng khả dụng Combo

> **Lưu ý:** Đây là hàm bổ trợ (helper), có thể tái sử dụng bởi nhiều API, không chỉ riêng GetLocalProductsV2.

## Thông tin chung (Common Metadata)
- **Phiên bản:** v0.1
- **Cập nhật gần nhất:** 11/08/2026
- **Author / Owner:** Nguyễn Tùng Dương
- **Reviewer / Approver:** Chưa chỉ định
- **Suite:** REGRESSION

---

## UT-005-01
- **Tiêu đề:** Combo không biến thể - tính đúng
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Happy | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-001 có công thức:
  - mat-001: qty=2, isCore=true, stock=100 → Max combo = 50
  - mat-002: qty=3, isCore=true, stock=99  → Max combo = 33
  ```
- **Input:** `productIds = ["combo-001"]`
- **Expected output:** `SellableQuantity = 33` (quyết định bởi mat-002 là Core có đáp ứng thấp nhất).
- **Rationale:** Xác nhận tính đúng theo min của tất cả thành phần Core.

---

## UT-005-02
- **Tiêu đề:** Cha combo - lấy min biến thể
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Happy | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-parent-001 (Sản phẩm) có 2 biến thể Combo:
  - variant-001: SellableQuantity = 50 (được tính từ công thức của variant-001)
  - variant-002: SellableQuantity = 20 (được tính từ công thức của variant-002)
  ```
- **Input:** `productIds = ["combo-parent-001"]`
- **Expected output:** `SellableQuantity = 20` (min của 2 biến thể con).
- **Rationale:** Xác nhận cha hiển thị số lượng khả dụng bằng min của các biến thể con.

---

## UT-005-03
- **Tiêu đề:** Sản phẩm thường - lấy trực tiếp từ Stock
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Happy | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  product-001 (Sản phẩm), không phải Combo, không có biến thể Combo, Stock = 15
  ```
- **Input:** `productIds = ["product-001"]`
- **Expected output:** `SellableQuantity = 15` (lấy trực tiếp từ trường Stock trong database).
- **Rationale:** Xác nhận sản phẩm thường được lấy số lượng trực tiếp từ tồn kho (Stock), không cần qua công thức tính toán.

---

## UT-005-04
- **Tiêu đề:** Không có Core → qty=0
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Boundary | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-001 có công thức nhưng toàn bộ là thành phần phụ (isCore=false)
  ```
- **Input:** `productIds = ["combo-001"]`
- **Expected output:** `SellableQuantity = 0`
- **Rationale:** Xác nhận Combo không có thành phần Core nào thì không thể tính sản xuất.

---

## UT-005-05
- **Tiêu đề:** Hết hàng → qty=0
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Happy | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-001 có 2 Core:
  - mat-001: stock = 0
  - mat-002: stock = 50
  ```
- **Input:** `productIds = ["combo-001"]`
- **Expected output:** `SellableQuantity = 0`
- **Rationale:** Xác nhận khi một thành phần Core hết hàng, Combo cũng hết hàng theo.

---

## UT-005-06
- **Tiêu đề:** RequiredQty=0 - tránh divide by zero
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Error | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-001 có Core mat-001 với quantity=0 (yêu cầu = 0), stock=100
  ```
- **Input:** `productIds = ["combo-001"]`
- **Expected output:** Không crash DivideByZeroException. Bỏ qua mat-001 và tính theo các Core khác. Nếu chỉ có mat-001 → Qty = 0.
- **Rationale:** Xác nhận xử lý an toàn lỗi chia cho 0.

---

## UT-005-07
- **Tiêu đề:** Lọc IsDeleted=false
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Boundary | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-001 có công thức:
  - mat-001 (Core, quantity=1, stock=100, IsDeleted=true)
  - mat-002 (Core, quantity=1, stock=50, IsDeleted=false)
  ```
- **Input:** `productIds = ["combo-001"]`
- **Expected output:** `SellableQuantity = 50` (mat-001 đã bị soft-delete nên bị loại trừ khỏi phép tính).
- **Rationale:** Xác nhận hệ thống bỏ qua các thành phần công thức đã bị soft-delete.

---

## UT-005-08
- **Tiêu đề:** Input rỗng - early return
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Quirk | **Priority:** P2
- **Precondition / Mock setup:** Không có dữ liệu setup đặc biệt
- **Input:** `productIds = []` (danh sách rỗng)
- **Expected output:** Trả về kết quả rỗng, không lỗi.
- **Rationale:** Xác nhận xử lý early return khi input trống.

---

## UT-005-09
- **Tiêu đề:** StockStatus logic (>0/"Còn hàng", =0/"Hết hàng")
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Determinism | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  Case 1: Core stock=1, reqQty=1 → Qty = 1
  Case 2: Core stock=0, reqQty=1 → Qty = 0
  ```
- **Input:** Chạy với 2 Case trên.
- **Expected output:**
  - Case 1: SellableQuantity = 1, StockStatus = "Còn hàng"
  - Case 2: SellableQuantity = 0, StockStatus = "Hết hàng"
- **Rationale:** Xác nhận quy đổi trạng thái tồn kho chuẩn xác từ số lượng.

---

## UT-005-10
- **Tiêu đề:** Component không tồn tại
- **Module:** PRICING - Tính số lượng khả dụng Combo
- **Unit under test:** ProductService.CalculateAvailableQty
- **Loại:** Quirk | **Priority:** P2
- **Precondition / Mock setup:**
  ```
  combo-001 có Core mat-001 không tồn tại trong bảng Product/Tồn kho
  ```
- **Input:** `productIds = ["combo-001"]`
- **Expected output:** `SellableQuantity = 0` (coi thành phần không tìm thấy có stock = 0).
- **Rationale:** Xác nhận hệ thống xử lý an toàn và không crash khi thiếu thông tin tồn kho thành phần.

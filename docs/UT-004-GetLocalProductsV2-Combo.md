# UT-004: API GetLocalProductsV2 - Tính số lượng khả dụng Combo

> **Lưu ý:** Logic tính toán được test trong UT-005. UT-004 chỉ verify response structure và integration.

## Thông tin chung (Common Metadata)
- **Phiên bản:** v0.1
- **Cập nhật gần nhất:** 11/08/2026
- **Author / Owner:** Nguyễn Tùng Dương
- **Reviewer / Approver:** Chưa chỉ định
- **Suite:** REGRESSION

---

## UT-004-01
- **Tiêu đề:** Combo không biến thể có SellableQuantity
- **Module:** PRODUCT - Lấy danh sách sản phẩm (Phiên bản 2)
- **Unit under test:** ProductController.GetLocalProductsV2
- **Loại:** Happy | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-001 (ProductType=Combo, ProductParent=null)
  Có 1 thành phần Core: Stock=100, RequiredQty=1
  ```
- **Input:** `GET /api/v2/products`
- **Expected output:**
  ```
  HTTP 200
  items chứa combo-001: SellableQuantity = 100, StockStatus = "Còn hàng"
  ```
- **Rationale:** Xác nhận response chứa SellableQuantity và StockStatus cho Combo không biến thể.

---

## UT-004-02
- **Tiêu đề:** Cha combo có biến thể có SellableQuantity
- **Module:** PRODUCT - Lấy danh sách sản phẩm (Phiên bản 2)
- **Unit under test:** ProductController.GetLocalProductsV2
- **Loại:** Happy | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-parent-001 (ProductType=Sản Phẩm) có 2 biến thể:
  - variant-001 (ProductType=Combo, ParentProductId=combo-parent-001): được tính ra qty=50 từ components
  - variant-002 (ProductType=Combo, ParentProductId=combo-parent-001): được tính ra qty=20 từ components
  ```
- **Input:** `GET /api/v2/products`
- **Expected output:**
  ```
  HTTP 200
  items chứa combo-parent-001: SellableQuantity = min(50, 20) = 20, StockStatus = "Còn hàng"
  ```
- **Rationale:** Xác nhận cha combo hiển thị min của các biến thể con.

---

## UT-004-03
- **Tiêu đề:** Sản phẩm thường không có SellableQuantity
- **Module:** PRODUCT - Lấy danh sách sản phẩm (Phiên bản 2)
- **Unit under test:** ProductController.GetLocalProductsV2
- **Loại:** Happy | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  product-001 (ProductType=Sản Phẩm), không phải Combo, không có biến thể Combo, Stock = 15
  ```
- **Input:** `GET /api/v2/products`
- **Expected output:**
  ```
  HTTP 200
  items chứa product-001: SellableQuantity = 15, StockStatus = "Còn hàng"
  ```
- **Rationale:** Xác nhận sản phẩm thường lấy số lượng trực tiếp từ tồn kho (Stock), không qua công thức Combo.

---

## UT-004-04
- **Tiêu đề:** Mixed products - chỉ combo có
- **Module:** PRODUCT - Lấy danh sách sản phẩm (Phiên bản 2)
- **Unit under test:** ProductController.GetLocalProductsV2
- **Loại:** Happy | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  product-001: Sản phẩm thường, Stock = 10
  combo-002: Combo không biến thể, tính toán từ công thức ra qty = 30
  product-003: Sản phẩm thường, Stock = 0
  ```
- **Input:** `GET /api/v2/products`
- **Expected output:**
  ```
  HTTP 200
  - product-001: SellableQuantity = 10, StockStatus = "Còn hàng"
  - combo-002: SellableQuantity = 30, StockStatus = "Còn hàng"
  - product-003: SellableQuantity = 0, StockStatus = "Hết hàng"
  ```
- **Rationale:** Xác nhận tính đồng bộ SellableQuantity và StockStatus cho cả sản phẩm thường (lấy trực tiếp) và Combo (tính toán).

---

## UT-004-05
- **Tiêu đề:** Danh sách rỗng - không lỗi
- **Module:** PRODUCT - Lấy danh sách sản phẩm (Phiên bản 2)
- **Unit under test:** ProductController.GetLocalProductsV2
- **Loại:** Happy | **Priority:** P2
- **Precondition / Mock setup:**
  ```
  Database không có sản phẩm nào
  ```
- **Input:** `GET /api/v2/products`
- **Expected output:**
  ```
  HTTP 200, items = [], totalCount = 0
  Không throw exception, không gọi hàm CalculateAvailableQty
  ```
- **Rationale:** Xác nhận early return khi không có sản phẩm, không gây lỗi.

---

## UT-004-06
- **Tiêu đề:** Combo không có công thức → qty=0
- **Module:** PRODUCT - Lấy danh sách sản phẩm (Phiên bản 2)
- **Unit under test:** ProductController.GetLocalProductsV2
- **Loại:** Boundary | **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-001 (ProductType=Combo), không có ProductSpecification
  ```
- **Input:** `GET /api/v2/products`
- **Expected output:**
  ```
  HTTP 200
  items chứa combo-001: SellableQuantity = 0, StockStatus = "Hết hàng"
  ```
- **Rationale:** Xác nhận Combo không có công thức → SellableQuantity = 0.

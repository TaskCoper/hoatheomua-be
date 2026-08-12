# UT-013: API GetLocalProductDetail - Chi tiết sản phẩm với số lượng khả dụng Combo

> **Lưu ý:** Logic tính toán số lượng khả dụng Combo được test trong UT-005. UT-013 chỉ verify response structure và integration.

---

## UT-013-01
- **Tiêu đề:** Lấy chi tiết sản phẩm - Sản phẩm thường không có SellableQuantity
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Happy
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  product-001:
  - Id = guid-001
  - ProductType = "Sản Phẩm"
  - ProductParent = null
  - Không có biến thể nào thuộc ProductType=Combo
  - IsDeleted = false
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-001}
  ```
- **Expected output:**
  ```
  HTTP 200
  Response chứa ProductDetailResponse:
  - Id = guid-001
  - SellableQuantity = null
  - StockStatus = null
  ```
- **Rationale:** Xác nhận sản phẩm thường không có SellableQuantity/StockStatus.

---

## UT-013-02
- **Tiêu đề:** Lấy chi tiết sản phẩm - Combo không biến thể có SellableQuantity
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Happy
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-001:
  - Id = guid-002
  - ProductType = "Combo"
  - ProductParent = null
  - Có 1 thành phần Core: Stock=100, RequiredQty=1
  - IsDeleted = false
  
  Expected: SellableQuantity = 100, StockStatus = "Còn hàng"
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-002}
  ```
- **Expected output:**
  ```
  HTTP 200
  Response chứa ProductDetailResponse:
  - Id = guid-002
  - ProductType = "Combo"
  - SellableQuantity = 100
  - StockStatus = "Còn hàng"
  ```
- **Rationale:** Xác nhận Combo không biến thể có SellableQuantity được tính đúng.

---

## UT-013-03
- **Tiêu đề:** Lấy chi tiết sản phẩm - Cha combo có biến thể có SellableQuantity
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Happy
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-parent-001:
  - Id = guid-003
  - ProductType = "Sản Phẩm"
  - ProductParent = null
  - Có 2 biến thể Combo con:
    - variant-001: SellableQuantity = 50
    - variant-002: SellableQuantity = 20
  - IsDeleted = false
  
  Expected: SellableQuantity = min(50, 20) = 20
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-003}
  ```
- **Expected output:**
  ```
  HTTP 200
  Response chứa ProductDetailResponse:
  - Id = guid-003
  - ProductType = "Sản Phẩm"
  - SellableQuantity = 20
  - StockStatus = "Còn hàng"
  ```
- **Rationale:** Xác nhận Cha combo hiển thị min của các biến thể con.

---

## UT-013-04
- **Tiêu đề:** Lấy chi tiết sản phẩm - Sản phẩm không tồn tại
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Error
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  Database không có sản phẩm với Id = guid-999
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-999}
  ```
- **Expected output:**
  ```
  HTTP 404
  Error response với message chứa "PRODUCT_NOT_FOUND"
  ```
- **Rationale:** Xác nhận khi sản phẩm không tồn tại thì throw NotFoundException.

---

## UT-013-05
- **Tiêu đề:** Lấy chi tiết sản phẩm - Combo hết hàng có StockStatus = "Hết hàng"
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Happy
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-002:
  - Id = guid-004
  - ProductType = "Combo"
  - ProductParent = null
  - Có 1 thành phần Core: Stock=0, RequiredQty=1
  - IsDeleted = false
  
  Expected: SellableQuantity = 0, StockStatus = "Hết hàng"
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-004}
  ```
- **Expected output:**
  ```
  HTTP 200
  Response chứa ProductDetailResponse:
  - Id = guid-004
  - SellableQuantity = 0
  - StockStatus = "Hết hàng"
  ```
- **Rationale:** Xác nhận Combo hết hàng có SellableQuantity = 0 và StockStatus = "Hết hàng".

---

## UT-013-06
- **Tiêu đề:** Lấy chi tiết sản phẩm - Combo không có công thức
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Boundary
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-003:
  - Id = guid-005
  - ProductType = "Combo"
  - ProductParent = null
  - Không có ProductSpecification nào
  - IsDeleted = false
  
  Expected: SellableQuantity = 0, StockStatus = "Hết hàng"
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-005}
  ```
- **Expected output:**
  ```
  HTTP 200
  Response chứa ProductDetailResponse:
  - Id = guid-005
  - SellableQuantity = 0
  - StockStatus = "Hết hàng"
  ```
- **Rationale:** Xác nhận Combo không có công thức → SellableQuantity = 0.

---

## UT-013-07
- **Tiêu đề:** Lấy chi tiết sản phẩm - Sản phẩm đã bị xóa mềm
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Happy
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  product-deleted:
  - Id = guid-006
  - ProductType = "Sản Phẩm"
  - IsDeleted = true
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-006}
  ```
- **Expected output:**
  ```
  HTTP 404
  Error response với message chứa "PRODUCT_NOT_FOUND"
  ```
- **Rationale:** Xác nhận sản phẩm đã bị xóa mềm thì coi như không tồn tại.

---

## UT-013-08
- **Tiêu đề:** Lấy chi tiết sản phẩm - Cha combo có biến thể đều hết hàng
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Happy
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  combo-parent-002:
  - Id = guid-007
  - ProductType = "Sản Phẩm"
  - ProductParent = null
  - Có 2 biến thể Combo con:
    - variant-001: SellableQuantity = 0
    - variant-002: SellableQuantity = 0
  - IsDeleted = false
  
  Expected: SellableQuantity = min(0, 0) = 0
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-007}
  ```
- **Expected output:**
  ```
  HTTP 200
  Response chứa ProductDetailResponse:
  - Id = guid-007
  - SellableQuantity = 0
  - StockStatus = "Hết hàng"
  ```
- **Rationale:** Xác nhận Cha combo có tất cả biến thể hết hàng → SellableQuantity = 0.

---

## UT-013-09
- **Tiêu đề:** Lấy chi tiết sản phẩm - Truyền Id của biến thể (sản phẩm con)
- **Ghi chú:** -
- **Phiên bản:** v0.1
- **Author:** Nguyễn Tùng Dương
- **Reviewer:** Chưa chỉ định
- **Approver:** Chưa chỉ định
- **Owner:** Nguyễn Tùng Dương
- **Cập nhật gần nhất:** 12/08/2026
- **Module:** PRODUCT - Chi tiết sản phẩm
- **Unit under test:** ProductController.GetLocalProductDetail
- **Loại:** Happy
- **Suite:** REGRESSION
- **Priority:** P1
- **Precondition / Mock setup:**
  ```
  Biến thể của combo (sản phẩm con):
  - Id = guid-008
  - ProductType = "Combo"
  - ProductParent = guid-003 (Id của cha combo)
  - Có thành phần Core: Stock=50, RequiredQty=1
  - IsDeleted = false
  
  Expected: SellableQuantity = 50 (tính riêng cho biến thể này)
  ```
- **Input:**
  ```
  GET /api/v1/products/{guid-008}
  ```
- **Expected output:**
  ```
  HTTP 200
  Response chứa ProductDetailResponse:
  - Id = guid-008
  - ProductType = "Combo"
  - ProductParent = guid-003
  - SellableQuantity = 50
  - StockStatus = "Còn hàng"
  ```
- **Rationale:** Xác nhận biến thể (sản phẩm con) vẫn là Product hợp lệ, trả về thông tin và SellableQuantity của chính biến thể đó.

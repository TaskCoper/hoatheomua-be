# Đối chiếu Fields: Excel (Nhanh.vn) ↔ Entity `Product.cs`

> Nguồn Excel: `Nhanh.vn_Products_2026-07-14_163156.xlsx` — **62 cột** (A → BJ)
> Entity: `HoaTheoMua.Repository/Entity/Product.cs`

## Bảng 1 — Tất cả fields từ Excel (Tiếng Việt / Tiếng Anh) ↔ Entity tương ứng

| Cột | Header Excel (Tiếng Việt) | English | Entity Field (`Product.cs`) | Kiểu | Trạng thái |
|-----|---------------------------|---------|-----------------------------|------|-----------|
| A  | ID sản phẩm | Product ID | `NhanhProductId` | `string?` | ✅ Có |
| B  | Id sản phẩm cha | Parent Product ID | `ParentProductId` | `string?` | ✅ Có |
| C  | Mã sản phẩm cha | Parent Product Code | *(chưa có)* | — | ❌ Thiếu |
| D  | Tên sản phẩm cha | Parent Product Name | *(chưa có)* | — | ❌ Thiếu |
| E  | Mã vạch | Barcode | `Barcode` | `string?` | ✅ Có |
| F  | Loại sản phẩm | Product Type | `ProductType` | `string?` | ✅ Có |
| G  | Mã sản phẩm | Product Code | `ProductCode` | `string?` | ✅ Có |
| H  | Tên sản phẩm | Product Name | `ProductName` | `string?` | ✅ Có |
| I  | Tên khác | Other Name / Alias | *(chưa có)* | — | ❌ Thiếu |
| J  | Đơn vị tính | Unit | `Unit` | `string?` | ✅ Có |
| K  | Link ảnh sản phẩm | Product Image Link | `CoverImage` | `string?` | ✅ Có |
| L  | Số tháng bảo hành | Warranty Months | `WarrantyMonths` | `int?` | ✅ Có |
| M  | Cân nặng cả vỏ hộp | Weight (with box) | `Weight` | `decimal?` | ✅ Có |
| N  | Giá nhập | Import Price | `ImportPrice` | `decimal?` | ✅ Có |
| O  | VAT nhập | Import VAT | `ImportVat` | `decimal?` | ✅ Có |
| P  | Giá nhập + VAT | Import Price + VAT | `ImportPriceWithVat` | `decimal?` | ✅ Có |
| Q  | Vat Mode giá nhập | Import VAT Mode | `ImportVatMode` | `string?` | ✅ Có |
| R  | Giá bán | Sale Price | `Price` | `decimal?` | ✅ Có |
| S  | Giá bán + VAT | Sale Price + VAT | `PriceWithVat` | `decimal?` | ✅ Có |
| T  | Giá bán chi nhánh | Branch Sale Price | `BranchPrice` | `decimal?` | ✅ Có |
| U  | Giá bán chi nhánh có VAT | Branch Sale Price with VAT | `BranchPriceWithVat` | `decimal?` | ✅ Có |
| V  | Vat mode giá bán | Sale VAT Mode | `SaleVatMode` | `string?` | ✅ Có |
| W  | Giá cũ | Old Price | `OldPrice` | `decimal?` | ✅ Có |
| X  | VAT | VAT | `Vat` | `decimal?` | ✅ Có |
| Y  | % Lợi nhuận | Profit Percentage | `ProfitPercentage` | `decimal?` | ✅ Có |
| Z  | Giá vốn | Cost Price | `CostPrice` | `decimal?` | ✅ Có |
| AA | Giá sỉ | Wholesale Price | `WholesalePrice` | `decimal?` | ✅ Có |
| AB | Giá sỉ + VAT | Wholesale Price + VAT | `WholesalePriceWithVat` | `decimal?` | ✅ Có |
| AC | Giá sỉ chi nhánh | Wholesale Branch Price | `WholesaleBranchPrice` | `decimal?` | ✅ Có |
| AD | Giá sỉ chi nhánh có VAT | Wholesale Branch Price with VAT | `WholesaleBranchPriceWithVat` | `decimal?` | ✅ Có |
| AE | Tổng tồn | Total Stock | `TotalStock` | `decimal?` | ✅ Có |
| AF | Đang chuyển kho | Stock Transferring | `StockTransferring` | `decimal?` | ✅ Có |
| AG | Tồn | Stock | `Stock` | `decimal?` | ✅ Có |
| AH | Đang Giao hàng | Delivering Stock | `DeliveringStock` | `decimal?` | ✅ Có |
| AI | Tồn trong kho | Warehouse Stock | `WarehouseStock` | `decimal?` | ✅ Có |
| AJ | Lỗi | Stock Error | `StockError` | `decimal?` | ✅ Có |
| AK | Tạm giữ | Held | `Held` | `decimal?` | ✅ Có |
| AL | Có thể bán | Available For Sale | `AvailableForSale` | `decimal?` | ✅ Có |
| AM | Bảo hành | Warranty (stock) | *(chưa có)* | — | ❌ Thiếu |
| AN | Tạm giữ linh kiện | Held Components | *(chưa có)* | — | ❌ Thiếu |
| AO | Chờ nhập hàng | Pending Import Stock | `PendingImportStock` | `decimal?` | ✅ Có |
| AP | Đặt trước | Pre-order | `PreOrder` | `decimal?` | ✅ Có |
| AQ | Trạng thái | Status | `Status` | `string?` | ✅ Có |
| AR | Mã danh mục | Category Code | `CategoryCode` | `string?` | ✅ Có |
| AS | Danh mục | Category Name | `CategoryName` | `string?` | ✅ Có |
| AT | Mã danh mục nội bộ | Internal Category Code | *(chưa có)* | — | ❌ Thiếu |
| AU | Thương hiệu | Brand | `Brand` | `string?` | ✅ Có |
| AV | Nhà cung cấp | Supplier | `Supplier` | `string?` | ✅ Có |
| AW | Địa chỉ bảo hành | Warranty Address | *(chưa có)* | — | ❌ Thiếu |
| AX | Link trên website | Website Link | `Slug` | `string?` | ✅ Có |
| AY | Chiều dài | Length | *(chưa có)* | — | ❌ Thiếu |
| AZ | Chiều rộng | Width | *(chưa có)* | — | ❌ Thiếu |
| BA | Chiều cao | Height | *(chưa có)* | — | ❌ Thiếu |
| BB | Thứ tự trên Website | Website Order | `WebOrder` | `int?` | ✅ Có |
| BC | Ngày tạo | Created Date | `NhanhCreatedAt` | `DateTimeOffset?` | ✅ Có |
| BD | Nhãn sản phẩm | Product Label | *(chưa có)* | — | ❌ Thiếu |
| BE | Giá bán chi nhánh | Branch Sale Price (dup) | `BranchPriceDuplicate` | `decimal?` | ✅ Có |
| BF | Giá sỉ chi nhánh | Wholesale Branch Price (dup) | `WholesaleBranchPriceDuplicate` | `decimal?` | ✅ Có |
| BG | Giá Tiền | Price Amount | `PriceLabel` | `string?` | ⚠️ Xem chú thích |
| BH | Kích thước | Size | `Size` | `string?` | ✅ Có |
| BI | Màu sắc | Color | `Color` | `string?` | ✅ Có |
| BJ | Xuất xứ | Origin | *(chưa có)* | — | ❌ Thiếu |

### ⚠️ Chú thích
- **Cột BG "Giá Tiền" (Price Amount)** map với `PriceLabel`. **Lưu ý kiểu dữ liệu:** "Giá Tiền" là **giá trị số**, trong khi `PriceLabel` đang khai báo `string?` → nếu cần tính toán/so sánh theo số nên cân nhắc đổi sang `decimal?` (hoặc đổi tên field cho khớp ngữ nghĩa, ví dụ `PriceAmount`).
- **Cột BD "Nhãn sản phẩm"** hiện **chưa có** field nhận → cần bổ sung (ví dụ `ProductLabel`).

---

## Bảng 2 — Tất cả fields của Entity `Product.cs`

| # | Entity Field | Kiểu | Map cột Excel | Ghi chú |
|---|-------------|------|--------------|---------|
| 1 | `SourceRowNumber` | `int` | — | Số thứ tự dòng nguồn (nội bộ) |
| 2 | `NhanhProductId` | `string?` | A | |
| 3 | `ParentProductId` | `string?` | B | |
| 4 | `Barcode` | `string?` | E | |
| 5 | `ProductType` | `string?` | F | |
| 6 | `ProductCode` | `string?` | G | |
| 7 | `ProductName` | `string?` | H | |
| 8 | `Unit` | `string?` | J | |
| 9 | `CoverImage` | `string?` | K | |
| 10 | `WarrantyMonths` | `int?` | L | |
| 11 | `Weight` | `decimal?` | M | |
| 12 | `ImportPrice` | `decimal?` | N | |
| 13 | `ImportVat` | `decimal?` | O | |
| 14 | `ImportPriceWithVat` | `decimal?` | P | |
| 15 | `ImportVatMode` | `string?` | Q | |
| 16 | `Price` | `decimal?` | R | |
| 17 | `PriceWithVat` | `decimal?` | S | |
| 18 | `BranchPrice` | `decimal?` | T | |
| 19 | `BranchPriceWithVat` | `decimal?` | U | |
| 20 | `SaleVatMode` | `string?` | V | |
| 21 | `OldPrice` | `decimal?` | W | |
| 22 | `Vat` | `decimal?` | X | |
| 23 | `ProfitPercentage` | `decimal?` | Y | |
| 24 | `CostPrice` | `decimal?` | Z | |
| 25 | `WholesalePrice` | `decimal?` | AA | |
| 26 | `WholesalePriceWithVat` | `decimal?` | AB | |
| 27 | `WholesaleBranchPrice` | `decimal?` | AC | |
| 28 | `WholesaleBranchPriceWithVat` | `decimal?` | AD | |
| 29 | `TotalStock` | `decimal?` | AE | |
| 30 | `StockTransferring` | `decimal?` | AF | |
| 31 | `Stock` | `decimal?` | AG | |
| 32 | `DeliveringStock` | `decimal?` | AH | |
| 33 | `WarehouseStock` | `decimal?` | AI | |
| 34 | `StockError` | `decimal?` | AJ | |
| 35 | `Held` | `decimal?` | AK | |
| 36 | `AvailableForSale` | `decimal?` | AL | |
| 37 | `PendingImportStock` | `decimal?` | AO | |
| 38 | `PreOrder` | `decimal?` | AP | |
| 39 | `Status` | `string?` | AQ | |
| 40 | `CategoryCode` | `string?` | AR | |
| 41 | `CategoryName` | `string?` | AS | |
| 42 | `Brand` | `string?` | AU | |
| 43 | `Supplier` | `string?` | AV | |
| 44 | `Slug` | `string?` | AX | |
| 45 | `WebOrder` | `int?` | BB | |
| 46 | `NhanhCreatedAt` | `DateTimeOffset?` | BC | |
| 47 | `BranchPriceDuplicate` | `decimal?` | BE | |
| 48 | `WholesaleBranchPriceDuplicate` | `decimal?` | BF | |
| 49 | `PriceLabel` | `string?` | BG | ⚠️ BG "Giá Tiền" là giá trị số — cân nhắc `decimal?` |
| 50 | `Size` | `string?` | BH | |
| 51 | `Color` | `string?` | BI | |
| 52 | `CreatedAt` | `DateTimeOffset` | — | Audit (nội bộ) |
| 53 | `UpdatedAt` | `DateTimeOffset?` | — | Audit (nội bộ) |

> Ngoài ra kế thừa từ `BaseEntity<Guid>`: `Id` (Guid), `IsDeleted` (bool) — không map với Excel.

---

## Tổng kết

- **Cột Excel:** 62 (A → BJ)
- **Đã map:** 50 cột
- **Thiếu 12 cột:** C, D, I, AM, AN, AT, AW, AY, AZ, BA, BD, BJ
- **Đã chốt:** `PriceLabel` map với cột BG "Giá Tiền" (⚠️ giá trị số nhưng field đang là `string?`)

### Danh sách field đề xuất bổ sung vào `Product.cs`

| Cột | Field đề xuất | Kiểu đề xuất |
|-----|--------------|--------------|
| C  | `ParentProductCode` | `string?` |
| D  | `ParentProductName` | `string?` |
| I  | `OtherName` | `string?` |
| AM | `WarrantyStock` | `decimal?` |
| AN | `HeldComponents` | `decimal?` |
| AT | `InternalCategoryCode` | `string?` |
| AW | `WarrantyAddress` | `string?` |
| AY | `Length` | `decimal?` |
| AZ | `Width` | `decimal?` |
| BA | `Height` | `decimal?` |
| BD | `ProductLabel` | `string?` |
| BJ | `Origin` | `string?` |

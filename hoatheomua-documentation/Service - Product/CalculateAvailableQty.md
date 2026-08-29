# Tính SellableQuantity (số lượng có thể bán)

Áp dụng cho `GET /api/v2/products` (`GetLocalProductsV2`) và `GetLocalProductDetail`.

## Phân loại sản phẩm

Phân biệt cha–con bằng chuỗi: `child.ParentProductId == parent.NhanhProductId`.
Sản phẩm gốc là sản phẩm có `ParentProductId` null hoặc rỗng.

| Loại                   | Điều kiện                                                                                | Cách tính                                                                       |
| ---------------------- | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| Sản phẩm thường        | `ProductType` khác `"Combo"`                                                             | `max(0, Stock)`                                                                 |
| Combo độc lập          | `ProductType == "Combo"`                                                                 | MIN theo nguyên liệu CORE                                                       |
| Cha của biến thể combo | `ProductType == "Sản phẩm"`, không có `ParentProductId`, có con `ProductType == "Combo"` | MAX số lượng của các biến thể                                                   |
| Biến thể con           | có `ParentProductId`                                                                     | Không hiển thị trong danh sách; chỉ xuất hiện trong `Children` của API chi tiết |

## Combo độc lập

Chỉ các dòng `ComboSpecification` có `IsCore = true`, `IsActive = true`, `IsDeleted = false`
và `Quantity > 0` mới ràng buộc số lượng bán được:

```
qty = MIN theo từng CORE spec:  floor(max(0, Mat.Stock) / spec.Quantity)
```

## Cha của biến thể combo

Các biến thể của cùng một sản phẩm cha phải dùng **cùng một tập nguyên liệu CORE** và chỉ khác
định mức. Vì mọi biến thể cùng rút từ một kho, số lượng của cha không được cộng số lượng từng
biến thể — cách cộng đó sẽ đếm cùng một tồn kho nhiều lần.

```
q_i = số lượng bán được của biến thể i

SellableQuantity(cha) = MAX(q_i)
```

Giá trị này biểu diễn số lượng tối đa có thể bán nếu khách chọn biến thể tiết kiệm nguyên liệu
nhất. Số lượng chính xác của từng lựa chọn vẫn được trả riêng trong `Children` của API chi tiết.

### Ví dụ

`mat.Stock = 100`, variant1 định mức 2, variant2 định mức 5:

```
q1 = floor(100/2) = 50
q2 = floor(100/5) = 20
→ SellableQuantity(cha) = MAX(50, 20) = 50
```

## StockStatus và UnavailableReason

`UnavailableReason` là mã hợp đồng cho frontend, không phải chuỗi hiển thị.

| Tình huống                                       | SellableQuantity | StockStatus  | UnavailableReason     |
| ------------------------------------------------ | ---------------- | ------------ | --------------------- |
| Còn hàng                                         | `> 0`            | `"Còn hàng"` | `null`                |
| Sản phẩm thường hết hàng                         | `0`              | `"Hết hàng"` | `null`                |
| Combo không có dòng công thức nào                | `0`              | `null`       | `"no_formula"`        |
| Combo có công thức nhưng không có CORE dùng được | `0`              | `null`       | `"no_core"`           |
| Combo hết nguyên liệu                            | `0`              | `null`       | `null`                |
| Lỗi khi tính                                     | `0`              | `null`       | `"calculation_error"` |

Với sản phẩm cha:

- Nếu ít nhất một biến thể có số lượng lớn hơn `0`, `StockStatus` là `"Còn hàng"` và
  `UnavailableReason` là `null`.
- Nếu mọi biến thể đều có số lượng `0`, `UnavailableReason` chỉ được giữ lại khi tất cả có cùng
  một lý do; các lý do khác nhau sẽ trả `null`.
- Lỗi tính ở một combo không làm hỏng các combo khác.

## Phạm vi tính toán

`SellableQuantity` là giá trị **tính trong C#**, không phải cột trong DB, nên không lọc/sắp xếp được
bằng SQL. Vì vậy API danh sách **không** hỗ trợ lọc hay sắp xếp theo nó — nó chỉ là dữ liệu trả kèm.
Nhờ đó `GetLocalProductsV2` phân trang ngay tại DB và chỉ tính tồn kho cho đúng các sản phẩm của
trang được yêu cầu (tối đa `pageSize` dòng), không phụ thuộc kích thước danh mục.

## Ghi chú triển khai

- Tồn kho âm luôn được clamp về 0; nguyên liệu không tìm thấy bản ghi coi như tồn kho 0.
- Dữ liệu công thức cần bảo đảm mọi biến thể của cùng một cha có cùng tập `MatId` CORE.
- `ComboSpecification` hiện **chưa có đường ghi** qua API — bản ghi chỉ tạo trực tiếp trong DB.
- Tồn kho combo **chưa bị trừ khi đặt hàng**; `Order` service chưa xử lý combo.

# ProductTest fix notes

## Service.cs đã sửa
- `HoaTheoMua.Service/Product/Service.cs:593`
  - Đổi điều kiện từ `x.Product.ParentProductId == null`
  - Thành `x.Product.ParentProductId == null || x.Product.ParentProductId == string.Empty`

- `HoaTheoMua.Service/Product/Service.cs:609`
  - Đổi điều kiện từ `x.ParentProductId == null`
  - Thành `x.ParentProductId == null || x.ParentProductId == string.Empty`

- `HoaTheoMua.Service/Product/Service.cs:646`
  - Đổi điều kiện từ `x.ParentProductId == null`
  - Thành `x.ParentProductId == null || x.ParentProductId == string.Empty`

- `HoaTheoMua.Service/Product/Service.cs:662`
  - Đổi điều kiện từ `x.Product.ParentProductId == null`
  - Thành `x.Product.ParentProductId == null || x.Product.ParentProductId == string.Empty`

- `HoaTheoMua.Service/Product/Service.cs:675`
  - Đổi điều kiện từ `x.ParentProductId == null`
  - Thành `x.ParentProductId == null || x.ParentProductId == string.Empty`

- `HoaTheoMua.Service/Product/Service.cs:736`
  - Đổi điều kiện từ `x.Product.ParentProductId == null`
  - Thành `x.Product.ParentProductId == null || x.Product.ParentProductId == string.Empty`

## Test liên quan
- `HoaTheoMua.Service.Test/ProductTest/PinAndMoveParentOnlyTest.cs:140`
  - `PinProduct_EmptyStringParentProductId_TreatedAsParent`
- `HoaTheoMua.Service.Test/ProductTest/PinAndMoveParentOnlyTest.cs:163`
  - `MoveProduct_EmptyStringParentProductId_TreatedAsParent`
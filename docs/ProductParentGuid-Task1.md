# Task 1 - Thêm `ParentProductGuid` cho Product

## 1. Mục tiêu

Quan hệ hiện tại từ Nhanh.vn:

```text
Product con.ParentProductId == Product cha.NhanhProductId
```

Quan hệ mới trong hệ thống:

```text
Product con.ParentProductGuid == Product cha.Id
```

Ví dụ:

```text
Product cha: NhanhProductId = "100", Id = Guid-A
Product con: ParentProductId = "100"

Kết quả: Product con.ParentProductGuid = Guid-A
```

Vẫn giữ `NhanhProductId` và `ParentProductId` vì đây là dữ liệu của Nhanh.vn.

## 2. Thêm field và navigation vào Product

File: `HoaTheoMua.Repository/Entity/Product.cs`

Thêm ba property:

```csharp
public Guid? ParentProductGuid { get; set; }
public Product? ParentProduct { get; set; }
public ICollection<Product> ChildProducts { get; set; } = new List<Product>();
```

Ý nghĩa:

- `ParentProductGuid`: khóa ngoại chứa `Id` của Product cha.
- `ParentProduct`: navigation từ Product con tới Product cha.
- `ChildProducts`: các Product con của Product cha.
- Product cha có `ParentProductGuid = null`.

Đây cũng là quan hệ trỏ về `Product`, tương tự `ComboSpecification.Sub` và `ComboSpecification.Mat` đều trỏ về `Product`. Điểm khác là quan hệ cha-con nằm ngay trong bảng `Products`, vì vậy không cần bảng trung gian.

## 3. Cấu hình quan hệ

File: `HoaTheoMua.Repository/Configurations/ProductConfiguration.cs`

Thêm:

```csharp
builder.HasOne(x => x.ParentProduct)
    .WithMany(x => x.ChildProducts)
    .HasForeignKey(x => x.ParentProductGuid)
    .OnDelete(DeleteBehavior.Restrict);
```

Quan hệ này có nghĩa:

```text
Một Product cha có nhiều ChildProducts.
Một Product con có tối đa một ParentProduct.
```

`ParentProductGuid` phải là `Guid?` vì Product cha không có cha.

Sau bước này bạn tự tạo migration và cập nhật database.

## 4. Cập nhật đúng giá trị `ParentProductGuid`

Sau khi cột đã được thêm, tạo một hàm cập nhật như sau:

```csharp
private async Task UpdateParentProductGuidsAsync()
{
    var products = await _dbContext.Products
        .Where(x => !x.IsDeleted)
        .ToListAsync();

    // Key: NhanhProductId của Product cha.
    // Value: Id nội bộ của Product cha.
    var productIdMap = products
        .Where(x => !string.IsNullOrWhiteSpace(x.NhanhProductId))
        .ToDictionary(
            x => x.NhanhProductId!.Trim(),
            x => x.Id);

    foreach (var product in products)
    {
        var parentNhanhId = product.ParentProductId?.Trim();

        if (string.IsNullOrWhiteSpace(parentNhanhId))
        {
            product.ParentProductGuid = null;
            continue;
        }

        product.ParentProductGuid = productIdMap.TryGetValue(
            parentNhanhId,
            out var parentGuid)
                ? parentGuid
                : null;
    }

    await _dbContext.SaveChangesAsync();
}
```

Logic chính của hàm:

```text
Lấy ParentProductId của Product con
    -> tìm Product có NhanhProductId tương ứng
    -> lấy Product.Id
    -> gán vào ParentProductGuid của Product con
```

Nếu `ParentProductId` null/rỗng hoặc không tìm thấy Product cha thì giữ `ParentProductGuid = null`. Không tự tạo Guid mới và không gán sang Product khác.

## 5. Thứ tự thực hiện

```text
1. Thêm field và navigation vào Product.
2. Thêm cấu hình quan hệ trong ProductConfiguration.
3. Tự tạo migration và cập nhật database.
4. Bảo đảm Product cha và Product con đã tồn tại trong bảng Products.
5. Chạy UpdateParentProductGuidsAsync().
6. Kiểm tra lại kết quả.
```

Phải cập nhật sau khi toàn bộ Product đã có trong database. Nếu xử lý từng dòng, Product con có thể xuất hiện trước Product cha và không tìm được `parent.Id`.

## 6. Kết quả đúng cần đạt

| Trường hợp | `ParentProductId` | `ParentProductGuid` |
|---|---|---|
| Product cha | null hoặc rỗng | null |
| Product con tìm thấy cha | bằng `NhanhProductId` của cha | bằng `Id` của cha |
| Product con chưa tìm thấy cha | có giá trị | null |

Checklist:

- [ ] Có `ParentProductGuid`, `ParentProduct` và `ChildProducts`.
- [ ] Có self-reference một-nhiều trong `ProductConfiguration`.
- [ ] `ParentProductGuid` của Product con bằng đúng `Id` của Product cha.
- [ ] Product cha có `ParentProductGuid = null`.
- [ ] Không resolve quan hệ trước khi toàn bộ Product đã được lưu.


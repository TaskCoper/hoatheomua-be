# ParentProductGuid - Migration và Excel import

## 1. Quy tắc quan hệ

Quan hệ từ Nhanh.vn:

```text
Product con.ParentProductId == Product cha.NhanhProductId
```

Quan hệ nội bộ cần lưu:

```text
Product con.ParentProductGuid = Product cha.Id
```

Ví dụ:

```text
Product cha:
Id = Guid-A
NhanhProductId = "100"

Product con:
ParentProductId = "100"

Kết quả:
Product con.ParentProductGuid = Guid-A
```

## 2. Entity và configuration

Trong `Product.cs`:

```csharp
public Guid? ParentProductGuid { get; set; }
public Product? ParentProduct { get; set; }
public ICollection<Product> ChildProducts { get; set; } = new List<Product>();
```

Trong `ProductConfiguration.cs`:

```csharp
builder.HasOne(x => x.ParentProduct)
    .WithMany(x => x.ChildProducts)
    .HasForeignKey(x => x.ParentProductGuid)
    .OnDelete(DeleteBehavior.Restrict);
```

## 3. Backfill trong migration

Trong `Up()`, đặt câu SQL này ngay sau `AddColumn`:

```csharp
migrationBuilder.Sql("""
    UPDATE "Products" AS child
    SET "ParentProductGuid" = parent."Id"
    FROM "Products" AS parent
    WHERE child."ParentProductId" IS NOT NULL
      AND child."ParentProductId" <> ''
      AND child."ParentProductId" = parent."NhanhProductId"
      AND child."Id" <> parent."Id";
    """);
```

Nếu không tìm thấy cha, `ParentProductGuid` giữ nguyên là null và migration vẫn hoàn thành.

Trong `Down()`:

```text
DropForeignKey
-> DropIndex
-> DropColumn ParentProductGuid
```

## 4. Cập nhật Excel import

Không thêm `ParentProductGuid` vào `ExcelRow`, vì file Nhanh.vn không có Guid nội bộ.

Trong `BuildUpsertBatchAsync`, giữ vòng lặp hiện tại để tạo/update toàn bộ Product trước:

```csharp
foreach (var row in rows)
{
    var product = ResolveProduct(row, existingMap, out var isUpdate);

    ApplyRowToProduct(row, product);
    product.UpdatedAt = DateTimeOffset.UtcNow;

    if (isUpdate)
    {
        batch.ToUpdate.Add(product);
    }
    else
    {
        batch.ToInsert.Add(product);
    }
}
```

Sau vòng lặp trên, toàn bộ Product mới đã có `Id` do `ResolveProduct` dùng `Guid.NewGuid()`.

Gộp Product trong database và Product vừa import để tìm cha:

```csharp
var allProducts = existingMap.Values
    .Concat(batch.ToInsert)
    .Concat(batch.ToUpdate)
    .DistinctBy(x => x.Id)
    .ToList();
```

Sau đó tìm cha và gán Guid:

```csharp
foreach (var productChild in batch.ToInsert.Concat(batch.ToUpdate))
{
    if (string.IsNullOrWhiteSpace(productChild.ParentProductId))
    {
        productChild.ParentProductGuid = null;
        continue;
    }

    var productParent = allProducts.FirstOrDefault(x =>
        x.Id != productChild.Id &&
        x.NhanhProductId == productChild.ParentProductId);

    if (productParent != null)
    {
        productChild.ParentProductGuid = productParent.Id;
    }
    else
    {
        productChild.ParentProductGuid = null;
    }
}
```

Cuối cùng giữ nguyên:

```csharp
return batch;
```

`PersistBatchAsync` không cần thay đổi. Nó sẽ lưu Product và `ParentProductGuid` trong transaction hiện tại.

## 5. Điểm cần chú ý

`existingMap` hiện chỉ load Product có `NhanhProductId` xuất hiện trong file. Nếu file Excel luôn là bản export đầy đủ thì đoạn trên đủ dùng.

Nếu có trường hợp import file chỉ chứa một phần sản phẩm và Product cha không nằm trong file, cần load thêm các Product có:

```text
NhanhProductId nằm trong danh sách ParentProductId của file
```

## Checklist

- [ ] Tạo/update toàn bộ Product trước.
- [ ] Tìm cha bằng `child.ParentProductId == parent.NhanhProductId`.
- [ ] Gán `child.ParentProductGuid = parent.Id`.
- [ ] Product không có cha thì `ParentProductGuid = null`.
- [ ] Gán quan hệ trước `PersistBatchAsync`.


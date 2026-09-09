```CSharp
public async Task<Response.LimitCoreProductResponse> GetLimitCoreProduct(Guid comboId)
    {
        var specifications = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x =>
                x.SubId == comboId &&
                !x.IsDeleted &&
                x.IsActive &&
                !x.Mat.IsDeleted)
            .Select(x => new
            {
                x.MatId,
                Quantity = x.Quantity ?? 0m,
                x.IsCore,
                x.IsActive,
                AvailableForSale = x.Mat.AvailableForSale ?? 0m
            })
            .ToListAsync();

        var activeCoreSpecifications = specifications
            .Where(x => x.IsCore && x.IsActive)
            .ToList();

        if (activeCoreSpecifications.Count == 0 ||
            activeCoreSpecifications.Any(x => x.Quantity <= 0))
        {
            throw new BadRequestException("Tồn tại vật liệu core với số lượng <= 0");
        }

        var sellableQuantity = activeCoreSpecifications
            .Select(x => Math.Floor(x.AvailableForSale / x.Quantity))
            .Min();

        return new Response.LimitCoreProductResponse
        {
            SellableQuantityCombo = sellableQuantity,
            Materials = specifications
                .Select(x => new Response.LimitCoreMaterialResponse
                {
                    MatId = x.MatId,
                    Quantity = x.Quantity,
                    IsCore = x.IsCore,
                    IsActive = x.IsActive,
                    AvailableForSale = x.AvailableForSale
                })
                .ToList()
        };
    }
```

## Phiên bản dùng chung cho Checkout và CreateOrder

```CSharp
public async Task<Response.LimitCoreProductResponse> GetLimitCoreProduct(Guid comboId)
{
    // root của Flower/Card phải trỏ về Product là Combo, không chỉ cần có ComboSpecification.
    var comboExists = await _dbContext.Products
        .AsNoTracking()
        .AnyAsync(product =>
            product.Id == comboId &&
            !product.IsDeleted &&
            product.IsActive &&
            product.ProductType == "Combo");

    if (!comboExists)
    {
        throw new BadRequestException("Combo nguồn không tồn tại hoặc không còn khả dụng");
    }

    // Không lọc IsActive/IsDeleted của specification hoặc material ngay tại đây.
    // Các bản ghi không hợp lệ của Core phải được phát hiện để chặn Checkout;
    // Support không hợp lệ phải được trả về để FE cảnh báo cần thay thế.
    var specifications = await _dbContext.ComboSpecifications
        .AsNoTracking()
        .Where(specification =>
            specification.SubId == comboId &&
            !specification.IsDeleted)
        .Select(specification => new
        {
            specification.MatId,
            Quantity = specification.Quantity ?? 0m,
            specification.IsCore,
            SpecificationIsActive = specification.IsActive,
            MaterialIsActive = specification.Mat.IsActive,
            MaterialIsDeleted = specification.Mat.IsDeleted,
            AvailableForSale = specification.Mat.AvailableForSale ?? 0m
        })
        .ToListAsync();

    var coreSpecifications = specifications
        .Where(specification => specification.IsCore)
        .ToList();

    var hasInvalidCore = coreSpecifications.Count == 0 ||
                         coreSpecifications.Any(specification =>
                             !specification.SpecificationIsActive ||
                             !specification.MaterialIsActive ||
                             specification.MaterialIsDeleted ||
                             specification.Quantity <= 0);

    if (hasInvalidCore)
    {
        throw new BadRequestException("Combo không có vật liệu Core hợp lệ để đặt hàng");
    }

    var sellableQuantity = coreSpecifications
        .Select(specification => Math.Floor(
            specification.AvailableForSale / specification.Quantity))
        .Min();

    return new Response.LimitCoreProductResponse
    {
        SellableQuantityCombo = sellableQuantity,
        Materials = specifications
            .Select(specification => new Response.LimitCoreMaterialResponse
            {
                MatId = specification.MatId,
                Quantity = specification.Quantity,
                IsCore = specification.IsCore,
                IsActive = specification.SpecificationIsActive,
                MaterialIsActive = specification.MaterialIsActive,
                MaterialIsDeleted = specification.MaterialIsDeleted,
                AvailableForSale = specification.AvailableForSale,
                NeedsReplacement = !specification.IsCore &&
                    (!specification.SpecificationIsActive ||
                     !specification.MaterialIsActive ||
                     specification.MaterialIsDeleted ||
                     specification.Quantity <= 0 ||
                     specification.AvailableForSale < specification.Quantity)
            })
            .ToList()
    };
}
```

Hàm trên yêu cầu bổ sung ba field vào `Response.LimitCoreMaterialResponse`:

```CSharp
public bool MaterialIsActive { get; set; }
public bool MaterialIsDeleted { get; set; }
public bool NeedsReplacement { get; set; }
```

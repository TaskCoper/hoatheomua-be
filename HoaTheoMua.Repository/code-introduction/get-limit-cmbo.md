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
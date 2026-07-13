using System.Globalization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace HoaTheoMua.Service.ProductImport;

using ProductEntity = HoaTheoMua.Repository.Entity.Product;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Response.ImportResult> ImportFromExcel(Stream fileStream)
    {
        var result = new Response.ImportResult();

        try
        {
            // --- Buffer to MemoryStream (allows seeking for validation + import) ---
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);

            // --- VALIDATE FILE STRUCTURE ---
            ms.Position = 0;
            var rawRows = await MiniExcel.QueryAsync(ms, useHeaderRow: false);
            var headerRow = rawRows.FirstOrDefault() as IDictionary<string, object?>;

            if (headerRow is not { Count: > 0 })
                throw new BadRequestException("File Excel rỗng");

            headerRow.TryGetValue("A", out var valA);
            var colA = valA?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(colA) ||
                !colA.Contains("ID", StringComparison.OrdinalIgnoreCase) ||
                headerRow.Count < 40)
            {
                throw new BadRequestException(
                    "File Excel không đúng định dạng xuất từ Nhanh.vn. " +
                    "Vui lòng upload file xuất từ Nhanh.vn, không dùng file tự chế.");
            }

            // --- ACTUAL IMPORT ---
            ms.Position = 0;
            var rows = (await MiniExcel.QueryAsync<Request.ExcelRow>(ms)).ToList();
            if (!rows.Any())
            {
                throw new BadRequestException("File Excel không có dữ liệu hoặc định dạng không đúng");
            }

            for (var i = 0; i < rows.Count; i++)
            {
                rows[i].SourceRowNumber = i + 2;
            }

            result.TotalRecords = rows.Count;

            var productIdsInFile = rows
                .Select(r => NullIfEmpty(r.NhanhProductId))
                .Where(id => id != null)
                .Distinct()
                .ToList();

            var existingProductsMap = await _dbContext.Products
                .Where(p => p.NhanhProductId != null && productIdsInFile.Contains(p.NhanhProductId) && !p.IsDeleted)
                .ToDictionaryAsync(p => p.NhanhProductId!);

            var listToUpdate = new List<ProductEntity>();
            var listToInsert = new List<ProductEntity>();

            foreach (var row in rows)
            {
                var cleanId = NullIfEmpty(row.NhanhProductId);
                ProductEntity? existingProduct = null;
                var isUpdate = cleanId != null && existingProductsMap.TryGetValue(cleanId, out existingProduct);

                var product = isUpdate && existingProduct != null ? existingProduct : new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                };

                product.SourceRowNumber = row.SourceRowNumber;
                product.NhanhProductId = cleanId;
                product.ParentProductId = NullIfEmpty(row.ParentProductId);
                product.Barcode = NullIfEmpty(row.Barcode);
                product.ProductType = NullIfEmpty(row.ProductType);
                product.ProductCode = NullIfEmpty(row.ProductCode);
                product.ProductName = NullIfEmpty(row.ProductName);
                product.Unit = NullIfEmpty(row.Unit);
                product.CoverImage = NullIfEmpty(row.CoverImage);

                product.WarrantyMonths = ParseInt(row.WarrantyMonthsRaw);
                product.Weight = ParseDecimal(row.WeightRaw);

                product.ImportPrice = ParseDecimal(row.ImportPriceRaw);
                product.ImportVat = ParseDecimal(row.ImportVatRaw);
                product.ImportPriceWithVat = ParseDecimal(row.ImportPriceWithVatRaw);
                product.ImportVatMode = NullIfEmpty(row.ImportVatMode);

                product.Price = ParseDecimal(row.PriceRaw);
                product.PriceWithVat = ParseDecimal(row.PriceWithVatRaw);
                product.BranchPrice = ParseDecimal(row.BranchPriceRaw);
                product.BranchPriceWithVat = ParseDecimal(row.BranchPriceWithVatRaw);
                product.SaleVatMode = NullIfEmpty(row.SaleVatMode);
                product.OldPrice = ParseDecimal(row.OldPriceRaw);
                product.Vat = ParseDecimal(row.VatRaw);

                product.ProfitPercentage = ParseDecimal(row.ProfitPercentageRaw);
                product.CostPrice = ParseDecimal(row.CostPriceRaw);
                product.WholesalePrice = ParseDecimal(row.WholesalePriceRaw);
                product.WholesalePriceWithVat = ParseDecimal(row.WholesalePriceWithVatRaw);
                product.WholesaleBranchPrice = ParseDecimal(row.WholesaleBranchPriceRaw);
                product.WholesaleBranchPriceWithVat = ParseDecimal(row.WholesaleBranchPriceWithVatRaw);

                product.TotalStock = ParseDecimal(row.TotalStockRaw);
                product.StockTransferring = ParseDecimal(row.StockTransferringRaw);
                product.Stock = ParseDecimal(row.StockRaw);
                product.DeliveringStock = ParseDecimal(row.DeliveringStockRaw);
                product.WarehouseStock = ParseDecimal(row.WarehouseStockRaw);
                product.StockError = ParseDecimal(row.StockErrorRaw);
                product.Held = ParseDecimal(row.HeldRaw);
                product.AvailableForSale = ParseDecimal(row.AvailableForSaleRaw);
                product.PendingImportStock = ParseDecimal(row.PendingImportStockRaw);
                product.PreOrder = ParseDecimal(row.PreOrderRaw);

                product.Status = NullIfEmpty(row.Status);
                product.CategoryCode = NullIfEmpty(row.CategoryCode);
                product.CategoryName = NullIfEmpty(row.CategoryName);
                product.Brand = NullIfEmpty(row.Brand);
                product.Supplier = NullIfEmpty(row.Supplier);
                product.Slug = NullIfEmpty(row.Slug);
                product.WebOrder = ParseInt(row.WebOrderRaw);
                product.NhanhCreatedAt = ParseDateTimeOffset(row.NhanhCreatedAtRaw);

                product.BranchPriceDuplicate = ParseDecimal(row.BranchPriceDuplicateRaw);
                product.WholesaleBranchPriceDuplicate = ParseDecimal(row.WholesaleBranchPriceDuplicateRaw);
                product.PriceLabel = NullIfEmpty(row.PriceLabel);
                product.Size = NullIfEmpty(row.Size);
                product.Color = NullIfEmpty(row.Color);

                product.UpdatedAt = DateTimeOffset.UtcNow;

                if (isUpdate)
                {
                    listToUpdate.Add(product);
                    result.UpdatedCount++;
                }
                else
                {
                    listToInsert.Add(product);
                    result.InsertedCount++;
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (listToInsert.Any())
                {
                    await _dbContext.Products.AddRangeAsync(listToInsert);
                }

                if (listToUpdate.Any())
                {
                    _dbContext.Products.UpdateRange(listToUpdate);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Import error details: {ex.Message}");
            result.ErrorCount = result.TotalRecords > 0 ? result.TotalRecords : 1;
            throw new BadRequestException($"Lỗi xử lý file Excel: {ex.Message}");
        }

        return result;
    }

    private static string? NullIfEmpty(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var trimmed = val.Trim();
        return trimmed.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }

    private static decimal? ParseDecimal(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();
        if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
        {
            return dec;
        }

        if (decimal.TryParse(val.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
        {
            return dec;
        }

        return null;
    }

    private static int? ParseInt(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();
        if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
        {
            return (int)Math.Round(dbl);
        }

        return null;
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();

        if (DateTimeOffset.TryParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dto))
        {
            return dto.ToUniversalTime();
        }

        if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out dto))
        {
            return dto.ToUniversalTime();
        }

        return null;
    }
}

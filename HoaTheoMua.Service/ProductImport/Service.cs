using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;

namespace HoaTheoMua.Service.ProductImport;

// Disambiguate Entity Product from Product namespace
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
            // MiniExcel: Query with head row check
            // MiniExcel index-based query ("A", "B", etc.) starts parsing from the first row containing data.
            // If the excel sheet has a header row, we skip it by ignoring the first item if it contains the header text.
            var rawRows = (await fileStream.QueryAsync<Request.ExcelRow>()).ToList();
            if (!rawRows.Any())
            {
                throw new BadRequestException("File Excel không có dữ liệu hoặc định dạng không đúng");
            }

            // Skip header row if it contains Vietnamese headers
            var rows = rawRows.Where(r =>
                r.NhanhProductId != "ID sản phẩm" &&
                !string.IsNullOrWhiteSpace(r.NhanhProductId) &&
                !string.IsNullOrWhiteSpace(r.ProductName)
            ).ToList();

            result.TotalRecords = rows.Count;

            // Fetch existing NhanhProductIds to do batch checking
            var productIdsInFile = rows.Select(r => r.NhanhProductId!.Trim()).Distinct().ToList();
            var existingProductsMap = await _dbContext.Products
                .Where(p => productIdsInFile.Contains(p.NhanhProductId) && !p.IsDeleted)
                .ToDictionaryAsync(p => p.NhanhProductId);

            var listToUpdate = new List<ProductEntity>();
            var listToInsert = new List<ProductEntity>();

            foreach (var row in rows)
            {
                var cleanId = row.NhanhProductId!.Trim();
                var isUpdate = existingProductsMap.TryGetValue(cleanId, out var existingProduct);

                var product = isUpdate ? existingProduct! : new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    NhanhProductId = cleanId,
                    ProductName = row.ProductName ?? string.Empty,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                // Map fields with type parse protection
                product.ParentProductId = NullIfEmpty(row.ParentProductId);
                product.Barcode = NullIfEmpty(row.Barcode);
                product.ProductType = NullIfEmpty(row.ProductType);
                product.ProductCode = NullIfEmpty(row.ProductCode);
                if (isUpdate)
                {
                    product.ProductName = row.ProductName ?? string.Empty;
                }
                product.Unit = NullIfEmpty(row.Unit);
                product.CoverImage = NullIfEmpty(row.CoverImage);

                product.WarrantyMonths = ParseInt(row.WarrantyMonthsRaw);
                product.Weight = ParseDecimal(row.WeightRaw);

                product.ImportPrice = ParseDecimal(row.ImportPriceRaw);
                product.ImportPriceWithVat = ParseDecimal(row.ImportPriceWithVatRaw);
                product.ImportVatMode = NullIfEmpty(row.ImportVatMode);

                product.Price = ParseDecimal(row.PriceRaw);
                product.PriceWithVat = ParseDecimal(row.PriceWithVatRaw);
                product.SaleVatMode = NullIfEmpty(row.SaleVatMode);

                product.ProfitPercentage = ParseDecimal(row.ProfitPercentageRaw);
                product.CostPrice = ParseDecimal(row.CostPriceRaw);
                product.WholesalePrice = ParseDecimal(row.WholesalePriceRaw);
                product.WholesalePriceWithVat = ParseDecimal(row.WholesalePriceWithVatRaw);

                product.TotalStock = ParseDecimal(row.TotalStockRaw);
                product.Stock = ParseDecimal(row.StockRaw);
                product.DeliveringStock = ParseDecimal(row.DeliveringStockRaw);
                product.WarehouseStock = ParseDecimal(row.WarehouseStockRaw);
                product.Held = ParseDecimal(row.HeldRaw);
                product.AvailableForSale = ParseDecimal(row.AvailableForSaleRaw);
                product.PreOrder = ParseDecimal(row.PreOrderRaw);
                product.PendingImportStock = ParseInt(row.PendingImportStockRaw);

                product.Status = NullIfEmpty(row.Status);
                product.CategoryCode = NullIfEmpty(row.CategoryCode);
                product.CategoryName = NullIfEmpty(row.CategoryName);
                product.Brand = NullIfEmpty(row.Brand);
                product.Supplier = NullIfEmpty(row.Supplier);
                product.Slug = NullIfEmpty(row.Slug);

                product.PriceLabel = NullIfEmpty(row.PriceLabel);
                product.Size = NullIfEmpty(row.Size);
                product.Color = NullIfEmpty(row.Color);

                // Date parse (Format YYYY-MM-DD HH:MM:SS)
                product.NhanhCreatedAt = ParseDateTimeOffset(row.NhanhCreatedAtRaw);

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

            // Save in transaction
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
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
        // Try parsing using comma as decimal separator for specific formats
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

        // 1. Try parse direct YYYY-MM-DD HH:MM:SS
        if (DateTimeOffset.TryParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dto))
        {
            return dto.ToUniversalTime();
        }

        // 2. Fallback general parse
        if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out dto))
        {
            return dto.ToUniversalTime();
        }

        return null;
    }
}

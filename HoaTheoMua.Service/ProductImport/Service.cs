using System.Globalization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;

namespace HoaTheoMua.Service.ProductImport;

using ProductEntity = HoaTheoMua.Repository.Entity.Product;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<Service> _logger;

    public Service(AppDbContext dbContext, ILogger<Service> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Response.ImportResult> ImportFromExcel(Stream fileStream)
    {
        // Đệm vào MemoryStream vì file cần đọc 2 lần (validate header + parse)
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);

        // Chỉ phần đọc file mới bọc thành 400 — lỗi DB để middleware trả 500
        var rows = await ReadAndValidateAsync(ms);

        var batch = await BuildUpsertBatchAsync(rows);

        _dbContext.Products.AddRange(batch.ToInsert);
        // Entity trong ToUpdate đã được change tracker theo dõi — không cần UpdateRange.
        // SaveChangesAsync tự chạy trong 1 transaction → all-or-nothing sẵn.
        await _dbContext.SaveChangesAsync();

        return new Response.ImportResult
        {
            TotalRecords = rows.Count,
            InsertedCount = batch.ToInsert.Count,
            UpdatedCount = batch.ToUpdate.Count
        };
    }

    // ── Đọc & validate file ────────────────────────────────────────────

    /** Validate header + parse toàn bộ dòng. Mọi lỗi đọc file → 400 (lỗi do file upload). */
    private async Task<List<Request.ExcelRow>> ReadAndValidateAsync(MemoryStream ms)
    {
        try
        {
            // 1. Kiểm tra dòng header: cột A chứa "ID" và có 40+ cột
            ms.Position = 0;
            var headerRow = (await MiniExcel.QueryAsync(ms, useHeaderRow: false))
                .FirstOrDefault() as IDictionary<string, object?>;

            if (headerRow is not { Count: > 0 })
                throw new BadRequestException("Excel file is empty");

            headerRow.TryGetValue("A", out var valA);
            var colA = valA?.ToString() ?? "";

            if (!colA.Contains("ID", StringComparison.OrdinalIgnoreCase) || headerRow.Count < 40)
                throw new BadRequestException(
                    "Invalid file format. Please upload an Excel file exported from Nhanh.vn.");

            // 2. Parse dòng dữ liệu, gán số dòng Excel gốc để truy vết
            ms.Position = 0;
            var rows = (await MiniExcel.QueryAsync<Request.ExcelRow>(ms)).ToList();

            if (rows.Count == 0)
                throw new BadRequestException("Excel file contains no data");

            for (var i = 0; i < rows.Count; i++)
                rows[i].SourceRowNumber = i + 2;

            return rows;
        }
        catch (AppException) { throw; }
        catch (Exception ex) // file hỏng / không phải xlsx thật
        {
            _logger.LogWarning(ex, "Failed to read product import Excel file");
            throw new BadRequestException($"Excel processing error: {ex.Message}");
        }
    }

    // ── Upsert batch ───────────────────────────────────────────────────

    private readonly record struct ImportBatch(
        List<ProductEntity> ToInsert,
        List<ProductEntity> ToUpdate);

    /** Nạp sản phẩm hiện có theo NhanhProductId (1 query), phân loại từng dòng insert/update. */
    private async Task<ImportBatch> BuildUpsertBatchAsync(List<Request.ExcelRow> rows)
    {
        var idsInFile = rows
            .Select(r => NullIfEmpty(r.NhanhProductId))
            .Where(id => id != null)
            .Distinct()
            .ToList();

        var map = idsInFile.Count > 0
            ? await _dbContext.Products
                .Where(p => p.NhanhProductId != null && idsInFile.Contains(p.NhanhProductId) && !p.IsDeleted)
                .ToDictionaryAsync(p => p.NhanhProductId!)
            : new Dictionary<string, ProductEntity>();

        var batch = new ImportBatch(new List<ProductEntity>(), new List<ProductEntity>());

        foreach (var row in rows)
        {
            var cleanId = NullIfEmpty(row.NhanhProductId);

            if (cleanId != null && map.TryGetValue(cleanId, out var product))
            {
                batch.ToUpdate.Add(product);
            }
            else
            {
                product = new ProductEntity { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
                batch.ToInsert.Add(product);

                // Đưa entity mới vào map — nếu file có dòng thứ 2 trùng ID,
                // dòng đó sẽ update entity này thay vì insert thêm bản ghi trùng
                if (cleanId != null)
                    map[cleanId] = product;
            }

            ApplyRowToProduct(row, product);
            product.UpdatedAt = DateTimeOffset.UtcNow;
        }

        return batch;
    }

    /** Áp dụng toàn bộ giá trị từ ExcelRow vào Product entity (map 62 cột A → BJ). */
    private static void ApplyRowToProduct(
        Request.ExcelRow row,
        ProductEntity product)
    {
        product.SourceRowNumber = row.SourceRowNumber;
        product.NhanhProductId = NullIfEmpty(row.NhanhProductId);
        product.ParentProductId = NullIfEmpty(row.ParentProductId);
        product.ParentProductCode = NullIfEmpty(row.ParentProductCode);
        product.ParentProductName = NullIfEmpty(row.ParentProductName);
        product.Barcode = NullIfEmpty(row.Barcode);
        product.ProductType = NullIfEmpty(row.ProductType);
        product.ProductCode = NullIfEmpty(row.ProductCode);
        product.ProductName = NullIfEmpty(row.ProductName);
        product.OtherName = NullIfEmpty(row.OtherName);
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
        product.WarrantyStock = ParseDecimal(row.WarrantyStockRaw);
        product.HeldComponents = ParseDecimal(row.HeldComponentsRaw);
        product.PendingImportStock = ParseDecimal(row.PendingImportStockRaw);
        product.PreOrder = ParseDecimal(row.PreOrderRaw);

        product.Status = NullIfEmpty(row.Status);
        product.CategoryCode = NullIfEmpty(row.CategoryCode);
        product.CategoryName = NullIfEmpty(row.CategoryName);
        product.InternalCategoryCode = NullIfEmpty(row.InternalCategoryCode);
        product.Brand = NullIfEmpty(row.Brand);
        product.Supplier = NullIfEmpty(row.Supplier);
        product.WarrantyAddress = NullIfEmpty(row.WarrantyAddress);
        product.Slug = NullIfEmpty(row.Slug);
        product.Length = ParseDecimal(row.LengthRaw);
        product.Width = ParseDecimal(row.WidthRaw);
        product.Height = ParseDecimal(row.HeightRaw);
        product.WebOrder = ParseInt(row.WebOrderRaw);
        product.NhanhCreatedAt = ParseDateTimeOffset(row.NhanhCreatedAtRaw);

        product.ProductLabel = NullIfEmpty(row.ProductLabel);
        product.BranchPriceDuplicate = ParseDecimal(row.BranchPriceDuplicateRaw);
        product.WholesaleBranchPriceDuplicate = ParseDecimal(row.WholesaleBranchPriceDuplicateRaw);
        product.PriceLabel = NullIfEmpty(row.PriceLabel);
        product.Size = NullIfEmpty(row.Size);
        product.Color = NullIfEmpty(row.Color);
        product.Origin = NullIfEmpty(row.Origin);
    }

    // ── Parse helpers ──────────────────────────────────────────────────

    /** Trả về null nếu chuỗi rỗng / chỉ chứa khoảng trắng / là chữ "null". */
    private static string? NullIfEmpty(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var trimmed = val.Trim();
        return trimmed.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }

    /** Parse số thập phân từ Excel (hỗ trợ cả dấu phẩy và chấm). */
    private static decimal? ParseDecimal(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();

        if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
            return dec;

        if (decimal.TryParse(val.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
            return dec;

        return null;
    }

    /** Parse số nguyên từ Excel, làm tròn nếu là số thập phân. */
    private static int? ParseInt(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();

        if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
            return (int)Math.Round(dbl);

        return null;
    }

    /** Parse ngày giờ dạng "yyyy-MM-dd HH:mm:ss" từ Excel. */
    private static DateTimeOffset? ParseDateTimeOffset(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();

        if (DateTimeOffset.TryParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dto))
            return dto.ToUniversalTime();

        if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out dto))
            return dto.ToUniversalTime();

        return null;
    }
}

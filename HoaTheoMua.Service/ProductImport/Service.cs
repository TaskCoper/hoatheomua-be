using System.Globalization;
using System.Text;
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
            using var ms = await BufferStreamAsync(fileStream);
            await ValidateFileStructureAsync(ms);

            var rows = await ReadRowsAsync(ms);
            result.TotalRecords = rows.Count;

            var isb = await BuildUpsertBatchAsync(rows);
            await PersistBatchAsync(isb);

            result.InsertedCount = isb.ToInsert.Count;
            result.UpdatedCount = isb.ToUpdate.Count;
        }
        catch (AppException) { throw; }
        catch (Exception ex)
        {
            Console.WriteLine($"Import error: {ex.Message}");
            throw new BadRequestException($"Excel processing error: {ex.Message}");
        }

        return result;
    }

    // ── Orchestration helpers ──────────────────────────────────────────

    /** Sao chép file stream vào MemoryStream để có thể seek (đọc nhiều lần). */
    //Tải toàn bộ nội dung của file từ network stream vào ram của server (memory stream)
    private static async Task<MemoryStream> BufferStreamAsync(Stream source)
    {
        var ms = new MemoryStream();
        await source.CopyToAsync(ms);
        return ms;
    }

    /** Kiểm tra dòng header: cột A phải chứa "ID" và file phải có 40+ cột. */
    //Validate cái file excel được nhận vào, có null không, cột đầu tiên có chứa id không
    //  có đủ độ dài tối thiếu không
    private static async Task ValidateFileStructureAsync(MemoryStream ms)
    {
        ms.Position = 0;
        var rawRows = await MiniExcel.QueryAsync(ms, useHeaderRow: false);
        var headerRow = rawRows.FirstOrDefault() as IDictionary<string, object?>;

        if (headerRow is not { Count: > 0 })
            throw new BadRequestException("Excel file is empty");

        headerRow.TryGetValue("A", out var valA);
        var colA = valA?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(colA) ||
            !colA.Contains("ID", StringComparison.OrdinalIgnoreCase) ||
            headerRow.Count < 40)
        {
            throw new BadRequestException(
                "Invalid file format. Please upload an Excel file exported from Nhanh.vn.");
        }
    }

    /** Parse dòng Excel qua MiniExcel và gán SourceRowNumber để truy vết. */
    private async Task<List<Request.ExcelRow>> ReadRowsAsync(MemoryStream ms)
    {
        ms.Position = 0;
        var rows = (await MiniExcel.QueryAsync<Request.ExcelRow>(ms)).ToList();

        if (rows.Count == 0)
            throw new BadRequestException("Excel file contains no data");

        for (var i = 0; i < rows.Count; i++)
            rows[i].SourceRowNumber = i + 2;

        return rows;
    }

    // ── Upsert batch ───────────────────────────────────────────────────

    private struct ImportBatch
    {
        public List<ProductEntity> ToInsert;
        public List<ProductEntity> ToUpdate;
    }

    /** Nạp sản phẩm từ DB theo NhanhProductId, phân loại từng dòng là insert hay update. */
    private async Task<ImportBatch> BuildUpsertBatchAsync(List<Request.ExcelRow> rows)
    {
        var idsInFile = rows
            .Select(r => NullIfEmpty(r.NhanhProductId))
            .Where(id => id != null)
            .Distinct()
            .ToList();
    //lúc này ta có được một list nhanhProdcutId từ file excel mà ta đã nhờ miniExcel đọc qua
        var existingMap = idsInFile.Count > 0
            ? await _dbContext.Products
                .Where(p => p.NhanhProductId != null && idsInFile.Contains(p.NhanhProductId) && !p.IsDeleted)
                .ToDictionaryAsync(p => p.NhanhProductId!)
            : new Dictionary<string, ProductEntity>();
    //lúc này ta có một dictionary chứ với key là: nhanhProductId và value là entity tương ứng của nó
        var batch = new ImportBatch
        {
            ToInsert = new List<ProductEntity>(),
            ToUpdate = new List<ProductEntity>()
        };

        foreach (var row in rows)
        {
            var product = ResolveProduct(row, existingMap, out var isUpdate);
            ApplyRowToProduct(row, product);
            product.UpdatedAt = DateTimeOffset.UtcNow;

            if (isUpdate)
                batch.ToUpdate.Add(product);
            else
                batch.ToInsert.Add(product);
        }

        return batch;
    }

    /** Tìm product theo NhanhProductId, nếu có thì update, không có thì tạo mới. */
    private static ProductEntity ResolveProduct(
        Request.ExcelRow row,
        Dictionary<string, ProductEntity> existingMap,
        out bool isUpdate)
    {
        var cleanId = NullIfEmpty(row.NhanhProductId);

        if (cleanId != null && existingMap.TryGetValue(cleanId, out var existing))
        {
            isUpdate = true;
            return existing;
        }

        isUpdate = false;
        return new ProductEntity
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    /** Áp dụng toàn bộ giá trị từ ExcelRow vào Product entity (map 50 cột). */
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

    /** Lưu batch vào DB trong 1 transaction (rollback nếu lỗi). */
    private async Task PersistBatchAsync(ImportBatch batch)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            if (batch.ToInsert.Count > 0)
                await _dbContext.Products.AddRangeAsync(batch.ToInsert);

            if (batch.ToUpdate.Count > 0)
                _dbContext.Products.UpdateRange(batch.ToUpdate);

            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Parse helpers ──────────────────────────────────────────────────

    /** Trả về null nếu chuỗi rỗng / chỉ chứa khoảng trắng / là chữ "null". */
    private static string? NullIfEmpty(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var cleaned = val.Replace('\u00A0', ' ').Trim().Normalize(NormalizationForm.FormC);
        return cleaned.Equals("null", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
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

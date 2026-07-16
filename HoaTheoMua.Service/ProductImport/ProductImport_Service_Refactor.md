# Review & Refactor: `ProductImport/Service.cs`

## 1. Đánh giá code hiện tại

### Vì sao code trên mạng "ngắn gọn" hơn?

Các video/tutorial import Excel thường ngắn vì họ **bỏ qua** những thứ code production bắt buộc phải có:

| Tutorial | Code này |
|---|---|
| Insert-only, chạy lại là trùng dữ liệu | **Upsert** theo `NhanhProductId` — import lại bao nhiêu lần cũng an toàn |
| Không validate — file sai là nổ exception khó hiểu | Validate header trước, báo lỗi 400 rõ ràng |
| Giả định dữ liệu sạch, đúng kiểu | Parse an toàn dữ liệu bẩn (`"null"`, dấu phẩy/chấm, `"3.0"`…) |
| Map 4–5 cột demo | Map **50 cột** thật — phần này không rút ngắn được |
| Query DB trong vòng lặp (N+1) | 1 query duy nhất + Dictionary lookup |

👉 **Cấu trúc tổng thể (5 bước, tách hàm nhỏ) là đúng hướng, không cần đập đi xây lại.**

### Nhưng có 4 vấn đề thật sự cần sửa

1. **`catch (Exception)` quá rộng ở `ImportFromExcel`** *(sai ngữ nghĩa HTTP)*
   Mọi lỗi ngoài dự kiến — kể cả **DB mất kết nối, bug code** — đều bị bọc thành `BadRequestException` → client nhận 400 "Excel processing error" trong khi lỗi thuộc về server (đáng lẽ 500). Chỉ nên bọc 400 quanh **phần đọc/parse file** (nơi lỗi thật sự do file người dùng upload); phần DB để middleware xử lý thành 500.
   Ngoài ra `Console.WriteLine` nên thay bằng `ILogger`.

2. **Transaction thủ công là thừa** *(code thừa ~20 dòng)*
   `SaveChangesAsync()` của EF Core **vốn đã chạy trong 1 transaction** — tất cả insert/update trong 1 lần save là all-or-nothing sẵn. `BeginTransaction/Commit/Rollback` chỉ cần khi có **nhiều lần** `SaveChanges` hoặc thao tác SQL raw xen kẽ. Cả `PersistBatchAsync` có thể xóa.

3. **`UpdateRange` là thừa và có hại nhẹ**
   Các entity trong `ToUpdate` được load từ chính `_dbContext` → đã được **change tracker theo dõi**; EF tự sinh UPDATE cho các cột thay đổi. Gọi `UpdateRange` ép EF đánh dấu **mọi cột** là modified → câu UPDATE to hơn không cần thiết. (`AddRangeAsync` cũng vậy — bản async chỉ có ích với HiLo value generator; dùng `AddRange` thường là đủ.)

4. **🐛 Bug: `NhanhProductId` mới xuất hiện 2 lần trong file → insert trùng 2 bản ghi**
   `existingMap` chỉ chứa sản phẩm **đã có trong DB**. Nếu file có 2 dòng cùng 1 ID mới, cả 2 dòng đều rơi vào nhánh "tạo mới" → 2 record trùng `NhanhProductId`. Fix: sau khi tạo entity mới, **đưa luôn vào map** để dòng sau cùng ID sẽ update entity đó thay vì insert thêm.

## 2. Code sau refactor

Ngắn hơn (~bỏ 40 dòng), đúng ngữ nghĩa lỗi, fix bug trùng ID. `ApplyRowToProduct` và 4 parse helper **giữ nguyên** (chúng đã đúng — chỉ dài vì bản chất 50 cột).

```csharp
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
            var isUpdate = cleanId != null && map.TryGetValue(cleanId, out var product);

            if (!isUpdate)
            {
                product = new ProductEntity { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
                batch.ToInsert.Add(product!);

                // FIX: đưa entity mới vào map — nếu file có dòng thứ 2 trùng ID,
                // dòng đó sẽ update entity này thay vì insert thêm bản ghi trùng
                if (cleanId != null)
                    map[cleanId] = product!;
            }
            else
            {
                batch.ToUpdate.Add(product!);
            }

            ApplyRowToProduct(row, product!);
            product!.UpdatedAt = DateTimeOffset.UtcNow;
        }

        return batch;
    }

    // ── Giữ nguyên từ bản cũ ───────────────────────────────────────────
    // ApplyRowToProduct(...)      — map 50 cột, không đổi
    // NullIfEmpty(...)            — không đổi
    // ParseDecimal(...)           — không đổi
    // ParseInt(...)               — không đổi
    // ParseDateTimeOffset(...)    — không đổi
}
```

> Lưu ý: thêm `ILogger<Service>` vào constructor — DI của ASP.NET Core tự cung cấp, không cần đăng ký gì thêm trong `Program.cs`.

## 3. Tóm tắt thay đổi

| # | Thay đổi | Lý do |
|---|---|---|
| 1 | Bỏ `try/catch` bao trùm ở `ImportFromExcel`; chỉ bọc 400 quanh phần đọc file | Lỗi DB/server phải là 500 (middleware lo), không phải 400 "Excel error" |
| 2 | Xóa `PersistBatchAsync` + transaction thủ công | `SaveChangesAsync` đã atomic sẵn |
| 3 | Bỏ `UpdateRange`, `AddRangeAsync` → `AddRange` | Entity update đã được track; `UpdateRange` ép ghi mọi cột |
| 4 | Gộp `BufferStream/Validate/ReadRows` thành `ReadAndValidateAsync` | Ít hàm hơn, cùng 1 mối quan tâm "đọc file" |
| 5 | Gộp `ResolveProduct` vào vòng lặp + **fix bug trùng ID mới trong file** | ID mới xuất hiện 2 lần không còn insert 2 bản ghi |
| 6 | `Console.WriteLine` → `ILogger` | Log chuẩn, có stack trace, theo môi trường |

## 4. Những gì KHÔNG đổi (và vì sao)

- **`ApplyRowToProduct` (50 dòng gán)** — nhìn dài nhưng là ánh xạ 1-1 tường minh, dễ debug từng cột. Có thể "rút ngắn" bằng reflection/AutoMapper nhưng sẽ **khó truy lỗi lệch cột hơn** — không đáng.
- **4 parse helper** — đã đúng và cần thiết cho dữ liệu Excel bẩn.
- **Chiến lược 1-query + Dictionary** — đã tối ưu, không N+1.

> ⚠️ Cảnh báo cũ vẫn còn nguyên giá trị: `Request.ExcelRow` khai báo 50 cột theo index, cần đối chiếu với file Nhanh.vn thực tế (62 cột) để tránh map lệch cột — xem `ProductImport_Service_Explained.md`.

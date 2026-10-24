using System.Security.Cryptography;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using Newtonsoft.Json;
using Npgsql;
using VoucherEntity = HoaTheoMua.Repository.Entity.Voucher;

namespace HoaTheoMua.Service.Voucher;

public class Service : IService
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int MaxGenerateAttempts = 5;

    // TEMPORARY: orders have no expiry yet, so a Pending order holds its voucher use only for this window.
    // When an order-expiry job exists (Pending -> Expired), delete this and count Pending orders by status alone.  
    private static readonly TimeSpan PendingHoldWindow = TimeSpan.FromMinutes(15);
    // PrepareOrderVoucher recounts uses from orders only when this many uses or fewer are left
    private const int RecountThreshold = 5;

    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> CreateVoucher(Request.CreateVoucherRequest request)
    {
        var isManualCode = !string.IsNullOrWhiteSpace(request.Code);
        string code;

        if (isManualCode)
        {
            code = request.Code!.Trim().ToUpperInvariant();

            // A deleted voucher frees its code (it is already off the active-code unique index)
            if (await _dbContext.Vouchers.AnyAsync(x => x.Code == code && !x.IsDeleted))
            {
                throw new ConflictException("Mã voucher này đã tồn tại. Vui lòng dùng mã khác.");
            }
        }
        else
        {
            code = await GenerateUniqueCode(request.CodePrefix, request.CodeLength!.Value, request.CodeSuffix);
        }

        var voucher = new VoucherEntity
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = request.Description?.Trim(),
            Visibility = request.Visibility == VoucherVisibilities.Public
                ? VoucherVisibility.Public
                : VoucherVisibility.Private,
            IsActive = false,
            DiscountType = request.DiscountType == VoucherDiscountTypes.Cash
                ? VoucherDiscountType.Cash
                : VoucherDiscountType.Percent,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MaxUsageCount = request.MaxUsageCount,
            UsedCount = 0,
            StartAt = request.StartAt!.Value.ToUniversalTime(),
            EndAt = request.EndAt?.ToUniversalTime(),
            MinOrderAmount = request.MinOrderAmount,
            MaxOrderAmount = request.MaxOrderAmount,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        _dbContext.Vouchers.Add(voucher);
        await _dbContext.SaveChangesAsync();

        return "Create voucher successfully";
    }

    public async Task<BasePaginationResponse> GetVouchers(
        int pageIndex, int pageSize, string? keyword, string? visibility, bool? isActive)
    {
        if (visibility != null && visibility is not (VoucherVisibilities.Public or VoucherVisibilities.Private))
        {
            throw new ValidationExceptionErrorV1(new List<ValidationError>
            {
                new() { Field = "visibility", Message = "Chế độ hiển thị chỉ chấp nhận public (công khai) hoặc private (riêng tư)." }
            });
        }

        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var query = _dbContext.Vouchers
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ProgramId == null); //lấy những voucher ko thuộc chương trình

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();
            var codeKeyword = keyword.ToUpperInvariant();
            query = query.Where(x => x.Code.Contains(codeKeyword)
                                     || (x.Description != null && EF.Functions.ILike(x.Description, $"%{keyword}%")));
        }

        if (visibility != null)
        {
            var visibilityValue = visibility == VoucherVisibilities.Public
                ? VoucherVisibility.Public
                : VoucherVisibility.Private;
            query = query.Where(x => x.Visibility == visibilityValue);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.VoucherResponse
            {
                Id = x.Id,
                // ProgramId = x.ProgramId,
                // ProgramName = x.Program != null ? x.Program.Name : null,
                Code = x.Code,
                Description = x.Description,
                Visibility = x.Visibility == VoucherVisibility.Public
                    ? VoucherVisibilities.Public
                    : VoucherVisibilities.Private,
                IsActive = x.IsActive,
                DiscountType = x.DiscountType == VoucherDiscountType.Cash
                    ? VoucherDiscountTypes.Cash
                    : VoucherDiscountTypes.Percent,
                DiscountValue = x.DiscountValue,
                MaxDiscountAmount = x.MaxDiscountAmount,
                MaxUsageCount = x.MaxUsageCount,
                UsedCount = x.UsedCount,
                StartAt = x.Program != null ? x.Program.StartAt : x.StartAt!.Value,
                EndAt = x.Program != null ? x.Program.EndAt : x.EndAt,
                MinOrderAmount = x.Program != null ? x.Program.MinOrderAmount : x.MinOrderAmount,
                MaxOrderAmount = x.Program != null ? x.Program.MaxOrderAmount : x.MaxOrderAmount,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }
    public async Task<List<Response.FilterVoucherResponse>> FilterVoucher(Request.FilterVoucherRequest request)
    {
        if (request.Products == null || request.Products.Count == 0)
        {
            throw new BadRequestException("Vui lòng chọn ít nhất một sản phẩm.");
        }

        foreach (var item in request.Products)
        {
            if (item.Quantity <= 0)
            {
                throw new BadRequestException("Số lượng sản phẩm phải lớn hơn 0.");
            }
        }

        if (request.OrderTime < 0)
        {
            throw new BadRequestException("Thời gian giao hàng không hợp lệ");
        }

        // if (request.CardTotal < 0)
        // {
        //     throw new BadRequestException("Tiền thiệp không được nhỏ hơn 0.");
        // }

        var ids = request.Products.Select(x => x.Id).Distinct().ToList();
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && ids.Contains(x.Id))
            .Select(x => new { x.Id, x.Price })
            .ToListAsync();

        if (products.Count != ids.Count)
        {
            throw new BadRequestException("Một số sản phẩm không còn tồn tại hoặc đã ngừng bán. Vui lòng kiểm tra lại.");
        }

        if (products.Any(x => x.Price == null))
        {
            throw new BadRequestException("Một số sản phẩm chưa có giá bán. Vui lòng thử lại sau.");
        }

        // Same base as CreateOrder: products plus greeting cards
        // var totalPrice = request.CardTotal ?? 0m;
        var totalPrice = 0m;
        foreach (var item in request.Products)
        {
            var product = products.First(x => x.Id == item.Id);
            totalPrice += product.Price!.Value * item.Quantity;
        }
        
        var now = DateTimeOffset.UtcNow;
        // OrderTime = 0 is urgent delivery, so it is delivered now
        var deliveryAt = request.OrderTime == 0 ? now : DateTimeOffset.FromUnixTimeSeconds(request.OrderTime);

        var vouchers = await _dbContext.Vouchers
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.IsActive // voucher admin đã tắt thì không hiện cho khách
                        && x.Visibility == VoucherVisibility.Public
                        && x.ProgramId == null) //
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Description,
                x.DiscountType,
                x.DiscountValue,
                x.MaxDiscountAmount,
                x.MinOrderAmount,
                x.MaxOrderAmount,
                x.IsActive,
                x.StartAt,
                x.EndAt,
                x.UsedCount,
                x.MaxUsageCount
            })
            .ToListAsync();

        var selectedQuery = vouchers
            .Select(x =>
            {
                // null = no limit
                var isValid = x.IsActive
                              && x.StartAt <= now
                              // && (x.EndAt == null || x.EndAt >= now)
                              && (x.EndAt == null || x.EndAt >= deliveryAt)
                              && x.UsedCount < x.MaxUsageCount
                              && (x.MinOrderAmount == null || totalPrice >= x.MinOrderAmount)
                              && (x.MaxOrderAmount == null || totalPrice <= x.MaxOrderAmount);
                var discountAmount = isValid
                    ? CalculateDiscount(x.DiscountType, x.DiscountValue, x.MaxDiscountAmount, totalPrice)
                    : 0m;

                return new Response.FilterVoucherResponse
                {
                    VoucherId = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    DiscountType = x.DiscountType == VoucherDiscountType.Cash
                        ? VoucherDiscountTypes.Cash
                        : VoucherDiscountTypes.Percent,
                    DiscountValue = x.DiscountValue,
                    MaxDiscountAmount = x.MaxDiscountAmount,
                    MinOrderAmount = x.MinOrderAmount,
                    MaxOrderAmount = x.MaxOrderAmount,
                    DiscountAmount = discountAmount,
                    ProductFeeAfterDiscount = totalPrice - discountAmount,
                    IsValid = isValid
                };
            })
            .OrderByDescending(x => x.IsValid)
            .ThenByDescending(x => x.DiscountAmount);
            
            
            var result = selectedQuery.ToList();

            return result;
    }

    public async Task<Response.VoucherExportFile> ExportVouchers(string? visibility, List<Guid>? programIds)
    {
        var ids = programIds?.Distinct().ToList() ?? new List<Guid>();
        var byProgram = ids.Count > 0;

        if (visibility != null && visibility is not ("all" or VoucherVisibilities.Public or VoucherVisibilities.Private))
        {
            throw CreateValidationException("visibility", "Bộ lọc hiển thị chỉ chấp nhận all (tất cả), public (công khai) hoặc private (riêng tư).");
        }

        if (visibility != null && byProgram)
        {
            throw CreateValidationException("programIds", "Chỉ chọn lọc theo chế độ hiển thị hoặc theo chương trình voucher.");
        }

        if (ids.Count > 50)
        {
            throw CreateValidationException("programIds", "Mỗi lần xuất dữ liệu chỉ được chọn tối đa 50 chương trình voucher.");
        }

        var query = _dbContext.Vouchers
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        IOrderedQueryable<VoucherEntity> ordered;
        string fileKind;

        if (byProgram)
        {
            var foundIds = await _dbContext.VoucherPrograms
                .Where(x => !x.IsDeleted && ids.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var missingIds = ids.Except(foundIds).ToList();
            if (missingIds.Count > 0)
            {
                throw new NotFoundException("Không tìm thấy một hoặc nhiều chương trình voucher đã chọn.");
            }

            ordered = query
                .Where(x => x.ProgramId != null && ids.Contains(x.ProgramId.Value))
                .OrderBy(x => x.Program!.Name)
                .ThenByDescending(x => x.CreatedAt);

            fileKind = ids.Count == 1 ? $"program-{ids[0].ToString()[..8]}" : $"programs-{ids.Count}";
        }
        else
        {
            if (visibility == VoucherVisibilities.Public)
            {
                query = query.Where(x => x.Visibility == VoucherVisibility.Public && x.ProgramId == null);
            }
            else if (visibility == VoucherVisibilities.Private)
            {
                query = query.Where(x => x.Visibility == VoucherVisibility.Private && x.ProgramId == null);
            }
            else
            {
                //all thì export ra public và private ko thuộc chương trình
                query = query.Where(x => x.ProgramId == null);
            }

            ordered = query.OrderByDescending(x => x.CreatedAt);
            fileKind = visibility ?? "all";
        }

        // Program vouchers read dates and order range from the program (effective values).
        // AsEnumerable streams rows from the database one by one while MiniExcel writes them.
        var rows = ordered
            .Select(x => new
            {
                x.Id,
                x.ProgramId,
                ProgramName = x.Program != null ? x.Program.Name : null,
                x.Code,
                x.Description,
                x.Visibility,
                x.IsActive,
                x.DiscountType,
                x.DiscountValue,
                x.MaxDiscountAmount,
                x.MaxUsageCount,
                x.UsedCount,
                StartAt = x.Program != null ? x.Program.StartAt : x.StartAt!.Value,
                EndAt = x.Program != null ? x.Program.EndAt : x.EndAt,
                MinOrderAmount = x.Program != null ? x.Program.MinOrderAmount : x.MinOrderAmount,
                MaxOrderAmount = x.Program != null ? x.Program.MaxOrderAmount : x.MaxOrderAmount,
                x.CreatedAt,
                x.UpdatedAt
            })
            .AsEnumerable()
            .Select(x => new Response.VoucherExportRow
            {
                Id = x.Id.ToString(),
                ProgramId = x.ProgramId?.ToString(),
                ProgramName = x.ProgramName,
                Code = x.Code,
                Description = x.Description,
                Visibility = x.Visibility == VoucherVisibility.Public ? VoucherVisibilities.Public : VoucherVisibilities.Private,
                IsActive = x.IsActive ? "true" : "false",
                DiscountType = x.DiscountType == VoucherDiscountType.Cash ? VoucherDiscountTypes.Cash : VoucherDiscountTypes.Percent,
                DiscountValue = x.DiscountValue,
                MaxDiscountAmount = x.MaxDiscountAmount,
                MaxUsageCount = x.MaxUsageCount,
                UsedCount = x.UsedCount,
                StartAt = FormatExportDate(x.StartAt),
                EndAt = x.EndAt.HasValue ? FormatExportDate(x.EndAt.Value) : null,
                MinOrderAmount = x.MinOrderAmount,
                MaxOrderAmount = x.MaxOrderAmount,
                CreatedAt = FormatExportDate(x.CreatedAt),
                UpdatedAt = x.UpdatedAt.HasValue ? FormatExportDate(x.UpdatedAt.Value) : null,
                IsDeleted = "false"
            });

        // Build the file on disk, not in memory: a large export would otherwise sit in RAM twice
        // (MemoryStream + ToArray). The temp file deletes itself when the response disposes the stream.
        var stream = new FileStream(
            Path.Combine(Path.GetTempPath(), $"voucher-export-{Guid.NewGuid():N}.xlsx"),
            FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, bufferSize: 81920,
            FileOptions.DeleteOnClose | FileOptions.Asynchronous);
        try
        {
            await MiniExcel.SaveAsAsync(stream, rows, sheetName: "Vouchers", excelType: ExcelType.XLSX);
            stream.Position = 0;
        }
        catch
        {
            await stream.DisposeAsync();
            throw;
        }

        return new Response.VoucherExportFile
        {
            Content = stream,
            FileName = $"vouchers-{fileKind}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx"
        };
    }

    public async Task<string> DeleteVoucher(Guid id)
    {
        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (voucher == null)
        {
            throw new NotFoundException("Không tìm thấy voucher.");
        }

        // Xóa voucher thì tắt luôn, để mã không còn giữ chỗ trong unique index
        // và không bị tính vào quota voucher đang hoạt động của chương trình.
        voucher.IsDeleted = true;
        voucher.IsActive = false;
        voucher.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return "Voucher deleted successfully";
    }

    public async Task<BasePaginationResponse> GetVoucherUsages(Guid id, int pageIndex, int pageSize)
    {
        // No IsDeleted filter on purpose: a deleted voucher's paid orders stay visible for reconciliation
        if (!await _dbContext.Vouchers.AnyAsync(x => x.Id == id))
        {
            throw new NotFoundException("Không tìm thấy voucher.");
        }

        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var snapshotFilter = JsonConvert.SerializeObject(new { voucherId = id });

        var query = _dbContext.Orders
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.Status == "Paid"
                        && EF.Functions.JsonContains(x.VoucherSnapshot!, snapshotFilter));

        var totalCount = await query.CountAsync();

        var rows = await query
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.FullName,
                x.Phone,
                x.VoucherSnapshot,
                UsedAt = x.Transactions.Min(t => (DateTimeOffset?)t.CreatedAt)
            })
            // PostgreSQL puts NULL first on DESC; keep orders without a payment time at the bottom
            .OrderBy(x => x.UsedAt == null)
            .ThenByDescending(x => x.UsedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rows
            .Select(x => new Response.VoucherUsageResponse
            {
                OrderId = x.Id,
                UserId = x.UserId,
                CustomerName = x.FullName,
                CustomerPhone = x.Phone,
                DiscountAmount = DeserializeVoucherSnapshot(x.VoucherSnapshot)?.DiscountAmount ?? 0,
                UsedAt = x.UsedAt
            })
            .ToList();

        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }

    public async Task<string> SetVouchersActive(Request.SetVouchersActiveRequest request)
    {
        var isActive = request.IsActive!.Value;

        var vouchers = await _dbContext.Vouchers
            .Include(x => x.Program)
            .Where(x => request.Ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync();

        // Khi bật active, kiểm tra mã trùng trước để báo lỗi rõ ràng
        if (isActive)
        {
            // Chỉ xét voucher đang tắt. Voucher đang bật thì giữ mã của nó rồi.
            var turningOn = vouchers.Where(x => !x.IsActive).ToList();
            var codes = turningOn.Select(x => x.Code).Distinct().ToList();
            var ids = turningOn.Select(x => x.Id).ToList();

            // Trường hợp 1: 2 voucher trong cùng request có cùng mã
            var sameCodeInRequest = turningOn
                .GroupBy(x => x.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            // Trường hợp 2: voucher khác (không nằm trong request) đang bật và dùng mã này
            var codesInUse = await _dbContext.Vouchers
                .AsNoTracking()
                .Where(x => x.IsActive && codes.Contains(x.Code) && !ids.Contains(x.Id))
                .Select(x => x.Code)
                .ToListAsync();

            // Voucher thuộc chương trình đang tắt thì không bật lẻ được
            var inOffProgram = turningOn
                .Where(x => x.Program != null && !x.Program.IsActive)
                .Select(x => x.Code)
                .OrderBy(x => x)
                .ToList();
            if (inOffProgram.Count > 0)
            {
                throw new ConflictException(
                    $"Không thể bật vì chương trình của voucher đang tắt: {string.Join(", ", inOffProgram)}. " +
                    "Hãy bật chương trình trước.");
            }

            var badCodes = sameCodeInRequest.Union(codesInUse).OrderBy(x => x).ToList();
            if (badCodes.Count > 0)
            {
                throw new ConflictException(
                    $"Không thể bật vì mã đang được voucher khác dùng: {string.Join(", ", badCodes)}. " +
                    "Hãy tắt voucher đang dùng mã này trước.");
            }
        }

        foreach (var voucher in vouchers)
        {
            voucher.IsActive = isActive;
            voucher.UpdatedAt = DateTimeOffset.UtcNow;
        }

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_Vouchers_Code"
        })
        {
            // Có người khác vừa bật mã này ngay sau lúc mình kiểm tra
            throw new ConflictException("Không thể bật vì mã voucher bị trùng. Vui lòng thử lại.");
        }

        return "Voucher status updated successfully.";
    }

    // public async Task<List<Response.PublicVoucherResponse>> GetPublicVouchers(decimal? productFee)
    public async Task<List<Response.PublicVoucherResponse>> GetPublicVouchers(decimal? productFee, long orderTime = 0)
    {
        if (orderTime < 0)
        {
            throw new BadRequestException("Thời gian giao hàng không hợp lệ");
        }

        var now = DateTimeOffset.UtcNow;
        // orderTime = 0 is urgent delivery, so it is delivered now
        var deliveryAt = orderTime == 0 ? now : DateTimeOffset.FromUnixTimeSeconds(orderTime);

        var query = _dbContext.Vouchers
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.IsActive
                        && x.Visibility == VoucherVisibility.Public
                        && x.ProgramId == null
                        && x.StartAt <= now
                        // && (x.EndAt == null || x.EndAt >= now)
                        && (x.EndAt == null || x.EndAt >= deliveryAt)
                        && x.UsedCount < x.MaxUsageCount);

        if (productFee.HasValue)
        {
            query = query.Where(x => (x.MinOrderAmount == null || x.MinOrderAmount <= productFee.Value)
                                     && (x.MaxOrderAmount == null || x.MaxOrderAmount >= productFee.Value));
        }

        var vouchers = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Description,
                x.DiscountType,
                x.DiscountValue,
                x.MaxDiscountAmount,
                x.MinOrderAmount,
                x.EndAt
            })
            .ToListAsync();

        return vouchers
            .Select(x => new Response.PublicVoucherResponse
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                DiscountType = x.DiscountType == VoucherDiscountType.Cash
                    ? VoucherDiscountTypes.Cash
                    : VoucherDiscountTypes.Percent,
                DiscountValue = x.DiscountValue,
                MaxDiscountAmount = x.MaxDiscountAmount,
                MinOrderAmount = x.MinOrderAmount,
                EndAt = x.EndAt,
                DiscountAmount = productFee.HasValue
                    ? CalculateDiscount(x.DiscountType, x.DiscountValue, x.MaxDiscountAmount, productFee.Value)
                    : 0
            })
            .ToList();
    }

    public async Task<Response.EstimateVoucherResponse> EstimateVoucher(Request.EstimateVoucherRequest request)
    {
        var voucher = await FindUsableVoucher(request.code!);

        // The UsedCount column may lag; PrepareOrderVoucher re-checks with the counted uses
        // return QuoteVoucher(voucher, request.productFee, voucher.UsedCount);
        // orderTime = 0 is urgent delivery, so it is delivered now
        var deliveryAt = request.orderTime == 0
            ? DateTimeOffset.UtcNow
            : DateTimeOffset.FromUnixTimeSeconds(request.orderTime);
        return QuoteVoucher(voucher, request.productFee, voucher.UsedCount, deliveryAt);
    }

    private async Task<VoucherEntity> FindUsableVoucher(string code)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var voucher = await _dbContext.Vouchers
            .AsNoTracking()
            .Include(x => x.Program)
            .FirstOrDefaultAsync(x => x.Code == normalizedCode
                                      && !x.IsDeleted
                                      && x.IsActive
                                      && (x.Program == null || (!x.Program.IsDeleted && x.Program.IsActive)));

        if (voucher == null)
        {
            throw new NotFoundException("Không tìm thấy voucher.");
        }

        return voucher;
    }

    // private static Response.EstimateVoucherResponse QuoteVoucher(VoucherEntity voucher, decimal productFee, int usedCount)
    private static Response.EstimateVoucherResponse QuoteVoucher(VoucherEntity voucher, decimal productFee, int usedCount,
        DateTimeOffset deliveryAt)
    {
        var startAt = voucher.Program?.StartAt ?? voucher.StartAt;
        var endAt = voucher.Program != null ? voucher.Program.EndAt : voucher.EndAt;
        var minOrderAmount = voucher.Program != null ? voucher.Program.MinOrderAmount : voucher.MinOrderAmount;
        var maxOrderAmount = voucher.Program != null ? voucher.Program.MaxOrderAmount : voucher.MaxOrderAmount;

        var now = DateTimeOffset.UtcNow;

        if (startAt > now)
        {
            throw new BadRequestException("Voucher chưa đến thời gian sử dụng.");
        }

        // if (endAt.HasValue && endAt.Value < now)
        // The end date is checked against the delivery time, not the time the order is placed
        if (endAt.HasValue && endAt.Value < deliveryAt)
        {
            throw new BadRequestException("Voucher đã hết hạn sử dụng.");
        }

        if (usedCount >= voucher.MaxUsageCount)
        {
            throw new ConflictException("Voucher đã hết lượt sử dụng.");
        }

        if (minOrderAmount.HasValue && productFee < minOrderAmount.Value)
        {
            throw new BadRequestException("Giá trị đơn hàng chưa đạt mức tối thiểu để dùng voucher này.");
        }

        if (maxOrderAmount.HasValue && productFee > maxOrderAmount.Value)
        {
            throw new BadRequestException("Giá trị đơn hàng vượt mức tối đa để dùng voucher này.");
        }

        var discountAmount = CalculateDiscount(
            voucher.DiscountType, voucher.DiscountValue, voucher.MaxDiscountAmount, productFee);

        return new Response.EstimateVoucherResponse
        {
            VoucherId = voucher.Id,
            Code = voucher.Code,
            Description = voucher.Description,
            DiscountType = voucher.DiscountType == VoucherDiscountType.Cash
                ? VoucherDiscountTypes.Cash
                : VoucherDiscountTypes.Percent,
            DiscountValue = voucher.DiscountValue,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            DiscountAmount = discountAmount,
            ProductFeeAfterDiscount = productFee - discountAmount
        };
    }

    public async Task<string> UpdateVouchers(Request.UpdateVouchersRequest request)
    {
        var ids = request.Items.Select(x => x.Id).ToList();

        var vouchers = await _dbContext.Vouchers
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync();

        var voucherById = vouchers.ToDictionary(x => x.Id);

        var notFoundIds = ids.Where(id => !voucherById.ContainsKey(id)).ToList();
        if (notFoundIds.Count > 0)
        {
            throw new NotFoundException("Không tìm thấy một hoặc nhiều voucher đã chọn.");
        }

        var errors = new List<ValidationError>();
        for (var i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            var voucher = voucherById[item.Id];

            var isProgramVoucher = voucher.ProgramId != null;

            if (isProgramVoucher && item.Visibility != null && item.Visibility != VoucherVisibilities.Private)
            {
                errors.Add(new ValidationError
                {
                    Field = $"items[{i}]",
                    Message = $"Voucher thuộc chương trình chỉ được để ở chế độ riêng tư ({voucher.Code})."
                });
                continue;
            }

            if (isProgramVoucher
                && (item.StartAt != null || item.EndAt != null || item.MinOrderAmount != null || item.MaxOrderAmount != null))
            {
                errors.Add(new ValidationError
                {
                    Field = $"items[{i}]",
                    Message = $"Ngày áp dụng và mức giá trị đơn hàng của voucher phải chỉnh trong chương trình ({voucher.Code})."
                });
                continue;
            }

            var discountType = voucher.DiscountType;
            if (item.DiscountType != null)
            {
                discountType = item.DiscountType == VoucherDiscountTypes.Cash
                    ? VoucherDiscountType.Cash
                    : VoucherDiscountType.Percent;
            }

            var discountValue = item.DiscountValue ?? voucher.DiscountValue;

            if (discountType == VoucherDiscountType.Percent && discountValue > 100)
            {
                errors.Add(new ValidationError
                {
                    Field = $"items[{i}]",
                    Message = $"Mức giảm theo phần trăm không được vượt quá 100% ({voucher.Code})."
                });
                continue;
            }

            if (discountType != VoucherDiscountType.Percent && item.MaxDiscountAmount.HasValue)
            {
                errors.Add(new ValidationError
                {
                    Field = $"items[{i}]",
                    Message = $"Chỉ đặt mức giảm tối đa khi giảm theo phần trăm ({voucher.Code})."
                });
                continue;
            }

            if (item.MaxUsageCount.HasValue && item.MaxUsageCount.Value < voucher.UsedCount)
            {
                errors.Add(new ValidationError
                {
                    Field = $"items[{i}]",
                    Message = $"Giới hạn lượt sử dụng không được thấp hơn số lượt đã dùng ({voucher.Code})."
                });
                continue;
            }

            if (isProgramVoucher)
            {
                continue;
            }

            var startAt = item.StartAt ?? voucher.StartAt;
            if (item.EndAt.HasValue && item.EndAt.Value <= startAt)
            {
                errors.Add(new ValidationError
                {
                    Field = $"items[{i}]",
                    Message = $"Thời gian kết thúc phải sau thời gian bắt đầu ({voucher.Code})."
                });
                continue;
            }

            // Only a changed end date must be in the future, so an expired voucher can still be edited
            if (item.EndAt.HasValue && item.EndAt != voucher.EndAt && item.EndAt.Value <= DateTimeOffset.UtcNow)
            {
                errors.Add(new ValidationError
                {
                    Field = $"items[{i}]",
                    Message = $"Thời gian kết thúc phải sau thời điểm hiện tại ({voucher.Code})."
                });
                continue;
            }

            if (item.MinOrderAmount.HasValue && item.MaxOrderAmount.HasValue
                && item.MaxOrderAmount <= item.MinOrderAmount)
            {
                errors.Add(new ValidationError
                {
                    Field = $"items[{i}]",
                    Message = $"Giá trị đơn hàng tối đa phải lớn hơn giá trị tối thiểu ({voucher.Code})."
                });
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationExceptionErrorV1(errors);
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var item in request.Items)
        {
            var voucher = voucherById[item.Id];

            if (item.Description != null)
            {
                voucher.Description = item.Description.Trim();
            }

            if (item.Visibility != null)
            {
                voucher.Visibility = item.Visibility == VoucherVisibilities.Public
                    ? VoucherVisibility.Public
                    : VoucherVisibility.Private;
            }

            if (item.DiscountType != null)
            {
                voucher.DiscountType = item.DiscountType == VoucherDiscountTypes.Cash
                    ? VoucherDiscountType.Cash
                    : VoucherDiscountType.Percent;
            }

            if (item.DiscountValue.HasValue)
            {
                voucher.DiscountValue = item.DiscountValue.Value;
            }

            if (item.MaxUsageCount.HasValue)
            {
                voucher.MaxUsageCount = item.MaxUsageCount.Value;
            }

            // New value. Not sent = null. Only a percent discount has a cap
            voucher.MaxDiscountAmount = voucher.DiscountType == VoucherDiscountType.Percent
                ? item.MaxDiscountAmount
                : null;

            // Program voucher takes dates and order amounts from its program, so leave them alone
            if (voucher.ProgramId == null)
            {
                if (item.StartAt.HasValue)
                {
                    voucher.StartAt = item.StartAt.Value.ToUniversalTime();
                }

                // New values. Not sent = null (no limit)
                voucher.EndAt = item.EndAt.HasValue ? item.EndAt.Value.ToUniversalTime() : null;
                voucher.MinOrderAmount = item.MinOrderAmount;
                voucher.MaxOrderAmount = item.MaxOrderAmount;
            }

            voucher.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();

        return "Update voucher successfully";
    }

    // Used by CreateOrder, inside the transaction that saves the order. Same checks and error messages as
    // EstimateVoucher, but "uses left" is counted from orders, and Pending orders still hold their use.
    // public async Task<Order.Request.VoucherSnapshot> PrepareOrderVoucher(string code, decimal productFee)
    public async Task<Order.Request.VoucherSnapshot> PrepareOrderVoucher(string code, decimal productFee,
        DateTimeOffset deliveryAt)
    {
        var voucher = await FindUsableVoucher(code);

        // Lock the voucher row until the caller commits, so two orders cannot both take the last use
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM \"Vouchers\" WHERE \"Id\" = {voucher.Id} FOR UPDATE");

        // var paidCount = await CountUsages(voucher.Id, pendingSince: null);
        // var reservedCount = await CountUsages(voucher.Id, pendingSince: DateTimeOffset.UtcNow - PendingHoldWindow);
        //
        // // The column is for display and only counts Paid orders; refresh it while we have the real number
        // await _dbContext.Vouchers
        //     .Where(x => x.Id == voucher.Id && x.UsedCount != paidCount)
        //     .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UsedCount, paidCount));

        // UsedCount = Paid + FulfillmentFailed uses (the webhook adds 1 per paid order, in the same commit).
        // Read after FOR UPDATE: the voucher from FindUsableVoucher was loaded before the lock.
        var usedCount = await _dbContext.Vouchers
            .AsNoTracking()
            .Where(x => x.Id == voucher.Id)
            .Select(x => x.UsedCount)
            .SingleAsync();
        var reservedCount = usedCount
                            + await CountUsages(voucher.Id, DateTimeOffset.UtcNow - PendingHoldWindow, includeUsed: false);

        // Old rows can be short: the old recount above wrote Paid only, so FulfillmentFailed uses were lost.
        // Near the limit, recount from orders and only raise the column, never lower it.
        if (voucher.MaxUsageCount - reservedCount <= RecountThreshold)
        {
            var countedUsed = await CountUsages(voucher.Id, pendingSince: null, includeUsed: true);
            if (countedUsed > usedCount)
            {
                await _dbContext.Vouchers
                    .Where(x => x.Id == voucher.Id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UsedCount, countedUsed));
                reservedCount += countedUsed - usedCount;
            }
        }

        // var estimate = QuoteVoucher(voucher, productFee, reservedCount);
        var estimate = QuoteVoucher(voucher, productFee, reservedCount, deliveryAt);

        return new Order.Request.VoucherSnapshot
        {
            VoucherId = estimate.VoucherId,
            Code = estimate.Code,
            DiscountType = estimate.DiscountType,
            DiscountValue = estimate.DiscountValue,
            MaxDiscountAmount = estimate.MaxDiscountAmount,
            DiscountAmount = estimate.DiscountAmount,
            ProgramId = voucher.ProgramId
        };
    }

    // // Uses = Paid orders whose snapshot holds this voucher id, plus Pending ones created after pendingSince
    // private async Task<int> CountUsages(Guid voucherId, DateTimeOffset? pendingSince)
    // {
    //     var snapshotFilter = JsonConvert.SerializeObject(new { voucherId });
    //
    //     return await _dbContext.Orders
    //         .AsNoTracking()
    //         .CountAsync(x => !x.IsDeleted
    //                          && (x.Status == "Paid"
    //                              || (pendingSince != null && x.Status == "Pending" && x.CreatedAt > pendingSince))
    //                          && EF.Functions.JsonContains(x.VoucherSnapshot, snapshotFilter));
    // }

    // includeUsed = true: Paid + FulfillmentFailed orders (money taken, the use is kept).
    // includeUsed = false: only Pending orders created after pendingSince (uses still being held).
    private async Task<int> CountUsages(Guid voucherId, DateTimeOffset? pendingSince, bool includeUsed)
    {
        var snapshotFilter = JsonConvert.SerializeObject(new { voucherId });

        return await _dbContext.Orders
            .AsNoTracking()
            .CountAsync(x => !x.IsDeleted
                             && ((includeUsed && (x.Status == "Paid" || x.Status == "FulfillmentFailed"))
                                 || (!includeUsed && pendingSince != null && x.Status == "Pending" && x.CreatedAt > pendingSince))
                             && EF.Functions.JsonContains(x.VoucherSnapshot, snapshotFilter));
    }

    private async Task<string> GenerateUniqueCode(string? prefix, int length, string? suffix)
    {
        for (var attempt = 1; attempt <= MaxGenerateAttempts; attempt++)
        {
            var code = GenerateCode(prefix, length, suffix);

            if (!await _dbContext.Vouchers.AnyAsync(x => x.Code == code && !x.IsDeleted))
            {
                return code;
            }
        }

        throw new ServerException("Không thể tạo mã voucher lúc này. Vui lòng thử lại sau.");
    }

    private static string GenerateCode(string? prefix, int length, string? suffix)
    {
        var random = RandomNumberGenerator.GetString(CodeAlphabet, length);
        return $"{prefix?.Trim().ToUpperInvariant()}{random}{suffix?.Trim().ToUpperInvariant()}";
    }

    
    private static decimal CalculateDiscount(
        VoucherDiscountType discountType, decimal discountValue, decimal? maxDiscountAmount, decimal productFee)
    {
        if (productFee <= 0)
        {
            return 0;
        }

        var discount = discountType == VoucherDiscountType.Cash
            ? discountValue
            : Math.Floor(productFee * discountValue / 100);

        if (discountType == VoucherDiscountType.Percent && maxDiscountAmount.HasValue)
        {
            discount = Math.Min(discount, maxDiscountAmount.Value);
        }

        return Math.Min(discount, productFee);
    }
    
    private static Order.Request.VoucherSnapshot? DeserializeVoucherSnapshot(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
            return null;
        
        try
        {
            return JsonConvert.DeserializeObject<Order.Request.VoucherSnapshot>(metadata);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string FormatExportDate(DateTimeOffset value)
        => value.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz");

    private static ValidationExceptionErrorV1 CreateValidationException(string field, string message)
        => new(new List<ValidationError> { new() { Field = field, Message = message } });
}

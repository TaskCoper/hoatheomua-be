using System.Security.Cryptography;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using Newtonsoft.Json;
using VoucherEntity = HoaTheoMua.Repository.Entity.Voucher;

namespace HoaTheoMua.Service.Voucher;

public class Service : IService
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int MaxGenerateAttempts = 5;

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

            if (await _dbContext.Vouchers.AnyAsync(x => x.Code == code))
            {
                throw new ConflictException("Voucher code already exists.");
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
        int pageIndex, int pageSize, string? keyword, string? visibility, bool? isActive, Guid? programId)
    {
        if (visibility != null && visibility is not (VoucherVisibilities.Public or VoucherVisibilities.Private))
        {
            throw new ValidationExceptionErrorV1(new List<ValidationError>
            {
                new() { Field = "visibility", Message = "Visibility must be public or private" }
            });
        }

        var query = _dbContext.Vouchers
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

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

        if (programId.HasValue)
        {
            query = query.Where(x => x.ProgramId == programId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.VoucherResponse
            {
                Id = x.Id,
                ProgramId = x.ProgramId,
                ProgramName = x.Program != null ? x.Program.Name : null,
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
    
    public async Task<Response.VoucherExportFile> ExportVouchers(string? visibility, List<Guid>? programIds)
    {
        var ids = programIds?.Distinct().ToList() ?? new List<Guid>();
        var byProgram = ids.Count > 0;

        if (visibility != null && visibility is not ("all" or VoucherVisibilities.Public or VoucherVisibilities.Private))
        {
            throw CreateValidationException("visibility", "Visibility must be all, public or private");
        }

        if (visibility != null && byProgram)
        {
            throw CreateValidationException("programIds", "Choose either visibility or programIds");
        }

        if (ids.Count > 50)
        {
            throw CreateValidationException("programIds", "Program ids must not exceed 50");
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
                throw new NotFoundException($"Voucher program not found: {string.Join(", ", missingIds)}");
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
                query = query.Where(x => x.Visibility == VoucherVisibility.Public);
            }
            else if (visibility == VoucherVisibilities.Private)
            {
                query = query.Where(x => x.Visibility == VoucherVisibility.Private);
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

        using var stream = new MemoryStream();
        await MiniExcel.SaveAsAsync(stream, rows, sheetName: "Vouchers", excelType: ExcelType.XLSX);

        return new Response.VoucherExportFile
        {
            Content = stream.ToArray(),
            FileName = $"vouchers-{fileKind}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx"
        };
    }

    public async Task<string> DeleteVoucher(Guid id)
    {
        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (voucher == null)
        {
            throw new NotFoundException("Voucher not found.");
        }

        voucher.IsDeleted = true;
        voucher.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return "Voucher deleted successfully";
    }

    public async Task<BasePaginationResponse> GetVoucherUsages(Guid id, int pageIndex, int pageSize)
    {
        if (!await _dbContext.Vouchers.AnyAsync(x => x.Id == id))
        {
            throw new NotFoundException("Voucher not found.");
        }

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
            .OrderByDescending(x => x.UsedAt)
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

        foreach (var voucher in vouchers)
        {
            voucher.IsActive = isActive;
            voucher.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return "Voucher status updated successfully.";
    }

    public async Task<List<Response.PublicVoucherResponse>> GetPublicVouchers(decimal? productFee)
    {
        var now = DateTimeOffset.UtcNow;

        var query = _dbContext.Vouchers
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.IsActive
                        && x.Visibility == VoucherVisibility.Public
                        && x.ProgramId == null
                        && x.StartAt <= now
                        && (x.EndAt == null || x.EndAt >= now)
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
        var code = request.code!.Trim().ToUpperInvariant();

        var voucher = await _dbContext.Vouchers
            .AsNoTracking()
            .Include(x => x.Program)
            .FirstOrDefaultAsync(x => x.Code == code
                                      && !x.IsDeleted
                                      && x.IsActive
                                      && (x.Program == null || !x.Program.IsDeleted));

        if (voucher == null)
        {
            throw new NotFoundException("Voucher not found.");
        }

        var startAt = voucher.Program?.StartAt ?? voucher.StartAt;
        var endAt = voucher.Program != null ? voucher.Program.EndAt : voucher.EndAt;
        var minOrderAmount = voucher.Program != null ? voucher.Program.MinOrderAmount : voucher.MinOrderAmount;
        var maxOrderAmount = voucher.Program != null ? voucher.Program.MaxOrderAmount : voucher.MaxOrderAmount;

        var now = DateTimeOffset.UtcNow;

        if (startAt > now)
        {
            throw new BadRequestException("Voucher is not active yet.");
        }

        if (endAt.HasValue && endAt.Value < now)
        {
            throw new BadRequestException("Voucher has expired.");
        }

        if (voucher.UsedCount >= voucher.MaxUsageCount)
        {
            throw new ConflictException("Voucher has no uses left.");
        }

        if (minOrderAmount.HasValue && request.productFee < minOrderAmount.Value)
        {
            throw new BadRequestException("Order total is below the minimum for this voucher.");
        }

        if (maxOrderAmount.HasValue && request.productFee > maxOrderAmount.Value)
        {
            throw new BadRequestException("Order total exceeds the maximum for this voucher.");
        }

        var discountAmount = CalculateDiscount(
            voucher.DiscountType, voucher.DiscountValue, voucher.MaxDiscountAmount, request.productFee);

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
            ProductFeeAfterDiscount = request.productFee - discountAmount
        };
    }

    private async Task<string> GenerateUniqueCode(string? prefix, int length, string? suffix)
    {
        for (var attempt = 1; attempt <= MaxGenerateAttempts; attempt++)
        {
            var code = GenerateCode(prefix, length, suffix);

            if (!await _dbContext.Vouchers.AnyAsync(x => x.Code == code))
            {
                return code;
            }
        }

        throw new ServerException("An unexpected error occurred.");
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
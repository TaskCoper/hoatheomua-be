using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Npgsql;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;
using VoucherProgramEntity = HoaTheoMua.Repository.Entity.VoucherProgram;

namespace HoaTheoMua.Service.VoucherProgram;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IVoucherCodeGenerator _codeGenerator;

    public Service(AppDbContext dbContext, IVoucherCodeGenerator? codeGenerator = null)
    {
        _dbContext = dbContext;
        _codeGenerator = codeGenerator ?? new VoucherCodeGenerator();
    }

    public async Task<Response.VoucherProgramResponse> CreateVoucherProgram(
        Request.CreateVoucherProgramRequest request, CancellationToken cancellationToken = default)
    {
        var program = new VoucherProgramEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            IsActive = false,
            StartAt = VoucherProgramDateTime.Parse(request.StartAt).ToUniversalTime(),
            EndAt = VoucherProgramDateTime.Parse(request.EndAt).ToUniversalTime(),
            MinOrderAmount = request.MinOrderAmount,
            MaxOrderAmount = request.MaxOrderAmount == 0m ? null : request.MaxOrderAmount,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        _dbContext.VoucherPrograms.Add(program);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(program);
    }

    public async Task<BasePaginationResponse> GetVoucherPrograms(
        Request.GetVoucherProgramsRequest request, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(1, request.pageIndex);
        var pageSize = request.pageSize > 0 ? Math.Min(request.pageSize, 100) : 20;
        var query = _dbContext.VoucherPrograms.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.keyword))
        {
            var keyword = request.keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(keyword));
        }

        if (request.isActive.HasValue)
            query = query.Where(x => x.IsActive == request.isActive.Value);

        if (request.isDelete.HasValue)
            query = query.Where(x => x.IsDeleted == request.isDelete.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (int)Math.Min((long)(pageIndex - 1) * pageSize, int.MaxValue);
        var items = offset >= totalCount ? new List<Response.VoucherProgramResponse>()
            : await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip(offset).Take(pageSize)
            .Select(x => new Response.VoucherProgramResponse
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                MinOrderAmount = x.MinOrderAmount,
                MaxOrderAmount = x.MaxOrderAmount,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync(cancellationToken);

        if (items.Count > 0)
        {
            // Aggregate both counters together, restricted to programs on this page.
            var programIds = items.Select(x => x.Id).ToArray();
            var counts = await _dbContext.Vouchers.AsNoTracking()
                .Where(x => x.ProgramId.HasValue && programIds.Contains(x.ProgramId.Value) && !x.IsDeleted)
                .GroupBy(x => x.ProgramId!.Value)
                .Select(group => new
                {
                    ProgramId = group.Key,
                    VoucherCount = group.Count(),
                    UsedVoucherCount = group.Count(x => x.UsedCount > 0)
                }).ToDictionaryAsync(x => x.ProgramId, cancellationToken);

            foreach (var item in items)
            {
                if (counts.TryGetValue(item.Id, out var count))
                {
                    item.VoucherCount = count.VoucherCount;
                    item.UsedVoucherCount = count.UsedVoucherCount;
                }
            }
        }

        var response = ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
        response.Value.HasNextPage = (long)pageIndex * pageSize < totalCount;
        return response;
    }

    public async Task<Response.VoucherProgramResponse> UpdateVoucherProgram(Guid id, Request.UpdateVoucherProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var program = await GetProgramForUpdate(id, cancellationToken);
        var startAt = request.StartAt == null ? program.StartAt
            : VoucherProgramDateTime.Parse(request.StartAt).ToUniversalTime();
        var endAt = request.EndAt == null ? program.EndAt
            : VoucherProgramDateTime.Parse(request.EndAt).ToUniversalTime();
        var minOrderAmount = request.MinOrderAmount ?? program.MinOrderAmount;
        var maxOrderAmount = request.MaxOrderAmount == 0m ? null : request.MaxOrderAmount ?? program.MaxOrderAmount;
        var errors = new List<ValidationError>();
        if (endAt <= startAt)
        {
            errors.Add(request.StartAt != null && request.EndAt == null
                ? new ValidationError { Field = "startAt", Message = "Start time must be earlier than the current end time" }
                : new ValidationError
                {
                    Field = "endAt",
                    Message = request.StartAt != null ? "End time must be later than start time"
                        : "End time must be later than the current start time"
                });
        }
        if (minOrderAmount.HasValue && maxOrderAmount.HasValue && maxOrderAmount < minOrderAmount)
        {
            errors.Add(request.MinOrderAmount.HasValue && !request.MaxOrderAmount.HasValue
                ? new ValidationError { Field = "minOrderAmount", Message = "Min order amount cannot exceed the current max order amount" }
                : new ValidationError
                {
                    Field = "maxOrderAmount",
                    Message = request.MinOrderAmount.HasValue ? "Max order amount cannot be less than min order amount"
                        : "Max order amount cannot be less than the current min order amount"
                });
        }
        if (errors.Count > 0)
            throw new ValidationExceptionErrorV1(errors);

        var now = DateTimeOffset.UtcNow;
        if (request.IsActive.HasValue)
        {
            var isActive = request.IsActive.Value;
            var vouchers = _dbContext.Vouchers.Where(x => x.ProgramId == id && !x.IsDeleted);
            await vouchers.ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsActive, isActive)
                .SetProperty(x => x.UpdatedAt, now), cancellationToken);

            program.IsActive = isActive;
        }

        if (request.Name != null)
            program.Name = request.Name.Trim();
        program.StartAt = startAt;
        program.EndAt = endAt;
        program.MinOrderAmount = minOrderAmount;
        program.MaxOrderAmount = maxOrderAmount;
        program.UpdatedAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);
        // The program is already loaded and updated. Read only the voucher statistics in one scan.
        var counts = await _dbContext.Vouchers.AsNoTracking()
            .Where(x => x.ProgramId == id && !x.IsDeleted)
            .GroupBy(x => x.ProgramId)
            .Select(group => new
            {
                VoucherCount = group.Count(),
                UsedVoucherCount = group.Count(x => x.UsedCount > 0)
            }).SingleOrDefaultAsync(cancellationToken);
        var result = ToResponse(program, counts?.VoucherCount ?? 0, counts?.UsedVoucherCount ?? 0);
        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    public async Task<string> DeleteVoucherProgram(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var program = await GetProgramForUpdate(id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var vouchers = _dbContext.Vouchers.Where(x => x.ProgramId == id);

        await vouchers.ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.IsDeleted, true)
            .SetProperty(x => x.UpdatedAt, now), cancellationToken);

        program.IsDeleted = true;
        program.UpdatedAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return "Program deleted";
    }

    public async Task<Response.GenerateProgramVouchersResponse> GenerateProgramVouchers(
        Guid id, Request.GenerateProgramVouchersRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        // All three mutations lock the same row, serializing generation, activation and deletion.
        await GetProgramForUpdate(id, cancellationToken);
        var count = await _dbContext.Vouchers.CountAsync(x => x.ProgramId == id, cancellationToken);
        if ((long)count + request.Quantity > 10000)
            throw new ConflictException("Voucher program cannot exceed 10000 vouchers.");

        var codes = new List<string>(request.Quantity);
        var excludedCodes = new HashSet<string>(StringComparer.Ordinal);
        var now = DateTimeOffset.UtcNow;
        var saved = false;
        var discountType = request.DiscountType == "cash" ? VoucherDiscountType.Cash : VoucherDiscountType.Percent;

        for (var round = 0; round < 5; round++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var missing = request.Quantity - codes.Count;
            var candidateCount = (int)Math.Ceiling(missing * 1.1);
            var candidates = new HashSet<string>(candidateCount, StringComparer.Ordinal);
            for (var i = 0; i < candidateCount; i++)
            {
                var code = _codeGenerator.Generate(request.CodePrefix, request.CodeLength, request.CodeSuffix);
                if (!excludedCodes.Contains(code))
                    candidates.Add(code);
            }

            if (candidates.Count > 0)
            {
                // Soft-deleted and standalone vouchers also reserve their codes permanently.
                var candidateCodes = candidates.ToArray();
                var existingCodes = await _dbContext.Vouchers.AsNoTracking()
                    .Where(x => candidateCodes.Contains(x.Code))
                    .Select(x => x.Code)
                    .ToListAsync(cancellationToken);
                excludedCodes.UnionWith(existingCodes);
                foreach (var code in candidates)
                {
                    // Reuse the exclusion set instead of allocating another set and selected array.
                    if (excludedCodes.Add(code))
                        codes.Add(code);
                    if (codes.Count == request.Quantity)
                        break;
                }
            }

            // Stage the complete quantity before writing; usually only one SaveChanges is needed.
            if (codes.Count != request.Quantity)
                continue;
            var vouchers = codes.Select(code => new Voucher
            {
                Id = Guid.NewGuid(), ProgramId = id, Code = code,
                Description = request.Description,
                DiscountType = discountType,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                Visibility = VoucherVisibility.Private,
                MaxUsageCount = request.MaxUsageCount,
                UsedCount = 0, IsActive = false, IsDeleted = false, CreatedAt = now
            }).ToList();

            try
            {
                _dbContext.Vouchers.AddRange(vouchers);
                // EF creates and rolls back its own savepoint inside the existing PostgreSQL transaction.
                await _dbContext.SaveChangesAsync(cancellationToken);
                saved = true;
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Vouchers_Code"
            })
            {
                // Replace conflicting codes below, then retry in the next round.
            }
            finally
            {
                // Detach only this batch, including on unexpected errors or cancellation.
                foreach (var voucher in vouchers)
                    _dbContext.Entry(voucher).State = EntityState.Detached;
            }

            if (saved)
                break;

            // A concurrent request claimed a code after the existence check. Keep the other codes
            // and generate replacements in the next round; EF rolled back the failed savepoint.
            var attemptedCodes = codes.ToArray();
            var conflicts = (await _dbContext.Vouchers.AsNoTracking()
                .Where(x => attemptedCodes.Contains(x.Code))
                .Select(x => x.Code)
                .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.Ordinal);
            codes.RemoveAll(conflicts.Contains);
        }

        if (!saved)
            throw new ServerException("Unable to generate enough unique voucher codes after 5 rounds.");

        await transaction.CommitAsync(cancellationToken);

        return new Response.GenerateProgramVouchersResponse
        {
            ProgramId = id, CreatedCount = codes.Count, Codes = codes
        };
    }

    private async Task<VoucherProgramEntity> GetProgramForUpdate(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.VoucherPrograms.Where(x => x.Id == id);

        // All callers hold a transaction. A no-op update acquires a row write lock
        // until commit, serializing mutations without changing the business timestamp.
        var affected = await query.ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.UpdatedAt, x => x.UpdatedAt), cancellationToken);
        if (affected == 0)
            throw new NotFoundException("Voucher program does not exist.");

        var program = await query.SingleOrDefaultAsync(cancellationToken)
                      ?? throw new NotFoundException("Voucher program does not exist.");
        if (program.IsDeleted)
            throw new NotFoundException("Voucher program has been deleted.");

        return program;
    }

    private static Response.VoucherProgramResponse ToResponse(VoucherProgramEntity program,
        int voucherCount = 0, int usedVoucherCount = 0)
    {
        return new Response.VoucherProgramResponse
        {
            Id = program.Id, Name = program.Name, IsActive = program.IsActive, IsDeleted = program.IsDeleted,
            StartAt = program.StartAt, EndAt = program.EndAt,
            MinOrderAmount = program.MinOrderAmount, MaxOrderAmount = program.MaxOrderAmount,
            VoucherCount = voucherCount, UsedVoucherCount = usedVoucherCount,
            CreatedAt = program.CreatedAt, UpdatedAt = program.UpdatedAt
        };
    }

}

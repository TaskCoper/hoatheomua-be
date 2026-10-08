using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;
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

    public async Task<string> CreateVoucherProgram(
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

        // return new Response.VoucherProgramResponse
        // {
        //     Id = program.Id,
        //     Name = program.Name,
        //     IsActive = program.IsActive,
        //     IsDeleted = program.IsDeleted,
        //     StartAt = program.StartAt,
        //     EndAt = program.EndAt,
        //     MinOrderAmount = program.MinOrderAmount,
        //     MaxOrderAmount = program.MaxOrderAmount,
        //     VoucherCount = 0,
        //     UsedVoucherCount = 0,
        //     CreatedAt = program.CreatedAt,
        //     UpdatedAt = program.UpdatedAt
        // };
        return "Create voucher program successfully";
    }

    public async Task<BasePaginationResponse> GetVoucherPrograms(
        Request.GetVoucherProgramsRequest request, CancellationToken cancellationToken = default)
    {
        var pageIndex = request.pageIndex <= 0 ? 1 : request.pageIndex;
        var pageSize = request.pageSize <= 0 ? 10 : Math.Min(request.pageSize, 100);
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
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
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
                VoucherCount = x.Vouchers.Count(v => !v.IsDeleted),
                // P2 reads the cached count; each used code counts once, regardless of its uses.
                UsedVoucherCount = x.Vouchers.Count(v => !v.IsDeleted && v.UsedCount > 0),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync(cancellationToken);

        var response = ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
        response.Value.HasNextPage = (long)pageIndex * pageSize < totalCount;
        return response;
    }

    public async Task<BasePaginationResponse> GetVoucherProgramsWithVouchers(
        int pageIndex, int pageSize, string? keyword, CancellationToken cancellationToken = default)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var query = _dbContext.VoucherPrograms
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var name = keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (int)Math.Min((long)(pageIndex - 1) * pageSize, int.MaxValue);

        var programs = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(offset)
            .Take(pageSize)
            .Select(x => new Response.VoucherProgramDetailResponse
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                MinOrderAmount = x.MinOrderAmount,
                MaxOrderAmount = x.MaxOrderAmount,
                VoucherCount = x.Vouchers.Count(v => !v.IsDeleted),
                UsedVoucherCount = x.Vouchers.Count(v => !v.IsDeleted && v.UsedCount > 0),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        // One query for every voucher of the programs on this page (not N queries), then group by program
        var programIds = programs.Select(x => x.Id).ToList();
        var vouchers = await _dbContext.Vouchers
            .AsNoTracking()
            .Where(x => x.ProgramId != null && programIds.Contains(x.ProgramId.Value) && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Code)
            .Select(x => new
            {
                ProgramId = x.ProgramId!.Value,
                Item = new Response.ProgramVoucherItem
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    DiscountType = x.DiscountType == VoucherDiscountType.Cash
                        ? Voucher.VoucherDiscountTypes.Cash
                        : Voucher.VoucherDiscountTypes.Percent,
                    DiscountValue = x.DiscountValue,
                    MaxDiscountAmount = x.MaxDiscountAmount,
                    MaxUsageCount = x.MaxUsageCount,
                    UsedCount = x.UsedCount,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }
            })
            .ToListAsync(cancellationToken);

        foreach (var program in programs)
        {
            program.Vouchers = vouchers
                .Where(x => x.ProgramId == program.Id)
                .Select(x => x.Item)
                .ToList();
        }

        var response = ApiResponseFactory.BasePagination(programs, pageIndex, pageSize, totalCount);
        response.Value.HasNextPage = (long)pageIndex * pageSize < totalCount;
        return response;
    }

    public async Task<Response.VoucherProgramResponse> UpdateVoucherProgram(Guid id, Request.UpdateVoucherProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = _dbContext.Database.IsRelational()
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
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
            if (_dbContext.Database.IsRelational())
            {
                await vouchers.ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsActive, isActive)
                    .SetProperty(x => x.UpdatedAt, now), cancellationToken);
            }
            else
            {
                foreach (var voucher in await vouchers.ToListAsync(cancellationToken))
                {
                    voucher.IsActive = isActive;
                    voucher.UpdatedAt = now;
                }
            }

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
        var result = await _dbContext.VoucherPrograms.AsNoTracking().Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new Response.VoucherProgramResponse
            {
                Id = x.Id, Name = x.Name, IsActive = x.IsActive, IsDeleted = x.IsDeleted,
                StartAt = x.StartAt, EndAt = x.EndAt,
                MinOrderAmount = x.MinOrderAmount, MaxOrderAmount = x.MaxOrderAmount,
                VoucherCount = x.Vouchers.Count(v => !v.IsDeleted),
                UsedVoucherCount = x.Vouchers.Count(v => !v.IsDeleted && v.UsedCount > 0),
                CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
            }).SingleAsync(cancellationToken);
        if (transaction != null)
            await transaction.CommitAsync(cancellationToken);

        return result;
    }

    public async Task<string> DeleteVoucherProgram(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = _dbContext.Database.IsRelational()
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
        var program = await GetProgramForUpdate(id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var vouchers = _dbContext.Vouchers.Where(x => x.ProgramId == id && !x.IsDeleted);

        if (_dbContext.Database.IsRelational())
        {
            await vouchers.ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.UpdatedAt, now), cancellationToken);
        }
        else
        {
            foreach (var voucher in await vouchers.ToListAsync(cancellationToken))
            {
                voucher.IsDeleted = true;
                voucher.UpdatedAt = now;
            }
        }

        program.IsDeleted = true;
        program.UpdatedAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);
        if (transaction != null)
            await transaction.CommitAsync(cancellationToken);

        return "Program deleted";
    }

    public async Task<Response.GenerateProgramVouchersResponse> GenerateProgramVouchers(
        Guid id, Request.GenerateProgramVouchersRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = _dbContext.Database.IsRelational()
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
        // All three mutations lock the same row, serializing generation, activation and deletion.
        await GetProgramForUpdate(id, cancellationToken);
        var count = await _dbContext.Vouchers.CountAsync(x => x.ProgramId == id, cancellationToken);
        if ((long)count + request.Quantity > 10000)
            throw new ConflictException("Voucher program cannot exceed 10000 vouchers.");

        var codes = new List<string>(request.Quantity);
        var issued = new HashSet<string>(StringComparer.Ordinal);
        var now = DateTimeOffset.UtcNow;
        // InMemory is used by unit tests. Production relies solely on the database's unique index.
        HashSet<string>? existingCodes = _dbContext.Database.IsRelational() ? null
            : (await _dbContext.Vouchers.Select(x => x.Code).ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.Ordinal);

        for (var round = 0; round < 5 && codes.Count < request.Quantity; round++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var missing = request.Quantity - codes.Count;
            var candidates = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < (int)Math.Ceiling(missing * 1.1); i++)
            {
                var code = _codeGenerator.Generate(request.CodePrefix, request.CodeLength, request.CodeSuffix);
                if (!issued.Contains(code))
                    candidates.Add(code);
            }

            var pending = candidates.ToArray();
            for (var offset = 0; offset < pending.Length && codes.Count < request.Quantity;)
            {
                // Never insert the surplus once the requested quantity has been reached.
                var batch = pending.Skip(offset).Take(request.Quantity - codes.Count).ToArray();
                offset += batch.Length;
                var inserted = existingCodes == null
                    ? await InsertVoucherBatch(id, request, batch, now, cancellationToken)
                    : batch.Where(code => existingCodes.Add(code)).ToList();
                codes.AddRange(inserted);
                issued.UnionWith(inserted);
            }
        }

        if (codes.Count != request.Quantity)
            throw new ServerException("Unable to generate enough unique voucher codes after 5 rounds.");

        if (existingCodes != null)
        {
            // Defer persistence until the entire batch is complete; failed test-provider batches stay empty.
            _dbContext.Vouchers.AddRange(codes.Select(code => new Repository.Entity.Voucher
            {
                Id = Guid.NewGuid(), ProgramId = id, Code = code,
                Description = request.Description,
                DiscountType = request.DiscountType == "cash" ? VoucherDiscountType.Cash : VoucherDiscountType.Percent,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                Visibility = VoucherVisibility.Private,
                MaxUsageCount = request.MaxUsageCount,
                IsActive = false, UsedCount = 0, IsDeleted = false, CreatedAt = now
            }));
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (transaction != null)
            await transaction.CommitAsync(cancellationToken);

        return new Response.GenerateProgramVouchersResponse
        {
            ProgramId = id, CreatedCount = codes.Count, Codes = codes
        };
    }

    private async Task<VoucherProgramEntity> GetProgramForUpdate(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.Database.IsRelational()
            ? _dbContext.VoucherPrograms.FromSqlInterpolated(
                $"SELECT * FROM \"VoucherPrograms\" WHERE \"Id\" = {id} FOR UPDATE")
            : _dbContext.VoucherPrograms.Where(x => x.Id == id);

        var program = await query.SingleOrDefaultAsync(cancellationToken)
                      ?? throw new NotFoundException("Voucher program does not exist.");
        if (program.IsDeleted)
            throw new NotFoundException("Voucher program has been deleted.");

        return program;
    }

    private async Task<List<string>> InsertVoucherBatch(Guid programId, Request.GenerateProgramVouchersRequest request,
        string[] codes, DateTimeOffset createdAt, CancellationToken cancellationToken)
    {
        // Loại các mã đã có (thay cho ON CONFLICT DO NOTHING). Mã của voucher đã xóa mềm không tính.
        var taken = await _dbContext.Vouchers.AsNoTracking()
            .Where(x => !x.IsDeleted && codes.Contains(x.Code))
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        var free = codes.Except(taken).ToArray();
        if (free.Length == 0)
            return [];

        // Giữ nguyên cách map cũ: "cash" là Cash, còn lại là Percent
        var discountType = request.DiscountType == "cash"
            ? VoucherDiscountType.Cash
            : VoucherDiscountType.Percent;

        var vouchers = free.Select(code => new Repository.Entity.Voucher()
        {
            Id = Guid.NewGuid(),
            ProgramId = programId,
            Code = code,
            Description = request.Description,
            DiscountType = discountType,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            Visibility = VoucherVisibility.Private,
            MaxUsageCount = request.MaxUsageCount,
            IsActive = false,
            CreatedAt = createdAt,
            UpdatedAt = null,
            IsDeleted = false
            // StartAt, EndAt, MinOrderAmount, MaxOrderAmount để null: lấy từ chương trình
        }).ToList();

        _dbContext.Vouchers.AddRange(vouchers);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return vouchers.Select(v => v.Code).ToList();
    }

}

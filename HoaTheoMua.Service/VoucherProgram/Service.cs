using HoaTheoMua.Repository;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;
using VoucherProgramEntity = HoaTheoMua.Repository.Entity.VoucherProgram;

namespace HoaTheoMua.Service.VoucherProgram;

public class                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Response.VoucherProgramResponse> CreateVoucherProgram(
        Request.CreateVoucherProgramRequest request, CancellationToken cancellationToken = default)
    {
        var program = new VoucherProgramEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            IsActive = false,
            StartAt = request.StartAt.ToUniversalTime(),
            EndAt = request.EndAt.ToUniversalTime(),
            MinOrderAmount = request.MinOrderAmount,
            MaxOrderAmount = request.MaxOrderAmount,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        _dbContext.VoucherPrograms.Add(program);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new Response.VoucherProgramResponse
        {
            Id = program.Id,
            Name = program.Name,
            IsActive = program.IsActive,
            StartAt = program.StartAt,
            EndAt = program.EndAt,
            MinOrderAmount = program.MinOrderAmount,
            MaxOrderAmount = program.MaxOrderAmount,
            VoucherCount = 0,
            UsedVoucherCount = 0,
            CreatedAt = program.CreatedAt,
            UpdatedAt = program.UpdatedAt
        };
    }

    public async Task<BasePaginationResponse> GetVoucherPrograms(
        Request.GetVoucherProgramsRequest request, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(1, request.PageIndex);
        var pageSize = request.PageSize > 0 ? request.PageSize : 20;
        var query = _dbContext.VoucherPrograms.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(keyword));
        }

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (int)Math.Min((long)(pageIndex - 1) * pageSize, int.MaxValue);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip(offset).Take(pageSize)
            .Select(x => new Response.VoucherProgramResponse
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
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
}

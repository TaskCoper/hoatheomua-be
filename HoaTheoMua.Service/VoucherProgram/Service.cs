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

    public async Task<string> CreateVoucherProgram(
        Request.CreateVoucherProgramRequest request, CancellationToken cancellationToken = default)
    {
        var program = new VoucherProgramEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            IsActive = false,
            StartAt = VoucherProgramDateTime.Parse(request.StartAt).ToUniversalTime(),
            EndAt = string.IsNullOrWhiteSpace(request.EndAt) ? null
                : VoucherProgramDateTime.Parse(request.EndAt).ToUniversalTime(),
            MinOrderAmount = request.MinOrderAmount,
            MaxOrderAmount = request.MaxOrderAmount == 0m ? null : request.MaxOrderAmount,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        _dbContext.VoucherPrograms.Add(program);
        await _dbContext.SaveChangesAsync(cancellationToken);

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
                VoucherCount = x.Vouchers.Count(v => !v.IsDeleted),
                UsedVoucherCount = x.Vouchers.Count(v => !v.IsDeleted && v.UsedCount > 0),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync(cancellationToken);

        var response = ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
        response.Value.HasNextPage = (long)pageIndex * pageSize < totalCount;
        return response;
    }

    public async Task<BasePaginationResponse> GetVoucherProgramsWithVouchers(
        int pageIndex, int pageSize, string? keyword, Guid? programId, CancellationToken cancellationToken = default)
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
        
        if (programId.HasValue)
        {
            query = query.Where(x => x.Id == programId.Value);
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

    public async Task<string> UpdateVoucherProgram(Guid id, Request.UpdateVoucherProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var program = await GetProgramForUpdate(id, cancellationToken);
        var startAt = request.StartAt == null ? program.StartAt
            : VoucherProgramDateTime.Parse(request.StartAt).ToUniversalTime();

        // EndAt, MinOrderAmount, MaxOrderAmount: new values. Not sent = null (no limit)
        DateTimeOffset? endAt = null;
        if (!string.IsNullOrWhiteSpace(request.EndAt))
        {
            endAt = VoucherProgramDateTime.Parse(request.EndAt).ToUniversalTime();
        }

        var minOrderAmount = request.MinOrderAmount;
        var maxOrderAmount = request.MaxOrderAmount;

        var errors = new List<ValidationError>();
        if (endAt.HasValue && endAt.Value <= startAt)
        {
            errors.Add(new ValidationError
            {
                Field = "endAt",
                Message = request.StartAt != null ? "End time must be later than start time"
                    : "End time must be later than the current start time"
            });
        }
        if (errors.Count > 0)
            throw new ValidationExceptionErrorV1(errors);

        var now = DateTimeOffset.UtcNow;
        if (request.IsActive.HasValue)
        {
            var isActive = request.IsActive.Value;
            var vouchers = _dbContext.Vouchers.Where(x => x.ProgramId == id && !x.IsDeleted);
            if (isActive)
            {
                // API kích hoạt toàn bộ voucher chưa xóa. Chỉ cần đếm đến 10.001 để biết có vượt hạn mức.
                // Tính cả mã đã active còn sót ở dữ liệu xóa mềm cũ vì index xét trực tiếp IsActive.
                var activeCountAfterUpdate = await _dbContext.Vouchers
                    .Where(x => x.ProgramId == id && (!x.IsDeleted || x.IsActive))
                    .Take(10001).CountAsync(cancellationToken);
                if (activeCountAfterUpdate > 10000)
                    throw new ConflictException("Voucher program cannot exceed 10000 active vouchers.");
            }

            try
            {
                // Unique index kiểm tra toàn bảng, kể cả hai chương trình kích hoạt đồng thời
                // hoặc các mã inactive trùng nhau trong chính chương trình này.
                await vouchers.ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsActive, isActive)
                    .SetProperty(x => x.UpdatedAt, now), cancellationToken);
            }
            catch (PostgresException exception) when (exception is
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Vouchers_Code"
            })
            {
                throw new ConflictException("Voucher codes must be unique among active vouchers across the system.");
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
        // var result = ToResponse(program);
        await transaction.CommitAsync(cancellationToken);

        // return result;
        return "Update voucher program successfully";
    }

    public async Task<string> DeleteVoucherProgram(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var program = await GetProgramForUpdate(id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var vouchers = _dbContext.Vouchers.Where(x => x.ProgramId == id && !x.IsDeleted);

        await vouchers.ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.IsDeleted, true)
            .SetProperty(x => x.IsActive, false)
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
        // Khóa dòng chương trình trong transaction để các thao tác sinh mã, cập nhật và xóa
        // trên cùng chương trình chờ nhau, tránh sinh mã trong lúc chương trình bị xóa.
        await GetProgramForUpdate(id, cancellationToken);
        // Mã mới active ngay. Khóa chương trình được giữ trong lúc đếm và lưu để tránh vượt hạn mức.
        // Không giới hạn tổng số mã lịch sử, chỉ đếm mã đang active.
        var activeCount = await _dbContext.Vouchers
            .Where(x => x.ProgramId == id && x.IsActive)
            .Take(10001).CountAsync(cancellationToken);
        if ((long)activeCount + request.Quantity > 10000)
            throw new ConflictException("Voucher program cannot exceed 10000 active vouchers.");

        // Các mã đã chọn được giữ qua nhiều vòng; có trong danh sách chưa có nghĩa là đã lưu DB.
        var codes = new List<string>(request.Quantity);
        // Ghi nhớ mã đang active trong DB hoặc đã chọn trong lô này, để không chọn lại ở vòng sau.
        var excludedCodes = new HashSet<string>(StringComparer.Ordinal);
        // Cả lô dùng chung thời điểm tạo, kể cả khi phải sinh bù qua nhiều vòng.
        var now = DateTimeOffset.UtcNow;
        var discountType = request.DiscountType == "cash" ? VoucherDiscountType.Cash : VoucherDiscountType.Percent;

        var saved = false;
        // Tiếp tục sinh bù và thử lưu đến khi lưu thành công đủ số lượng.
        // Không giới hạn số vòng; vẫn dừng khi request bị hủy hoặc gặp lỗi khác ngoài trùng mã.
        while (!saved)
        {
            // Dừng nếu request bị hủy; các thao tác DB bên dưới cũng nhận cancellationToken.
            cancellationToken.ThrowIfCancellationRequested();
            // Chỉ sinh bù số mã còn thiếu, giữ lại các mã đã chọn từ vòng trước.
            var missing = request.Quantity - codes.Count;
            // Thử sinh dư khoảng 10%, làm tròn lên để dự phòng trùng.
            // Ví dụ: thiếu 3 mã thì thử sinh 4 lần; thiếu 1 mã thì thử sinh 2 lần.
            var candidateCount = (int)Math.Ceiling(missing * 1.1);
            // HashSet giữ các mã ứng viên không trùng nhau trong riêng vòng hiện tại.
            var candidates = new HashSet<string>(candidateCount, StringComparer.Ordinal);
            // Vòng trong chạy candidateCount lần nếu không có lỗi/hủy; mã bị bỏ qua vẫn tính một lần sinh.
            for (var i = 0; i < candidateCount; i++)
            {
                // Mã có dạng: tiền tố + chuỗi ngẫu nhiên dài CodeLength + hậu tố.
                var code = _codeGenerator.Generate(request.CodePrefix, request.CodeLength, request.CodeSuffix);
                // Contains chỉ kiểm tra, không thêm mã vào excludedCodes.
                // Nếu chưa bị loại, Add ở dòng sau thêm vào candidates; mã trùng trong HashSet không được thêm lại.
                if (!excludedCodes.Contains(code))
                    candidates.Add(code);
            }

            if (candidates.Count > 0)
            {
                // Kiểm tra cả nhóm mã bằng một truy vấn, không truy vấn riêng từng mã.
                // Chỉ mã đang active được giữ chỗ, trên toàn bảng kể cả voucher độc lập.
                // Mã inactive ở cùng hoặc khác chương trình được phép tái sử dụng.
                var candidateCodes = candidates.ToArray();
                var existingCodes = await _dbContext.Vouchers.AsNoTracking()
                    .Where(x => x.IsActive && candidateCodes.Contains(x.Code))
                    .Select(x => x.Code)
                    .ToListAsync(cancellationToken);
                // Đưa các mã đang active đã tìm thấy vào tập loại trừ trước khi chọn mã.
                excludedCodes.UnionWith(existingCodes);
                foreach (var code in candidates)
                {
                    // Add vừa thêm vừa trả kết quả: true nếu mã chưa có, false nếu đã có.
                    // Chỉ mã mới được đưa vào codes; đồng thời ghi nhớ để vòng sau không chọn lại.
                    if (excludedCodes.Add(code))
                        codes.Add(code);
                    // Đủ số lượng thì dừng chọn; break này chỉ thoát foreach, chưa thoát vòng while.
                    if (codes.Count == request.Quantity)
                        break;
                }
            }

            // Chưa đủ mã thì sang vòng tiếp theo, chưa lưu một phần lô.
            if (codes.Count != request.Quantity)
                continue;

            var vouchers = codes.Select(code => new Repository.Entity.Voucher
            {
                Id = Guid.NewGuid(), ProgramId = id, Code = code,
                Description = request.Description,
                DiscountType = discountType,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                Visibility = VoucherVisibility.Private,
                MaxUsageCount = request.MaxUsageCount,
                UsedCount = 0, IsActive = true, IsDeleted = false, CreatedAt = now
            }).ToList();

            try
            {
                _dbContext.Vouchers.AddRange(vouchers);
                // EF dùng savepoint trong transaction: nếu trùng mã, hoàn tác cả lần lưu này.
                await _dbContext.SaveChangesAsync(cancellationToken);
                saved = true;
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Vouchers_Code"
            })
            {
                // Request ở chương trình khác có thể chiếm mã sau bước kiểm tra.
                // Tìm mã xung đột ở phía dưới và sinh bù trong vòng tiếp theo.
            }
            finally
            {
                // Bỏ theo dõi lô vừa thử lưu, tránh giữ entity Added sau lỗi.
                // Detached không xóa dữ liệu khỏi DB; lỗi khác vẫn được ném ra ngoài.
                foreach (var voucher in vouchers)
                    _dbContext.Entry(voucher).State = EntityState.Detached;
            }

            if (saved)
                break;

            var attemptedCodes = codes.ToArray();
            var conflicts = (await _dbContext.Vouchers.AsNoTracking()
                .Where(x => x.IsActive && attemptedCodes.Contains(x.Code))
                .Select(x => x.Code)
                .ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);
            // Giữ các mã chưa bị chiếm trong bộ nhớ; vòng sau chỉ sinh bù phần thiếu.
            codes.RemoveAll(conflicts.Contains);
        }

        // Xác nhận toàn bộ lô và giải phóng khóa chương trình trước khi trả kết quả thành công.
        await transaction.CommitAsync(cancellationToken);

        // Thành công luôn trả đúng Quantity mã, không trả về một lô thiếu số lượng.
        return new Response.GenerateProgramVouchersResponse
        {
            ProgramId = id, CreatedCount = codes.Count, Codes = codes
        };
    }

    private async Task<VoucherProgramEntity> GetProgramForUpdate(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.VoucherPrograms.Where(x => x.Id == id);

        // Các hàm gọi đều đã mở transaction. Gán UpdatedAt bằng chính nó vẫn thực hiện UPDATE
        // để giữ khóa dòng đến khi transaction kết thúc, nhưng không đổi giá trị thời gian nghiệp vụ.
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

    private static Response.VoucherProgramResponse ToResponse(VoucherProgramEntity program)
    {
        return new Response.VoucherProgramResponse
        {
            Id = program.Id, Name = program.Name, IsActive = program.IsActive, IsDeleted = program.IsDeleted,
            StartAt = program.StartAt, EndAt = program.EndAt,
            MinOrderAmount = program.MinOrderAmount, MaxOrderAmount = program.MaxOrderAmount,
            CreatedAt = program.CreatedAt, UpdatedAt = program.UpdatedAt
        };
    }

}
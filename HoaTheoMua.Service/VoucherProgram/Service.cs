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
            // Gộp thống kê tổng số mã và số mã đã sử dụng, chỉ tính các chương trình trong trang này.
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
        // Chương trình đã được tải và cập nhật; chỉ truy vấn thêm hai thống kê voucher trong cùng một lần.
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
        // Khóa dòng chương trình trong transaction để các thao tác sinh mã, cập nhật và xóa
        // trên cùng chương trình chờ nhau, tránh cùng đếm hạn mức rồi tạo vượt số lượng.
        await GetProgramForUpdate(id, cancellationToken);
        // Tính cả voucher đã xóa mềm: mỗi chương trình được phát hành tối đa 10.000 mã.
        var count = await _dbContext.Vouchers.CountAsync(x => x.ProgramId == id, cancellationToken);
        if ((long)count + request.Quantity > 10000)
            throw new ConflictException("Voucher program cannot exceed 10000 vouchers.");

        // Các mã đã chọn được giữ qua nhiều vòng; có trong danh sách chưa có nghĩa là đã lưu DB.
        var codes = new List<string>(request.Quantity);
        // Ghi nhớ mã đã tồn tại trong DB hoặc đã chọn, để không chọn lại ở vòng sau.
        var excludedCodes = new HashSet<string>(StringComparer.Ordinal);
        // Cả lô dùng chung thời điểm tạo, kể cả khi phải thử lưu lại.
        var now = DateTimeOffset.UtcNow;
        // Chỉ thành true sau khi SaveChangesAsync thành công; transaction chưa commit ở thời điểm đó.
        var saved = false;
        var discountType = request.DiscountType == "cash" ? VoucherDiscountType.Cash : VoucherDiscountType.Percent;

        // Tối đa 5 vòng (round từ 0 đến 4), bao gồm cả sinh bù và thử lại sau lỗi trùng mã.
        // Không bắt buộc chạy đủ 5 vòng: lưu thành công thì thoát bằng break ở phía dưới.
        for (var round = 0; round < 5; round++)
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
                // Không lọc chương trình hay trạng thái xóa: mã ở chương trình khác, mã độc lập
                // và mã đã xóa mềm đều đã được giữ chỗ, không được cấp lại.
                var candidateCodes = candidates.ToArray();
                var existingCodes = await _dbContext.Vouchers.AsNoTracking()
                    .Where(x => candidateCodes.Contains(x.Code))
                    .Select(x => x.Code)
                    .ToListAsync(cancellationToken);
                // Đưa các mã đã tồn tại trong DB vào tập loại trừ trước khi chọn mã.
                excludedCodes.UnionWith(existingCodes);
                foreach (var code in candidates)
                {
                    // Add vừa thêm vừa trả kết quả: true nếu mã chưa có, false nếu đã có.
                    // Chỉ mã mới được đưa vào codes; đồng thời ghi nhớ để vòng sau không chọn lại.
                    if (excludedCodes.Add(code))
                        codes.Add(code);
                    // Đủ số lượng thì dừng chọn; break này chỉ thoát foreach, chưa thoát vòng round.
                    if (codes.Count == request.Quantity)
                        break;
                }
            }

            // Chưa đủ mã thì sang vòng round tiếp theo, chưa tạo hay lưu voucher nào.
            if (codes.Count != request.Quantity)
                continue;
            // Đủ mã mới tạo toàn bộ lô entity, dùng chung cấu hình giảm giá trong request.
            // Voucher mới luôn riêng tư, chưa kích hoạt và chưa được sử dụng.
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
                // AddRange đăng ký các entity cần thêm; SaveChangesAsync mới gửi lệnh lưu xuống DB.
                _dbContext.Vouchers.AddRange(vouchers);
                // Trong transaction PostgreSQL hiện tại, EF tạo savepoint trước khi lưu.
                // Nếu lưu lỗi, EF hoàn tác về savepoint để không giữ lại một phần lô vừa thử lưu.
                await _dbContext.SaveChangesAsync(cancellationToken);
                saved = true;
            }
            // Chỉ bắt lỗi trùng mã tại unique index IX_Vouchers_Code; lỗi khác được ném ra ngoài.
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Vouchers_Code"
            })
            {
                // Giữ saved = false. Phía dưới sẽ tìm mã bị trùng, loại khỏi codes và thử ở vòng sau.
            }
            finally
            {
                // Luôn bỏ theo dõi các entity của lô này, dù lưu thành công, lỗi hay bị hủy.
                // Tránh entity lưu thất bại còn ở trạng thái Added và bị lưu lại trong lần sau.
                // Detached chỉ bỏ theo dõi trong DbContext, không xóa dữ liệu khỏi DB.
                foreach (var voucher in vouchers)
                    _dbContext.Entry(voucher).State = EntityState.Detached;
            }

            // Lưu thành công thì thoát vòng round ngay; sau đó mới commit transaction.
            if (saved)
                break;

            // Chỉ đến đây khi lưu bị trùng mã: request khác có thể đã chiếm mã sau bước kiểm tra.
            // Khóa chương trình không ngăn request của chương trình khác chọn cùng mã.
            // Truy vấn lại để biết mã nào đã tồn tại; giữ các mã còn lại trong bộ nhớ để sinh bù.
            var attemptedCodes = codes.ToArray();
            var conflicts = (await _dbContext.Vouchers.AsNoTracking()
                .Where(x => attemptedCodes.Contains(x.Code))
                .Select(x => x.Code)
                .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.Ordinal);
            // Ví dụ codes = [A, B, C], B bị trùng thì còn [A, C]; vòng sau chỉ cần chọn thêm 1 mã.
            // A và C chưa được lưu bởi request này vì lần lưu lỗi đã được hoàn tác.
            codes.RemoveAll(conflicts.Contains);
        }

        // Hết 5 vòng vẫn chưa lưu được thì báo lỗi. Transaction chưa commit được rollback khi dispose.
        if (!saved)
            throw new ServerException("Unable to generate enough unique voucher codes after 5 rounds.");

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

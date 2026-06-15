using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.CompanyAddress;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Response.CompanyAddressResponse> GetCompanyAddress()
    {
        var result = await _dbContext.CompanyAddresses
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new Response.CompanyAddressResponse
            {
                Id = x.Id,
                AddressLine = x.AddressLine,
                Ward = x.Ward,
                City = x.City,
                Country = x.Country,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                MapId = x.MapId,
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            throw new NotFoundException("Không tìm thấy địa chỉ công ty");
        }

        return result;
    }

    public async Task<Response.CompanyAddressResponse> GetCompanyAddressById(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("Id địa chỉ công ty không hợp lệ");
        }

        var result = await _dbContext.CompanyAddresses
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new Response.CompanyAddressResponse
            {
                Id = x.Id,
                AddressLine = x.AddressLine,
                Ward = x.Ward,
                MapId = x.MapId,
                City = x.City,
                Country = x.Country,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            throw new NotFoundException("Không tìm thấy địa chỉ công ty");
        }

        return result;
    }

    public async Task<Response.CompanyAddressResponse> CreateCompanyAddress(
        Request.CreateCompanyAddressRequest request)
    {
        var hasActiveCompanyAddress = await _dbContext.CompanyAddresses
            .AnyAsync(x => !x.IsDeleted);

        if (hasActiveCompanyAddress)
        {
            throw new ConflictException("Chỉ được phép tồn tại một địa chỉ công ty trong cùng một khoảng thời gian");
        }

        var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // var now = DateTimeOffset.UtcNow;
            //
            // var activeAddresses = await _dbContext.CompanyAddresses
            //     .Where(x => !x.IsDeleted)
            //     .ToListAsync();
            //
            // foreach (var address in activeAddresses)
            // {
            //     address.IsDeleted = true;
            //     address.UpdatedAt = now;
            // }

            var companyAddress = new Repository.Entity.CompanyAddress
            {
                Id = Guid.NewGuid(),
                AddressLine = request.AddressLine.Trim(),
                Ward = request.Ward?.Trim(),
                MapId = request.MapId.Trim(),
                City = request.City.Trim(),
                Country = request.Country.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null,
                IsDeleted = false
            };

            await _dbContext.CompanyAddresses.AddAsync(companyAddress);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return new Response.CompanyAddressResponse
            {
                Id = companyAddress.Id,
                AddressLine = companyAddress.AddressLine,
                Ward = companyAddress.Ward,
                MapId = companyAddress.MapId,
                City = companyAddress.City,
                Country = companyAddress.Country,
                Latitude = companyAddress.Latitude,
                Longitude = companyAddress.Longitude,
                CreatedAt = companyAddress.CreatedAt,
                UpdatedAt = companyAddress.UpdatedAt
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> UpdateCompanyAddress(
        Guid id,
        Request.UpdateCompanyAddressRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("Id địa chỉ công ty không hợp lệ");
        }

        var companyAddress = await _dbContext.CompanyAddresses
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (companyAddress == null)
        {
            throw new NotFoundException("Không tìm thấy địa chỉ công ty");
        }

        var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            companyAddress.AddressLine = request.AddressLine.Trim();
            companyAddress.Ward = request.Ward?.Trim();
            companyAddress.MapId =  request.MapId.Trim();
            companyAddress.City = request.City.Trim();
            companyAddress.Country = request.Country.Trim();
            companyAddress.Latitude = request.Latitude;
            companyAddress.Longitude = request.Longitude;
            companyAddress.UpdatedAt = DateTimeOffset.UtcNow;

            _dbContext.CompanyAddresses.Update(companyAddress);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return "Cập nhật địa chỉ công ty thành công.";
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> DeleteCompanyAddress(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("Id địa chỉ công ty không hợp lệ");
        }

        var companyAddress = await _dbContext.CompanyAddresses
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (companyAddress == null)
        {
            throw new NotFoundException("Không tìm thấy địa chỉ công ty");
        }

        companyAddress.IsDeleted = true;
        companyAddress.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.CompanyAddresses.Update(companyAddress);
        await _dbContext.SaveChangesAsync();

        return "Xóa địa chỉ công ty thành công.";
    }
}
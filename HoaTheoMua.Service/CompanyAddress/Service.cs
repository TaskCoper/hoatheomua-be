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

   public async Task<Response.GetCompanyAddressesResponse> GetCompanyAddresses(
    Request.GetCompanyAddressesRequest request)
{
    var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
    var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

    var query = _dbContext.CompanyAddresses
        .Where(x => !x.IsDeleted);

    if (!string.IsNullOrWhiteSpace(request.Keyword))
    {
        var keyword = request.Keyword.Trim().ToLower();

        query = query.Where(x =>
            x.AddressLine.ToLower().Contains(keyword)
            || (x.Ward != null && x.Ward.ToLower().Contains(keyword))
            || (x.District != null && x.District.ToLower().Contains(keyword))
            || x.City.ToLower().Contains(keyword)
            || x.Country.ToLower().Contains(keyword)
        );
    }

    if (!string.IsNullOrWhiteSpace(request.City))
    {
        var city = request.City.Trim().ToLower();

        query = query.Where(x => x.City.ToLower().Contains(city));
    }

    var totalItems = await query.CountAsync();

    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var items = await query
        .OrderByDescending(x => x.CreatedAt)
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new Response.CompanyAddressResponse
        {
            Id = x.Id,
            AddressLine = x.AddressLine,
            Ward = x.Ward,
            District = x.District,
            City = x.City,
            Country = x.Country,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        })
        .ToListAsync();

    return new Response.GetCompanyAddressesResponse
    {
        Items = items,
        PageIndex = pageIndex,
        PageSize = pageSize,
        TotalItems = totalItems,
        TotalPages = totalPages,
        PreviousPage = pageIndex > 1 ? pageIndex - 1 : null,
        NextPage = pageIndex < totalPages ? pageIndex + 1 : null
    };
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
                District = x.District,
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
        var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var companyAddress = new Repository.Entity.CompanyAddress
            {
                Id = Guid.NewGuid(),
                AddressLine = request.AddressLine.Trim(),
                Ward = request.Ward?.Trim(),
                District = request.District?.Trim(),
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
                District = companyAddress.District,
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
            companyAddress.District = request.District?.Trim();
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
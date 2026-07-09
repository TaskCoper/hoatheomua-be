using FluentValidation;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using AddressEntity = HoaTheoMua.Repository.Entity.Address;

namespace HoaTheoMua.Service.Address;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<Request.CreateAddressRequest> _createValidator;
    private readonly IValidator<Request.UpdateAddressRequest> _updateValidator;

    public Service(
        AppDbContext dbContext,
        ICurrentUserService currentUser,
        IValidator<Request.CreateAddressRequest> createValidator,
        IValidator<Request.UpdateAddressRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<Response.AddressResponse>> GetMyAddresses()
    {
        var userId = _currentUser.GetRequiredUserId();

        var entities = await _dbContext.Addresses
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

        return entities.Select(ToResponse).ToList();
    }

    public async Task<Response.AddressResponse> GetAddressById(Guid id)
    {
        var userId = _currentUser.GetRequiredUserId();

        var entity = await _dbContext.Addresses
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Address not found");
        }

        return ToResponse(entity);
    }

    public async Task<string> CreateAddress(Request.CreateAddressRequest request)
    {
        await ValidateAsync(_createValidator, request);

        var userId = _currentUser.GetRequiredUserId();

        // Địa chỉ đầu tiên của user luôn được đặt làm mặc định.
        var hasAny = await _dbContext.Addresses
            .AnyAsync(x => x.UserId == userId && !x.IsDeleted);
        var isDefault = request.IsDefault || !hasAny;

        if (isDefault)
        {
            await ClearDefaultAsync(userId);
        }

        var entity = new AddressEntity
        {
            UserId = userId,
            ReceiverName = request.ReceiverName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AddressLine = request.AddressLine.Trim(),
            Ward = request.Ward?.Trim(),
            City = request.City.Trim(),
            Country = request.Country.Trim(),
            MapId = request.MapId?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = isDefault,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null,
            IsDeleted = false
        };

        await _dbContext.Addresses.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return "Cập nhật thành công";
    }

    public async Task<string> UpdateAddress(Guid id, Request.UpdateAddressRequest request)
    {
        await ValidateAsync(_updateValidator, request);

        var userId = _currentUser.GetRequiredUserId();

        var entity = await _dbContext.Addresses
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Address not found");
        }

        entity.ReceiverName = request.ReceiverName.Trim();
        entity.PhoneNumber = request.PhoneNumber.Trim();
        entity.AddressLine = request.AddressLine.Trim();
        entity.Ward = request.Ward?.Trim();
        entity.City = request.City.Trim();
        entity.Country = request.Country.Trim();
        entity.MapId = request.MapId?.Trim();
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        // Cho phép nâng địa chỉ này lên mặc định khi cập nhật.
        if (request.IsDefault == true && !entity.IsDefault)
        {
            await ClearDefaultAsync(userId);
            entity.IsDefault = true;
        }

        _dbContext.Addresses.Update(entity);
        await _dbContext.SaveChangesAsync();

        return "Cập nhật thành công";
    }

    public async Task<string> DeleteAddress(Guid id)
    {
        var userId = _currentUser.GetRequiredUserId();

        var entity = await _dbContext.Addresses
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Address not found");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        // Nếu xóa địa chỉ mặc định, đôn địa chỉ mới nhất còn lại lên làm mặc định.
        if (entity.IsDefault)
        {
            entity.IsDefault = false;

            var next = await _dbContext.Addresses
                .Where(x => x.UserId == userId && x.Id != id && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (next != null)
            {
                next.IsDefault = true;
            }
        }

        await _dbContext.SaveChangesAsync();

        return "Xóa thành công";
    }

    public async Task<string> SetDefaultAddress(Guid id)
    {
        var userId = _currentUser.GetRequiredUserId();

        var entity = await _dbContext.Addresses
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);

        if (entity == null)
        {
            throw new NotFoundException("Address not found");
        }

        if (!entity.IsDefault)
        {
            await ClearDefaultAsync(userId);
            entity.IsDefault = true;
            await _dbContext.SaveChangesAsync();
        }

        return "Đặt địa chỉ mặc định thành công";
    }

    // Bỏ cờ mặc định trên mọi địa chỉ hiện tại của user.
    // Không gọi SaveChanges — caller chịu trách nhiệm lưu trong cùng một lần.
    private async Task ClearDefaultAsync(Guid userId)
    {
        var currentDefaults = await _dbContext.Addresses
            .Where(x => x.UserId == userId && x.IsDefault && !x.IsDeleted)
            .ToListAsync();

        foreach (var address in currentDefaults)
        {
            address.IsDefault = false;
        }
    }

    private static async Task ValidateAsync<T>(IValidator<T> validator, T request)
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
        {
            throw new ValidationExceptionError(result.Errors.First().ErrorMessage);
        }
    }

    private static Response.AddressResponse ToResponse(AddressEntity x) => new()
    {
        Id = x.Id,
        UserId = x.UserId,
        ReceiverName = x.ReceiverName,
        PhoneNumber = x.PhoneNumber,
        AddressLine = x.AddressLine,
        Ward = x.Ward,
        City = x.City,
        Country = x.Country,
        MapId = x.MapId,
        Latitude = x.Latitude,
        Longitude = x.Longitude,
        IsDefault = x.IsDefault,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt
    };
}

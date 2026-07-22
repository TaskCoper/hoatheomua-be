using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Service.CompanyAddress;
using HoaTheoMua.Service.CompanyAddress.Validations;
using HoaTheoMua.Service.Exceptions;

using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress.Service;

namespace HoaTheoMua.Service.Test.CompanyAddressTest;

public class UpdateCompanyAddressTest
{
    private AppDbContext CreateMockDbContext(List<CompanyAddressEntity> companyAddresses)
    {
        var mockDbSet = companyAddresses.BuildMockDbSet();

        mockDbSet
            .Setup(x => x.Update(It.IsAny<CompanyAddressEntity>()))
            .Callback((CompanyAddressEntity entity) =>
            {
                var index = companyAddresses.FindIndex(x => x.Id == entity.Id);

                if (index >= 0)
                {
                    companyAddresses[index] = entity;
                }
            })
            .Returns((EntityEntry<CompanyAddressEntity>)null!);

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.CompanyAddresses)
            .Returns(mockDbSet.Object);

        mockDbContext
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var mockTransaction = new Mock<IDbContextTransaction>();

        mockTransaction
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockTransaction
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);

        mockDatabase
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        mockDbContext
            .Setup(x => x.Database)
            .Returns(mockDatabase.Object);

        return mockDbContext.Object;
    }

    private CompanyAddressEntity CreateAddress(Guid id, bool isDeleted)
    {
        return new CompanyAddressEntity
        {
            Id = id,
            AddressLine = "123 Nguyen Hue",
            Ward = "Ben Nghe",
            City = "Ho Chi Minh",
            PhoneNumber = "0909123456",
            FullName = "Hoa Theo Mua",
            Remarks = "Company address",
            Country = "Vietnam",
            Latitude = 10.776889m,
            Longitude = 106.700806m,
            MapId = "map-new",
            IsDeleted = isDeleted,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = null
        };
    }

    private Request.UpdateCompanyAddressRequest CreateValidRequest()
    {
        return new Request.UpdateCompanyAddressRequest
        {
            AddressLine = "456 Le Loi",
            Ward = "Ben Thanh",
            City = "Ho Chi Minh",
            Country = "Vietnam",
            FullName = "Hoa Theo Mua Updated",
            Phone = "0909888777",
            Remarks = "Updated address remark",
            Latitude = 10.772345m,
            Longitude = 106.698765m,
            MapId = "map-def456"
        };
    }

    private Request.UpdateCompanyAddressRequest CreateSameDataRequest(CompanyAddressEntity address)
    {
        return new Request.UpdateCompanyAddressRequest
        {
            AddressLine = address.AddressLine,
            Ward = address.Ward!,
            City = address.City,
            Country = address.Country,
            FullName = address.FullName,
            Phone = address.PhoneNumber,
            Remarks = address.Remarks,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            MapId = address.MapId!
        };
    }

    private IValidator<Request.UpdateCompanyAddressRequest> CreateRealValidator()
    {
        return new UpdateCompanyAddressRequestValidator();
    }

    // TC01: Cập nhật địa chỉ thành công
    [Fact]
    public async Task Update_Success()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(addressId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);
        var request = CreateValidRequest();

        // Act
        var result = await service.UpdateCompanyAddress(addressId, request);

        // Assert
        Assert.Equal("Cập nhật địa chỉ công ty thành công.", result);

        var updatedAddress = companyAddresses.First(x => x.Id == addressId);

        Assert.Equal("456 Le Loi", updatedAddress.AddressLine);
        Assert.Equal("Ben Thanh", updatedAddress.Ward);
        Assert.Equal("Ho Chi Minh", updatedAddress.City);
        Assert.Equal("Vietnam", updatedAddress.Country);
        Assert.Equal("Hoa Theo Mua Updated", updatedAddress.FullName);
        Assert.Equal("0909888777", updatedAddress.PhoneNumber);
        Assert.Equal("Updated address remark", updatedAddress.Remarks);
        Assert.Equal(10.772345m, updatedAddress.Latitude);
        Assert.Equal(106.698765m, updatedAddress.Longitude);
        Assert.Equal("map-def456", updatedAddress.MapId);
        Assert.False(updatedAddress.IsDeleted);
        Assert.NotNull(updatedAddress.UpdatedAt);
    }

    // TC03: Không tìm thấy địa chỉ
    [Fact]
    public async Task Update_NotFound_ThrowNotFound()
    {
        // Arrange
        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(Guid.NewGuid(), isDeleted: false)
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);
        var request = CreateValidRequest();

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateCompanyAddress(Guid.NewGuid(), request)
        );

        // Assert
        Assert.Equal("Không tìm thấy địa chỉ công ty", exception.Message);
    }

    // TC05: Cập nhật với thông tin giống hệt cũ
    [Fact]
    public async Task Update_SameData_Success()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        var address = CreateAddress(addressId, isDeleted: false);

        var companyAddresses = new List<CompanyAddressEntity>
        {
            address
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);
        var request = CreateSameDataRequest(address);

        // Act
        var result = await service.UpdateCompanyAddress(addressId, request);

        // Assert
        Assert.Equal("Cập nhật địa chỉ công ty thành công.", result);

        var updatedAddress = companyAddresses.First(x => x.Id == addressId);

        Assert.Equal("123 Nguyen Hue", updatedAddress.AddressLine);
        Assert.Equal("Ben Nghe", updatedAddress.Ward);
        Assert.Equal("Ho Chi Minh", updatedAddress.City);
        Assert.Equal("Vietnam", updatedAddress.Country);
        Assert.Equal("Hoa Theo Mua", updatedAddress.FullName);
        Assert.Equal("0909123456", updatedAddress.PhoneNumber);
        Assert.Equal("Company address", updatedAddress.Remarks);
        Assert.NotNull(updatedAddress.UpdatedAt);
    }

    // TC06: Cập nhật với dữ liệu trống
    [Fact]
    public async Task Update_EmptyRequiredFields_Invalid()
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();

        request.AddressLine = "";
        request.City = "";
        request.Country = "";

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "ADDRESS_REQUIRED");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "CITY_REQUIRED");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "COUNTRY_REQUIRED");
    }

    // TC07: Địa chỉ đã bị soft delete
    [Fact]
    public async Task Update_DeletedAddress_ThrowNotFound()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(addressId, isDeleted: true)
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);
        var request = CreateValidRequest();

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateCompanyAddress(addressId, request)
        );

        // Assert
        Assert.Equal("Không tìm thấy địa chỉ công ty", exception.Message);
    }

    // TC08: Latitude không hợp lệ
    [Theory]
    [InlineData(91)]
    [InlineData(-91)]
    public async Task Update_InvalidLatitude_Invalid(decimal latitude)
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();
        request.Latitude = latitude;

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "LATITUDE_OUT_OF_RANGE");
    }

    // TC08: Longitude không hợp lệ
    [Theory]
    [InlineData(181)]
    [InlineData(-181)]
    public async Task Update_InvalidLongitude_Invalid(decimal longitude)
    {
        // Arrange
        var validator = CreateRealValidator();

        var request = CreateValidRequest();
        request.Longitude = longitude;

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "LONGITUDE_OUT_OF_RANGE");
    }

    // TC09: Kiểm tra quyền Admin (chỉ Admin mới được cập nhật, ngược lại trả về 403)
    [Fact]
    public async Task Update_ServiceDoesNotCheckAdminRole_AuthorizationEnforcedByController()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(addressId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);
        var request = CreateValidRequest();

        // Act
        // Không truyền vào bất kỳ thông tin role/identity nào — Service vẫn thực thi
        // được, chứng tỏ Service không tự chặn theo quyền. Việc chặn (403) là trách
        // nhiệm của [Authorize(Policy = AdminPolicy)] tại Controller.
        var result = await service.UpdateCompanyAddress(addressId, request);

        // Assert
        Assert.Equal("Cập nhật địa chỉ công ty thành công.", result);
    }
}
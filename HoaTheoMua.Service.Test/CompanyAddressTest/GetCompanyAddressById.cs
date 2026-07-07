using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;

using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress.Service;

namespace HoaTheoMua.Service.Test.CompanyAddressTest;

public class GetCompanyAddressByIdTest
{
    private AppDbContext CreateMockDbContext(List<CompanyAddressEntity> companyAddresses)
    {
        var mockDbSet = companyAddresses.BuildMockDbSet();

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.CompanyAddresses)
            .Returns(mockDbSet.Object);

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

    // TC01: Lấy địa chỉ theo ID thành công
    [Fact]
    public async Task GetById_Success()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(addressId, isDeleted: false)
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var result = await service.GetCompanyAddressById(addressId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addressId, result.Id);
        Assert.Equal("123 Nguyen Hue", result.AddressLine);
        Assert.Equal("Ben Nghe", result.Ward);
        Assert.Equal("Ho Chi Minh", result.City);
        Assert.Equal("Vietnam", result.Country);
        Assert.Equal("0909123456", result.Phone);
        Assert.Equal("Hoa Theo Mua", result.FullName);
        Assert.Equal("Company address", result.Remarks);
        Assert.Equal(10.776889m, result.Latitude);
        Assert.Equal(106.700806m, result.Longitude);
        Assert.Equal("map-new", result.MapId);
    }

    // TC03: ID không tồn tại
    [Fact]
    public async Task GetById_NotFound_ThrowBadRequest()
    {
        // Arrange
        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(Guid.NewGuid(), isDeleted: false)
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetCompanyAddressById(Guid.NewGuid())
        );

        // Assert
        Assert.Equal("Id địa chỉ công ty không hợp lệ", exception.Message);
    }

    // TC03: ID thuộc địa chỉ đã bị soft delete
    [Fact]
    public async Task GetById_DeletedAddress_ThrowBadRequest()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(addressId, isDeleted: true)
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetCompanyAddressById(addressId)
        );

        // Assert
        Assert.Equal("Id địa chỉ công ty không hợp lệ", exception.Message);
    }

    // TC04: Địa chỉ có field nullable = null vẫn trả về thành công
    [Fact]
    public async Task GetById_NullableFields_ReturnSuccess()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        var companyAddresses = new List<CompanyAddressEntity>
        {
            new CompanyAddressEntity
            {
                Id = addressId,
                AddressLine = "Address Without Optional Fields",
                Ward = null,
                City = "Ho Chi Minh",
                PhoneNumber = "0909123456",
                FullName = "Hoa Theo Mua",
                Remarks = "Company address",
                Country = "Vietnam",
                Latitude = null,
                Longitude = null,
                MapId = null,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null
            }
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var result = await service.GetCompanyAddressById(addressId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addressId, result.Id);
        Assert.Equal("Address Without Optional Fields", result.AddressLine);
        Assert.Null(result.Ward);
        Assert.Null(result.Latitude);
        Assert.Null(result.Longitude);
        Assert.Null(result.MapId);
    }
}
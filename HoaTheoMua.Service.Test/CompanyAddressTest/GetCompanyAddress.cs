using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;

using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress.Service;

namespace HoaTheoMua.Service.Test.CompanyAddressTest;

public class GetCompanyAddressTest
{
    private AppDbContext CreateMockDbContext(List<CompanyAddressEntity> companyAddresses)
    {
        var mockDbSet = companyAddresses.BuildMockDbSet();

        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);

        mockDbContext
            .Setup(x => x.CompanyAddresses)
            .Returns(mockDbSet.Object);

        return mockDbContext.Object;
    }

    private CompanyAddressEntity CreateAddress(
        string addressLine,
        bool isDeleted,
        DateTimeOffset createdAt)
    {
        return new CompanyAddressEntity
        {
            Id = Guid.NewGuid(),
            AddressLine = addressLine,
            Ward = "Ward 2",
            City = "Ho Chi Minh",
            PhoneNumber = "0222222222",
            FullName = "New Company",
            Remarks = "New address remark",
            Country = "Vietnam",
            Latitude = 10.776889m,
            Longitude = 106.700806m,
            MapId = "map-new",
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            UpdatedAt = null
        };
    }

    // TC01: Lấy địa chỉ công ty thành công
    [Fact]
    public async Task Get_Success()
    {
        // Arrange
        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(
                addressLine: "New Address",
                isDeleted: false,
                createdAt: DateTimeOffset.UtcNow
            )
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var result = await service.GetCompanyAddress();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Address", result.AddressLine);
        Assert.Equal("Ward 2", result.Ward);
        Assert.Equal("Ho Chi Minh", result.City);
        Assert.Equal("Vietnam", result.Country);
        Assert.Equal("0222222222", result.Phone);
        Assert.Equal("New Company", result.FullName);
        Assert.Equal("New address remark", result.Remarks);
        Assert.Equal(10.776889m, result.Latitude);
        Assert.Equal(106.700806m, result.Longitude);
        Assert.Equal("map-new", result.MapId);
    }

    // TC02: DB rỗng thì trả về null
    [Fact]
    public async Task Get_EmptyDb_ReturnNull()
    {
        // Arrange
        var companyAddresses = new List<CompanyAddressEntity>();

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var result = await service.GetCompanyAddress();

        // Assert
        Assert.Null(result);
    }

    // TC02: Tất cả địa chỉ đều IsDeleted = true thì trả về null
    [Fact]
    public async Task Get_AllDeleted_ReturnNull()
    {
        // Arrange
        var companyAddresses = new List<CompanyAddressEntity>
        {
            CreateAddress(
                addressLine: "Deleted Address",
                isDeleted: true,
                createdAt: DateTimeOffset.UtcNow
            )
        };

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var result = await service.GetCompanyAddress();

        // Assert
        Assert.Null(result);
    }

    // TC04: Các field nullable bị null thì vẫn trả về không lỗi
    [Fact]
    public async Task Get_NullableFields_ReturnSuccess()
    {
        // Arrange
        var companyAddresses = new List<CompanyAddressEntity>
        {
            new CompanyAddressEntity
            {
                Id = Guid.NewGuid(),
                AddressLine = "Address Without Optional Fields",
                Ward = null,
                City = "Ho Chi Minh",
                PhoneNumber = "0222222222",
                FullName = "New Company",
                Remarks = "Remark",
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
        var result = await service.GetCompanyAddress();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Address Without Optional Fields", result.AddressLine);
        Assert.Null(result.Ward);
        Assert.Null(result.Latitude);
        Assert.Null(result.Longitude);
        Assert.Null(result.MapId);
    }
}
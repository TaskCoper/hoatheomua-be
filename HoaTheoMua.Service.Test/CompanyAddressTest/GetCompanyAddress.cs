using Microsoft.EntityFrameworkCore;
using Xunit;

using HoaTheoMua.Repository;
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress.Service;

namespace HoaTheoMua.Service.Test.CompanyAddressTest;

public class GetCompanyAddress
{
    [Fact]
    public async Task GetCompanyAddreses()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);

        dbContext.CompanyAddresses.Add(
            new CompanyAddressEntity
            {
                Id = Guid.NewGuid(),
                AddressLine = "New Address",
                Ward = "Ward 2",
                City = "Ho Chi Minh",
                PhoneNumber = "0222222222",
                FullName = "New Company",
                Remarks = "New address remark",
                Country = "Vietnam",
                Latitude = 10.776889m,
                Longitude = 106.700806m,
                MapId = "map-new",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        );

        await dbContext.SaveChangesAsync();

        var service = new CompanyAddressService(dbContext);

        // Act
        var result = await service.GetCompanyAddress();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Address", result.AddressLine);
        Assert.Equal("Ho Chi Minh", result.City);
        Assert.Equal("0222222222", result.Phone);
    }
}
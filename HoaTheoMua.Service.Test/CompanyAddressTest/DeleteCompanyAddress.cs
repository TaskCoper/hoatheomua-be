using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using MockQueryable.Moq;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;

using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress.Service;

namespace HoaTheoMua.Service.Test.CompanyAddressTest;

public class DeleteCompanyAddressTest
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

    // TC01: Xóa địa chỉ thành công
    [Fact]
    public async Task Delete_Success()
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
        var result = await service.DeleteCompanyAddress(addressId);

        // Assert
        Assert.Equal("Xóa địa chỉ công ty thành công.", result);

        var deletedAddress = companyAddresses.First(x => x.Id == addressId);

        Assert.True(deletedAddress.IsDeleted);
        Assert.NotNull(deletedAddress.UpdatedAt);
    }

    // TC02: ID không hợp lệ
    [Fact]
    public async Task Delete_EmptyId_ThrowBadRequest()
    {
        // Arrange
        var companyAddresses = new List<CompanyAddressEntity>();

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.DeleteCompanyAddress(Guid.Empty)
        );

        // Assert
        Assert.Equal("Id địa chỉ công ty không hợp lệ", exception.Message);
    }

    // TC03: Không tìm thấy địa chỉ
    [Fact]
    public async Task Delete_NotFound_ThrowNotFound()
    {
        // Arrange
        var companyAddresses = new List<CompanyAddressEntity>();

        var dbContext = CreateMockDbContext(companyAddresses);
        var service = new CompanyAddressService(dbContext);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteCompanyAddress(Guid.NewGuid())
        );

        // Assert
        Assert.Equal("Không tìm thấy địa chỉ công ty", exception.Message);
    }

    // TC05: Địa chỉ đã bị soft delete trước đó
    [Fact]
    public async Task Delete_AlreadyDeleted_ThrowNotFound()
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
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteCompanyAddress(addressId)
        );

        // Assert
        Assert.Equal("Không tìm thấy địa chỉ công ty", exception.Message);
    }
}
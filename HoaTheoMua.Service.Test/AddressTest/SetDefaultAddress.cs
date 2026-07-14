using System.Linq.Expressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using HoaTheoMua.Service.Test.TestSupport;

using AddressEntity = HoaTheoMua.Repository.Entity.Address;
using AddressService = HoaTheoMua.Service.Address.Service;

namespace HoaTheoMua.Service.Test.AddressTest;

public class SetDefaultAddressTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid OtherUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid AddressId =
        Guid.Parse("63141f8c-3c91-496d-afbd-8f3a872cb178");

    private static readonly Guid OldDefaultAddressId =
        Guid.Parse("853f1665-996e-42bd-92a4-0275143346eb");

    private static readonly Guid NotFoundAddressId =
        Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static AddressService CreateService(
        List<AddressEntity> addresses,
        out Mock<AppDbContext> dbContextMock)
    {
        var addressDbSetMock = BuildMockDbSet(addresses);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .Options;

        dbContextMock = new Mock<AppDbContext>(options);

        dbContextMock
            .Setup(x => x.Addresses)
            .Returns(addressDbSetMock.Object);

        dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(UserId);

        return new AddressService(dbContextMock.Object, currentUserMock.Object);
    }

    private static Mock<DbSet<T>> BuildMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();

        var dbSetMock = new Mock<DbSet<T>>();

        dbSetMock.As<IAsyncEnumerable<T>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(sourceList.GetEnumerator()));

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.Expression)
            .Returns(queryable.Expression);

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.ElementType)
            .Returns(queryable.ElementType);

        dbSetMock.As<IQueryable<T>>()
            .Setup(x => x.GetEnumerator())
            .Returns(() => sourceList.GetEnumerator());

        return dbSetMock;
    }

    private static AddressEntity CreateAddressEntity(
        Guid? id = null,
        Guid? userId = null,
        string receiverName = "quanquan",
        string phoneNumber = "0981234567",
        string addressLine = "123 Le Van Viet",
        string? ward = "Long Thanh My",
        string city = "Ho Chi Minh",
        string country = "Viet Nam",
        string? mapId = "map_123",
        decimal? latitude = 72,
        decimal? longitude = 27,
        bool isDefault = false,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        return new AddressEntity
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? UserId,
            ReceiverName = receiverName,
            PhoneNumber = phoneNumber,
            AddressLine = addressLine,
            Ward = ward,
            City = city,
            Country = country,
            MapId = mapId,
            Latitude = latitude,
            Longitude = longitude,
            IsDefault = isDefault,
            IsDeleted = isDeleted,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = updatedAt
        };
    }

    // TC01
    [Fact]
    public async Task SetDefaultAddress_Success()
    {
        var oldDefaultAddress = CreateAddressEntity(
            id: OldDefaultAddressId,
            receiverName: "old default address",
            isDefault: true,
            isDeleted: false);

        var newDefaultAddress = CreateAddressEntity(
            id: AddressId,
            receiverName: "new default address",
            isDefault: false,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            oldDefaultAddress,
            newDefaultAddress
        };

        var service = CreateService(addresses, out var dbContextMock);

        var result = await service.SetDefaultAddress(AddressId);

        Assert.Equal("Đặt địa chỉ mặc định thành công", result);

        Assert.False(oldDefaultAddress.IsDefault);
        Assert.True(newDefaultAddress.IsDefault);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC02
    [Fact]
    public async Task SetDefaultAddress_NotFound_ThrowNotFound()
    {
        var addresses = new List<AddressEntity>();

        var service = CreateService(addresses, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.SetDefaultAddress(NotFoundAddressId));

        Assert.Equal("Address not found", ex.Message);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC03
    [Fact]
    public async Task SetDefaultAddress_AlreadyDefault_ReturnSuccessWithoutSaveChanges()
    {
        var defaultAddress = CreateAddressEntity(
            id: AddressId,
            receiverName: "current default address",
            isDefault: true,
            isDeleted: false);

        var normalAddress = CreateAddressEntity(
            id: OldDefaultAddressId,
            receiverName: "normal address",
            isDefault: false,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            defaultAddress,
            normalAddress
        };

        var service = CreateService(addresses, out var dbContextMock);

        var result = await service.SetDefaultAddress(AddressId);

        Assert.Equal("Đặt địa chỉ mặc định thành công", result);

        Assert.True(defaultAddress.IsDefault);
        Assert.False(normalAddress.IsDefault);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC04
    [Fact]
    public async Task SetDefaultAddress_OtherUserAddress_ThrowNotFound()
    {
        var otherUserAddress = CreateAddressEntity(
            id: AddressId,
            userId: OtherUserId,
            receiverName: "other user address",
            isDefault: false,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            otherUserAddress
        };

        var service = CreateService(addresses, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.SetDefaultAddress(AddressId));

        Assert.Equal("Address not found", ex.Message);
        Assert.False(otherUserAddress.IsDefault);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Async-queryable shims moved to TestSupport/MockAsyncQueryable.cs
}
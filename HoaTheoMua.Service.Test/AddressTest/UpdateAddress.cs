using System.Linq.Expressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using HoaTheoMua.Service.Test.TestSupport;

using AddressEntity = HoaTheoMua.Repository.Entity.Address;
using AddressService = HoaTheoMua.Service.Address.Service;
using AddressRequest = HoaTheoMua.Service.Address.Request;

namespace HoaTheoMua.Service.Test.AddressTest;

public class UpdateAddressTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid OtherUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid AddressId =
        Guid.Parse("853f1665-996e-42bd-92a4-0275143346eb");

    private static readonly Guid OldDefaultAddressId =
        Guid.Parse("63141f8c-3c91-496d-afbd-8f3a872cb178");

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

        dbSetMock
            .Setup(x => x.Update(It.IsAny<T>()))
            .Returns<T>(_ => null!);

        dbSetMock
            .Setup(x => x.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) =>
            {
                sourceList.Add(entity);
            })
            .Returns<T, CancellationToken>((_, _) =>
                new ValueTask<EntityEntry<T>>((EntityEntry<T>)null!));

        return dbSetMock;
    }

    private static AddressRequest.UpdateAddressRequest CreateValidRequest(
        string receiverName = "quanquan",
        string phoneNumber = "0981234567",
        string addressLine = "12345 Le Van Viet",
        string? ward = "Long Binh",
        string city = "Ho Chi Minh",
        string country = "Viet Nam",
        string? mapId = "ChIJt4qIbx4bdTERB1AQwW0l01g",
        decimal? latitude = 77,
        decimal? longitude = 97,
        bool? isDefault = null)
    {
        return new AddressRequest.UpdateAddressRequest
        {
            ReceiverName = receiverName,
            PhoneNumber = phoneNumber,
            AddressLine = addressLine,
            Ward = ward,
            City = city,
            Country = country,
            MapId = mapId,
            Latitude = latitude,
            Longitude = longitude,
            IsDefault = isDefault
        };
    }

    private static AddressEntity CreateAddressEntity(
        Guid? id = null,
        Guid? userId = null,
        string receiverName = "old receiver",
        string phoneNumber = "0900000000",
        string addressLine = "Old address",
        string? ward = "Old ward",
        string city = "Ho Chi Minh",
        string country = "Viet Nam",
        string? mapId = "old_map",
        decimal? latitude = 10,
        decimal? longitude = 20,
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
    public async Task UpdateAddress_Success()
    {
        var address = CreateAddressEntity(
            id: AddressId,
            userId: UserId,
            isDefault: false,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            address
        };

        var service = CreateService(addresses, out var dbContextMock);

        var request = CreateValidRequest();

        var result = await service.UpdateAddress(AddressId, request);

        Assert.Equal("Cập nhật thành công", result);

        Assert.Equal("quanquan", address.ReceiverName);
        Assert.Equal("0981234567", address.PhoneNumber);
        Assert.Equal("12345 Le Van Viet", address.AddressLine);
        Assert.Equal("Long Binh", address.Ward);
        Assert.Equal("Ho Chi Minh", address.City);
        Assert.Equal("Viet Nam", address.Country);
        Assert.Equal("ChIJt4qIbx4bdTERB1AQwW0l01g", address.MapId);
        Assert.Equal(77, address.Latitude);
        Assert.Equal(97, address.Longitude);
        Assert.False(address.IsDefault);
        Assert.False(address.IsDeleted);
        Assert.NotNull(address.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC02
    [Fact]
    public async Task UpdateAddress_NotFound_ThrowNotFound()
    {
        var addresses = new List<AddressEntity>();

        var service = CreateService(addresses, out var dbContextMock);

        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAddress(NotFoundAddressId, request));

        Assert.Equal("Address not found", ex.Message);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC03
    [Fact]
    public async Task UpdateAddress_OtherUserAddress_ThrowNotFound()
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

        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAddress(AddressId, request));

        Assert.Equal("Address not found", ex.Message);
        Assert.Equal("other user address", otherUserAddress.ReceiverName);
        Assert.Null(otherUserAddress.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC04
    [Fact]
    public async Task UpdateAddress_DeletedAddress_ThrowNotFound()
    {
        var deletedAddress = CreateAddressEntity(
            id: AddressId,
            userId: UserId,
            receiverName: "deleted address",
            isDefault: false,
            isDeleted: true);

        var addresses = new List<AddressEntity>
        {
            deletedAddress
        };

        var service = CreateService(addresses, out var dbContextMock);

        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAddress(AddressId, request));

        Assert.Equal("Address not found", ex.Message);
        Assert.Equal("deleted address", deletedAddress.ReceiverName);
        Assert.True(deletedAddress.IsDeleted);
        Assert.Null(deletedAddress.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC05
    [Fact]
    public async Task UpdateAddress_SetDefault_ClearOldDefault()
    {
        var oldDefaultAddress = CreateAddressEntity(
            id: OldDefaultAddressId,
            receiverName: "old default address",
            isDefault: true,
            isDeleted: false);

        var addressNeedUpdate = CreateAddressEntity(
            id: AddressId,
            receiverName: "normal address",
            isDefault: false,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            oldDefaultAddress,
            addressNeedUpdate
        };

        var service = CreateService(addresses, out var dbContextMock);

        var request = CreateValidRequest(
            isDefault: true);

        var result = await service.UpdateAddress(AddressId, request);

        Assert.Equal("Cập nhật thành công", result);

        Assert.False(oldDefaultAddress.IsDefault);
        Assert.True(addressNeedUpdate.IsDefault);

        Assert.Equal("quanquan", addressNeedUpdate.ReceiverName);
        Assert.Equal("0981234567", addressNeedUpdate.PhoneNumber);
        Assert.Equal("12345 Le Van Viet", addressNeedUpdate.AddressLine);
        Assert.Equal("Long Binh", addressNeedUpdate.Ward);
        Assert.NotNull(addressNeedUpdate.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC06
    [Fact]
    public async Task UpdateAddress_AlreadyDefault_UpdateInfoOnly()
    {
        var defaultAddress = CreateAddressEntity(
            id: AddressId,
            receiverName: "old default address",
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

        var request = CreateValidRequest(
            receiverName: "updated default address",
            isDefault: true);

        var result = await service.UpdateAddress(AddressId, request);

        Assert.Equal("Cập nhật thành công", result);

        Assert.True(defaultAddress.IsDefault);
        Assert.False(normalAddress.IsDefault);

        Assert.Equal("updated default address", defaultAddress.ReceiverName);
        Assert.Equal("0981234567", defaultAddress.PhoneNumber);
        Assert.Equal("12345 Le Van Viet", defaultAddress.AddressLine);
        Assert.Equal("Long Binh", defaultAddress.Ward);
        Assert.NotNull(defaultAddress.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Async-queryable shims moved to TestSupport/MockAsyncQueryable.cs
}
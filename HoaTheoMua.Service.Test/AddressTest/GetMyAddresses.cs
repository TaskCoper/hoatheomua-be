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

public class GetMyAddressesTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid OtherUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid AddressId1 =
        Guid.Parse("853f1665-996e-42bd-92a4-0275143346eb");

    private static readonly Guid AddressId2 =
        Guid.Parse("63141f8c-3c91-496d-afbd-8f3a872cb178");

    private static readonly Guid AddressId3 =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static AddressService CreateService(
        List<AddressEntity> addresses,
        out Mock<AppDbContext> dbContextMock,
        bool throwUnauthorized = false)
    {
        var addressDbSetMock = BuildMockDbSet(addresses);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .Options;

        dbContextMock = new Mock<AppDbContext>(options);

        dbContextMock
            .Setup(x => x.Addresses)
            .Returns(addressDbSetMock.Object);

        var currentUserMock = new Mock<ICurrentUserService>();

        if (throwUnauthorized)
        {
            currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Throws(new UnauthorizedException("User not found"));
        }
        else
        {
            currentUserMock
                .Setup(x => x.GetRequiredUserId())
                .Returns(UserId);
        }

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
        string? mapId = "ChIJt4qIbx4bdTERB1AQwW0l01g",
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
    public async Task GetMyAddresses_Success()
    {
        var defaultAddress = CreateAddressEntity(
            id: AddressId1,
            userId: UserId,
            receiverName: "quanquan",
            phoneNumber: "0981234567",
            addressLine: "123 Le Van Viet",
            ward: "Long Thanh My",
            city: "Ho Chi Minh",
            country: "Viet Nam",
            mapId: "ChIJt4qIbx4bdTERB1AQwW0l01g",
            latitude: 72,
            longitude: 27,
            isDefault: true,
            isDeleted: false,
            createdAt: DateTimeOffset.Parse("2026-06-29T09:41:20.598966+00:00"));

        var normalAddress = CreateAddressEntity(
            id: AddressId2,
            userId: UserId,
            receiverName: "string",
            phoneNumber: "string",
            addressLine: "string",
            ward: "string",
            city: "string",
            country: "string",
            mapId: "string",
            latitude: 0,
            longitude: 0,
            isDefault: false,
            isDeleted: false,
            createdAt: DateTimeOffset.Parse("2026-06-29T09:38:51.894295+00:00"));

        var otherUserAddress = CreateAddressEntity(
            id: AddressId3,
            userId: OtherUserId,
            receiverName: "other user",
            isDefault: true,
            isDeleted: false);

        var deletedAddress = CreateAddressEntity(
            userId: UserId,
            receiverName: "deleted address",
            isDefault: false,
            isDeleted: true);

        var addresses = new List<AddressEntity>
        {
            defaultAddress,
            normalAddress,
            otherUserAddress,
            deletedAddress
        };

        var service = CreateService(addresses, out _);

        var result = await service.GetMyAddresses();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(AddressId1, result[0].Id);
        Assert.Equal(UserId, result[0].UserId);
        Assert.Equal("quanquan", result[0].ReceiverName);
        Assert.Equal("0981234567", result[0].PhoneNumber);
        Assert.Equal("123 Le Van Viet", result[0].AddressLine);
        Assert.Equal("Long Thanh My", result[0].Ward);
        Assert.Equal("Ho Chi Minh", result[0].City);
        Assert.Equal("Viet Nam", result[0].Country);
        Assert.Equal("ChIJt4qIbx4bdTERB1AQwW0l01g", result[0].MapId);
        Assert.Equal(72, result[0].Latitude);
        Assert.Equal(27, result[0].Longitude);
        Assert.True(result[0].IsDefault);
        Assert.Null(result[0].UpdatedAt);

        Assert.Equal(AddressId2, result[1].Id);
        Assert.Equal(UserId, result[1].UserId);
        Assert.Equal("string", result[1].ReceiverName);
        Assert.False(result[1].IsDefault);
    }

    // TC02
    [Fact]
    public async Task GetMyAddresses_SortDefaultFirstThenNewestCreatedAt()
    {
        var oldestDefaultAddress = CreateAddressEntity(
            id: AddressId1,
            receiverName: "default address",
            isDefault: true,
            isDeleted: false,
            createdAt: DateTimeOffset.UtcNow.AddDays(-10));

        var newestNormalAddress = CreateAddressEntity(
            id: AddressId2,
            receiverName: "newest normal address",
            isDefault: false,
            isDeleted: false,
            createdAt: DateTimeOffset.UtcNow.AddDays(-1));

        var olderNormalAddress = CreateAddressEntity(
            id: AddressId3,
            receiverName: "older normal address",
            isDefault: false,
            isDeleted: false,
            createdAt: DateTimeOffset.UtcNow.AddDays(-5));

        var addresses = new List<AddressEntity>
        {
            olderNormalAddress,
            newestNormalAddress,
            oldestDefaultAddress
        };

        var service = CreateService(addresses, out _);

        var result = await service.GetMyAddresses();

        Assert.Equal(3, result.Count);

        Assert.Equal(AddressId1, result[0].Id);
        Assert.True(result[0].IsDefault);

        Assert.Equal(AddressId2, result[1].Id);
        Assert.False(result[1].IsDefault);

        Assert.Equal(AddressId3, result[2].Id);
        Assert.False(result[2].IsDefault);
    }

    // TC03
    [Fact]
    public async Task GetMyAddresses_UserHasNoAddress_ReturnEmptyList()
    {
        var addresses = new List<AddressEntity>();

        var service = CreateService(addresses, out _);

        var result = await service.GetMyAddresses();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // TC04
    [Fact]
    public async Task GetMyAddresses_NoUserIdInToken_ThrowUnauthorized()
    {
        var addresses = new List<AddressEntity>
        {
            CreateAddressEntity()
        };

        var service = CreateService(
            addresses,
            out _,
            throwUnauthorized: true);

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.GetMyAddresses());

        Assert.Equal("User not found", ex.Message);
    }

    // Async-queryable shims moved to TestSupport/MockAsyncQueryable.cs
}
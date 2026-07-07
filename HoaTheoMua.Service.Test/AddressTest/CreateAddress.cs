using System.Linq.Expressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

using AddressEntity = HoaTheoMua.Repository.Entity.Address;
using AddressService = HoaTheoMua.Service.Address.Service;
using AddressRequest = HoaTheoMua.Service.Address.Request;

namespace HoaTheoMua.Service.Test.AddressTest;

public class CreateAddressTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid OtherUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

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
            .Setup(x => x.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) =>
            {
                sourceList.Add(entity);
            })
            .Returns<T, CancellationToken>((_, _) =>
                new ValueTask<EntityEntry<T>>((EntityEntry<T>)null!));

        dbSetMock
            .Setup(x => x.Update(It.IsAny<T>()))
            .Returns<T>(_ => null!);

        return dbSetMock;
    }

    private static AddressRequest.CreateAddressRequest CreateValidRequest(
        string receiverName = "quanquan",
        string phoneNumber = "0981234567",
        string addressLine = "123 Le Van Viet",
        string? ward = "Long Thanh My",
        string city = "Ho Chi Minh",
        string country = "Viet Nam",
        string? mapId = "ChIJt4qIbx4bdTERB1AQwW0l01g",
        decimal? latitude = 72,
        decimal? longitude = 27,
        bool isDefault = true)
    {
        return new AddressRequest.CreateAddressRequest
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
        DateTimeOffset? createdAt = null)
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
            UpdatedAt = null
        };
    }

    // TC01
    [Fact]
    public async Task CreateAddress_Success()
    {
        var addresses = new List<AddressEntity>();
        var service = CreateService(addresses, out var dbContextMock);

        var request = CreateValidRequest();

        var result = await service.CreateAddress(request);

        var savedAddress = Assert.Single(addresses);

        Assert.Equal("Cập nhật thành công", result);
        Assert.Equal(UserId, savedAddress.UserId);
        Assert.Equal("quanquan", savedAddress.ReceiverName);
        Assert.Equal("0981234567", savedAddress.PhoneNumber);
        Assert.Equal("123 Le Van Viet", savedAddress.AddressLine);
        Assert.Equal("Long Thanh My", savedAddress.Ward);
        Assert.Equal("Ho Chi Minh", savedAddress.City);
        Assert.Equal("Viet Nam", savedAddress.Country);
        Assert.Equal("ChIJt4qIbx4bdTERB1AQwW0l01g", savedAddress.MapId);
        Assert.Equal(72, savedAddress.Latitude);
        Assert.Equal(27, savedAddress.Longitude);
        Assert.True(savedAddress.IsDefault);
        Assert.False(savedAddress.IsDeleted);
        Assert.Null(savedAddress.UpdatedAt);

        dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC02
    [Fact]
    public async Task CreateAddress_FirstAddress_AutoDefault()
    {
        var addresses = new List<AddressEntity>();
        var service = CreateService(addresses, out _);

        var request = CreateValidRequest(isDefault: false);

        var result = await service.CreateAddress(request);

        var savedAddress = Assert.Single(addresses);

        Assert.Equal("Cập nhật thành công", result);
        Assert.True(savedAddress.IsDefault);
    }

    // TC03
    [Fact]
    public async Task CreateAddress_WithIsDefaultTrue_ClearOldDefault()
    {
        var oldDefaultAddress = CreateAddressEntity(
            receiverName: "old default",
            isDefault: true);

        var addresses = new List<AddressEntity>
        {
            oldDefaultAddress
        };

        var service = CreateService(addresses, out _);

        var request = CreateValidRequest(
            receiverName: "new default",
            isDefault: true);

        var result = await service.CreateAddress(request);

        var newAddress = addresses.Single(x => x.ReceiverName == "new default");

        Assert.Equal("Cập nhật thành công", result);
        Assert.False(oldDefaultAddress.IsDefault);
        Assert.True(newAddress.IsDefault);
        Assert.Equal(2, addresses.Count);
    }

    // TC04
    [Fact]
    public async Task CreateAddress_WithIsDefaultFalse_WhenUserHasAddress_CreateNonDefault()
    {
        var oldDefaultAddress = CreateAddressEntity(
            receiverName: "old default",
            isDefault: true);

        var addresses = new List<AddressEntity>
        {
            oldDefaultAddress
        };

        var service = CreateService(addresses, out _);

        var request = CreateValidRequest(
            receiverName: "new non default",
            isDefault: false);

        var result = await service.CreateAddress(request);

        var newAddress = addresses.Single(x => x.ReceiverName == "new non default");

        Assert.Equal("Cập nhật thành công", result);
        Assert.True(oldDefaultAddress.IsDefault);
        Assert.False(newAddress.IsDefault);
        Assert.Equal(2, addresses.Count);
    }

    // TC05
    [Fact]
    public async Task CreateAddress_TrimRequestData()
    {
        var addresses = new List<AddressEntity>();
        var service = CreateService(addresses, out _);

        var request = CreateValidRequest(
            receiverName: "  quanquan  ",
            phoneNumber: "  0981234567  ",
            addressLine: "  123 Le Van Viet  ",
            ward: "  Long Thanh My  ",
            city: "  Ho Chi Minh  ",
            country: "  Viet Nam  ",
            mapId: "  map_123  ");

        var result = await service.CreateAddress(request);

        var savedAddress = Assert.Single(addresses);

        Assert.Equal("Cập nhật thành công", result);
        Assert.Equal("quanquan", savedAddress.ReceiverName);
        Assert.Equal("0981234567", savedAddress.PhoneNumber);
        Assert.Equal("123 Le Van Viet", savedAddress.AddressLine);
        Assert.Equal("Long Thanh My", savedAddress.Ward);
        Assert.Equal("Ho Chi Minh", savedAddress.City);
        Assert.Equal("Viet Nam", savedAddress.Country);
        Assert.Equal("map_123", savedAddress.MapId);
    }

    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];

            var executionResult = typeof(IQueryProvider)
                .GetMethods()
                .First(method =>
                    method.Name == nameof(IQueryProvider.Execute) &&
                    method.IsGenericMethod &&
                    method.GetParameters().Length == 1)
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new object[] { expression });

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private class TestAsyncEnumerable<T> :
        EnumerableQuery<T>,
        IAsyncEnumerable<T>,
        IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider =>
            new TestAsyncQueryProvider<T>(this);
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
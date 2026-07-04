using System.Linq.Expressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

using AddressEntity = HoaTheoMua.Repository.Entity.Address;
using AddressService = HoaTheoMua.Service.Address.Service;

namespace HoaTheoMua.Service.Test.AddressTest;

public class GetAddressByIdTest
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid OtherUserId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid AddressId =
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
        string? mapId = "ChIJt4qIbx4bdTERB1AQwW0l01g",
        decimal? latitude = 72,
        decimal? longitude = 27,
        bool isDefault = true,
        bool isDeleted = false,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        return new AddressEntity
        {
            Id = id ?? AddressId,
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
            CreatedAt = createdAt ?? DateTimeOffset.Parse("2026-06-29T09:41:20.598966+00:00"),
            UpdatedAt = updatedAt
        };
    }

    // TC01
    [Fact]
    public async Task GetAddressById_Success()
    {
        var address = CreateAddressEntity(
            id: AddressId,
            userId: UserId,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            address
        };

        var service = CreateService(addresses, out _);

        var result = await service.GetAddressById(AddressId);

        Assert.NotNull(result);
        Assert.Equal(AddressId, result.Id);
        Assert.Equal(UserId, result.UserId);
        Assert.Equal("quanquan", result.ReceiverName);
        Assert.Equal("0981234567", result.PhoneNumber);
        Assert.Equal("123 Le Van Viet", result.AddressLine);
        Assert.Equal("Long Thanh My", result.Ward);
        Assert.Equal("Ho Chi Minh", result.City);
        Assert.Equal("Viet Nam", result.Country);
        Assert.Equal("ChIJt4qIbx4bdTERB1AQwW0l01g", result.MapId);
        Assert.Equal(72, result.Latitude);
        Assert.Equal(27, result.Longitude);
        Assert.True(result.IsDefault);
        Assert.Equal(address.CreatedAt, result.CreatedAt);
        Assert.Null(result.UpdatedAt);
    }

    // TC02
    [Fact]
    public async Task GetAddressById_NotFound_ThrowNotFound()
    {
        var addresses = new List<AddressEntity>();

        var service = CreateService(addresses, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetAddressById(NotFoundAddressId));

        Assert.Equal("Address not found", ex.Message);
    }

    // TC03
    [Fact]
    public async Task GetAddressById_OtherUserAddress_ThrowNotFound()
    {
        var otherUserAddress = CreateAddressEntity(
            id: AddressId,
            userId: OtherUserId,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            otherUserAddress
        };

        var service = CreateService(addresses, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetAddressById(AddressId));

        Assert.Equal("Address not found", ex.Message);
    }

    // TC04
    [Fact]
    public async Task GetAddressById_DeletedAddress_ThrowNotFound()
    {
        var deletedAddress = CreateAddressEntity(
            id: AddressId,
            userId: UserId,
            isDeleted: true);

        var addresses = new List<AddressEntity>
        {
            deletedAddress
        };

        var service = CreateService(addresses, out _);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetAddressById(AddressId));

        Assert.Equal("Address not found", ex.Message);
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
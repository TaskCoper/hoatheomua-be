using System.Linq.Expressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

using AddressEntity = HoaTheoMua.Repository.Entity.Address;
using AddressService = HoaTheoMua.Service.Address.Service;

namespace HoaTheoMua.Service.Test.AddressTest;

public class DeleteAddressTest
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
    public async Task DeleteAddress_Success()
    {
        var address = CreateAddressEntity(
            id: AddressId,
            isDefault: false,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            address
        };

        var service = CreateService(addresses, out var dbContextMock);

        var result = await service.DeleteAddress(AddressId);

        Assert.Equal("Xóa thành công", result);
        Assert.True(address.IsDeleted);
        Assert.False(address.IsDefault);
        Assert.NotNull(address.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // TC02
    [Fact]
    public async Task DeleteAddress_NotFound_ThrowNotFound()
    {
        var addresses = new List<AddressEntity>();

        var service = CreateService(addresses, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteAddress(NotFoundAddressId));

        Assert.Equal("Address not found", ex.Message);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC03
    [Fact]
    public async Task DeleteAddress_OtherUserAddress_ThrowNotFound()
    {
        var otherUserAddress = CreateAddressEntity(
            id: AddressId,
            userId: OtherUserId,
            isDefault: false,
            isDeleted: false);

        var addresses = new List<AddressEntity>
        {
            otherUserAddress
        };

        var service = CreateService(addresses, out var dbContextMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteAddress(AddressId));

        Assert.Equal("Address not found", ex.Message);
        Assert.False(otherUserAddress.IsDeleted);
        Assert.Null(otherUserAddress.UpdatedAt);

        dbContextMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // TC04
    [Fact]
    public async Task DeleteAddress_NonDefaultAddress_OnlySoftDelete()
    {
        var defaultAddress = CreateAddressEntity(
            receiverName: "default address",
            isDefault: true,
            createdAt: DateTimeOffset.UtcNow.AddDays(-3));

        var deletedAddress = CreateAddressEntity(
            id: AddressId,
            receiverName: "normal address",
            isDefault: false,
            createdAt: DateTimeOffset.UtcNow.AddDays(-1));

        var addresses = new List<AddressEntity>
        {
            defaultAddress,
            deletedAddress
        };

        var service = CreateService(addresses, out _);

        var result = await service.DeleteAddress(AddressId);

        Assert.Equal("Xóa thành công", result);
        Assert.True(deletedAddress.IsDeleted);
        Assert.False(deletedAddress.IsDefault);
        Assert.NotNull(deletedAddress.UpdatedAt);

        Assert.False(defaultAddress.IsDeleted);
        Assert.True(defaultAddress.IsDefault);
    }

    // TC05
    [Fact]
    public async Task DeleteAddress_DefaultAddress_WithOtherAddresses_SetNewestRemainingAsDefault()
    {
        var deletedDefaultAddress = CreateAddressEntity(
            id: AddressId,
            receiverName: "deleted default",
            isDefault: true,
            createdAt: DateTimeOffset.UtcNow.AddDays(-5));

        var olderAddress = CreateAddressEntity(
            receiverName: "older address",
            isDefault: false,
            createdAt: DateTimeOffset.UtcNow.AddDays(-3));

        var newestAddress = CreateAddressEntity(
            receiverName: "newest address",
            isDefault: false,
            createdAt: DateTimeOffset.UtcNow.AddDays(-1));

        var addresses = new List<AddressEntity>
        {
            deletedDefaultAddress,
            olderAddress,
            newestAddress
        };

        var service = CreateService(addresses, out _);

        var result = await service.DeleteAddress(AddressId);

        Assert.Equal("Xóa thành công", result);

        Assert.True(deletedDefaultAddress.IsDeleted);
        Assert.False(deletedDefaultAddress.IsDefault);
        Assert.NotNull(deletedDefaultAddress.UpdatedAt);

        Assert.False(olderAddress.IsDefault);
        Assert.True(newestAddress.IsDefault);
    }

    // TC06
    [Fact]
    public async Task DeleteAddress_DefaultAddress_WithoutOtherAddresses_NoNewDefault()
    {
        var deletedDefaultAddress = CreateAddressEntity(
            id: AddressId,
            receiverName: "only default address",
            isDefault: true,
            createdAt: DateTimeOffset.UtcNow.AddDays(-1));

        var addresses = new List<AddressEntity>
        {
            deletedDefaultAddress
        };

        var service = CreateService(addresses, out _);

        var result = await service.DeleteAddress(AddressId);

        Assert.Equal("Xóa thành công", result);

        Assert.True(deletedDefaultAddress.IsDeleted);
        Assert.False(deletedDefaultAddress.IsDefault);
        Assert.NotNull(deletedDefaultAddress.UpdatedAt);

        Assert.DoesNotContain(
            addresses,
            x => x.UserId == UserId && !x.IsDeleted && x.IsDefault);
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
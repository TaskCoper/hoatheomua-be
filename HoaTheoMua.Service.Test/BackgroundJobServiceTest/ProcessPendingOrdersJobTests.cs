using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.BackgroundJobService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Quartz;
using Xunit;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;

namespace HoaTheoMua.Service.Test.BackgroundJobServiceTest;

public class ProcessPendingOrdersJobTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 9, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_ExpiredPendingOrder_ChangesToCancelled()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder("Pending", Now.AddMinutes(-3));

        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        await CreateJob(dbContext).Execute(CreateExecutionContext());

        dbContext.ChangeTracker.Clear();
        var updatedOrder = await dbContext.Orders.SingleAsync(x => x.Id == order.Id);

        Assert.Equal("Cancelled", updatedOrder.Status);
        Assert.Equal(Now, updatedOrder.UpdatedAt);
    }

    [Fact]
    public async Task Execute_NotExpiredOrNotPending_KeepsOrdersUnchanged()
    {
        await using var dbContext = CreateDbContext();//10h00 - 2-> 9h58
        var notExpiredOrder = CreateOrder("Pending", Now.AddMinutes(-1));//9h59
        var paidOrder = CreateOrder("Paid", Now.AddMinutes(-10));//9h50

        await dbContext.Orders.AddRangeAsync(notExpiredOrder, paidOrder);
        await dbContext.SaveChangesAsync();

        await CreateJob(dbContext).Execute(CreateExecutionContext());

        dbContext.ChangeTracker.Clear();
        var orders = await dbContext.Orders
            .Where(x => x.Id == notExpiredOrder.Id || x.Id == paidOrder.Id)
            .ToDictionaryAsync(x => x.Id);

        Assert.Equal("Pending", orders[notExpiredOrder.Id].Status);
        Assert.Null(orders[notExpiredOrder.Id].UpdatedAt);
        Assert.Equal("Paid", orders[paidOrder.Id].Status);
        Assert.Null(orders[paidOrder.Id].UpdatedAt);
    }

    private static ProcessPendingOrdersJob CreateJob(AppDbContext dbContext)
    {
        return new ProcessPendingOrdersJob(
            dbContext,
            Options.Create(new PendingOrderJobOptions
            {
                TimeoutMinutes = 2,
                IntervalMinutes = 1
            }),
            new FixedTimeProvider(Now),
            NullLogger<ProcessPendingOrdersJob>.Instance);
    }

    private static IJobExecutionContext CreateExecutionContext()
    {
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        return context.Object;
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"pending-order-job-{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    private static OrderEntity CreateOrder(string status, DateTimeOffset createdAt)
    {
        return new OrderEntity
        {
            Id = Guid.NewGuid(),
            Ward = 1,
            City = 1,
            Address = "Test address",
            ShortAddress = "Test",
            FullName = "Test User",
            Phone = "0900000000",
            Remarks = string.Empty,
            Status = status,
            TrackNumber = string.Empty,
            OrderTime = 0,
            PhoneReceiver = "0900000000",
            FullNameReceiver = "Test Receiver",
            PaymentMethod = PaymentMethod.VA,
            PaymentGateway = PaymentGateway.Sepay,
            ShippingGateway = ShippingGateway.Ahamove,
            UserId = Guid.NewGuid(),
            TagIds = [229],
            CreatedAt = createdAt
        };
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

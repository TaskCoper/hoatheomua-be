using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Test.VoucherProgramTest;
using Microsoft.EntityFrameworkCore;
using Xunit;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using VoucherService = HoaTheoMua.Service.Voucher.Service;

namespace HoaTheoMua.Service.Test.VoucherTest;

// PrepareOrderVoucher counts uses with JSON queries and locks the voucher row, so it needs real PostgreSQL
public class PrepareOrderVoucherTest
{
    [VoucherPostgresFact]
    public async Task PendingOrderInsideHoldWindow_TakesTheLastUse()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var voucher = await SeedVoucher(db, maxUsageCount: 1);
        db.Orders.Add(Order(voucher.Id, "Pending", DateTimeOffset.UtcNow.AddMinutes(-5)));
        await db.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => new VoucherService(db).PrepareOrderVoucher(voucher.Code, 200_000));

        Assert.Equal("Voucher đã hết lượt sử dụng.", error.Message);
    }

    [VoucherPostgresTheory]
    [InlineData("Pending", 20)] // abandoned: older than the 15-minute hold window
    [InlineData("Cancelled", 1)]
    public async Task AbandonedOrCancelledOrder_DoesNotHoldAUse(string status, int minutesAgo)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var voucher = await SeedVoucher(db, maxUsageCount: 1);
        db.Orders.Add(Order(voucher.Id, status, DateTimeOffset.UtcNow.AddMinutes(-minutesAgo)));
        await db.SaveChangesAsync();

        var snapshot = await new VoucherService(db).PrepareOrderVoucher(voucher.Code, 200_000);

        Assert.Equal(voucher.Id, snapshot.VoucherId);
        Assert.Equal(20_000, snapshot.DiscountAmount);
    }

    [VoucherPostgresFact]
    public async Task UsedCountColumn_IsResyncedToPaidOrdersOnly()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var voucher = await SeedVoucher(db, maxUsageCount: 10, usedCount: 7);
        db.Orders.AddRange(
            Order(voucher.Id, "Paid", DateTimeOffset.UtcNow.AddDays(-1)),
            Order(voucher.Id, "Paid", DateTimeOffset.UtcNow.AddDays(-1)),
            Order(voucher.Id, "Pending", DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        await new VoucherService(db).PrepareOrderVoucher(voucher.Code, 200_000);

        db.ChangeTracker.Clear();
        Assert.Equal(2, (await db.Vouchers.SingleAsync()).UsedCount);
    }

    [VoucherPostgresFact]
    public async Task InactiveVoucherWithSameCode_IsIgnored()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var inactive = Voucher("SAME", 1, active: false);
        inactive.CreatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        db.Vouchers.Add(inactive);
        await db.SaveChangesAsync();
        var active = await SeedVoucher(db, maxUsageCount: 1, code: "SAME");

        var snapshot = await new VoucherService(db).PrepareOrderVoucher("same", 200_000);

        Assert.Equal(active.Id, snapshot.VoucherId);
    }

    [VoucherPostgresFact]
    public async Task ConcurrentOrders_SecondWaitsForTheFirst_ThenSeesItsPendingOrder()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        Repository.Entity.Voucher voucher;
        await using (var seed = fixture.Context())
            voucher = await SeedVoucher(seed, maxUsageCount: 1);

        await using var firstDb = fixture.Context();
        await using var secondDb = fixture.Context();
        await using var firstTransaction = await firstDb.Database.BeginTransactionAsync();
        var firstSnapshot = await new VoucherService(firstDb).PrepareOrderVoucher(voucher.Code, 200_000);

        await using var secondTransaction = await secondDb.Database.BeginTransactionAsync();
        var second = new VoucherService(secondDb).PrepareOrderVoucher(voucher.Code, 200_000);

        // The second order blocks on the row lock while the first is still saving its order
        await Task.Delay(500);
        Assert.False(second.IsCompleted);

        firstDb.Orders.Add(Order(firstSnapshot.VoucherId, "Pending", DateTimeOffset.UtcNow));
        await firstDb.SaveChangesAsync();
        await firstTransaction.CommitAsync();

        await Assert.ThrowsAsync<ConflictException>(() => second.WaitAsync(TimeSpan.FromSeconds(30)));
    }

    private static async Task<Repository.Entity.Voucher> SeedVoucher(
        Repository.AppDbContext db, int maxUsageCount, int usedCount = 0, string code = "SAVE10")
    {
        var voucher = Voucher(code, maxUsageCount, active: true);
        voucher.UsedCount = usedCount;
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        return voucher;
    }

    private static Repository.Entity.Voucher Voucher(string code, int maxUsageCount, bool active) => new()
    {
        Id = Guid.NewGuid(), Code = code, IsActive = active, Visibility = VoucherVisibility.Public,
        DiscountType = VoucherDiscountType.Percent, DiscountValue = 10, MaxUsageCount = maxUsageCount,
        StartAt = DateTimeOffset.UtcNow.AddDays(-1), EndAt = DateTimeOffset.UtcNow.AddDays(1),
        CreatedAt = DateTimeOffset.UtcNow, IsDeleted = false
    };

    // Snapshot serialized the same way CreateOrder stores it
    private static OrderEntity Order(Guid voucherId, string status, DateTimeOffset createdAt) => new()
    {
        Id = Guid.NewGuid(),
        Ward = 1,
        City = 1,
        Address = "Test address",
        ShortAddress = "Test",
        FullName = "Test customer",
        Phone = "0900000000",
        Remarks = "",
        Status = status,
        TrackNumber = "TEST-ORDER",
        OrderTime = 0,
        PhoneReceiver = "0900000000",
        FullNameReceiver = "Test receiver",
        PaymentGateway = PaymentGateway.Sepay,
        ShippingGateway = ShippingGateway.Ahamove,
        VoucherSnapshot = System.Text.Json.JsonSerializer.Serialize(new Order.Request.VoucherSnapshot
        {
            VoucherId = voucherId, Code = "SAVE10", DiscountType = "percent", DiscountValue = 10, DiscountAmount = 20_000
        }),
        CreatedAt = createdAt
    };
}

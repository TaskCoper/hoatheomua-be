using System.Reflection;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Test.VoucherProgramTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HoaTheoMua.Service.Test.OrderTest;

public class OrderTransactionPostgres
{
    [VoucherPostgresTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SavePendingOrder_RespectsTransactionOwnership(bool hasOuterTransaction, bool rollback)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        db.Configs.Add(new Repository.Entity.Config
        {
            Id = Guid.NewGuid(), Key = "nhanhvn.order_tags", Group = "nhanhvn", Kind = "setting",
            Value = """{"value":[229],"options":[{"label":"8h - 10h","value":229}],"slotLimits":{"229":5}}""",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        await using var outer = hasOuterTransaction ? await db.Database.BeginTransactionAsync() : null;
        var order = new Repository.Entity.Order
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = "Pending", TrackNumber = "",
            Ward = 1, City = 1, OrderTime = 0,
            Address = "Test", ShortAddress = "Test", FullName = "Test", Phone = "0900000000",
            PhoneReceiver = "0900000000", FullNameReceiver = "Test", Remarks = "", TagIds = [229],
            PaymentGateway = PaymentGateway.Sepay, ShippingGateway = ShippingGateway.Ahamove,
            CreatedAt = DateTimeOffset.UtcNow
        };
        using var http = new HttpClient();
        var service = new Service.Order.Service(db, null!, null!, http, null!,
            new ConfigurationBuilder().Build(), null!);
        var save = typeof(Service.Order.Service).GetMethod("SavePendingOrderWithSlotValidation",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)save.Invoke(service,
            [order, Array.Empty<OrderDetails>(), DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)), 229, CancellationToken.None])!;

        await using var verify = fixture.Context();
        if (outer != null)
        {
            Assert.Same(outer, db.Database.CurrentTransaction);
            Assert.False(await verify.Orders.AnyAsync(x => x.Id == order.Id));
            if (rollback)
                await outer.RollbackAsync();
            else
                await outer.CommitAsync();
        }
        else
        {
            Assert.Null(db.Database.CurrentTransaction);
        }

        Assert.Equal(!rollback, await verify.Orders.AnyAsync(x => x.Id == order.Id));
    }
}

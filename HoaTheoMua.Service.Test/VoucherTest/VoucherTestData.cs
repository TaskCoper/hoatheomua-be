using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.Test.VoucherTest;

internal static class VoucherTestData
{
    internal static AppDbContext Database() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    internal static Repository.Entity.Voucher Voucher(
        string code = "SAVE10", bool active = true, VoucherVisibility visibility = VoucherVisibility.Public)
    {
        var now = DateTimeOffset.UtcNow;
        return new Repository.Entity.Voucher
        {
            Id = Guid.NewGuid(), Code = code, Description = "Test voucher", IsActive = active,
            Visibility = visibility, DiscountType = VoucherDiscountType.Percent, DiscountValue = 10,
            MaxDiscountAmount = 50_000, MaxUsageCount = 10, UsedCount = 0,
            StartAt = now.AddDays(-1), EndAt = now.AddDays(1), MinOrderAmount = 100_000,
            MaxOrderAmount = 2_000_000, CreatedAt = now, IsDeleted = false
        };
    }

    internal static Repository.Entity.VoucherProgram Program(bool active = true) => new()
    {
        Id = Guid.NewGuid(), Name = "Autumn sale", IsActive = active,
        StartAt = DateTimeOffset.UtcNow.AddDays(-1), EndAt = DateTimeOffset.UtcNow.AddDays(1),
        MinOrderAmount = 100_000, MaxOrderAmount = 2_000_000,
        CreatedAt = DateTimeOffset.UtcNow, IsDeleted = false
    };
}

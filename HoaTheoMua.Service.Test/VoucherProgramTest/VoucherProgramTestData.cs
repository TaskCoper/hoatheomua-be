using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.VoucherProgram;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

internal static class VoucherProgramTestData
{
    public static AppDbContext Database() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public static Repository.Entity.VoucherProgram Program(bool active = false, bool deleted = false) => new()
    {
        Id = Guid.NewGuid(), Name = "20/10", IsActive = active, IsDeleted = deleted,
        StartAt = DateTimeOffset.UtcNow, EndAt = DateTimeOffset.UtcNow.AddDays(7),
        MinOrderAmount = 200000, MaxOrderAmount = 2000000, CreatedAt = DateTimeOffset.UtcNow
    };

    public static Voucher Voucher(Guid? programId, string code, bool active = false, bool deleted = false) => new()
    {
        Id = Guid.NewGuid(), ProgramId = programId, Code = code, IsActive = active, IsDeleted = deleted,
        DiscountType = VoucherDiscountType.Cash, DiscountValue = 10000,
        Visibility = VoucherVisibility.Private, MaxUsageCount = 5, UsedCount = 2,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public static Request.GenerateProgramVouchersRequest GenerateRequest(int quantity = 10) => new()
    {
        CodePrefix = "HTM", CodeLength = 7, CodeSuffix = "26", Quantity = quantity,
        MaxUsageCount = 2, DiscountType = "percent", DiscountValue = 20,
        MaxDiscountAmount = 100000, Description = "Quà tặng 20/10"
    };

    internal sealed class SequenceGenerator(params string[] codes) : IVoucherCodeGenerator
    {
        private int _index;
        public int Calls => _index;
        public string Generate(string? prefix, int length, string? suffix) => codes[Math.Min(_index++, codes.Length - 1)];
    }
}

using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FeatureService = HoaTheoMua.Service.VoucherProgram.Service;
using static HoaTheoMua.Service.Test.VoucherProgramTest.VoucherProgramTestData;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class GenerateProgramVouchers
{
    [Theory]
    [InlineData("cash", false)]
    [InlineData("percent", true)]
    public async Task GeneratesExactQuantity_WithSharedPolicy_PrivateInactiveAndInheritedSchedule(string type, bool active)
    {
        await using var db = Database();
        var program = Program(active);
        db.VoucherPrograms.Add(program);
        await db.SaveChangesAsync();
        var request = GenerateRequest(1000);
        request.DiscountType = type;
        request.MaxDiscountAmount = type == "percent" ? 100000 : null;
        var before = DateTimeOffset.UtcNow;

        var result = await new FeatureService(db).GenerateProgramVouchers(program.Id, request);
        db.ChangeTracker.Clear();
        var vouchers = await db.Vouchers.ToListAsync();

        Assert.Equal(program.Id, result.ProgramId);
        Assert.Equal(1000, result.CreatedCount);
        Assert.Equal(1000, result.Codes.Distinct().Count());
        Assert.Equal(result.Codes.Order(), vouchers.Select(x => x.Code).Order());
        Assert.All(vouchers, voucher =>
        {
            Assert.Matches(@"\AHTM[ABCDEFGHJKLMNPQRSTUVWXYZ23456789]{7}26\z", voucher.Code);
            Assert.Equal(program.Id, voucher.ProgramId);
            Assert.Equal(VoucherVisibility.Private, voucher.Visibility);
            Assert.Equal(type == "cash" ? VoucherDiscountType.Cash : VoucherDiscountType.Percent, voucher.DiscountType);
            Assert.Equal(request.DiscountValue, voucher.DiscountValue);
            Assert.Equal(request.MaxDiscountAmount, voucher.MaxDiscountAmount);
            Assert.Equal(request.MaxUsageCount, voucher.MaxUsageCount);
            Assert.Equal(request.Description, voucher.Description);
            Assert.False(voucher.IsActive);
            Assert.False(voucher.IsDeleted);
            Assert.Equal(0, voucher.UsedCount);
            Assert.Null(voucher.StartAt);
            Assert.Null(voucher.EndAt);
            Assert.Null(voucher.MinOrderAmount);
            Assert.Null(voucher.MaxOrderAmount);
            Assert.Null(voucher.UpdatedAt);
            Assert.InRange(voucher.CreatedAt, before, DateTimeOffset.UtcNow);
        });
        Assert.Equal(active, (await db.VoucherPrograms.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Collision_WithDeletedCodeAndWithinBatch_IsRetried_AndFurtherBatchesAreAllowed()
    {
        await using var db = Database();
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(Voucher(null, "TAKEN", deleted: true));
        await db.SaveChangesAsync();
        var generator = new SequenceGenerator("TAKEN", "NEWAA", "NEWAA", "NEWBB", "NEWCC");
        var service = new FeatureService(db, generator);

        var first = await service.GenerateProgramVouchers(program.Id, GenerateRequest(2));
        var second = await service.GenerateProgramVouchers(program.Id, GenerateRequest(1));

        Assert.Equal(new[] { "NEWAA", "NEWBB" }, first.Codes);
        Assert.Equal(new[] { "NEWCC" }, second.Codes);
        Assert.Equal(3, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id));
        Assert.True((await db.Vouchers.SingleAsync(x => x.Code == "TAKEN")).IsDeleted);
    }

    [Fact]
    public async Task CollisionExhaustion_RollsBackPartialBatchAfterFiveRounds()
    {
        await using var db = Database();
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(Voucher(null, "TAKEN", deleted: true));
        await db.SaveChangesAsync();
        var generator = new SequenceGenerator("NEWAA", "TAKEN");

        await Assert.ThrowsAsync<ServerException>(() => new FeatureService(db, generator)
            .GenerateProgramVouchers(program.Id, GenerateRequest(2)));

        Assert.Equal(11, generator.Calls); // 3 candidates in round one, then 2 per remaining round.
        Assert.Empty(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync());
        Assert.Single(await db.Vouchers.ToListAsync());
    }

    [Fact]
    public async Task ProgramLimit_IncludesDeletedCodes_AllowsExactly10000_AndRejectsNextBatch()
    {
        await using var db = Database();
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.AddRange(Enumerable.Range(0, 9999).Select(i => Voucher(program.Id, $"OLD{i}", deleted: i == 0)));
        await db.SaveChangesAsync();
        var service = new FeatureService(db);

        Assert.Equal(1, (await service.GenerateProgramVouchers(program.Id, GenerateRequest(1))).CreatedCount);
        var error = await Assert.ThrowsAsync<ConflictException>(() => service.GenerateProgramVouchers(program.Id, GenerateRequest(1)));

        Assert.Equal("Voucher program cannot exceed 10000 vouchers.", error.Message);
        Assert.Equal(10000, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id));
    }
}

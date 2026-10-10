using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using FeatureService = HoaTheoMua.Service.VoucherProgram.Service;
using static HoaTheoMua.Service.Test.VoucherProgramTest.VoucherProgramTestData;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class GenerateProgramVouchers
{
    [VoucherPostgresTheory]
    [InlineData("cash", false)]
    [InlineData("percent", true)]
    public async Task GeneratesExactQuantity_WithSharedPolicy_PrivateActiveAndInheritedSchedule(string type, bool active)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
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
            Assert.True(voucher.IsActive);
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

    [VoucherPostgresFact]
    public async Task Collision_WithActiveCodeAndWithinBatch_IsRetried_AndFurtherBatchesAreAllowed()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(Voucher(null, "TAKEN", active: true));
        await db.SaveChangesAsync();
        var generator = new SequenceGenerator("TAKEN", "NEWAA", "NEWAA", "NEWBB", "NEWCC");
        var service = new FeatureService(db, generator);

        var first = await service.GenerateProgramVouchers(program.Id, GenerateRequest(2));
        var second = await service.GenerateProgramVouchers(program.Id, GenerateRequest(1));

        Assert.Equal(new[] { "NEWAA", "NEWBB" }, first.Codes);
        Assert.Equal(new[] { "NEWCC" }, second.Codes);
        Assert.Equal(3, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id));
        Assert.True((await db.Vouchers.SingleAsync(x => x.Code == "TAKEN")).IsActive);
    }

    [VoucherPostgresFact]
    public async Task CollisionsBeyondFiveRounds_ContinueUntilFullBatchIsSaved()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(Voucher(null, "TAKEN", active: true));
        await db.SaveChangesAsync();
        var generator = new SequenceGenerator(
            new[] { "NEWAA" }.Concat(Enumerable.Repeat("TAKEN", 12)).Append("NEWBB").ToArray());

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var result = await new FeatureService(db, generator)
            .GenerateProgramVouchers(program.Id, GenerateRequest(2), timeout.Token);

        Assert.True(generator.Calls > 11); // Năm vòng đầu chỉ có 11 lần sinh, vẫn chưa đủ mã.
        Assert.Equal(new[] { "NEWAA", "NEWBB" }, result.Codes);
        Assert.Equal(2, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id && x.IsActive));
        Assert.Equal(3, await db.Vouchers.CountAsync());
    }

    [VoucherPostgresFact]
    public async Task Generation_AllowsMoreThan10000Total_WhenOldCodesAreInactive()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program(true);
        db.VoucherPrograms.Add(program);
        db.Vouchers.AddRange(Enumerable.Range(0, 10000).Select(i => Voucher(program.Id, $"OLD{i}")));
        await db.SaveChangesAsync();
        var service = new FeatureService(db);

        Assert.Equal(1, (await service.GenerateProgramVouchers(program.Id, GenerateRequest(1))).CreatedCount);
        Assert.Equal(1, (await service.GenerateProgramVouchers(program.Id, GenerateRequest(1))).CreatedCount);

        Assert.Equal(10002, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id));
        Assert.Equal(2, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id && x.IsActive));
    }

    [VoucherPostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generation_ReusesInactiveCodes_FromSameProgramOtherProgramAndStandalone(bool deleted)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        var other = Program();
        db.VoucherPrograms.AddRange(program, other);
        db.Vouchers.AddRange(Voucher(program.Id, "SAME1", deleted: deleted),
            Voucher(other.Id, "OTHER", deleted: deleted), Voucher(null, "ALONE", deleted: deleted));
        await db.SaveChangesAsync();

        var result = await new FeatureService(db, new SequenceGenerator("SAME1", "OTHER", "ALONE", "EXTRA"))
            .GenerateProgramVouchers(program.Id, GenerateRequest(3));

        Assert.Equal(new[] { "SAME1", "OTHER", "ALONE" }, result.Codes);
        Assert.Equal(6, await db.Vouchers.CountAsync());
        Assert.Equal(2, await db.Vouchers.CountAsync(x => x.Code == "SAME1"));
        Assert.Equal(3, await db.Vouchers.CountAsync(x => x.IsActive));
    }

    [VoucherPostgresFact]
    public async Task Generation_StagesCollisionRoundsBeforeSaving_AndLeavesNoTrackedBatch()
    {
        var observer = new VoucherSaveObserver();
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context(observer);
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(Voucher(null, "TAKEN", active: true));
        await db.SaveChangesAsync();
        observer.VoucherSaves = 0;
        var generator = new SequenceGenerator("TAKEN", "NEWAA", "NEWAA", "NEWBB", "NEWCC");

        var result = await new FeatureService(db, generator).GenerateProgramVouchers(program.Id, GenerateRequest(2));

        Assert.Equal(new[] { "NEWAA", "NEWBB" }, result.Codes);
        Assert.Equal(1, observer.VoucherSaves);
        Assert.DoesNotContain(db.ChangeTracker.Entries<Repository.Entity.Voucher>(), x => x.Entity.ProgramId == program.Id);
        Assert.Equal(2, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id));
    }

    [VoucherPostgresFact]
    public async Task UnexpectedSaveFailure_DoesNotLeakFailedVouchersIntoNextRequest()
    {
        var observer = new VoucherSaveObserver();
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context(observer);
        var program = Program();
        db.VoucherPrograms.Add(program);
        await db.SaveChangesAsync();
        observer.FailNextVoucherSave = true;
        var service = new FeatureService(db, new SequenceGenerator("FAIL1", "EXTRA", "NEWAA", "NEWBB"));

        var error = await Assert.ThrowsAsync<DbUpdateException>(() =>
            service.GenerateProgramVouchers(program.Id, GenerateRequest(1)));

        Assert.Equal("Forced voucher write failure.", error.Message);
        Assert.Empty(await db.Vouchers.ToListAsync());
        Assert.Empty(db.ChangeTracker.Entries<Repository.Entity.Voucher>());
        var result = await service.GenerateProgramVouchers(program.Id, GenerateRequest(1));
        Assert.Equal(new[] { "NEWAA" }, result.Codes);
        Assert.Equal("NEWAA", (await db.Vouchers.SingleAsync()).Code);
    }

    [VoucherPostgresFact]
    public async Task Generation_AllowsExactly10000Active_RejectsExcess_AndAllowsMoreAfterDeactivation()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program(true);
        db.VoucherPrograms.Add(program);
        db.Vouchers.AddRange(Enumerable.Range(0, 9999).Select(i => Voucher(program.Id, $"ACTIVE{i}", active: true)));
        await db.SaveChangesAsync();
        var service = new FeatureService(db);

        Assert.Equal(1, (await service.GenerateProgramVouchers(program.Id, GenerateRequest(1))).CreatedCount);
        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            service.GenerateProgramVouchers(program.Id, GenerateRequest(1)));
        Assert.Equal("Voucher program cannot exceed 10000 active vouchers.", error.Message);
        Assert.Equal(10000, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id && x.IsActive));

        await service.UpdateVoucherProgram(program.Id, new() { IsActive = false });
        Assert.Equal(1, (await service.GenerateProgramVouchers(program.Id, GenerateRequest(1))).CreatedCount);
        Assert.Equal(10001, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id));
        Assert.Equal(1, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id && x.IsActive));
    }

    private sealed class VoucherSaveObserver : SaveChangesInterceptor
    {
        public int VoucherSaves { get; set; }
        public bool FailNextVoucherSave { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<Repository.Entity.Voucher>()
                .Any(x => x.State == EntityState.Added))
            {
                VoucherSaves++;
                if (FailNextVoucherSave)
                {
                    FailNextVoucherSave = false;
                    throw new DbUpdateException("Forced voucher write failure.");
                }
            }

            return ValueTask.FromResult(result);
        }
    }
}

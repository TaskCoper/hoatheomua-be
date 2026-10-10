using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
using FeatureService = HoaTheoMua.Service.VoucherProgram.Service;
using static HoaTheoMua.Service.Test.VoucherProgramTest.VoucherProgramTestData;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class ActiveVoucherRules
{
    [Fact]
    public void ActiveCodeIndex_AndMigration_AreGlobalAndFiltered()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused").Options);
        var entity = db.Model.FindEntityType(typeof(Repository.Entity.Voucher))!;
        var index = Assert.Single(entity.GetIndexes(), x => x.Properties.Count == 1 && x.Properties[0].Name == "Code");
        Assert.True(index.IsUnique);
        Assert.Equal("\"IsActive\" = true", index.GetFilter());

        // Sinh SQL để kiểm tra migration, không kết nối hay thay đổi DB.
        var migrator = db.GetService<IMigrator>();
        var sql = migrator.GenerateScript("20261007021252_voucher", "20261008120000_UniqueActiveVoucherCodes");
        Assert.Contains("DROP INDEX \"IX_Vouchers_Code\"", sql);
        Assert.Contains("CREATE UNIQUE INDEX \"IX_Vouchers_Code\" ON \"Vouchers\" (\"Code\") WHERE \"IsActive\" = true", sql);
        var down = migrator.GenerateScript("20261008120000_UniqueActiveVoucherCodes", "20261007021252_voucher");
        Assert.Contains("CREATE UNIQUE INDEX \"IX_Vouchers_Code\" ON \"Vouchers\" (\"Code\");", down);
    }

    [VoucherPostgresFact]
    public async Task Activation_Allows10000_Rejects10001_AndStillAllowsDeactivation()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.AddRange(Enumerable.Range(0, 10000).Select(i => Voucher(program.Id, $"LIMIT{i}")));
        await db.SaveChangesAsync();
        var service = new FeatureService(db);

        await service.UpdateVoucherProgram(program.Id, new() { IsActive = true });
        Assert.Equal(10000, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id && x.IsActive));
        // Chuẩn bị thêm một mã inactive để kiểm tra giới hạn khi kích hoạt cả chương trình.
        db.Vouchers.Add(Voucher(program.Id, "EXTRA"));
        await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateVoucherProgram(program.Id, new() { IsActive = true, Name = "Must not change" }));
        Assert.Equal("Voucher program cannot exceed 10000 active vouchers.", error.Message);
        db.ChangeTracker.Clear();
        Assert.Equal("20/10", (await db.VoucherPrograms.SingleAsync()).Name);
        Assert.Equal(10000, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id && x.IsActive));

        await service.UpdateVoucherProgram(program.Id, new() { IsActive = false });
        Assert.Equal(10001, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id));
        Assert.False(await db.Vouchers.AnyAsync(x => x.ProgramId == program.Id && x.IsActive));
    }

    [VoucherPostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Activation_RejectsCodeActiveElsewhere_AndRollsBackWholeBatch(bool standalone)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        var other = Program(true);
        db.VoucherPrograms.AddRange(program, other);
        db.Vouchers.AddRange(Voucher(program.Id, "SHARED"), Voucher(program.Id, "UNIQUE"),
            Voucher(standalone ? null : other.Id, "SHARED", active: true));
        await db.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            new FeatureService(db).UpdateVoucherProgram(program.Id, new() { IsActive = true, Name = "Must not change" }));

        Assert.Equal("Voucher codes must be unique among active vouchers across the system.", error.Message);
        db.ChangeTracker.Clear();
        Assert.False(await db.Vouchers.AnyAsync(x => x.ProgramId == program.Id && x.IsActive));
        var saved = await db.VoucherPrograms.SingleAsync(x => x.Id == program.Id);
        Assert.False(saved.IsActive);
        Assert.Equal("20/10", saved.Name);
        Assert.Null(saved.UpdatedAt);
    }

    [VoucherPostgresFact]
    public async Task Activation_RejectsDuplicateInactiveCodesWithinProgram()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.AddRange(Voucher(program.Id, "SAME1"), Voucher(program.Id, "SAME1"));
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() =>
            new FeatureService(db).UpdateVoucherProgram(program.Id, new() { IsActive = true }));

        Assert.Equal(2, await db.Vouchers.CountAsync());
        Assert.False(await db.Vouchers.AnyAsync(x => x.IsActive));
    }

    [VoucherPostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeactivationOrDeletion_ReleasesCodeForAnotherProgram(bool delete)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var first = Program(true);
        var second = Program();
        db.VoucherPrograms.AddRange(first, second);
        db.Vouchers.AddRange(Voucher(first.Id, "REUSE", active: true), Voucher(second.Id, "REUSE"));
        await db.SaveChangesAsync();
        var service = new FeatureService(db);

        if (delete)
            await service.DeleteVoucherProgram(first.Id);
        else
            await service.UpdateVoucherProgram(first.Id, new() { IsActive = false });
        await service.UpdateVoucherProgram(second.Id, new() { IsActive = true });

        db.ChangeTracker.Clear();
        var vouchers = await db.Vouchers.ToListAsync();
        Assert.False(vouchers.Single(x => x.ProgramId == first.Id).IsActive);
        Assert.Equal(delete, vouchers.Single(x => x.ProgramId == first.Id).IsDeleted);
        Assert.True(vouchers.Single(x => x.ProgramId == second.Id).IsActive);
    }
}

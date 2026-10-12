using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.VoucherProgram;
using HoaTheoMua.Service.VoucherProgram.Validate;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FeatureService = HoaTheoMua.Service.VoucherProgram.Service;
using static HoaTheoMua.Service.Test.VoucherProgramTest.VoucherProgramTestData;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class ManageVoucherProgram
{
    [VoucherPostgresTheory]
    [InlineData(0, 0, null)]
    [InlineData(200000, 0, null)]
    [InlineData(3000000, 0, null)]
    [InlineData(null, 0, null)]
    [InlineData(null, null, 2000000)]
    [InlineData(null, 1000000, 1000000)]
    public async Task Update_ZeroMaxRemovesUpperLimit_WhileNullKeepsExistingLimit(int? min, int? max, int? expectedMax)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        db.VoucherPrograms.Add(program);
        await db.SaveChangesAsync();
        var request = new Request.UpdateVoucherProgramRequest
        {
            Name = program.Name, MinOrderAmount = min, MaxOrderAmount = max
        };
        Assert.True(new UpdateVoucherProgramRequestValidator().Validate(request).IsValid);

        var result = await new FeatureService(db).UpdateVoucherProgram(program.Id, request);

        db.ChangeTracker.Clear();
        var saved = await db.VoucherPrograms.SingleAsync();
        Assert.Equal((decimal?)expectedMax, saved.MaxOrderAmount);
        Assert.Equal((decimal?)min ?? 200000m, saved.MinOrderAmount);
    }

    [VoucherPostgresFact]
    public async Task Update_ChangesOnlySuppliedFields_NormalizesUtc_AndPreservesVoucherStatesWhenActiveIsOmitted()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program(true);
        var voucher = VoucherProgramTestData.NewVoucher(program.Id, "FIRST", false);
        var originalEnd = program.EndAt;
        var createdAt = program.CreatedAt;
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        var start = program.StartAt.AddDays(-1).ToOffset(TimeSpan.FromHours(7));

        var result = await new FeatureService(db).UpdateVoucherProgram(program.Id, new()
        {
            Name = "  Tên mới  ", StartAt = start.ToString("O"), MinOrderAmount = 0
        });

        db.ChangeTracker.Clear();
        var saved = await db.VoucherPrograms.SingleAsync();
        var savedVoucher = await db.Vouchers.SingleAsync();
        Assert.Equal("Tên mới", saved.Name);
        Assert.Equal(start.ToUniversalTime(), saved.StartAt);
        Assert.Equal(TimeSpan.Zero, saved.StartAt.Offset);
        Assert.Equal(originalEnd, saved.EndAt);
        Assert.Equal(0m, saved.MinOrderAmount);
        Assert.Equal(2000000m, saved.MaxOrderAmount);
        Assert.Equal(createdAt, saved.CreatedAt);
        Assert.True(saved.IsActive);
        Assert.False(savedVoucher.IsActive);
        Assert.Null(savedVoucher.UpdatedAt);
        Assert.NotNull(saved.UpdatedAt);
        Assert.Equal("Update voucher program successfully", result);
    }

    [VoucherPostgresTheory]
    [InlineData("startAt", "startAt", "Start time must be earlier than the current end time")]
    [InlineData("endAt", "endAt", "Thời gian kết thúc phải sau thời gian bắt đầu hiện tại của chương trình.")]
    [InlineData("minOrderAmount", "minOrderAmount", "Min order amount cannot exceed the current max order amount")]
    [InlineData("maxOrderAmount", "maxOrderAmount", "Max order amount cannot be less than the current min order amount")]
    public async Task Update_ValidatesMergedValuesBeforeChangingProgramOrVouchers(string changedField, string errorField, string message)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(VoucherProgramTestData.NewVoucher(program.Id, "FIRST", false));
        await db.SaveChangesAsync();
        var request = new Request.UpdateVoucherProgramRequest { IsActive = true, Name = "Must not be saved" };
        switch (changedField)
        {
            case "startAt": request.StartAt = program.EndAt!.Value.ToString("O"); break;
            case "endAt": request.EndAt = program.StartAt.ToString("O"); break;
            case "minOrderAmount": request.MinOrderAmount = 2000001; break;
            case "maxOrderAmount": request.MaxOrderAmount = 199999; break;
        }

        var error = await Assert.ThrowsAsync<ValidationExceptionErrorV1>(() => new FeatureService(db)
            .UpdateVoucherProgram(program.Id, request));

        var validationError = Assert.Single(error.Errors);
        Assert.Equal(errorField, validationError.Field);
        Assert.Equal(message, validationError.Message);
        db.ChangeTracker.Clear();
        var saved = await db.VoucherPrograms.SingleAsync();
        Assert.Equal("20/10", saved.Name);
        Assert.False(saved.IsActive);
        Assert.Null(saved.UpdatedAt);
        Assert.False((await db.Vouchers.SingleAsync()).IsActive);
        Assert.Null((await db.Vouchers.SingleAsync()).UpdatedAt);
    }

    [VoucherPostgresTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Active_SetsProgramAndEveryLiveVoucher_WithoutChangingUnrelatedOrDeletedVouchers(bool active)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program(!active);
        var first = VoucherProgramTestData.NewVoucher(program.Id, "FIRST", true);
        var second = VoucherProgramTestData.NewVoucher(program.Id, "SECOND", false);
        var deleted = VoucherProgramTestData.NewVoucher(program.Id, "DELETED", !active, true);
        var unrelated = VoucherProgramTestData.NewVoucher(null, "OTHER", !active);
        db.VoucherPrograms.Add(program);
        db.Vouchers.AddRange(first, second, deleted, unrelated);
        await db.SaveChangesAsync();
        var before = DateTimeOffset.UtcNow;

        var result = await new FeatureService(db).UpdateVoucherProgram(program.Id, new() { IsActive = active });

        db.ChangeTracker.Clear();
        var saved = await db.VoucherPrograms.SingleAsync();
        Assert.Equal(active, saved.IsActive);
        Assert.InRange(saved.UpdatedAt!.Value, before, DateTimeOffset.UtcNow);
        var all = await db.Vouchers.ToDictionaryAsync(x => x.Code);
        foreach (var code in new[] { "FIRST", "SECOND" })
        {
            Assert.Equal(active, all[code].IsActive);
            Assert.Equal(saved.UpdatedAt, all[code].UpdatedAt);
            Assert.Equal(2, all[code].UsedCount);
        }
        Assert.Equal(!active, all["DELETED"].IsActive);
        Assert.Equal(!active, all["OTHER"].IsActive);
        Assert.Null(all["DELETED"].UpdatedAt);
        Assert.Null(all["OTHER"].UpdatedAt);
    }

    [VoucherPostgresFact]
    public async Task Delete_SoftDeletesEntireProgram_PreservesIssuedCodesAndUsage_AndLeavesOtherVouchers()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program(true);
        db.VoucherPrograms.Add(program);
        db.Vouchers.AddRange(VoucherProgramTestData.NewVoucher(program.Id, "FIRST", true), VoucherProgramTestData.NewVoucher(program.Id, "DELETED", false, true),
            VoucherProgramTestData.NewVoucher(null, "OTHER", true));
        await db.SaveChangesAsync();

        Assert.Equal("Program deleted", await new FeatureService(db).DeleteVoucherProgram(program.Id));

        db.ChangeTracker.Clear();
        var saved = await db.VoucherPrograms.SingleAsync();
        Assert.True(saved.IsDeleted);
        Assert.NotNull(saved.UpdatedAt);
        Assert.Equal(3, await db.Vouchers.CountAsync());
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), voucher =>
        {
            Assert.True(voucher.IsDeleted);
            if (voucher.Code == "DELETED")
                Assert.Null(voucher.UpdatedAt);
            else
                Assert.Equal(saved.UpdatedAt, voucher.UpdatedAt);
            Assert.Equal(2, voucher.UsedCount);
        });
        Assert.False((await db.Vouchers.SingleAsync(x => x.Code == "OTHER")).IsDeleted);
        Assert.Equal(new[] { "DELETED", "FIRST", "OTHER" }, await db.Vouchers.OrderBy(x => x.Code).Select(x => x.Code).ToArrayAsync());
    }

    [VoucherPostgresTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AllMutations_ReturnNotFound_ForMissingOrDeletedProgram(bool deleted)
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program(deleted: deleted);
        if (deleted)
        {
            db.VoucherPrograms.Add(program);
            await db.SaveChangesAsync();
        }
        var service = new FeatureService(db);

        var active = await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateVoucherProgram(program.Id, new() { IsActive = true }));
        var delete = await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteVoucherProgram(program.Id));
        var generate = await Assert.ThrowsAsync<NotFoundException>(() => service.GenerateProgramVouchers(program.Id, GenerateRequest()));

        Assert.Equal(deleted ? "Chương trình voucher đã bị xóa." : "Không tìm thấy chương trình voucher.", active.Message);
        Assert.Equal(active.Message, delete.Message);
        Assert.Equal(active.Message, generate.Message);
        Assert.Empty(await db.Vouchers.ToListAsync());
    }

    [VoucherPostgresFact]
    public async Task Update_UsesCurrentDatabaseValues_InNewRequestContext()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        db.VoucherPrograms.Add(program);
        await db.SaveChangesAsync();
        await using (var other = fixture.Context())
        {
            var updated = await other.VoucherPrograms.SingleAsync();
            updated.MinOrderAmount = 500000;
            updated.MaxOrderAmount = 700000;
            await other.SaveChangesAsync();
        }

        // Mô phỏng request mới: không mang entity được theo dõi từ bước chuẩn bị dữ liệu.
        db.ChangeTracker.Clear();
        var result = await new FeatureService(db).UpdateVoucherProgram(program.Id, new() { Name = "New name" });

        db.ChangeTracker.Clear();
        var saved = await db.VoucherPrograms.SingleAsync();
        Assert.Equal(500000m, saved.MinOrderAmount);
        Assert.Equal(700000m, saved.MaxOrderAmount);
    }

    [VoucherPostgresFact]
    public async Task ProgramDeletedByAnotherRequest_CannotBeMutated()
    {
        await using var fixture = await VoucherProgramPostgres.PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program();
        db.VoucherPrograms.Add(program);
        await db.SaveChangesAsync();
        await using (var other = fixture.Context())
        {
            await new FeatureService(other).DeleteVoucherProgram(program.Id);
        }
        db.ChangeTracker.Clear();
        var service = new FeatureService(db);

        var error = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateVoucherProgram(program.Id, new() { IsActive = true }));
        Assert.Equal("Chương trình voucher đã bị xóa.", error.Message);
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteVoucherProgram(program.Id));
        await Assert.ThrowsAsync<NotFoundException>(() => service.GenerateProgramVouchers(program.Id, GenerateRequest()));
        Assert.Empty(await db.Vouchers.ToListAsync());
    }
}

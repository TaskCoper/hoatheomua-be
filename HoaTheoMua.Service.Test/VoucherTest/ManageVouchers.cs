using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Voucher;
using Xunit;
using VoucherService = HoaTheoMua.Service.Voucher.Service;

namespace HoaTheoMua.Service.Test.VoucherTest;

public class ManageVouchersTest
{
    [Fact]
    public async Task GetVouchers_ReturnsOnlyStandaloneNotDeletedVouchers()
    {
        await using var db = VoucherTestData.Database();
        var program = VoucherTestData.Program();
        var standalone = VoucherTestData.Voucher("STANDALONE10");
        var programVoucher = VoucherTestData.Voucher("PROGRAM10", visibility: VoucherVisibility.Private);
        programVoucher.Program = program;
        programVoucher.ProgramId = program.Id;
        var deleted = VoucherTestData.Voucher("DELETED10");
        deleted.IsDeleted = true;
        db.AddRange(program, standalone, programVoucher, deleted);
        await db.SaveChangesAsync();

        var result = await new VoucherService(db).GetVouchers(1, 10, null, null, null);

        var item = Assert.Single(result.Value.Items.Cast<Response.VoucherResponse>());
        Assert.Equal(standalone.Id, item.Id);
        // Assert.Null(item.ProgramId);
        Assert.Equal(standalone.StartAt, item.StartAt);
        Assert.Equal(standalone.EndAt, item.EndAt);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetVouchers_FiltersByVisibilityAndActive()
    {
        await using var db = VoucherTestData.Database();
        var match = VoucherTestData.Voucher("PRIV1", active: true, visibility: VoucherVisibility.Private);
        var wrongVisibility = VoucherTestData.Voucher("PUB1", active: true);
        var wrongActive = VoucherTestData.Voucher("PRIV2", active: false, visibility: VoucherVisibility.Private);
        db.AddRange(match, wrongVisibility, wrongActive);
        await db.SaveChangesAsync();

        var result = await new VoucherService(db).GetVouchers(1, 10, null, VoucherVisibilities.Private, true);

        var item = Assert.Single(result.Value.Items.Cast<Response.VoucherResponse>());
        Assert.Equal(match.Id, item.Id);
    }

    [Fact]
    public async Task GetVouchers_InvalidVisibility_ThrowsValidationError()
    {
        await using var db = VoucherTestData.Database();
        await Assert.ThrowsAsync<ValidationExceptionErrorV1>(
            () => new VoucherService(db).GetVouchers(1, 10, null, "hidden", null));
    }

    [Fact]
    public async Task DeleteVoucher_SoftDeletesExistingVoucher()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();

        var message = await new VoucherService(db).DeleteVoucher(voucher.Id);

        Assert.Equal("Voucher deleted successfully", message);
        Assert.True(voucher.IsDeleted);
        Assert.NotNull(voucher.UpdatedAt);
    }

    [Fact]
    public async Task DeleteVoucher_MissingOrDeleted_ThrowsNotFound()
    {
        await using var db = VoucherTestData.Database();
        var deleted = VoucherTestData.Voucher();
        deleted.IsDeleted = true;
        db.Vouchers.Add(deleted);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => new VoucherService(db).DeleteVoucher(deleted.Id));
        await Assert.ThrowsAsync<NotFoundException>(() => new VoucherService(db).DeleteVoucher(Guid.NewGuid()));
    }

    [Fact]
    public async Task SetVouchersActive_UpdatesExistingVouchersAndIgnoresMissingIds()
    {
        await using var db = VoucherTestData.Database();
        var activeProgram = VoucherTestData.Program();
        var inactiveProgram = VoucherTestData.Program(active: false);
        var standalone = VoucherTestData.Voucher("ONE", active: false);
        var activeProgramVoucher = VoucherTestData.Voucher("TWO", active: false);
        activeProgramVoucher.Program = activeProgram;
        activeProgramVoucher.ProgramId = activeProgram.Id;
        var inactiveProgramVoucher = VoucherTestData.Voucher("THREE", active: false);
        inactiveProgramVoucher.Program = inactiveProgram;
        inactiveProgramVoucher.ProgramId = inactiveProgram.Id;
        var missingId = Guid.NewGuid();
        db.AddRange(activeProgram, inactiveProgram, standalone, activeProgramVoucher, inactiveProgramVoucher);
        await db.SaveChangesAsync();

        var result = await new VoucherService(db).SetVouchersActive(new Request.SetVouchersActiveRequest
        {
            Ids = [standalone.Id, activeProgramVoucher.Id, inactiveProgramVoucher.Id, missingId], IsActive = true
        });

        Assert.Equal("Voucher status updated successfully.", result);
        db.ChangeTracker.Clear();
        Assert.Equal(3, db.Vouchers.Count());
        Assert.All(db.Vouchers, voucher =>
        {
            Assert.True(voucher.IsActive);
            Assert.NotNull(voucher.UpdatedAt);
        });
    }
}

using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Voucher;
using Xunit;
using VoucherService = HoaTheoMua.Service.Voucher.Service;

namespace HoaTheoMua.Service.Test.VoucherTest;

public class ManageVouchersTest
{
    [Fact]
    public async Task GetVouchers_FiltersAndUsesProgramEffectiveValues()
    {
        await using var db = VoucherTestData.Database();
        var program = VoucherTestData.Program();
        var included = VoucherTestData.Voucher("PROGRAM10", visibility: VoucherVisibility.Private);
        included.Program = program;
        included.ProgramId = program.Id;
        included.StartAt = null;
        included.EndAt = null;
        var excluded = VoucherTestData.Voucher("PUBLIC10");
        db.AddRange(program, included, excluded);
        await db.SaveChangesAsync();

        var result = await new VoucherService(db).GetVouchers(1, 10, null, VoucherVisibilities.Private, true, program.Id);

        var item = Assert.Single(result.Value.Items.Cast<Response.VoucherResponse>());
        Assert.Equal(included.Id, item.Id);
        Assert.Equal(program.Name, item.ProgramName);
        Assert.Equal(program.StartAt, item.StartAt);
        Assert.Equal(program.EndAt, item.EndAt);
        Assert.Equal(program.MinOrderAmount, item.MinOrderAmount);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetVouchers_InvalidVisibility_ThrowsValidationError()
    {
        await using var db = VoucherTestData.Database();
        await Assert.ThrowsAsync<ValidationExceptionErrorV1>(
            () => new VoucherService(db).GetVouchers(1, 10, null, "hidden", null, null));
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
    public async Task SetVouchersActive_ReportsUpdatedSkippedAndMissingIds()
    {
        await using var db = VoucherTestData.Database();
        var activeProgram = VoucherTestData.Program();
        var inactiveProgram = VoucherTestData.Program(active: false);
        var standalone = VoucherTestData.Voucher("ONE", active: false);
        var allowed = VoucherTestData.Voucher("TWO", active: false);
        allowed.Program = activeProgram;
        allowed.ProgramId = activeProgram.Id;
        var skipped = VoucherTestData.Voucher("THREE", active: false);
        skipped.Program = inactiveProgram;
        skipped.ProgramId = inactiveProgram.Id;
        var missingId = Guid.NewGuid();
        db.AddRange(activeProgram, inactiveProgram, standalone, allowed, skipped);
        await db.SaveChangesAsync();

        var result = await new VoucherService(db).SetVouchersActive(new Request.SetVouchersActiveRequest
        {
            Ids = [standalone.Id, allowed.Id, skipped.Id, missingId], IsActive = true
        });

        Assert.Equal(new[] { standalone.Id, allowed.Id }, result.UpdatedIds);
        Assert.Equal(missingId, Assert.Single(result.NotFoundIds));
        var skippedItem = Assert.Single(result.SkippedItems);
        Assert.Equal(skipped.Id, skippedItem.Id);
        Assert.Equal("Program is not active", skippedItem.Reason);
        Assert.True(standalone.IsActive);
        Assert.True(allowed.IsActive);
        Assert.False(skipped.IsActive);
    }
}

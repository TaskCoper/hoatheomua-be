using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Voucher;
using Xunit;
using VoucherService = HoaTheoMua.Service.Voucher.Service;

namespace HoaTheoMua.Service.Test.VoucherTest;

public class PublicAndEstimateVoucherTest
{
    [Fact]
    public async Task GetPublicVouchers_ReturnsEligibleStandaloneVouchersAndCalculatesDiscount()
    {
        await using var db = VoucherTestData.Database();
        var eligible = VoucherTestData.Voucher("SAVE20");
        eligible.DiscountValue = 20;
        eligible.MaxDiscountAmount = 30_000;
        var privateVoucher = VoucherTestData.Voucher("PRIVATE", visibility: VoucherVisibility.Private);
        var exhausted = VoucherTestData.Voucher("USEDUP");
        exhausted.UsedCount = exhausted.MaxUsageCount;
        var programVoucher = VoucherTestData.Voucher("PROGRAM");
        var program = VoucherTestData.Program();
        programVoucher.Program = program;
        programVoucher.ProgramId = program.Id;
        db.AddRange(eligible, privateVoucher, exhausted, program, programVoucher);
        await db.SaveChangesAsync();

        var result = await new VoucherService(db).GetPublicVouchers(200_000);

        var item = Assert.Single(result);
        Assert.Equal(eligible.Id, item.Id);
        Assert.Equal(30_000, item.DiscountAmount);
    }

    [Fact]
    public async Task EstimateVoucher_NormalizesCodeAndCapsPercentDiscount()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher("SAVE20");
        voucher.DiscountValue = 20;
        voucher.MaxDiscountAmount = 30_000;
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();

        var result = await new VoucherService(db).EstimateVoucher(new Request.EstimateVoucherRequest
        {
            Code = " save20 ", ProductFee = 200_000
        });

        Assert.Equal(voucher.Id, result.VoucherId);
        Assert.Equal(30_000, result.DiscountAmount);
        Assert.Equal(170_000, result.ProductFeeAfterDiscount);
    }

    [Theory]
    [InlineData("future")]
    [InlineData("expired")]
    [InlineData("exhausted")]
    [InlineData("below-minimum")]
    [InlineData("above-maximum")]
    public async Task EstimateVoucher_RejectsVoucherOutsideUsageRules(string scenario)
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        var fee = 200_000m;
        var now = DateTimeOffset.UtcNow;
        switch (scenario)
        {
            case "future": voucher.StartAt = now.AddHours(1); break;
            case "expired": voucher.EndAt = now.AddHours(-1); break;
            case "exhausted": voucher.UsedCount = voucher.MaxUsageCount; break;
            case "below-minimum": fee = 50_000; break;
            case "above-maximum": fee = 3_000_000; break;
        }
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        var action = () => new VoucherService(db).EstimateVoucher(
            new Request.EstimateVoucherRequest { Code = voucher.Code, ProductFee = fee });

        if (scenario == "exhausted")
            await Assert.ThrowsAsync<ConflictException>(action);
        else
            await Assert.ThrowsAsync<BadRequestException>(action);
    }

    [Fact]
    public async Task EstimateVoucher_MissingInactiveOrDeleted_ThrowsNotFound()
    {
        await using var db = VoucherTestData.Database();
        var inactive = VoucherTestData.Voucher("INACTIVE", active: false);
        var deleted = VoucherTestData.Voucher("DELETED");
        deleted.IsDeleted = true;
        db.AddRange(inactive, deleted);
        await db.SaveChangesAsync();
        var service = new VoucherService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.EstimateVoucher(
            new Request.EstimateVoucherRequest { Code = "MISSING", ProductFee = 100_000 }));
        await Assert.ThrowsAsync<NotFoundException>(() => service.EstimateVoucher(
            new Request.EstimateVoucherRequest { Code = inactive.Code, ProductFee = 100_000 }));
        await Assert.ThrowsAsync<NotFoundException>(() => service.EstimateVoucher(
            new Request.EstimateVoucherRequest { Code = deleted.Code, ProductFee = 100_000 }));
    }
}

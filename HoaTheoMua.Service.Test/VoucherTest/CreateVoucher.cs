using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Voucher;
using Xunit;
using VoucherService = HoaTheoMua.Service.Voucher.Service;

namespace HoaTheoMua.Service.Test.VoucherTest;

public class CreateVoucherTest
{
    [Fact]
    public async Task ManualCode_IsNormalizedAndPersisted()
    {
        await using var db = VoucherTestData.Database();
        var request = ValidRequest();
        request.Code = "  save20  ";

        var result = await new VoucherService(db).CreateVoucher(request);

        Assert.Equal("Create voucher successfully", result);
        var voucher = Assert.Single(db.Vouchers);
        Assert.Equal("SAVE20", voucher.Code);
        Assert.Equal("A useful voucher", voucher.Description);
        Assert.Equal(VoucherVisibility.Public, voucher.Visibility);
        Assert.Equal(VoucherDiscountType.Percent, voucher.DiscountType);
        Assert.False(voucher.IsActive);
        Assert.False(voucher.IsDeleted);
    }

    [Fact]
    public async Task DuplicateManualCode_ThrowsConflict()
    {
        await using var db = VoucherTestData.Database();
        db.Vouchers.Add(VoucherTestData.Voucher("SAVE20"));
        await db.SaveChangesAsync();
        var request = ValidRequest();
        request.Code = "save20";

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => new VoucherService(db).CreateVoucher(request));

        Assert.Equal("Voucher code already exists.", error.Message);
        Assert.Single(db.Vouchers);
    }

    [Fact]
    public async Task GeneratedCode_UsesRequestedShape()
    {
        await using var db = VoucherTestData.Database();
        var request = ValidRequest();
        request.Code = null;
        request.CodePrefix = " ht ";
        request.CodeLength = 6;
        request.CodeSuffix = " 26 ";

        await new VoucherService(db).CreateVoucher(request);

        var code = Assert.Single(db.Vouchers).Code;
        Assert.StartsWith("HT", code);
        Assert.EndsWith("26", code);
        Assert.Equal(10, code.Length);
    }

    private static Request.CreateVoucherRequest ValidRequest() => new()
    {
        Code = "SAVE20", Description = "  A useful voucher  ", Visibility = VoucherVisibilities.Public,
        DiscountType = VoucherDiscountTypes.Percent, DiscountValue = 20, MaxDiscountAmount = 50_000,
        MaxUsageCount = 5, StartAt = DateTimeOffset.UtcNow.AddHours(-1),
        EndAt = DateTimeOffset.UtcNow.AddDays(2), MinOrderAmount = 100_000, MaxOrderAmount = 2_000_000
    };
}

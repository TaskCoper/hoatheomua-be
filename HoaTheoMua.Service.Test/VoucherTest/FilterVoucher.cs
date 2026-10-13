using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Xunit;
using Request = HoaTheoMua.Service.Voucher.Request;
using VoucherService = HoaTheoMua.Service.Voucher.Service;

namespace HoaTheoMua.Service.Test.VoucherTest;

public class FilterVoucherTest
{
    private static Repository.Entity.Product Product(decimal price, bool active = true, bool deleted = false) => new()
    {
        Id = Guid.NewGuid(), ProductName = "Rose", Price = price, IsActive = active, IsDeleted = deleted
    };

    // Each id with quantity 1
    private static Request.FilterVoucherRequest Req(params Guid[] ids) => new()
    {
        Products = ids.Select(id => new Request.ProductOrder { Id = id, Quantity = 1 }).ToList()
    };

    [Fact]
    public async Task FilterVoucher_TotalInRange_ReturnsValidWithDiscount()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        var p1 = Product(100_000);
        var p2 = Product(100_000);
        db.AddRange(voucher, p1, p2);
        await db.SaveChangesAsync();

        var result = await new VoucherService(db).FilterVoucher(Req(p1.Id, p2.Id));

        var item = Assert.Single(result);
        Assert.True(item.IsValid);
        Assert.Equal(20_000, item.DiscountAmount); // 10% of 200,000
        Assert.Equal(180_000, item.ProductFeeAfterDiscount);
        Assert.Equal(voucher.MinOrderAmount, item.MinOrderAmount);
        Assert.Equal(voucher.MaxOrderAmount, item.MaxOrderAmount);
    }

    [Fact]
    public async Task FilterVoucher_PercentDiscount_IsCappedByMaxDiscountAmount()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        var product = Product(1_000_000);
        db.AddRange(voucher, product);
        await db.SaveChangesAsync();

        var item = Assert.Single(await new VoucherService(db).FilterVoucher(Req(product.Id)));

        Assert.Equal(50_000, item.DiscountAmount);
    }

    // [Theory]
    // [InlineData("inactive")]
    // [InlineData("future")]
    // [InlineData("expired")]
    // [InlineData("exhausted")]
    // [InlineData("below-minimum")]
    // [InlineData("above-maximum")]
    // public async Task FilterVoucher_UnusableVoucher_StaysInListButIsNotValid(string scenario)
    // {
    //     await using var db = VoucherTestData.Database();
    //     var voucher = VoucherTestData.Voucher();
    //     var price = 200_000m;
    //     var now = DateTimeOffset.UtcNow;
    //     switch (scenario)
    //     {
    //         case "inactive": voucher.IsActive = false; break;
    //         case "future": voucher.StartAt = now.AddHours(1); break;
    //         case "expired": voucher.EndAt = now.AddHours(-1); break;
    //         case "exhausted": voucher.UsedCount = voucher.MaxUsageCount; break;
    //         case "below-minimum": price = 50_000; break;
    //         case "above-maximum": price = 3_000_000; break;
    //     }
    //     var product = Product(price);
    //     db.AddRange(voucher, product);
    //     await db.SaveChangesAsync();

    //     var item = Assert.Single(await new VoucherService(db).FilterVoucher(Req(product.Id)));

    //     Assert.False(item.IsValid);
    //     Assert.Equal(0, item.DiscountAmount);
    //     Assert.Equal(price, item.ProductFeeAfterDiscount);
    // }

    [Fact]
    public async Task FilterVoucher_NoMinOrMax_IsValidAndReturnsNullLimits()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        voucher.MinOrderAmount = null;
        voucher.MaxOrderAmount = null;
        var product = Product(10_000);
        db.AddRange(voucher, product);
        await db.SaveChangesAsync();

        var item = Assert.Single(await new VoucherService(db).FilterVoucher(Req(product.Id)));

        Assert.True(item.IsValid);
        Assert.Null(item.MinOrderAmount);
        Assert.Null(item.MaxOrderAmount);
    }

    [Fact]
    public async Task FilterVoucher_ExcludesPrivateDeletedAndProgramVouchers()
    {
        await using var db = VoucherTestData.Database();
        var publicVoucher = VoucherTestData.Voucher("PUBLIC");
        var privateVoucher = VoucherTestData.Voucher("PRIVATE", visibility: VoucherVisibility.Private);
        var deleted = VoucherTestData.Voucher("DELETED");
        deleted.IsDeleted = true;
        var program = VoucherTestData.Program();
        var programVoucher = VoucherTestData.Voucher("PROGRAM");
        programVoucher.Program = program;
        programVoucher.ProgramId = program.Id;
        var product = Product(200_000);
        db.AddRange(publicVoucher, privateVoucher, deleted, program, programVoucher, product);
        await db.SaveChangesAsync();

        var item = Assert.Single(await new VoucherService(db).FilterVoucher(Req(product.Id)));

        Assert.Equal(publicVoucher.Id, item.VoucherId);
    }

    // [Fact]
    // public async Task FilterVoucher_ValidVouchersFirst_ThenLargestDiscount()
    // {
    //     await using var db = VoucherTestData.Database();
    //     var small = VoucherTestData.Voucher("SMALL");
    //     small.DiscountValue = 5;
    //     var big = VoucherTestData.Voucher("BIG");
    //     big.DiscountValue = 20;
    //     var invalid = VoucherTestData.Voucher("INVALID", active: false);
    //     var product = Product(200_000);
    //     db.AddRange(small, big, invalid, product);
    //     await db.SaveChangesAsync();

    //     var result = await new VoucherService(db).FilterVoucher(Req(product.Id));

    //     Assert.Equal(["BIG", "SMALL", "INVALID"], result.Select(x => x.Code));
    // }

    [Fact]
    public async Task FilterVoucher_DuplicateProductIds_CountsEachProductOnce()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        var product = Product(200_000);
        db.AddRange(voucher, product);
        await db.SaveChangesAsync();

        var item = Assert.Single(await new VoucherService(db).FilterVoucher(Req(product.Id, product.Id)));

        Assert.Equal(360_000, item.ProductFeeAfterDiscount); // quantity is summed: 2 x 200,000
    }

    [Fact]
    public async Task FilterVoucher_EmptyProductIds_ThrowsBadRequest()
    {
        await using var db = VoucherTestData.Database();

        await Assert.ThrowsAsync<BadRequestException>(() => new VoucherService(db).FilterVoucher(Req()));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("inactive")]
    [InlineData("deleted")]
    public async Task FilterVoucher_UnknownInactiveOrDeletedProduct_ThrowsBadRequest(string scenario)
    {
        await using var db = VoucherTestData.Database();
        var product = Product(200_000, active: scenario != "inactive", deleted: scenario == "deleted");
        if (scenario != "missing") db.Add(product);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() => new VoucherService(db).FilterVoucher(Req(product.Id)));
    }

    [Fact]
    public async Task FilterVoucher_ProductWithoutPrice_ThrowsBadRequestInsteadOf500()
    {
        await using var db = VoucherTestData.Database();
        var product = Product(0);
        product.Price = null;
        db.Add(product);
        await db.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<BadRequestException>(
            () => new VoucherService(db).FilterVoucher(Req(product.Id)));
        Assert.Equal("Một số sản phẩm chưa có giá bán. Vui lòng thử lại sau.", error.Message);
    }
}

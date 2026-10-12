using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Voucher;
using Microsoft.EntityFrameworkCore;
using Xunit;
using VoucherService = HoaTheoMua.Service.Voucher.Service;

namespace HoaTheoMua.Service.Test.VoucherTest;

public class UpdateVouchersTest
{
    [Fact]
    public async Task UpdatesBatch_PersistsRequestedFieldsAndSharedTimestamp()
    {
        await using var db = VoucherTestData.Database();
        var first = VoucherTestData.Voucher("FIRST");
        var second = VoucherTestData.Voucher("SECOND", visibility: VoucherVisibility.Private);
        second.MaxDiscountAmount = null;
        var untouched = VoucherTestData.Voucher("UNTOUCHED");
        db.Vouchers.AddRange(first, second, untouched);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var startAt = new DateTimeOffset(2030, 1, 1, 9, 0, 0, TimeSpan.FromHours(7));
        var endAt = startAt.AddDays(7);
        var before = DateTimeOffset.UtcNow;

        var result = await new VoucherService(db).UpdateVouchers(new Request.UpdateVouchersRequest
        {
            // Reverse the database insertion order to check that updates are matched by id.
            Items =
            [
                new()
                {
                    Id = second.Id, Description = "  Cash voucher  ", Visibility = VoucherVisibilities.Public,
                    DiscountType = VoucherDiscountTypes.Cash, DiscountValue = 20_000, MaxUsageCount = 30
                },
                new()
                {
                    Id = first.Id, Description = "  Updated voucher  ", Visibility = VoucherVisibilities.Private,
                    DiscountType = VoucherDiscountTypes.Percent, DiscountValue = 25, MaxDiscountAmount = 75_000,
                    MaxUsageCount = 20, StartAt = startAt, EndAt = endAt,
                    MinOrderAmount = 200_000, MaxOrderAmount = 3_000_000
                }
            ]
        });

        var after = DateTimeOffset.UtcNow;
        Assert.Equal("Update voucher successfully", result);
        db.ChangeTracker.Clear();
        var updatedFirst = await db.Vouchers.SingleAsync(x => x.Id == first.Id);
        Assert.Equal("Updated voucher", updatedFirst.Description);
        Assert.Equal(VoucherVisibility.Private, updatedFirst.Visibility);
        Assert.Equal(VoucherDiscountType.Percent, updatedFirst.DiscountType);
        Assert.Equal(25m, updatedFirst.DiscountValue);
        Assert.Equal(75_000m, updatedFirst.MaxDiscountAmount);
        Assert.Equal(20, updatedFirst.MaxUsageCount);
        Assert.Equal(startAt.ToUniversalTime(), updatedFirst.StartAt);
        Assert.Equal(TimeSpan.Zero, updatedFirst.StartAt!.Value.Offset);
        Assert.Equal(endAt.ToUniversalTime(), updatedFirst.EndAt);
        Assert.Equal(TimeSpan.Zero, updatedFirst.EndAt!.Value.Offset);
        Assert.Equal(200_000m, updatedFirst.MinOrderAmount);
        Assert.Equal(3_000_000m, updatedFirst.MaxOrderAmount);
        Assert.InRange(updatedFirst.UpdatedAt!.Value, before, after);
        var updatedSecond = await db.Vouchers.SingleAsync(x => x.Id == second.Id);
        Assert.Equal("Cash voucher", updatedSecond.Description);
        Assert.Equal(VoucherVisibility.Public, updatedSecond.Visibility);
        Assert.Equal(VoucherDiscountType.Cash, updatedSecond.DiscountType);
        Assert.Equal(20_000m, updatedSecond.DiscountValue);
        Assert.Null(updatedSecond.MaxDiscountAmount);
        Assert.Equal(30, updatedSecond.MaxUsageCount);
        Assert.Equal(updatedFirst.UpdatedAt, updatedSecond.UpdatedAt);
        var unchanged = await db.Vouchers.SingleAsync(x => x.Id == untouched.Id);
        Assert.Equal("Test voucher", unchanged.Description);
        Assert.Null(unchanged.UpdatedAt);
        Assert.Equal(3, await db.Vouchers.CountAsync());
    }

    [Fact]
    public async Task PartialUpdate_PreservesOmittedFieldsAndIdentity()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        voucher.UsedCount = 3;
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new VoucherService(db).UpdateVouchers(Batch(new Request.UpdateVoucherItem()
        {
            Id = voucher.Id, Description = "   "
        }));

        db.ChangeTracker.Clear();
        var updated = await db.Vouchers.SingleAsync();
        Assert.Equal(string.Empty, updated.Description);
        Assert.Equal(voucher.Id, updated.Id);
        Assert.Equal(voucher.Code, updated.Code);
        Assert.Equal(voucher.ProgramId, updated.ProgramId);
        Assert.Equal(voucher.Visibility, updated.Visibility);
        Assert.Equal(voucher.DiscountType, updated.DiscountType);
        Assert.Equal(voucher.DiscountValue, updated.DiscountValue);
        Assert.Null(updated.MaxDiscountAmount);
        Assert.Equal(voucher.MaxUsageCount, updated.MaxUsageCount);
        Assert.Equal(voucher.UsedCount, updated.UsedCount);
        Assert.Equal(voucher.StartAt, updated.StartAt);
        Assert.Null(updated.EndAt);
        Assert.Null(updated.MinOrderAmount);
        Assert.Null(updated.MaxOrderAmount);
        Assert.Equal(voucher.IsActive, updated.IsActive);
        Assert.Equal(voucher.IsDeleted, updated.IsDeleted);
        Assert.Equal(voucher.CreatedAt, updated.CreatedAt);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingOrDeletedVoucher_RejectsEntireBatch(bool deleted)
    {
        await using var db = VoucherTestData.Database();
        var valid = VoucherTestData.Voucher("VALID");
        var unavailable = VoucherTestData.Voucher("UNAVAILABLE");
        unavailable.IsDeleted = deleted;
        db.Vouchers.Add(valid);
        if (deleted) db.Vouchers.Add(unavailable);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var error = await Assert.ThrowsAsync<NotFoundException>(() => new VoucherService(db).UpdateVouchers(
            Batch(new Request.UpdateVoucherItem() { Id = valid.Id, Description = "Changed" },
                new() { Id = unavailable.Id, Description = "Changed" })));

        Assert.Equal($"Voucher not found: {unavailable.Id}", error.Message);
        AssertNoUpdates(db);
    }

    [Fact]
    public async Task ProgramVoucher_UpdatesAllowedFieldsAndPreservesProgramValues()
    {
        await using var db = VoucherTestData.Database();
        var program = VoucherTestData.Program();
        var voucher = VoucherTestData.Voucher(visibility: VoucherVisibility.Private);
        voucher.Program = program;
        voucher.ProgramId = program.Id;
        voucher.StartAt = voucher.EndAt = null;
        voucher.MinOrderAmount = voucher.MaxOrderAmount = null;
        voucher.DiscountType = VoucherDiscountType.Cash;
        voucher.DiscountValue = 20_000;
        voucher.MaxDiscountAmount = null;
        db.AddRange(program, voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new VoucherService(db).UpdateVouchers(Batch(new Request.UpdateVoucherItem()
        {
            Id = voucher.Id, Description = "  Program discount  ", Visibility = VoucherVisibilities.Private,
            DiscountType = VoucherDiscountTypes.Percent, DiscountValue = 15,
            MaxDiscountAmount = 40_000, MaxUsageCount = 25
        }));

        db.ChangeTracker.Clear();
        var updated = await db.Vouchers.SingleAsync();
        Assert.Equal("Program discount", updated.Description);
        Assert.Equal(VoucherVisibility.Private, updated.Visibility);
        Assert.Equal(VoucherDiscountType.Percent, updated.DiscountType);
        Assert.Equal(15m, updated.DiscountValue);
        Assert.Equal(40_000m, updated.MaxDiscountAmount);
        Assert.Equal(25, updated.MaxUsageCount);
        Assert.Equal(program.Id, updated.ProgramId);
        Assert.Null(updated.StartAt);
        Assert.Null(updated.EndAt);
        Assert.Null(updated.MinOrderAmount);
        Assert.Null(updated.MaxOrderAmount);
        Assert.NotNull(updated.UpdatedAt);
        var unchangedProgram = await db.VoucherPrograms.SingleAsync();
        Assert.Equal(program.StartAt, unchangedProgram.StartAt);
        Assert.Equal(program.EndAt, unchangedProgram.EndAt);
        Assert.Equal(program.MinOrderAmount, unchangedProgram.MinOrderAmount);
        Assert.Equal(program.MaxOrderAmount, unchangedProgram.MaxOrderAmount);
        Assert.Null(unchangedProgram.UpdatedAt);
    }

    [Theory]
    [InlineData("visibility", "Program voucher must stay private")]
    [InlineData("startAt", "Program voucher dates and order amounts come from the program")]
    [InlineData("endAt", "Program voucher dates and order amounts come from the program")]
    [InlineData("minOrderAmount", "Program voucher dates and order amounts come from the program")]
    [InlineData("maxOrderAmount", "Program voucher dates and order amounts come from the program")]
    public async Task ProgramVoucher_RejectsRestrictedFields(string field, string message)
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher(visibility: VoucherVisibility.Private);
        voucher.ProgramId = Guid.NewGuid();
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var item = new Request.UpdateVoucherItem { Id = voucher.Id, Description = "Changed" };
        switch (field)
        {
            case "visibility": item.Visibility = VoucherVisibilities.Public; break;
            case "startAt": item.StartAt = DateTimeOffset.UtcNow; break;
            case "endAt": item.EndAt = DateTimeOffset.UtcNow.AddDays(2); break;
            case "minOrderAmount": item.MinOrderAmount = 200_000; break;
            case "maxOrderAmount": item.MaxOrderAmount = 3_000_000; break;
        }

        await AssertValidationError(db, item, $"{message} ({voucher.Code})");
    }

    [Theory]
    [InlineData("newValue")]
    [InlineData("retainedType")]
    [InlineData("retainedValue")]
    public async Task PercentOver100_RejectsMergedDiscount(string scenario)
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        var item = new Request.UpdateVoucherItem { Id = voucher.Id, Description = "Changed" };
        if (scenario == "retainedValue")
        {
            voucher.DiscountType = VoucherDiscountType.Cash;
            voucher.DiscountValue = 20_000;
            voucher.MaxDiscountAmount = null;
            item.DiscountType = VoucherDiscountTypes.Percent;
        }
        else
        {
            item.DiscountValue = 101;
            if (scenario == "newValue") item.DiscountType = VoucherDiscountTypes.Percent;
        }
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await AssertValidationError(db, item, $"Discount percent must not exceed 100 ({voucher.Code})");
    }

    [Theory]
    [InlineData("newCap")]
    [InlineData("retainedCap")]
    [InlineData("changedType")]
    public async Task CashDiscountWithCap_RejectsMergedDiscount(string scenario)
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        var item = new Request.UpdateVoucherItem
        {
            Id = voucher.Id, Description = "Changed", MaxDiscountAmount = voucher.MaxDiscountAmount
        };
        if (scenario == "changedType") item.DiscountType = VoucherDiscountTypes.Cash;
        else
        {
            voucher.DiscountType = VoucherDiscountType.Cash;
            if (scenario == "newCap")
            {
                voucher.MaxDiscountAmount = null;
                item.MaxDiscountAmount = 30_000;
            }
        }
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await AssertValidationError(db, item, $"Max discount amount is only for percent ({voucher.Code})");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UsageLimitBelowUsedCount_RejectsStandaloneAndProgramVoucher(bool programVoucher)
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher(visibility: VoucherVisibility.Private);
        voucher.UsedCount = 5;
        if (programVoucher) voucher.ProgramId = Guid.NewGuid();
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await AssertValidationError(db, new() { Id = voucher.Id, MaxUsageCount = 4 },
            $"Max usage count cannot be less than used count ({voucher.Code})");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public async Task EndAtNotAfterStartAt_RejectsMergedDates(bool updateStart, bool equal)
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var item = new Request.UpdateVoucherItem { Id = voucher.Id, Description = "Changed", EndAt = voucher.EndAt };
        if (updateStart) item.StartAt = voucher.EndAt!.Value.AddHours(equal ? 0 : 1);
        else item.EndAt = voucher.StartAt!.Value.AddHours(equal ? 0 : -1);

        await AssertValidationError(db, item, $"End time must be later than start time ({voucher.Code})");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MaxOrderBelowMinOrder_RejectsMergedAmounts(bool updateMin)
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var item = new Request.UpdateVoucherItem
        {
            Id = voucher.Id, Description = "Changed",
            MinOrderAmount = voucher.MinOrderAmount, MaxOrderAmount = voucher.MaxOrderAmount
        };
        if (updateMin) item.MinOrderAmount = voucher.MaxOrderAmount + 1;
        else item.MaxOrderAmount = voucher.MinOrderAmount - 1;

        await AssertValidationError(db, item, $"Max order amount cannot be less than min order amount ({voucher.Code})");
    }

    [Fact]
    public async Task BoundaryValues_Accepts100PercentUsageEqualToUsedAndEqualOrderAmounts()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        voucher.UsedCount = 5;
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new VoucherService(db).UpdateVouchers(Batch(new Request.UpdateVoucherItem()
        {
            Id = voucher.Id, DiscountValue = 100, MaxUsageCount = 5,
            MinOrderAmount = 200_000, MaxOrderAmount = 200_000
        }));

        db.ChangeTracker.Clear();
        var updated = await db.Vouchers.SingleAsync();
        Assert.Equal(100m, updated.DiscountValue);
        Assert.Equal(5, updated.MaxUsageCount);
        Assert.Equal(5, updated.UsedCount);
        Assert.Equal(200_000m, updated.MinOrderAmount);
        Assert.Equal(200_000m, updated.MaxOrderAmount);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task OpenEndedVoucher_AcceptsPartialUpdateWithoutOptionalLimits()
    {
        await using var db = VoucherTestData.Database();
        var voucher = VoucherTestData.Voucher();
        voucher.EndAt = null;
        voucher.MinOrderAmount = voucher.MaxOrderAmount = null;
        voucher.MaxDiscountAmount = null;
        db.Vouchers.Add(voucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new VoucherService(db).UpdateVouchers(Batch(new Request.UpdateVoucherItem() { Id = voucher.Id, DiscountValue = 20 }));

        db.ChangeTracker.Clear();
        var updated = await db.Vouchers.SingleAsync();
        Assert.Equal(20m, updated.DiscountValue);
        Assert.Equal("Test voucher", updated.Description);
        Assert.Null(updated.EndAt);
        Assert.Null(updated.MinOrderAmount);
        Assert.Null(updated.MaxOrderAmount);
        Assert.Null(updated.MaxDiscountAmount);
    }

    [Fact]
    public async Task MultipleInvalidItems_AggregatesIndexedErrorsAndDoesNotUpdateAnyVoucher()
    {
        await using var db = VoucherTestData.Database();
        var valid = VoucherTestData.Voucher("VALID");
        var invalidPercent = VoucherTestData.Voucher("INVALID_PERCENT");
        var invalidUsage = VoucherTestData.Voucher("INVALID_USAGE");
        invalidUsage.UsedCount = 5;
        db.Vouchers.AddRange(valid, invalidPercent, invalidUsage);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var error = await Assert.ThrowsAsync<ValidationExceptionErrorV1>(() => new VoucherService(db).UpdateVouchers(
            Batch(new Request.UpdateVoucherItem() { Id = valid.Id, Description = "Changed", DiscountValue = 20 },
                new() { Id = invalidPercent.Id, Description = "Changed", DiscountValue = 101 },
                new() { Id = invalidUsage.Id, Description = "Changed", MaxUsageCount = 4 })));

        Assert.Collection(error.Errors,
            first =>
            {
                Assert.Equal("items[1]", first.Field);
                Assert.Equal("Discount percent must not exceed 100 (INVALID_PERCENT)", first.Message);
            },
            second =>
            {
                Assert.Equal("items[2]", second.Field);
                Assert.Equal("Max usage count cannot be less than used count (INVALID_USAGE)", second.Message);
            });
        AssertNoUpdates(db);
        Assert.All(db.Vouchers, voucher => Assert.Equal(10m, voucher.DiscountValue));
    }

    private static Request.UpdateVouchersRequest Batch(params Request.UpdateVoucherItem[] items)
        => new() { Items = items.ToList() };

    private static async Task AssertValidationError(AppDbContext db, Request.UpdateVoucherItem item, string message)
    {
        var error = await Assert.ThrowsAsync<ValidationExceptionErrorV1>(
            () => new VoucherService(db).UpdateVouchers(Batch(item)));

        var validationError = Assert.Single(error.Errors);
        Assert.Equal("items[0]", validationError.Field);
        Assert.Equal(message, validationError.Message);
        AssertNoUpdates(db);
    }

    private static void AssertNoUpdates(AppDbContext db)
    {
        db.ChangeTracker.DetectChanges();
        Assert.All(db.ChangeTracker.Entries(), entry => Assert.Equal(EntityState.Unchanged, entry.State));
        db.ChangeTracker.Clear();
        Assert.All(db.Vouchers, voucher =>
        {
            Assert.Equal("Test voucher", voucher.Description);
            Assert.Null(voucher.UpdatedAt);
        });
    }
}



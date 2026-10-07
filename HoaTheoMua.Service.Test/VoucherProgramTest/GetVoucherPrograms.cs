using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.VoucherProgram;
using Microsoft.EntityFrameworkCore;
using Xunit;
using VoucherProgramEntity = HoaTheoMua.Repository.Entity.VoucherProgram;
using VoucherProgramService = HoaTheoMua.Service.VoucherProgram.Service;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class GetVoucherPrograms
{
    [Fact]
    public async Task FiltersByNameAndActiveState_ExcludesDeletedPrograms()
    {
        await using var db = CreateDbContext();
        db.VoucherPrograms.AddRange(
            Program("SALE 20/10", 1, active: true),
            Program("Sale 20/10 old", 2, active: true, deleted: true),
            Program("sale 20/10 draft", 3),
            Program("Christmas", 4, active: true));
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db).GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { Keyword = "  sAlE 20/10  ", IsActive = true });

        var item = Assert.Single(result.Value.Items.Cast<Response.VoucherProgramResponse>());
        Assert.Equal("SALE 20/10", item.Name);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageIndex);
        Assert.Equal(20, result.Value.PageSize);
        Assert.False(result.Value.HasNextPage);
        Assert.False(result.Value.HasPreviousPage);
    }

    [Fact]
    public async Task PaginatesNewestFirst_AndCountsCodesRatherThanTotalUses()
    {
        await using var db = CreateDbContext();
        var oldest = Program("Oldest", 1);
        var middle = Program("Middle", 2);
        var newest = Program("Newest", 3);
        db.VoucherPrograms.AddRange(oldest, middle, newest);
        db.Vouchers.AddRange(
            Voucher(middle.Id, "USED1", 3),
            Voucher(middle.Id, "USED2", 1),
            Voucher(middle.Id, "UNUSED", 0),
            Voucher(middle.Id, "DELETED", 10, deleted: true),
            Voucher(newest.Id, "OTHER", 20));
        await db.SaveChangesAsync();

        var service = new VoucherProgramService(db);
        var firstPage = await service.GetVoucherPrograms(new Request.GetVoucherProgramsRequest { PageSize = 1 });
        Assert.Equal(newest.Id, Assert.Single(firstPage.Value.Items.Cast<Response.VoucherProgramResponse>()).Id);
        Assert.True(firstPage.Value.HasNextPage);

        var secondPage = await service.GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { PageIndex = 2, PageSize = 1 });
        var item = Assert.Single(secondPage.Value.Items.Cast<Response.VoucherProgramResponse>());
        Assert.Equal(middle.Id, item.Id);
        Assert.Equal(3, item.VoucherCount);
        Assert.Equal(2, item.UsedVoucherCount);
        Assert.Equal(3, secondPage.Value.TotalCount);
        Assert.True(secondPage.Value.HasNextPage);
        Assert.True(secondPage.Value.HasPreviousPage);
        Assert.Equal(middle.StartAt, item.StartAt);
        Assert.Equal(middle.EndAt, item.EndAt);
        Assert.Equal(middle.MinOrderAmount, item.MinOrderAmount);
        Assert.Equal(middle.MaxOrderAmount, item.MaxOrderAmount);
    }

    [Fact]
    public async Task InactiveFilter_ReturnsDraftWithZeroCounts()
    {
        await using var db = CreateDbContext();
        db.VoucherPrograms.AddRange(Program("Active", 1, active: true), Program("Draft", 2));
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db).GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { IsActive = false });

        var item = Assert.Single(result.Value.Items.Cast<Response.VoucherProgramResponse>());
        Assert.Equal("Draft", item.Name);
        Assert.False(item.IsActive);
        Assert.Equal(0, item.VoucherCount);
        Assert.Equal(0, item.UsedVoucherCount);
    }

    [Fact]
    public async Task EmptyList_NormalizesNonPositivePagination()
    {
        await using var db = CreateDbContext();
        var result = await new VoucherProgramService(db).GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { PageIndex = 0, PageSize = 0, Keyword = "  " });

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageIndex);
        Assert.Equal(20, result.Value.PageSize);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LargePage_DoesNotOverflowIntoEarlierPages()
    {
        await using var db = CreateDbContext();
        db.VoucherPrograms.Add(Program("One", 1));
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db).GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { PageIndex = int.MaxValue, PageSize = 20 });

        Assert.Empty(result.Value.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.False(result.Value.HasNextPage);
        Assert.True(result.Value.HasPreviousPage);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static VoucherProgramEntity Program(string name, int sequence, bool active = false, bool deleted = false) => new()
    {
        Id = Guid.NewGuid(), Name = name, IsActive = active, IsDeleted = deleted,
        StartAt = DateTimeOffset.Parse("2026-10-14T17:00:00Z"),
        EndAt = DateTimeOffset.Parse("2026-10-31T16:59:59Z"),
        MinOrderAmount = 200000m, MaxOrderAmount = 1000000m,
        CreatedAt = DateTimeOffset.Parse("2026-10-01T00:00:00Z").AddMinutes(sequence)
    };

    private static Voucher Voucher(Guid programId, string code, int usedCount, bool deleted = false) => new()
    {
        Id = Guid.NewGuid(), ProgramId = programId, Code = code,
        UsedCount = usedCount, MaxUsageCount = 100, IsDeleted = deleted,
        CreatedAt = DateTimeOffset.UtcNow
    };
}

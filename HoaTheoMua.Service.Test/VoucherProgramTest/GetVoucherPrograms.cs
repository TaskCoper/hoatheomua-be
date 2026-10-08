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
    [Theory]
    [InlineData(null, 2)]
    [InlineData(true, 1)]
    [InlineData(false, 1)]
    public async Task FiltersByNameActiveAndOptionalDeletionState(bool? isDelete, int expectedCount)
    {
        await using var db = CreateDbContext();
        db.VoucherPrograms.AddRange(
            Program("SALE 20/10", 1, active: true),
            Program("Sale 20/10 old", 2, active: true, deleted: true),
            Program("sale 20/10 draft", 3),
            Program("Christmas", 4, active: true));
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db).GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { keyword = "  sAlE 20/10  ", isActive = true, isDelete = isDelete });

        var items = result.Value.Items.Cast<Response.VoucherProgramResponse>().ToArray();
        var expectedNames = isDelete switch
        {
            true => new[] { "Sale 20/10 old" },
            false => new[] { "SALE 20/10" },
            null => new[] { "Sale 20/10 old", "SALE 20/10" }
        };
        Assert.Equal(expectedNames, items.Select(x => x.Name));
        if (isDelete.HasValue)
            Assert.All(items, item => Assert.Equal(isDelete.Value, item.IsDeleted));
        else
        {
            Assert.True(items[0].IsDeleted);
            Assert.False(items[1].IsDeleted);
        }
        Assert.Equal(expectedCount, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageIndex);
        Assert.Equal(20, result.Value.PageSize);
        Assert.False(result.Value.HasNextPage);
        Assert.False(result.Value.HasPreviousPage);
    }

    [Fact]
    public async Task AdminList_IncludesExpiredAndDeletedProgramsAlongsideCurrentAndFuturePrograms()
    {
        await using var db = CreateDbContext();
        var now = DateTimeOffset.UtcNow;
        var expired = Program("Expired", 1, active: true);
        expired.StartAt = now.AddDays(-2);
        expired.EndAt = now.AddDays(-1);
        var deleted = Program("Deleted expired", 2, deleted: true);
        deleted.StartAt = expired.StartAt;
        deleted.EndAt = expired.EndAt;
        var current = Program("Current", 3, active: true);
        current.StartAt = now.AddDays(-1);
        current.EndAt = now.AddDays(1);
        var future = Program("Future", 4);
        future.StartAt = now.AddDays(1);
        future.EndAt = now.AddDays(2);
        db.VoucherPrograms.AddRange(expired, deleted, current, future);
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db).GetVoucherPrograms(new());

        var items = result.Value.Items.Cast<Response.VoucherProgramResponse>().ToArray();
        Assert.Equal(4, result.Value.TotalCount);
        Assert.Equal(new[] { future.Id, current.Id, deleted.Id, expired.Id }, items.Select(x => x.Id));
        Assert.True(items[2].IsDeleted);
        Assert.False(items[3].IsDeleted);
        Assert.Equal(expired.EndAt, items[3].EndAt);
        Assert.True(items[3].EndAt < now);
    }

    [Fact]
    public async Task PaginatesNewestFirst_AndReturnsOnlyProgramFields()
    {
        await using var db = CreateDbContext();
        var oldest = Program("Oldest", 1);
        var middle = Program("Middle", 2);
        var newest = Program("Newest", 3, deleted: true);
        db.VoucherPrograms.AddRange(oldest, middle, newest);
        db.Vouchers.AddRange(
            Voucher(middle.Id, "USED1", 3),
            Voucher(middle.Id, "USED2", 1),
            Voucher(middle.Id, "UNUSED", 0),
            Voucher(middle.Id, "DELETED", 10, deleted: true),
            Voucher(newest.Id, "OTHER", 20));
        await db.SaveChangesAsync();

        var service = new VoucherProgramService(db);
        var firstPage = await service.GetVoucherPrograms(new Request.GetVoucherProgramsRequest { pageSize = 1 });
        var firstItem = Assert.Single(firstPage.Value.Items.Cast<Response.VoucherProgramResponse>());
        Assert.Equal(newest.Id, firstItem.Id);
        Assert.True(firstItem.IsDeleted);
        Assert.True(firstPage.Value.HasNextPage);

        var secondPage = await service.GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { pageIndex = 2, pageSize = 1 });
        var item = Assert.Single(secondPage.Value.Items.Cast<Response.VoucherProgramResponse>());
        Assert.Equal(middle.Id, item.Id);
        var json = System.Text.Json.JsonSerializer.SerializeToElement(item,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        Assert.False(json.TryGetProperty("usedVoucherCount", out _));
        Assert.False(json.TryGetProperty("voucherCount", out _));
        Assert.Equal(3, secondPage.Value.TotalCount);
        Assert.True(secondPage.Value.HasNextPage);
        Assert.True(secondPage.Value.HasPreviousPage);
        Assert.Equal(middle.StartAt, item.StartAt);
        Assert.Equal(middle.EndAt, item.EndAt);
        Assert.Equal(middle.MinOrderAmount, item.MinOrderAmount);
        Assert.Equal(middle.MaxOrderAmount, item.MaxOrderAmount);
    }

    [Fact]
    public async Task InactiveFilter_ReturnsDraft()
    {
        await using var db = CreateDbContext();
        db.VoucherPrograms.AddRange(Program("Active", 1, active: true), Program("Draft", 2));
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db).GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { isActive = false });

        var item = Assert.Single(result.Value.Items.Cast<Response.VoucherProgramResponse>());
        Assert.Equal("Draft", item.Name);
        Assert.False(item.IsActive);
    }

    [Fact]
    public async Task EmptyList_NormalizesNonPositivePagination()
    {
        await using var db = CreateDbContext();
        var result = await new VoucherProgramService(db).GetVoucherPrograms(
            new Request.GetVoucherProgramsRequest { pageIndex = 0, pageSize = 0, keyword = "  " });

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
            new Request.GetVoucherProgramsRequest { pageIndex = int.MaxValue, pageSize = 20 });

        Assert.Empty(result.Value.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.False(result.Value.HasNextPage);
        Assert.True(result.Value.HasPreviousPage);
    }

    [Fact]
    public async Task OversizedPage_IsBounded_AndNextPageKeepsCorrectOffset()
    {
        await using var db = CreateDbContext();
        db.VoucherPrograms.AddRange(Enumerable.Range(1, 125).Select(i => Program($"Program {i}", i)));
        await db.SaveChangesAsync();
        var service = new VoucherProgramService(db);

        var first = await service.GetVoucherPrograms(new() { pageSize = int.MaxValue });
        var second = await service.GetVoucherPrograms(new() { pageIndex = 2, pageSize = int.MaxValue });

        Assert.Equal(100, first.Value.PageSize);
        Assert.Equal(125, first.Value.TotalCount);
        Assert.Equal(100, first.Value.Items.Count());
        Assert.True(first.Value.HasNextPage);
        Assert.Equal(100, second.Value.PageSize);
        Assert.Equal(25, second.Value.Items.Count());
        Assert.Equal("Program 25", second.Value.Items.Cast<Response.VoucherProgramResponse>().First().Name);
        Assert.False(second.Value.HasNextPage);
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

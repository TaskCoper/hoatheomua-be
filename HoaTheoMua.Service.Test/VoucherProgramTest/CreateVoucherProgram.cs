using HoaTheoMua.Repository;
using HoaTheoMua.Service.VoucherProgram;
using Microsoft.EntityFrameworkCore;
using Xunit;
using VoucherProgramService = HoaTheoMua.Service.VoucherProgram.Service;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class CreateVoucherProgram
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(200000, 800000)]
    [InlineData(0, 0)]
    [InlineData(200000, 0)]
    public async Task CreatesInactiveProgramWithoutVouchers_AndConvertsScheduleToUtc(int? min, int? max)
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var request = new Request.CreateVoucherProgramRequest
        {
            Name = "  20/10  ",
            StartAt = "2026-10-15T00:00:00+07:00",
            EndAt = "2026-10-31T23:59:59+07:00",
            MinOrderAmount = min,
            MaxOrderAmount = max
        };
        var before = DateTimeOffset.UtcNow;

        var result = await new VoucherProgramService(db).CreateVoucherProgram(request);
        db.ChangeTracker.Clear();
        var saved = await db.VoucherPrograms.SingleAsync();

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(result.Id, saved.Id);
        Assert.Equal("20/10", saved.Name);
        Assert.Equal(saved.Name, result.Name);
        Assert.False(saved.IsActive);
        Assert.False(result.IsActive);
        Assert.False(saved.IsDeleted);
        Assert.Null(saved.UpdatedAt);
        Assert.Null(result.UpdatedAt);
        Assert.InRange(saved.CreatedAt, before, DateTimeOffset.UtcNow);
        Assert.Equal(saved.CreatedAt, result.CreatedAt);
        Assert.Equal(TimeSpan.Zero, saved.StartAt.Offset);
        Assert.Equal(TimeSpan.Zero, saved.EndAt.Offset);
        Assert.Equal(DateTimeOffset.Parse("2026-10-14T17:00:00+00:00"), saved.StartAt);
        Assert.Equal(DateTimeOffset.Parse("2026-10-31T16:59:59+00:00"), saved.EndAt);
        Assert.Equal(saved.StartAt, result.StartAt);
        Assert.Equal(saved.EndAt, result.EndAt);
        Assert.Equal(request.MinOrderAmount, saved.MinOrderAmount);
        Assert.Equal(request.MaxOrderAmount == 0m ? null : request.MaxOrderAmount, saved.MaxOrderAmount);
        Assert.Equal(saved.MinOrderAmount, result.MinOrderAmount);
        Assert.Equal(saved.MaxOrderAmount, result.MaxOrderAmount);
        Assert.Empty(await db.Vouchers.ToListAsync());
    }
}

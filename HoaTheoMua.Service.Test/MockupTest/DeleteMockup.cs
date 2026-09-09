using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MockupEntity = HoaTheoMua.Repository.Entity.Mockup;
using MockupService = HoaTheoMua.Service.Mockup.Service;

namespace HoaTheoMua.Service.Test.MockupTest;

public class DeleteMockup
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteMockup_ExistingMockup_SoftDeletesWithoutChangingStatusOrImage(bool isActive)
    {
        await using var db = CreateDbContext();
        var mockup = Mockup(isActive);
        await db.Mockups.AddAsync(mockup);
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteMockup(mockup.Id);
        var saved = await db.Mockups.SingleAsync(x => x.Id == mockup.Id);

        Assert.Equal(mockup.Id, result.Id);
        Assert.Equal("Mockup Sinh Nhật 1", result.Name);
        Assert.True(result.IsDeleted);
        Assert.True(saved.IsDeleted);
        Assert.Equal(isActive, saved.IsActive);
        Assert.Equal("https://storage.example.com/mockups/test.jpg", saved.ImageUrl);
        Assert.Equal(mockup.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task DeleteMockup_UnknownId_ThrowsNotFoundException()
    {
        await using var db = CreateDbContext();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(db).DeleteMockup(Guid.NewGuid()));

        Assert.Equal("Mockup không tồn tại.", exception.Message);
        Assert.Empty(db.Mockups);
    }

    [Fact]
    public async Task DeleteMockup_AlreadyDeletedMockup_ThrowsAndKeepsRecord()
    {
        await using var db = CreateDbContext();
        var mockup = Mockup(isActive: true, isDeleted: true);
        await db.Mockups.AddAsync(mockup);
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => CreateService(db).DeleteMockup(mockup.Id));
        var saved = await db.Mockups.SingleAsync(x => x.Id == mockup.Id);

        Assert.Equal("Mockup đã bị xóa trước đó.", exception.Message);
        Assert.True(saved.IsDeleted);
        Assert.True(saved.IsActive);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static MockupService CreateService(AppDbContext dbContext) =>
        new(dbContext, new HttpContextAccessor());

    private static MockupEntity Mockup(bool isActive, bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Mockup Sinh Nhật 1",
        ImageUrl = "https://storage.example.com/mockups/test.jpg",
        IsActive = isActive,
        IsDeleted = isDeleted,
        CreatedAt = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero)
    };
}

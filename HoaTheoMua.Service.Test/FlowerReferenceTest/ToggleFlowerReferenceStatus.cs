using HoaTheoMua.Repository;
using FlowerReferenceEntity = HoaTheoMua.Repository.Entity.FlowerReference;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FlowerReferenceService = HoaTheoMua.Service.FlowerReference.Service;

namespace HoaTheoMua.Service.Test.FlowerReferenceTest;

public class ToggleFlowerReferenceStatus
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ToggleMockupStatus_ActiveMockup_FlipsStatusAndUpdatesTimestamp(bool initialStatus, bool expectedStatus)
    {
        await using var db = CreateDbContext();
        var mockup = Mockup(isActive: initialStatus);
        await db.FlowerReferences.AddAsync(mockup);
        await db.SaveChangesAsync();
        var before = DateTimeOffset.UtcNow;

        var result = await CreateService(db).ToggleFlowerReferenceStatus(mockup.Id);
        var saved = await db.FlowerReferences.SingleAsync(x => x.Id == mockup.Id);

        Assert.Equal(expectedStatus, result.IsActive);
        Assert.Equal(expectedStatus, saved.IsActive);
        Assert.False(saved.IsDeleted);
        Assert.NotNull(result.UpdatedAt);
        Assert.True(result.UpdatedAt >= before);
        Assert.Equal(result.UpdatedAt, saved.UpdatedAt);
    }

    [Fact]
    public async Task ToggleMockupStatus_UnknownId_ThrowsNotFoundException()
    {
        await using var db = CreateDbContext();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(db).ToggleFlowerReferenceStatus(Guid.NewGuid()));

        Assert.Equal("FlowerReference không hợp lệ hoặc đã bị xóa.", exception.Message);
        Assert.Empty(db.FlowerReferences);
    }

    [Fact]
    public async Task ToggleMockupStatus_DeletedMockup_ThrowsAndDoesNotChangeIt()
    {
        await using var db = CreateDbContext();
        var mockup = Mockup(isActive: true, isDeleted: true);
        await db.FlowerReferences.AddAsync(mockup);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(db).ToggleFlowerReferenceStatus(mockup.Id));

        var saved = await db.FlowerReferences.SingleAsync(x => x.Id == mockup.Id);
        Assert.True(saved.IsActive);
        Assert.True(saved.IsDeleted);
        Assert.Null(saved.UpdatedAt);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static FlowerReferenceService CreateService(AppDbContext dbContext) =>
        new(dbContext, new HttpContextAccessor());

    private static FlowerReferenceEntity Mockup(bool isActive, bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Mockup Sinh Nhật 1",
        ImageUrl = "https://storage.example.com/mockups/test.jpg",
        IsActive = isActive,
        IsDeleted = isDeleted,
        Type = HoaTheoMua.Repository.Enum.FlowerReferenceType.Flower,
        CreatedAt = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero)
    };
}

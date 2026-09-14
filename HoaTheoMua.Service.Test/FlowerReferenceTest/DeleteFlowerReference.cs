using HoaTheoMua.Repository;
using FlowerReferenceEntity = HoaTheoMua.Repository.Entity.FlowerReference;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FlowerReferenceService = HoaTheoMua.Service.FlowerReference.Service;

namespace HoaTheoMua.Service.Test.FlowerReferenceTest;

public class DeleteFlowerReference
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteMockup_ExistingMockup_SoftDeletesWithoutChangingStatusOrImage(bool isActive)
    {
        await using var db = CreateDbContext();
        var mockup = Mockup(isActive);
        await db.FlowerReferences.AddAsync(mockup);
        await db.SaveChangesAsync();

        var result = await CreateService(db).DeleteFlowerReference(mockup.Id);
        var saved = await db.FlowerReferences.SingleAsync(x => x.Id == mockup.Id);

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
            CreateService(db).DeleteFlowerReference(Guid.NewGuid()));

        Assert.Equal("FlowerReference không tồn tại.", exception.Message);
        Assert.Empty(db.FlowerReferences);
    }

    [Fact]
    public async Task DeleteMockup_AlreadyDeletedMockup_ThrowsAndKeepsRecord()
    {
        await using var db = CreateDbContext();
        var mockup = Mockup(isActive: true, isDeleted: true);
        await db.FlowerReferences.AddAsync(mockup);
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => CreateService(db).DeleteFlowerReference(mockup.Id));
        var saved = await db.FlowerReferences.SingleAsync(x => x.Id == mockup.Id);

        Assert.Equal("FlowerReference không tồn tại.", exception.Message);
        Assert.True(saved.IsDeleted);
        Assert.True(saved.IsActive);
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

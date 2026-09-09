using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Mockup;
using HoaTheoMua.Service.Mockup.Validate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MockupEntity = HoaTheoMua.Repository.Entity.Mockup;
using MockupService = HoaTheoMua.Service.Mockup.Service;

namespace HoaTheoMua.Service.Test.MockupTest;

public class CreateMockup
{
    [Fact]
    public async Task CreateMockup_ValidRequest_SavesMockupWithInactiveAndNotDeletedDefaults()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var before = DateTimeOffset.UtcNow;

        var result = await service.CreateMockup(new Request.CreateMockupRequest
        {
            Name = "  Mockup Sinh Nhật 1  ",
            Description = "Mockup bó hoa sinh nhật với màu sắc tươi sáng",
            ImageUrl = "https://storage.example.com/mockups/test.jpg"
        });

        var saved = Assert.Single(db.Mockups);
        Assert.Equal(saved.Id, result.Id);
        Assert.Equal("Mockup Sinh Nhật 1", result.Name);
        Assert.Equal("https://storage.example.com/mockups/test.jpg", result.ImageUrl);
        Assert.False(result.IsActive);
        Assert.False(result.IsDeleted);
        Assert.Null(result.UpdatedAt);
        Assert.True(result.CreatedAt >= before);
        Assert.False(saved.IsActive);
        Assert.False(saved.IsDeleted);
    }

    [Fact]
    public async Task CreateMockup_DuplicateActiveNameIgnoringCaseAndWhitespace_ThrowsConflict()
    {
        await using var db = CreateDbContext();
        await db.Mockups.AddAsync(Mockup("Mockup Sinh Nhật 1"));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.CreateMockup(new Request.CreateMockupRequest
        {
            Name = "  mockup   sinh nhật 1  ",
            ImageUrl = "https://storage.example.com/mockups/test.jpg"
        }));

        Assert.Equal("Tên Mockup đã tồn tại", exception.Message);
        Assert.Single(db.Mockups);
    }

    [Fact]
    public async Task CreateMockup_DeletedName_CanBeReused()
    {
        await using var db = CreateDbContext();
        await db.Mockups.AddAsync(Mockup("Mockup cũ", isDeleted: true));
        await db.SaveChangesAsync();

        await CreateService(db).CreateMockup(new Request.CreateMockupRequest
        {
            Name = "Mockup cũ",
            ImageUrl = "https://storage.example.com/mockups/test.jpg"
        });

        Assert.Equal(2, await db.Mockups.CountAsync());
        Assert.Single(await db.Mockups.Where(x => !x.IsDeleted).ToListAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateMockupValidator_MissingOrWhitespaceName_IsInvalid(string? name)
    {
        var result = new CreateMockupValidator().Validate(new Request.CreateMockupRequest
        {
            Name = name,
            ImageUrl = "https://storage.example.com/mockups/test.jpg"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(Request.CreateMockupRequest.Name));
    }

    [Fact]
    public void CreateMockupValidator_NameOrDescriptionTooLong_IsInvalid()
    {
        var validator = new CreateMockupValidator();

        var longName = validator.Validate(new Request.CreateMockupRequest
        {
            Name = new string('a', 51),
            ImageUrl = "https://storage.example.com/mockups/test.jpg"
        });
        var longDescription = validator.Validate(new Request.CreateMockupRequest
        {
            Name = "Mockup Test",
            Description = new string('a', 201),
            ImageUrl = "https://storage.example.com/mockups/test.jpg"
        });

        Assert.Contains(longName.Errors, x => x.PropertyName == nameof(Request.CreateMockupRequest.Name));
        Assert.Contains(longDescription.Errors, x => x.PropertyName == nameof(Request.CreateMockupRequest.Description));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-url")]
    public void CreateMockupValidator_MissingOrInvalidImageUrl_IsInvalid(string? imageUrl)
    {
        var result = new CreateMockupValidator().Validate(new Request.CreateMockupRequest
        {
            Name = "Mockup Test",
            ImageUrl = imageUrl
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(Request.CreateMockupRequest.ImageUrl));
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static MockupService CreateService(AppDbContext dbContext) =>
        new(dbContext, new HttpContextAccessor());

    private static MockupEntity Mockup(string name, bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        ImageUrl = "https://storage.example.com/mockups/existing.jpg",
        IsDeleted = isDeleted,
        CreatedAt = DateTimeOffset.UtcNow
    };
}

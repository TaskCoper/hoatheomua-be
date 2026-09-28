using HoaTheoMua.Repository;
using FlowerReferenceEntity = HoaTheoMua.Repository.Entity.FlowerReference;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.FlowerReference;
using HoaTheoMua.Service.FlowerReference.Validate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FlowerReferenceService = HoaTheoMua.Service.FlowerReference.Service;

namespace HoaTheoMua.Service.Test.FlowerReferenceTest;

public class CreateFlowerReference
{
    [Fact]
    public async Task CreateMockup_ValidRequest_SavesMockupWithInactiveAndNotDeletedDefaults()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var before = DateTimeOffset.UtcNow;

        var result = await service.CreateFlowerReference(new Request.CreateFlowerReferenceRequest
        {
            Name = "  Mockup Sinh Nhật 1  ",
            Description = "Mockup bó hoa sinh nhật với màu sắc tươi sáng",
            ImageUrl = "https://storage.example.com/mockups/test.jpg",
            Type = "flower"
        });

        var saved = Assert.Single(db.FlowerReferences);
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
        await db.FlowerReferences.AddAsync(Mockup("Mockup Sinh Nhật 1"));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.CreateFlowerReference(new Request.CreateFlowerReferenceRequest
        {
            Name = "  mockup   sinh nhật 1  ",
            ImageUrl = "https://storage.example.com/mockups/test.jpg",
            Type = "flower"
        }));

        Assert.Equal("Tên hình tham khảo đã tồn tại. Vui lòng chọn tên khác.", exception.Message);
        Assert.Single(db.FlowerReferences);
    }

    [Fact]
    public async Task CreateMockup_DeletedName_CanBeReused()
    {
        await using var db = CreateDbContext();
        await db.FlowerReferences.AddAsync(Mockup("Mockup cũ", isDeleted: true));
        await db.SaveChangesAsync();

        await CreateService(db).CreateFlowerReference(new Request.CreateFlowerReferenceRequest
        {
            Name = "Mockup cũ",
            ImageUrl = "https://storage.example.com/mockups/test.jpg",
            Type = "flower"
        });

        Assert.Equal(2, await db.FlowerReferences.CountAsync());
        Assert.Single(await db.FlowerReferences.Where(x => !x.IsDeleted).ToListAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateMockupValidator_MissingOrWhitespaceName_IsInvalid(string? name)
    {
        var result = new CreateFlowerReferenceValidator().Validate(new Request.CreateFlowerReferenceRequest
        {
            Name = name,
            ImageUrl = "https://storage.example.com/mockups/test.jpg",
            Type = "flower"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(Request.CreateFlowerReferenceRequest.Name));
    }

    [Fact]
    public void CreateMockupValidator_NameOrDescriptionTooLong_IsInvalid()
    {
        var validator = new CreateFlowerReferenceValidator();

        var longName = validator.Validate(new Request.CreateFlowerReferenceRequest
        {
            Name = new string('a', 51),
            ImageUrl = "https://storage.example.com/mockups/test.jpg",
            Type = "flower"
        });
        var longDescription = validator.Validate(new Request.CreateFlowerReferenceRequest
        {
            Name = "Mockup Test",
            Description = new string('a', 201),
            ImageUrl = "https://storage.example.com/mockups/test.jpg",
            Type = "flower"
        });

        Assert.Contains(longName.Errors, x => x.PropertyName == nameof(Request.CreateFlowerReferenceRequest.Name));
        Assert.Contains(longDescription.Errors, x => x.PropertyName == nameof(Request.CreateFlowerReferenceRequest.Description));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-url")]
    public void CreateMockupValidator_MissingOrInvalidImageUrl_IsInvalid(string? imageUrl)
    {
        var result = new CreateFlowerReferenceValidator().Validate(new Request.CreateFlowerReferenceRequest
        {
            Name = "Mockup Test",
            ImageUrl = imageUrl,
            Type = "flower"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(Request.CreateFlowerReferenceRequest.ImageUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0")]
    [InlineData("invalid")]
    public void CreateFlowerReferenceValidator_InvalidType_IsInvalid(string? type)
    {
        var result = new CreateFlowerReferenceValidator().Validate(new Request.CreateFlowerReferenceRequest
        {
            Name = "Flower Reference",
            ImageUrl = "https://storage.example.com/mockups/test.jpg",
            Type = type
        });

        Assert.Contains(result.Errors, x => x.PropertyName == nameof(Request.CreateFlowerReferenceRequest.Type));
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static FlowerReferenceService CreateService(AppDbContext dbContext) =>
        new(dbContext, new HttpContextAccessor());

    private static FlowerReferenceEntity Mockup(string name, bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        ImageUrl = "https://storage.example.com/mockups/existing.jpg",
        IsDeleted = isDeleted,
        Type = HoaTheoMua.Repository.Enum.FlowerReferenceType.Flower,
        CreatedAt = DateTimeOffset.UtcNow
    };
}

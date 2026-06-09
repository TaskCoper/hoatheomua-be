using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class CategoryNewsSeed
{
    public static void SeedCategoryNews( this EntityTypeBuilder<CategoryNews> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        builder.HasData(
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), Title = "Technology", UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ParentCategoryNewsId = null, CreatedAt = baseDate, IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), Title = "Software Development", UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ParentCategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444441"), CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444443"), Title = "AI & Machine Learning", UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ParentCategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444441"), CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Title = "Health & Wellness", UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ParentCategoryNewsId = null, CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444445"), Title = "Nutrition", UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ParentCategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444444"), CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444446"), Title = "Fitness", UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ParentCategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444444"), CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444447"), Title = "Finance", UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ParentCategoryNewsId = null, CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444448"), Title = "Investing", UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ParentCategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444447"), CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444449"), Title = "Entertainment", UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ParentCategoryNewsId = null, CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new CategoryNews { Id = Guid.Parse("44444444-4444-4444-4444-444444444450"), Title = "Movies", UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ParentCategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444449"), CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

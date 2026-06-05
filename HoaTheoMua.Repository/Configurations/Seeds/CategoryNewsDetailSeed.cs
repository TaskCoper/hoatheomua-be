using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class CategoryNewsDetailSeed
{
    public static void SeedCategoryNewsDetail(this EntityTypeBuilder<CategoryNewsDetail> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        builder.HasData(
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444442"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777771"), CreatedAt = baseDate, IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111112"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444443"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777772"), CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111113"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444445"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777773"), CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111114"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444446"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777774"), CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111115"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444448"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777775"), CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111116"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444450"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777776"), CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111117"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444447"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777777"), CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111118"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444441"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777778"), CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111119"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444444"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777779"), CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new CategoryNewsDetail { Id = Guid.Parse("c1111111-1111-1111-1111-111111111120"), CategoryNewsId = Guid.Parse("44444444-4444-4444-4444-444444444449"), NewsId = Guid.Parse("77777777-7777-7777-7777-777777777780"), CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

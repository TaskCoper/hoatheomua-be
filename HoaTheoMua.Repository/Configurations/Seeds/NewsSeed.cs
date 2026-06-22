using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class NewsSeed
{
    public static void SeedNews(this EntityTypeBuilder<News> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        // Author* are the profile snapshot; in real writes they come from the JWT claims.
        // Seeded UserIds map to the legacy dev accounts: 1111=Admin, 2222=Staff, 3333=Normal.
        builder.HasData(
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777771"), Title = "C# 12 Released", Description = "New features in C# 12 include primary constructors...", ViewCount = 100, CoverImage = "https://example.com/csharp.jpg", Status = NewsStatus.Published, UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), AuthorName = "Admin User", AuthorEmail = "admin@example.com", CreatedAt = baseDate, IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777772"), Title = "AI is taking over", Description = "Generative AI is changing the landscape of software engineering.", ViewCount = 550, CoverImage = "https://example.com/ai.jpg", Status = NewsStatus.Published, UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), AuthorName = "Admin User", AuthorEmail = "admin@example.com", CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777773"), Title = "Healthy Eating Habits", Description = "Top 10 tips for maintaining a balanced diet.", ViewCount = 300, CoverImage = "https://example.com/health.jpg", Status = NewsStatus.Published, UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), AuthorName = "Staff User", AuthorEmail = "staff@example.com", CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777774"), Title = "Workout Routines", Description = "Quick 15-minute workouts for busy people.", ViewCount = 120, CoverImage = "https://example.com/workout.jpg", Status = NewsStatus.Published, UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), AuthorName = "Staff User", AuthorEmail = "staff@example.com", CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777775"), Title = "Stock Market Surge", Description = "Tech stocks reach an all-time high.", ViewCount = 800, CoverImage = "https://example.com/finance.jpg", Status = NewsStatus.Draft, UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), AuthorName = "Normal User", AuthorEmail = "user@example.com", CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777776"), Title = "Top 10 Movies of 2024", Description = "A review of the best movies released this year.", ViewCount = 1500, CoverImage = "https://example.com/movies.jpg", Status = NewsStatus.Published, UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), AuthorName = "Normal User", AuthorEmail = "user@example.com", CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Title = "Real Estate Trends", Description = "What to expect in the housing market.", ViewCount = 420, CoverImage = "https://example.com/realestate.jpg", Status = NewsStatus.Published, UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), AuthorName = "Admin User", AuthorEmail = "admin@example.com", CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777778"), Title = "Upcoming Tech Gadgets", Description = "The most anticipated gadgets of the season.", ViewCount = 750, CoverImage = "https://example.com/gadgets.jpg", Status = NewsStatus.Draft, UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), AuthorName = "Staff User", AuthorEmail = "staff@example.com", CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777779"), Title = "Mental Health Awareness", Description = "Importance of taking mental health seriously.", ViewCount = 600, CoverImage = "https://example.com/mentalhealth.jpg", Status = NewsStatus.Published, UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), AuthorName = "Admin User", AuthorEmail = "admin@example.com", CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new News { Id = Guid.Parse("77777777-7777-7777-7777-777777777780"), Title = "Travel Destinations", Description = "Must-visit places for your next vacation.", ViewCount = 950, CoverImage = "https://example.com/travel.jpg", Status = NewsStatus.Published, UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), AuthorName = "Staff User", AuthorEmail = "staff@example.com", CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

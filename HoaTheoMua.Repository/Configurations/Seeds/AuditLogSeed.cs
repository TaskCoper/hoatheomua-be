using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class AuditLogSeed
{
    public static void SeedAudit(this ModelBuilder modelBuilder)
    {
        var baseDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        modelBuilder.Entity<AuditLog>().HasData(
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111111"),
                Action = "Create",
                Description = "Created a new user",
                OldValue = "",
                NewValue = "User Data",
                EntityType = "User",
                EntityId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = baseDate,
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111112"),
                Action = "Update",
                Description = "Updated news title",
                OldValue = "Old Title",
                NewValue = "New Title",
                EntityType = "News",
                EntityId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CreatedAt = baseDate.AddDays(1),
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111113"),
                Action = "Delete",
                Description = "Deleted old category",
                OldValue = "Category Data",
                NewValue = "",
                EntityType = "CategoryNews",
                EntityId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = baseDate.AddDays(2),
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111114"),
                Action = "Create",
                Description = "Created news article",
                OldValue = "",
                NewValue = "News Data",
                EntityType = "News",
                EntityId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CreatedAt = baseDate.AddDays(3),
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111115"),
                Action = "Login",
                Description = "User logged in",
                OldValue = "",
                NewValue = "Session Started",
                EntityType = "User",
                EntityId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CreatedAt = baseDate.AddDays(4),
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111116"),
                Action = "Update",
                Description = "Updated user profile",
                OldValue = "Old Name",
                NewValue = "New Name",
                EntityType = "User",
                EntityId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CreatedAt = baseDate.AddDays(5),
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111117"),
                Action = "Create",
                Description = "Added category detail",
                OldValue = "",
                NewValue = "Mapping Data",
                EntityType = "CategoryNewsDetail",
                EntityId = Guid.Parse("c1111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = baseDate.AddDays(6),
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111118"),
                Action = "Create",
                Description = "Added company address",
                OldValue = "",
                NewValue = "Address Data",
                EntityType = "CompanyAddress",
                EntityId = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = baseDate.AddDays(7),
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111119"),
                Action = "ResetPassword",
                Description = "Requested password reset",
                OldValue = "",
                NewValue = "Token Generated",
                EntityType = "User",
                EntityId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CreatedAt = baseDate.AddDays(8),
                IsDeleted = false
            },
            new AuditLog
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111120"),
                Action = "Logout",
                Description = "User logged out",
                OldValue = "Session Active",
                NewValue = "Session Ended",
                EntityType = "User",
                EntityId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CreatedAt = baseDate.AddDays(9),
                IsDeleted = false
            }
        );
    }
}
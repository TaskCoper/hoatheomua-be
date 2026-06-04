using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class UserSeed
{
    public static void SeedUser(this ModelBuilder modelBuilder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        modelBuilder.Entity<User>().HasData(
            new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Admin User", Email = "admin@example.com", HashPassword = "hashed_password_1", PhoneNumber = "0123456780", Role = Role.Admin, CreatedAt = baseDate, IsDeleted = false },
            new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Staff User", Email = "staff@example.com", HashPassword = "hashed_password_2", PhoneNumber = "0123456781", Role = Role.Staff, CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Normal User", Email = "user@example.com", HashPassword = "hashed_password_3", PhoneNumber = "0123456782", Role = Role.User, CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), Name = "Alice Smith", Email = "alice@example.com", HashPassword = "hashed_password_4", PhoneNumber = "0123456783", Role = Role.User, CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333335"), Name = "Bob Johnson", Email = "bob@example.com", HashPassword = "hashed_password_5", PhoneNumber = "0123456784", Role = Role.User, CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333336"), Name = "Charlie Brown", Email = "charlie@example.com", HashPassword = "hashed_password_6", PhoneNumber = "0123456785", Role = Role.User, CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333337"), Name = "Diana Prince", Email = "diana@example.com", HashPassword = "hashed_password_7", PhoneNumber = "0123456786", Role = Role.User, CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333338"), Name = "Evan Wright", Email = "evan@example.com", HashPassword = "hashed_password_8", PhoneNumber = "0123456787", Role = Role.User, CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333339"), Name = "Fiona Gallagher", Email = "fiona@example.com", HashPassword = "hashed_password_9", PhoneNumber = "0123456788", Role = Role.User, CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333340"), Name = "George Lucas", Email = "george@example.com", HashPassword = "hashed_password_10", PhoneNumber = "0123456789", Role = Role.User, CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

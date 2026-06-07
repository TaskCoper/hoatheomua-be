using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class UserSeed
{
    public static void SeedUser(this EntityTypeBuilder<User> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        builder.HasData(
            new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), FirstName = "Admin", LastName = "User",Email = "admin@example.com", IsVerify = true,HashPassword = "hashed_password_1", PhoneNumber = "0123456780", Role = Role.Admin, CreatedAt = baseDate, IsDeleted = false },
            new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), FirstName = "Staff", LastName = "User",Email = "staff@example.com", IsVerify = true,HashPassword = "hashed_password_2", PhoneNumber = "0123456781", Role = Role.Staff, CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), FirstName = "Normal", LastName = "User",Email = "user@example.com", IsVerify = true,HashPassword = "hashed_password_3", PhoneNumber = "0123456782", Role = Role.User, CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), FirstName = "Alice", LastName = "Smith",Email = "alice@example.com", IsVerify = true,HashPassword = "hashed_password_4", PhoneNumber = "0123456783", Role = Role.User, CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333335"), FirstName = "Bob", LastName = "Johnson",Email = "bob@example.com", IsVerify = true,HashPassword = "hashed_password_5", PhoneNumber = "0123456784", Role = Role.User, CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333336"), FirstName = "Charlie", LastName = "Brown",Email = "charlie@example.com", IsVerify = true,HashPassword = "hashed_password_6", PhoneNumber = "0123456785", Role = Role.User, CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333337"), FirstName = "Diana", LastName = "Prince",Email = "diana@example.com", IsVerify = true,HashPassword = "hashed_password_7", PhoneNumber = "0123456786", Role = Role.User, CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333338"), FirstName = "Evan", LastName = "Wright",Email = "evan@example.com", IsVerify = true,HashPassword = "hashed_password_8", PhoneNumber = "0123456787", Role = Role.User, CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333339"), FirstName = "Fiona", LastName = "Gallagher",Email = "fiona@example.com", IsVerify = true,HashPassword = "hashed_password_9", PhoneNumber = "0123456788", Role = Role.User, CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333340"), FirstName = "George", LastName = "Lucas",Email = "george@example.com", IsVerify = true,HashPassword = "hashed_password_10", PhoneNumber = "0123456789", Role = Role.User, CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

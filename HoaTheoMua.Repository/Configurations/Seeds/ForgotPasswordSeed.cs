using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class ForgotPasswordSeed
{
    public static void SeedForgotPassword(EntityTypeBuilder<ForgotPassword> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        builder.HasData(
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111111"), Otp = "123456", IsUsed = true, ExpiresAt = baseDate.AddMinutes(15), UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedAt = baseDate, IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111112"), Otp = "654321", IsUsed = false, ExpiresAt = baseDate.AddDays(1).AddMinutes(15), UserId = Guid.Parse("33333333-3333-3333-3333-333333333334"), CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111113"), Otp = "111222", IsUsed = true, ExpiresAt = baseDate.AddDays(2).AddMinutes(15), UserId = Guid.Parse("33333333-3333-3333-3333-333333333335"), CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111114"), Otp = "333444", IsUsed = false, ExpiresAt = baseDate.AddDays(3).AddMinutes(15), UserId = Guid.Parse("33333333-3333-3333-3333-333333333336"), CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111115"), Otp = "555666", IsUsed = true, ExpiresAt = baseDate.AddDays(4).AddMinutes(15), UserId = Guid.Parse("33333333-3333-3333-3333-333333333337"), CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111116"), Otp = "777888", IsUsed = false, ExpiresAt = baseDate.AddDays(5).AddMinutes(15), UserId = Guid.Parse("33333333-3333-3333-3333-333333333338"), CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111117"), Otp = "999000", IsUsed = true, ExpiresAt = baseDate.AddDays(6).AddMinutes(15), UserId = Guid.Parse("33333333-3333-3333-3333-333333333339"), CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111118"), Otp = "121212", IsUsed = false, ExpiresAt = baseDate.AddDays(7).AddMinutes(15), UserId = Guid.Parse("33333333-3333-3333-3333-333333333340"), CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111119"), Otp = "343434", IsUsed = true, ExpiresAt = baseDate.AddDays(8).AddMinutes(15), UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new ForgotPassword { Id = Guid.Parse("f1111111-1111-1111-1111-111111111120"), Otp = "565656", IsUsed = false, ExpiresAt = baseDate.AddDays(9).AddMinutes(15), UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

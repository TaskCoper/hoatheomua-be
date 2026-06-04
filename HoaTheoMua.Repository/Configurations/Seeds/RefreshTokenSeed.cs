using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class RefreshTokenSeed
{
    public static void SeedRefreshToken(this ModelBuilder modelBuilder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        modelBuilder.Entity<RefreshToken>().HasData(
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"), RefreshTokenHash = "hash1", IpAddress = "192.168.1.1", UserAgent = "Mozilla", DeviceLabel = "Desktop", ExpiredAt = baseDate.AddDays(30), UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), CreatedAt = baseDate, IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111112"), RefreshTokenHash = "hash2", IpAddress = "192.168.1.2", UserAgent = "Chrome", DeviceLabel = "Mobile", ExpiredAt = baseDate.AddDays(31), UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111113"), RefreshTokenHash = "hash3", IpAddress = "192.168.1.3", UserAgent = "Safari", DeviceLabel = "Tablet", ExpiredAt = baseDate.AddDays(32), UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111114"), RefreshTokenHash = "hash4", IpAddress = "192.168.1.4", UserAgent = "Edge", DeviceLabel = "Desktop", ExpiredAt = baseDate.AddDays(33), UserId = Guid.Parse("33333333-3333-3333-3333-333333333334"), CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111115"), RefreshTokenHash = "hash5", IpAddress = "192.168.1.5", UserAgent = "Firefox", DeviceLabel = "Mobile", ExpiredAt = baseDate.AddDays(34), UserId = Guid.Parse("33333333-3333-3333-3333-333333333335"), CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111116"), RefreshTokenHash = "hash6", IpAddress = "192.168.1.6", UserAgent = "Opera", DeviceLabel = "Desktop", ExpiredAt = baseDate.AddDays(35), UserId = Guid.Parse("33333333-3333-3333-3333-333333333336"), CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111117"), RefreshTokenHash = "hash7", IpAddress = "192.168.1.7", UserAgent = "Mozilla", DeviceLabel = "Tablet", ExpiredAt = baseDate.AddDays(36), UserId = Guid.Parse("33333333-3333-3333-3333-333333333337"), CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111118"), RefreshTokenHash = "hash8", IpAddress = "192.168.1.8", UserAgent = "Chrome", DeviceLabel = "Mobile", ExpiredAt = baseDate.AddDays(37), UserId = Guid.Parse("33333333-3333-3333-3333-333333333338"), CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111119"), RefreshTokenHash = "hash9", IpAddress = "192.168.1.9", UserAgent = "Safari", DeviceLabel = "Desktop", ExpiredAt = baseDate.AddDays(38), UserId = Guid.Parse("33333333-3333-3333-3333-333333333339"), CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new RefreshToken { Id = Guid.Parse("a1111111-1111-1111-1111-111111111120"), RefreshTokenHash = "hash10", IpAddress = "192.168.1.10", UserAgent = "Edge", DeviceLabel = "Tablet", ExpiredAt = baseDate.AddDays(39), UserId = Guid.Parse("33333333-3333-3333-3333-333333333340"), CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

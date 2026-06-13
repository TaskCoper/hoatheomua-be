using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class OrderSeed
{
    public static void SeedOrder(this EntityTypeBuilder<Order> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        builder.HasData(
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM1011" ,NhanhTrackingUrl = "https://123", TrackNumber = "TRK1001", NhanhOrderId = "NO14" ,Status = "Pending", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedAt = baseDate, IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM104311" ,NhanhTrackingUrl = "https://123", TrackNumber = "TRK1002", NhanhOrderId = "NO14" ,Status = "Paid", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333334"), CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444443"), AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM865" ,NhanhTrackingUrl = "https://123", TrackNumber = "TRK1003", NhanhOrderId = "NO14" ,Status = "Shipped", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333335"), CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM5543" ,NhanhTrackingUrl = "https://123", TrackNumber = "TRK1004", NhanhOrderId = "NO14" ,Status = "Delivered", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333336"), CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444445"),AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM6444" ,NhanhTrackingUrl = "https://123" , TrackNumber = "TRK1005", NhanhOrderId = "NO14" ,Status = "Cancelled", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333337"), CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444446"), AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM1234t1" ,NhanhTrackingUrl = "https://123", TrackNumber = "TRK1006", NhanhOrderId = "NO14" ,Status = "Pending", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333338"), CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444447"), AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM10234" ,NhanhTrackingUrl = "https://123" ,TrackNumber = "TRK1007", NhanhOrderId = "NO14" ,Status = "Paid", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333339"), CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444448"),AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM1435" ,NhanhTrackingUrl = "https://123" , TrackNumber = "TRK1008", NhanhOrderId = "NO14" ,Status = "Pending", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444449"),AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM1015" ,NhanhTrackingUrl = "https://123" , TrackNumber = "TRK1009", NhanhOrderId = "NO14" ,Status = "Paid", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new Order { Id = Guid.Parse("44444444-4444-4444-4444-444444444450"),AhahaMoveTrackingUrl = "https://ahahovetrackingUrl" , AhahaMoveOrderId = "AHM1013" ,NhanhTrackingUrl = "https://123 ", TrackNumber = "TRK1010", NhanhOrderId = "NO14" ,Status = "Shipped", Metadata = "{}", UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

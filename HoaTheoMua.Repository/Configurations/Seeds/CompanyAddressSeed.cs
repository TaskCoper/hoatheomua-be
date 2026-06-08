using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class CompanyAddressSeed
{
    public static void SeedCompanyAddress( this EntityTypeBuilder<CompanyAddress> builder)
    //this ở đây giúp biến cái hàm SeedCompanyAddress: thành một cái Extension Method của builder
    //do đó khi cần thì builder chấm ra là có
    //nếu ko dùng this, thì thì khi vào trong CompanyAddressConfiguration bạn phải CompanyAddressSeed.SeedCompanyAddress(builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        builder.HasData(
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"), AddressLine = "123 Main St", Ward = "Ward 1", District = "District 1",  City = "Ho Chi Minh City", Country = "Vietnam", CreatedAt = baseDate, IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111112"), AddressLine = "456 Elm St", Ward = "Ward 2", District = "District 2",  City = "Ho Chi Minh City", Country = "Vietnam", CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111113"), AddressLine = "789 Oak St", Ward = "Ward 3", District = "District 3",  City = "Hanoi", Country = "Vietnam", CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111114"), AddressLine = "321 Pine St", Ward = "Ward 4", District = "District 4",  City = "Danang", Country = "Vietnam", CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111115"), AddressLine = "654 Maple St", Ward = "Ward 5", District = "District 5",  City = "Hue", Country = "Vietnam", CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111116"), AddressLine = "987 Cedar St", Ward = "Ward 6", District = "District 6", City = "Can Tho", Country = "Vietnam", CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111117"), AddressLine = "159 Birch St", Ward = "Ward 7", District = "District 7", City = "Hai Phong", Country = "Vietnam", CreatedAt = baseDate.AddDays(6), IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111118"), AddressLine = "753 Spruce St", Ward = "Ward 8", District = "District 8", City = "Nha Trang", Country = "Vietnam", CreatedAt = baseDate.AddDays(7), IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111119"), AddressLine = "852 Fir St", Ward = "Ward 9", District = "District 9", City = "Da Lat", Country = "Vietnam", CreatedAt = baseDate.AddDays(8), IsDeleted = false },
            new CompanyAddress { Id = Guid.Parse("e1111111-1111-1111-1111-111111111120"), AddressLine = "951 Ash St", Ward = "Ward 10", District = "District 10", City = "Vung Tau", Country = "Vietnam", CreatedAt = baseDate.AddDays(9), IsDeleted = false }
        );
    }
}

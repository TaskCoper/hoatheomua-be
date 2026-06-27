using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class CompanyAddressSeed
{
    public static void SeedCompanyAddress(this EntityTypeBuilder<CompanyAddress> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        builder.HasData(
            new CompanyAddress
            {
                Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
                AddressLine = "Số 13 Đào Duy Anh, Phường 9",
                Ward = "Phường Sài Gòn",
                MapId = "h4gORrahdlpw0Z8t4761TmmZ0TqpdZTffrY5FJFeuZJptWwspnit5C7RQkC8Z-TfMbtCEaZch_dwpGdal162dna1U3ihlJsAc7dBVRZdnc_6XjVIBoQFyl3ClWiqXAOzX",
                City = "Hồ Chí Minh",
                FullName = "Hoa Theo Mua",
                PhoneNumber = "84908765432",
                Remarks = "Đến nơi lấy hàng đọc mã đơn để nhận hàng",
                Country = "Vietnam",
                Latitude = 10.806088849000048m,
                Longitude = 106.67715580100008m,
                CreatedAt = baseDate,
                UpdatedAt = null,
                IsDeleted = false
            }
        );
    }
}

using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class AuditLogSeed
{
    public static void SeedAudit(this EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasData(
  
        );
    }
}
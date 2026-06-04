using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(al => al.Description)
            .IsRequired();
        builder.Property(al => al.EntityType)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(al => al.EntityId)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(al => al.CreatedAt)
            .HasDefaultValueSql("now()");
    }
}
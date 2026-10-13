using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class VoucherProgramConfiguration : IEntityTypeConfiguration<VoucherProgram>
{
    public void Configure(EntityTypeBuilder<VoucherProgram> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.MinOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.MaxOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasMany(x => x.Vouchers)
            .WithOne(x => x.Program)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

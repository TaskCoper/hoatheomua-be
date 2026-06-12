using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(t => t.SePayId)
            .IsRequired();

        builder.Property(t => t.Gateway)
            .HasMaxLength(50);

        builder.Property(t => t.AccountNumber)
            .HasMaxLength(100);

        builder.Property(t => t.SubAccount)
            .HasMaxLength(100);

        builder.Property(t => t.Code)
            .HasMaxLength(100);

        builder.Property(t => t.Content)
            .HasMaxLength(500);

        builder.Property(t => t.TransferType)
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.ReferenceCode)
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Success");

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("now()");
    }
}

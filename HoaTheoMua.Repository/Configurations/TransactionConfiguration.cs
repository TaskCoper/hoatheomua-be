using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Configurations.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // The gateway's own id for the payment event: BaoKim's alphanumeric `request_id`, or the
        // SePay transaction id rendered as text.
        builder.Property(t => t.RequestId)
            .IsRequired()
            .HasMaxLength(100);

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


        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("now()");

        // Database-level idempotency for the payment webhooks. Both gateways redeliver, and the
        // in-code `if (order.Status == "Paid") return;` guard is a read-then-act race that two
        // concurrent redeliveries can both pass. RequestId already carries each gateway's own event
        // id (BaoKim `request_id`, SePay's transaction id as text), so this pair is the natural key:
        // the loser of the race takes a 23505 and the webhook treats it as a replay.
        builder.HasIndex(t => new { t.Gateway, t.RequestId })
            .IsUnique();

        builder.SeedTransaction();
    }
}

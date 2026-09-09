using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

/// <summary>
/// Written as a plain IEntityTypeConfiguration rather than MassTransit's SagaClassMap so that
/// ApplyConfigurationsFromAssembly picks it up like every other mapping in this folder, and so that
/// AppDbContext does not have to derive from SagaDbContext.
/// </summary>
public class OrderFulfillmentStateConfiguration : IEntityTypeConfiguration<OrderFulfillmentState>
{
    public void Configure(EntityTypeBuilder<OrderFulfillmentState> builder)
    {
        // CorrelationId is the Order.Id, supplied by MassTransit - never database-generated.
        builder.HasKey(x => x.CorrelationId);
        builder.Property(x => x.CorrelationId).ValueGeneratedNever();

        builder.Property(x => x.CurrentState)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Gateway).HasMaxLength(50);
        builder.Property(x => x.ShippingPartyOrderId).HasMaxLength(100);
        builder.Property(x => x.FailedStep).HasMaxLength(100);

        // OrderFulfillmentStateMachine truncates to this length; a NhanhVn rejection body would
        // otherwise overflow and fail the saga's own SaveChanges.
        builder.Property(x => x.FailureReason).HasMaxLength(1000);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        // Ops queries: "which orders are stuck / need a manual refund" filters on state, then on
        // how long the row has sat untouched.
        builder.HasIndex(x => x.CurrentState);
        builder.HasIndex(x => x.UpdatedAt);
    }
}

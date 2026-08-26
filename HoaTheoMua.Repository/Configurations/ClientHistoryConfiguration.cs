using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class ClientHistoryConfiguration : IEntityTypeConfiguration<ClientHistory>
{
    public void Configure(EntityTypeBuilder<ClientHistory> builder)
    {
        builder.ToTable("ClientHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Input).IsRequired();
        builder.Property(x => x.BaseId).IsRequired();
        builder.Property(x => x.OutputId).IsRequired();
        builder.Property(x => x.OrderId).IsRequired(false);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.SystemPromptId).IsRequired();
        builder.Property(x => x.Metadata).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.Type, x.OutputId });

        builder.HasOne(x => x.SystemPrompt)
            .WithMany(x => x.ClientHistories)
            .HasForeignKey(x => x.SystemPromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

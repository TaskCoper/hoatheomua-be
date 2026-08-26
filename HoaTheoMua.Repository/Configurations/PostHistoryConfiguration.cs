using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class PostHistoryConfiguration : IEntityTypeConfiguration<PostHistory>
{
    public void Configure(EntityTypeBuilder<PostHistory> builder)
    {
        builder.ToTable("PostHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Input).IsRequired();
        builder.Property(x => x.BaseId).IsRequired();
        builder.Property(x => x.OutputId).IsRequired();
        builder.Property(x => x.SystemPromptId).IsRequired();
        builder.Property(x => x.Metadata).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.OutputId);

        builder.HasOne(x => x.SystemPrompt)
            .WithMany(x => x.PostHistories)
            .HasForeignKey(x => x.SystemPromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

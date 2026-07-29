using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class HistoryConfiguration : IEntityTypeConfiguration<History>
{
    public void Configure(EntityTypeBuilder<History> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Input)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();
        
        builder.Property(x => x.Metadata)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.HasOne(x => x.SystemPrompt)
            .WithMany(x => x.Histories)
            .HasForeignKey(x => x.SystemPromptId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.TemplateCardBase)
            .WithMany(x => x.Histories)
            .HasForeignKey(x => x.TemplateCardBaseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Product)
            .WithMany(x => x.Histories)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.PostCoreBase)
            .WithMany(x => x.Histories)
            .HasForeignKey(x => x.PostCoreBaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

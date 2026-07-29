using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class TemplateCardBaseConfiguration : IEntityTypeConfiguration<TemplateCardBase>
{
    public void Configure(EntityTypeBuilder<TemplateCardBase> builder)
    {

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.LimitString)
            .IsRequired();

        builder.Property(x => x.PictureUrl)
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");
        
    }
}

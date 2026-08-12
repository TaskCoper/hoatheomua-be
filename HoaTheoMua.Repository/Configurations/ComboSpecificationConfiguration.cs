using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class ComboSpecificationConfiguration: IEntityTypeConfiguration<ComboSpecification>
{
    public void Configure(EntityTypeBuilder<ComboSpecification> builder)
    {
        builder.ToTable("ComboSpecification");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.IsCore)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasOne(x => x.Sub)
            .WithMany(x => x.SubIds)
            .HasForeignKey(x => x.SubId)
            .OnDelete(DeleteBehavior.Restrict);
       
        builder.HasOne(x => x.Mat)
            .WithMany(x => x.MatIds)
            .HasForeignKey(x => x.MatId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => new { x.SubId, x.MatId })
            .IsUnique();
        
    }

}
using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class MockupConfiguration : IEntityTypeConfiguration<Mockup>
{
    public void Configure(EntityTypeBuilder<Mockup> builder)
    {
        builder.ToTable("Mockups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).IsRequired();
        builder.Property(x => x.Description).IsRequired();
    }
}

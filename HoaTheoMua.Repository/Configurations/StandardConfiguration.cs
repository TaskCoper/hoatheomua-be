using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class StandardConfiguration : IEntityTypeConfiguration<Standard>
{
    public void Configure(EntityTypeBuilder<Standard> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Description)
            .IsRequired();

       builder.Property(x => x.Type)
           .IsRequired();
    }
}

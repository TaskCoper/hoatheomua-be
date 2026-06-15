using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class ConfigConfiguration
{
    public void Configure(EntityTypeBuilder<Config> builder)
    {
        builder.Property(x => x.Value)
            .IsRequired();
        builder.Property(x => x.Key)
            .IsRequired();
        
        
    }
}
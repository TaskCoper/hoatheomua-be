using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class ConfigDynamicConfiguration
{
    public void Configure(EntityTypeBuilder<ConfigDynamic> builder)
    {
        builder.Property(x => x.Value)
            .IsRequired();
        builder.Property(x => x.Key)
            .IsRequired();
        
        
    }
}
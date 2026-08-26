using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class BasePostConfiguration : IEntityTypeConfiguration<BasePost>
{
    public void Configure(EntityTypeBuilder<BasePost> builder)
    {
        builder.ToTable("BasePosts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.ImageUrl).IsRequired();
    }
}

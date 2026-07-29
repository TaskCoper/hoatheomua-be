using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class PostCoreBaseConfiguration : IEntityTypeConfiguration<PostCoreBase>
{
    public void Configure(EntityTypeBuilder<PostCoreBase> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.PictureUrl)
            .IsRequired();

       
    }
}

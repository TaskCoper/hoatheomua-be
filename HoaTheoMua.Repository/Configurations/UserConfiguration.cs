using HoaTheoMua.Repository.Configurations.Seeds;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(u => u.IsVerify)
            .IsRequired();
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(10);
        builder.Property(u => u.HashPassword)
            .IsRequired()
            .HasMaxLength(256);
        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);
        builder.HasIndex(u => u.Email)
            .IsUnique();
        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.Property(u => u.Role)
            .HasDefaultValue(Role.User);

        builder.HasMany(x => x.RefreshToken)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.ResetPassword)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.AuditLog)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.CategoryNews)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.News)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Orders)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.SeedUser();
    }
}
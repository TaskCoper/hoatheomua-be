using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class PlatformAccountConfiguration : IEntityTypeConfiguration<PlatformAccount>
{
    public void Configure(EntityTypeBuilder<PlatformAccount> b)
    {
        b.ToTable("PlatformAccount", t =>
        {
            t.HasCheckConstraint("CK_PlatformAccount_Status", "\"Status\" IN ('Connected','ReauthRequired','Disabled')");
            t.HasCheckConstraint("CK_PlatformAccount_Credentials", "\"Status\" <> 'Connected' OR \"ProtectedCredentials\" IS NOT NULL");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Platform).HasColumnType("\"PlatformType\"");
        b.Property(x => x.ExternalAccountId).HasMaxLength(128).IsRequired();
        b.Property(x => x.PageId).HasMaxLength(128);
        b.Property(x => x.DisplayName).HasMaxLength(300).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.GrantedScopes).HasColumnType("text[]");
        b.Property(x => x.ProtectedCredentials).HasColumnType("text");
        b.Property(x => x.LastErrorCode).HasMaxLength(80);
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasIndex(x => new { x.Platform, x.ExternalAccountId }).IsUnique();
        b.HasIndex(x => new { x.Status, x.AccessTokenExpiresAt });
    }
}

public class PlatformOAuthSessionConfiguration : IEntityTypeConfiguration<PlatformOAuthSession>
{
    public void Configure(EntityTypeBuilder<PlatformOAuthSession> b)
    {
        b.ToTable("PlatformOAuthSession");
        b.HasKey(x => x.Id);
        b.Property(x => x.Platform).HasColumnType("\"PlatformType\"");
        b.Property(x => x.StateHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.BrowserHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.ProtectedVerifier).HasColumnType("text");
        b.HasIndex(x => x.StateHash).IsUnique();
        b.HasIndex(x => x.ExpiresAt);
        b.HasOne<PlatformAccount>().WithMany().HasForeignKey(x => x.TargetAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

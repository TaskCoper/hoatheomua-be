using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Repository.Data;

public class AppDbContext : DbContext
{
    public  AppDbContext(DbContextOptions options) : base(options)
    {}
    public DbSet<User> Users { get; set; }
    public DbSet<CategoryNews> CategoryNewsList { get; set; }
    public DbSet<News> NewsList { get; set; }
    public DbSet<CategoryNewsDetail> CategoryNewsDetails { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ResetPassword> ResetPasswords { get; set; }
    public DbSet<CompanyAddress> CompanyAddresses { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.Property(u => u.PhoneNumber).HasMaxLength(20);
            builder.Property(u => u.HashPassword).IsRequired();
            builder.Property(u => u.IsDeleted).HasDefaultValue(false);
            builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");

            builder.HasIndex(u => u.Email).IsUnique();

            builder.HasMany(u => u.RefreshToken).WithOne(t => t.User).HasForeignKey(t => t.UserID).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.ResetPassword).WithOne(p => p.User).HasForeignKey(p => p.UserID).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.AuditLog).WithOne(a => a.User).HasForeignKey(a => a.UserID).OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.CategoryNews).WithOne(c => c.User).HasForeignKey(c => c.UserID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CategoryNews>(builder =>
        {
            builder.Property(c => c.Title).IsRequired().HasMaxLength(255);
            builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");

            builder.HasOne(c => c.ParentCategoryNews)
                   .WithMany(c => c.ChildCategoryNews)
                   .HasForeignKey(c => c.ParentCategoryNewsID)
                   .OnDelete(DeleteBehavior.Restrict); 
        });

        modelBuilder.Entity<News>(builder =>
        {
            builder.Property(n => n.Title).IsRequired().HasMaxLength(500);
            builder.Property(n => n.ViewCount).HasDefaultValue(0);
            builder.Property(n => n.Status).HasMaxLength(50).HasDefaultValue("Draft");
            builder.Property(n => n.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CategoryNewsDetail>(builder =>
        {
            builder.HasIndex(cnd => new { cnd.CategoryNewsID, cnd.NewsID }).IsUnique();

            builder.HasOne(cnd => cnd.CategoryNews)
                   .WithMany(c => c.CategoryNewsDetail)
                   .HasForeignKey(cnd => cnd.CategoryNewsID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cnd => cnd.news)
                   .WithMany(n => n.CategoryNewsDetail)
                   .HasForeignKey(cnd => cnd.NewsID)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.Property(rt => rt.RefreshTokenHash).IsRequired();
            builder.Property(rt => rt.IpAddress).HasMaxLength(45);
            builder.Property(rt => rt.DeviceLabel).HasMaxLength(255);
            builder.Property(rt => rt.CreatedAt).HasDefaultValueSql("now()");

            builder.HasIndex(rt => rt.RefreshTokenHash).IsUnique();
        });

        modelBuilder.Entity<ResetPassword>(builder =>
        {
            builder.Property(rp => rp.TokenHash).IsRequired();
            builder.Property(rp => rp.IsUsed).HasDefaultValue(false);
            builder.Property(rp => rp.CreatedAt).HasDefaultValueSql("now()");

            builder.HasIndex(rp => rp.TokenHash).IsUnique();
        });

        modelBuilder.Entity<CompanyAddress>(builder =>
        {
            builder.Property(ca => ca.AddressLine).HasMaxLength(500);
            builder.Property(ca => ca.Ward).HasMaxLength(100);
            builder.Property(ca => ca.District).HasMaxLength(100);
            builder.Property(ca => ca.Province).HasMaxLength(100);
            builder.Property(ca => ca.City).HasMaxLength(100);
            builder.Property(ca => ca.Country).HasMaxLength(100);
            builder.Property(ca => ca.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.Property(al => al.Action).IsRequired().HasMaxLength(255);
            builder.Property(al => al.Entity).HasMaxLength(255);
            builder.Property(al => al.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}
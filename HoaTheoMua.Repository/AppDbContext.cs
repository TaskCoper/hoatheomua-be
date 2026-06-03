using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
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
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(10);
            builder.Property(u => u.HashPassword)
                .IsRequired();
            builder.Property(u => u.IsDeleted)
                .HasDefaultValue(false);
            builder.HasIndex(u => u.Email)
                .IsUnique();
            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("now()");

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
        });

        modelBuilder.Entity<CategoryNews>(builder =>
        {
            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("now()");
            builder.HasOne(c => c.ParentCategoryNews)
                   .WithMany(c => c.ChildCategoryNews)
                   .HasForeignKey(c => c.ParentCategoryNewsId)
                   .OnDelete(DeleteBehavior.Restrict); 
        });

        modelBuilder.Entity<News>(builder =>
        {
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(n => n.Description)
                .IsRequired();
            builder.Property(n => n.ViewCount)
                .HasDefaultValue(0);
            builder.Property(n => n.Status)
                .HasMaxLength(50)
                .HasDefaultValue(NewsStatus.Draft);
            builder.Property(n => n.CreatedAt)
                .HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CategoryNewsDetail>(builder =>
        {
            builder.HasIndex(cnd => new { cnd.CategoryNewsId, cnd.NewsId }).IsUnique();

            builder.HasOne(cnd => cnd.CategoryNews)
                   .WithMany(c => c.CategoryNewsDetail)
                   .HasForeignKey(cnd => cnd.CategoryNewsId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cnd => cnd.News)
                   .WithMany(n => n.CategoryNewsDetail)
                   .HasForeignKey(cnd => cnd.NewsId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.Property(rt => rt.RefreshTokenHash)
                .IsRequired();
            builder.Property(rt => rt.IpAddress)
                .HasMaxLength(45);
            builder.Property(rt => rt.DeviceLabel)
                .HasMaxLength(255);
            builder.Property(rt => rt.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.HasIndex(rt => rt.RefreshTokenHash)
                .IsUnique();
        });

        modelBuilder.Entity<ResetPassword>(builder =>
        {
            builder.Property(rp => rp.Otp)
                .IsRequired();
            builder.Property(rp => rp.IsUsed)
                .HasDefaultValue(false);
            builder.Property(rp => rp.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.HasIndex(rp => rp.Otp)
                .IsUnique();
        });

        modelBuilder.Entity<CompanyAddress>(builder =>
        {
            builder.Property(ca => ca.AddressLine)
                .HasMaxLength(500);
            builder.Property(ca => ca.Ward)
                .HasMaxLength(100);
            builder.Property(ca => ca.District)
                .HasMaxLength(100);
            builder.Property(ca => ca.Province)
                .HasMaxLength(100);
            builder.Property(ca => ca.City)
                .HasMaxLength(100);
            builder.Property(ca => ca.Country)
                .HasMaxLength(100);
            builder.Property(ca => ca.CreatedAt)
                .HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.Property(al => al.Action)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(al => al.Description)
                .IsRequired();
            builder.Property(al => al.EntityType)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(al => al.EntityId)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(al => al.CreatedAt)
                .HasDefaultValueSql("now()");
        });
    }
}
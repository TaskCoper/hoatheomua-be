using HoaTheoMua.Repository.Configurations.Seeds;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Repository;

public class AppDbContext : DbContext
{
    public  AppDbContext(DbContextOptions options) : base(options)
    {}
    public DbSet<User> Users { get; set; }
    public DbSet<CategoryNews> CategoryNewsList { get; set; }
    public DbSet<News> NewsList { get; set; }
    public DbSet<CategoryNewsDetail> CategoryNewsDetails { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ForgotPassword> ResetPasswords { get; set; }
    public DbSet<CompanyAddress> CompanyAddresses { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        modelBuilder.SeedData();
    }
}
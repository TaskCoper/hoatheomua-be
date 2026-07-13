using HoaTheoMua.Repository.Configurations.Seeds;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Repository;

public class AppDbContext : DbContext
{
    public  AppDbContext(DbContextOptions options) : base(options)
    {}
    public DbSet<CategoryNews> CategoryNewsList { get; set; }
    public DbSet<News> NewsList { get; set; }
    public DbSet<CategoryNewsDetail> CategoryNewsDetails { get; set; }
    public DbSet<CompanyAddress> CompanyAddresses { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Config> Configs { get; set; }
    public DbSet<JobDescriptions> JobDescriptions { get; set; }
    public DbSet<RecruitmentRequest> RecruitmentRequests { get; set; }
    public DbSet<Address> Addresses { get; set; }

    public DbSet<OrderDetails> OrderDetails { get; set; }
    public DbSet<Product> Products { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

    }
}
using HoaTheoMua.Repository.Configurations.Seeds;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {}
    public virtual DbSet<CategoryNews> CategoryNewsList { get; set; }
    public virtual DbSet<News> NewsList { get; set; }
    public virtual DbSet<CategoryNewsDetail> CategoryNewsDetails { get; set; }
    public virtual DbSet<CompanyAddress> CompanyAddresses { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<Config> Configs { get; set; }
    public virtual DbSet<JobDescriptions> JobDescriptions { get; set; }
    public virtual DbSet<RecruitmentRequest> RecruitmentRequests { get; set; }
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<SystemPrompt> SystemPrompts { get; set; }
    public virtual DbSet<CardTemplate> CardTemplates { get; set; }
    public virtual DbSet<BasePost> BasePosts { get; set; }
    public virtual DbSet<BasePostImage> BasePostImages { get; set; }
    public virtual DbSet<PlatformAccount> PlatformAccounts { get; set; }
    public virtual DbSet<PlatformOAuthSession> PlatformOAuthSessions { get; set; }
    public virtual DbSet<PostPublication> PostPublications { get; set; }
    public virtual DbSet<PostPublicationAttempt> PostPublicationAttempts { get; set; }
    public virtual DbSet<PostPublicationReview> PostPublicationReviews { get; set; }
    public virtual DbSet<PlatformPost> PlatformPosts { get; set; }
    public virtual DbSet<GeneratedPostImage> GeneratedPostImages { get; set; }
    public virtual DbSet<PlatformPostImage> PlatformPostImages { get; set; }

    public virtual DbSet<GeneratedPost> GeneratedPosts { get; set; }
    public virtual DbSet<GeneratedFlower> GeneratedFlowers { get; set; }
    public virtual DbSet<GeneratedCard> GeneratedCards { get; set; }
    public virtual DbSet<PostHistory> PostHistories { get; set; }
    public virtual DbSet<FlowerReference> FlowerReferences { get; set; }
    public virtual DbSet<ClientHistory> ClientHistories { get; set; }

    public virtual DbSet<OrderDetails> OrderDetails { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    
    public virtual DbSet<OrderNhanhVn>  OrderNhanhVns { get; set; }
    public virtual DbSet<OrderNhanhVnDetail>  OrderNhanhVnDetails { get; set; }

    public virtual DbSet<ComboSpecification> ComboSpecifications { get; set; }
    public virtual DbSet<CategoryProduct> CategoryProducts { get; set; }
    public virtual DbSet<CategoryProductDetail> CategoryProductDetails { get; set; }

    /// <summary>Persisted progress of the order-fulfilment saga, keyed by Order.Id.</summary>
    public virtual DbSet<OrderFulfillmentState> OrderFulfillmentStates { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresEnum<SystemPromptType>(name: "SystemPromptType");
        modelBuilder.HasPostgresEnum<PlatformType>(name: "PlatformType");
        modelBuilder.HasPostgresEnum<CardFormType>(name: "CardFormType");
        modelBuilder.HasPostgresEnum<ClientHistoryType>(name: "ClientHistoryType");
        modelBuilder.HasPostgresEnum<FlowerReferenceType>(name: "FlowerReferenceType");
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

        // MassTransit transactional outbox/inbox. All three are required: the webhook publishes
        // through UseBusOutbox (OutboxMessage + OutboxState) and every consumer endpoint runs
        // UseEntityFrameworkOutbox, which dedups via InboxState.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}

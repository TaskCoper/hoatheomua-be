using FluentValidation;
using HoaTheoMua.Extentions;
using HoaTheoMua.Filters;
using HoaTheoMua.Middleware;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;
using AssemblyReference = HoaTheoMua.Service.AssemblyReference;
using CategoryNewsService = HoaTheoMua.Service.CategoryNew;
using CategoryProductService = HoaTheoMua.Service.CategoryProduct;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress;
using NewService = HoaTheoMua.Service.New;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions;
using RecruitmentRequestService = HoaTheoMua.Service.RecruitmentRequests;
using OrderService = HoaTheoMua.Service.Order;
using WebhookService = HoaTheoMua.Service.Webhook;
using ProductService = HoaTheoMua.Service.Product;
using ConfigService = HoaTheoMua.Service.Config;
using AddressService = HoaTheoMua.Service.Address;
using AhamoveService = HoaTheoMua.Service.AhamoveService;
using NhanhVnService = HoaTheoMua.Service.NhanhVnService;
using ProductImportService = HoaTheoMua.Service.ProductImport;
using BaoKimService = HoaTheoMua.Service.BaoKim;
using OrderNhanhVnImportService = HoaTheoMua.Service.OrderNhanhVnImport;
using RevalidateService = HoaTheoMua.Service.Revalidate;
using MaterialService = HoaTheoMua.Service.Material;
using XanhSmService = HoaTheoMua.Service.XanhSm;
using FulfillmentService = HoaTheoMua.Service.Fulfillment;
using HoaTheoMua.Service.DependencyInjection.Extensions;
using SystemPromptService = HoaTheoMua.Service.SystemPrompt;
using PostCoreService = HoaTheoMua.Service.PostCore;
using PostGenerationService = HoaTheoMua.Service.PostGeneration;
using PlatformAccountService = HoaTheoMua.Service.PlatformAccount;
using AiService = HoaTheoMua.Service.AiService;
using FlowerReferenceService = HoaTheoMua.Service.FlowerReference;
using CardTemplateService = HoaTheoMua.Service.CardTemplate;
using CardService = HoaTheoMua.Service.Card;
using FlowerService = HoaTheoMua.Service.Flower;
using VoucherService = HoaTheoMua.Service.Voucher;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("PostgresSql");
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
dataSourceBuilder.MapEnum<SystemPromptType>("SystemPromptType");
dataSourceBuilder.MapEnum<PlatformType>("PlatformType");
dataSourceBuilder.MapEnum<CardFormType>("CardFormType");
dataSourceBuilder.MapEnum<ClientHistoryType>("ClientHistoryType");
dataSourceBuilder.MapEnum<FlowerReferenceType>("FlowerReferenceType");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource)
);

builder.Services.ConfigureRateLimiter();
builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddHttpContextAccessor();

var trustForwardedHeaders = builder.Configuration.GetValue<bool>("ReverseProxy:TrustForwardedHeaders");
if (trustForwardedHeaders)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;

        // The API container is not published by the profile-based compose setup; only the
        // Cloudflare tunnel can reach it. Do not enable this setting when exposing the API
        // directly unless KnownProxies/KnownNetworks are restricted to the real proxy.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

builder.Services.AddScoped<HoaTheoMua.Service.Common.ICurrentUserService, HoaTheoMua.Service.Common.CurrentUserService>();
builder.Services.AddScoped<CategoryNewsService.IService, CategoryNewsService.Service>();
builder.Services.AddScoped<CategoryProductService.IService, CategoryProductService.Service>();
builder.Services.AddScoped<CompanyAddressService.IService, CompanyAddressService.Service>();
builder.Services.AddScoped<NewService.IService, NewService.Service>();
builder.Services.AddScoped<OrderService.IService, OrderService.Service>();
builder.Services.AddScoped<WebhookService.IService, WebhookService.Service>();
builder.Services.AddScoped<XanhSmService.XanhSmWebhookService>();
builder.Services.AddScoped<XanhSmService.XanhSmDeliveryCompletionService>();
builder.Services.AddHttpClient<NhanhVnService.INhanhVnDeliveryStatusClient, NhanhVnService.NhanhVnDeliveryStatusClient>(
    "NhanhVnDeliveryStatus", client => client.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddScoped<JobDescriptionService.IService, JobDescriptionService.Service>();
builder.Services.AddScoped<RecruitmentRequestService.IService, RecruitmentRequestService.Service>();
builder.Services.AddScoped<VoucherService.IService, VoucherService.Service>();
builder.Services.AddHttpClient<AhamoveService.IService, AhamoveService.Service>("AhamoveService");
builder.Services.AddHttpClient<XanhSmService.IService, XanhSmService.Service>("XanhSmService", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<BaoKimService.IService, BaoKimService.Service>("BaoKimService");
builder.Services.AddScoped<ProductService.IService, ProductService.Service>();
builder.Services.AddScoped<ConfigService.IService, ConfigService.Service>();
builder.Services.AddScoped<AddressService.IService, AddressService.Service>();
builder.Services.AddScoped<NhanhVnService.IService, NhanhVnService.Service>();
builder.Services.AddScoped<ProductImportService.IService, ProductImportService.Service>();
builder.Services.AddScoped<OrderNhanhVnImportService.IService, OrderNhanhVnImportService.Service>();
builder.Services.AddScoped<RevalidateService.IService, RevalidateService.Service>();
builder.Services.AddScoped<MaterialService.IService, MaterialService.Service>();
builder.Services.AddScoped<FlowerReferenceService.IService, FlowerReferenceService.Service>();
builder.Services.AddScoped<PostCoreService.IService, PostCoreService.Service>();
builder.Services.AddScoped<PostGenerationService.IService, PostGenerationService.Service>();
builder.Services.Configure<HoaTheoMua.Service.PostPublishing.PublishingOptions>(builder.Configuration.GetSection("PublishingOptions"));
builder.Services.AddScoped<HoaTheoMua.Service.PostPublishing.Store>();
builder.Services.AddScoped<HoaTheoMua.Service.PostPublishing.Worker>();
builder.Services.AddScoped<HoaTheoMua.Service.PostPublishing.Recovery>();
builder.Services.AddScoped<HoaTheoMua.Service.PostPublishing.IService, HoaTheoMua.Service.PostPublishing.Service>();
builder.Services.AddScoped<HoaTheoMua.Service.PostInsights.IService, HoaTheoMua.Service.PostInsights.Service>();
builder.Services.AddScoped<HoaTheoMua.Service.PostInsights.MetaInsightsClient>();
builder.Services.AddHostedService<HoaTheoMua.Service.PostPublishing.RecoveryHostedService>();
HoaTheoMua.Service.PostPublishing.Registration.AddPostGenPublishingAdapters(builder.Services);
builder.Services.Configure<PlatformAccountService.PlatformAccountOptions>(builder.Configuration.GetSection(nameof(PlatformAccountService.PlatformAccountOptions)));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<PlatformAccountService.TokenProtector>();
builder.Services.AddScoped<PlatformAccountService.IService, PlatformAccountService.Service>();
builder.Services.AddHttpClient<PlatformAccountService.IOAuthProvider, PlatformAccountService.OAuthProvider>("PostGenOAuth", client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
    client.MaxResponseContentBufferSize = 1024 * 1024;
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false })
  .RemoveAllLoggers(); // OAuth endpoints can carry secrets in query parameters; do not log request URLs.

builder.Services.AddHttpClient<AiService.IService, AiService.Service>("PostGenAi", client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan; // Per-call timeout and total generation budget use cancellation tokens.
    client.MaxResponseContentBufferSize = 1024 * 1024;
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

builder.Services.AddScoped<SystemPromptService.IService, SystemPromptService.Service>();
builder.Services.AddScoped<CardTemplateService.IService, CardTemplateService.Service>();
builder.Services.AddScoped<CardService.IService, CardService.Service>();
builder.Services.Configure<FlowerService.FlowerImageOptions>(options =>
{
    builder.Configuration.GetSection("FlowerImage").Bind(options);
    options.StorageApiBase = builder.Configuration["TASKCOPER_API_BASE"] ?? options.StorageApiBase;
    options.StorageEmail = builder.Configuration["TASKCOPER_EMAIL"] ?? options.StorageEmail;
    options.StoragePassword = builder.Configuration["TASKCOPER_PASSWORD"] ?? options.StoragePassword;
    options.ObjectPrefix = (builder.Configuration["TASKCOPER_STORAGE_PREFIX"] ?? options.ObjectPrefix).Trim('/');
});
builder.Services.AddSingleton<FlowerService.ITaskcoperStorageSession, FlowerService.TaskcoperStorageSession>();
builder.Services.AddScoped<FlowerService.IFlowerImageBranding, FlowerService.FlowerImageBranding>();
builder.Services.Configure<HoaTheoMua.Service.Card.CardImageOptions>(options =>
{
    builder.Configuration.GetSection("FlowerImage").Bind(options);
    options.ObjectPrefix = "general/ai-cards";
    options.LogoWidthRatio = 0.16;
    builder.Configuration.GetSection("CardImage").Bind(options);
});
builder.Services.AddScoped<HoaTheoMua.Service.Card.ICardImageBranding, HoaTheoMua.Service.Card.CardImageBranding>();
builder.Services.AddHttpClient(FlowerService.FlowerImageBranding.SourceClient, client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
}).ConfigurePrimaryHttpMessageHandler(FlowerService.PublicImageHttpHandler.Create).RemoveAllLoggers();
builder.Services.AddHttpClient(FlowerService.FlowerImageBranding.StorageClient, client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.MaxResponseContentBufferSize = 64 * 1024;
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
  .RemoveAllLoggers(); // Presigned URLs carry signatures; never log them.
builder.Services.AddScoped<FlowerService.Service>();
builder.Services.AddScoped<FlowerService.IService>(sp => sp.GetRequiredService<FlowerService.Service>());
builder.Services.AddOptions<HoaTheoMua.Service.FlowerJobs.FlowerJobOptions>()
    .Bind(builder.Configuration.GetSection("FlowerJobs"))
    .Validate(o => o.MaxActivePerUser is >= 1 and <= 10 && o.Concurrency is >= 1 and <= 16
        && o.BudgetSeconds is >= 30 and <= 1800 && o.LeaseSeconds is >= 30 and <= 300
        && o.RecoveryIntervalSeconds is >= 5 and <= 60, "Invalid FlowerJobs options").ValidateOnStart();
builder.Services.AddScoped<HoaTheoMua.Service.FlowerJobs.Service>();
builder.Services.AddScoped<HoaTheoMua.Service.FlowerJobs.Store>();
builder.Services.AddScoped<HoaTheoMua.Service.FlowerJobs.Worker>();
builder.Services.AddScoped<HoaTheoMua.Service.FlowerJobs.Recovery>();
builder.Services.AddHostedService<HoaTheoMua.Service.FlowerJobs.RecoveryHostedService>();
builder.Services.AddOptions<HoaTheoMua.Service.CardJobs.CardJobOptions>()
    .Bind(builder.Configuration.GetSection("CardJobs"))
    .Validate(o => o.MaxActivePerUser is >= 1 and <= 10 && o.Concurrency is >= 1 and <= 16
        && o.BudgetSeconds is >= 30 and <= 1800 && o.LeaseSeconds is >= 30 and <= 300
        && o.RecoveryIntervalSeconds is >= 5 and <= 60, "Invalid CardJobs options").ValidateOnStart();
builder.Services.AddScoped<HoaTheoMua.Service.Card.Service>();
builder.Services.AddScoped<HoaTheoMua.Service.CardJobs.Service>();
builder.Services.AddScoped<HoaTheoMua.Service.CardJobs.Store>();
builder.Services.AddScoped<HoaTheoMua.Service.CardJobs.Worker>();
builder.Services.AddScoped<HoaTheoMua.Service.CardJobs.Recovery>();
builder.Services.AddHostedService<HoaTheoMua.Service.CardJobs.RecoveryHostedService>();
// Typed HttpClient, not AddScoped: this service makes outbound calls to Ahamove/XanhSm and NhanhVn, and
// registering it with AddScoped would silently hand it the unnamed default HttpClient the way
// Webhook.Service and Revalidate.Service currently get theirs.
builder.Services.AddHttpClient<FulfillmentService.IService, FulfillmentService.Service>("FulfillmentService");


// MassTransit + RabbitMQ: consumers, the fulfilment saga and the EF transactional outbox. Runs
// in-process alongside the API, so the outbox shares this host's AppDbContext scope.
builder.Services.AddMasstransitRabbitMqInfrastructure(builder.Configuration);

// AddMassTransit registers a bus health check of its own, so /health reflects the RabbitMQ
// connection as well as the app being up.
builder.Services.AddHealthChecks();

builder.Services.AddValidatorsFromAssembly(AssemblyReference.Assembly);
builder.Services.AddAutoMapper(x => x.AddMaps(AssemblyReference.Assembly));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://hoatheomua-dev-client.bentran1vn.com",
                "https://www.hoatheomua.com",
                "https://hoatheomua-dev-fe-client.bentran1vn.com",
                "https://taskcoper.hoatheomua.com",
                "https://hoatheomua.com"
                )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}


if (trustForwardedHeaders)
{
    app.UseForwardedHeaders();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    app.UseSwaggerAPI();
}
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

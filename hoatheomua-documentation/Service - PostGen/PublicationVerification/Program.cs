using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Controllers;
using HoaTheoMua.Extentions;
using HoaTheoMua.Middleware;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.DependencyInjection.Extensions;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Publishing = HoaTheoMua.Service.PostPublishing;
using RepositoryAccount = HoaTheoMua.Repository.Entity.PlatformAccount;

if (args.Contains("--adapters")) { await AdapterProbe.Run(); return; }
if (args.Contains("--scheduling")) { await AdapterProbe.Run(scheduling: true); return; }

var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set local POSTGEN_TEST_CONNECTION_STRING"));
if (connection.Host is not ("localhost" or "127.0.0.1")) throw new InvalidOperationException("Local test database only");
var database = "publication_probe_" + Guid.NewGuid().ToString("N");
await using var admin = new NpgsqlConnection(connection.ConnectionString); await admin.OpenAsync();
await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
var evidence = new List<object>(); var checks = 0; var requests = 0; var cleanup = false;
try
{
    connection.Database = database;
    var ds = new NpgsqlDataSourceBuilder(connection.ConnectionString); ds.EnableDynamicJson();
    ds.MapEnum<PlatformType>("PlatformType"); ds.MapEnum<SystemPromptType>("SystemPromptType");
    ds.MapEnum<CardFormType>("CardFormType"); ds.MapEnum<ClientHistoryType>("ClientHistoryType");
    await using var source = ds.Build();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
    builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:55443");
    const string jwtKey = "postgen-publication-local-test-jwt-only-never-use-for-production-64";
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        { ["JwtOption:Issuer"] = "probe", ["JwtOption:Audience"] = "probe", ["JwtOption:AccessTokenKey"] = jwtKey });
    builder.Services.AddDbContext<AppDbContext>(b => b.UseNpgsql(source));
    builder.Services.AddControllers().AddApplicationPart(typeof(PostPublishingController).Assembly);
    builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
    builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddJwtServices(builder.Configuration); builder.Services.ConfigureRateLimiter();
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.Configure<PlatformAccountOptions>(o =>
    {
        o.ActiveEncryptionKeyId = "probe"; o.EncryptionKeys["probe"] = Convert.ToBase64String(new byte[32]); o.Meta.GraphApiVersion = "v25.0";
    });
    builder.Services.AddSingleton<TokenProtector>();
    builder.Services.Configure<PublishingOptions>(o => { o.FacebookEnabled = true; o.RecoveryIntervalSeconds = 1; });
    var adapter = new FakeAdapter(); builder.Services.AddSingleton<IPublishingAdapter>(adapter);
    builder.Services.AddScoped<Store>(); builder.Services.AddScoped<Worker>(); builder.Services.AddScoped<Recovery>();
    builder.Services.AddScoped<Publishing.IService, Publishing.Service>(); builder.Services.AddHostedService<RecoveryHostedService>();
    builder.Services.AddScoped<HoaTheoMua.Service.PostCore.IService, HoaTheoMua.Service.PostCore.Service>();
    builder.Services.AddScoped<HoaTheoMua.Service.PostGeneration.IService, HoaTheoMua.Service.PostGeneration.Service>();
    var ai = new NoAi(); builder.Services.AddSingleton<HoaTheoMua.Service.AiService.IService>(ai);
    // Exercise production registration, including the exception to global consumer retry/inbox.
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["RabbitMqOptions:Host"] = "127.0.0.1", ["RabbitMqOptions:Port"] = "55672",
        ["RabbitMqOptions:VirtualHost"] = "/", ["RabbitMqOptions:Username"] = "guest", ["RabbitMqOptions:Password"] = "guest"
    });
    builder.Services.AddMasstransitRabbitMqInfrastructure(builder.Configuration);
    // Shared registration also includes fulfilment consumers; this probe must never execute them.
    builder.Services.AddScoped<HoaTheoMua.Service.Fulfillment.IService>(_ => throw new InvalidOperationException("Unexpected fulfilment message in publication probe"));
    builder.Services.AddScoped<HoaTheoMua.Service.XanhSm.XanhSmDeliveryCompletionService>(_ => throw new InvalidOperationException("Unexpected delivery message in publication probe"));
    await using var app = builder.Build();
    Guid postId; Guid accountId;
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); await db.Database.MigrateAsync();
        await db.Database.OpenConnectionAsync(); await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
        (postId, accountId) = await Seed(db, scope.ServiceProvider.GetRequiredService<TokenProtector>());
    }
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
    await app.StartAsync();
    using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:55443") };
    var actor = Guid.NewGuid();
    string Jwt(string role) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("probe", "probe",
        [new Claim("UserId", actor.ToString()), new Claim(ClaimTypes.Role, role), new Claim("IsVerified", "true")],
        expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256)));
    void Check(bool condition, string label) { if (!condition) throw new Exception(label); checks++; }
    async Task<JsonElement> Send(string method, string path, int expected, object? body = null)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (body != null) request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request); requests++;
        var raw = await response.Content.ReadAsStringAsync();
        Check((int)response.StatusCode == expected, $"{method} {path}: expected {expected}, got {(int)response.StatusCode}: {raw}");
        Check(!raw.Contains("probe-token") && !raw.Contains(jwtKey), "No credentials in HTTP response");
        evidence.Add(new { method, path, status = (int)response.StatusCode });
        return string.IsNullOrWhiteSpace(raw) ? default : JsonDocument.Parse(raw).RootElement.Clone();
    }
    var root = $"/api/v1/posts/{postId}/platforms/facebook";
    var previewPath = root + $"/publication-preview?accountId={accountId}";
    var randomJob = "/api/v1/post-gen/publications/" + Guid.NewGuid();
    var endpoints = new[] { ("GET", previewPath), ("POST", root + "/publish"), ("GET", root + "/publications"), ("GET", randomJob),
        ("POST", randomJob + "/cancel"), ("POST", randomJob + "/retry"), ("POST", randomJob + "/recheck"), ("POST", randomJob + "/resolve") };
    foreach (var role in new[] { "anonymous", "Staff" })
    {
        client.DefaultRequestHeaders.Authorization = role == "anonymous" ? null : new AuthenticationHeaderValue("Bearer", Jwt(role));
        foreach (var (method, path) in endpoints) await Send(method, path, role == "anonymous" ? 401 : 403, method == "POST" ? new { } : null);
    }
    client.DefaultRequestHeaders.Authorization = new("Bearer", Jwt("Admin"));
    var preview = (await Send("GET", previewPath, 200)).GetProperty("value");
    Check(preview.GetProperty("canPublish").GetBoolean(), "Preview ready with fake adapter"); Check(adapter.Calls == 0, "Preview did not call provider");
    var payload = new { accountId, idempotencyKey = "http-publication-one", expectedRevision = preview.GetProperty("revision").GetString() };
    var accepted = (await Send("POST", root + "/publish", 202, payload)).GetProperty("value");
    var publicationId = accepted.GetProperty("publicationId").GetGuid(); var pathJob = "/api/v1/post-gen/publications/" + publicationId;
    await adapter.Entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
    var duplicate = (await Send("POST", root + "/publish", 202, payload)).GetProperty("value");
    Check(duplicate.GetProperty("publicationId").GetGuid() == publicationId, "Repeated request has same job");
    await Send("POST", root + "/publish", 409, new { accountId, payload.idempotencyKey, expectedRevision = new string('A', 64) });
    await Send("PUT", $"/api/v1/posts/{postId}", 409, new { title = "Edited", content = "Edited", images = Array.Empty<object>() });
    await Send("DELETE", $"/api/v1/posts/{postId}", 409);
    await Send("PUT", root + "/content", 409, new { title = "Edited", content = "Edited" });
    await Send("PUT", root + "/images", 409, new { images = Array.Empty<object>() });
    await Send("GET", root + "/content/versions", 200);
    var running = (await Send("GET", pathJob, 200)).GetProperty("value").GetProperty("job");
    await Send("POST", pathJob + "/cancel", 409, new { expectedRevision = running.GetProperty("revision").GetInt64() });
    adapter.Release.SetResult();
    JsonElement current = default;
    for (var i = 0; i < 30; i++)
    {
        current = (await Send("GET", pathJob, 200)).GetProperty("value").GetProperty("job");
        if (current.GetProperty("status").GetString() == "NeedsReview") break;
        await Task.Delay(300);
    }
    Check(current.GetProperty("status").GetString() == "NeedsReview", "Timeout becomes NeedsReview through RabbitMQ");
    Check(adapter.Publishes == 1, "One publish despite timeout");
    await Send("POST", pathJob + "/retry", 409, new { expectedRevision = current.GetProperty("revision").GetInt64() });
    // Stop automatic polls while testing explicit optimistic-concurrency review requests.
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"PostPublication\" SET \"NextRunAt\" = NULL WHERE \"Id\" = {publicationId}");
    }
    current = (await Send("POST", pathJob + "/recheck", 200, new { expectedRevision = current.GetProperty("revision").GetInt64() })).GetProperty("value");
    Check(current.GetProperty("status").GetString() == "NeedsReview", "Unknown provider response stays unresolved");
    adapter.Verified = true;
    var resolved = (await Send("POST", pathJob + "/resolve", 200, new { expectedRevision = current.GetProperty("revision").GetInt64(),
        action = "ConfirmPublished", note = "Checked the provider ID", externalPostId = "fake-page_post" })).GetProperty("value");
    Check(resolved.GetProperty("status").GetString() == "Succeeded", "Provider evidence resolves success");
    Check(adapter.Publishes == 1 && ai.Calls == 0, "Review did not republish or call AI");
    var history = (await Send("GET", root + "/publications?pageIndex=1&pageSize=1", 200)).GetProperty("value");
    Check(history.GetProperty("totalCount").GetInt32() == 1, "History contains exactly one publication");
    await Send("POST", root + "/publish", 409, new { accountId, idempotencyKey = "another-key", expectedRevision = (await Send("GET", previewPath, 200)).GetProperty("value").GetProperty("revision").GetString() });
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var job = await db.PostPublications.SingleAsync();
        Check(job.Status == "Succeeded" && job.ExternalPostId == "fake-page_post", "DB final state and external ID");
        Check(Store.Read<PublicationSnapshot>(job.Snapshot).Content == "Manual snapshot", "Snapshot retained exact text");
        Check(await db.PostPublicationReviews.AllAsync(x => x.ActorId == actor), "Review actor recorded");
        Check(await db.PostPublicationAttempts.CountAsync(x => x.Operation == "Publish") == 1, "Only one durable publish attempt");
    }
    await app.StopAsync();
    evidence.Add(new { provider = "fake", adapter.Publishes, aiCalls = ai.Calls, queueTransport = "RabbitMQ", database = "PostgreSQL" });
}
finally
{
    await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync(); cleanup = true;
}
var destination = Path.Combine(AppContext.BaseDirectory, "publication-api-evidence.json");
await File.WriteAllTextAsync(destination, JsonSerializer.Serialize(new { status = "PASS", checks, requests, cleanup, evidence }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"PASS: {requests} HTTP requests, {checks} checks; database cleanup={cleanup}; evidence={destination}");

static async Task<(Guid, Guid)> Seed(AppDbContext db, TokenProtector protector)
{
    var post = new BasePost { Id = Guid.NewGuid(), Title = "Original", Content = "Original" }; db.BasePosts.Add(post);
    var target = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.Id, Platform = PlatformType.Facebook, Status = PlatformPostStatus.Ready };
    db.PlatformPosts.Add(target); await db.SaveChangesAsync();
    var content = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = target.Id, Source = ContentSource.Manual, Title = "Snapshot", Content = "Manual snapshot" };
    db.GeneratedPosts.Add(content); await db.SaveChangesAsync(); target.OfficialGenerationId = content.Id;
    var original = new BasePostImage { Id = Guid.NewGuid(), BasePostId = post.Id, ImageUrl = "https://example.com/original.jpg", SortOrder = 1 }; db.BasePostImages.Add(original);
    var image = new GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = post.Id, SourceBasePostImageId = original.Id, SourceImageUrl = original.ImageUrl,
        ImageUrl = "https://example.com/generated.jpg", Status = ImageStatus.Succeeded, RatioCode = AspectRatioType.Square1x1 }; db.GeneratedPostImages.Add(image);
    db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = target.Id, SourceBasePostImageId = original.Id,
        SelectedGeneratedPostImageId = image.Id, RatioCode = image.RatioCode, SortOrder = 1 });
    var account = new RepositoryAccount { Id = Guid.NewGuid(), Platform = PlatformType.Facebook, ExternalAccountId = "fake-page", PageId = "fake-page",
        DisplayName = "Fake Page", GrantedScopes = ["pages_manage_posts", "pages_read_engagement"], Revision = 1 };
    account.ProtectedCredentials = protector.Protect(Store.Json(new Credentials { AccessToken = "probe-token" }), $"postgen:account:{account.Id}");
    db.PlatformAccounts.Add(account); await db.SaveChangesAsync(); return (post.Id, account.Id);
}

sealed class FakeAdapter : IPublishingAdapter
{
    public PlatformType Platform => PlatformType.Facebook;
    public TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int Calls; public int Publishes; public volatile bool Verified;
    public IReadOnlyList<string> Validate(PublicationSnapshot snapshot) => [];
    public Task<IReadOnlyList<string>> Preflight(PublicationSnapshot snapshot, Credentials credentials, CancellationToken ct) => Task.FromResult<IReadOnlyList<string>>([]);
    public async Task<PublicationResult> Execute(PublicationOperation operation, PublicationSnapshot snapshot, PublicationCheckpoint checkpoint,
        Credentials credentials, string? candidate, CancellationToken ct)
    {
        Interlocked.Increment(ref Calls);
        if (operation == PublicationOperation.Prepare)
        {
            Entered.TrySetResult(); await Release.Task.WaitAsync(ct);
            return new(PublicationOutcome.Continue) { Checkpoint = new() { ReadyToPublish = true }, RetryAfterSeconds = 1 };
        }
        if (operation == PublicationOperation.Publish) { Interlocked.Increment(ref Publishes); throw new TimeoutException("Simulated lost publish response"); }
        return Verified && candidate == "fake-page_post" ? new(PublicationOutcome.Succeeded) { ExternalPostId = candidate, ExternalPostUrl = "https://facebook.com/fake-page_post" }
            : new(PublicationOutcome.Unknown) { RetryAfterSeconds = 3600 };
    }
}
sealed class NoAi : HoaTheoMua.Service.AiService.IService
{
    public int Calls;
    public Task<HoaTheoMua.Service.AiService.Response.GeneratedContent> GenerateContent(HoaTheoMua.Service.AiService.Request.ContentInput input, CancellationToken cancellationToken = default)
    { Calls++; throw new InvalidOperationException("Publishing must not call AI"); }
    public Task<HoaTheoMua.Service.AiService.Response.GeneratedImage> GenerateImage(HoaTheoMua.Service.AiService.Request.ImageInput input, CancellationToken cancellationToken = default)
    { Calls++; throw new InvalidOperationException("Publishing must not call AI"); }
}

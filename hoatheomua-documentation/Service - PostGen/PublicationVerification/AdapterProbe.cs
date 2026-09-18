using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Publishing = HoaTheoMua.Service.PostPublishing;

static class AdapterProbe
{
    public static async Task Run(bool scheduling = false)
    {
        var started = DateTimeOffset.UtcNow; var checks = new List<object>(); var requests = new List<object>();
        var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING") ?? throw new Exception("Set local test connection"));
        if (connection.Host is not ("localhost" or "127.0.0.1")) throw new Exception("Local test database only");
        var database = "adapter_probe_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        void Check(bool condition, string name, object? evidence = null)
        { if (!condition) throw new Exception(name); checks.Add(new { name, status = "PASS", evidence }); }
        try
        {
            connection.Database = database; var ds = new NpgsqlDataSourceBuilder(connection.ConnectionString); ds.EnableDynamicJson();
            ds.MapEnum<PlatformType>("PlatformType"); ds.MapEnum<SystemPromptType>("SystemPromptType"); ds.MapEnum<CardFormType>("CardFormType"); ds.MapEnum<ClientHistoryType>("ClientHistoryType");
            await using var source = ds.Build();
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:55444");
            const string jwtKey = "postgen-adapter-local-test-jwt-not-for-production-64-characters-only";
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtOption:Issuer"] = "adapter-probe", ["JwtOption:Audience"] = "adapter-probe", ["JwtOption:AccessTokenKey"] = jwtKey,
                ["RabbitMqOptions:Host"] = "127.0.0.1", ["RabbitMqOptions:Port"] = "55672", ["RabbitMqOptions:VirtualHost"] = "/",
                ["RabbitMqOptions:Username"] = "guest", ["RabbitMqOptions:Password"] = "guest"
            });
            builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(source));
            builder.Services.AddControllers().AddApplicationPart(typeof(PostPublishingController).Assembly);
            builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>(); builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); builder.Services.AddJwtServices(builder.Configuration); builder.Services.ConfigureRateLimiter();
            var scheduleClock = new ScheduleProbe.Clock();
            builder.Services.AddSingleton<TimeProvider>(scheduling ? scheduleClock : TimeProvider.System);
            builder.Services.Configure<PlatformAccountOptions>(o =>
            {
                o.ActiveEncryptionKeyId = "probe"; o.EncryptionKeys["probe"] = Convert.ToBase64String(new byte[32]);
                o.Meta.Enabled = true; o.Meta.AppId = "test-app"; o.Meta.AppSecret = "test-app-secret"; o.Meta.GraphApiVersion = "v25.0";
                o.Meta.FacebookCallbackUrl = "http://localhost/api/v1/platform-accounts/facebook/callback";
                o.Meta.InstagramCallbackUrl = "http://localhost/api/v1/platform-accounts/instagram/callback";
            });
            builder.Services.AddSingleton<TokenProtector>();
            builder.Services.Configure<PublishingOptions>(o => { o.FacebookEnabled = o.InstagramEnabled = true; o.RecoveryIntervalSeconds = 1; });
            builder.Services.AddPostGenPublishingAdapters(); // Same registration as production. Replace HTTP transport only.
            var provider = new Provider();
            builder.Services.AddHttpClient<MetaPublishingClient>("PostGenPublishing").ConfigurePrimaryHttpMessageHandler(() => provider);
            builder.Services.AddHttpClient<IOAuthProvider, OAuthProvider>("ProbeOAuth", c => c.Timeout = TimeSpan.FromSeconds(15))
                .ConfigurePrimaryHttpMessageHandler(() => provider).RemoveAllLoggers();
            var media = new ImageProvider();
            builder.Services.AddHttpClient<PublishingMediaClient>("PostGenPublishingMedia").ConfigurePrimaryHttpMessageHandler(() => media);
            builder.Services.AddScoped<Store>(); builder.Services.AddScoped<Worker>(); builder.Services.AddScoped<Recovery>();
            builder.Services.AddScoped<Publishing.IService, Publishing.Service>(); builder.Services.AddHostedService<RecoveryHostedService>();
            builder.Services.AddMasstransitRabbitMqInfrastructure(builder.Configuration);
            builder.Services.AddScoped<HoaTheoMua.Service.Fulfillment.IService>(_ => throw new Exception("Unexpected fulfilment"));
            builder.Services.AddScoped<HoaTheoMua.Service.XanhSm.XanhSmDeliveryCompletionService>(_ => throw new Exception("Unexpected delivery"));
            await using var app = builder.Build();
            await using (var scope = app.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); await db.Database.MigrateAsync();
                await db.Database.OpenConnectionAsync(); await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
                Check(scope.ServiceProvider.GetServices<IPublishingAdapter>().Select(x => x.GetType().Name).Order().SequenceEqual(new[] { "FacebookPublisher", "InstagramPublisher" }), "Real adapters registered");
            }
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
            await app.StartAsync();
            using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:55444") };
            string Jwt(string role) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("adapter-probe", "adapter-probe",
                [new Claim("UserId", Guid.NewGuid().ToString()), new Claim(ClaimTypes.Role, role), new Claim("IsVerified", "true")],
                expires: DateTime.UtcNow.AddMinutes(15), signingCredentials: new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256)));
            async Task<JsonElement> Send(string method, string path, int expected, object? body = null)
            {
                using var req = new HttpRequestMessage(new HttpMethod(method), path); if (body != null) req.Content = JsonContent.Create(body);
                var watch = Stopwatch.StartNew(); using var response = await client.SendAsync(req); var raw = await response.Content.ReadAsStringAsync();
                Check((int)response.StatusCode == expected, $"{method} {path}: {expected}", new { actual = (int)response.StatusCode });
                Check(!raw.Contains("probe-token") && !raw.Contains(jwtKey) && !raw.Contains("test-app-secret"), "HTTP response redacted");
                var json = string.IsNullOrWhiteSpace(raw) ? default : JsonDocument.Parse(raw).RootElement.Clone();
                requests.Add(new { method, path, status = (int)response.StatusCode, latency_ms = watch.ElapsedMilliseconds,
                    traceId = json.ValueKind == JsonValueKind.Object && json.TryGetProperty("traceId", out var trace) ? trace.GetString() : null });
                return json;
            }
            var random = "/api/v1/post-gen/publications/" + Guid.NewGuid();
            await Send("GET", random, 401); client.DefaultRequestHeaders.Authorization = new("Bearer", Jwt("Staff")); await Send("GET", random, 403);
            client.DefaultRequestHeaders.Authorization = new("Bearer", Jwt("Admin")); await Send("GET", random, 404);
            if (scheduling) await ScheduleProbe.Exercise(app, client, Send, Check, Jwt, provider, scheduleClock);
            else foreach (var platform in new[] { PlatformType.Facebook, PlatformType.Instagram })
            foreach (var scenario in new[] { "single", "carousel", "timeout", "bad-media" })
            {
                provider.Scenario = scenario; provider.Publishes = 0; provider.Creates = 0; provider.Caption = "Snapshot\n\nManual snapshot";
                media.Invalid = scenario == "bad-media";
                Guid post; Guid account;
                await using (var scope = app.Services.CreateAsyncScope())
                    (post, account) = await Seed(scope.ServiceProvider.GetRequiredService<AppDbContext>(), scope.ServiceProvider.GetRequiredService<TokenProtector>(), platform, scenario == "single" ? 1 : 2);
                var root = $"/api/v1/posts/{post}/platforms/{platform.ToString().ToLowerInvariant()}";
                var preview = (await Send("GET", root + $"/publication-preview?accountId={account}", 200)).GetProperty("value");
                Check(preview.GetProperty("canPublish").GetBoolean(), "Snapshot passes local preview");
                Check(provider.Creates == 0, "Preview creates no remote media");
                var payload = new { accountId = account, idempotencyKey = "adapter-probe", expectedRevision = preview.GetProperty("revision").GetString() };
                var job = (await Send("POST", root + "/publish", 202, payload)).GetProperty("value"); var id = job.GetProperty("publicationId").GetGuid();
                var duplicate = (await Send("POST", root + "/publish", 202, payload)).GetProperty("value");
                Check(duplicate.GetProperty("publicationId").GetGuid() == id, "HTTP duplicate has same publication");
                await Send("POST", root + "/publish", 409, new { payload.accountId, payload.idempotencyKey, expectedRevision = new string('A', 64) });
                var desired = scenario == "timeout" ? "NeedsReview" : scenario == "bad-media" ? "Failed" : "Succeeded";
                var deadline = DateTimeOffset.UtcNow.AddSeconds(40); PostPublication? persisted = null;
                while (DateTimeOffset.UtcNow < deadline)
                {
                    await using var scope = app.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    persisted = await db.PostPublications.AsNoTracking().SingleAsync(x => x.Id == id);
                    if (persisted.Status is "Succeeded" or "NeedsReview" or "Failed") break;
                    await Task.Delay(150);
                }
                Check(persisted?.Status == desired, $"{platform}/{scenario} reaches {desired}", new { persisted?.Status, persisted?.ErrorCode });
                Check(provider.Publishes == (scenario == "bad-media" ? 0 : 1), "At most one publish command", new { platform, scenario, provider.Publishes });
                var detail = (await Send("GET", "/api/v1/post-gen/publications/" + id, 200)).GetProperty("value");
                Check(detail.GetProperty("snapshot").GetProperty("text").GetString() == provider.Caption, "Persisted snapshot matches outbound text");
                await using (var scope = app.Services.CreateAsyncScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    Check(await db.PostPublicationAttempts.CountAsync(x => x.PublicationId == id && x.Operation == "Publish") == (scenario == "bad-media" ? 0 : 1), "Durable publish attempt count");
                    var row = await db.PostPublications.SingleAsync(x => x.Id == id); row.NextRunAt = null; await db.SaveChangesAsync();
                }
                if (scenario == "timeout")
                {
                    var revision = detail.GetProperty("job").GetProperty("revision").GetInt64();
                    await Send("POST", $"/api/v1/post-gen/publications/{id}/retry", 409, new { expectedRevision = revision });
                    var reviewed = (await Send("POST", $"/api/v1/post-gen/publications/{id}/recheck", 200, new { expectedRevision = revision })).GetProperty("value");
                    Check(reviewed.GetProperty("status").GetString() == "NeedsReview" && provider.Publishes == 1, "Recheck is read-only after timeout");
                    await Send("POST", $"/api/v1/post-gen/publications/{id}/resolve", 200, new { expectedRevision = reviewed.GetProperty("revision").GetInt64(), action = "Close", note = "End local test with uncertain provider result" });
                }
                Console.WriteLine($"PASS {platform}/{scenario}");
            }
            await app.StopAsync();
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
        var destination = Path.Combine(AppContext.BaseDirectory, scheduling ? "schedule-api-evidence.json" : "api-integration-tester.json");
        await File.WriteAllTextAsync(destination, JsonSerializer.Serialize(new { skill = "api-integration-tester", status = "PASS", started_at = started,
            checks, requests, failures = Array.Empty<string>(), redactions = new[] { "JWT", "OAuth credentials", "raw provider errors" },
            cleanup = true, liveMeta = false, aiCalls = 0, adapterImplementation = "production", transport = "fake HttpMessageHandler", database = "PostgreSQL", queue = "RabbitMQ" }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"PASS: {requests.Count} HTTP requests, {checks.Count} checks; evidence={destination}");
    }
    internal static async Task<(Guid, Guid)> Seed(AppDbContext db, TokenProtector protector, PlatformType platform, int count)
    {
        var post = new BasePost { Id = Guid.NewGuid(), Title = "Original", Content = "Original" }; db.BasePosts.Add(post);
        var target = new PlatformPost { Id = Guid.NewGuid(), BasePostId = post.Id, Platform = platform, Status = PlatformPostStatus.Ready }; db.PlatformPosts.Add(target); await db.SaveChangesAsync();
        var content = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = target.Id, Source = ContentSource.Manual, Title = "Snapshot", Content = "Manual snapshot" }; db.GeneratedPosts.Add(content); await db.SaveChangesAsync(); target.OfficialGenerationId = content.Id;
        for (var i = 1; i <= count; i++)
        {
            var original = new BasePostImage { Id = Guid.NewGuid(), BasePostId = post.Id, ImageUrl = $"https://example.com/{i}.jpg", SortOrder = i }; db.BasePostImages.Add(original);
            var image = new GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = post.Id, SourceBasePostImageId = original.Id, SourceImageUrl = original.ImageUrl, ImageUrl = original.ImageUrl, Status = ImageStatus.Succeeded, RatioCode = AspectRatioType.Square1x1 }; db.GeneratedPostImages.Add(image);
            db.PlatformPostImages.Add(new() { Id = Guid.NewGuid(), PlatformPostId = target.Id, SourceBasePostImageId = original.Id, SelectedGeneratedPostImageId = image.Id, RatioCode = image.RatioCode, SortOrder = i });
        }
        var account = await db.PlatformAccounts.SingleOrDefaultAsync(x => x.Platform == platform);
        if (account == null)
        {
            account = new() { Id = Guid.NewGuid(), Platform = platform, ExternalAccountId = platform == PlatformType.Facebook ? "123" : "456", PageId = "123", DisplayName = "Probe",
                GrantedScopes = ["pages_read_engagement", "pages_manage_posts", "instagram_basic", "instagram_content_publish"], Revision = 1 };
            account.ProtectedCredentials = protector.Protect(Store.Json(new Credentials { AccessToken = "probe-token" }), $"postgen:account:{account.Id}"); db.PlatformAccounts.Add(account);
        }
        await db.SaveChangesAsync(); return (post.Id, account.Id);
    }
    internal sealed class Provider : HttpMessageHandler
    {
        public string Scenario = ""; public string Caption = ""; public int Publishes; public int Creates;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (request.RequestUri!.Host != "graph.facebook.com") throw new Exception("Unexpected provider host");
            var path = request.RequestUri.AbsolutePath;
            var raw = request.Content == null ? request.RequestUri.Query.TrimStart('?') : await request.Content.ReadAsStringAsync(ct);
            var fields = raw.Split('&', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('=', 2)).ToDictionary(x => WebUtility.UrlDecode(x[0]), x => WebUtility.UrlDecode(x[1]));
            object result;
            if (path.EndsWith("/debug_token")) result = new { data = new { is_valid = true, app_id = "test-app", scopes = new[] { "pages_read_engagement", "pages_manage_posts", "instagram_basic", "instagram_content_publish" } } };
            else if (path.EndsWith("/123") && request.Method == HttpMethod.Get) result = new { id = "123", name = "Probe", instagram_business_account = new { id = "456", username = "probe" } };
            else if (path.EndsWith("/content_publishing_limit")) result = new { data = new[] { new { quota_usage = 0, config = new { quota_total = 50 } } } };
            else if (request.Method == HttpMethod.Get) result = fields["fields"] == "status_code" ? new { id = path.Split('/').Last(), status_code = "FINISHED" } : new { id = "999", permalink = "https://www.instagram.com/p/probe/" };
            else if (path.EndsWith("/feed") || path.EndsWith("/media_publish") || (fields.TryGetValue("published", out var published) && published == "true"))
            {
                Interlocked.Increment(ref Publishes);
                if (Scenario == "timeout") throw new TaskCanceledException("Simulated lost response");
                if (fields.TryGetValue("caption", out var caption) && caption != Caption || fields.TryGetValue("message", out var message) && message != Caption) throw new Exception("Changed snapshot text");
                result = path.EndsWith("/photos") ? new { id = "800", post_id = "123_999" } : new { id = path.EndsWith("/feed") ? "123_999" : "999" };
            }
            else { Interlocked.Increment(ref Creates); result = new { id = (800 + Creates).ToString() }; }
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(result) };
        }
    }
    private sealed class ImageProvider : HttpMessageHandler
    {
        public bool Invalid;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (request.Headers.Authorization != null || request.RequestUri!.Host != "example.com") throw new Exception("Unexpected media request");
            byte[] bytes = Invalid ? "invalid media"u8.ToArray() : [0xff, 0xd8, 0xff, 0xc0, 0, 11, 8, 4, 56, 4, 56, 1, 1, 0x11, 0, 0xff, 0xd9];
            var content = new ByteArrayContent(bytes); content.Headers.ContentType = new("image/jpeg");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }
}

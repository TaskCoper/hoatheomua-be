using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentValidation;
using HoaTheoMua.Controllers;
using HoaTheoMua.Extentions;
using HoaTheoMua.Filters;
using HoaTheoMua.Middleware;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Core = HoaTheoMua.Service.PostCore;
using ConfigApi = HoaTheoMua.Service.Config;
using AiApi = HoaTheoMua.Service.AiService;
using GenerationApi = HoaTheoMua.Service.PostGeneration;

var connectionString = Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set POSTGEN_TEST_CONNECTION_STRING to a migrated, disposable local PostgreSQL database.");
var connectionSettings = new NpgsqlConnectionStringBuilder(connectionString);
if (connectionSettings.Host is not ("localhost" or "127.0.0.1"))
    throw new InvalidOperationException("This verification probe requires a local disposable database.");
var data = new NpgsqlDataSourceBuilder(connectionString);
data.EnableDynamicJson();
data.MapEnum<PlatformType>("PlatformType"); data.MapEnum<SystemPromptType>("SystemPromptType");
data.MapEnum<CardFormType>("CardFormType"); data.MapEnum<ClientHistoryType>("ClientHistoryType");
await using var dataSource = data.Build();
var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:55440");
builder.Configuration.AddInMemoryCollection(new Dictionary<string,string?> {
    ["JwtOption:Issuer"] = "postgen-test", ["JwtOption:Audience"] = "postgen-test",
    ["JwtOption:AccessTokenKey"] = "postgen-local-integration-test-key-64-characters-long-only-for-test"
});
builder.Services.AddDbContext<AppDbContext>(x => x.UseNpgsql(dataSource));
builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
builder.Services.AddControllers(x => x.Filters.Add<ValidationActionFilter>()).AddApplicationPart(typeof(PostCoreController).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Core.Service).Assembly);
builder.Services.AddScoped<Core.IService, Core.Service>();
builder.Services.AddScoped<GenerationApi.IService, GenerationApi.Service>();
var aiHandler = new ProbeAiHandler();
builder.Services.AddSingleton(aiHandler);
builder.Services.AddHttpClient<AiApi.IService, AiApi.Service>(c => c.Timeout = Timeout.InfiniteTimeSpan)
    .ConfigurePrimaryHttpMessageHandler(() => aiHandler);
builder.Services.ConfigureRateLimiter();
builder.Services.AddScoped<ConfigApi.IService, ConfigApi.Service>();
builder.Services.AddSingleton<HoaTheoMua.Service.Revalidate.IService, TestRevalidate>();
builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddJwtServices(builder.Configuration);
await using var app = builder.Build();
if (args.Contains("--migrate"))
{
    await using var migrationScope = app.Services.CreateAsyncScope();
    await migrationScope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
await app.StartAsync();
using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:55440") };
var user1 = Guid.NewGuid(); var user2 = Guid.NewGuid();
var evidence = new List<object>();
string Token(Guid userId, string role) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("postgen-test", "postgen-test",
    new[] { new Claim("UserId", userId.ToString()), new Claim(ClaimTypes.Role, role), new Claim("IsVerified", "true") },
    expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtOption:AccessTokenKey"]!)), SecurityAlgorithms.HmacSha256)));
async Task<JsonElement> Call(string name, HttpMethod method, string url, object? body, int status = 200, Guid? user = null, string role = "Admin")
{
    using var request = new HttpRequestMessage(method, url);
    if (role != "Anonymous") request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(user ?? user1, role));
    if (body != null) request.Content = JsonContent.Create(body);
    using var response = await client.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();
    if ((int)response.StatusCode != status) throw new Exception($"{name}: expected {status}, got {(int)response.StatusCode}: {content}");
    JsonElement json;
    try { json = string.IsNullOrWhiteSpace(content) ? JsonSerializer.SerializeToElement(new {}) : JsonDocument.Parse(content).RootElement.Clone(); }
    catch (JsonException) { json = JsonSerializer.SerializeToElement(new { detail = content }); }
    if (status == 429) Check(response.Headers.Contains("Retry-After"), "Heavy limiter returns retry delay");
    evidence.Add(new { name, method = method.Method, url, request = body, status, response = json });
    Console.WriteLine($"PASS {name} ({status})");
    return json;
}
void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
var input = new { title = "Hoa mùa thu", content = "Nội dung gốc", hashtag = "#hoa", images = new[] {
    new { imageUrl = "https://example.com/2.jpg", sortOrder = 2 },
    new { imageUrl = "https://example.com/1.jpg", sortOrder = 1 },
    new { imageUrl = "https://example.com/3.jpg", sortOrder = 3 } } };
try
{
    await Call("AnonymousDenied", HttpMethod.Get, "/api/v1/posts", null, 401, role: "Anonymous");
    await Call("CustomerDenied", HttpMethod.Get, "/api/v1/posts", null, 403, role: "Customer");
    await Call("CreateWithoutImages", HttpMethod.Post, "/api/v1/posts", new { title = "x", content = "y", images = Array.Empty<object>() }, 422);
    var created = await Call("CreatePostCore", HttpMethod.Post, "/api/v1/posts", input);
    var id = created.GetProperty("value").GetProperty("basePostId").GetGuid();
    var url = $"/api/v1/posts/{id}";
    var list = await Call("GetPostCoreList", HttpMethod.Get, "/api/v1/posts?pageSize=200&keyword=hoa", null);
    Check(list.GetProperty("value").GetProperty("items").GetArrayLength() == 1, "Post list filters keyword and has one item");
    var detail = await Call("GetPostCoreDetail", HttpMethod.Get, url, null);
    Check(!detail.GetProperty("value").TryGetProperty("images", out _), "Detail omits shared image history");
    var images = detail.GetProperty("value").GetProperty("baseImages").EnumerateArray().ToArray();
    Check(images.Select(x => x.GetProperty("sortOrder").GetInt32()).SequenceEqual(new[] {1,2,3}), "Source image ordering");
    await Call("FilterByPlatform", HttpMethod.Get, "/api/v1/posts?platform=facebook", null);
    await Call("CrossUserRead", HttpMethod.Get, url, null, 200, user2);
    var sharedList = await Call("CrossUserList", HttpMethod.Get, "/api/v1/posts", null, user: user2);
    Check(sharedList.GetProperty("value").GetProperty("items").EnumerateArray().Any(x => x.GetProperty("basePostId").GetGuid() == id), "Second user sees the shared post in the list");
    await Call("CrossUserUpdate", HttpMethod.Put, url, new { title = "Sửa bởi user khác", content = "Nội dung gốc", images = images.Select(x => new { id = x.GetProperty("id").GetGuid(), imageUrl = x.GetProperty("imageUrl").GetString(), sortOrder = x.GetProperty("sortOrder").GetInt32() }).ToArray() }, 200, user2);
    var updated = await Call("UpdatePostCore", HttpMethod.Put, url, new { title = "Title đã sửa", content = "Nội dung gốc", hashtag = "#hoa", images = images.Reverse().Select((x,i) => new { id = x.GetProperty("id").GetGuid(), imageUrl = x.GetProperty("imageUrl").GetString(), sortOrder = i+1 }).ToArray() });
    Check(updated.GetProperty("value").GetProperty("baseImages")[0].GetProperty("id").GetGuid() == images[2].GetProperty("id").GetGuid(), "Reorder succeeds against PostgreSQL unique index");
    var generationInput = new { platforms = new[] { new { platform = "facebook" }, new { platform = "instagram" } } };
    await Call("GenerateCustomerDenied", HttpMethod.Post, url + "/generate", generationInput, 403, role: "Customer");
    await Call("GenerateDuplicatePlatforms", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook" }, new { platform = "facebook" } } }, 422);
    await Call("GenerateImagesDeferred", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook", usePreset = true } } }, 422);
    var generated = await Call("GenerateTwoPlatforms", HttpMethod.Post, url + "/generate", generationInput);
    Check(!generated.GetProperty("value").TryGetProperty("images", out _), "Generate omits shared image history");
    Check(generated.GetProperty("value").GetProperty("platforms").GetArrayLength() == 2, "Generate returns two platform posts");
    Check(aiHandler.Calls == 2, "Exactly one chat completion per requested platform");
    foreach (var platform in generated.GetProperty("value").GetProperty("platforms").EnumerateArray())
    {
        Check(platform.GetProperty("status").GetString() == "Generated", "Generated platform state");
        Check(!platform.TryGetProperty("versions", out _), "Platform returns only one official content object without versions");
        Check(platform.GetProperty("content").GetProperty("versionNo").GetInt32() == 1, "Official content is version one");
    }
    aiHandler.FailOnCall = aiHandler.Calls + 2;
    await Call("GenerateSecondPlatformFailure", HttpMethod.Post, url + "/generate", generationInput, 502);
    var failedGeneration = await Call("FailedGenerationPreservesOfficial", HttpMethod.Get, url, null);
    Check(failedGeneration.GetProperty("value").GetProperty("platforms").EnumerateArray().All(x => x.GetProperty("status").GetString() == "Failed" && !x.TryGetProperty("versions", out _) && x.GetProperty("content").GetProperty("versionNo").GetInt32() == 1), "Failed request preserves previous official versions without partial writes");
    await Call("RemovedPresetGet", HttpMethod.Get, "/api/v1/post-gen/presets", null, 404);
    await Call("RemovedPresetPut", HttpMethod.Put, "/api/v1/post-gen/presets", new {}, 404);
    await Call("RemovedStandardGet", HttpMethod.Get, "/api/v1/platform-standards", null, 404);
    await Call("RemovedStandardPut", HttpMethod.Put, $"/api/v1/platform-standards/{Guid.NewGuid()}", new {}, 404);
    var preset = await Call("CreateRatioConfig", HttpMethod.Post, "/api/v1/configs", new
    {
        group = "postgen", key = "postgen.ratio-preset", kind = "Setting",
        value = new { instagram = new { ratios = new[] { "Portrait4x5" }, imageSelectionMode = "all_images" } }
    });
    var removedStandards = await Call("RemovedPlatformConfigKeys", HttpMethod.Get, "/api/v1/configs/settings?groupName=postgen&key=postgen.platform.", null);
    Check(removedStandards.GetProperty("value").GetArrayLength() == 0, "No separate platform config remains");
    var presetAfter = await Call("RatioPresetUnchanged", HttpMethod.Get,
        $"/api/v1/configs/{preset.GetProperty("value").GetProperty("id").GetGuid()}", null);
    Check(presetAfter.GetProperty("value").GetProperty("value").GetProperty("instagram").GetProperty("ratios")[0].GetString() == "Portrait4x5", "Consolidated configs preserve the ratio preset");
    const string promptSettingsUrl = "/api/v1/configs/settings?groupName=postgen&key=postgen.system-prompt.";
    await Call("AnonymousPromptReadDenied", HttpMethod.Get, promptSettingsUrl, null, 401, role: "Anonymous");
    await Call("StaffPromptReadDenied", HttpMethod.Get, promptSettingsUrl, null, 403, role: "Staff");
    var promptList = await Call("ReadPlatformPrompts", HttpMethod.Get, promptSettingsUrl, null);
    var prompts = promptList.GetProperty("value").EnumerateArray().ToArray();
    Check(prompts.Length == 3, "Three independent system prompts seeded in Config");
    Check(prompts.Select(x => x.GetProperty("value").GetProperty("content").GetString()).Distinct().Count() == 3, "Each platform has distinct complete prompt content");
    var facebookPrompt = prompts.Single(x => x.GetProperty("key").GetString() == "postgen.system-prompt.facebook");
    var promptId = facebookPrompt.GetProperty("id").GetGuid();
    var promptUrl = $"/api/v1/configs/{promptId}";
    var promptUpdate = new { group = "postgen", key = "postgen.system-prompt.facebook", kind = "Setting", isPublic = false,
        value = new { isActive = true, content = "Prompt Facebook tùy chỉnh\nGiữ nguyên nội dung đầy đủ." } };
    await Call("StaffPromptUpdateDenied", HttpMethod.Put, promptUrl, promptUpdate, 403, role: "Staff");
    await Call("UpdateFacebookPrompt", HttpMethod.Put, promptUrl, promptUpdate);
    var promptSaved = await Call("SecondAdminReadsPrompt", HttpMethod.Get, promptUrl, null, user: user2);
    Check(promptSaved.GetProperty("value").GetProperty("value").GetProperty("content").GetString() == promptUpdate.value.content, "Updated prompt roundtrips exactly through Config API");
    await Call("DisableFacebookViaPrompt", HttpMethod.Put, promptUrl, new { group = "postgen", key = "postgen.system-prompt.facebook", kind = "Setting", isPublic = false, value = new { isActive = false, content = promptUpdate.value.content } });
    var callsBeforeDisabled = aiHandler.Calls;
    await Call("GenerateDisabledPrompt", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook" } } }, 400);
    Check(aiHandler.Calls == callsBeforeDisabled, "Disabled prompt prevents outbound AI calls");
    await Call("EnableFacebookViaPrompt", HttpMethod.Put, promptUrl, promptUpdate);
    var promptsAfter = await Call("OtherSystemPromptsUnchanged", HttpMethod.Get, promptSettingsUrl, null);
    foreach (var original in prompts.Where(x => x.GetProperty("id").GetGuid() != promptId))
        Check(promptsAfter.GetProperty("value").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == original.GetProperty("id").GetGuid()).GetProperty("value").GetRawText() == original.GetProperty("value").GetRawText(), "Updating Facebook leaves another platform prompt unchanged");
    await Call("RejectDuplicatePrompt", HttpMethod.Post, "/api/v1/configs", promptUpdate, 400);
    var privatePrompts = await Call("SystemPromptsPrivate", HttpMethod.Get, "/api/v1/configs/public?groupName=postgen&key=postgen.system-prompt.", null, role: "Anonymous");
    Check(privatePrompts.GetProperty("value").ValueKind == JsonValueKind.Null, "System prompts remain private");
    await Call("DeleteFacebookPrompt", HttpMethod.Delete, promptUrl, null, user: user2);
    var deletedPrompt = await Call("DeletedPromptExcluded", HttpMethod.Get, promptSettingsUrl + "facebook", null);
    Check(deletedPrompt.GetProperty("value").GetArrayLength() == 0, "Deleted system prompt excluded from settings");
    await Call("GenerateMissingPromptNoFallback", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook" } } }, 400);
    for (var i = 0; i < 4; i++)
        await Call("GenerateMissingPromptRateAttempt" + i, HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook" } } }, 400);
    await Call("GenerateHeavyRateLimited", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook" } } }, 429);
    var replacementPrompt = await Call("RecreateFacebookPrompt", HttpMethod.Post, "/api/v1/configs", promptUpdate);
    Check(replacementPrompt.GetProperty("value").GetProperty("id").GetGuid() != promptId, "Recreated system prompt has a new Config id");
    await Call("DeletePostCore", HttpMethod.Delete, url, null, user: user2);
    var after = await Call("DeletedPostList", HttpMethod.Get, "/api/v1/posts", null);
    Check(after.GetProperty("value").GetProperty("items").GetArrayLength() == 0, "Deleted post excluded from list");
    await Call("DeletedPostDetail", HttpMethod.Get, url, null, 404);
    await using var scope = app.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Check((await db.BasePosts.SingleAsync(x => x.Id == id)).IsDeleted, "Post is soft deleted in PostgreSQL");
    Check(await db.BasePostImages.CountAsync(x => x.BasePostId == id && x.IsDeleted) == 3, "Source images retained in PostgreSQL");
    Check(await db.SystemPrompts.CountAsync(x => x.Type == SystemPromptType.Post) == 1, "Post system prompt seeded once");
    Check(await db.Configs.CountAsync(x => x.Group == "postgen" && (x.Key == "postgen.enabled" || x.Key == "postgen.image_instruction")) == 2, "Two global PostGen configs seeded");
    Check((await db.Configs.AsNoTracking().SingleAsync(x => x.Id == promptId)).IsDeleted, "Deleted system prompt retained in PostgreSQL");
    Check(await db.Configs.CountAsync(x => x.Group == "postgen" && x.Key == "postgen.system-prompt.facebook" && !x.IsDeleted) == 1, "Exactly one active Facebook system prompt in PostgreSQL");
    Check(!await db.Configs.AnyAsync(x => x.Group == "postgen" && x.Key.StartsWith("postgen.platform.")), "Retired platform keys removed from PostgreSQL");
    Check(await db.GeneratedPosts.CountAsync() == 2, "Only two successful content versions persisted");
    foreach (var version in await db.GeneratedPosts.AsNoTracking().ToListAsync())
    {
        Check(version.SystemPromptConfigId.HasValue && version.SystemPromptId == null && version.ParentGeneratedPostId == null, "AI version uses Config reference with no legacy or parent prompt");
        Check(aiHandler.Prompts.Contains(version.PromptSnapshot!), "Snapshot exactly matches sent system prompt");
        Check(version.AiRequestId != null && version.AiModel == "chatgpt/gpt-5.6" && version.DurationMs.HasValue, "Provider metadata persisted");
    }
    Check(!await db.PostHistories.AnyAsync() && !await db.ClientHistories.AnyAsync(), "No legacy history writes");
    Check(!await db.GeneratedCards.AnyAsync(x => x.FormType == CardFormType.Calligraphy), "Existing CardFormType mapping remains queryable");
    Check(!await db.ClientHistories.AnyAsync(x => x.Type == ClientHistoryType.Post), "Existing ClientHistoryType mapping remains queryable");
    await File.WriteAllTextAsync(Path.Combine(Path.GetTempPath(), "postgen-api-evidence.json"), JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
}
finally { await app.StopAsync(); }

// Only external cache revalidation is stubbed; ConfigController/Service and PostgreSQL are real.
sealed class TestRevalidate : HoaTheoMua.Service.Revalidate.IService
{
    public Task<string> RevalidateApi(JsonElement body) => Task.FromResult("test revalidation");
}

// Network boundary only: real AiService serializes/parses the provider contract.
sealed class ProbeAiHandler : HttpMessageHandler
{
    public int Calls { get; private set; }
    public int FailOnCall { get; set; }
    public List<string> Prompts { get; } = new();
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
        var messages = json.RootElement.GetProperty("messages");
        Prompts.Add(messages[0].GetProperty("content")[0].GetProperty("text").GetString()!);
        if (Calls == FailOnCall) return new(HttpStatusCode.InternalServerError);
        return new(HttpStatusCode.OK) { Content = JsonContent.Create(new { id = "chatcmpl-probe-" + Calls,
            model = "chatgpt/gpt-5.6", choices = new[] { new { index = 0, finish_reason = "stop", message = new { role = "assistant",
            content = JsonSerializer.Serialize(new { title = "Bó hoa Cảm Ơn", content = "Nội dung AI kiểm thử", hashtag = "#HoaTheoMua" }) } } } }) };
    }
}

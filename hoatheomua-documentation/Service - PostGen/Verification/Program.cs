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
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
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
    var json = string.IsNullOrWhiteSpace(content) ? JsonSerializer.SerializeToElement(new {}) : JsonDocument.Parse(content).RootElement.Clone();
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
    var images = detail.GetProperty("value").GetProperty("baseImages").EnumerateArray().ToArray();
    Check(images.Select(x => x.GetProperty("sortOrder").GetInt32()).SequenceEqual(new[] {1,2,3}), "Source image ordering");
    await Call("FilterByPlatform", HttpMethod.Get, "/api/v1/posts?platform=facebook", null);
    await Call("CrossUserRead", HttpMethod.Get, url, null, 200, user2);
    var sharedList = await Call("CrossUserList", HttpMethod.Get, "/api/v1/posts", null, user: user2);
    Check(sharedList.GetProperty("value").GetProperty("items").EnumerateArray().Any(x => x.GetProperty("basePostId").GetGuid() == id), "Second user sees the shared post in the list");
    await Call("CrossUserUpdate", HttpMethod.Put, url, new { title = "Sửa bởi user khác", content = "Nội dung gốc", images = images.Select(x => new { id = x.GetProperty("id").GetGuid(), imageUrl = x.GetProperty("imageUrl").GetString(), sortOrder = x.GetProperty("sortOrder").GetInt32() }).ToArray() }, 200, user2);
    var updated = await Call("UpdatePostCore", HttpMethod.Put, url, new { title = "Title đã sửa", content = "Nội dung gốc", hashtag = "#hoa", images = images.Reverse().Select((x,i) => new { id = x.GetProperty("id").GetGuid(), imageUrl = x.GetProperty("imageUrl").GetString(), sortOrder = i+1 }).ToArray() });
    Check(updated.GetProperty("value").GetProperty("baseImages")[0].GetProperty("id").GetGuid() == images[2].GetProperty("id").GetGuid(), "Reorder succeeds against PostgreSQL unique index");
    await Call("RemovedPresetGet", HttpMethod.Get, "/api/v1/post-gen/presets", null, 404);
    await Call("RemovedPresetPut", HttpMethod.Put, "/api/v1/post-gen/presets", new {}, 404);
    await Call("RemovedStandardGet", HttpMethod.Get, "/api/v1/platform-standards", null, 404);
    await Call("RemovedStandardPut", HttpMethod.Put, $"/api/v1/platform-standards/{Guid.NewGuid()}", new {}, 404);
    var preset = await Call("CreateRatioConfig", HttpMethod.Post, "/api/v1/configs", new
    {
        group = "postgen", key = "postgen.ratio-preset", kind = "Setting",
        value = new { instagram = new { ratios = new[] { "Portrait4x5" }, imageSelectionMode = "all_images" } }
    });
    const string settingsUrl = "/api/v1/configs/settings?groupName=postgen&key=postgen.platform.";
    await Call("AnonymousConfigDenied", HttpMethod.Get, settingsUrl, null, 401, role: "Anonymous");
    await Call("StaffConfigDenied", HttpMethod.Get, settingsUrl, null, 403, role: "Staff");
    var standards = await Call("ReadPlatformConfigs", HttpMethod.Get, settingsUrl, null);
    var configs = standards.GetProperty("value").EnumerateArray().ToArray();
    Check(configs.Length == 3, "Three platform configs migrated");
    var instagram = configs.Single(x => x.GetProperty("key").GetString() == "postgen.platform.instagram");
    var standardId = instagram.GetProperty("id").GetGuid();
    var standardUrl = $"/api/v1/configs/{standardId}";
    var standard = new { group = "postgen", key = "postgen.platform.instagram", kind = "Setting", isPublic = false,
        value = new { description = "Mô tả chuẩn được cập nhật", maxContentLength = 2000, maxHashtagCount = 10, maxImageCount = 9, isActive = false } };
    await Call("UpdatePlatformConfig", HttpMethod.Put, standardUrl, standard);
    var saved = await Call("ReadUpdatedPlatformConfig", HttpMethod.Get, standardUrl, null, user: user2);
    Check(saved.GetProperty("value").GetProperty("value").GetProperty("description").GetString() == standard.value.description, "Second admin reads persisted platform update");
    var others = await Call("OtherPlatformsUnchanged", HttpMethod.Get, settingsUrl, null);
    foreach (var original in configs.Where(x => x.GetProperty("id").GetGuid() != standardId))
        Check(others.GetProperty("value").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == original.GetProperty("id").GetGuid()).GetProperty("value").GetRawText() == original.GetProperty("value").GetRawText(), "Other platform config unchanged");
    await Call("DeletePlatformConfig", HttpMethod.Delete, standardUrl, null);
    var missing = await Call("DeletedPlatformExcluded", HttpMethod.Get, settingsUrl + "instagram", null);
    Check(missing.GetProperty("value").GetArrayLength() == 0, "Deleted config excluded from settings");
    var recreated = await Call("CreatePlatformConfig", HttpMethod.Post, "/api/v1/configs", standard);
    Check(recreated.GetProperty("value").GetProperty("id").GetGuid() != standardId, "Create after soft delete allocates a new config id");
    await Call("RejectDuplicatePlatformKey", HttpMethod.Post, "/api/v1/configs", standard, 400);
    var publicConfigs = await Call("PlatformConfigsPrivate", HttpMethod.Get, "/api/v1/configs/public?groupName=postgen&key=postgen.platform.", null, role: "Anonymous");
    Check(publicConfigs.GetProperty("value").ValueKind == JsonValueKind.Null, "Platform configs remain private");
    var presetAfter = await Call("RatioPresetUnchanged", HttpMethod.Get,
        $"/api/v1/configs/{preset.GetProperty("value").GetProperty("id").GetGuid()}", null);
    Check(presetAfter.GetProperty("value").GetProperty("value").GetProperty("instagram").GetProperty("ratios")[0].GetString() == "Portrait4x5", "Platform changes preserve the ratio preset");
    await Call("DeletePostCore", HttpMethod.Delete, url, null, user: user2);
    var after = await Call("DeletedPostList", HttpMethod.Get, "/api/v1/posts", null);
    Check(after.GetProperty("value").GetProperty("items").GetArrayLength() == 0, "Deleted post excluded from list");
    await Call("DeletedPostDetail", HttpMethod.Get, url, null, 404);
    await using var scope = app.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Check((await db.BasePosts.SingleAsync(x => x.Id == id)).IsDeleted, "Post is soft deleted in PostgreSQL");
    Check(await db.BasePostImages.CountAsync(x => x.BasePostId == id && x.IsDeleted) == 3, "Source images retained in PostgreSQL");
    Check(await db.SystemPrompts.CountAsync(x => x.Type == SystemPromptType.Post) == 1, "Post system prompt seeded once");
    Check(await db.Configs.CountAsync(x => x.Group == "postgen" && (x.Key == "postgen.enabled" || x.Key == "postgen.image_instruction")) == 2, "Two global PostGen configs seeded");
    Check((await db.Configs.AsNoTracking().SingleAsync(x => x.Id == standardId)).IsDeleted, "Deleted platform config retained in PostgreSQL");
    Check(await db.Configs.CountAsync(x => x.Group == "postgen" && x.Key == "postgen.platform.instagram" && !x.IsDeleted) == 1, "One active Instagram config after recreation");
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

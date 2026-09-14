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
    new[] { new Claim("UserId", userId.ToString()), new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, role), new Claim("IsVerified", "true") },
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
    foreach (var (name, body) in new (string, object)[] {
        ("Omitted", new { title = "x", content = "y" }),
        ("Null", new { title = "x", content = "y", images = (object?)null }),
        ("Empty", new { title = "x", content = "y", images = Array.Empty<object>() }) })
    {
        var draft = await Call("CreateWithoutImages" + name, HttpMethod.Post, "/api/v1/posts", body);
        var draftUrl = "/api/v1/posts/" + draft.GetProperty("value").GetProperty("basePostId").GetGuid();
        Check(draft.GetProperty("value").GetProperty("baseImages").GetArrayLength() == 0, "Draft has no source images");
        await Call("EditWithoutImages" + name, HttpMethod.Put, draftUrl, new { title = "x", content = "Updated draft", images = Array.Empty<object>() });
        await Call("UpdateMissingImagesRejected" + name, HttpMethod.Put, draftUrl, new { title = "x", content = "y" }, 422);
        var attached = await Call("AttachImagesLater" + name, HttpMethod.Put, draftUrl, input);
        Check(attached.GetProperty("value").GetProperty("baseImages").GetArrayLength() == 3, "Can attach source images after creation");
        await Call("DeleteDraft" + name, HttpMethod.Delete, draftUrl, null);
    }
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
    var generationInput = new { mode = "content", platforms = new[] { new { platform = "facebook" }, new { platform = "instagram" } } };
    await Call("GenerateCustomerDenied", HttpMethod.Post, url + "/generate", generationInput, 403, role: "Customer");
    await Call("GenerateDuplicatePlatforms", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook" }, new { platform = "facebook" } } }, 422);
    await Call("GenerateConflictingImageSelection", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook", usePreset = true, ratios = Array.Empty<object>() } } }, 422);
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
    // Separate test identity keeps the existing rate-limit regression deterministic.
    var imageUser = Guid.NewGuid();
    var sourceIds = updated.GetProperty("value").GetProperty("baseImages").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToArray();
    var imageInput = new { mode = "images", platforms = new object[] {
        new { platform = "facebook", ratios = new[] { new { ratioCode = "Portrait4x5", basePostImageIds = sourceIds } } },
        new { platform = "instagram", usePreset = true } } };
    var imageCallsBefore = aiHandler.ImageCalls;
    var textCallsBeforeImages = aiHandler.Calls - aiHandler.ImageCalls;
    aiHandler.FailImageNumber = imageCallsBefore + 2;
    var imageGenerated = await Call("GenerateSharedImagesPartial", HttpMethod.Post, url + "/generate", imageInput, user: imageUser);
    Check(aiHandler.ImageCalls - imageCallsBefore == 3, "Three shared image pairs produce exactly three AI image calls");
    var imagePlatforms = imageGenerated.GetProperty("value").GetProperty("platforms").EnumerateArray().ToArray();
    Check(imagePlatforms.All(x => x.GetProperty("status").GetString() == "Partial" && x.GetProperty("images").GetArrayLength() == 2), "Partial returns selected successful images only");
    var selected = imagePlatforms[0].GetProperty("images").EnumerateArray().Select(x => x.GetProperty("selectedGeneratedPostImageId").GetGuid()).ToArray();
    Check(selected.SequenceEqual(imagePlatforms[1].GetProperty("images").EnumerateArray().Select(x => x.GetProperty("selectedGeneratedPostImageId").GetGuid())), "Platforms share generated image IDs");
    Check(imagePlatforms.All(p => p.GetProperty("images").EnumerateArray().All(x => x.GetProperty("imageUrl").GetString()!.StartsWith("https://example.com/generated-") && x.GetProperty("pixelHeight").GetInt32() == 1350)), "Official images include URLs and target dimensions");
    var readImages = await Call("ReadSelectedImages", HttpMethod.Get, url, null);
    Check(readImages.GetProperty("value").GetProperty("platforms")[0].GetProperty("status").GetString() == "Partial", "Partial state survives a fresh GET");
    var retryImages = await Call("GenerateImagesAgain", HttpMethod.Post, url + "/generate", imageInput, user: imageUser);
    var retryPlatforms = retryImages.GetProperty("value").GetProperty("platforms").EnumerateArray().ToArray();
    Check(retryPlatforms.All(x => x.GetProperty("status").GetString() == "Generated" && x.GetProperty("images").GetArrayLength() == 3), "Retry fills previously empty slots");
    Check(selected.All(i => retryPlatforms[0].GetProperty("images").EnumerateArray().Any(x => x.GetProperty("selectedGeneratedPostImageId").GetGuid() == i)), "Retry retains previously selected image versions");
    var beforeInvalid = aiHandler.Calls;
    await Call("RejectForeignImage", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook", ratios = new[] { new { ratioCode = "Square1x1", basePostImageIds = new[] { Guid.NewGuid() } } } } } }, 400, imageUser);
    await Call("RejectImagePairLimit", HttpMethod.Post, url + "/generate", new { platforms = new[] { new { platform = "facebook", ratios = Enum.GetNames<AspectRatioType>().Select(r => new { ratioCode = r, basePostImageIds = sourceIds }).ToArray() } } }, 422, imageUser);
    Check(aiHandler.Calls == beforeInvalid, "Invalid image plans do not call AI");
    aiHandler.FailAllImages = true;
    var allFailed = await Call("GenerateAllImagesFailed", HttpMethod.Post, url + "/generate", imageInput, user: imageUser);
    Check(allFailed.GetProperty("value").GetProperty("platforms").EnumerateArray().All(p => p.GetProperty("status").GetString() == "Failed" && p.GetProperty("images").GetArrayLength() == 3 && p.GetProperty("content").ValueKind == JsonValueKind.Object), "All image failures retain prior images and existing content");
    aiHandler.FailAllImages = false;
    Check(aiHandler.Calls - aiHandler.ImageCalls == textCallsBeforeImages, "Images mode never calls content generation");
    foreach (var platform in allFailed.GetProperty("value").GetProperty("platforms").EnumerateArray())
    {
        var original = generated.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == platform.GetProperty("platform").GetString());
        Check(original.GetProperty("content").GetRawText() == platform.GetProperty("content").GetRawText(), "Image success retry and failure preserve official content exactly");
    }
    await Call("RejectInvalidGenerationMode", HttpMethod.Post, url + "/generate", new { mode = "unknown", platforms = new[] { new { platform = "facebook" } } }, 422, imageUser);
    await Call("RejectImageModeWithoutSelection", HttpMethod.Post, url + "/generate", new { mode = "images", platforms = new[] { new { platform = "facebook" } } }, 422, imageUser);
    await Call("RejectContentModeWithImages", HttpMethod.Post, url + "/generate", new { mode = "content", platforms = new[] { new { platform = "facebook", usePreset = true } } }, 422, imageUser);
    var contentOnly = await Call("GenerateContentAfterImages", HttpMethod.Post, url + "/generate", new { mode = "content", platforms = new[] { new { platform = "instagram" } } }, user: imageUser);
    var instagramAfter = contentOnly.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == "instagram");
    var instagramBefore = allFailed.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == "instagram");
    Check(instagramAfter.GetProperty("images").GetRawText() == instagramBefore.GetProperty("images").GetRawText(), "Content mode preserves selected images");
    Check(instagramAfter.GetProperty("content").GetProperty("versionNo").GetInt32() == 2, "Only explicit content generation increments content version");
    Check(aiHandler.ImageCalls == imageCallsBefore + 9, "Content mode does not call image generation");
    var versionUser = Guid.NewGuid();
    var contentUrl = url + "/platforms/facebook/content";
    var originalContent = generated.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == "facebook").GetProperty("content");
    var sourceV1 = originalContent.GetProperty("generationId").GetGuid();
    await Call("ContentHistoryAnonymousDenied", HttpMethod.Get, contentUrl + "/versions", null, 401, role: "Anonymous");
    await Call("ContentEditCustomerDenied", HttpMethod.Put, contentUrl, new { content = "Denied" }, 403, role: "Customer");
    await Call("ContentSelectCustomerDenied", HttpMethod.Put, contentUrl + "/official", new { generationId = sourceV1 }, 403, role: "Customer");
    await Call("ContentRegenerateCustomerDenied", HttpMethod.Post, contentUrl + "/regenerate", new {}, 403, role: "Customer");
    await Call("ContentMissingPlatform", HttpMethod.Get, url + "/platforms/zalo/content/versions", null, 404, versionUser);
    await Call("ContentUnknownPlatform", HttpMethod.Get, url + "/platforms/tiktok/content/versions", null, 422, versionUser);
    var regeneratedContent = await Call("RegenerateContentFromVersion", HttpMethod.Post, contentUrl + "/regenerate", new { sourceGenerationId = sourceV1, feedback = "First feedback" }, user: versionUser);
    var v2 = regeneratedContent.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == "facebook").GetProperty("content");
    Check(v2.GetProperty("parentGeneratedPostId").GetGuid() == sourceV1 && v2.GetProperty("versionNo").GetInt32() == 2, "Regenerate preserves source lineage and selects v2");
    var beforeManualCalls = aiHandler.Calls;
    var manualResponse = await Call("EditContentManual", HttpMethod.Put, contentUrl, new { title = "Manual", content = "Manual from HTTP", hashtag = "#manual" }, user: versionUser, role: "Staff");
    var manualVersion = manualResponse.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == "facebook").GetProperty("content");
    var manualId = manualVersion.GetProperty("generationId").GetGuid();
    var manualAgain = await Call("EditContentManualAgain", HttpMethod.Put, contentUrl, new { title = "Manual", content = "Manual saved twice", hashtag = "#manual" }, user: user2);
    var manualAgainVersion = manualAgain.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == "facebook").GetProperty("content");
    Check(manualAgainVersion.GetProperty("generationId").GetGuid() == manualId && manualAgainVersion.GetProperty("versionNo").GetInt32() == 3 && aiHandler.Calls == beforeManualCalls, "Repeated cross-user manual saves reuse v3 without AI");
    await Call("RegenerateContentFromManual", HttpMethod.Post, contentUrl + "/regenerate", new { sourceGenerationId = manualId, feedback = "Only this feedback" }, user: versionUser);
    Check(aiHandler.ContentInputs.Last().GetProperty("previousContent").GetProperty("content").GetString() == "Manual saved twice", "AI receives the selected manual source");
    Check(aiHandler.ContentInputs.Last().GetProperty("feedback").GetString() == "Only this feedback" && !aiHandler.ContentInputs.Last().ToString().Contains("First feedback"), "Feedback is not accumulated");
    var history = await Call("GetContentVersions", HttpMethod.Get, contentUrl + "/versions", null, user: versionUser, role: "Staff");
    var contentVersions = history.GetProperty("value").GetProperty("versions").EnumerateArray().ToArray();
    Check(contentVersions.Select(v => v.GetProperty("source").GetString()).SequenceEqual(new[] { "Ai", "Ai", "Manual", "Ai" }), "Four content versions follow Ai Ai Manual Ai lifecycle");
    Check(contentVersions.Count(v => v.GetProperty("isOfficial").GetBoolean()) == 1, "Exactly one official content in history");
    var historyAgain = await Call("GetContentVersionsAgain", HttpMethod.Get, contentUrl + "/versions", null, user: versionUser);
    Check(history.GetProperty("value").GetRawText() == historyAgain.GetProperty("value").GetRawText(), "History ordering and version numbers remain stable");
    await Call("RegenerateContentBranchFromOld", HttpMethod.Post, contentUrl + "/regenerate", new { sourceGenerationId = sourceV1, feedback = "Branch from oldest" }, user: versionUser);
    Check(aiHandler.ContentInputs.Last().GetProperty("previousContent").GetProperty("content").GetString() == originalContent.GetProperty("content").GetString(), "Branch sends original version rather than latest content");
    var beforeSelectionCalls = aiHandler.Calls;
    var selectedOld = await Call("SelectOfficialOldContent", HttpMethod.Put, contentUrl + "/official", new { generationId = sourceV1 }, user: versionUser);
    var selectedFacebook = selectedOld.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == "facebook");
    Check(selectedFacebook.GetProperty("content").GetProperty("versionNo").GetInt32() == 1 && aiHandler.Calls == beforeSelectionCalls, "Selecting old content does not generate or create a version");
    Check(selectedFacebook.GetProperty("images").GetRawText() == allFailed.GetProperty("value").GetProperty("platforms").EnumerateArray().Single(p => p.GetProperty("platform").GetString() == "facebook").GetProperty("images").GetRawText(), "All content workflows preserve selected image IDs and ordering");
    Check(!selectedFacebook.TryGetProperty("versions", out _), "Mutation detail still omits history");
    var oldOfficial = await Call("M7OfficialOldContent", HttpMethod.Get, url + "/platforms/facebook/official", null, user: versionUser);
    Check(oldOfficial.GetProperty("value").GetProperty("generationId").GetGuid() == sourceV1
        && oldOfficial.GetProperty("value").GetProperty("content").GetString() == originalContent.GetProperty("content").GetString(), "Official payload uses chosen old content, not newest version");
    var foreignContent = instagramAfter.GetProperty("content").GetProperty("generationId").GetGuid();
    await Call("ContentForeignSourceRejected", HttpMethod.Post, contentUrl + "/regenerate", new { sourceGenerationId = foreignContent }, 400, versionUser);
    await Call("ContentForeignOfficialRejected", HttpMethod.Put, contentUrl + "/official", new { generationId = foreignContent }, 400, versionUser);
    await Call("ContentEmptyBodyRejected", HttpMethod.Put, contentUrl, new { content = " " }, 422, versionUser);
    await Call("ContentFeedbackLimitRejected", HttpMethod.Post, contentUrl + "/regenerate", new { feedback = new string('x', 2001) }, 422, versionUser);
    aiHandler.FailOnCall = aiHandler.Calls + 1;
    await Call("ContentRegenerationFailure", HttpMethod.Post, contentUrl + "/regenerate", new {}, 502, versionUser);
    var afterContentFailure = await Call("ContentHistoryAfterFailure", HttpMethod.Get, contentUrl + "/versions", null, user: versionUser);
    Check(afterContentFailure.GetProperty("value").GetProperty("versions").GetArrayLength() == 5 && afterContentFailure.GetProperty("value").GetProperty("officialGenerationId").GetGuid() == sourceV1, "Failed content regeneration adds no version and keeps selected old content");
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
    await Call("DeletedPostContentHistory", HttpMethod.Get, contentUrl + "/versions", null, 404, versionUser);
    await using var scope = app.Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Check((await db.BasePosts.SingleAsync(x => x.Id == id)).IsDeleted, "Post is soft deleted in PostgreSQL");
    Check(await db.BasePostImages.CountAsync(x => x.BasePostId == id && x.IsDeleted) == 3, "Source images retained in PostgreSQL");
    Check(await db.SystemPrompts.CountAsync(x => x.Type == SystemPromptType.Post) == 1, "Post system prompt seeded once");
    Check(await db.Configs.CountAsync(x => x.Group == "postgen" && (x.Key == "postgen.enabled" || x.Key == "postgen.image_instruction")) == 2, "Two global PostGen configs seeded");
    Check((await db.Configs.AsNoTracking().SingleAsync(x => x.Id == promptId)).IsDeleted, "Deleted system prompt retained in PostgreSQL");
    Check(await db.Configs.CountAsync(x => x.Group == "postgen" && x.Key == "postgen.system-prompt.facebook" && !x.IsDeleted) == 1, "Exactly one active Facebook system prompt in PostgreSQL");
    Check(!await db.Configs.AnyAsync(x => x.Group == "postgen" && x.Key.StartsWith("postgen.platform.")), "Retired platform keys removed from PostgreSQL");
    Check(await db.GeneratedPosts.CountAsync() == 7, "Seven content versions persisted across both platforms");
    foreach (var version in await db.GeneratedPosts.AsNoTracking().Where(x => x.Source == ContentSource.Ai).ToListAsync())
    {
        Check(version.SystemPromptConfigId.HasValue && version.SystemPromptId == null, "AI version uses Config reference with no legacy prompt");
        Check(aiHandler.Prompts.Contains(version.PromptSnapshot!), "Snapshot exactly matches sent system prompt");
        Check(version.AiRequestId != null && version.AiModel == "chatgpt/gpt-5.6" && version.DurationMs.HasValue, "Provider metadata persisted");
    }
    var manualStored = await db.GeneratedPosts.SingleAsync(x => x.Source == ContentSource.Manual);
    Check(manualStored.Id == manualId && manualStored.Content == "Manual saved twice" && manualStored.SystemPromptConfigId == null && manualStored.PromptSnapshot == null && manualStored.AiModel == null, "Manual row remains preserved without AI metadata");
    Check(await db.GeneratedPosts.CountAsync(x => x.ParentGeneratedPostId == manualId) == 1, "Generated child pins manual revision");
    Check(await db.GeneratedPostImages.CountAsync(x => x.BasePostId == id) == 9, "Nine image attempts retained after soft delete");
    Check(await db.GeneratedPostImages.CountAsync(x => x.BasePostId == id && x.Status == ImageStatus.Failed && x.ImageUrl == null && x.ErrorMessage != null) == 4, "Four failed image attempts retain errors without URL");
    Check(await db.PlatformPostImages.CountAsync(x => x.SelectedGeneratedPostImageId != null) == 6, "Six platform slots retain selected images");
    Check(!await db.PostHistories.AnyAsync() && !await db.ClientHistories.AnyAsync(), "No legacy history writes");
    Check(!await db.GeneratedCards.AnyAsync(x => x.FormType == CardFormType.Calligraphy), "Existing CardFormType mapping remains queryable");
    Check(!await db.ClientHistories.AnyAsync(x => x.Type == ClientHistoryType.Post), "Existing ClientHistoryType mapping remains queryable");
    // Milestone 6: shared history and independent platform selections through real HTTP.
    var imageVersionUser = Guid.NewGuid(); aiHandler.FailAllImages = false; aiHandler.FailImageNumber = 0;
    var imageDraft = await Call("M6CreatePost", HttpMethod.Post, "/api/v1/posts", input, user: imageVersionUser);
    var imagePostId = imageDraft.GetProperty("value").GetProperty("basePostId").GetGuid();
    var imagePostUrl = "/api/v1/posts/" + imagePostId;
    var imageHistoryUrl = imagePostUrl + "/images";
    var imageSources = imageDraft.GetProperty("value").GetProperty("baseImages").EnumerateArray().Take(2).ToArray();
    var versionSourceIds = imageSources.Select(x => x.GetProperty("id").GetGuid()).ToArray();
    await Call("M6AnonymousDenied", HttpMethod.Get, imageHistoryUrl, null, 401, role: "Anonymous");
    await Call("M6CustomerDenied", HttpMethod.Get, imageHistoryUrl, null, 403, role: "Customer");
    var emptyImageHistory = await Call("M6EmptyHistory", HttpMethod.Get, imageHistoryUrl, null, user: imageVersionUser);
    Check(emptyImageHistory.GetProperty("value").GetProperty("groups").GetArrayLength() == 0, "Image history is empty before generating");
    await Call("M7MissingPlatform", HttpMethod.Get, imagePostUrl + "/platforms/facebook/official", null, 404, imageVersionUser);
    await Call("M7AnonymousDenied", HttpMethod.Get, imagePostUrl + "/platforms/facebook/official", null, 401, role: "Anonymous");
    await Call("M7CustomerDenied", HttpMethod.Get, imagePostUrl + "/platforms/facebook/official", null, 403, role: "Customer");
    await Call("M7InvalidPlatform", HttpMethod.Get, imagePostUrl + "/platforms/tiktok/official", null, 422, imageVersionUser);
    await Call("M6MissingPlatform", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", new { images = Array.Empty<object>() }, 404, imageVersionUser);
    var initialImageDetail = await Call("M6GenerateSharedImages", HttpMethod.Post, imagePostUrl + "/generate", new { platforms = new[] { "facebook", "instagram" }.Select(p => new { platform = p, ratios = new[] { new { ratioCode = "Square1x1", basePostImageIds = versionSourceIds } } }) }, user: imageVersionUser);
    JsonElement PlatformValue(JsonElement detailValue, string name) => detailValue.GetProperty("platforms").EnumerateArray().Single(x => x.GetProperty("platform").GetString() == name);
    var initialValue = initialImageDetail.GetProperty("value");
    var fbImages = PlatformValue(initialValue, "facebook").GetProperty("images").EnumerateArray().ToArray();
    var firstImageId = fbImages[0].GetProperty("selectedGeneratedPostImageId").GetGuid();
    var secondImageId = fbImages[1].GetProperty("selectedGeneratedPostImageId").GetGuid();
    var regenImageRequest = new { sourceBasePostImageId = versionSourceIds[0], ratioCode = "Square1x1", sourceGeneratedPostImageId = firstImageId, feedback = "Nền sáng hơn" };
    await Call("M6RegenerateCustomerDenied", HttpMethod.Post, imageHistoryUrl + "/regenerate", regenImageRequest, 403, role: "Customer");
    await Call("M6SelectCustomerDenied", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", new { images = Array.Empty<object>() }, 403, role: "Customer");
    var regeneratedImage = await Call("M6RegenerateImage", HttpMethod.Post, imageHistoryUrl + "/regenerate", regenImageRequest, user: imageVersionUser);
    var imageV2 = regeneratedImage.GetProperty("value"); var imageV2Id = imageV2.GetProperty("generatedPostImageId").GetGuid();
    Check(imageV2.GetProperty("versionNo").GetInt32() == 2 && imageV2.GetProperty("parentGeneratedPostImageId").GetGuid() == firstImageId, "Image v2 pins v1 and has stable pair number");
    var sentParts = aiHandler.ImageInputs.Last().GetProperty("messages")[0].GetProperty("content");
    Check(sentParts[1].GetProperty("image_url").GetProperty("url").GetString() == imageSources[0].GetProperty("imageUrl").GetString()
        && sentParts[3].GetProperty("image_url").GetProperty("url").GetString() == fbImages[0].GetProperty("imageUrl").GetString(), "Provider gets original reference then selected revision in order");
    var afterImageRegen = await Call("M6DetailAfterRegeneration", HttpMethod.Get, imagePostUrl, null, user: imageVersionUser);
    Check(initialValue.GetRawText() == afterImageRegen.GetProperty("value").GetRawText(), "Regenerating a shared image leaves full detail unchanged");
    var newSelection = new { images = new[] {
        new { sourceBasePostImageId = versionSourceIds[1], ratioCode = "Square1x1", selectedGeneratedPostImageId = secondImageId, sortOrder = 1 },
        new { sourceBasePostImageId = versionSourceIds[0], ratioCode = "Square1x1", selectedGeneratedPostImageId = imageV2Id, sortOrder = 2 } } };
    var selectionCalls = aiHandler.Calls;
    var chosenImages = await Call("M6SelectFacebookV2AndReorder", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", newSelection, user: imageVersionUser);
    var chosenValue = chosenImages.GetProperty("value");
    Check(PlatformValue(chosenValue, "facebook").GetProperty("status").GetString() == "Ready", "Content plus selected images becomes Ready");
    Check(PlatformValue(chosenValue, "facebook").GetProperty("images")[1].GetProperty("selectedGeneratedPostImageId").GetGuid() == imageV2Id, "Facebook selects v2 at requested order");
    Check(PlatformValue(chosenValue, "instagram").GetRawText() == PlatformValue(initialValue, "instagram").GetRawText(), "Instagram retains v1 and original state");
    Check(PlatformValue(chosenValue, "facebook").GetProperty("content").GetRawText() == PlatformValue(initialValue, "facebook").GetProperty("content").GetRawText(), "Selecting images preserves official content");
    var officialUrl = imagePostUrl + "/platforms/facebook/official";
    var officialReady = await Call("M7OfficialReady", HttpMethod.Get, officialUrl, null, user: imageVersionUser);
    var officialValue = officialReady.GetProperty("value");
    Check(officialValue.GetProperty("isReady").GetBoolean() && officialValue.GetProperty("status").GetString() == "Ready"
        && officialValue.GetProperty("notReadyReasons").GetArrayLength() == 0, "Official selected payload is Ready");
    var chosenContent = PlatformValue(chosenValue, "facebook").GetProperty("content");
    foreach (var key in new[] { "title", "content", "hashtag", "generationId" })
        Check(officialValue.GetProperty(key).GetRawText() == chosenContent.GetProperty(key).GetRawText(), "Official carries exact selected " + key);
    Check(officialValue.GetProperty("images").GetRawText() == PlatformValue(chosenValue, "facebook").GetProperty("images").GetRawText(), "Official images match selected IDs, URLs, pixels and order");
    var officialAgain = await Call("M7OfficialReadAgain", HttpMethod.Get, officialUrl, null, user: imageVersionUser, role: "Staff");
    Check(officialValue.GetRawText() == officialAgain.GetProperty("value").GetRawText(), "Official read is stable across accounts");
    Check(!officialValue.TryGetProperty("versions", out _) && !officialValue.TryGetProperty("baseImages", out _), "Official excludes history and source-image collections");
    var imageHistory = await Call("M6ReadHistory", HttpMethod.Get, imageHistoryUrl, null, user: imageVersionUser);
    var imageHistoryAgain = await Call("M6ReadHistoryAgain", HttpMethod.Get, imageHistoryUrl, null, user: imageVersionUser);
    Check(imageHistory.GetProperty("value").GetRawText() == imageHistoryAgain.GetProperty("value").GetRawText(), "Image history order and numbers are stable");
    Check(aiHandler.Calls == selectionCalls, "Selecting/reordering/history do not call AI");
    await Call("M6ForeignSelectedImage", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", new { images = new[] { new { sourceBasePostImageId = versionSourceIds[0], ratioCode = "Square1x1", selectedGeneratedPostImageId = Guid.NewGuid(), sortOrder = 1 } } }, 400, imageVersionUser);
    await Call("M6WrongSlot", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", new { images = new[] { new { sourceBasePostImageId = versionSourceIds[1], ratioCode = "Square1x1", selectedGeneratedPostImageId = imageV2Id, sortOrder = 1 } } }, 400, imageVersionUser);
    await Call("M6DuplicateOrder", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", new { images = newSelection.images.Select(x => new { x.sourceBasePostImageId, x.ratioCode, x.selectedGeneratedPostImageId, sortOrder = 1 }) }, 400, imageVersionUser);
    await Call("M6MissingImages", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", new {}, 422, imageVersionUser);
    await Call("M6InvalidRatio", HttpMethod.Post, imageHistoryUrl + "/regenerate", new { sourceBasePostImageId = versionSourceIds[0], ratioCode = 999 }, 422, imageVersionUser);
    aiHandler.FailAllImages = true;
    var failedImage = await Call("M6FailedRegenerationRetained", HttpMethod.Post, imageHistoryUrl + "/regenerate", regenImageRequest, user: imageVersionUser);
    var failedImageId = failedImage.GetProperty("value").GetProperty("generatedPostImageId").GetGuid();
    Check(failedImage.GetProperty("value").GetProperty("status").GetString() == "Failed" && failedImage.GetProperty("value").GetProperty("imageUrl").ValueKind == JsonValueKind.Null, "Failed image attempt is explicit in 200 response");
    aiHandler.FailAllImages = false;
    await Call("M6FailedParentRejected", HttpMethod.Post, imageHistoryUrl + "/regenerate", new { sourceBasePostImageId = versionSourceIds[0], ratioCode = "Square1x1", sourceGeneratedPostImageId = failedImageId }, 400, imageVersionUser);
    await Call("M6FailedSelectionRejected", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", new { images = new[] { new { sourceBasePostImageId = versionSourceIds[0], ratioCode = "Square1x1", selectedGeneratedPostImageId = failedImageId, sortOrder = 1 } } }, 400, imageVersionUser);
    var reuseCalls = aiHandler.Calls;
    await Call("M6ReuseV2ForInstagram", HttpMethod.Put, imagePostUrl + "/platforms/instagram/images", newSelection, user: imageVersionUser);
    Check(aiHandler.Calls == reuseCalls, "Instagram can reuse existing versions without generation");
    var clearedImages = await Call("M6ClearFacebookImages", HttpMethod.Put, imagePostUrl + "/platforms/facebook/images", new { images = Array.Empty<object>() }, user: imageVersionUser);
    Check(PlatformValue(clearedImages.GetProperty("value"), "facebook").GetProperty("images").GetArrayLength() == 0
        && PlatformValue(clearedImages.GetProperty("value"), "facebook").GetProperty("status").GetString() == "Generated", "Clearing images removes Ready state");
    var officialNoImages = await Call("M7OfficialMissingImages", HttpMethod.Get, officialUrl, null, user: imageVersionUser);
    Check(!officialNoImages.GetProperty("value").GetProperty("isReady").GetBoolean()
        && officialNoImages.GetProperty("value").GetProperty("notReadyReasons")[0].GetString() == "MissingImages"
        && officialNoImages.GetProperty("value").GetProperty("status").GetString() == "Generated", "Official incomplete selection returns 200 with MissingImages");
    await Call("M6RemoveSources", HttpMethod.Put, imagePostUrl, new { title = "Keep text", content = "Keep text", images = Array.Empty<object>() }, user: imageVersionUser);
    var removedSourceHistory = await Call("M6HistoryRetainsRemovedSources", HttpMethod.Get, imageHistoryUrl, null, user: imageVersionUser);
    Check(removedSourceHistory.GetProperty("value").GetProperty("groups").EnumerateArray().All(x => x.GetProperty("sourceIsDeleted").GetBoolean()), "Removed sources keep history but are marked unavailable");
    await Call("M6RemovedSourceRejected", HttpMethod.Post, imageHistoryUrl + "/regenerate", regenImageRequest, 400, imageVersionUser);
    await Call("M6DeletePost", HttpMethod.Delete, imagePostUrl, null, user: imageVersionUser);
    await Call("M6DeletedHistory", HttpMethod.Get, imageHistoryUrl, null, 404, imageVersionUser);
    Check(await db.GeneratedPostImages.CountAsync(x => x.BasePostId == imagePostId) == 4, "M6 stores three succeeded images and one failed attempt");
    Check(await db.GeneratedPostImages.CountAsync(x => x.BasePostId == imagePostId && x.ParentGeneratedPostImageId == firstImageId) == 2, "Success and failure both preserve parent lineage");
    Check(await db.GeneratedPosts.CountAsync(x => db.PlatformPosts.Any(p => p.Id == x.PlatformPostId && p.BasePostId == imagePostId)) == 2, "M6 does not regenerate content");
    await Call("M7DeletedPost", HttpMethod.Get, officialUrl, null, 404, imageVersionUser);
    var imageOnlyUser = Guid.NewGuid();
    var imageOnlyPost = await Call("M7CreateImageOnlyPost", HttpMethod.Post, "/api/v1/posts", input, user: imageOnlyUser);
    var imageOnlyId = imageOnlyPost.GetProperty("value").GetProperty("basePostId").GetGuid();
    var imageOnlyUrl = "/api/v1/posts/" + imageOnlyId;
    var imageOnlySource = imageOnlyPost.GetProperty("value").GetProperty("baseImages")[0].GetProperty("id").GetGuid();
    await Call("M7GenerateImagesWithoutContent", HttpMethod.Post, imageOnlyUrl + "/generate", new { mode = "images", platforms = new[] { new { platform = "facebook", ratios = new[] { new { ratioCode = "Square1x1", basePostImageIds = new[] { imageOnlySource } } } } } }, user: imageOnlyUser);
    var officialCalls = aiHandler.Calls;
    var withoutContent = await Call("M7OfficialMissingContent", HttpMethod.Get, imageOnlyUrl + "/platforms/facebook/official", null, user: imageOnlyUser);
    Check(withoutContent.GetProperty("value").GetProperty("content").ValueKind == JsonValueKind.Null
        && withoutContent.GetProperty("value").GetProperty("generationId").ValueKind == JsonValueKind.Null
        && !withoutContent.GetProperty("value").GetProperty("isReady").GetBoolean()
        && withoutContent.GetProperty("value").GetProperty("notReadyReasons")[0].GetString() == "MissingContent", "Official with only images reports missing content, never falls back to core");
    Check(aiHandler.Calls == officialCalls, "Official GET does not call AI");
    await Call("M7DeleteImageOnlyPost", HttpMethod.Delete, imageOnlyUrl, null, user: imageOnlyUser);
    Check(!await db.BasePosts.AnyAsync(x => !x.IsDeleted), "All HTTP fixture posts are soft deleted");
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
    private int calls;
    private int imageCalls;
    public int Calls => calls;
    public int ImageCalls => imageCalls;
    public int FailImageNumber { get; set; }
    public bool FailAllImages { get; set; }
    public int FailOnCall { get; set; }
    public List<string> Prompts { get; } = new();
    public List<JsonElement> ContentInputs { get; } = new();
    public List<JsonElement> ImageInputs { get; } = new();
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var call = Interlocked.Increment(ref calls);
        using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
        var messages = json.RootElement.GetProperty("messages");
        if (messages.EnumerateArray().Any(m => m.GetProperty("content").EnumerateArray().Any(p => p.GetProperty("type").GetString() == "image_url")))
        {
            lock (ImageInputs) ImageInputs.Add(json.RootElement.Clone());
            var imageCall = Interlocked.Increment(ref imageCalls);
            if (FailAllImages || imageCall == FailImageNumber) return new(HttpStatusCode.InternalServerError);
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new { id = "chatcmpl-image-" + imageCall,
                model = "chatgpt/gpt-5.6", choices = new[] { new { index = 0, finish_reason = "stop", message = new { role = "assistant",
                content = $"![generated image](https://example.com/generated-{imageCall}.png)" } } } }) };
        }
        Prompts.Add(messages[0].GetProperty("content")[0].GetProperty("text").GetString()!);
        using var contentInput = JsonDocument.Parse(messages[1].GetProperty("content")[0].GetProperty("text").GetString()!);
        ContentInputs.Add(contentInput.RootElement.Clone());
        if (call == FailOnCall) return new(HttpStatusCode.InternalServerError);
        return new(HttpStatusCode.OK) { Content = JsonContent.Create(new { id = "chatcmpl-probe-" + Calls,
            model = "chatgpt/gpt-5.6", choices = new[] { new { index = 0, finish_reason = "stop", message = new { role = "assistant",
            content = JsonSerializer.Serialize(new { title = "Bó hoa Cảm Ơn", content = "Nội dung AI kiểm thử", hashtag = "#HoaTheoMua" }) } } } }) };
    }
}

using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Controllers;
using HoaTheoMua.Extentions;
using HoaTheoMua.Middleware;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.DependencyInjection.Extensions;
using HoaTheoMua.Service.FlowerJobs;
using HoaTheoMua.Service.Test.TestSupport;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Npgsql;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;
using Flower = HoaTheoMua.Service.Flower;
using Jobs = HoaTheoMua.Service.FlowerJobs;

namespace HoaTheoMua.Service.Test.FlowerTest;

public class FlowerJobIntegrationFact : FactAttribute
{
    public FlowerJobIntegrationFact()
    {
        if (Environment.GetEnvironmentVariable("FLOWER_JOBS_INTEGRATION") != "1")
            Skip = "Set FLOWER_JOBS_INTEGRATION=1 and local POSTGEN_TEST_CONNECTION_STRING/RabbitMQ to run.";
    }
}

public class FlowerJobsIntegration
{
    [FlowerJobIntegrationFact]
    public async Task HttpOutboxRabbitWorkerPollingCancellationAndConcurrentIdempotency()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "flower_jobs_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(settings.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        settings.Database = database;
        try
        {
            var ds = new NpgsqlDataSourceBuilder(settings.ConnectionString); ds.EnableDynamicJson();
            ds.MapEnum<PlatformType>("PlatformType"); ds.MapEnum<SystemPromptType>("SystemPromptType");
            ds.MapEnum<CardFormType>("CardFormType"); ds.MapEnum<ClientHistoryType>("ClientHistoryType"); ds.MapEnum<FlowerReferenceType>("FlowerReferenceType");
            await using var source = ds.Build();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options;
            await using (var migrate = new AppDbContext(options)) await migrate.Database.MigrateAsync();
            using var fixture = new FlowerGeneration.Fixture(options); FlowerJobsTests.ConfigureAi(fixture);
            var delaySeconds = int.TryParse(Environment.GetEnvironmentVariable("FLOWER_JOB_TEST_DELAY_SECONDS"), out var seconds) ? seconds : 4;
            var calls = 0;
            fixture.Ai.Setup(x => x.ApplyFlowerMockup(It.IsAny<Ai.Request.FlowerImageInput>(), It.IsAny<Ai.Response.GeneratedImage>(), It.IsAny<CancellationToken>()))
                .Returns<Ai.Request.FlowerImageInput, Ai.Response.GeneratedImage, CancellationToken>(async (_, _, ct) =>
                {
                    Interlocked.Increment(ref calls);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
                    return new("https://example.com/mockup.png", "test", "mockup", delaySeconds * 1000);
                });
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            const string key = "flower-jobs-local-test-signing-key-never-use-in-production-123456789";
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtOption:Issuer"] = "flower-test", ["JwtOption:Audience"] = "flower-test", ["JwtOption:AccessTokenKey"] = key,
                ["RabbitMqOptions:Host"] = "127.0.0.1", ["RabbitMqOptions:Port"] = "5679", ["RabbitMqOptions:VirtualHost"] = "/",
                ["RabbitMqOptions:Username"] = "guest", ["RabbitMqOptions:Password"] = "guest"
            });
            builder.Services.AddDbContext<AppDbContext>(b => b.UseNpgsql(source));
            builder.Services.AddControllers().AddApplicationPart(typeof(FlowerJobsController).Assembly);
            builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
            builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddJwtServices(builder.Configuration);
            builder.Services.AddSingleton(TimeProvider.System); builder.Services.Configure<FlowerJobOptions>(_ => { });
            builder.Services.AddSingleton(fixture.Ai.Object); builder.Services.AddSingleton(fixture.Branding.Object);
            builder.Services.AddScoped<Flower.Service>(sp =>
            {
                var db = sp.GetRequiredService<AppDbContext>(); var context = sp.GetRequiredService<IHttpContextAccessor>();
                return new(db, sp.GetRequiredService<ICurrentUserService>(), context, fixture.Ai.Object,
                    new HoaTheoMua.Service.Product.Service(db, context, new HttpClient(), builder.Configuration, MapperFactory.Create()), fixture.Branding.Object);
            });
            builder.Services.AddScoped<Jobs.Service>(); builder.Services.AddScoped<Store>(); builder.Services.AddScoped<Worker>();
            builder.Services.AddScoped<Recovery>();
            // Production queue registration (including exclusion from transaction-wrapped consumer retry).
            builder.Services.AddMasstransitRabbitMqInfrastructure(builder.Configuration);
            builder.Services.AddScoped<HoaTheoMua.Service.Fulfillment.IService>(_ => throw new InvalidOperationException("Unexpected fulfillment"));
            builder.Services.AddScoped<HoaTheoMua.Service.XanhSm.XanhSmDeliveryCompletionService>(_ => throw new InvalidOperationException("Unexpected delivery"));
            builder.Services.AddScoped<HoaTheoMua.Service.PostPublishing.Worker>(_ => throw new InvalidOperationException("Unexpected publication"));
            await using var app = builder.Build();
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
            await app.StartAsync();
            try
            {
                using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()), Timeout = TimeSpan.FromSeconds(10) };
                var actor = Guid.NewGuid();
                string Token(Guid user) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("flower-test", "flower-test",
                    [new Claim("UserId", user.ToString()), new Claim(ClaimTypes.Role, "Customer"), new Claim("IsVerified", "true")],
                    expires: DateTime.UtcNow.AddMinutes(15), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(actor));
                async Task<JobResponse> Post(string idempotencyKey)
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai-flower-jobs") { Content = JsonContent.Create(fixture.Request) };
                    request.Headers.Add("Idempotency-Key", idempotencyKey);
                    using var response = await client.SendAsync(request);
                    var json = await response.Content.ReadAsStringAsync();
                    Assert.True(response.StatusCode == HttpStatusCode.Accepted, json);
                    return JobJson.Read<JobResponse>(JsonDocument.Parse(json).RootElement.GetProperty("value").GetRawText());
                }
                async Task<JobResponse> Get(Guid id)
                {
                    using var response = await client.GetAsync($"/api/v1/ai-flower-jobs/{id}"); response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                    return JobJson.Read<JobResponse>(json.GetProperty("value").GetRawText());
                }
                var watch = Stopwatch.StartNew();
                var submissions = await Task.WhenAll(Post("concurrent-key"), Post("concurrent-key"));
                Assert.Equal(submissions[0].JobId, submissions[1].JobId); Assert.True(watch.Elapsed < TimeSpan.FromSeconds(10));
                var id = submissions[0].JobId;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(Guid.NewGuid()));
                Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/ai-flower-jobs/{id}")).StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/api/v1/ai-flower-jobs/{id}/cancel", null)).StatusCode);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(actor));
                var active = await client.GetFromJsonAsync<JsonElement>("/api/v1/ai-flower-jobs?active=true");
                Assert.Contains(active.GetProperty("value").GetProperty("items").EnumerateArray(), item => item.GetProperty("jobId").GetGuid() == id);
                JobResponse state;
                var deadline = DateTimeOffset.UtcNow.AddSeconds(delaySeconds + 45);
                do
                {
                    await Task.Delay(500); state = await Get(id);
                } while (state.Status is "Queued" or "Running" && DateTimeOffset.UtcNow < deadline);
                Assert.Equal("Succeeded", state.Status); Assert.NotNull(state.Result); Assert.Equal(1, calls);
                Assert.True(watch.Elapsed.TotalSeconds >= delaySeconds);
                // Duplicate delivery after commit cannot repeat AI or result persistence.
                await app.Services.GetRequiredService<IBus>().Publish(new FlowerGenerationRequested(id));
                await Task.Delay(500);
                await using (var check = new AppDbContext(options))
                {
                    Assert.Equal(1, await check.GeneratedFlowers.CountAsync()); Assert.Equal(1, await check.ClientHistories.CountAsync());
                    Assert.False(check.Database.HasPendingModelChanges());
                }
                Assert.Equal(1, calls);
                var cancelJob = await Post("cancel-running");
                deadline = DateTimeOffset.UtcNow.AddSeconds(15);
                do { await Task.Delay(200); state = await Get(cancelJob.JobId); }
                while (state.Status == "Queued" && DateTimeOffset.UtcNow < deadline);
                using var canceled = await client.PostAsync($"/api/v1/ai-flower-jobs/{cancelJob.JobId}/cancel", null); canceled.EnsureSuccessStatusCode();
                deadline = DateTimeOffset.UtcNow.AddSeconds(15);
                do { await Task.Delay(250); state = await Get(cancelJob.JobId); }
                while (state.Status is "Queued" or "Running" && DateTimeOffset.UtcNow < deadline);
                Assert.Equal("Canceled", state.Status);
                await using var final = new AppDbContext(options); Assert.Equal(1, await final.GeneratedFlowers.CountAsync());
            }
            finally { await app.StopAsync(); }
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
        }
    }
}

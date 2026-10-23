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
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.PostImport;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace HoaTheoMua.Service.Test.PostImportTest;

public class Api
{
    [Fact]
    public async Task MultipartContractAuthorizationReplayExportAndActorIsolation()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("bulk-import-api-test-signing-key-only"));
        var actor = Guid.NewGuid();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
        builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
        var database = Guid.NewGuid().ToString();
        builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(database));
        builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IOptions<PublishingOptions>>(Options.Create(new PublishingOptions()));
        builder.Services.AddScoped<WorkbookReader>();
        builder.Services.AddScoped<HoaTheoMua.Service.PostImport.IService, HoaTheoMua.Service.PostImport.Service>();
        builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
        builder.Services.AddControllers().AddApplicationPart(typeof(PostImportController).Assembly);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.TokenValidationParameters = new() { ValidateIssuer = false, ValidateAudience = false,
                ValidateLifetime = true, ValidateIssuerSigningKey = true, IssuerSigningKey = key, RoleClaimType = "role" };
        });
        builder.Services.AddAuthorization(o => o.AddPolicy(JwtExtensions.AdminPolicy, p => p.RequireRole("Admin")));
        await using var app = builder.Build();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
            const string route = "/api/v1/post-imports";
            using var anonymous = await client.GetAsync(route); Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
            void Login(string role, Guid user) => client.DefaultRequestHeaders.Authorization = new("Bearer", new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(claims: [new Claim("role", role), new Claim("UserId", user.ToString())], expires: DateTime.UtcNow.AddMinutes(5),
                    signingCredentials: new(key, SecurityAlgorithms.HmacSha256))));
            Login("User", actor);
            using var forbidden = await client.GetAsync(route); Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
            Login("Admin", actor);
            using var missing = await client.PostAsync(route, new MultipartFormDataContent()); Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
            var bytes = await Imports.Book();
            async Task<Guid> Upload()
            {
                using var form = new MultipartFormDataContent();
                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                form.Add(content, "file", "posts.xlsx");
                using var request = new HttpRequestMessage(HttpMethod.Post, route) { Content = form };
                request.Headers.Add("Idempotency-Key", "http-replay");
                using var response = await client.SendAsync(request);
                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var id = json.RootElement.GetProperty("value").GetProperty("id").GetGuid();
                Assert.Equal(route + "/" + id, response.Headers.Location!.ToString());
                return id;
            }
            var id = await Upload(); Assert.Equal(id, await Upload());
            using var detail = await client.GetAsync(route + "/" + id); Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
            using var data = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
            Assert.Equal(2, data.RootElement.GetProperty("value").GetProperty("rows")[0].GetProperty("rowNumber").GetInt32());
            using var export = await client.GetAsync(route + "/" + id + "/export"); Assert.Equal(HttpStatusCode.OK, export.StatusCode);
            using var archive = new System.IO.Compression.ZipArchive(new MemoryStream(await export.Content.ReadAsByteArrayAsync()));
            Assert.NotNull(archive.GetEntry("xl/workbook.xml"));
            Login("Admin", Guid.NewGuid());
            using var other = await client.GetAsync(route + "/" + id); Assert.Equal(HttpStatusCode.NotFound, other.StatusCode);
            using var otherExport = await client.GetAsync(route + "/" + id + "/export"); Assert.Equal(HttpStatusCode.NotFound, otherExport.StatusCode);
        }
        finally { await app.StopAsync(); }
    }
}

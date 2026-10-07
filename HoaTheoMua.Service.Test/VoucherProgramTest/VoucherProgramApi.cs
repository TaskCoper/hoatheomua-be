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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using VoucherProgramFeature = HoaTheoMua.Service.VoucherProgram;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class VoucherProgramApi
{
    [Fact]
    public async Task AdminEndpoints_EnforceAuthorization_ValidateRequests_AndReturnPlanEnvelopes()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("voucher-program-api-test-signing-key-only"));
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        builder.Services.AddScoped<VoucherProgramFeature.IService, VoucherProgramFeature.Service>();
        builder.Services.AddValidatorsFromAssembly(typeof(VoucherProgramFeature.Service).Assembly);
        builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
        builder.Services.AddControllers(options => options.Filters.Add<ValidationActionFilter>())
            .AddApplicationPart(typeof(VoucherProgramController).Assembly)
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, ValidateAudience = false,
                ValidateLifetime = true, ValidateIssuerSigningKey = true,
                IssuerSigningKey = key, RoleClaimType = "role"
            };
        });
        builder.Services.AddAuthorization(options => options.AddPolicy(JwtExtensions.AdminPolicy, policy => policy.RequireRole("Admin")));

        await using var app = builder.Build();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        try
        {
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            const string route = "/api/v1/admin/voucher-programs";
            var unknownRoute = route + "/" + Guid.NewGuid();
            var batchRequest = VoucherProgramTestData.GenerateRequest(3);
            var request = new VoucherProgramFeature.Request.CreateVoucherProgramRequest
            {
                Name = "20/10",
                StartAt = DateTimeOffset.Parse("2026-10-15T00:00:00+07:00"),
                EndAt = DateTimeOffset.Parse("2026-10-31T23:59:59+07:00"),
                MinOrderAmount = 200000
            };

            using var anonymousPost = await client.PostAsJsonAsync(route, request);
            using var anonymousGet = await client.GetAsync(route);
            Assert.Equal(HttpStatusCode.Unauthorized, anonymousPost.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, anonymousGet.StatusCode);
            await AssertMutationAuthorization(client, unknownRoute, batchRequest, HttpStatusCode.Unauthorized);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token("Customer", key));
            using var customerPost = await client.PostAsJsonAsync(route, request);
            using var customerGet = await client.GetAsync(route);
            Assert.Equal(HttpStatusCode.Forbidden, customerPost.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, customerGet.StatusCode);
            await AssertMutationAuthorization(client, unknownRoute, batchRequest, HttpStatusCode.Forbidden);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token("Admin", key));
            request.EndAt = request.StartAt;
            using var invalidPost = await client.PostAsJsonAsync(route, request);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidPost.StatusCode);
            using var error = JsonDocument.Parse(await invalidPost.Content.ReadAsStringAsync());
            Assert.Equal("VALIDATION_ERROR", error.RootElement.GetProperty("messageCode").GetString());
            var validationError = Assert.Single(error.RootElement.GetProperty("errors").EnumerateArray());
            Assert.Equal("endAt", validationError.GetProperty("field").GetString());
            Assert.Equal("End time must be later than start time", validationError.GetProperty("message").GetString());

            request.EndAt = DateTimeOffset.Parse("2026-10-31T23:59:59+07:00");
            using var post = await client.PostAsJsonAsync(route, request);
            Assert.Equal(HttpStatusCode.OK, post.StatusCode);
            using var created = JsonDocument.Parse(await post.Content.ReadAsStringAsync());
            Assert.True(created.RootElement.GetProperty("isSuccess").GetBoolean());
            var program = created.RootElement.GetProperty("value");
            Assert.False(program.GetProperty("isActive").GetBoolean());
            Assert.Equal(TimeSpan.Zero, program.GetProperty("startAt").GetDateTimeOffset().Offset);
            Assert.Equal(0, program.GetProperty("voucherCount").GetInt32());
            Assert.Equal(0, program.GetProperty("usedVoucherCount").GetInt32());

            using var get = await client.GetAsync(route + "?keyword=20%2F10&isActive=false");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            using var listed = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
            var page = listed.RootElement.GetProperty("value");
            var item = Assert.Single(page.GetProperty("items").EnumerateArray());
            Assert.Equal(program.GetProperty("id").GetGuid(), item.GetProperty("id").GetGuid());
            Assert.Equal(1, page.GetProperty("totalCount").GetInt32());
            Assert.Equal(1, page.GetProperty("pageIndex").GetInt32());
            Assert.Equal(20, page.GetProperty("pageSize").GetInt32());

            var programRoute = route + "/" + program.GetProperty("id").GetGuid();
            using var emptyUpdate = await client.PutAsJsonAsync(programRoute, new { });
            Assert.Equal(HttpStatusCode.UnprocessableEntity, emptyUpdate.StatusCode);
            using var invalidUpdate = await client.PutAsJsonAsync(programRoute, new
            {
                endAt = request.StartAt, isActive = true
            });
            Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidUpdate.StatusCode);
            using var updateError = JsonDocument.Parse(await invalidUpdate.Content.ReadAsStringAsync());
            Assert.Equal("endAt", Assert.Single(updateError.RootElement.GetProperty("errors").EnumerateArray())
                .GetProperty("field").GetString());
            batchRequest.Quantity = 1001;
            using var invalidBatch = await client.PostAsJsonAsync(programRoute + "/vouchers", batchRequest);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidBatch.StatusCode);
            batchRequest.Quantity = 3;
            using var missingBatch = await client.PostAsJsonAsync(unknownRoute + "/vouchers", batchRequest);
            Assert.Equal(HttpStatusCode.NotFound, missingBatch.StatusCode);

            using var batch = await client.PostAsJsonAsync(programRoute + "/vouchers", batchRequest);
            Assert.Equal(HttpStatusCode.OK, batch.StatusCode);
            using var generated = JsonDocument.Parse(await batch.Content.ReadAsStringAsync());
            var batchValue = generated.RootElement.GetProperty("value");
            Assert.Equal(program.GetProperty("id").GetGuid(), batchValue.GetProperty("programId").GetGuid());
            Assert.Equal(3, batchValue.GetProperty("createdCount").GetInt32());
            Assert.Equal(3, batchValue.GetProperty("codes").GetArrayLength());
            Assert.True(generated.RootElement.GetProperty("isSuccess").GetBoolean());
            Assert.NotEmpty(generated.RootElement.GetProperty("traceId").GetString()!);

            using var activate = await client.PutAsJsonAsync(programRoute, new { name = "20/10 cập nhật", isActive = true });
            Assert.Equal(HttpStatusCode.OK, activate.StatusCode);
            using var activated = JsonDocument.Parse(await activate.Content.ReadAsStringAsync());
            Assert.True(activated.RootElement.GetProperty("value").GetProperty("isActive").GetBoolean());
            Assert.Equal("20/10 cập nhật", activated.RootElement.GetProperty("value").GetProperty("name").GetString());
            using var activeList = await client.GetAsync(route + "?isActive=true");
            using var activePage = JsonDocument.Parse(await activeList.Content.ReadAsStringAsync());
            Assert.Equal(3, Assert.Single(activePage.RootElement.GetProperty("value").GetProperty("items").EnumerateArray())
                .GetProperty("voucherCount").GetInt32());

            using var deactivate = await client.PutAsJsonAsync(programRoute, new { isActive = false });
            Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);
            using var deactivated = JsonDocument.Parse(await deactivate.Content.ReadAsStringAsync());
            Assert.False(deactivated.RootElement.GetProperty("value").GetProperty("isActive").GetBoolean());
            using var delete = await client.DeleteAsync(programRoute);
            Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
            using var deleted = JsonDocument.Parse(await delete.Content.ReadAsStringAsync());
            Assert.Equal("Program deleted", deleted.RootElement.GetProperty("value").GetString());
            await AssertMutationAuthorization(client, programRoute, batchRequest, HttpStatusCode.NotFound);
            using var finalList = await client.GetAsync(route);
            using var finalPage = JsonDocument.Parse(await finalList.Content.ReadAsStringAsync());
            Assert.Empty(finalPage.RootElement.GetProperty("value").GetProperty("items").EnumerateArray());
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static string Token(string role, SecurityKey key) => new JwtSecurityTokenHandler().WriteToken(
        new JwtSecurityToken(claims: [new Claim("role", role)], expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)));

    private static async Task AssertMutationAuthorization(HttpClient client, string route,
        VoucherProgramFeature.Request.GenerateProgramVouchersRequest request, HttpStatusCode expected)
    {
        using var active = await client.PutAsJsonAsync(route, new { isActive = true });
        using var delete = await client.DeleteAsync(route);
        using var batch = await client.PostAsJsonAsync(route + "/vouchers", request);
        Assert.Equal(expected, active.StatusCode);
        Assert.Equal(expected, delete.StatusCode);
        Assert.Equal(expected, batch.StatusCode);
    }
}

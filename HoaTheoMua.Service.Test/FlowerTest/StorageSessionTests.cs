using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Flower;
using HoaTheoMua.Service.Test.PostGenTest;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HoaTheoMua.Service.Test.FlowerTest;

public class StorageSessionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RefreshesExpiredSessionAndRelogsInIfRefreshRejected(bool rejectRefresh)
    {
        var logins = 0; var refreshes = 0; var presigns = 0;
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/login"))
            {
                logins++;
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
                Assert.Equal("storage@example.com", body.RootElement.GetProperty("email").GetString());
                Assert.Equal("test-only", body.RootElement.GetProperty("password").GetString());
                return Auth(logins == 1 ? "expired" : "fresh");
            }
            if (request.RequestUri.AbsolutePath.EndsWith("/refresh_token"))
            {
                refreshes++;
                Assert.Contains("refreshToken=refresh-only", request.Headers.GetValues("Cookie").Single());
                return rejectRefresh ? new(HttpStatusCode.Unauthorized) : Auth("fresh");
            }
            presigns++;
            if (presigns == 1) return new(HttpStatusCode.Unauthorized);
            Assert.Equal("fresh", request.Headers.Authorization!.Parameter);
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new
            { value = new { presignedUrl = "https://storage.example.com/bucket/logo.png?signature=test", contentType = "image/png", uploadMethod = "PUT" } }) };
        });
        using var session = Session(handler);
        await session.PresignPngAsync("general/image.png", default);
        await session.PresignPngAsync("general/image2.png", default);
        Assert.Equal(rejectRefresh ? 2 : 1, logins);
        Assert.Equal(1, refreshes);
        Assert.Equal(3, presigns);
    }

    [Fact]
    public async Task LoginFailureStopsBeforePresignAndDoesNotLeakProviderResponse()
    {
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        { Content = new StringContent("sensitive login body") }));
        using var session = Session(handler);
        var error = await Assert.ThrowsAsync<BadGatewayException>(() => session.PresignPngAsync("general/a.png", default));
        Assert.DoesNotContain("sensitive", error.ToString());
        Assert.Equal(1, handler.Calls);
    }

    private static HttpResponseMessage Auth(string token)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { value = new { } }) };
        response.Headers.TryAddWithoutValidation("Set-Cookie", $"accessToken={token}; Path=/; Secure; HttpOnly");
        response.Headers.TryAddWithoutValidation("Set-Cookie", "refreshToken=refresh-only; Path=/; Secure; HttpOnly");
        return response;
    }

    private static TaskcoperStorageSession Session(HttpMessageHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));
        return new(factory.Object, Options.Create(new FlowerImageOptions
        { StorageApiBase = "https://api.example.com", StorageEmail = "storage@example.com", StoragePassword = "test-only" }));
    }
}

using System.Net;
using System.Net.Http.Json;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Flower;
using HoaTheoMua.Service.Test.PostGenTest;
using Microsoft.Extensions.Options;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace HoaTheoMua.Service.Test.FlowerTest;

public class FlowerBrandingTests
{
    [Fact]
    public async Task LogoIsBottomRightWithMarginAndTransparencyAndSourceIsUnchanged()
    {
        using var canvas = new Image<Rgba32>(100, 100, new Rgba32(0, 0, 255));
        using var logo = new Image<Rgba32>(20, 10, new Rgba32(255, 0, 0, 128));
        logo[0, 0] = new Rgba32(0, 0, 0, 0);
        var original = Png(canvas);
        var copy = original.ToArray();
        var result = await FlowerLogoComposer.ComposeAsync(original, Png(logo), new());
        Assert.Equal(copy, original);
        Assert.Equal((77, 87, 20, 10), (result.LogoX, result.LogoY, result.LogoWidth, result.LogoHeight));
        using var output = Image.Load<Rgba32>(result.Png);
        Assert.Equal(100, output.Width); Assert.Equal(100, output.Height);
        Assert.Equal(new Rgba32(0, 0, 255), output[0, 0]);
        Assert.Equal(new Rgba32(0, 0, 255), output[99, 99]);
        Assert.Equal(new Rgba32(0, 0, 255), output[77, 87]);
        var mixed = output[85, 90];
        Assert.InRange(mixed.R, (byte)127, (byte)129);
        Assert.InRange(mixed.B, (byte)126, (byte)128);
        Assert.Equal(64, result.LogoSha256.Length);
    }

    [Theory]
    [InlineData(1000, 1000, 200, 100)]
    [InlineData(1500, 1000, 200, 100)]
    [InlineData(1000, 1500, 200, 100)]
    [InlineData(2000, 500, 100, 200)]
    [InlineData(500, 2000, 100, 200)]
    [InlineData(1, 1, 20, 10)]
    public async Task CardLogoFitsShortEdgeWithoutCroppingOrStretchingCanvas(int width, int height, int logoWidth, int logoHeight)
    {
        using var canvas = new Image<Rgba32>(width, height, new Rgba32(0, 0, 255));
        using var logo = new Image<Rgba32>(logoWidth, logoHeight, new Rgba32(255, 0, 0));
        var result = await FlowerLogoComposer.ComposeAsync(Png(canvas), Png(logo),
            new HoaTheoMua.Service.Card.CardImageOptions(), fitShortEdge: true);
        using var output = Image.Load<Rgba32>(result.Png);
        Assert.Equal((width, height), (output.Width, output.Height));
        var edge = Math.Min(width, height);
        var bound = Math.Max(1, (int)Math.Round(edge * 0.16));
        Assert.InRange(result.LogoWidth, 1, bound);
        Assert.InRange(result.LogoHeight, 1, bound);
        var margin = (int)Math.Round(edge * 0.03);
        Assert.Equal(margin, width - result.LogoX - result.LogoWidth);
        Assert.Equal(margin, height - result.LogoY - result.LogoHeight);
        Assert.InRange(result.LogoX, 0, width - 1);
        Assert.InRange(result.LogoY, 0, height - 1);
        if (edge > 1)
        {
            Assert.Equal(new Rgba32(0, 0, 255), output[width / 2, height / 2]);
            Assert.InRange(Math.Abs((double)result.LogoWidth / result.LogoHeight - (double)logoWidth / logoHeight), 0, 0.03);
        }
    }

    [Fact]
    public async Task RejectsInvalidImagesAndPixelOrUploadLimits()
    {
        using var source = new Image<Rgba32>(100, 100);
        var bytes = Png(source);
        await Assert.ThrowsAsync<BadGatewayException>(() => FlowerLogoComposer.ComposeAsync(new byte[] { 1, 2, 3 }, bytes, new()));
        await Assert.ThrowsAsync<BadGatewayException>(() => FlowerLogoComposer.ComposeAsync(bytes, bytes, new() { MaxImagePixels = 10 }));
        await Assert.ThrowsAsync<BadGatewayException>(() => FlowerLogoComposer.ComposeAsync(bytes, bytes, new() { MaxUploadBytes = 1 }));
    }

    [Fact]
    public async Task RealLogoAssetIsPackagedAndCanBeRendered()
    {
        using var canvas = new Image<Rgba32>(1024, 1024, new Rgba32(225, 234, 225));
        var logo = await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "public/logo.png"));
        var result = await FlowerLogoComposer.ComposeAsync(Png(canvas), logo, new());
        Assert.Equal(205, result.LogoWidth);
        Assert.Equal(31, result.Width - result.LogoX - result.LogoWidth);
        Assert.Equal(31, result.Height - result.LogoY - result.LogoHeight);
        var preview = Environment.GetEnvironmentVariable("FLOWER_LOGO_PREVIEW_PATH");
        if (!string.IsNullOrWhiteSpace(preview)) await File.WriteAllBytesAsync(preview, result.Png);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UsesAuthenticatedPresignThenRawPngPutAndReturnsDurablePublicUrl(bool cookieAuth)
    {
        using var canvas = new Image<Rgba32>(100, 100, new Rgba32(0, 0, 255));
        var requests = new List<string>();
        var handler = new AiContentClient.Handler(async (request, ct) =>
        {
            requests.Add(request.Method + " " + request.RequestUri!.AbsolutePath);
            if (request.RequestUri.Host == "images.example.com")
            {
                Assert.Null(request.Headers.Authorization);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Png(canvas)) };
            }
            if (request.RequestUri.AbsolutePath.EndsWith("/login"))
            {
                Assert.Null(request.Headers.Authorization);
                return Login(cookieAuth);
            }
            if (request.RequestUri.Host == "api.example.com")
            {
                Assert.Equal("test-token", request.Headers.Authorization!.Parameter);
                Assert.Contains("contentType=image%2Fpng", request.RequestUri.Query);
                return Presign();
            }
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Null(request.Headers.Authorization);
            Assert.False(request.Headers.Contains("x-amz-meta-owner"));
            Assert.Equal("image/png", request.Content!.Headers.ContentType!.MediaType);
            var bytes = await request.Content.ReadAsByteArrayAsync(ct);
            Assert.Equal(bytes.Length, request.Content.Headers.ContentLength);
            using var rendered = Image.Load<Rgba32>(bytes);
            Assert.Equal(100, rendered.Width);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var service = Client(handler, cookieAuth);
        var flowerId = Guid.NewGuid();
        var output = await service.ApplyLogoAsync("https://images.example.com/mockup.png", flowerId);
        Assert.Equal(new[] { "GET /mockup.png", "POST /api/v1/users/login", "GET /api/v1/storages/presigned-url", "PUT /bucket/upload.png" }, requests);
        Assert.StartsWith($"https://files.example.com/bucket/general/ai-flowers/{flowerId:N}/", output.ImageUrl);
        Assert.DoesNotContain("signature", output.ImageUrl);
        Assert.EndsWith(".png", output.ImageUrl);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task StopsOnDownloadPresignOrUploadFailureWithoutRetry(int failureAt)
    {
        using var image = new Image<Rgba32>(100, 100);
        var calls = 0;
        var handler = new AiContentClient.Handler((_, _) => Task.FromResult(++calls == failureAt
            ? new HttpResponseMessage(HttpStatusCode.BadGateway)
            : calls == 1 ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Png(image)) } : calls == 2 ? Login(false) : Presign()));
        await Assert.ThrowsAsync<BadGatewayException>(() => Client(handler).ApplyLogoAsync("https://images.example.com/a.png", Guid.NewGuid()));
        Assert.Equal(failureAt, calls);
    }

    [Theory]
    [InlineData("127.0.0.1", false)]
    [InlineData("10.0.0.1", false)]
    [InlineData("169.254.169.254", false)]
    [InlineData("::1", false)]
    [InlineData("::ffff:192.168.1.1", false)]
    [InlineData("fc00::1", false)]
    [InlineData("8.8.8.8", true)]
    [InlineData("2606:4700:4700::1111", true)]
    public void ImageConnectionsRejectPrivateAddresses(string address, bool allowed) =>
        Assert.Equal(allowed, PublicImageHttpHandler.IsPublicAddress(IPAddress.Parse(address)));

    private static HttpResponseMessage Presign() => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new { value = new { presignedUrl = "https://storage.example.com/bucket/upload.png?signature=secret",
            contentType = "image/png", uploadMethod = "PUT", requiredHeaders = new[] { "Content-Type: image/png", "x-amz-meta-owner: access_token" } } })
    };

    private static FlowerImageBranding Client(HttpMessageHandler handler, bool cookie = false)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler));
        var options = Options.Create(new FlowerImageOptions
        { StorageApiBase = "https://api.example.com", StorageEmail = "test@example.com", StoragePassword = "test-only",
          PublicBaseUrl = "https://files.example.com/bucket" });
        return new(factory.Object, options, new TaskcoperStorageSession(factory.Object, options));
    }

    private static HttpResponseMessage Login(bool cookie)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        { Content = JsonContent.Create(new { value = new { accessToken = cookie ? null : "test-token", refreshToken = cookie ? null : "refresh-only" } }) };
        if (cookie)
        {
            response.Headers.TryAddWithoutValidation("Set-Cookie", "accessToken=test-token; Path=/; Secure; HttpOnly");
            response.Headers.TryAddWithoutValidation("Set-Cookie", "refreshToken=refresh-only; Path=/; Secure; HttpOnly");
        }
        return response;
    }

    private static byte[] Png(Image<Rgba32> image)
    {
        using var stream = new MemoryStream(); image.SaveAsPng(stream); return stream.ToArray();
    }
}

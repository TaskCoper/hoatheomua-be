using System.Net.Http.Headers;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.Flower;

public class FlowerImageBranding(IHttpClientFactory clients, IOptions<FlowerImageOptions> settings,
    ITaskcoperStorageSession storage) : IFlowerImageBranding
{
    public const string SourceClient = "FlowerImageSource";
    public const string StorageClient = "FlowerImageStorage";
    private FlowerImageOptions Options => settings.Value;

    public void ValidateConfiguration()
    {
        storage.ValidateConfiguration();
        if ((!string.IsNullOrWhiteSpace(Options.PublicBaseUrl)
                && (!TryHttpUri(Options.PublicBaseUrl, out var publicBase) || publicBase.Scheme != "https"
                    || !string.IsNullOrEmpty(publicBase.Query)))
            || Options.TimeoutSeconds is < 1 or > 180
            || Options.MaxDownloadBytes is < 1 or > 50 * 1024 * 1024
            || Options.MaxUploadBytes is < 1 or > 5 * 1024 * 1024
            || Options.MaxImagePixels is < 1 or > 40_000_000
            || !double.IsFinite(Options.LogoWidthRatio) || Options.LogoWidthRatio is <= 0 or > 0.5
            || !double.IsFinite(Options.MarginRatio) || Options.MarginRatio is < 0 or > 0.2
            || string.IsNullOrWhiteSpace(Options.ObjectPrefix)
            || Options.ObjectPrefix.Split('/').Any(part => part.Length == 0 || part is "." or ".."
                || part.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_')))
            || string.IsNullOrWhiteSpace(Options.LogoPath) || !File.Exists(LogoPath()))
            throw new ServerException("Flower image storage/logo configuration is invalid");
    }

    public async Task<BrandedFlowerImage> ApplyLogoAsync(string mockupImageUrl, Guid flowerId,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        if (flowerId == Guid.Empty || !TryHttpUri(mockupImageUrl, out var sourceUri))
            throw new BadGatewayException("Mockup image URL is invalid");
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(Options.TimeoutSeconds));
        try
        {
            byte[] mockup;
            using (var request = new HttpRequestMessage(HttpMethod.Get, sourceUri))
            using (var response = await clients.CreateClient(SourceClient).SendAsync(request,
                       HttpCompletionOption.ResponseHeadersRead, budget.Token))
            {
                if (!response.IsSuccessStatusCode) throw StorageError();
                mockup = await ReadBounded(response.Content, Options.MaxDownloadBytes, budget.Token);
            }
            var logo = await File.ReadAllBytesAsync(LogoPath(), budget.Token);
            var rendered = await FlowerLogoComposer.ComposeAsync(mockup, logo, Options, budget.Token);
            // A fresh key per attempt avoids overwriting a prior image if a caller retries.
            var key = $"{Options.ObjectPrefix}/{flowerId:N}/{Guid.NewGuid():N}.png";
            var uploadUrl = await storage.PresignPngAsync(key, budget.Token);
            var publicUrl = string.IsNullOrWhiteSpace(Options.PublicBaseUrl)
                ? new Uri(uploadUrl).GetLeftPart(UriPartial.Path)
                : $"{Options.PublicBaseUrl.TrimEnd('/')}/{key}";
            if (publicUrl.Length > 2048) throw new ServerException("Flower image storage URL is too long");
            using (var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl))
            {
                request.Content = new ByteArrayContent(rendered.Png);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                request.Content.Headers.ContentLength = rendered.Png.Length;
                // Public-only transport, no Authorization, cookies or redirect forwarding.
                using var response = await clients.CreateClient(SourceClient).SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, budget.Token);
                if (!response.IsSuccessStatusCode) throw StorageError();
            }
            return new(publicUrl, key, rendered.LogoSha256, rendered.Width, rendered.Height,
                rendered.LogoX, rendered.LogoY, rendered.LogoWidth, rendered.LogoHeight);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { throw StorageError(); }
        catch (Exception ex) when (ex is HttpRequestException or IOException or JsonException
            or InvalidOperationException or KeyNotFoundException) { throw StorageError(); }
    }

    private string LogoPath() => Path.IsPathRooted(Options.LogoPath)
        ? Options.LogoPath : Path.Combine(AppContext.BaseDirectory, Options.LogoPath);

    private static bool TryHttpUri(string? url, out Uri uri) => Uri.TryCreate(url, UriKind.Absolute, out uri!)
        && uri.Scheme is "http" or "https" && string.IsNullOrEmpty(uri.UserInfo)
        && string.IsNullOrEmpty(uri.Fragment) && uri.Port is 80 or 443;

    private static async Task<byte[]> ReadBounded(HttpContent content, int maximum, CancellationToken token)
    {
        if (content.Headers.ContentLength > maximum) throw StorageError();
        await using var input = await content.ReadAsStreamAsync(token);
        using var output = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = await input.ReadAsync(buffer, token)) != 0)
        {
            if (output.Length + read > maximum) throw StorageError();
            output.Write(buffer, 0, read);
        }
        return output.ToArray();
    }

    private static BadGatewayException StorageError() => new("Flower image download or upload failed");
}

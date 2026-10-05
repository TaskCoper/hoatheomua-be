namespace HoaTheoMua.Service.Flower;

public class FlowerImageOptions
{
    public string StorageApiBase { get; set; } = "https://hoatheomua-taskcoper-api.bentran1vn.com";
    public string StorageEmail { get; set; } = "";
    public string StoragePassword { get; set; } = "";
    // Optional CDN/bucket base; empty uses the presigned object URL without its query.
    public string PublicBaseUrl { get; set; } = "";
    public string ObjectPrefix { get; set; } = "general/ai-flowers";
    public string LogoPath { get; set; } = "public/logo.png";
    public double LogoWidthRatio { get; set; } = 0.20;
    public double MarginRatio { get; set; } = 0.03;
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxDownloadBytes { get; set; } = 20 * 1024 * 1024;
    // Existing presign API advertises a 5 MiB limit.
    public int MaxUploadBytes { get; set; } = 5 * 1024 * 1024;
    public long MaxImagePixels { get; set; } = 20_000_000;
}

public interface IFlowerImageBranding
{
    void ValidateConfiguration();
    Task<BrandedFlowerImage> ApplyLogoAsync(string mockupImageUrl, Guid flowerId, CancellationToken cancellationToken = default);
}

public record BrandedFlowerImage(string ImageUrl, string ObjectKey, string LogoSha256,
    int ImageWidth, int ImageHeight, int LogoX, int LogoY, int LogoWidth, int LogoHeight);

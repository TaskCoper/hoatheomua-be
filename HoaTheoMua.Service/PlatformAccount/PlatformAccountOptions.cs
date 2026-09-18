using System.Text.RegularExpressions;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.PlatformAccount;

public class PlatformAccountOptions
{
    public string ActiveEncryptionKeyId { get; set; } = "";
    public Dictionary<string, string> EncryptionKeys { get; set; } = new();
    public int OAuthStateMinutes { get; set; } = 10;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public MetaOptions Meta { get; set; } = new();
    public ZaloOptions Zalo { get; set; } = new();

    public class MetaOptions
    {
        public bool Enabled { get; set; }
        public string AppId { get; set; } = "";
        public string AppSecret { get; set; } = "";
        public string GraphApiVersion { get; set; } = ""; // Explicitly pin the version approved for the app.
        public string ConfigId { get; set; } = ""; // Facebook Login for Business; user access token configuration.
        public string FacebookCallbackUrl { get; set; } = "";
        public string InstagramCallbackUrl { get; set; } = "";
    }
    public class ZaloOptions
    {
        public bool Enabled { get; set; }
        public string AppId { get; set; } = "";
        public string AppSecret { get; set; } = "";
        public string CallbackUrl { get; set; } = "";
    }
    public string Callback(PlatformType platform) => platform switch
    {
        PlatformType.Facebook => Meta.FacebookCallbackUrl,
        PlatformType.Instagram => Meta.InstagramCallbackUrl,
        _ => Zalo.CallbackUrl
    };
    public void RequireConfigured(PlatformType platform)
    {
        var meta = platform != PlatformType.Zalo;
        if (!(meta ? Meta.Enabled : Zalo.Enabled))
            throw new AccountException(503, "PLATFORM_NOT_CONFIGURED", "Nền tảng chưa được bật kết nối.");
        var appId = meta ? Meta.AppId : Zalo.AppId;
        var secret = meta ? Meta.AppSecret : Zalo.AppSecret;
        var callback = Callback(platform);
        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(secret)
            || (meta && !Regex.IsMatch(Meta.GraphApiVersion, @"^v\d+\.\d+$"))
            || (meta && Meta.ConfigId.Length > 0 && !Regex.IsMatch(Meta.ConfigId, @"\A[0-9]+\z"))
            || !Uri.TryCreate(callback, UriKind.Absolute, out var uri)
            || !(uri.Scheme == "https" || (uri.Scheme == "http" && uri.IsLoopback))
            || uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0
            || !uri.AbsolutePath.EndsWith($"/platform-accounts/{platform.ToString().ToLowerInvariant()}/callback", StringComparison.Ordinal)
            || OAuthStateMinutes is < 1 or > 30 || RequestTimeoutSeconds is < 1 or > 60)
            throw new AccountException(503, "PLATFORM_NOT_CONFIGURED", "Cấu hình ứng dụng hoặc callback nền tảng chưa hợp lệ.");
    }
}

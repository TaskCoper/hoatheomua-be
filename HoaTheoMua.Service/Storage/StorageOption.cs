namespace HoaTheoMua.Service.Storage;

public class StorageOption
{
    public string Region { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public int PresignedUrlExpirationMinutes { get; set; } = 15;
    public string[] AllowedFileTypes { get; set; } = [];
    public long MaxFileSizeBytes { get; set; } = 104_857_600;
    public int PendingExpirationHours { get; set; } = 2;
}

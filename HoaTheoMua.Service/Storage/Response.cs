namespace HoaTheoMua.Service.Storage;

public static class Response
{
    public record PresignedUploadResponse(
        Guid FileId,
        string UploadUrl,
        string StorageKey,
        Dictionary<string, string> RequiredHeaders,
        DateTimeOffset ExpiresAt);

    public record DownloadUrlResponse(string DownloadUrl, DateTimeOffset ExpiresAt);

    public record ConfirmUploadResponse(string Message);
}

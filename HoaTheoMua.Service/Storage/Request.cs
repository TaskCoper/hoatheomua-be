namespace HoaTheoMua.Service.Storage;

public static class Request
{
    public record GetUploadUrlRequest(string FileName, string ContentType, long FileSizeBytes);
}

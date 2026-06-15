namespace HoaTheoMua.Service.Storage;

public interface IService
{
    Task<Response.PresignedUploadResponse> GenerateUploadUrlAsync(Request.GetUploadUrlRequest request);
    Task<Response.DownloadUrlResponse> GenerateDownloadUrlAsync(Guid fileId, int expirationMinutes = 60);
    Task<Response.ConfirmUploadResponse> ConfirmUploadAsync(Guid fileId);
    Task<bool> DeleteFileAsync(Guid fileId);
}

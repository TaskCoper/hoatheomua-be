using Amazon.S3;
using Amazon.S3.Model;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.Storage;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IAmazonS3 _s3Client;
    private readonly StorageOption _storageOption;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<Service> _logger;

    public Service(AppDbContext dbContext, IAmazonS3 s3Client, IOptions<StorageOption> storageOptions,
        IHttpContextAccessor httpContextAccessor, ILogger<Service> logger)
    {
        _dbContext = dbContext;
        _s3Client = s3Client;
        _storageOption = storageOptions.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Response.PresignedUploadResponse> GenerateUploadUrlAsync(Request.GetUploadUrlRequest request)
    {
        var userId = GetCurrentUserId();
        ValidateUploadRequest(request);

        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(request.FileName);
        var storageKey = $"uploads/{userId}/{DateTimeOffset.UtcNow:yyyy/MM}/{fileId}{extension}";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_storageOption.PresignedUrlExpirationMinutes);

        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            FileName = request.FileName,
            StorageKey = storageKey,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            BucketName = GetBucketName(),
            UploadedByUserId = userId,
            UploadStatus = FileUploadStatus.Pending,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.FileMetadata.AddAsync(fileMetadata);
        await _dbContext.SaveChangesAsync();

        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = GetBucketName(),
            Key = storageKey,
            Verb = HttpVerb.PUT,
            Expires = expiresAt.UtcDateTime,
            ContentType = request.ContentType
        };

        var uploadUrl = await _s3Client.GetPreSignedURLAsync(presignRequest);

        return new Response.PresignedUploadResponse(
            fileMetadata.Id,
            uploadUrl,
            storageKey,
            new Dictionary<string, string> { ["Content-Type"] = request.ContentType },
            expiresAt);
    }

    public async Task<Response.DownloadUrlResponse> GenerateDownloadUrlAsync(Guid fileId, int expirationMinutes = 60)
    {
        var userId = GetCurrentUserId();
        var file = await _dbContext.FileMetadata
            .FirstOrDefaultAsync(f => f.Id == fileId && f.UploadedByUserId == userId && !f.IsDeleted);

        if (file == null || file.UploadStatus != FileUploadStatus.Completed)
        {
            throw new NotFoundException("File không tồn tại hoặc chưa upload hoàn tất.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        var presignRequest = new GetPreSignedUrlRequest
        {
            BucketName = file.BucketName,
            Key = file.StorageKey,
            Verb = HttpVerb.GET,
            Expires = expiresAt.UtcDateTime
        };

        var downloadUrl = await _s3Client.GetPreSignedURLAsync(presignRequest);
        return new Response.DownloadUrlResponse(downloadUrl, expiresAt);
    }

    public async Task<Response.ConfirmUploadResponse> ConfirmUploadAsync(Guid fileId)
    {
        var userId = GetCurrentUserId();
        var file = await _dbContext.FileMetadata
            .FirstOrDefaultAsync(f => f.Id == fileId && f.UploadedByUserId == userId && !f.IsDeleted);

        if (file == null)
        {
            throw new NotFoundException("File metadata không tồn tại.");
        }

        try
        {
            var metadata = await _s3Client.GetObjectMetadataAsync(file.BucketName, file.StorageKey);

            file.UploadStatus = FileUploadStatus.Completed;
            file.FileSizeBytes = metadata.ContentLength;
            file.CompletedAt = DateTimeOffset.UtcNow;
            file.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();
            return new Response.ConfirmUploadResponse("File đã được xác nhận và đồng bộ trạng thái thành công.");
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            file.UploadStatus = FileUploadStatus.Failed;
            file.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("S3 object not found while confirming file {FileId}.", fileId);
            throw new BadRequestException("Không tìm thấy object trên S3. Vui lòng upload file trước khi confirm.");
        }
    }

    public async Task<bool> DeleteFileAsync(Guid fileId)
    {
        var userId = GetCurrentUserId();
        var file = await _dbContext.FileMetadata
            .FirstOrDefaultAsync(f => f.Id == fileId && f.UploadedByUserId == userId && !f.IsDeleted);

        if (file == null)
        {
            return false;
        }

        await _s3Client.DeleteObjectAsync(file.BucketName, file.StorageKey);

        file.UploadStatus = FileUploadStatus.Deleted;
        file.IsDeleted = true;
        file.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private void ValidateUploadRequest(Request.GetUploadUrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new BadRequestException("Tên file không được để trống.");
        }

        if (request.FileSizeBytes <= 0)
        {
            throw new BadRequestException("Dung lượng file không hợp lệ.");
        }

        if (request.FileSizeBytes > _storageOption.MaxFileSizeBytes)
        {
            throw new BadRequestException(
                $"Dung lượng file vượt quá giới hạn {_storageOption.MaxFileSizeBytes} bytes.");
        }

        var allowedFileTypes = _storageOption.AllowedFileTypes.Select(x => x.ToLowerInvariant()).ToHashSet();
        if (!allowedFileTypes.Contains(request.ContentType.ToLowerInvariant()))
        {
            throw new BadRequestException($"Loại file '{request.ContentType}' không được hỗ trợ.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Access token không hợp lệ hoặc thiếu UserId.");
        }

        return userId;
    }

    private string GetBucketName()
    {
        if (string.IsNullOrWhiteSpace(_storageOption.BucketName))
        {
            throw new InvalidOperationException("AWS BucketName chưa được cấu hình.");
        }

        return _storageOption.BucketName;
    }
}
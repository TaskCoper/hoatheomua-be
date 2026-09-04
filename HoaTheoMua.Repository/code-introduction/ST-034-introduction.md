# ST-034 - Tải ảnh mẫu hoa AI

## 0. Phạm vi

- Backend chỉ kiểm tra quyền rồi trả **signed download URL**.
- FE tải ảnh trực tiếp từ Cloudinary; file không đi qua backend.
- Không gọi AI, không tạo history/output, không trừ quota.
- Ảnh phải được ST-033 lưu dưới dạng PNG, đã gắn logo.

## I. Dữ liệu sử dụng

| Field | Mục đích |
|---|---|
| `generated_flowers.id` | `outputId` và Cloudinary `publicId`. |
| `user_id` | Kiểm tra chủ sở hữu. |
| `flower_generation_status` | Chỉ tải khi `generated`. |
| `image_url` | Xác nhận output đã có ảnh. |

Không dùng `base_id`: chọn F03 thì tải đúng ảnh F03.

## II. API cần thêm

| API | Mục đích |
|---|---|
| `GET /api/v1/generated-flowers/{outputId}/download-url` | Trả URL Cloudinary có thời hạn. |

Luồng:

```text
FE → BE kiểm tra owner/status → trả signed URL
FE → Cloudinary tải file trực tiếp
```

## III. Điều kiện Cloudinary

- Cần package `CloudinaryDotNet >= 1.29.0`.
- ST-033 upload ảnh với delivery type `authenticated`.
- Quy ước `publicId = generated-flowers/{outputId:N}`.
- URL tải có hiệu lực 5 phút và `attachment = true`.

> Nếu upload bằng type mặc định `upload`, ảnh vẫn là public. Signed URL khi đó không giải quyết đầy đủ yêu cầu chỉ chủ sở hữu được tải.

## IV. Structure code

```text
HoaTheoMua.Service/GeneratedFlower/
├── Request.cs
├── Response.cs
├── IService.cs
└── Service.cs
```

### `Request.cs`

```csharp
namespace HoaTheoMua.Service.GeneratedFlower;

public class Request
{
    public class GetFlowerDownloadUrlRequest
    {
        public Guid OutputId { get; set; }
    }
}
```

### `Response.cs`

```csharp
namespace HoaTheoMua.Service.GeneratedFlower;

public class Response
{
    public class GetFlowerDownloadUrlResponse
    {
        public string DownloadUrl { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
```

### `IService.cs`

```csharp
namespace HoaTheoMua.Service.GeneratedFlower;

public interface IService
{
    Task<Response.GetFlowerDownloadUrlResponse> GetFlowerDownloadUrl(
        Request.GetFlowerDownloadUrlRequest request);
}
```

### `Service.cs`

```csharp
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.GeneratedFlower;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly Cloudinary _cloudinary;

    public Service(
        AppDbContext dbContext,
        ICurrentUserService currentUser,
        Cloudinary cloudinary)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _cloudinary = cloudinary;
    }

    public async Task<Response.GetFlowerDownloadUrlResponse>
        GetFlowerDownloadUrl(Request.GetFlowerDownloadUrlRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();

        var flower = await _dbContext.GeneratedFlowers
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == request.OutputId && !x.IsDeleted);

        if (flower == null)
            throw new NotFoundException("Không tìm thấy mẫu hoa.");

        if (flower.UserId != userId)
            throw new ForbiddenException(
                "Bạn không có quyền tải xuống mẫu hoa này.");

        if (flower.Status != FlowerGenerationStatus.Generated ||
            string.IsNullOrWhiteSpace(flower.ImageUrl))
            throw new ConflictException(
                "Ảnh đang được tạo. Vui lòng đợi hoàn tất để tải xuống.");

        var publicId = $"generated-flowers/{flower.Id:N}";

        // Kiểm tra asset còn tồn tại, không tải nội dung ảnh qua BE.
        var resource = await _cloudinary.GetResourceAsync(
            new GetResourceParams(publicId)
            {
                ResourceType = ResourceType.Image,
                Type = "authenticated"
            });

        if (resource.Error != null)
            throw new BadGatewayException(
                "Không thể tải mẫu hoa. Vui lòng thử lại.");

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            "Asia/Ho_Chi_Minh");
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            timeZone);

        var fileName =
            $"mauhoa_{flower.Id:N}_{localNow:yyyyMMdd_HHmmss}.png";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

        var downloadUrl = _cloudinary.DownloadPrivate(
            publicId: publicId,
            attachment: true,
            format: "png",
            type: "authenticated",
            expiresAt: expiresAt.ToUnixTimeSeconds(),
            resourceType: "image",
            targetFilename: fileName);

        return new Response.GetFlowerDownloadUrlResponse
        {
            DownloadUrl = downloadUrl,
            FileName = fileName,
            ExpiresAt = expiresAt
        };
    }
}
```

`GetResourceAsync` chỉ kiểm tra metadata; backend không đọc byte ảnh.

## V. Controller

```csharp
using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeneratedFlowerService = HoaTheoMua.Service.GeneratedFlower;
using Request = HoaTheoMua.Service.GeneratedFlower.Request;

namespace HoaTheoMua.Controllers;

[ApiController]
[Route("api/v1/generated-flowers")]
[Authorize(Policy = JwtExtensions.CustomerPolicy)]
public class GeneratedFlowerController : ControllerBase
{
    private readonly GeneratedFlowerService.IService _generatedFlowerService;

    public GeneratedFlowerController(
        GeneratedFlowerService.IService generatedFlowerService)
    {
        _generatedFlowerService = generatedFlowerService;
    }

    [HttpGet("{outputId:guid}/download-url")]
    public async Task<IActionResult> GetFlowerDownloadUrl(
        [FromRoute] Guid outputId)
    {
        var result = await _generatedFlowerService.GetFlowerDownloadUrl(
            new Request.GetFlowerDownloadUrlRequest
            {
                OutputId = outputId
            });

        return Ok(ApiResponseFactory.Base(
            result,
            true,
            "",
            HttpContext.TraceIdentifier));
    }
}
```
```csharp
public async Task<string> UploadImageAsync(
      IFormFile file,
      Guid outputId)
  {
      if (file == null || file.Length == 0)
          throw new ArgumentException("File is empty or null", nameof(file));

      if (!IsImageFile(file))
          throw new ArgumentException("File is not an image", nameof(file));

      await using var stream = file.OpenReadStream();

      var publicId = $"generated-flowers/{outputId:N}";

      var uploadParams = new ImageUploadParams
      {
          File = new FileDescription(file.FileName, stream),
          PublicId = publicId,
          Type = "authenticated",
          Format = "png"
      };

      var uploadResult = await _cloudinary.UploadAsync(uploadParams);

      return uploadResult.SecureUrl.ToString();
  }
  
public string CreateImageDownloadUrl(
      Guid outputId,
      string fileName)
  {
      var publicId = $"generated-flowers/{outputId:N}";

      var expiresAt = DateTimeOffset.UtcNow
          .AddMinutes(5)
          .ToUnixTimeSeconds();

      return _cloudinary.DownloadPrivate(
          publicId: publicId,
          attachment: true,
          format: "png",
          type: "authenticated",
          expiresAt: expiresAt,
          resourceType: "image",
          targetFilename: fileName);
  }
```



## VI. Request/response

### Request

```http
GET /api/v1/generated-flowers/{outputId}/download-url
Authorization: Bearer {token}
```

### Response

```json
{
  "data": {
    "downloadUrl": "https://api.cloudinary.com/...&signature=...",
    "fileName": "mauhoa_f03_20260903_103000.png",
    "expiresAt": "2026-09-03T03:35:00Z"
  }
}
```

FE dùng `downloadUrl` ngay sau khi nhận response. Hết hạn thì gọi lại API để lấy URL mới.

### Lỗi

| HTTP | Trường hợp |
|---:|---|
| `404` | Output không tồn tại. |
| `403` | Output thuộc khách hàng khác. |
| `409` | Ảnh chưa `generated`. |
| `502` | Asset không còn trên Cloudinary. |

## VII. Core flow

```text
outputId F03
→ BE kiểm tra F03.user_id và status
→ BE tạo signed URL 5 phút
→ FE tải F03.png trực tiếp từ Cloudinary
→ BE không truyền byte ảnh, không thay đổi quota/database
```

## VIII. Điểm cần chốt với ST-033

1. Upload ảnh dưới type `authenticated`, format PNG.
2. Dùng đúng public ID `generated-flowers/{outputId:N}`.
3. Lưu `image_url` sau khi upload thành công.
4. API secret chỉ nằm ở backend, không trả cho FE.


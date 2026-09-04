# ST-041 - Tải xuống thiệp AI đã tạo

## I. Hiểu nhanh US

US-041 chỉ dùng để tải lại ảnh thiệp đã tạo thành công; không gọi AI, không tạo History và không trừ quota.

Hình dung đơn giản:

```text
FE gửi GeneratedCardId
→ BE kiểm tra Card, History và ownership
→ BE tạo signed URL Cloudinary
→ FE tải ảnh trực tiếp từ Cloudinary.
```

Có hai ngữ cảnh:

```text
Từ History:
GeneratedCardId
→ tên file dùng CardId.

Từ Chi tiết Order:
GeneratedCardId + OrderId
→ kiểm tra Card thuộc đúng Order
→ tên file dùng mã Order.
```

Luồng hoạt động:

1. Người dùng bấm **"Tải xuống thiệp"**.
2. FE gọi API với `generatedCardId`.
3. Nếu đang ở Chi tiết Order, FE gửi thêm `orderId`.
4. BE kiểm tra Card tồn tại, có Card History, thuộc user và có `image_url`.
5. Nếu có `orderId`, BE kiểm tra Order thuộc user và kiểm tra liên kết Card–Order theo phương án được team chốt.
6. BE kiểm tra asset Cloudinary còn tồn tại rồi tạo signed URL có hạn 5 phút.
7. FE dùng URL để tải trực tiếp từ Cloudinary; ảnh không đi qua BE.

Chỉ cần 1 API:

```http
GET /api/v1/generated-cards/{generatedCardId}/download-url
GET /api/v1/generated-cards/{generatedCardId}/download-url?orderId={orderId}
```

Cùng một API, `orderId` là tùy chọn:

- Không có `orderId`: tải từ History.
- Có `orderId`: tải từ Chi tiết Order.

Response:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "downloadUrl": "https://cloudinary.com/signed-url...",
    "fileName": "thiep_CARD-001_20260903_103000.png",
    "expiresAt": "2026-09-03T03:35:00Z"
  }
}
```

Điểm quan trọng: **BE không trả byte ảnh hoặc permanent `image_url`; BE chỉ trả signed URL sau khi kiểm tra quyền.**

Theo quyết định mới nhất:

- Card chỉ được tạo sau khi Order đã thanh toán.
- Người dùng có thể tạo Card mới hoặc chọn Card cũ từ History cho Order đã thanh toán.

> [!IMPORTANT]
> **NOTICE — Liên kết Card–Order chưa chốt:** Code minh họa bên dưới đang tạm kiểm tra bằng `generated_cards.order_id`. Nếu team chốt lưu Card được chọn tại `OrderDetails.Metadata.card.generatedCardId`, API tải từ Order phải kiểm tra giá trị trong Metadata thay cho việc chỉ so sánh `generated_cards.order_id`. Trường hợp chọn Card cũ không được sửa `order_id` của Card đó vì có thể làm sai Order gốc. Hãy cập nhật lại ST-041 ngay khi contract này được xác nhận.

## II. Code implement API

Module:

```text
HoaTheoMua.Service/GeneratedCard/
├── Request.cs
├── Response.cs
├── IService.cs
└── Service.cs
```

### API 1 - Lấy signed URL tải thiệp

**Vai trò:** Kiểm tra quyền và trả URL tạm thời để FE tải đúng ảnh thiệp từ Cloudinary.

**Request**

```http
GET /api/v1/generated-cards/{generatedCardId}/download-url
GET /api/v1/generated-cards/{generatedCardId}/download-url?orderId={orderId}
```

```csharp
// Request.cs — nested class trong Request
public class GetDownloadUrlRequest
{
    public Guid? OrderId { get; set; }
}
```

`generatedCardId` là route parameter nên không lặp lại trong Request DTO.

**Response**

```csharp
// Response.cs — nested class trong Response
public class GetDownloadUrlResponse
{
    public string DownloadUrl { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
}
```

**IService**

```csharp
Task<Response.GetDownloadUrlResponse> GetDownloadUrl(
    Guid generatedCardId,
    Request.GetDownloadUrlRequest request);
```

**Service**

```csharp
using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.GeneratedCard;

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

    public async Task<Response.GetDownloadUrlResponse> GetDownloadUrl(
        Guid generatedCardId,
        Request.GetDownloadUrlRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();

        var card = await _dbContext.GeneratedCards
            .AsNoTracking()
            .Where(x => x.Id == generatedCardId && !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.OrderId,
                x.ImageUrl
            })
            .FirstOrDefaultAsync();

        if (card == null)
            throw new NotFoundException(
                "Không tìm thấy thiệp hoặc thiệp không còn tồn tại.");

        var ownsHistory = await _dbContext.ClientHistories
            .AsNoTracking()
            .AnyAsync(x =>
                x.OutputId == generatedCardId &&
                x.Type == GenerationType.Card &&
                x.UserId == userId &&
                !x.IsDeleted);

        if (card.UserId != userId || !ownsHistory)
            throw new ForbiddenException(
                "Bạn không có quyền thực hiện thao tác này.");

        if (string.IsNullOrWhiteSpace(card.ImageUrl))
            throw new ConflictException("Ảnh không còn khả dụng.");

        var fileCode = card.Id.ToString("N");

        // NOTICE: Đây là phương án tạm dùng generated_cards.order_id.
        // Nếu Card chọn từ History được liên kết qua
        // OrderDetails.Metadata.card.generatedCardId thì phải query Metadata tại đây.
        if (request.OrderId.HasValue)
        {
            var order = await _dbContext.Orders
                .AsNoTracking()
                .Where(x =>
                    x.Id == request.OrderId.Value &&
                    x.UserId == userId &&
                    !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.TrackNumber
                })
                .FirstOrDefaultAsync();

            // TODO sau khi chốt contract:
            // Nếu dùng OrderDetails.Metadata, thay điều kiện card.OrderId
            // bằng kiểm tra Metadata.card.generatedCardId == generatedCardId.
            if (order == null || card.OrderId != order.Id)
                throw new ForbiddenException(
                    "Không thể tải thiệp từ đơn hàng này.");

            fileCode = SanitizeFilePart(order.TrackNumber);
        }

        var publicId = $"generated-cards/{card.Id:N}";
        var resource = await _cloudinary.GetResourceAsync(
            new GetResourceParams(publicId)
            {
                ResourceType = ResourceType.Image,
                Type = "authenticated"
            });

        if (resource.Error != null)
            throw new BadGatewayException(
                "Tải thiệp thất bại. Vui lòng thử lại.");

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
            "Asia/Ho_Chi_Minh");
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            timeZone);

        var fileName =
            $"thiep_{fileCode}_{localNow:yyyyMMdd_HHmmss}.png";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

        var downloadUrl = _cloudinary.DownloadPrivate(
            publicId: publicId,
            attachment: true,
            format: "png",
            type: "authenticated",
            expiresAt: expiresAt.ToUnixTimeSeconds(),
            resourceType: "image",
            targetFilename: fileName);

        return new Response.GetDownloadUrlResponse
        {
            DownloadUrl = downloadUrl,
            FileName = fileName,
            ExpiresAt = expiresAt
        };
    }

    private static string SanitizeFilePart(string value)
    {
        var sanitized = Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9_-]", "_");
        return string.IsNullOrWhiteSpace(sanitized) ? "order" : sanitized;
    }
}
```

Luồng query của Service:

```text
GeneratedCardId
→ query generated_cards
→ query client_histories với type=card và output_id=GeneratedCardId
→ kiểm tra cùng UserId
→ nếu có OrderId: query Orders và kiểm tra liên kết Card–Order
→ NOTICE: nguồn liên kết chưa chốt giữa generated_cards.order_id
  và OrderDetails.Metadata.card.generatedCardId
→ kiểm tra asset Cloudinary
→ tạo signed URL
→ trả DownloadUrl + FileName + ExpiresAt.
```

Lỗi chính:

| HTTP | Trường hợp |
|---:|---|
| `404` | Card không tồn tại hoặc đã xóa. |
| `403` | Card không thuộc user hoặc Order không khớp Card. |
| `409` | Card chưa có `image_url`. |
| `502` | Asset Cloudinary không tồn tại hoặc không thể kiểm tra. |

## III. Điều kiện phụ thuộc từ ST-035/ST-036

Không cần thêm bảng hoặc field. Khi upload ảnh Card phải thống nhất:

```text
publicId = generated-cards/{generatedCardId:N}
delivery type = authenticated
format = png
```

Sau khi upload thành công:

```text
generated_cards.image_url = URL ảnh
generated_cards.user_id = UserId sở hữu
generated_cards.order_id = OrderId đã tạo Card mới (nếu team chốt dùng field này)
client_histories.type = card
client_histories.output_id = GeneratedCardId
```

Khi chọn Card cũ từ History, phương án đang chờ xác nhận là:

```text
OrderDetails.Metadata.card.generatedCardId = GeneratedCardId được chọn
OrderDetails.Metadata.card.selectionSource = history
```

Không cập nhật `generated_cards.order_id` của Card cũ sang Order mới.

Nếu không thống nhất `publicId` và `authenticated`, API không thể tạo đúng signed URL tải xuống.

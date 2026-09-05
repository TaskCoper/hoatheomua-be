# US-046 - Admin tai xuong thiep AI

## I. Cac API can lam

US-046 dung de Admin lay link tai anh thiep AI cua khach hang; khong goi AI, khong tao history moi va khong tru quota.

Can **1 API**:

`GET /api/v1/admin/generated-cards/{generatedCardId}/download`

- FE goi khi Admin bam **Tai anh thiep xuong** tu danh sach US-044 hoac chi tiet US-043.
- BE kiem tra quyen Admin, kiem tra history `type = card`, card chua xoa va co `imageUrl`.
- Tra `imageUrl` va `fileName`; FE tai truc tiep tu storage.

Luong:

```text
Admin bam Tai anh
-> FE goi API download
-> BE check card + history hop le
-> FE dung imageUrl + fileName de tai anh
```

Luu y:

- Khong stream binary qua BE de tranh nang server khi nhieu Admin tai cung luc.
- Khong tao anh moi, khong tao `client_histories`, khong thay doi quota.
- Ten file dung format `thiep_[generatedCardId]_[yyyyMMdd_HHmmss].png`.
- Gio tao ten file dung timezone `Asia/Ho_Chi_Minh`.
- Route bat buoc gan `[Authorize(Policy = JwtExtensions.AdminPolicy)]`.

## II. Code tung API

### API 1 - Lay link tai anh thiep AI cho Admin

**Vai tro:** Tra link anh va ten file an toan de FE tai thiep AI truc tiep tu storage.

### IService

```csharp
namespace HoaTheoMua.Service.AdminGeneratedCard;

public interface IService
{
    Task<Response.CardDownloadResponse> GetDownloadUrl(
        Guid generatedCardId);
}
```

### Request

```http
GET /api/v1/admin/generated-cards/{generatedCardId}/download
Authorization: Bearer {admin-token}
```

Route parameter don gian nen khong can Request DTO.

### Response

```csharp
namespace HoaTheoMua.Service.AdminGeneratedCard;

public class Response
{
    public class CardDownloadResponse
    {
        public Guid GeneratedCardId { get; set; }
        public required string ImageUrl { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
    }
}
```

Payload mau:

```json
{
  "generatedCardId": "ca000000-0000-0000-0000-000000000001",
  "imageUrl": "https://res.cloudinary.com/.../card-01.png",
  "fileName": "thiep_ca00000000000000000000000000000001_20260905_143000.png",
  "contentType": "image/png"
}
```

### Service

```csharp
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.AdminGeneratedCard;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Response.CardDownloadResponse> GetDownloadUrl(
        Guid generatedCardId)
    {
        if (generatedCardId == Guid.Empty)
            throw new BadRequestException("GeneratedCardId khong hop le.");

        var card = await (
            from history in _dbContext.ClientHistories.AsNoTracking()
            join generatedCard in _dbContext.GeneratedCards.AsNoTracking()
                on history.OutputId equals generatedCard.Id
            where !history.IsDeleted &&
                  !generatedCard.IsDeleted &&
                  history.Type == GenerationType.Card &&
                  history.UserId == generatedCard.UserId &&
                  history.OutputId == generatedCardId
            select new
            {
                generatedCard.Id,
                generatedCard.ImageUrl
            })
            .FirstOrDefaultAsync();

        if (card == null)
            throw new NotFoundException("Khong tim thay thiep AI.");

        if (string.IsNullOrWhiteSpace(card.ImageUrl))
            throw new BadRequestException("Anh thiep khong con kha dung.");

        var fileName = BuildDownloadFileName(card.Id);

        return new Response.CardDownloadResponse
        {
            GeneratedCardId = card.Id,
            ImageUrl = card.ImageUrl,
            FileName = fileName,
            ContentType = "image/png"
        };
    }

    private static string BuildDownloadFileName(Guid generatedCardId)
    {
        var timeZone = FindVietnamTimeZone();
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            timeZone);

        return $"thiep_{generatedCardId:N}_{vietnamTime:yyyyMMdd_HHmmss}.png";
    }

    private static TimeZoneInfo FindVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
    }
}
```

## III. DB con thieu

DB hien tai du cho US-046:

```text
client_histories.output_id -> generated_cards.id
client_histories.type = card
generated_cards.image_url
```

Khong can them field moi.

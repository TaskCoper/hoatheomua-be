# US-043 - Admin xem chi tiet thiep AI

## I. Cac API can lam

US-043 dung de Admin xem chi tiet mot thiep AI da tao; khong hien thi gia/bill va khong sua du lieu.

Can **1 API**:

`GET /api/v1/admin/generated-cards/{generatedCardId}`

- FE goi khi Admin bam **Xem chi tiet** tu US-044.
- Tra anh thiep, thong tin tao thiep, snapshot template, trang thai file va cac Order lien quan.
- Khong tra gia thiep, bill snapshot hay base/extra/total price.

Luong:

```text
Admin bam Xem chi tiet
-> FE goi API chi tiet
-> BE lay client_histories(type=card) + generated_cards
-> BE lay Order lien quan qua OrderDetails.GeneratedCardIds
-> FE render chi tiet, lightbox va nut tai anh neu co imageUrl
```

Luu y:

- Template lay tu snapshot `client_histories.metadata.card_template.name`, khong lay live tu `card_templates`.
- Sender, receiver, message, size, form type lay tu `generated_cards`.
- Order lien quan lay qua `OrderDetails.GeneratedCardIds`.
- Neu `imageUrl` rong/null thi tra `fileStatus = unavailable`, FE disable nut tai anh.
- Neu storage load anh loi, FE hien placeholder; BE khong goi AI tao lai.
- Route bat buoc gan `[Authorize(Policy = JwtExtensions.AdminPolicy)]`.

## II. Code tung API

### API 1 - Lay chi tiet thiep AI

**Vai tro:** Tra chi tiet mot thiep AI cho Admin, kem danh sach Order dang dung thiep do.

### IService

```csharp
namespace HoaTheoMua.Service.AdminGeneratedCard;

public interface IService
{
    Task<Response.CardDetailResponse> GetById(
        Guid generatedCardId);
}
```

### Request

```http
GET /api/v1/admin/generated-cards/{generatedCardId}
Authorization: Bearer {admin-token}
```

Route parameter don gian nen khong can Request DTO.

### Response

```csharp
namespace HoaTheoMua.Service.AdminGeneratedCard;

public class Response
{
    public class CardDetailResponse
    {
        public Guid HistoryId { get; set; }
        public Guid GeneratedCardId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? ImageUrl { get; set; }
        public required string FileStatus { get; set; }
        public bool CanDownload { get; set; }
        public string? TemplateName { get; set; }
        public string? SizeKey { get; set; }
        public string? SizeName { get; set; }
        public decimal? SizeWidth { get; set; }
        public decimal? SizeHeight { get; set; }
        public required string FormType { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string? MessageContent { get; set; }
        public string? AttachedImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RelatedOrderResponse> Orders { get; set; } = [];
    }

    public class RelatedOrderResponse
    {
        public Guid OrderId { get; set; }
        public int? NhanhOrderId { get; set; }
        public required string Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
```

Payload mau:

```json
{
  "historyId": "1f2c0000-0000-0000-0000-000000000001",
  "generatedCardId": "ca000000-0000-0000-0000-000000000001",
  "customerId": "10000000-0000-0000-0000-000000000001",
  "customerName": "Nguyen Van A",
  "customerPhone": "0901234567",
  "customerEmail": "nguyenvana@example.com",
  "imageUrl": "https://res.cloudinary.com/.../card-01.png",
  "fileStatus": "available",
  "canDownload": true,
  "templateName": "Co dien",
  "sizeName": "10x15",
  "formType": "calligraphy",
  "senderName": "Minh",
  "receiverName": "Lan",
  "messageContent": "Chuc mung sinh nhat",
  "attachedImageUrl": null,
  "createdAt": "2026-09-05T10:30:00Z",
  "orders": [
    {
      "orderId": "0a000000-0000-0000-0000-000000000001",
      "nhanhOrderId": 123456,
      "status": "Paid",
      "createdAt": "2026-09-05T11:00:00Z"
    }
  ]
}
```

### Service

```csharp
using System.Text.Json;
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

    public async Task<Response.CardDetailResponse> GetById(
        Guid generatedCardId)
    {
        if (generatedCardId == Guid.Empty)
            throw new BadRequestException("GeneratedCardId khong hop le.");

        var result = await (
            from history in _dbContext.ClientHistories.AsNoTracking()
            join card in _dbContext.GeneratedCards.AsNoTracking()
                on history.OutputId equals card.Id
            where !history.IsDeleted &&
                  !card.IsDeleted &&
                  history.Type == GenerationType.Card &&
                  history.UserId == card.UserId &&
                  history.OutputId == generatedCardId
            select new Response.CardDetailResponse
            {
                HistoryId = history.Id,
                GeneratedCardId = card.Id,
                CustomerId = card.UserId,
                ImageUrl = card.ImageUrl,
                FileStatus = string.IsNullOrWhiteSpace(card.ImageUrl)
                    ? "unavailable"
                    : "available",
                CanDownload = !string.IsNullOrWhiteSpace(card.ImageUrl),
                TemplateName = ReadTemplateName(history.Metadata),
                SizeKey = card.SizeKey,
                SizeName = card.SizeName,
                SizeWidth = card.SizeWidth,
                SizeHeight = card.SizeHeight,
                FormType = card.FormType.ToString(),
                SenderName = card.SenderName,
                ReceiverName = card.ReceiverName,
                MessageContent = card.MessageContent,
                AttachedImageUrl = card.AttachedImageUrl,
                CreatedAt = history.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (result == null)
            throw new NotFoundException("Khong tim thay thiep AI.");

        var relatedOrders = await (
                from detail in _dbContext.OrderDetails.AsNoTracking()
                join order in _dbContext.Orders.AsNoTracking()
                    on detail.OrderId equals order.Id
                where !detail.IsDeleted &&
                      !order.IsDeleted &&
                      order.UserId == result.CustomerId &&
                      detail.GeneratedCardIds != null &&
                      detail.GeneratedCardIds.Contains(generatedCardId)
                select new
                {
                    OrderId = order.Id,
                    order.NhanhOrderId,
                    order.Status,
                    order.CreatedAt,
                    CustomerName = order.FullName,
                    CustomerPhone = order.Phone,
                    CustomerEmail = order.ReceiptEmail
                })
            .Distinct()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.OrderId)
            .ToListAsync();

        result.Orders = relatedOrders
            .Select(x => new Response.RelatedOrderResponse
            {
                OrderId = x.OrderId,
                NhanhOrderId = x.NhanhOrderId,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        var latestOrder = relatedOrders.FirstOrDefault();
        if (latestOrder != null)
        {
            result.CustomerName = latestOrder.CustomerName;
            result.CustomerPhone = latestOrder.CustomerPhone;
            result.CustomerEmail = latestOrder.CustomerEmail;
        }

        return result;
    }

    private static string? ReadTemplateName(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
            return null;

        try
        {
            using var json = JsonDocument.Parse(metadata);

            return json.RootElement.TryGetProperty("card_template", out var template) &&
                   template.TryGetProperty("name", out var name)
                ? name.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
```

Do `db_tong.md` chua co bang user profile, `CustomerName/Phone/Email` nen lay tu service user/profile neu solution co san. Neu chua co service do, BE chi tra chac chan duoc `CustomerId`; thong tin contact co the xem trong cac Order lien quan.

## III. DB con thieu

Neu bat buoc hien thi ho ten, so dien thoai, email customer ngay ca khi thiep chua lien ket Order:

```text
Can co nguon User/Profile theo user_id
```

Trong `db_tong.md` hien chi co `generated_cards.user_id`, khong co bang user profile noi bo. Khong nen lay sai tu `generated_cards.sender_name` vi day la nguoi gui tren thiep, khong chac la ten khach hang.

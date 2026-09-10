# US-043 - Admin xem chi tiet thiep

## I. Cac API can lam

US-043 dung de Admin xem chi tiet mot thiep da tao, gom ca AI Card va Handmade Card; bao gom snapshot gia cua thiep va cac Order da su dung thiep; khong sua du lieu.

Can **1 API**:

`GET /api/v1/admin/generated-cards/{generatedCardId}`

- FE goi khi Admin bam **Xem chi tiet** tu US-044.
- Tra anh thiep, thong tin tao thiep, snapshot template, snapshot gia va cac Order lien quan.
- `totalFee` trong moi Order la tong tien cua ca don tai thoi diem truy van; khong phai gia rieng cua thiep.

Luong:

```text
Admin bam Xem chi tiet
-> FE goi API chi tiet
-> BE lay client_histories(type=card hoac handmade-card) + generated_cards
-> BE lay Order lien quan qua OrderDetails.GeneratedCardIds
-> FE render chi tiet, lightbox va nut tai anh neu co imageUrl
```

Luu y:

- Template lay tu snapshot `client_histories.metadata.card_template.name`, khong lay live tu `card_templates`.
- Sender, receiver, message, size, form type lay tu `generated_cards`.
- `client_histories.type` chi dung de xac nhan history thiep: chap nhan `card` va `handmade-card`.
- Loai hien thi `AI` hay `Handmade` lay tu `generated_cards.card_type`, khong suy ra tu `client_histories.type`.
- Order lien quan lay qua `OrderDetails.GeneratedCardIds`.
- Neu `imageUrl` rong/null, FE tu disable nut tai anh.
- Neu storage load anh loi, FE hien placeholder; BE khong goi AI tao lai.
- Route bat buoc gan `[Authorize(Policy = JwtExtensions.AdminPolicy)]`.

## II. Code tung API

### API 1 - Lay chi tiet thiep

**Vai tro:** Tra chi tiet mot thiep cho Admin, kem danh sach Order dang dung thiep do.

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
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? RawImage { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? NameCustomer { get; set; }
        public string? PhoneCustomer { get; set; }
        public string? AddressCustomer { get; set; }
        public required string CardType { get; set; }
        public required string FormType { get; set; }
        public string SenderName { get; set; } = null!;
        public string ReceiverName { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public string? AttachedImageUrl { get; set; }
        public string? SizeName { get; set; }
        public decimal SizeWidth { get; set; }
        public decimal SizeHeight { get; set; }
        public decimal SizeBasePrice { get; set; }
        public int SizeMaxWords { get; set; }
        public int WordCount { get; set; }
        public string? WordConfigSnapshot { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ExtraPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? TemplateName { get; set; }
        public List<RelatedOrderResponse> Orders { get; set; } = [];
    }

    public class RelatedOrderResponse
    {
        public Guid Id { get; set; }
        public required string Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public decimal TotalFee { get; set; }
    }
}
```

Payload mau:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "historyId": "1f2c0000-0000-0000-0000-000000000002",
    "generatedCardId": "ca000000-0000-0000-0000-000000000002",
    "content": "Thiệp chúc mừng phong cách hiện đại",
    "imageUrl": "https://res.cloudinary.com/demo/card-02.png",
    "rawImage": null,
    "createdAt": "2026-09-05T10:30:00Z",
    "nameCustomer": "Tấn Hùng",
    "phoneCustomer": "0XXX",
    "addressCustomer": "AHC",
    "cardType": "AI",
    "formType": "Printed",
    "senderName": "Minh",
    "receiverName": "Lan",
    "messageContent": "Chúc bạn luôn hạnh phúc",
    "attachedImageUrl": null,
    "sizeName": "10 x 15",
    "sizeWidth": 10,
    "sizeHeight": 15,
    "sizeBasePrice": 20000,
    "sizeMaxWords": 30,
    "wordCount": 5,
    "wordConfigSnapshot": null,
    "basePrice": 20000,
    "extraPrice": 0,
    "totalPrice": 20000,
    "templateName": "Hiện đại",
    "orders": [
      {
        "id": "0a000000-0000-0000-0000-000000000010",
        "status": "Paid",
        "createdAt": "2026-09-05T11:00:00Z",
        "totalFee": 520000
      },
      {
        "id": "0a000000-0000-0000-0000-000000000011",
        "status": "Paid",
        "createdAt": "2026-09-06T09:15:00Z",
        "totalFee": 760000
      }
    ]
  }
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

        var cardHistoryTypes = new[] { "card", "handmade-card" };

        var cardRow = await (
            from history in _dbContext.ClientHistories.AsNoTracking()
            join card in _dbContext.GeneratedCards.AsNoTracking()
                on history.OutputId equals card.Id
            where !history.IsDeleted &&
                  !card.IsDeleted &&
                  cardHistoryTypes.Contains(history.Type) &&
                  history.UserId == card.UserId &&
                  history.OutputId == generatedCardId
            select new
            {
                UserId = card.UserId,
                Result = new Response.CardDetailResponse
                {
                    HistoryId = history.Id,
                    GeneratedCardId = card.Id,
                    Content = card.Content,
                    ImageUrl = card.ImageUrl,
                    RawImage = card.RawImage,
                    CreatedAt = history.CreatedAt,
                    CardType = card.CardType.ToString(),
                    FormType = card.FormType.ToString(),
                    SenderName = card.SenderName,
                    ReceiverName = card.ReceiverName,
                    MessageContent = card.MessageContent,
                    AttachedImageUrl = card.AttachedImageUrl,
                    SizeName = card.SizeName,
                    SizeWidth = card.SizeWidth,
                    SizeHeight = card.SizeHeight,
                    SizeBasePrice = card.SizeBasePrice,
                    SizeMaxWords = card.SizeMaxWords,
                    WordCount = card.WordCount,
                    WordConfigSnapshot = card.WordConfigSnapshot,
                    BasePrice = card.BasePrice,
                    ExtraPrice = card.ExtraPrice,
                    TotalPrice = card.TotalPrice,
                    TemplateName = ReadTemplateName(history.Metadata)
                }
            })
            .FirstOrDefaultAsync();

        if (cardRow == null)
            throw new NotFoundException("Khong tim thay thiep.");

        var result = cardRow.Result;

        var relatedOrders = await (
                from detail in _dbContext.OrderDetails.AsNoTracking()
                join order in _dbContext.Orders.AsNoTracking()
                    on detail.OrderId equals order.Id
                where !detail.IsDeleted &&
                      !order.IsDeleted &&
                      order.UserId == cardRow.UserId &&
                      detail.GeneratedCardIds != null &&
                      detail.GeneratedCardIds.Contains(generatedCardId)
                select new
                {
                    Id = order.Id,
                    order.Status,
                    order.CreatedAt,
                    order.TotalFee,
                    NameCustomer = order.FullName,
                    PhoneCustomer = order.Phone,
                    AddressCustomer = order.Address
                })
            .Distinct()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        result.Orders = relatedOrders
            .Select(x => new Response.RelatedOrderResponse
            {
                Id = x.Id,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                TotalFee = x.TotalFee
            })
            .ToList();

        // Các Order của Card có cùng thông tin customer, nên lấy một Order là đủ.
        var customerOrder = relatedOrders.FirstOrDefault();
        if (customerOrder != null)
        {
            result.NameCustomer = customerOrder.NameCustomer;
            result.PhoneCustomer = customerOrder.PhoneCustomer;
            result.AddressCustomer = customerOrder.AddressCustomer;
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

`nameCustomer`, `phoneCustomer`, `addressCustomer` lay tu mot Order co chua thiep nay (`Order.FullName`, `Order.Phone`, `Order.Address`). Cac Order cua cung Card co cung thong tin customer. Khong lay tu `senderName`, vi do la nguoi gui duoc in tren thiep, khong chac la customer dat hang. Danh sach `orders` van sap xep moi nhat truoc chi de FE hien thi.

## III. DB con thieu

Neu bat buoc hien thi ho ten, so dien thoai, email customer ngay ca khi thiep chua lien ket Order:

```text
Can co nguon User/Profile theo user_id
```

Trong `db_tong.md` hien chi co `generated_cards.user_id`, khong co bang user profile noi bo. Khong nen lay sai tu `generated_cards.sender_name` vi day la nguoi gui tren thiep, khong chac la ten khach hang.

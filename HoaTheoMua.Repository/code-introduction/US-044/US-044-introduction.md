# US-044 — Admin xem danh sách thiệp

## I. Mục đích API

`GET /api/v1/admin/generated-cards`

Admin dùng API này để xem danh sách thiệp AI và Handmade, có tìm kiếm, lọc và phân trang. API không thay đổi dữ liệu.

- Sắp xếp thiệp mới tạo trước.
- Search theo **tên customer** hoặc **tên mẫu thiệp** (`client_histories.metadata.card_template.name`).
- Filter theo `sizeName`: `S`, `M` hoặc `L`.
- Mỗi Card bắt buộc đã được gắn vào một `OrderDetails.GeneratedCardIds`; thông tin customer lấy từ một Order chứa Card đó.
- Khi tìm theo customer, query OrderDetail bằng `GeneratedCardIds.Contains(cardId)`. Vì vậy việc chuyển từ một `GeneratedCard` sang mảng `GeneratedCardIds uuid[]` là phù hợp; không làm thay đổi response của API.
- Route bắt buộc gắn `[Authorize(Policy = JwtExtensions.AdminPolicy)]`.

## II. Contract

### IService

```csharp
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.AdminGeneratedCard;

public interface IService
{
    Task<BasePaginationResponse> GetCards(Request.GetCardsRequest request);
}
```

### Request

```http
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10&keyword=&sizeName=
```

```csharp
namespace HoaTheoMua.Service.AdminGeneratedCard;

public class Request
{
    public class GetCardsRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Keyword { get; set; }
        public string? SizeName { get; set; }
    }
}
```

`pageSize` không dùng whitelist 5/10/20/...; mọi giá trị dương đều hợp lệ. Nếu `pageIndex` hoặc `pageSize` nhỏ hơn hoặc bằng 0 thì BE lần lượt dùng `1` và `10`.

### Response DTO

```csharp
namespace HoaTheoMua.Service.AdminGeneratedCard;

public class Response
{
    public class CardListItemResponse
    {
        public Guid HistoryId { get; set; }
        public Guid GeneratedCardId { get; set; }
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? RawImage { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CardType { get; set; } = null!;
        public string FormType { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string ReceiverName { get; set; } = null!;
        public string? NameCustomer { get; set; }
        public string? PhoneCustomer { get; set; }
        public string? AddressCustomer { get; set; }
        public string MessageContent { get; set; } = null!;
        public string? AttachedImageUrl { get; set; }
        public string SizeName { get; set; } = null!;
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
    }
}
```

Không trả `sizeKey`, `customerId`, `formType` filter hay dữ liệu Order trong từng item.

### Payload mẫu

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "items": [
      {
        "historyId": "11111111-1111-1111-1111-111111111111",
        "generatedCardId": "22222222-2222-2222-2222-222222222222",
        "content": "Thiệp sinh nhật phong cách cổ điển",
        "imageUrl": "https://res.cloudinary.com/demo/card.png",
        "rawImage": null,
        "createdAt": "2026-08-22T08:15:00Z",
        "cardType": "AI",
        "formType": "Calligraphy",
        "senderName": "Nguyễn Văn A",
        "receiverName": "Lan",
        "nameCustomer": "Tấn Hùng",
        "phoneCustomer": "0XXX",
        "addressCustomer": "AHC",
        "messageContent": "Chúc mừng sinh nhật",
        "attachedImageUrl": null,
        "sizeName": "S",
        "sizeWidth": 10,
        "sizeHeight": 15,
        "sizeBasePrice": 20000,
        "sizeMaxWords": 30,
        "wordCount": 4,
        "wordConfigSnapshot": null,
        "basePrice": 20000,
        "extraPrice": 0,
        "totalPrice": 20000,
        "templateName": "Modern Pink"
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

## III. Main flow Service

```csharp
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.AdminGeneratedCard;

// Dịch sang PostgreSQL: jsonb_extract_path_text(metadata, 'card_template', 'name').
// Không được gọi method này ngoài LINQ-to-Entities.
public static class JsonbDbFunctions
{
    [DbFunction("jsonb_extract_path_text", IsBuiltIn = true)]
    public static string? GetCardTemplateName(
        string metadata, string level1, string level2) =>
        throw new NotSupportedException();
}

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasePaginationResponse> GetCards(
        Request.GetCardsRequest request)
    {
        request.PageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
        request.PageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var cardHistoryTypes = new[] { "card", "handmade-card" };

        var source =
            from history in _dbContext.ClientHistories.AsNoTracking()
            join card in _dbContext.GeneratedCards.AsNoTracking()
                on history.OutputId equals card.Id
            where !history.IsDeleted &&
                  !card.IsDeleted &&
                  cardHistoryTypes.Contains(history.Type) &&
                  history.UserId == card.UserId
            select new { History = history, Card = card };

        if (!string.IsNullOrWhiteSpace(request.SizeName))
        {
            var sizeName = request.SizeName.Trim().ToUpperInvariant();
            source = source.Where(x => x.Card.SizeName == sizeName);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            var keywordPattern = $"%{keyword}%";

            source = source.Where(x =>
                // PostgreSQL filter JSONB ngay trong DB, không tải metadata về C#.
                EF.Functions.ILike(
                    JsonbDbFunctions.GetCardTemplateName(
                        x.History.Metadata, "card_template", "name") ?? string.Empty,
                    keywordPattern) ||
                // EXISTS: chỉ cần có một Order của Card có tên customer khớp.
                _dbContext.OrderDetails.Any(detail =>
                    !detail.IsDeleted &&
                    detail.GeneratedCardIds != null &&
                    detail.GeneratedCardIds.Contains(x.Card.Id) &&
                    _dbContext.Orders.Any(order =>
                        !order.IsDeleted &&
                        order.Id == detail.OrderId &&
                        EF.Functions.ILike(order.FullName, keywordPattern))));
        }

        var totalCount = await source.CountAsync();

        var rows = await source
            .OrderByDescending(x => x.History.CreatedAt)
            .ThenByDescending(x => x.Card.Id)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new
            {
                HistoryId = x.History.Id,
                x.History.Metadata,
                GeneratedCardId = x.Card.Id,
                x.Card.Content,
                x.Card.ImageUrl,
                x.Card.RawImage,
                x.Card.CardType,
                x.Card.FormType,
                x.Card.SenderName,
                x.Card.ReceiverName,
                x.Card.MessageContent,
                x.Card.AttachedImageUrl,
                x.Card.SizeName,
                x.Card.SizeWidth,
                x.Card.SizeHeight,
                x.Card.SizeBasePrice,
                x.Card.SizeMaxWords,
                x.Card.WordCount,
                x.Card.WordConfigSnapshot,
                x.Card.BasePrice,
                x.Card.ExtraPrice,
                x.Card.TotalPrice,
                CreatedAt = x.History.CreatedAt
            })
            .ToListAsync();

        var selectedCardIds = rows.Select(x => x.GeneratedCardId).ToArray();

        // Chỉ lấy OrderDetail có mảng Card giao với Card của trang hiện tại.
        // Npgsql dịch Any(... Contains(...)) này thành toán tử PostgreSQL array overlap (&&).
        var orderRows = await (
                from detail in _dbContext.OrderDetails.AsNoTracking()
                join order in _dbContext.Orders.AsNoTracking()
                    on detail.OrderId equals order.Id
                where !detail.IsDeleted &&
                      !order.IsDeleted &&
                      detail.GeneratedCardIds != null &&
                      detail.GeneratedCardIds.Any(cardId =>
                          selectedCardIds.Contains(cardId))
                select new
                {
                    detail.GeneratedCardIds,
                    order.FullName,
                    order.Phone,
                    order.Address
                })
            .ToListAsync();

        var customerByCardId = orderRows
            .SelectMany(x => x.GeneratedCardIds!
                .Where(cardId => selectedCardIds.Contains(cardId))
                .Select(cardId => new
                {
                    CardId = cardId,
                    x.FullName,
                    x.Phone,
                    x.Address
                }))
            .GroupBy(x => x.CardId)
            .ToDictionary(
                x => x.Key,
                x => x.First());

        var items = rows.Select(x =>
        {
            customerByCardId.TryGetValue(x.GeneratedCardId, out var customer);

            return new Response.CardListItemResponse
            {
                HistoryId = x.HistoryId,
                GeneratedCardId = x.GeneratedCardId,
                Content = x.Content,
                ImageUrl = x.ImageUrl,
                RawImage = x.RawImage,
                CreatedAt = x.CreatedAt,
                CardType = x.CardType == GeneratedCardType.Ai ? "AI" : "Handmade",
                FormType = x.FormType.ToString(),
                SenderName = x.SenderName,
                ReceiverName = x.ReceiverName,
                NameCustomer = customer?.FullName,
                PhoneCustomer = customer?.Phone,
                AddressCustomer = customer?.Address,
                MessageContent = x.MessageContent,
                AttachedImageUrl = x.AttachedImageUrl,
                SizeName = x.SizeName,
                SizeWidth = x.SizeWidth,
                SizeHeight = x.SizeHeight,
                SizeBasePrice = x.SizeBasePrice,
                SizeMaxWords = x.SizeMaxWords,
                WordCount = x.WordCount,
                WordConfigSnapshot = x.WordConfigSnapshot,
                BasePrice = x.BasePrice,
                ExtraPrice = x.ExtraPrice,
                TotalPrice = x.TotalPrice,
                TemplateName = ReadTemplateName(x.Metadata)
            };
        }).ToList();

        return ApiResponseFactory.BasePagination(
            items, request.PageIndex, request.PageSize, totalCount);
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

## IV. Dữ liệu cần có

- `OrderDetails.GeneratedCardIds` phải là `uuid[]` và luôn chứa Card được mua.
- `generated_cards` là snapshot nguồn cho toàn bộ dữ liệu thiệp và giá.
- `Order.FullName`, `Order.Phone`, `Order.Address` là nguồn của `nameCustomer`, `phoneCustomer`, `addressCustomer`.
- `client_histories.metadata.card_template.name` là snapshot nguồn của `templateName` và search tên mẫu.

## V. Index cần có cho API này

```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE INDEX ix_order_details_generated_card_ids_gin
    ON order_details USING gin (generated_card_ids);

CREATE INDEX ix_orders_full_name_trgm
    ON orders USING gin (full_name gin_trgm_ops)
    WHERE is_deleted = false;

CREATE INDEX ix_client_histories_card_template_name_trgm
    ON client_histories USING gin
    ((metadata #>> '{card_template,name}') gin_trgm_ops)
    WHERE is_deleted = false;
```

Các index trên hỗ trợ tương ứng: tìm phần tử trong `GeneratedCardIds uuid[]`, search gần đúng `Order.FullName`, và search gần đúng tên template trong JSONB. Không có chúng thì query vẫn đúng nhưng sẽ chậm khi dữ liệu tăng.

# US-044 — Admin xem danh sách thiệp AI

## I. Các API cần làm

US-044 dùng để Admin tra cứu toàn bộ thiệp AI; không xem giá và không thay đổi dữ liệu.

Cần **1 API**:

`GET /api/v1/admin/generated-cards`

- FE gọi khi mở **AI Custom → Thiệp**, tìm kiếm, lọc hoặc chuyển trang.
- Trả danh sách mới nhất trước; mỗi `client_histories` loại `card` là một item.
- Tìm theo `generated_cards.id`, `orders.id` hoặc tên tại `generated_cards.sender_name`.
- Lọc theo Size và hình thức.

Luồng:

`Admin mở màn hình → FE gọi API danh sách → BE kiểm tra quyền → query + filter + phân trang → FE render bảng`

Lưu ý:

- Bấm **Xem chi tiết** dùng API của US-043, không tạo API trùng trong US-044.
- Lightbox dùng `imageUrl` trong item. Nếu storage tải lỗi, FE hiện placeholder; BE không gọi AI tạo lại.
- Không join thẳng Order vào danh sách vì một thiệp có thể thuộc nhiều Order và sẽ bị lặp item. Chỉ dùng `Any` để tìm theo Order.
- Mã thiệp và mã Order đều là UUID; không xử lý tiền tố `CARD-`, `ORD-`, `NhanhOrderId` hoặc `TrackNumber`.
- Route bắt buộc gắn `[Authorize(Policy = JwtExtensions.AdminPolicy)]`.

## II. Code từng API

### API 1 — Lấy danh sách thiệp AI

**Vai trò:** Trả danh sách thiệp AI có phân trang, tìm kiếm và lọc cho Admin.

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
GET /api/v1/admin/generated-cards?pageIndex=1&pageSize=10&keyword=&sizeKey=&formType=
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
        public string? SizeKey { get; set; }
        public FormType? FormType { get; set; }
    }
}
```

### Response

```csharp
namespace HoaTheoMua.Service.AdminGeneratedCard;

public class Response
{
    public class CardListItemResponse
    {
        public Guid HistoryId { get; set; }
        public Guid GeneratedCardId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public required string ImageUrl { get; set; }
        public string? TemplateName { get; set; }
        public string? SizeKey { get; set; }
        public string? SizeName { get; set; }
        public required string FormType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

### Service

```csharp
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.AdminGeneratedCard;

public class Service : IService
{
    private static readonly int[] AllowedPageSizes = [5, 10, 20, 30, 40, 50];
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasePaginationResponse> GetCards(
        Request.GetCardsRequest request)
    {
        request.PageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;

        if (!AllowedPageSizes.Contains(request.PageSize))
            throw new BadRequestException("Page size chỉ nhận 5, 10, 20, 30, 40 hoặc 50.");

        var source =
            from history in _dbContext.ClientHistories.AsNoTracking()
            join card in _dbContext.GeneratedCards.AsNoTracking()
                on history.OutputId equals card.Id
            where !history.IsDeleted
                && !card.IsDeleted
                && history.Type == GenerationType.Card
                && history.UserId == card.UserId
                && card.ImageUrl != null
                && card.ImageUrl != ""
            select new
            {
                History = history,
                Card = card
            };

        if (!string.IsNullOrWhiteSpace(request.SizeKey))
        {
            var sizeKey = request.SizeKey.Trim();
            source = source.Where(x => x.Card.SizeKey == sizeKey);
        }

        if (request.FormType.HasValue)
            source = source.Where(x => x.Card.FormType == request.FormType.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            var keywordPattern = $"%{keyword}%";
            Guid? searchedId = Guid.TryParse(keyword, out var id)
                ? id
                : null;

            source = source.Where(x =>
                (searchedId.HasValue && x.Card.Id == searchedId.Value) ||
                EF.Functions.ILike(x.Card.SenderName, keywordPattern) ||
                _dbContext.OrderDetails.Any(detail =>
                    !detail.IsDeleted &&
                    detail.GeneratedCardIds != null &&
                    detail.GeneratedCardIds.Contains(x.Card.Id) &&
                    _dbContext.Orders.Any(order =>
                        !order.IsDeleted &&
                        order.Id == detail.OrderId &&
                        searchedId.HasValue &&
                        order.Id == searchedId.Value)));
        }

        var totalCount = await source.CountAsync();

        var rows = await source
            .OrderByDescending(x => x.History.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new
            {
                HistoryId = x.History.Id,
                x.History.Metadata,
                GeneratedCardId = x.Card.Id,
                CustomerId = x.Card.UserId,
                x.Card.ImageUrl,
                x.Card.SizeKey,
                x.Card.SizeName,
                x.Card.FormType,
                CustomerName = x.Card.SenderName,
                CreatedAt = x.History.CreatedAt
            })
            .ToListAsync();

        var items = rows.Select(x => new Response.CardListItemResponse
        {
            HistoryId = x.HistoryId,
            GeneratedCardId = x.GeneratedCardId,
            CustomerId = x.CustomerId,
            CustomerName = x.CustomerName,
            ImageUrl = x.ImageUrl!,
            TemplateName = ReadTemplateName(x.Metadata),
            SizeKey = x.SizeKey,
            SizeName = x.SizeName,
            FormType = x.FormType.ToString(),
            CreatedAt = x.CreatedAt
        }).ToList();

        return ApiResponseFactory.BasePagination(
            items,
            request.PageIndex,
            request.PageSize,
            totalCount);
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

`CustomerName` và tìm kiếm tên đều dùng snapshot `generated_cards.sender_name`; Order chỉ được truy vấn khi keyword là `orders.id`.

# US-047 - Admin xem danh sach mau hoa Custom AI

## I. Cac API can lam

US-047 dung de Admin xem danh sach tat ca mau hoa Custom AI da tao; khong hien thi gia/bill va khong thay doi du lieu.

Can **1 API**:

`GET /api/v1/admin/generated-flowers`

- FE goi khi Admin mo **AI Custom > Mau hoa**, tim kiem, loc combo hoac chuyen trang.
- Tra moi `client_histories(type = flower)` thanh dung 1 item.
- Sap xep moi nhat truoc, co phan trang server.
- Loc theo mot hoac nhieu `comboIds`.
- Tim theo `generated_flowers.id`, `orders.id`, hoac ten nguoi gui trong snapshot/input.

Luong:

```text
Admin mo AI Custom > Mau hoa
-> FE goi API danh sach
-> BE query client_histories(type=flower) + generated_flowers
-> BE filter/search/paging
-> FE render bang, lightbox dung imageUrl
```

Luu y:

- Moi ket qua flower co anh hop le hien thi dung 1 item, khong bi lap theo Order.
- `comboId = client_histories.base_id`.
- `generatedFlowerId = client_histories.output_id = generated_flowers.id`.
- Combo name lay tu snapshot `client_histories.metadata.product.name` neu co; khong phu thuoc Product live.
- `customerName` trong US nay la ten nguoi gui/ten da nhap khi tao hoa, lay tu snapshot/input; khong phai User service.
- Danh sach khong tra gia/bill; gia xem o chi tiet Order.
- Bam **Xem chi tiet** dung API cua US-048.
- Route bat buoc gan `[Authorize(Policy = JwtExtensions.AdminPolicy)]`.

## II. Code tung API

### API 1 - Lay danh sach mau hoa Custom AI

**Vai tro:** Tra danh sach mau hoa AI co phan trang, tim kiem va loc Combo cho Admin.

### IService

```csharp
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.AdminGeneratedFlower;

public interface IService
{
    Task<BasePaginationResponse> GetFlowers(
        Request.GetFlowersRequest request);
}
```

### Request

```http
GET /api/v1/admin/generated-flowers?pageIndex=1&pageSize=10&keyword=&comboIds=...
Authorization: Bearer {admin-token}
```

```csharp
namespace HoaTheoMua.Service.AdminGeneratedFlower;

public class Request
{
    public class GetFlowersRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Keyword { get; set; }
        public List<Guid> ComboIds { get; set; } = [];
    }
}
```

### Response

```csharp
namespace HoaTheoMua.Service.AdminGeneratedFlower;

public class Response
{
    public class FlowerListItemResponse
    {
        public Guid HistoryId { get; set; }
        public Guid GeneratedFlowerId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ComboId { get; set; }
        public string? ComboName { get; set; }
        public string? CustomerName { get; set; }
        public required string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

Payload mau:

```json
{
  "items": [
    {
      "historyId": "1f2c0000-0000-0000-0000-000000000001",
      "generatedFlowerId": "fa000000-0000-0000-0000-000000000001",
      "customerId": "10000000-0000-0000-0000-000000000001",
      "comboId": "c1000000-0000-0000-0000-000000000001",
      "comboName": "Combo sinh nhat",
      "customerName": "Nguyen Van A",
      "imageUrl": "https://res.cloudinary.com/.../flower-01.png",
      "createdAt": "2026-09-05T10:30:00Z"
    }
  ],
  "pageIndex": 1,
  "pageSize": 10,
  "totalCount": 1
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

namespace HoaTheoMua.Service.AdminGeneratedFlower;

public class Service : IService
{
    private static readonly int[] AllowedPageSizes = [5, 10, 20, 30, 40, 50];
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasePaginationResponse> GetFlowers(
        Request.GetFlowersRequest request)
    {
        request.PageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;

        if (!AllowedPageSizes.Contains(request.PageSize))
            throw new BadRequestException("Page size chi nhan 5, 10, 20, 30, 40 hoac 50.");

        var source =
            from history in _dbContext.ClientHistories.AsNoTracking()
            join flower in _dbContext.GeneratedFlowers.AsNoTracking()
                on history.OutputId equals flower.Id
            where !history.IsDeleted &&
                  !flower.IsDeleted &&
                  history.Type == GenerationType.Flower &&
                  history.UserId == flower.UserId &&
                  flower.ImageUrl != null &&
                  flower.ImageUrl != ""
            select new
            {
                History = history,
                Flower = flower
            };

        if (request.ComboIds.Count > 0)
        {
            var comboIds = request.ComboIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            source = source.Where(x => comboIds.Contains(x.History.BaseId));
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            var keywordPattern = $"%{keyword}%";
            Guid? searchedId = Guid.TryParse(keyword, out var id)
                ? id
                : null;

            source = source.Where(x =>
                (searchedId.HasValue && x.Flower.Id == searchedId.Value) ||
                EF.Functions.ILike(x.History.Input, keywordPattern) ||
                _dbContext.OrderDetails.Any(detail =>
                    !detail.IsDeleted &&
                    detail.GeneratedFlowerIds != null &&
                    detail.GeneratedFlowerIds.Contains(x.Flower.Id) &&
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
                GeneratedFlowerId = x.Flower.Id,
                CustomerId = x.Flower.UserId,
                ComboId = x.History.BaseId,
                x.Flower.ImageUrl,
                CreatedAt = x.History.CreatedAt
            })
            .ToListAsync();

        var items = rows.Select(x => new Response.FlowerListItemResponse
        {
            HistoryId = x.HistoryId,
            GeneratedFlowerId = x.GeneratedFlowerId,
            CustomerId = x.CustomerId,
            ComboId = x.ComboId,
            ComboName = ReadComboName(x.Metadata),
            CustomerName = ReadCustomerName(x.Metadata),
            ImageUrl = x.ImageUrl!,
            CreatedAt = x.CreatedAt
        }).ToList();

        return ApiResponseFactory.BasePagination(
            items,
            request.PageIndex,
            request.PageSize,
            totalCount);
    }

    private static string? ReadComboName(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
            return null;

        try
        {
            using var json = JsonDocument.Parse(metadata);

            return json.RootElement.TryGetProperty("product", out var product) &&
                   product.TryGetProperty("name", out var name)
                ? name.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadCustomerName(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
            return null;

        try
        {
            using var json = JsonDocument.Parse(metadata);

            return json.RootElement.TryGetProperty("user_input", out var input) &&
                   input.TryGetProperty("name", out var name)
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

`CustomerName` o day la ten nguoi gui/ten request tao hoa trong snapshot, khong phai ten account user.

## III. DB con thieu

DB hien tai du cho danh sach:

```text
client_histories.base_id -> combo id
client_histories.output_id -> generated_flowers.id
client_histories.metadata -> snapshot combo/mockup/input
generated_flowers.image_url
OrderDetails.GeneratedFlowerIds
```

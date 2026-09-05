# US-045 — Customer xem lịch sử thiệp đã tạo

## I. Hiểu nhanh US

US-045 dùng để customer xem danh sách và chi tiết các thiệp AI đã tạo thành công; không generate, regenerate, tải ảnh hay gắn thiệp vào Checkout.

Hình dung đơn giản:

```text
Customer mở “Thiệp thiết kế”
→ FE lấy danh sách History có phân trang
→ Customer bấm một thiệp
→ FE lấy chi tiết thiệp kèm các Order đã sử dụng
→ Customer có thể bấm Order để sang màn Order
```

BE lấy một History item như sau:

```text
client_histories của customer
→ type = card
→ output_id = generated_cards.id
→ lấy snapshot thiệp trong generated_cards
```

Ví dụ:

```text
H01: type = card, output_id = CARD-01
H02: type = card, output_id = CARD-02

OrderDetail A generated_card_ids = [CARD-01]
OrderDetail B generated_card_ids = [CARD-01]
```

`CARD-01` chỉ xuất hiện một lần trong History; API chi tiết trả cả Order A và Order B.

Luồng FE gọi API:

1. Mở trang “Thiệp thiết kế” → gọi API 1.
2. Bấm “Xem chi tiết” → gọi API 2.
3. Bấm một Order → dùng API chi tiết Order hiện có.
4. Bấm tải ảnh → dùng API của US-041.
5. Bấm tạo lại → dùng API của US-036.

Cần 2 API:

```http
GET /api/v1/card-histories?pageIndex=1&pageSize=20
```

Lấy danh sách History thiệp của customer, mới nhất trước.

```http
GET /api/v1/card-histories/{generatedCardId}
```

Lấy toàn bộ snapshot và các Order đã sử dụng thiệp để mở màn chi tiết.

**Điểm quan trọng:**

- `client_histories.output_id` xác định chính xác `generated_cards.id`.
- `base_id` chỉ là nguồn tạo thiệp; US này không đệ quy và không group theo `base_id`.
- `userId` lấy từ JWT; mọi API đều kiểm tra ownership tại BE.
- Thiệp chưa có Order vẫn xuất hiện bình thường.
- Trạng thái Order không làm mất History.
- FE render `imageUrl` trực tiếp; ảnh storage lỗi thì hiển thị placeholder.
- Không có draft: chỉ History đã được lưu thành công mới xuất hiện.

## II. Code implement từng API

### DTO dùng chung

**Request.cs**

```csharp
namespace HoaTheoMua.Service.CardHistory;

public class Request
{
    public class PaginationRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
```

**Response.cs**

```csharp
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.CardHistory;

public class Response
{
    public class CardHistoryItemResponse
    {
        public Guid GeneratedCardId { get; set; }
        public string? ImageUrl { get; set; }
        public FormType FormType { get; set; }
        public string? SizeName { get; set; }
        public decimal? SizeWidth { get; set; }
        public decimal? SizeHeight { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class CardHistoryDetailResponse
    {
        public Guid GeneratedCardId { get; set; }
        public Guid SourceId { get; set; }
        public string? HistoryMetadata { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public FormType FormType { get; set; }
        public string? SizeKey { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string? MessageContent { get; set; }
        public string? AttachedImageUrl { get; set; }
        public string? SizeName { get; set; }
        public decimal? SizeWidth { get; set; }
        public decimal? SizeHeight { get; set; }
        public decimal? SizeBasePrice { get; set; }
        public int? SizeMaxWords { get; set; }
        public int? WordCount { get; set; }
        public string? WordConfigSnapshot { get; set; }
        public bool IsConfirmed { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? ExtraPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime GeneratedAt { get; set; }
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

### API 1 — Lấy danh sách History thiệp

**Vai trò:** trả bảng History khi customer mở “Thiệp thiết kế”.

**Request**

```http
GET /api/v1/card-histories?pageIndex=1&pageSize=20
Authorization: Bearer {customer-token}
```

**Response payload**

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "items": [
      {
        "generatedCardId": "ca000000-0000-0000-0000-000000000001",
        "imageUrl": "https://res.cloudinary.com/.../card-01.png",
        "formType": "calligraphy",
        "sizeName": "10x15",
        "sizeWidth": 10,
        "sizeHeight": 15,
        "generatedAt": "2026-09-05T10:30:00Z"
      }
    ],
    "pageIndex": 1,
    "pageSize": 20,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

Không có History thì trả `200 OK` với `items: []`.

**IService**

```csharp
Task<BasePaginationResponse> GetAll(
    Request.PaginationRequest request);
```

**Service**

```csharp
public async Task<BasePaginationResponse> GetAll(
    Request.PaginationRequest request)
{
    ValidatePagination(request);
    var userId = _currentUser.GetRequiredUserId();

    var histories = _dbContext.ClientHistories
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.Type == GenerationType.Card &&
            !x.IsDeleted);

    var cards = _dbContext.GeneratedCards
        .AsNoTracking()
        .Where(x => x.UserId == userId && !x.IsDeleted);

    var query = (
        from history in histories
        join card in cards
            on history.OutputId equals card.Id
        select new Response.CardHistoryItemResponse
        {
            GeneratedCardId = card.Id,
            ImageUrl = card.ImageUrl,
            FormType = card.FormType,
            SizeName = card.SizeName,
            SizeWidth = card.SizeWidth,
            SizeHeight = card.SizeHeight,
            GeneratedAt = card.CreatedAt
        }).Distinct();

    var totalCount = await query.CountAsync();
    var items = await query
        .OrderByDescending(x => x.GeneratedAt)
        .ThenByDescending(x => x.GeneratedCardId)
        .Skip((request.PageIndex - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync();

    return ApiResponseFactory.BasePagination(
        items,
        request.PageIndex,
        request.PageSize,
        totalCount);
}
```

API này không join `OrderDetails`, nên một thiệp dùng cho nhiều Order vẫn chỉ có một History item.

### API 2 — Lấy chi tiết một thiệp History

**Vai trò:** trả ảnh, toàn bộ snapshot và các Order liên quan khi customer bấm “Xem chi tiết”.

**Request**

```http
GET /api/v1/card-histories/ca000000-0000-0000-0000-000000000001
Authorization: Bearer {customer-token}
```

`generatedCardId` là route parameter đơn giản nên không cần Request DTO.

**Response payload rút gọn**

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "generatedCardId": "ca000000-0000-0000-0000-000000000001",
    "sourceId": "c1000000-0000-0000-0000-000000000000",
    "imageUrl": "https://res.cloudinary.com/.../card-01.png",
    "formType": "calligraphy",
    "senderName": "An",
    "receiverName": "Bình",
    "messageContent": "Chúc mừng sinh nhật",
    "sizeName": "10x15",
    "wordCount": 4,
    "totalPrice": 30000,
    "generatedAt": "2026-09-05T10:30:00Z",
    "orders": [
      {
        "orderId": "0a000000-0000-0000-0000-000000000001",
        "nhanhOrderId": 123456,
        "status": "Paid",
        "createdAt": "2026-09-05T11:00:00Z"
      }
    ]
  }
}
```

**IService**

```csharp
Task<Response.CardHistoryDetailResponse> GetById(
    Guid generatedCardId);
```

**Service**

```csharp
public async Task<Response.CardHistoryDetailResponse> GetById(
    Guid generatedCardId)
{
    if (generatedCardId == Guid.Empty)
    {
        throw new BadRequestException(
            "GeneratedCardId không hợp lệ");
    }

    var userId = _currentUser.GetRequiredUserId();

    var result = await (
        from history in _dbContext.ClientHistories.AsNoTracking()
        join card in _dbContext.GeneratedCards.AsNoTracking()
            on history.OutputId equals card.Id
        where history.OutputId == generatedCardId &&
              history.UserId == userId &&
              history.Type == GenerationType.Card &&
              !history.IsDeleted &&
              card.UserId == userId &&
              !card.IsDeleted
        select new Response.CardHistoryDetailResponse
        {
            GeneratedCardId = card.Id,
            SourceId = history.BaseId,
            HistoryMetadata = history.Metadata,
            Content = card.Content,
            ImageUrl = card.ImageUrl,
            FormType = card.FormType,
            SizeKey = card.SizeKey,
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
            IsConfirmed = card.IsConfirmed,
            BasePrice = card.BasePrice,
            ExtraPrice = card.ExtraPrice,
            TotalPrice = card.TotalPrice,
            GeneratedAt = card.CreatedAt
        })
        .FirstOrDefaultAsync();

    if (result == null)
    {
        throw new NotFoundException(
            "Không tìm thấy lịch sử thiệp");
    }

    result.Orders = await (
            from detail in _dbContext.OrderDetails.AsNoTracking()
            join order in _dbContext.Orders.AsNoTracking()
                on detail.OrderId equals order.Id
            where order.UserId == userId &&
                  !order.IsDeleted &&
                  !detail.IsDeleted &&
                  detail.GeneratedCardIds.Contains(generatedCardId)
            select new Response.RelatedOrderResponse
            {
                OrderId = order.Id,
                NhanhOrderId = order.NhanhOrderId,
                Status = order.Status,
                CreatedAt = order.CreatedAt
            })
        .Distinct()
        .OrderByDescending(x => x.CreatedAt)
        .ThenByDescending(x => x.OrderId)
        .ToListAsync();

    return result;
}
```

Lọc ownership ngay trong query nên ID thuộc customer khác không trả ảnh, metadata hoặc Order liên quan. Thiệp chưa dùng cho Order nào trả `orders: []`.

`GeneratedCardIds` là cột `uuid[]`, vì vậy không cần deserialize hoặc truy vấn JSON.

### IService.cs hoàn chỉnh

```csharp
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.CardHistory;

public interface IService
{
    Task<BasePaginationResponse> GetAll(
        Request.PaginationRequest request);

    Task<Response.CardHistoryDetailResponse> GetById(
        Guid generatedCardId);
}
```

### Thành phần dùng chung trong Service.cs

```csharp
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.CardHistory;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public Service(
        AppDbContext dbContext,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    // Đặt 2 method phía trên tại đây.

    private static void ValidatePagination(
        Request.PaginationRequest request)
    {
        if (request.PageIndex < 1 ||
            request.PageSize < 1 ||
            request.PageSize > 100)
        {
            throw new BadRequestException(
                "PageIndex phải từ 1 và PageSize phải từ 1 đến 100");
        }
    }
}
```

## III. DB còn thiếu

Thay hai field đơn hiện tại bằng hai cột mảng trong `OrderDetails`:

```text
GeneratedCardIds uuid[] [not null, default: '{}']
GeneratedFlowerIds uuid[] [not null, default: '{}']
```

- `GeneratedCardIds`: các thiệp được dùng trong OrderDetail.
- `GeneratedFlowerIds`: các ảnh hoa AI được dùng trong OrderDetail.
- `Metadata`: chỉ giữ snapshot combo, giá, nguyên liệu và dữ liệu cố định tại lúc đặt hàng; không dùng để chứa hai danh sách ID trên.

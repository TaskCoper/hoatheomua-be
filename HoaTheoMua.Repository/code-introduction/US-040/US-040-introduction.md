# US-040 — Xem lịch sử mẫu hoa Custom AI

## I. Hiểu nhanh US

US-040 chỉ đọc lịch sử hoa AI của customer, không tạo/re-generate ảnh, Checkout hay Order.

1. `GET /api/v1/flower-histories?pageIndex=1&pageSize=20`: lấy tất cả ảnh, mới nhất trước.
2. `GET /api/v1/flower-histories/{generatedFlowerId}/root-group`: lấy mọi ảnh cùng Combo nguồn; mỗi ảnh kèm Order đã dùng chính ảnh đó.
3. `GET /api/v1/flower-histories/{generatedFlowerId}/session`: lấy các version của một phiên generate/regenerate.

```text
Root     = Combo nguồn chung.
BaseId   = nguồn trực tiếp của lần generate.
OutputId = Generated Flower tạo ra.

CA → GF1 → GF2 → GF3  : một phiên.
CA → GF4               : phiên generate mới, cùng Combo CA.
```

## II. DTO dùng chung

```csharp
public class Response
{
    public class FlowerHistoryItemResponse
    {
        public Guid GeneratedFlowerId { get; set; }
        public Guid ComboId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
    }

    public class FlowerRootGroupResponse
    {
        public Guid RootComboId { get; set; }
        public List<FlowerRootGroupItemResponse> Items { get; set; } = new();
    }

    public class FlowerRootGroupItemResponse
    {
        public Guid GeneratedFlowerId { get; set; }
        public Guid BaseId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
        public List<FlowerOrderResponse> Orders { get; set; } = new();
    }

    public class FlowerOrderResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public decimal TotalFee { get; set; }
    }

    public class FlowerSessionResponse
    {
        public Guid RootComboId { get; set; }
        public Guid SessionStartGeneratedFlowerId { get; set; }
        public List<FlowerVersionResponse> Items { get; set; } = new();
    }

    public class FlowerVersionResponse
    {
        public Guid GeneratedFlowerId { get; set; }
        public Guid BaseId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
    }
}
```

## III. API 1 — Lấy toàn bộ History

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
Authorization: Bearer {customer-token}
```

`comboId` lấy từ `history.Root`, không lấy `history.BaseId`.

```csharp
public async Task<BasePaginationResponse> GetAll(
    Request.GetFlowerHistoriesRequest request)
{
    request.PageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
    request.PageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

    var userId = _currentUser.GetRequiredUserId();
    var query =
        from history in _dbContext.ClientHistories.AsNoTracking()
        join flower in _dbContext.GeneratedFlowers.AsNoTracking()
            on history.OutputId equals flower.Id
        where history.UserId == userId &&
              history.Type == GenerationType.Flower &&
              !history.IsDeleted &&
              flower.UserId == userId &&
              !flower.IsDeleted
        select new Response.FlowerHistoryItemResponse
        {
            GeneratedFlowerId = flower.Id,
            ComboId = history.Root,
            ImageUrl = flower.ImageUrl,
            GeneratedAt = flower.CreatedAt
        };

    var totalCount = await query.CountAsync();
    var items = await query
        .OrderByDescending(x => x.GeneratedAt)
        .ThenByDescending(x => x.GeneratedFlowerId)
        .Skip((request.PageIndex - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync();

    return ApiResponseFactory.BasePagination(
        items, request.PageIndex, request.PageSize, totalCount);
}
```

## IV. API 2 — Lấy nhóm ảnh cùng Combo nguồn

```http
GET /api/v1/flower-histories/f2000000-0000-0000-0000-000000000002/root-group
Authorization: Bearer {customer-token}
```

Mỗi `orders` thuộc đúng ảnh ở item đó, qua quan hệ:

```text
OrderDetails.GeneratedFlower = GeneratedFlower.Id
```

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "rootComboId": "ca000000-0000-0000-0000-000000000001",
    "items": [
      {
        "generatedFlowerId": "f4000000-0000-0000-0000-000000000004",
        "baseId": "ca000000-0000-0000-0000-000000000001",
        "imageUrl": "https://res.cloudinary.com/demo/gf4.png",
        "generatedAt": "2026-09-06T09:00:00Z",
        "orders": []
      },
      {
        "generatedFlowerId": "f3000000-0000-0000-0000-000000000003",
        "baseId": "f2000000-0000-0000-0000-000000000002",
        "imageUrl": "https://res.cloudinary.com/demo/gf3.png",
        "generatedAt": "2026-09-05T11:00:00Z",
        "orders": [
          {
            "orderId": "0a000000-0000-0000-0000-000000000010",
            "status": "Paid",
            "createdAt": "2026-09-06T10:00:00Z",
            "totalFee": 520000
          }
        ]
      },
      {
        "generatedFlowerId": "f2000000-0000-0000-0000-000000000002",
        "baseId": "f1000000-0000-0000-0000-000000000001",
        "imageUrl": "https://res.cloudinary.com/demo/gf2.png",
        "generatedAt": "2026-09-05T10:30:00Z",
        "orders": []
      }
    ]
  }
}
```

```csharp
public async Task<Response.FlowerRootGroupResponse> GetRootGroup(
    Guid generatedFlowerId)
{
    var userId = _currentUser.GetRequiredUserId();

    var source = await _dbContext.ClientHistories
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.Type == GenerationType.Flower &&
            x.OutputId == generatedFlowerId &&
            !x.IsDeleted)
        .Select(x => new { x.Root })
        .FirstOrDefaultAsync();

    if (source == null || source.Root == Guid.Empty)
    {
        throw new NotFoundException("Không tìm thấy lịch sử mẫu hoa");
    }

    var flowerRows = await (
            from history in _dbContext.ClientHistories.AsNoTracking()
            join flower in _dbContext.GeneratedFlowers.AsNoTracking()
                on history.OutputId equals flower.Id
            where history.UserId == userId &&
                  history.Type == GenerationType.Flower &&
                  history.Root == source.Root &&
                  !history.IsDeleted &&
                  flower.UserId == userId &&
                  !flower.IsDeleted
            orderby flower.CreatedAt descending, flower.Id descending
            select new
            {
                GeneratedFlowerId = flower.Id,
                BaseId = history.BaseId,
                ImageUrl = flower.ImageUrl,
                GeneratedAt = flower.CreatedAt
            })
        .ToListAsync();

    var generatedFlowerIds = flowerRows
        .Select(x => x.GeneratedFlowerId)
        .ToList();

    var orderRows = await (
            from detail in _dbContext.OrderDetails.AsNoTracking()
            join order in _dbContext.Orders.AsNoTracking()
                on detail.OrderId equals order.Id
            where detail.GeneratedFlower.HasValue &&
                  generatedFlowerIds.Contains(detail.GeneratedFlower.Value) &&
                  !detail.IsDeleted &&
                  order.UserId == userId &&
                  !order.IsDeleted
            select new
            {
                GeneratedFlowerId = detail.GeneratedFlower.Value,
                OrderId = order.Id,
                order.Status,
                order.CreatedAt,
                order.TotalFee
            })
        .ToListAsync();

    var ordersByFlowerId = orderRows
        .GroupBy(x => x.GeneratedFlowerId)
        .ToDictionary(
            group => group.Key,
            group => group
                .GroupBy(x => x.OrderId)
                .Select(order => order.First())
                .OrderByDescending(order => order.CreatedAt)
                .ThenByDescending(order => order.OrderId)
                .Select(order => new Response.FlowerOrderResponse
                {
                    OrderId = order.OrderId,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    TotalFee = order.TotalFee
                })
                .ToList());

    var items = flowerRows
        .Select(flower => new Response.FlowerRootGroupItemResponse
        {
            GeneratedFlowerId = flower.GeneratedFlowerId,
            BaseId = flower.BaseId,
            ImageUrl = flower.ImageUrl,
            GeneratedAt = flower.GeneratedAt,
            Orders = ordersByFlowerId.TryGetValue(
                flower.GeneratedFlowerId, out var orders)
                ? orders
                : new List<Response.FlowerOrderResponse>()
        })
        .ToList();

    return new Response.FlowerRootGroupResponse
    {
        RootComboId = source.Root,
        Items = items
    };
}
```

Không query theo `OrderDetails.ProductId = rootComboId`: điều đó chỉ chứng minh Order mua Combo, không chứng minh Order đã dùng ảnh hoa AI nào.

## V. API 3 — Lấy version trong một phiên generate/regenerate

```http
GET /api/v1/flower-histories/f2000000-0000-0000-0000-000000000002/session
Authorization: Bearer {customer-token}
```

FE gửi `GF2` trong chuỗi `GF1 → GF2 → GF3` thì API trả `GF1, GF2, GF3`; không trả `GF4`.

```csharp
private sealed class FlowerHistoryRow
{
    public Guid RootComboId { get; init; }
    public Guid BaseId { get; init; }
    public Guid GeneratedFlowerId { get; init; }
    public string? ImageUrl { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
}

public async Task<Response.FlowerSessionResponse> GetSession(Guid generatedFlowerId)
{
    var userId = _currentUser.GetRequiredUserId();

    var selected = await (
            from history in _dbContext.ClientHistories.AsNoTracking()
            join flower in _dbContext.GeneratedFlowers.AsNoTracking()
                on history.OutputId equals flower.Id
            where history.UserId == userId &&
                  history.Type == GenerationType.Flower &&
                  history.OutputId == generatedFlowerId &&
                  !history.IsDeleted &&
                  flower.UserId == userId &&
                  !flower.IsDeleted
            select new FlowerHistoryRow
            {
                RootComboId = history.Root,
                BaseId = history.BaseId,
                GeneratedFlowerId = flower.Id,
                ImageUrl = flower.ImageUrl,
                GeneratedAt = flower.CreatedAt
            })
        .FirstOrDefaultAsync();

    if (selected == null || selected.RootComboId == Guid.Empty)
    {
        throw new NotFoundException("Không tìm thấy lịch sử mẫu hoa");
    }

    var rows = await (
            from history in _dbContext.ClientHistories.AsNoTracking()
            join flower in _dbContext.GeneratedFlowers.AsNoTracking()
                on history.OutputId equals flower.Id
            where history.UserId == userId &&
                  history.Type == GenerationType.Flower &&
                  history.Root == selected.RootComboId &&
                  !history.IsDeleted &&
                  flower.UserId == userId &&
                  !flower.IsDeleted
            select new FlowerHistoryRow
            {
                RootComboId = history.Root,
                BaseId = history.BaseId,
                GeneratedFlowerId = flower.Id,
                ImageUrl = flower.ImageUrl,
                GeneratedAt = flower.CreatedAt
            })
        .ToListAsync();

    var previousRows = new List<FlowerHistoryRow>();
    var current = selected;
    var visited = new HashSet<Guid> { selected.GeneratedFlowerId };

    while (current.BaseId != current.RootComboId)
    {
        var parent = rows.SingleOrDefault(x => x.GeneratedFlowerId == current.BaseId);
        if (parent == null || !visited.Add(parent.GeneratedFlowerId))
        {
            throw new BadRequestException("Chuỗi tạo mẫu hoa không hợp lệ");
        }

        previousRows.Add(parent);
        current = parent;
    }

    previousRows.Reverse();
    var sessionRows = new List<FlowerHistoryRow>(previousRows) { selected };
    current = selected;

    while (true)
    {
        var next = rows.SingleOrDefault(x => x.BaseId == current.GeneratedFlowerId);
        if (next == null) break;
        if (!visited.Add(next.GeneratedFlowerId))
            throw new BadRequestException("Chuỗi tạo mẫu hoa không hợp lệ");

        sessionRows.Add(next);
        current = next;
    }

    return new Response.FlowerSessionResponse
    {
        RootComboId = selected.RootComboId,
        SessionStartGeneratedFlowerId = sessionRows[0].GeneratedFlowerId,
        Items = sessionRows.Select(x => new Response.FlowerVersionResponse
        {
            GeneratedFlowerId = x.GeneratedFlowerId,
            BaseId = x.BaseId,
            ImageUrl = x.ImageUrl,
            GeneratedAt = x.GeneratedAt
        }).ToList()
    };
}
```

## VI. IService.cs

```csharp
public interface IService
{
    Task<BasePaginationResponse> GetAll(Request.GetFlowerHistoriesRequest request);
    Task<Response.FlowerRootGroupResponse> GetRootGroup(Guid generatedFlowerId);
    Task<Response.FlowerSessionResponse> GetSession(Guid generatedFlowerId);
}
```

## VII. Dữ liệu cần đồng bộ trước khi implement

- Entity `ClientHistory` cần có mapping `Root`.
- Generate đầu tiên lưu `baseId = root = comboId`.
- Regenerate lưu `root` kế thừa Combo nguồn và `baseId = generatedFlowerId` trực tiếp được regenerate.
- Khi customer đặt mẫu hoa AI, `OrderDetails.GeneratedFlower` phải lưu `GeneratedFlower.Id` đã chọn.

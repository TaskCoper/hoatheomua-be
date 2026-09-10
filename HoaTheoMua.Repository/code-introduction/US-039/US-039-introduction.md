# US-039 — Cộng giá thiệp đã tạo vào CreateOrder

## I. Các API cần làm

US-039 mở rộng API tạo Order hiện có để nhận danh sách thiệp đã tạo kèm số lượng trong từng dòng `items`, lấy `generated_cards.total_price` và cộng vào bill. Không tạo lại Card và không tính lại giá Card từ Config.

Cần 1 API hiện có:

1. `POST /api/v1/orders`
   - FE gọi khi customer xác nhận tạo Order.
   - Mỗi item có thể gửi `generatedFlowerId` và `generatedCards[]`.
   - Mỗi phần tử `generatedCards[]` bắt buộc có `generatedCardId` và `quantity`.
   - Service chỉ nhận Card thuộc đúng customer và có Product gốc trùng `items[].productId`; sau đó lấy giá snapshot, nhân số lượng, cộng bill và lưu snapshot Card–OrderDetail.

```text
Customer xác nhận Checkout
→ FE gửi từng item hoa và danh sách thiệp/số lượng của item đó
→ CreateOrder lấy total_price của các Card
→ CreateOrder cộng tiền hoa + tiền thiệp
→ CreateOrder lưu Order, OrderDetails, OrderDetailCards
→ Payment dùng TotalFee đã bao gồm giá Card
```

## II. Code từng API

### API 1 — Tạo Order có thiệp đã tạo

**Vai trò:** lấy `total_price` snapshot của Card và nhân đúng `quantity` FE gửi.

### IService

`IService.CreateOrder` giữ nguyên chữ ký; không cần API/service public mới.

```csharp
Task<CreateOrderResponse> CreateOrder(Request.CreateOrderRequest request);
```

### Request

```http
POST /api/v1/orders
Authorization: Bearer {customer-token}
Content-Type: application/json
```

`OrderDetails.Number` là số lượng hoa, không phải số lượng thiệp. Vì một dòng hoa có thể không có thiệp, hoặc có nhiều loại thiệp với số lượng khác nhau, request phải mang collection `GeneratedCards`.

```csharp
// HoaTheoMua.Service/AhamoveService/AhaMoveRequest.cs
public class Items
{
    public int ItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Number { get; set; }

    public Guid? GeneratedFlowerId { get; set; }
    public List<GeneratedCardItem> GeneratedCards { get; set; } = new();
}

public class GeneratedCardItem
{
    public Guid GeneratedCardId { get; set; }
    public int Quantity { get; set; }
}
```

Các field Card chỉ là dữ liệu Order nội bộ; mapper gọi Ahamove tiếp tục chỉ dùng `ItemId`, `ProductId`, `Number` như hiện tại.

Ví dụ: customer mua 2 bó hoa Product `123`, trong đó cần 2 bản của Card A và 1 bản Card B; Product `456` không có thiệp.

```json
{
  "gateway": "Sepay",
  "paymentGateway": "Sepay",
  "shippingGateway": "Ahamove",
  "paymentMethod": "VA",
  "latitude": 10.7769,
  "longitude": 106.7009,
  "ward": 1,
  "city": 79,
  "address": "1 Nguyễn Huệ, Quận 1, TP.HCM",
  "shortAddress": "Quận 1, TP.HCM",
  "fullName": "Nguyễn Văn A",
  "phone": "0900000000",
  "remarks": "",
  "orderTime": 0,
  "phoneReceiver": "0900000000",
  "fullNameReceiver": "Nguyễn Văn A",
  "isExportInvoice": false,
  "items": [
    {
      "itemId": 123,
      "productId": "11111111-1111-1111-1111-111111111111",
      "number": 2,
      "generatedFlowerId": "22222222-2222-2222-2222-222222222222",
      "generatedCards": [
        {
          "generatedCardId": "33333333-3333-3333-3333-333333333333",
          "quantity": 2
        },
        {
          "generatedCardId": "44444444-4444-4444-4444-444444444444",
          "quantity": 1
        }
      ]
    },
    {
      "itemId": 456,
      "productId": "55555555-5555-5555-5555-555555555555",
      "number": 1,
      "generatedFlowerId": null,
      "generatedCards": []
    }
  ]
}
```

`generatedCardId` là `generated_cards.id`, không phải `generated_flowers.id`. Card AI, HandMade và Card chọn từ History đều dùng bảng `generated_cards`.

#### Rule bắt buộc giữa `generatedCards[]` và `productId`

Mỗi `generatedCardId` phải có `client_histories` của chính customer với `type = "card"` hoặc `"handmade_card"`, `output_id = generatedCardId`, và `root = items[].productId`.

```text
generated_cards.id = generatedCardId
→ client_histories.output_id = generatedCardId
→ client_histories.root phải bằng item.productId
```

Điều này áp dụng cho cả hai nguồn tạo Card:

- Card tạo trực tiếp từ Product: `history.base_id = history.root = productId`.
- Card tạo từ Generated Flower: `history.base_id = generatedFlowerId`, nhưng `history.root` vẫn là Product ID gốc và phải bằng `item.productId`.

Vì vậy, Card của Product khác bị từ chối. Rule hiện tại đối chiếu Product gốc qua `root`; không bắt buộc Card phải được tạo từ chính `item.generatedFlowerId` nếu hai Card/Flower có cùng Product gốc.

Thêm validation vào `CreateOrderRequestValidator`:

```csharp
RuleForEach(x => x.Items).ChildRules(item =>
{
    item.RuleForEach(x => x.GeneratedCards).ChildRules(card =>
    {
        card.RuleFor(x => x.GeneratedCardId)
            .NotEmpty()
            .WithMessage("GENERATED_CARD_ID_REQUIRED");

        card.RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("GENERATED_CARD_QUANTITY_MUST_BE_GREATER_THAN_ZERO");
    });
});
```

### Response

Response `CreateOrderResponse` giữ nguyên. `TotalFee` và payment amount trả về đã bao gồm giá Card.

### Service

Thêm record và hàm private sau vào `HoaTheoMua.Service/Order/Service.cs`. Hàm chỉ đọc snapshot `GeneratedCard.TotalPrice`, không đọc `Config`/`CardTemplate` và không tính lại giá thiệp.

```csharp
private sealed record GeneratedCardPriceSnapshot(
    int ItemId,
    Guid GeneratedCardId,
    int Quantity,
    decimal UnitPrice)
{
    public decimal TotalPrice => UnitPrice * Quantity;
}

private async Task<IReadOnlyList<GeneratedCardPriceSnapshot>>
    GetGeneratedCardPriceSnapshotsAsync(
        IReadOnlyCollection<AhaMoveRequest.Items> items,
        Guid userId)
{
    var requestedCards = items
        .SelectMany(item => item.GeneratedCards.Select(card => new
        {
            item.ItemId,
            item.ProductId,
            card.GeneratedCardId,
            card.Quantity
        }))
        .ToList();

    if (requestedCards.Count == 0)
    {
        return Array.Empty<GeneratedCardPriceSnapshot>();
    }

    if (requestedCards.Any(x => x.GeneratedCardId == Guid.Empty || x.Quantity <= 0))
    {
        throw new BadRequestException("GENERATED_CARD_REQUEST_INVALID");
    }

    if (requestedCards
        .GroupBy(x => new { x.ItemId, x.GeneratedCardId })
        .Any(group => group.Count() > 1))
    {
        throw new BadRequestException("GENERATED_CARD_DUPLICATED_IN_ITEM");
    }

    var generatedCardIds = requestedCards
        .Select(x => x.GeneratedCardId)
        .Distinct()
        .ToList();

    var cards = await _dbContext.GeneratedCards
        .AsNoTracking()
        .Where(x => generatedCardIds.Contains(x.Id) && x.UserId == userId)
        .Select(x => new { x.Id, x.TotalPrice })
        .ToListAsync();

    if (cards.Count != generatedCardIds.Count)
    {
        throw new BadRequestException("GENERATED_CARD_NOT_FOUND");
    }

    var cardHistories = await _dbContext.ClientHistories
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            generatedCardIds.Contains(x.OutputId) &&
            (x.Type == "card" || x.Type == "handmade_card"))
        .Select(x => new { x.OutputId, x.Root })
        .ToListAsync();

    if (cardHistories.Count != generatedCardIds.Count)
    {
        throw new BadRequestException("GENERATED_CARD_HISTORY_NOT_FOUND");
    }

    var rootProductIdByCardId = cardHistories
        .ToDictionary(x => x.OutputId, x => x.Root);

    if (requestedCards.Any(x =>
        rootProductIdByCardId[x.GeneratedCardId] == Guid.Empty ||
        rootProductIdByCardId[x.GeneratedCardId] != x.ProductId))
    {
        throw new BadRequestException("GENERATED_CARD_PRODUCT_MISMATCH");
    }

    var unitPriceByCardId = cards.ToDictionary(x => x.Id, x => x.TotalPrice);

    return requestedCards
        .Select(x => new GeneratedCardPriceSnapshot(
            x.ItemId,
            x.GeneratedCardId,
            x.Quantity,
            unitPriceByCardId[x.GeneratedCardId]))
        .ToList();
}
```

Trong `CreateOrder`, gọi hàm sau `EnsureItemsInStockAsync(...)`. `generatedCardFee` là tổng `total_price × quantity` của tất cả thiệp trong request.

```csharp
var comboSnapshots = await EnsureItemsInStockAsync(request.Items);

var generatedCardPriceSnapshots = await GetGeneratedCardPriceSnapshotsAsync(
    request.Items,
    userId);

var generatedCardFee = generatedCardPriceSnapshots.Sum(x => x.TotalPrice);

var companyAddress = await _dbContext.CompanyAddresses
    .Where(x => x.IsDeleted == false)
    .FirstOrDefaultAsync();

var totalShippingFee = await EstimateShippingFeeAsync(
    request,
    products,
    quantityByItemId,
    companyAddress);

var productFee = products.Sum(x => x.Prices.Retail * quantityByItemId[x.Id]);
var totalProductFee = productFee + generatedCardFee;
var vatFee = string.IsNullOrEmpty(request.TaxCode) ? 0m : totalProductFee * VatRate;
```

`newOrder` dùng biến tổng mới:

```csharp
ProductFee = totalProductFee,
TotalFee = totalProductFee + totalShippingFee + vatFee,
```

`GeneratedFlower` vẫn là một field của `OrderDetails`, vì một dòng hoa chỉ chọn tối đa một mẫu hoa AI. Không ghi `GeneratedCard` vào `OrderDetails` nữa: một dòng hoa nay có collection Card nên phải lưu ở `OrderDetailCards`.

```csharp
var requestItemByItemId = request.Items.ToDictionary(x => x.ItemId);

var newOrderDetails = products.Select(x =>
{
    comboSnapshots.TryGetValue(x.Id, out var metadata);
    var requestItem = requestItemByItemId[x.Id];

    return new OrderDetails
    {
        Id = Guid.NewGuid(),
        OrderId = newOrder.Id,
        ProductId = productIdByItemId[x.Id],
        Name = x.Name,
        ItemId = x.Id,
        Price = (int)x.Prices.Retail,
        Number = quantityByItemId[x.Id],
        Height = x.Shipping.Height,
        Weight = x.Shipping.Weight,
        Length = x.Shipping.Length,
        Width = x.Shipping.Width,
        GeneratedFlower = requestItem.GeneratedFlowerId,
        Metadata = metadata == null
            ? null
            : JsonConvert.SerializeObject(metadata),
        CreatedAt = DateTime.UtcNow
    };
}).ToList();

var orderDetailIdByItemId = newOrderDetails.ToDictionary(x => x.ItemId, x => x.Id);

var newOrderDetailCards = generatedCardPriceSnapshots
    .Select(x => new OrderDetailCard
    {
        Id = Guid.NewGuid(),
        OrderDetailId = orderDetailIdByItemId[x.ItemId],
        GeneratedCardId = x.GeneratedCardId,
        Quantity = x.Quantity,
        UnitPrice = x.UnitPrice,
        TotalPrice = x.TotalPrice,
        CreatedAt = DateTime.UtcNow
    })
    .ToList();
```

Ở cả nhánh BaoKim và SePay, lưu thêm `OrderDetailCards` trong cùng lần save với Order và OrderDetails:

```csharp
await _dbContext.Orders.AddAsync(newOrder);
await _dbContext.OrderDetails.AddRangeAsync(newOrderDetails);
await _dbContext.OrderDetailCards.AddRangeAsync(newOrderDetailCards);
await _dbContext.SaveChangesAsync();
```

Ví dụ Card A có `total_price = 15_000`, `quantity = 2`; Card B có `total_price = 39_000`, `quantity = 1`. Hàm trả `generatedCardFee = 69_000`.

## III. DB còn thiếu — cần thêm để lưu số lượng thiệp

Hai field hiện có trên `OrderDetails` không thể biểu diễn nhiều Card và quantity. Giữ `GeneratedFlower`; thay field `GeneratedCard` bằng bảng con sau:

```csharp
public class OrderDetailCard
{
    public Guid Id { get; set; }
    public Guid OrderDetailId { get; set; }
    public Guid GeneratedCardId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

Thêm `DbSet<OrderDetailCard> OrderDetailCards` vào `AppDbContext`, mapping `order_detail_cards`, FK `order_detail_id → order_details.id`, và migration. `UnitPrice`/`TotalPrice` là snapshot tại thời điểm Order, để bill cũ không bị thay đổi nếu dữ liệu Card về sau thay đổi.

`generated_cards.total_price` là nguồn để tính giá; `generated_flowers` không có giá thiệp.

Rule đối chiếu Card–Product cần `client_histories.root`. Entity `ClientHistory` hiện tại chưa có property/mapping `Root`, nên cần bổ sung `Guid Root` và mapping `root` trước khi dùng đoạn validate trên.

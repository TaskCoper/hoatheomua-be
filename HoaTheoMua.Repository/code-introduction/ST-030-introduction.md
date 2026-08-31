# ST-030 — Khởi tạo mẫu hoa

## 0. Phạm vi

1. Khách hàng chọn một Combo sáng tạo.
2. Nhập Tên dài 1–100 ký tự sau khi trim và xem thông tin Combo nguồn.
3. Chọn đúng một Mockup; tối đa 4 Mockup/trang.
4. Nhấn **Hoàn thành**: backend kiểm tra lại Combo và Mockup.
5. Tạo `generated_flowers` trạng thái `draft` và một `client_histories` tương ứng.
6. Mở Chi tiết bản Draft; STORY-030 **chưa gọi AI**.

Quy ước đã chốt:

```text
Lần đầu:      base_id = F01, output_id = F01
Regenerate 1: base_id = F01, output_id = F02
Regenerate 2: base_id = F01, output_id = F03
```

- `base_id`: ID `generated_flowers` bản gốc, dùng để gom nhóm.
- `output_id`: ID output của đúng phiên bản hiện tại.
- `type = flower`: tra `output_id` trong `generated_flowers`.
- Không khai báo FK polymorphic từ `client_histories.output_id`.

## I. Nguồn dữ liệu và code có sẵn

| Nguồn | Trạng thái trong solution | Dùng cho ST-030 |
|---|---|---|
| `Products` | Có Entity, DbSet, service và API | Danh sách/chi tiết Combo, tồn kho. |
| `ComboSpecifications` | Có Entity, DbSet, service và API | Thành phần Core/Support của Combo. |
| `mockup` | Có trong `db_tong.md`, chưa có Entity/DbSet/service | Danh sách và kiểm tra Mockup. |
| `generated_flowers` | Có trong `db_tong.md`, chưa có Entity/DbSet/service | Lưu Draft và các output. |
| `client_histories` | Có trong `db_tong.md`, chưa có Entity/DbSet/service | Lưu `type`, `base_id`, `output_id`. |

Các hàm Product **đang tồn tại**:

```csharp
// HoaTheoMua.Service/Product/IService.cs
Task<BasePaginationResponse> GetLocalProductsV2(
    Request.LocalProductQueryRequest request);

Task<Response.GetProductDetailByCustomerResponse>
    GetProductDetailByCustomer(Guid id);

Task<Response.ComboSpecificationResponse?>
    GetComboSpecificationAsync(Guid id);
```

`GetLimitCoreProduct(Guid comboId)` chỉ có trong `Order.Service`, chưa có trong `Order.IService` và chưa có endpoint. ST-030 không gọi hàm này.

## II. API cần có

| # | Trạng thái | API | Mục đích |
|---|---|---|---|
| 1 | Có sẵn | `GET /api/v2/products` | Lấy danh sách Combo sáng tạo. |
| 2 | Có sẵn | `GET /api/v2/products/{id}/detail` | Lấy chi tiết và `SellableQuantity` của Combo. |
| 3 | Có sẵn | `GET /api/v1/products/{id}/combo-specification` | Lấy Product thành phần của Combo. |
| 4 | Cần thêm | `GET /api/v1/mockups` | Lấy Mockup Active, chưa xóa. |
| 5 | Cần thêm | `POST /api/v1/generated-flowers/drafts` | Tạo Draft và ClientHistory trong một transaction. |
| 6 | Cần thêm | `GET /api/v1/client-histories/{historyId}` | Mở lại Draft bằng `type + output_id`. |

## III. DB tổng và phần code còn thiếu

`db_tong.md` đã có đúng các field sau:

```text
generated_flowers:
  id
  name
  input_snapshot json
  image_url
  user_id
  order_id
  flower_generation_status
  is_deleted
  created_at
  updated_at

client_histories:
  id
  user_id
  input
  base_id
  output_id
  order_id
  type
  system_prompt_id
  metadata
  is_deleted
  created_at
  updated_at
```

Không thêm `flower_requests`, `client_history_inputs`, `group_id`, `parent_id` hoặc `based_on`.

Phần thiếu trong solution là Entity, Configuration và DbSet:

```csharp
public enum GenerationType
{
    Flower,
    Card
}

public enum FlowerGenerationStatus
{
    Draft,
    Running,
    Generated,
    Failed
}
```

```csharp
public class GeneratedFlower : BaseEntity<Guid>, IAuditableEntity
{
    public string? Name { get; set; }
    public JsonDocument? InputSnapshot { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? UserId { get; set; }
    public Guid? OrderId { get; set; }
    public FlowerGenerationStatus FlowerGenerationStatus { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ClientHistory : BaseEntity<Guid>, IAuditableEntity
{
    public Guid? UserId { get; set; }
    public string? Input { get; set; }
    public Guid? BaseId { get; set; }
    public Guid? OutputId { get; set; }
    public Guid? OrderId { get; set; }
    public GenerationType Type { get; set; }
    public Guid? SystemPromptId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class Mockup : BaseEntity<Guid>, IAuditableEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
```

```csharp
// AppDbContext.cs
public virtual DbSet<GeneratedFlower> GeneratedFlowers { get; set; }
public virtual DbSet<ClientHistory> ClientHistories { get; set; }
public virtual DbSet<Mockup> Mockups { get; set; }
```

Configuration phải map `InputSnapshot` đúng kiểu `json` như `db_tong.md`; không tự đổi thành `jsonb`.

## IV. DTO và service cần thêm

### 1. Request, response

```csharp
namespace HoaTheoMua.Service.GeneratedFlower;

public static class Request
{
    public sealed class GetMockupsRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 4;
    }

    public sealed class CreateDraftRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid ComboId { get; set; }
        public Guid MockupId { get; set; }
    }
}
```

```csharp
public static class Response
{
    public sealed class MockupItem
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public sealed class CreateDraftResponse
    {
        public Guid HistoryId { get; set; }
        public Guid OutputId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid ComboId { get; set; }
        public Guid MockupId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public sealed class FlowerHistoryDetail
    {
        public Guid HistoryId { get; set; }
        public Guid BaseId { get; set; }
        public Guid OutputId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public FlowerInputSnapshot Snapshot { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
```

### 2. Snapshot

```csharp
public sealed class FlowerInputSnapshot
{
    public int SchemaVersion { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public ComboSnapshot Combo { get; set; } = null!;
    public MockupSnapshot Mockup { get; set; } = null!;
}

public sealed class ComboSnapshot
{
    public Guid Id { get; set; }
    public string? ProductName { get; set; }
    public string? CoverImage { get; set; }
    public decimal? Price { get; set; }
    public int SellableQuantity { get; set; }
    public List<MaterialSnapshot> Materials { get; set; } = new();
}

public sealed class MaterialSnapshot
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? CoverImage { get; set; }
    public bool IsCore { get; set; }
    public decimal QuantityPerCombo { get; set; }
    public string? Unit { get; set; }
    public decimal AvailableForSale { get; set; }
}

public sealed class MockupSnapshot
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
```

### 3. Interface

Mọi hàm mới được dùng bên dưới đều khai báo tại đây:

```csharp
namespace HoaTheoMua.Service.GeneratedFlower;

public interface IService
{
    Task<BasePaginationResponse> GetMockups(
        Request.GetMockupsRequest request);

    Task<Response.CreateDraftResponse> CreateDraft(
        Request.CreateDraftRequest request);

    Task<Response.FlowerHistoryDetail> GetHistoryDetail(
        Guid historyId);
}
```

### 4. Service và dependency

Các field được dùng trong implementation bên dưới được khai báo tại đây:

```csharp
namespace HoaTheoMua.Service.GeneratedFlower;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly HoaTheoMua.Service.Product.IService _productService;
    private readonly ICurrentUserService _currentUser;

    public Service(
        AppDbContext dbContext,
        HoaTheoMua.Service.Product.IService productService,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _productService = productService;
        _currentUser = currentUser;
    }

    // Đặt ba method GetMockups, CreateDraft và GetHistoryDetail bên dưới vào class này.
}
```

## V. Request, response và implementation từng API

### API 1 — Danh sách Combo sáng tạo `[Có sẵn]`

**Request**

```http
GET /api/v2/products?productType=1&categoryIds={creativeComboCategoryId}&IsActive=true&pageIndex=1&pageSize=10
```

- `productType=1`: `ProductType.Combo` đang có trong solution.
- `categoryIds`: ID category Combo sáng tạo lấy từ dữ liệu `CategoryProducts`; không tạo enum/category ID giả.

**Code đang có**

```csharp
// ProductController.cs
[HttpGet("/api/v2/products")]
public async Task<IActionResult> GetLocalProductsV2(
    [FromQuery] Request.LocalProductQueryRequest request)
{
    var result = await _productService.GetLocalProductsV2(request);
    return Ok(result);
}
```

### API 2 — Chi tiết Combo `[Có sẵn]`

**Request**

```http
GET /api/v2/products/{comboId}/detail
Authorization: Bearer {token}
```

**Dữ liệu trả về đang có**

```text
Id, ProductName, CoverImage, Price, OldPrice,
SellableQuantity, NhanhProductId, IsActive, Parent
```

**Code đang có**

```csharp
await _productService.GetProductDetailByCustomer(comboId);
```

### API 3 — Thành phần Combo `[Có sẵn]`

**Request**

```http
GET /api/v1/products/{comboId}/combo-specification
```

**Dữ liệu trả về đang có**

```text
SubId, ProductCode, ProductName, CoverImage,
Materials[].MaterialId, MaterialName, CoverImage,
IsCore, QuantityPerCombo, Unit, AvailableForSale, MaxComboPossible
```

**Code đang có**

```csharp
await _productService.GetComboSpecificationAsync(comboId);
```

### API 4 — Danh sách Mockup `[Cần thêm]`

**Request**

```http
GET /api/v1/mockups?pageIndex=1&pageSize=4
Authorization: Bearer {token}
```

**Response**

```json
{
  "value": {
    "items": [
      {
        "id": "mockup-uuid",
        "name": "Phòng khách",
        "description": "...",
        "imageUrl": "https://..."
      }
    ],
    "pageIndex": 1,
    "pageSize": 4,
    "totalCount": 5
  }
}
```

**Implementation mới — đã định nghĩa đầy đủ**

```csharp
public async Task<BasePaginationResponse> GetMockups(
    Request.GetMockupsRequest request)
{
    var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
    var pageSize = request.PageSize <= 0
        ? 4
        : Math.Min(request.PageSize, 4);

    var query = _dbContext.Mockups
        .AsNoTracking()
        .Where(x => x.IsActive && !x.IsDeleted)
        .OrderByDescending(x => x.CreatedAt);

    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new Response.MockupItem
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            ImageUrl = x.ImageUrl
        })
        .ToListAsync();

    return ApiResponseFactory.BasePagination(
        items, pageIndex, pageSize, totalCount);
}
```

### API 5 — Tạo bản Draft `[Cần thêm]`

**Request**

```http
POST /api/v1/generated-flowers/drafts
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "name": "Mẫu hoa sinh nhật mẹ",
  "comboId": "combo-product-uuid",
  "mockupId": "mockup-uuid"
}
```

Không nhận `userId`, `status`, `baseId` hoặc `outputId` từ frontend.

**Response — `201 Created`**

```json
{
  "value": {
    "historyId": "history-uuid",
    "outputId": "generated-flower-uuid",
    "name": "Mẫu hoa sinh nhật mẹ",
    "status": "draft",
    "comboId": "combo-product-uuid",
    "mockupId": "mockup-uuid",
    "createdAt": "2026-08-31T10:00:00Z"
  }
}
```

**Implementation mới — chỉ gọi hàm có thật hoặc đã định nghĩa**

```csharp
public async Task<Response.CreateDraftResponse> CreateDraft(
    Request.CreateDraftRequest request)
{
    var userId = _currentUser.GetRequiredUserId();
    var name = request.Name?.Trim() ?? string.Empty;

    if (name.Length is < 1 or > 100)
        throw new BadRequestException("Tên phải từ 1 đến 100 ký tự.");

    // Hai hàm này đang có trong Product.IService.
    var combo = await _productService
        .GetProductDetailByCustomer(request.ComboId);
    var specification = await _productService
        .GetComboSpecificationAsync(request.ComboId);

    if (!combo.IsActive || combo.SellableQuantity is null or <= 0)
        throw new BadRequestException(
            "Combo đã chọn không còn khả dụng. Vui lòng chọn combo khác.");

    if (specification is null)
        throw new BadRequestException("Combo không có công thức thành phần.");

    var mockup = await _dbContext.Mockups
        .AsNoTracking()
        .FirstOrDefaultAsync(x =>
            x.Id == request.MockupId &&
            x.IsActive &&
            !x.IsDeleted)
        ?? throw new BadRequestException(
            "Mockup đã chọn không còn khả dụng. Vui lòng chọn Mockup khác.");

    var snapshot = new FlowerInputSnapshot
    {
        SchemaVersion = 1,
        Name = name,
        Combo = new ComboSnapshot
        {
            Id = combo.Id,
            ProductName = combo.ProductName,
            CoverImage = combo.CoverImage,
            Price = combo.Price,
            SellableQuantity = combo.SellableQuantity.Value,
            Materials = specification.Materials.Select(x =>
                new MaterialSnapshot
                {
                    ProductId = Guid.Parse(x.MaterialId!),
                    ProductName = x.MaterialName,
                    CoverImage = x.CoverImage,
                    IsCore = x.IsCore ?? false,
                    QuantityPerCombo = x.QuantityPerCombo ?? 0,
                    Unit = x.Unit,
                    AvailableForSale = x.AvailableForSale
                }).ToList()
        },
        Mockup = new MockupSnapshot
        {
            Id = mockup.Id,
            Name = mockup.Name,
            Description = mockup.Description,
            ImageUrl = mockup.ImageUrl
        }
    };

    var now = DateTimeOffset.UtcNow;
    var flowerId = Guid.NewGuid();
    var historyId = Guid.NewGuid();

    await using var transaction =
        await _dbContext.Database.BeginTransactionAsync();

    _dbContext.GeneratedFlowers.Add(new GeneratedFlower
    {
        Id = flowerId,
        Name = name,
        InputSnapshot = JsonSerializer.SerializeToDocument(snapshot),
        ImageUrl = null,
        UserId = userId,
        OrderId = null,
        FlowerGenerationStatus = FlowerGenerationStatus.Draft,
        IsDeleted = false,
        CreatedAt = now
    });

    _dbContext.ClientHistories.Add(new ClientHistory
    {
        Id = historyId,
        UserId = userId,
        Input = name,
        BaseId = flowerId,
        OutputId = flowerId,
        OrderId = null,
        Type = GenerationType.Flower,
        SystemPromptId = null,
        Metadata = null,
        IsDeleted = false,
        CreatedAt = now
    });

    await _dbContext.SaveChangesAsync();
    await transaction.CommitAsync();

    return new Response.CreateDraftResponse
    {
        HistoryId = historyId,
        OutputId = flowerId,
        Name = name,
        Status = "draft",
        ComboId = combo.Id,
        MockupId = mockup.Id,
        CreatedAt = now
    };
}
```

Transaction bảo đảm lần lưu thất bại không để lại `generated_flowers` hoặc `client_histories` dở dang. Retry sau lần rollback sẽ chỉ tạo một cặp record thành công.

### API 6 — Mở lại Draft `[Cần thêm]`

**Request**

```http
GET /api/v1/client-histories/{historyId}
Authorization: Bearer {token}
```

**Response**

```json
{
  "value": {
    "historyId": "history-uuid",
    "baseId": "generated-flower-root-uuid",
    "outputId": "generated-flower-uuid",
    "type": "flower",
    "name": "Mẫu hoa sinh nhật mẹ",
    "status": "draft",
    "imageUrl": null,
    "snapshot": {
      "schemaVersion": 1,
      "combo": {},
      "mockup": {}
    },
    "createdAt": "2026-08-31T10:00:00Z"
  }
}
```

**Implementation mới — đã định nghĩa đầy đủ**

```csharp
public async Task<Response.FlowerHistoryDetail> GetHistoryDetail(
    Guid historyId)
{
    var userId = _currentUser.GetRequiredUserId();

    var history = await _dbContext.ClientHistories
        .AsNoTracking()
        .FirstOrDefaultAsync(x =>
            x.Id == historyId &&
            x.UserId == userId &&
            !x.IsDeleted)
        ?? throw new NotFoundException("Không tìm thấy lịch sử.");

    if (history.Type != GenerationType.Flower ||
        history.OutputId is null ||
        history.BaseId is null)
    {
        throw new BadRequestException("Lịch sử không phải mẫu hoa.");
    }

    // type = flower nên output_id được tra trong generated_flowers.
    var flower = await _dbContext.GeneratedFlowers
        .AsNoTracking()
        .FirstOrDefaultAsync(x =>
            x.Id == history.OutputId.Value &&
            !x.IsDeleted)
        ?? throw new NotFoundException("Không tìm thấy mẫu hoa.");

    if (flower.InputSnapshot is null)
        throw new BadRequestException("Mẫu hoa không có input snapshot.");

    var snapshot = flower.InputSnapshot.RootElement
        .Deserialize<FlowerInputSnapshot>()
        ?? throw new BadRequestException("Input snapshot không hợp lệ.");

    return new Response.FlowerHistoryDetail
    {
        HistoryId = history.Id,
        BaseId = history.BaseId.Value,
        OutputId = history.OutputId.Value,
        Type = "flower",
        Name = flower.Name ?? snapshot.Name,
        Status = flower.FlowerGenerationStatus
            .ToString().ToLowerInvariant(),
        ImageUrl = flower.ImageUrl,
        Snapshot = snapshot,
        CreatedAt = flower.CreatedAt
    };
}
```

### Controller và DI cho API mới

```csharp
[ApiController]
[Authorize(Policy = JwtExtensions.CustomerPolicy)]
public class GeneratedFlowerController : ControllerBase
{
    private readonly HoaTheoMua.Service.GeneratedFlower.IService _service;

    public GeneratedFlowerController(
        HoaTheoMua.Service.GeneratedFlower.IService service)
    {
        _service = service;
    }

    [HttpGet("/api/v1/mockups")]
    public async Task<IActionResult> GetMockups(
        [FromQuery] Request.GetMockupsRequest request)
        => Ok(await _service.GetMockups(request));

    [HttpPost("/api/v1/generated-flowers/drafts")]
    public async Task<IActionResult> CreateDraft(
        [FromBody] Request.CreateDraftRequest request)
    {
        var result = await _service.CreateDraft(request);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponseFactory.Base(
                result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpGet("/api/v1/client-histories/{historyId:guid}")]
    public async Task<IActionResult> GetHistoryDetail(Guid historyId)
    {
        var result = await _service.GetHistoryDetail(historyId);
        return Ok(ApiResponseFactory.Base(
            result, true, "", HttpContext.TraceIdentifier));
    }
}
```

```csharp
// Program.cs
using GeneratedFlowerService = HoaTheoMua.Service.GeneratedFlower;

builder.Services.AddScoped<
    GeneratedFlowerService.IService,
    GeneratedFlowerService.Service>();
```

## VI. Ba điểm khó hình dung

### 1. Gom các lần generate cùng nhóm

Không group bằng Tên, Combo hoặc Mockup vì các giá trị này có thể trùng. Group bằng `base_id`:

```text
F01 Draft/Generated:  base_id = F01, output_id = F01
F02 Regenerate lần 1: base_id = F01, output_id = F02
F03 Regenerate lần 2: base_id = F01, output_id = F03
```

Khi regenerate từ F02, lấy `currentHistory.BaseId = F01`; không lấy `F02` làm base mới.

```sql
SELECT *
FROM client_histories
WHERE user_id = :userId
  AND type = 'flower'
  AND base_id = :baseId
  AND is_deleted = false
ORDER BY created_at DESC;
```

### 2. Truy vấn bằng `output_id`

Backend đọc `client_histories.type` trước:

```text
flower -> generated_flowers.id = output_id
card   -> generated_cards.id = output_id
```

Không có FK nhưng vẫn query/join được. Application chịu trách nhiệm tạo output và history trong cùng transaction, đồng thời kiểm tra ownership bằng `user_id`.

### 3. Mở lại Draft và render dữ liệu đã chọn

Lưu tại `generated_flowers`:

- `name`: tên hiển thị.
- `flower_generation_status`: Draft/Running/Generated/Failed.
- `input_snapshot`: JSON chứa Combo, Product thành phần và Mockup.

```json
{
  "schemaVersion": 1,
  "name": "Mẫu hoa sinh nhật mẹ",
  "combo": {
    "id": "combo-product-uuid",
    "productName": "Combo mùa xuân",
    "coverImage": "https://...",
    "price": 500000,
    "sellableQuantity": 3,
    "materials": [
      {
        "productId": "material-product-uuid",
        "productName": "Hoa hồng",
        "coverImage": "https://...",
        "isCore": true,
        "quantityPerCombo": 3,
        "unit": "bông",
        "availableForSale": 30
      }
    ]
  },
  "mockup": {
    "id": "mockup-uuid",
    "name": "Phòng khách",
    "description": "...",
    "imageUrl": "https://..."
  }
}
```

- `combo.id` là `Products.Id` của Combo.
- `materials[].productId` là `Products.Id` của từng thành phần.
- Render Draft từ snapshot để giữ đúng dữ liệu đã chọn.
- Khi người dùng bấm gọi AI ở STORY-033, dùng các ID trong snapshot để kiểm tra lại dữ liệu hiện hành.

## VII. Điểm chưa đáp ứng được từ DB/code hiện tại

ST-030-02 yêu cầu hiển thị **mô tả Combo**, nhưng `Products` trong `db_tong.md` và `GetProductDetailByCustomerResponse` đều không có field mô tả Product. Không được tự tạo `combo.Description` trong code.

Cần BA/lead chốt một trong hai:

1. Mô tả lấy từ field hiện có nào; hoặc
2. Bổ sung field mô tả cho `Products` và cập nhật `db_tong.md`.

////////////////////////
Trong file này: 'd:/dev/csharp/projects/Hoatheomua/hoatheomua-be/HoaTheoMua.Repository/code-introduction/ST-030-introduction.md'. Tôi có thắc mắc ở chỗ: Khi regenerate từ F02, lấy currentHistory.BaseId = F01; không
  lấy F02 làm base mới. Làm sao để biết được mà lấy F01 mà lấy được


• F01 được lấy từ chính bản ghi client_histories tương ứng với F02, không lấy từ bảng generated_flowers.

  Ví dụ dữ liệu:

  client_histories

  H01: base_id = F01, output_id = F01
  H02: base_id = F01, output_id = F02
  H03: base_id = F01, output_id = F03

  Khi người dùng chọn F02 để regenerate, backend tìm History của F02:

  var currentHistory = await _dbContext.ClientHistories
      .AsNoTracking()
      .SingleOrDefaultAsync(x =>
          x.UserId == userId &&
          x.Type == GenerationType.Flower &&
          x.OutputId == request.OutputId && // F02
          !x.IsDeleted)
      ?? throw new NotFoundException("Không tìm thấy lịch sử mẫu hoa.");

  Kết quả trả về là H02:

  currentHistory.OutputId = F02
  currentHistory.BaseId   = F01

  Sau đó tạo output mới F03:

  var newFlowerId = Guid.NewGuid(); // F03

  var newHistory = new ClientHistory
  {
      Id = Guid.NewGuid(),
      UserId = userId,
      Type = GenerationType.Flower,

      // Copy base gốc từ History hiện tại.
      BaseId = currentHistory.BaseId, // F01

      // Output vừa tạo.
      OutputId = newFlowerId // F03
  };

  Luồng truyền base_id:

  Tạo ban đầu:
  H01.base_id = F01

  Regenerate từ F01:
  H02.base_id = H01.base_id = F01

  Regenerate từ F02:
  H03.base_id = H02.base_id = F01

  Regenerate từ F03:
  H04.base_id = H03.base_id = F01

  Vì mọi History đều sao chép base_id gốc nên dù regenerate từ phiên bản nào, backend vẫn lấy được F01.

  API regenerate có thể nhận:

  {
    "outputId": "F02"
  }

  Hoặc tốt hơn là nhận historyId, sau đó kiểm tra ownership và lấy thẳng History:

  {
    "historyId": "H02"
  }

  Mình nghiêng về historyId vì nó xác định trực tiếp client_histories; backend không cần tìm ngược từ output_id. Nhưng cả hai cách đều lấy được F01.
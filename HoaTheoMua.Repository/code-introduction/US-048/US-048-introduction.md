# US-048 - Admin xem chi tiet mau hoa Custom AI

## I. Cac API can lam

US-048 dung de Admin xem chi tiet mot mau hoa Custom AI; khong hien thi gia/bill va khong kiem tra lai Combo live.

Can **1 API**:

`GET /api/v1/admin/generated-flowers/{generatedFlowerId}`

- FE goi khi Admin bam **Xem chi tiet** tu US-047.
- Tra anh ket qua, Combo snapshot, Core/Support snapshot, Mockup snapshot, yeu cau tuy chinh va Order lien quan.
- Khong tra gia/bill; gia xem trong chi tiet Order.

Luong:

```text
Admin bam Xem chi tiet
-> FE goi API chi tiet
-> BE lay client_histories(type=flower) + generated_flowers
-> BE doc snapshot tu generated_flowers.input_snapshot hoac client_histories.metadata
-> BE lay Order lien quan qua OrderDetails.GeneratedFlowerIds
-> FE render chi tiet va lightbox bang imageUrl/mockupImageUrl
```

Luu y:

- `comboId = client_histories.base_id`.
- `generatedFlowerId = client_histories.output_id = generated_flowers.id`.
- Combo/Core/Support/Mockup/Yeu cau tuy chinh lay tu snapshot, khong lay live tu Product/Mockup.
- `customerName` o day la ten nguoi gui/ten da nhap khi tao hoa trong snapshot, khong phai User service.
- Neu `imageUrl` rong/null thi `fileStatus = unavailable`, FE hien trang thai loi vung anh.
- Route bat buoc gan `[Authorize(Policy = JwtExtensions.AdminPolicy)]`.

## II. Code tung API

### API 1 - Lay chi tiet mau hoa Custom AI

**Vai tro:** Tra chi tiet mot mau hoa AI cho Admin, kem snapshot nguon va Order lien quan.

### IService

```csharp
namespace HoaTheoMua.Service.AdminGeneratedFlower;

public interface IService
{
    Task<Response.FlowerDetailResponse> GetById(
        Guid generatedFlowerId);
}
```

### Request

```http
GET /api/v1/admin/generated-flowers/{generatedFlowerId}
Authorization: Bearer {admin-token}
```

Route parameter don gian nen khong can Request DTO.

### Response

```csharp
namespace HoaTheoMua.Service.AdminGeneratedFlower;

public class Response
{
    public class FlowerDetailResponse
    {
        public Guid HistoryId { get; set; }
        public Guid GeneratedFlowerId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid ComboId { get; set; }
        public string? ComboName { get; set; }
        public string? ComboImageUrl { get; set; }
        public string? ImageUrl { get; set; }
        public required string FileStatus { get; set; }
        public UserInputResponse? UserInput { get; set; }
        public MockupResponse? Mockup { get; set; }
        public List<MaterialResponse> CoreMaterials { get; set; } = [];
        public List<MaterialResponse> SupportMaterials { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public List<RelatedOrderResponse> Orders { get; set; } = [];
    }

    public class UserInputResponse
    {
        public string? Name { get; set; }
        public string? Occasion { get; set; }
        public string? Style { get; set; }
        public decimal? Budget { get; set; }
        public string? Note { get; set; }
    }

    public class MockupResponse
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class MaterialResponse
    {
        public Guid? MaterialId { get; set; }
        public string? Name { get; set; }
        public decimal? Quantity { get; set; }
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
  "generatedFlowerId": "fa000000-0000-0000-0000-000000000001",
  "customerId": "10000000-0000-0000-0000-000000000001",
  "customerName": "Nguyen Van A",
  "comboId": "c1000000-0000-0000-0000-000000000001",
  "comboName": "Combo sinh nhat",
  "comboImageUrl": "https://res.cloudinary.com/.../combo.png",
  "imageUrl": "https://res.cloudinary.com/.../flower-ai.png",
  "fileStatus": "available",
  "userInput": {
    "name": "Nguyen Van A",
    "occasion": "sinh_nhat",
    "style": "tone nhe",
    "budget": 500000,
    "note": "Bo cuc tron"
  },
  "mockup": {
    "id": "0b000000-0000-0000-0000-000000000001",
    "name": "Phong khach",
    "imageUrl": "https://res.cloudinary.com/.../mockup.png"
  },
  "coreMaterials": [
    {
      "materialId": "m1000000-0000-0000-0000-000000000001",
      "name": "Hoa hong",
      "quantity": 3
    }
  ],
  "supportMaterials": [],
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
using System.Text.Json.Serialization;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.AdminGeneratedFlower;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Response.FlowerDetailResponse> GetById(
        Guid generatedFlowerId)
    {
        if (generatedFlowerId == Guid.Empty)
            throw new BadRequestException("GeneratedFlowerId khong hop le.");

        var row = await (
            from history in _dbContext.ClientHistories.AsNoTracking()
            join flower in _dbContext.GeneratedFlowers.AsNoTracking()
                on history.OutputId equals flower.Id
            where !history.IsDeleted &&
                  !flower.IsDeleted &&
                  history.Type == GenerationType.Flower &&
                  history.UserId == flower.UserId &&
                  history.OutputId == generatedFlowerId
            select new
            {
                HistoryId = history.Id,
                HistoryMetadata = history.Metadata,
                HistoryBaseId = history.BaseId,
                FlowerId = flower.Id,
                flower.UserId,
                flower.ImageUrl,
                flower.InputSnapshot,
                CreatedAt = history.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (row == null)
            throw new NotFoundException("Khong tim thay mau hoa Custom AI.");

        var snapshot = ReadSnapshot(row.InputSnapshot) ??
                       ReadSnapshot(row.HistoryMetadata);

        var result = new Response.FlowerDetailResponse
        {
            HistoryId = row.HistoryId,
            GeneratedFlowerId = row.FlowerId,
            CustomerId = row.UserId,
            CustomerName = snapshot?.UserInput?.Name,
            ComboId = row.HistoryBaseId,
            ComboName = snapshot?.Product?.Name,
            ComboImageUrl = snapshot?.Product?.Image?.Url,
            ImageUrl = row.ImageUrl,
            FileStatus = string.IsNullOrWhiteSpace(row.ImageUrl)
                ? "unavailable"
                : "available",
            UserInput = MapUserInput(snapshot?.UserInput),
            Mockup = MapMockup(snapshot?.Mockup),
            CoreMaterials = MapMaterials(snapshot?.CoreMaterials),
            SupportMaterials = MapMaterials(snapshot?.SupportMaterials),
            CreatedAt = row.CreatedAt
        };

        result.Orders = await (
                from detail in _dbContext.OrderDetails.AsNoTracking()
                join order in _dbContext.Orders.AsNoTracking()
                    on detail.OrderId equals order.Id
                where !detail.IsDeleted &&
                      !order.IsDeleted &&
                      order.UserId == result.CustomerId &&
                      detail.GeneratedFlowerIds != null &&
                      detail.GeneratedFlowerIds.Contains(generatedFlowerId)
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

    private static FlowerSnapshot? ReadSnapshot(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return JsonSerializer.Deserialize<FlowerSnapshot>(
                value,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static Response.UserInputResponse? MapUserInput(
        SnapshotUserInput? input)
    {
        if (input == null)
            return null;

        return new Response.UserInputResponse
        {
            Name = input.Name,
            Occasion = input.Occasion,
            Style = input.Style,
            Budget = input.Budget,
            Note = input.Note
        };
    }

    private static Response.MockupResponse? MapMockup(
        SnapshotMockup? mockup)
    {
        if (mockup == null)
            return null;

        return new Response.MockupResponse
        {
            Id = mockup.Id,
            Name = mockup.Name,
            ImageUrl = mockup.Image?.Url
        };
    }

    private static List<Response.MaterialResponse> MapMaterials(
        List<SnapshotMaterial>? materials)
    {
        if (materials == null)
            return [];

        return materials.Select(x => new Response.MaterialResponse
        {
            MaterialId = x.MaterialId,
            Name = x.Name,
            Quantity = x.Quantity
        }).ToList();
    }

    private class FlowerSnapshot
    {
        [JsonPropertyName("product")]
        public SnapshotProduct? Product { get; set; }

        [JsonPropertyName("user_input")]
        public SnapshotUserInput? UserInput { get; set; }

        [JsonPropertyName("mockup")]
        public SnapshotMockup? Mockup { get; set; }

        [JsonPropertyName("core_materials")]
        public List<SnapshotMaterial>? CoreMaterials { get; set; }

        [JsonPropertyName("support_materials")]
        public List<SnapshotMaterial>? SupportMaterials { get; set; }
    }

    private class SnapshotProduct
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("image")]
        public SnapshotImage? Image { get; set; }
    }

    private class SnapshotUserInput
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("occasion")]
        public string? Occasion { get; set; }

        [JsonPropertyName("style")]
        public string? Style { get; set; }

        [JsonPropertyName("budget")]
        public decimal? Budget { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }

    private class SnapshotMockup
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("image")]
        public SnapshotImage? Image { get; set; }
    }

    private class SnapshotMaterial
    {
        [JsonPropertyName("material_id")]
        public Guid? MaterialId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; set; }
    }

    private class SnapshotImage
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
```

## III. DB con thieu

DB hien tai du cho US-048:

```text
client_histories.base_id -> combo id
client_histories.output_id -> generated_flowers.id
client_histories.metadata -> snapshot combo/core/support/mockup/input
generated_flowers.input_snapshot -> snapshot da gui AI
generated_flowers.image_url
OrderDetails.GeneratedFlowerIds
```

Khong can them field moi. Dieu kien quan trong la snapshot luc tao hoa phai co du `product`, `user_input`, `mockup`, `core_materials`, `support_materials`.

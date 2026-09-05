# US-040 — Xem lịch sử mẫu hoa Custom AI

## I. Hiểu nhanh US

US-040 dùng để customer xem các ảnh hoa AI đã tạo thành công; không tạo hoa, regenerate, tải ảnh, lưu draft hay hiển thị AI job đang chạy.

Có 2 nhu cầu độc lập:

```text
Customer vào trang History
→ FE gọi API lấy toàn bộ lịch sử có phân trang
→ BE trả các ảnh mới nhất trước.

FE cần gợi ý theo một combo
→ FE gửi comboId
→ BE trả tối đa 3 ảnh gần nhất của combo đó.

Customer bấm một kết quả
→ FE gửi generatedFlowerId
→ BE trả chi tiết kết quả nếu thuộc customer hiện tại.
```

BE truy vấn chung theo quy tắc:

```text
client_histories của customer hiện tại
→ type = flower
→ join generated_flowers bằng output_id
→ sắp xếp thời gian tạo giảm dần
```

Riêng API lấy 3 ảnh gần nhất có thêm:

```text
base_id = comboId → Take(3)
```

Ví dụ với `comboId = C01`:

```text
H01: base_id = C01, output_id = F01, created_at = 08:00
H02: base_id = C01, output_id = F02, created_at = 09:00
H03: base_id = C01, output_id = F03, created_at = 10:00
H04: base_id = C01, output_id = F04, created_at = 11:00
```

- API History có thể trả cả `F04, F03, F02, F01` theo phân trang.
- API gần nhất trả đúng `F04, F03, F02`.
- Không API nào dùng `GroupBy`.

Cần 3 API:

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
```

Lấy toàn bộ lịch sử hoa AI của customer hiện tại, có phân trang.

```http
GET /api/v1/flower-histories/latest?comboId={comboId}
```

Lấy tối đa 3 ảnh hoa AI gần nhất của một combo.

```http
GET /api/v1/flower-histories/{generatedFlowerId}
```

Lấy chi tiết một kết quả khi customer bấm vào ảnh.

**Điểm quan trọng:**

- `client_histories.base_id` là `comboId`.
- `client_histories.output_id` là `generated_flowers.id`.
- `userId` lấy từ JWT, không nhận từ FE.
- Chỉ lấy record `type = flower` và chưa bị xóa.
- Không kiểm tra `Products.IsActive/IsDeleted`: lịch sử vẫn tồn tại khi combo nguồn inactive hoặc bị xóa; dữ liệu nguồn đã được giữ trong `generated_flowers.input_snapshot`.
- FE render trực tiếp `imageUrl`; một ảnh lỗi hiển thị placeholder và không làm lỗi toàn bộ danh sách.
- Không có draft nên reload chỉ còn các lần generate đã lưu thành công.

> Chốt mới này thay phần group theo “yêu cầu tạo mẫu hoa”, yêu cầu chưa generate và trạng thái job sau reload trong Story/System Test cũ.

## II. Code implement từng API

### DTO dùng chung

**Request.cs**

```csharp
namespace HoaTheoMua.Service.FlowerHistory;

public class Request
{
    public class GetFlowerHistoriesRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
```

**Response.cs**

```csharp
namespace HoaTheoMua.Service.FlowerHistory;

public class Response
{
    public class FlowerHistoryItemResponse
    {
        public Guid GeneratedFlowerId { get; set; }
        public Guid ComboId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class FlowerHistoryDetailResponse
    {
        public Guid GeneratedFlowerId { get; set; }
        public Guid ComboId { get; set; }
        public string? ImageUrl { get; set; }
        public string? InputSnapshot { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
```

### API 1 — Lấy toàn bộ lịch sử hoa AI

**Vai trò:** dùng khi customer mở trang History; trả toàn bộ kết quả của chính customer theo từng trang, mới nhất trước.

**Request**

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
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
        "generatedFlowerId": "f4000000-0000-0000-0000-000000000000",
        "comboId": "c1000000-0000-0000-0000-000000000000",
        "imageUrl": "https://res.cloudinary.com/.../f04.png",
        "generatedAt": "2026-09-05T11:00:00Z"
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

Không có lịch sử thì trả `200 OK` với `items: []`.

**IService.cs**

```csharp
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.FlowerHistory;

public interface IService
{
    Task<BasePaginationResponse> GetAll(
        Request.GetFlowerHistoriesRequest request);
}
```

**Service**

```csharp
public async Task<BasePaginationResponse> GetAll(
    Request.GetFlowerHistoriesRequest request)
{
    if (request.PageIndex < 1 ||
        request.PageSize < 1 ||
        request.PageSize > 100)
    {
        throw new BadRequestException(
            "PageIndex phải từ 1 và PageSize phải từ 1 đến 100");
    }

    var userId = _currentUser.GetRequiredUserId();

    var histories = _dbContext.ClientHistories
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.Type == GenerationType.Flower &&
            !x.IsDeleted);

    var flowers = _dbContext.GeneratedFlowers
        .AsNoTracking()
        .Where(x => x.UserId == userId && !x.IsDeleted);

    var query =
        from history in histories
        join flower in flowers
            on history.OutputId equals flower.Id
        select new Response.FlowerHistoryItemResponse
        {
            GeneratedFlowerId = flower.Id,
            ComboId = history.BaseId,
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
        items,
        request.PageIndex,
        request.PageSize,
        totalCount);
}
```

### API 2 — Lấy 3 mẫu hoa AI gần nhất theo combo

**Vai trò:** dùng khi FE cần render nhanh 3 kết quả gần nhất của combo đang xem.

**Request**

```http
GET /api/v1/flower-histories/latest?comboId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa
Authorization: Bearer {customer-token}
```

`comboId` là query parameter đơn giản nên không cần Request DTO.

**Response payload**

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": [
    {
      "generatedFlowerId": "f4000000-0000-0000-0000-000000000000",
      "comboId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "imageUrl": "https://res.cloudinary.com/.../f04.png",
      "generatedAt": "2026-09-05T11:00:00Z"
    },
    {
      "generatedFlowerId": "f3000000-0000-0000-0000-000000000000",
      "comboId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "imageUrl": "https://res.cloudinary.com/.../f03.png",
      "generatedAt": "2026-09-05T10:00:00Z"
    },
    {
      "generatedFlowerId": "f2000000-0000-0000-0000-000000000000",
      "comboId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "imageUrl": "https://res.cloudinary.com/.../f02.png",
      "generatedAt": "2026-09-05T09:00:00Z"
    }
  ]
}
```

Không có dữ liệu thì trả `200 OK` với `value: []`.

**IService.cs**

```csharp
Task<List<Response.FlowerHistoryItemResponse>> GetLatestByCombo(
    Guid comboId);
```

**Service**

```csharp
public async Task<List<Response.FlowerHistoryItemResponse>> GetLatestByCombo(
    Guid comboId)
{
    if (comboId == Guid.Empty)
    {
        throw new BadRequestException("ComboId không hợp lệ");
    }

    var userId = _currentUser.GetRequiredUserId();

    var histories = _dbContext.ClientHistories
        .AsNoTracking()
        .Where(x =>
            x.UserId == userId &&
            x.Type == GenerationType.Flower &&
            x.BaseId == comboId &&
            !x.IsDeleted);

    var flowers = _dbContext.GeneratedFlowers
        .AsNoTracking()
        .Where(x => x.UserId == userId && !x.IsDeleted);

    return await (
            from history in histories
            join flower in flowers
                on history.OutputId equals flower.Id
            orderby flower.CreatedAt descending, flower.Id descending
            select new Response.FlowerHistoryItemResponse
            {
                GeneratedFlowerId = flower.Id,
                ComboId = history.BaseId,
                ImageUrl = flower.ImageUrl,
                GeneratedAt = flower.CreatedAt
            })
        .Take(3)
        .ToListAsync();
}
```

### API 3 — Lấy chi tiết một kết quả hoa AI

**Vai trò:** dùng khi customer bấm một ảnh trong History; trả ảnh và snapshot của đúng kết quả đó.

**IService**

```csharp
Task<Response.FlowerHistoryDetailResponse> GetById(
    Guid generatedFlowerId);
```

**Request**

```http
GET /api/v1/flower-histories/f4000000-0000-0000-0000-000000000000
Authorization: Bearer {customer-token}
```

`generatedFlowerId` là route parameter đơn giản nên không cần Request DTO.

**Response**

```json
{
  "generatedFlowerId": "f4000000-0000-0000-0000-000000000000",
  "comboId": "c1000000-0000-0000-0000-000000000000",
  "imageUrl": "https://res.cloudinary.com/.../f04.png",
  "inputSnapshot": "{...}",
  "generatedAt": "2026-09-05T11:00:00Z"
}
```

**Service**

```csharp
public async Task<Response.FlowerHistoryDetailResponse> GetById(
    Guid generatedFlowerId)
{
    if (generatedFlowerId == Guid.Empty)
    {
        throw new BadRequestException(
            "GeneratedFlowerId không hợp lệ");
    }

    var userId = _currentUser.GetRequiredUserId();

    var result = await (
        from history in _dbContext.ClientHistories.AsNoTracking()
        join flower in _dbContext.GeneratedFlowers.AsNoTracking()
            on history.OutputId equals flower.Id
        where history.OutputId == generatedFlowerId &&
              history.UserId == userId &&
              history.Type == GenerationType.Flower &&
              !history.IsDeleted &&
              flower.UserId == userId &&
              !flower.IsDeleted
        select new Response.FlowerHistoryDetailResponse
        {
            GeneratedFlowerId = flower.Id,
            ComboId = history.BaseId,
            ImageUrl = flower.ImageUrl,
            InputSnapshot = flower.InputSnapshot,
            GeneratedAt = flower.CreatedAt
        })
        .FirstOrDefaultAsync();

    if (result == null)
    {
        throw new NotFoundException(
            "Không tìm thấy lịch sử mẫu hoa");
    }

    return result;
}
```

Ownership được lọc ngay trong query; ID thuộc customer khác không trả ảnh hoặc snapshot.

### IService.cs hoàn chỉnh

```csharp
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.FlowerHistory;

public interface IService
{
    Task<BasePaginationResponse> GetAll(
        Request.GetFlowerHistoriesRequest request);

    Task<List<Response.FlowerHistoryItemResponse>> GetLatestByCombo(
        Guid comboId);

    Task<Response.FlowerHistoryDetailResponse> GetById(
        Guid generatedFlowerId);
}
```

Ba method được đặt trong cùng `Service : IService`, inject `AppDbContext` và `ICurrentUserService` qua constructor theo convention hiện tại.

API danh sách chỉ select field FE cần; riêng API chi tiết mới tải `input_snapshot`. Không gọi storage và không đưa binary ảnh qua BE.

# ST-040 - Xem lịch sử mẫu hoa Custom AI

## I. Hiểu nhanh US

US-040 chỉ dùng để xem lại các mẫu hoa AI đã tạo, không gọi AI và không tạo kết quả mới.

Hình dung đơn giản:

```text
Một lần tạo hoa đầu tiên
→ một yêu cầu tạo mẫu hoa
→ các lần regenerate giữ nguyên ProductId và Name
→ các kết quả có cùng ProductId + Name nằm chung một nhóm.
```

Ví dụ người dùng tạo yêu cầu tên **"Hoa sinh nhật mẹ"** từ Product `P01`:

```text
F01: name = "Hoa sinh nhật mẹ"
F02: name = "Hoa sinh nhật mẹ" — regenerate từ F01
F03: name = "Hoa sinh nhật mẹ" — regenerate tiếp
```

Trong `client_histories`:

```text
H01: base_id = P01, output_id = F01
H02: base_id = P01, output_id = F02
H03: base_id = P01, output_id = F03
```

- `base_id = P01`: Product/Combo nguồn.
- `output_id`: kết quả cụ thể trong `generated_flowers`.
- Nhóm logic: `(user_id, base_id, generated_flowers.name)`.

Quy tắc bắt buộc:

- Khi tạo yêu cầu mới, `Name` phải unique trong phạm vi `(UserId, ProductId)`.
- BE chuẩn hóa và kiểm tra tên; không chỉ phụ thuộc FE.
- Regenerate không nhận tên mới; BE sao chép chính xác tên từ mẫu nguồn.
- Không được group chỉ bằng `base_id`, vì một Product có thể được dùng cho nhiều yêu cầu.

Luồng hoạt động:

1. Người dùng mở **"Yêu cầu tạo mẫu hoa"**.
2. FE gọi API lấy danh sách nhóm.
3. BE lấy Flower History của user và group theo `ProductId + Name`.
4. FE hiển thị mỗi nhóm thành một card:

```text
Hoa sinh nhật mẹ
Product: P01
Ảnh đại diện: F03
Tổng số mẫu: 3
```

5. Người dùng chọn một card.
6. FE gửi lại đúng `ProductId + Name` của card đó.
7. BE lấy các history có cùng `base_id = ProductId`, join `output_id` sang `generated_flowers` và lọc đúng `Name`.
8. FE hiển thị các kết quả mới nhất trước.

Chỉ cần 2 API:

```http
GET /api/v1/flower-histories/groups
```

Lấy danh sách nhóm theo `ProductId + Name`.

```http
GET /api/v1/flower-histories/groups/{productId}?name=Hoa%20sinh%20nhật%20mẹ
```

Lấy tất cả kết quả của đúng một nhóm.

Điểm quan trọng: **`base_id` vẫn là ProductId như `docs-db`; một yêu cầu được xác định bằng tổ hợp `UserId + ProductId + Name`.**

Theo quyết định mới nhất:

- Không lưu draft/yêu cầu chưa có kết quả.
- Chỉ kết quả có ảnh hợp lệ mới xuất hiện trong lịch sử và được tính vào tổng số mẫu.
- Ảnh Cloudinary không còn truy cập được thì FE hiển thị placeholder; BE vẫn giữ và trả record.
- US này không kiểm tra quota, không generate hoa và không tạo Order.

> STORY-040 cũ yêu cầu hiển thị yêu cầu chưa có kết quả và trạng thái đang tạo sau reload. Hai trường hợp này không còn đáp ứng được khi đã chốt không lưu draft/job; BA cần cập nhật lại AC/ST tương ứng.

## II. Code implement từng API

Module:

```text
HoaTheoMua.Service/FlowerHistory/
├── Request.cs
├── Response.cs
├── IService.cs
└── Service.cs
```

Dependency chung của `Service`:

```csharp
public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public Service(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    // Hai method bên dưới đặt trong class này.
}
```

### API 1 - Lấy danh sách nhóm

**Vai trò:** FE gọi khi mở màn hình; mỗi item tương ứng một nhóm `ProductId + Name`.

**Request**

```http
GET /api/v1/flower-histories/groups?pageIndex=1&pageSize=10
```

```csharp
// Request.cs — nested class trong Request
public class GetGroupsRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

**Response**

```csharp
// Response.cs — nested class trong Response
public class GroupResponse
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string CoverImageUrl { get; set; } = null!;
    public int TotalResults { get; set; }
    public DateTime LastGeneratedAt { get; set; }
}
```

`ProductId` và `Name` trong response là khóa logic để FE gọi API 2.

**IService**

```csharp
Task<BasePaginationResponse> GetGroups(Request.GetGroupsRequest request);
```

**Service**

```csharp
public async Task<BasePaginationResponse> GetGroups(
    Request.GetGroupsRequest request)
{
    var userId = _currentUser.GetRequiredUserId();
    var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
    var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

    var source =
        from history in _dbContext.ClientHistories.AsNoTracking()
        join flower in _dbContext.GeneratedFlowers.AsNoTracking()
            on history.OutputId equals flower.Id
        where history.UserId == userId
              && history.Type == GenerationType.Flower
              && !history.IsDeleted
              && !flower.IsDeleted
              && flower.FlowerGenerationStatus == FlowerGenerationStatus.Generated
              && flower.Name != null
              && flower.Name != ""
              && flower.ImageUrl != null
              && flower.ImageUrl != ""
        select new
        {
            ProductId = history.BaseId,
            history.CreatedAt,
            flower.Name,
            flower.ImageUrl
        };

    var query = source
        .GroupBy(x => new { x.ProductId, x.Name })
        .Select(group => new Response.GroupResponse
        {
            ProductId = group.Key.ProductId,
            Name = group.Key.Name,
            CoverImageUrl = group
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ImageUrl)
                .First(),
            TotalResults = group.Count(),
            LastGeneratedAt = group.Max(x => x.CreatedAt)
        });

    var totalCount = await query.CountAsync();
    var items = await query
        .OrderByDescending(x => x.LastGeneratedAt)
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return ApiResponseFactory.BasePagination(
        items, pageIndex, pageSize, totalCount);
}
```

### API 2 - Lấy kết quả trong một nhóm

**Vai trò:** FE gửi `ProductId + Name`; BE trả các output thuộc đúng yêu cầu, mới nhất trước.

**Request**

```http
GET /api/v1/flower-histories/groups/{productId}?name=Hoa%20sinh%20nhật%20mẹ&pageIndex=1&pageSize=10
```

```csharp
// Request.cs — nested class trong Request
public class GetGroupResultsRequest
{
    public string Name { get; set; } = null!;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

**Response**

```csharp
// Response.cs — nested class trong Response
public class ItemResponse
{
    public Guid HistoryId { get; set; }
    public Guid ProductId { get; set; }
    public Guid GeneratedFlowerId { get; set; }
    public string Name { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
```

**IService**

```csharp
Task<BasePaginationResponse> GetGroupResults(
    Guid productId,
    Request.GetGroupResultsRequest request);
```

**Service**

```csharp
public async Task<BasePaginationResponse> GetGroupResults(
    Guid productId,
    Request.GetGroupResultsRequest request)
{
    var userId = _currentUser.GetRequiredUserId();
    var name = request.Name?.Trim();

    if (string.IsNullOrWhiteSpace(name))
        throw new BadRequestException("Name is required");

    var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
    var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

    var query =
        from history in _dbContext.ClientHistories.AsNoTracking()
        join flower in _dbContext.GeneratedFlowers.AsNoTracking()
            on history.OutputId equals flower.Id
        where history.UserId == userId
              && history.BaseId == productId
              && history.Type == GenerationType.Flower
              && !history.IsDeleted
              && !flower.IsDeleted
              && flower.FlowerGenerationStatus == FlowerGenerationStatus.Generated
              && flower.Name == name
              && flower.ImageUrl != null
              && flower.ImageUrl != ""
        orderby history.CreatedAt descending
        select new Response.ItemResponse
        {
            HistoryId = history.Id,
            ProductId = history.BaseId,
            GeneratedFlowerId = history.OutputId,
            Name = flower.Name,
            ImageUrl = flower.ImageUrl,
            CreatedAt = history.CreatedAt
        };

    var totalCount = await query.CountAsync();
    if (totalCount == 0)
        throw new NotFoundException("Flower history group not found");

    var items = await query
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return ApiResponseFactory.BasePagination(
        items, pageIndex, pageSize, totalCount);
}
```

Các `using` cần có:

```csharp
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;
```

## III. Điều kiện phụ thuộc từ ST-030/ST-033

ST-040 không cần thêm bảng hoặc field, nhưng API tạo/regenerate phải đảm bảo:

```text
Create mới:
- Chuẩn hóa Name.
- Không cho trùng Name trong cùng UserId + ProductId.
- client_histories.base_id = ProductId.

Regenerate:
- Chỉ nhận GeneratedFlowerId nguồn, không nhận Name mới.
- Lấy ProductId từ history có output_id = GeneratedFlowerId nguồn.
- Sao chép Name từ GeneratedFlower nguồn.
- History mới giữ base_id = ProductId, output_id = GeneratedFlowerId mới.
```

Nếu không giữ ba quy tắc này, US-040 có thể gộp sai hoặc tách sai nhóm.

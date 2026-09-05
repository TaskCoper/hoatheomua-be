# US-042 — Chọn thiệp từ History cho Checkout

## I. Các API cần làm

US-042 dùng để FE lấy các thiệp History thuộc đúng Combo đang Checkout; không lưu Checkout, không gọi AI và không ghi dữ liệu khi customer chọn/bỏ thiệp.

Cần 1 API:

```http
GET /api/v1/card-histories/eligible?comboId={comboId}&pageIndex=1&pageSize=20
```

- FE gọi khi customer mở “Chọn thiệp từ lịch sử”.
- API trả các thiệp của customer có nguồn bắt đầu từ đúng `comboId`, mới nhất trước.

```text
Customer mở History tại Checkout
→ FE gọi API
→ Customer chọn/thay/bỏ thiệp trong state FE
→ FE gửi generatedCardId đã chọn sang US-039 khi tạo Order
```

Hai trường hợp được lấy:

```text
Card tạo trực tiếp từ Combo
→ card_history.base_id = comboId

Card tạo từ Generated Flower
→ card_history.base_id = generatedFlowerId
→ flower_history.output_id = generatedFlowerId
→ flower_history.base_id = comboId
```

Lưu ý:

- `comboId` là ID nguồn của Combo, tương ứng `ComboSpecifications.SubId`; không phải `ComboSpecifications.Id`.
- Chỉ lấy History `type = card`, đúng ownership và có `generated_cards` chưa bị xóa.
- Template nguồn inactive/xóa không chặn sử dụng lại Card đã tạo.
- Card từng dùng cho Order khác vẫn được hiển thị.
- Không có API chọn/thay/bỏ Card vì Checkout chưa thanh toán chỉ nằm ở FE và reload sẽ mất.
- US-039 phải kiểm tra lại ownership, nguồn Combo và ảnh trước khi tạo Order; sau đó lưu ID vào `OrderDetails.GeneratedCardIds`.
- Không kiểm tra file bằng request đến storage; FE xử lý ảnh lỗi và US-039 kiểm tra lại điều kiện cuối.

## II. Code từng API

### API 1 — Lấy Card History hợp lệ theo Combo

**Vai trò:** trả danh sách thiệp mà customer được phép chọn cho Combo đang Checkout.

### IService

```csharp
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.CardHistoryCheckout;

public interface IService
{
    Task<BasePaginationResponse> GetEligibleCards(
        Request.GetEligibleCardsRequest request);
}
```

### Request

```http
GET /api/v1/card-histories/eligible?comboId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa&pageIndex=1&pageSize=20
Authorization: Bearer {customer-token}
```

```csharp
namespace HoaTheoMua.Service.CardHistoryCheckout;

public class Request
{
    public class GetEligibleCardsRequest
    {
        public Guid ComboId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
```

### Response

```csharp
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.CardHistoryCheckout;

public class Response
{
    public class EligibleCardResponse
    {
        public Guid GeneratedCardId { get; set; }
        public string? ImageUrl { get; set; }
        public FormType FormType { get; set; }
        public string? SizeName { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string? MessageContent { get; set; }
        public decimal? SnapshotPrice { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
```

`SnapshotPrice` giúp FE tham khảo. Giá Order cuối cùng phải được US-039 kiểm tra lại.

### Service

```csharp
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.CardHistoryCheckout;

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

    public async Task<BasePaginationResponse> GetEligibleCards(
        Request.GetEligibleCardsRequest request)
    {
        ValidateRequest(request);
        var userId = _currentUser.GetRequiredUserId();

        var generatedFlowerIds = _dbContext.ClientHistories
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.Type == GenerationType.Flower &&
                x.BaseId == request.ComboId &&
                !x.IsDeleted)
            .Select(x => x.OutputId);

        var cardHistories = _dbContext.ClientHistories
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.Type == GenerationType.Card &&
                !x.IsDeleted &&
                (x.BaseId == request.ComboId ||
                 generatedFlowerIds.Contains(x.BaseId)));

        var cards = _dbContext.GeneratedCards
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted);

        var query = (
            from history in cardHistories
            join card in cards
                on history.OutputId equals card.Id
            select new Response.EligibleCardResponse
            {
                GeneratedCardId = card.Id,
                ImageUrl = card.ImageUrl,
                FormType = card.FormType,
                SizeName = card.SizeName,
                SenderName = card.SenderName,
                ReceiverName = card.ReceiverName,
                MessageContent = card.MessageContent,
                SnapshotPrice = card.TotalPrice,
                GeneratedAt = card.CreatedAt
            })
            .Distinct();

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

    private static void ValidateRequest(
        Request.GetEligibleCardsRequest request)
    {
        if (request.ComboId == Guid.Empty)
        {
            throw new BadRequestException("ComboId không hợp lệ");
        }

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

`Contains` được EF Core dịch thành subquery tại DB; không tải toàn bộ `generatedFlowerIds` lên memory và không phát sinh N+1 query.

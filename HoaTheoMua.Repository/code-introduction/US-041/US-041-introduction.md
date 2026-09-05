# US-041 — Tải ảnh thiệp đã tạo

## I. Các API cần làm

US-041 chỉ cần **1 API**:

`GET /api/v1/generated-cards/{generatedCardId}/image-url`

- Dùng chung khi tải thiệp từ History hoặc màn hình chi tiết Order.
- BE kiểm tra thiệp thuộc customer đang đăng nhập rồi trả `image_url`.
- FE dùng URL để tải trực tiếp từ nền tảng lưu trữ.

Luồng ngắn gọn:

`FE gửi generatedCardId → BE kiểm tra ownership → lấy generated_cards.image_url → trả URL → FE tải ảnh`

Lưu ý:

- Không cần `orderId`: cùng một thiệp luôn dùng cùng một `image_url`.
- Không cần join `client_histories` hoặc `orders`; `generated_cards.user_id` đủ để kiểm tra ownership.
- Không gọi AI, không trừ quota, không tạo thêm History và không thay đổi dữ liệu.
- Customer được tải lại nhiều lần.
- BE không tải file, kiểm tra file trên storage hoặc tạo tên file. FE xử lý việc tải, tên file và lỗi URL không còn khả dụng.
- Quy ước này thay thế các BR/ST cũ yêu cầu BE trả file PNG, tạo tên file và kiểm tra liên kết Card–Order. Các rule về ownership và không làm thay đổi dữ liệu vẫn giữ nguyên.

## II. Code API

### 1. Vai trò

Trả URL ảnh của một thiệp thuộc customer hiện tại.

### IService

```csharp
namespace HoaTheoMua.Service.GeneratedCard;

public interface IService
{
    Task<Response.CardImageUrlResponse> GetImageUrl(Guid generatedCardId);
}
```

### Request

Không cần class Request. `generatedCardId` lấy từ route:

```http
GET /api/v1/generated-cards/{generatedCardId}/image-url
```

### Response

```csharp
namespace HoaTheoMua.Service.GeneratedCard;

public class Response
{
    public class CardImageUrlResponse
    {
        public Guid GeneratedCardId { get; set; }
        public required string ImageUrl { get; set; }
    }
}
```

### Service

```csharp
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.GeneratedCard;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public Service(
        AppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Response.CardImageUrlResponse> GetImageUrl(
        Guid generatedCardId)
    {
        if (generatedCardId == Guid.Empty)
            throw new BadRequestException("Generated card id không hợp lệ.");

        var userId = _currentUserService.GetRequiredUserId();

        var card = await _dbContext.GeneratedCards
            .AsNoTracking()
            .Where(x =>
                x.Id == generatedCardId &&
                x.UserId == userId &&
                !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.ImageUrl
            })
            .FirstOrDefaultAsync();

        if (card == null || string.IsNullOrWhiteSpace(card.ImageUrl))
            throw new NotFoundException("Không tìm thấy ảnh thiệp.");

        return new Response.CardImageUrlResponse
        {
            GeneratedCardId = card.Id,
            ImageUrl = card.ImageUrl
        };
    }
}
```

Không cần bổ sung field hoặc bảng mới cho US-041.

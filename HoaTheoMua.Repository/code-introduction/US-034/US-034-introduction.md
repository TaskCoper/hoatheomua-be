# US-034 — Lấy link tải ảnh mẫu hoa AI

## I. Hiểu nhanh US

US-034 chỉ dùng để customer lấy lại URL của một ảnh hoa AI đã tạo; BE không tải file, không đổi định dạng, không gắn lại logo và không tạo presigned URL.

Hình dung đơn giản:

```text
Customer bấm tải ảnh
→ FE gửi generatedFlowerId
→ BE kiểm tra ảnh thuộc customer hiện tại
→ BE đọc generated_flowers.image_url
→ FE nhận URL và tải trực tiếp từ nền tảng lưu trữ
```

Luồng hoạt động:

1. Khi generate thành công, ảnh đã được upload lên storage và URL được lưu tại `generated_flowers.image_url`.
2. FE gửi `generatedFlowerId` của ảnh customer chọn.
3. BE lấy `userId` từ JWT và tìm đúng record chưa bị xóa của customer.
4. BE chỉ trả `imageUrl`.
5. FE dùng URL để hiển thị hoặc tải trực tiếp.

Chỉ cần 1 API:

```http
GET /api/v1/generated-flowers/{generatedFlowerId}/image-url
```

**Điểm quan trọng:**

- Không cần join `client_histories` vì `generated_flowers` đã có `id` và `user_id`.
- Không kiểm tra trạng thái hiện tại của Combo hoặc Mockup.
- Record tồn tại đồng nghĩa lần generate đã thành công; hệ thống không lưu draft.
- Gọi API nhiều lần không ảnh hưởng quota AI.
- BE không gọi storage để kiểm tra file và không truyền binary ảnh.

> Chốt mới này thay phần BE lấy file, chuẩn hóa PNG và đặt tên file trong Story/System Test cũ. Việc tải file từ URL do FE và nền tảng lưu trữ xử lý.

## II. Code implement từng API

### API 1 — Lấy URL ảnh hoa AI

**Vai trò:** trả URL của đúng ảnh customer đã chọn để FE tải trực tiếp.

**Request**

```http
GET /api/v1/generated-flowers/f4000000-0000-0000-0000-000000000000/image-url
Authorization: Bearer {customer-token}
```

`generatedFlowerId` là route parameter đơn giản nên không cần Request DTO.

**Response payload**

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "generatedFlowerId": "f4000000-0000-0000-0000-000000000000",
    "imageUrl": "https://res.cloudinary.com/.../f04.png"
  }
}
```

**Response.cs**

```csharp
namespace HoaTheoMua.Service.GeneratedFlower;

public class Response
{
    public class FlowerImageUrlResponse
    {
        public Guid GeneratedFlowerId { get; set; }
        public required string ImageUrl { get; set; }
    }
}
```

**IService.cs**

```csharp
namespace HoaTheoMua.Service.GeneratedFlower;

public interface IService
{
    Task<Response.FlowerImageUrlResponse> GetImageUrl(
        Guid generatedFlowerId);
}
```

**Service.cs**

```csharp
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.GeneratedFlower;

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

    public async Task<Response.FlowerImageUrlResponse> GetImageUrl(
        Guid generatedFlowerId)
    {
        if (generatedFlowerId == Guid.Empty)
        {
            throw new BadRequestException(
                "GeneratedFlowerId không hợp lệ");
        }

        var userId = _currentUser.GetRequiredUserId();

        var flower = await _dbContext.GeneratedFlowers
            .AsNoTracking()
            .Where(x =>
                x.Id == generatedFlowerId &&
                x.UserId == userId &&
                !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.ImageUrl
            })
            .FirstOrDefaultAsync();

        if (flower == null || string.IsNullOrWhiteSpace(flower.ImageUrl))
        {
            throw new NotFoundException(
                "Không tìm thấy ảnh mẫu hoa");
        }

        return new Response.FlowerImageUrlResponse
        {
            GeneratedFlowerId = flower.Id,
            ImageUrl = flower.ImageUrl
        };
    }
}
```

Nếu ID không tồn tại hoặc thuộc customer khác, API không trả URL ảnh.

# ST-042 - Chọn thiệp từ History theo Combo

## I. Hiểu nhanh US

US-042 dùng để lấy các thiệp AI mà người dùng đã tạo cho một Combo, để họ chọn lại ngay tại Checkout; không gọi AI và không tạo dữ liệu mới.

Tại Checkout có ba lựa chọn:

```text
Thiệp Free    → chọn ngay.
Thiệp History → chọn ngay từ danh sách của Combo.
Thiệp AI mới  → thanh toán xong mới chuyển sang màn generate.
```

Riêng ST-042:

```text
Checkout đang dùng Combo P01
→ FE gửi comboId = P01
→ BE tìm Card History có base_id = P01
→ join output_id sang generated_cards.id
→ trả tất cả thiệp hợp lệ của Combo P01
→ người dùng chọn một thiệp
→ FE giữ GeneratedCardId trong state Checkout.
```

Ví dụ:

```text
client_histories:
H01: type=card, base_id=P01, output_id=C01
H02: type=card, base_id=P01, output_id=C02
H03: type=card, base_id=P02, output_id=C03

FE gửi P01
→ BE trả C01 và C02
→ không trả C03.
```

Luồng hoạt động:

1. Người dùng chọn **"Thiệp từ lịch sử"** tại Checkout.
2. FE lấy `comboId` của Combo đang đặt.
3. FE gọi API lấy Card History theo `comboId`.
4. BE lọc `client_histories` theo user hiện tại, `type=card` và `base_id=comboId`.
5. BE join `output_id` với `generated_cards.id`.
6. BE chỉ trả Card chưa xóa và có `image_url`, mới nhất trước.
7. Người dùng chọn một Card; FE giữ `GeneratedCardId` trong state.
8. Reload trước khi tạo Order thì lựa chọn bị mất vì không lưu draft.
9. Khi tạo Order, FE gửi `GeneratedCardId` đã chọn trong request Order.

Chỉ cần 1 API:

```http
GET /api/v1/combos/{comboId}/card-histories?pageIndex=1&pageSize=10
```

Điểm quan trọng: **ST-042 chỉ lấy danh sách. Thao tác chọn Card là state của FE, không cần API ghi riêng vào DB.**

> [!IMPORTANT]
> Cách query hai bảng này chỉ đúng khi team thống nhất `client_histories(type=card).base_id` luôn là `ComboId`. Nếu Card tạo từ `GeneratedFlower` đang lưu `base_id = GeneratedFlowerId` như một số tài liệu `docs-db`, API này sẽ không tìm thấy Card đó; khi ấy phải đổi quy ước `base_id` hoặc truy ngược thêm Flower History.

## II. Code implement API

Module:

```text
HoaTheoMua.Service/CardHistory/
├── Request.cs
├── Response.cs
├── IService.cs
└── Service.cs
```

### API 1 - Lấy Card History theo Combo

**Vai trò:** Trả các thiệp của đúng user và đúng Combo để FE hiển thị tại Checkout.

**Request**

```http
GET /api/v1/combos/{comboId}/card-histories?pageIndex=1&pageSize=10
```

```csharp
// Request.cs — nested class trong Request
public class GetByComboRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

`comboId` là route parameter nên không lặp lại trong Request DTO.

**Response**

```csharp
// Response.cs — nested class trong Response
public class CardHistoryItemResponse
{
    public Guid HistoryId { get; set; }
    public Guid ComboId { get; set; }
    public Guid GeneratedCardId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string SizeKey { get; set; } = null!;
    public FormType FormType { get; set; }
    public string SenderName { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public string MessageContent { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**IService**

```csharp
Task<BasePaginationResponse> GetByCombo(
    Guid comboId,
    Request.GetByComboRequest request);
```

**Service**

```csharp
public async Task<BasePaginationResponse> GetByCombo(
    Guid comboId,
    Request.GetByComboRequest request)
{
    var userId = _currentUser.GetRequiredUserId();
    var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
    var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

    var query =
        from history in _dbContext.ClientHistories.AsNoTracking()
        join card in _dbContext.GeneratedCards.AsNoTracking()
            on history.OutputId equals card.Id
        where history.UserId == userId
              && history.Type == GenerationType.Card
              && history.BaseId == comboId
              && !history.IsDeleted
              && card.UserId == userId
              && !card.IsDeleted
              && card.ImageUrl != null
              && card.ImageUrl != ""
        orderby history.CreatedAt descending
        select new Response.CardHistoryItemResponse
        {
            HistoryId = history.Id,
            ComboId = history.BaseId,
            GeneratedCardId = card.Id,
            ImageUrl = card.ImageUrl,
            SizeKey = card.SizeKey,
            FormType = card.FormType,
            SenderName = card.SenderName,
            ReceiverName = card.ReceiverName,
            MessageContent = card.MessageContent,
            TotalPrice = card.TotalPrice,
            CreatedAt = history.CreatedAt
        };

    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return ApiResponseFactory.BasePagination(
        items, pageIndex, pageSize, totalCount);
}
```

Luồng query:

```text
ComboId + UserId
→ query client_histories
→ lọc type=card và base_id=ComboId
→ join output_id với generated_cards.id
→ lọc Card chưa xóa và có image_url
→ sắp xếp mới nhất trước
→ phân trang
→ trả danh sách Card.
```

Payload mẫu:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "items": [
      {
        "historyId": "11111111-1111-1111-1111-111111111111",
        "comboId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "generatedCardId": "22222222-2222-2222-2222-222222222222",
        "imageUrl": "https://cloudinary.com/card-c01.png",
        "sizeKey": "size_A",
        "formType": "calligraphy",
        "senderName": "An",
        "receiverName": "Mẹ",
        "messageContent": "Chúc mẹ sinh nhật vui vẻ",
        "totalPrice": 60000,
        "createdAt": "2026-09-03T08:00:00Z"
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

Các `using` cần có:

```csharp
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Models;
using Microsoft.EntityFrameworkCore;
```

## III. Sau khi FE chọn Card

FE chỉ giữ dữ liệu tạm:

```json
{
  "cardOption": "history",
  "generatedCardId": "22222222-2222-2222-2222-222222222222"
}
```

Khi người dùng tạo Order:

```text
FE gửi GeneratedCardId trong request Order
→ BE kiểm tra lại ownership và image_url
→ BE tính tiền thiệp
→ BE tạo Order
→ lưu Card được chọn theo contract Card–Order mà team chốt.
```

ST-042 không cần thêm bảng, field hoặc API ghi dữ liệu riêng.

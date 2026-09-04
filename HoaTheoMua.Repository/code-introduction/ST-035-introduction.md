# ST-035 - Tạo thiệp cá nhân hóa bằng AI tại Checkout

## 0. Core flow

```text
Mở form
→ lấy Template + Size + giá + quota
→ nhập nội dung/chọn hình thức/có thể đính kèm ảnh
→ tạo AI Job
→ AI thành công: lưu generated_cards + tạo client_histories
→ khách bấm Xác nhận: chỉ gắn card vào Checkout
```

- `Tạo lại` gọi cùng API tạo AI với một `Idempotency-Key` mới.
- Thiệp mới **không tự được chọn** cho Checkout.
- ST-035 không tạo Order và không thanh toán.
- Hàm gọi AI do Tech Lead cung cấp.

## I. Dữ liệu và code hiện có

| Nguồn | DB tổng | Code hiện tại | Cách dùng |
|---|---:|---:|---|
| `card_templates` | Có | Chưa có | Template còn `is_active` và chưa `is_deleted`. |
| `generated_cards` | Có | Chưa có | Lưu input, ảnh kết quả và snapshot giá. |
| `client_histories` | Có | Chưa có | Chỉ tạo khi AI có ảnh hợp lệ. |
| `system_prompts` | Có | Chưa có | Lấy prompt có `type = card`. |
| `Configs` | Có | Có | Có thể lưu cấu hình Size/giá. |
| Checkout tạm | Không | Chưa có | Phụ thuộc ST-038. Không được giả lập bằng `orders`. |

Method Config có thể tái sử dụng:

```csharp
Task<List<Response.ConfigGroupResponse>> GetConfigGroups(string groupName);
```

Tên `group`, `key` và schema JSON cho Size vẫn phải được team chốt trước khi gọi method này.

## II. Các API cần có

| # | API | Mục đích |
|---:|---|---|
| 1 | `GET /api/v1/checkouts/{checkoutId}/card-generation-context` | Lấy Template, Size, giá và quota còn lại. |
| 2 | `POST /api/v1/checkouts/{checkoutId}/card-generations` | Tạo mới hoặc tạo lại một AI Job. |
| 3 | `GET /api/v1/card-generations/{generationId}` | FE polling trạng thái sau khi reload/rời màn hình. |
| 4 | `PUT /api/v1/checkouts/{checkoutId}/selected-card` | Xác nhận một card làm thiệp hiện tại của Checkout. |

Chỉ API 2 gọi AI và tính quota. API 4 không gọi AI, không tạo History, không tính quota.

## III. DB còn thiếu để chạy production

### `generated_cards`

Đề xuất bổ sung:

| Field | Kiểu | Ý nghĩa |
|---|---|---|
| `status` | `card_generation_status` | `running`, `generated`, `failed`. |
| `idempotency_key` | `varchar` | Chống một request tạo nhiều job/ảnh/quota. |
| `checkout_id` | `uuid` | Scope giới hạn 03 lần của phiên Checkout; không FK vì Checkout là phiên tạm. |
| `source_id` | `uuid` | ID mẫu hoa nguồn của Checkout. |
| `source_type` | `varchar` | Phân biệt `product` hoặc `generated_flower`. |
| `input_snapshot` | `json` | Payload cố định để job vẫn chạy khi Checkout/Core thay đổi. |

Index bắt buộc:

```text
UNIQUE (user_id, idempotency_key)
INDEX  (user_id, created_at, status)
INDEX  (checkout_id, status)
```

Không dùng `client_histories` để giữ job `running`: US yêu cầu **chưa có History khi chưa có ảnh hợp lệ**.

### Checkout

`db_tong.md` không có bảng Checkout. ST-038 yêu cầu Checkout là phiên tạm, còn sau refresh nhưng mất khi rời luồng.

Vì vậy ST-038 phải cung cấp một storage tạm, ví dụ Redis/session, chứa tối thiểu:

```text
checkoutId, userId, sourceType, sourceId, selectedCardId, isCompleted
```

Không dùng `generated_cards.order_id` làm `checkoutId`: trước khi hoàn tất chưa có Order.

### `is_confirmed`

`generated_cards.is_confirmed` không đủ để xác định card hiện tại vì một card có thể được dùng lại ở nhiều Checkout.

Nguồn đúng phải là:

```text
Checkout.selectedCardId
```

`is_confirmed` cần được bỏ hoặc định nghĩa lại; không dùng nó làm quan hệ hiện tại của Checkout.

## IV. Structure module

```text
HoaTheoMua.Service/CardGeneration/
├── Request.cs
├── Response.cs
├── IService.cs
└── Service.cs
```

### `Request.cs`

```csharp
namespace HoaTheoMua.Service.CardGeneration;

public class Request
{
    public class GetContextRequest
    {
        public Guid CheckoutId { get; set; }
    }

    public class GenerateCardRequest
    {
        public Guid CheckoutId { get; set; }
        public Guid TemplateId { get; set; }
        public string SizeKey { get; set; } = null!;
        public string FormType { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string ReceiverName { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public IFormFile? AttachedImage { get; set; }
        public string IdempotencyKey { get; set; } = null!;
    }

    public class GetGenerationRequest
    {
        public Guid GenerationId { get; set; }
    }

    public class SelectCardRequest
    {
        public Guid CheckoutId { get; set; }
        public Guid OutputId { get; set; }
    }
}
```

### `Response.cs`

```csharp
namespace HoaTheoMua.Service.CardGeneration;

public class Response
{
    public class ContextResponse
    {
        public List<TemplateItem> Templates { get; set; } = new();
        public List<SizeItem> Sizes { get; set; } = new();
        public int DailyRemaining { get; set; }
        public int CheckoutRemaining { get; set; }
    }

    public class TemplateItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }

    public class SizeItem
    {
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxWords { get; set; }
    }

    public class GenerationResponse
    {
        public Guid GenerationId { get; set; }
        public string Status { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public Guid? HistoryId { get; set; }
    }

    public class SelectCardResponse
    {
        public Guid CheckoutId { get; set; }
        public Guid SelectedCardId { get; set; }
        public decimal CurrentPrice { get; set; }
    }
}
```

### `IService.cs`

```csharp
namespace HoaTheoMua.Service.CardGeneration;

public interface IService
{
    Task<Response.ContextResponse> GetContext(Request.GetContextRequest request);
    Task<Response.GenerationResponse> GenerateCard(Request.GenerateCardRequest request);
    Task<Response.GenerationResponse> GetGeneration(Request.GetGenerationRequest request);
    Task<Response.SelectCardResponse> SelectCard(Request.SelectCardRequest request);
}
```

### Dependency từ ST-038 và Tech Lead

```csharp
public interface ICheckoutSessionService
{
    Task<CheckoutSnapshot> GetRequired(Guid checkoutId, Guid userId);
    Task SelectCard(Guid checkoutId, Guid userId, Guid cardId);
}

public class CheckoutSnapshot
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SourceType { get; set; } = null!;
    public Guid SourceId { get; set; }
    public Guid? SelectedCardId { get; set; }
    public bool IsCompleted { get; set; }
}

public interface ICardAiService
{
    Task<string> GeneratePng(CardAiInput input);
}

public interface ICardGenerationQueue
{
    Task Enqueue(Guid generationId);
}

public class CardAiInput
{
    public Guid GenerationId { get; set; }
    public string SourceSnapshot { get; set; } = null!;
    public string TemplateImageUrl { get; set; } = null!;
    public string FormType { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public string MessageContent { get; set; } = null!;
    public string? AttachedImageUrl { get; set; }
}
```

`ICheckoutSessionService` thuộc ST-038. `ICardAiService` do Tech Lead implement và trả URL ảnh PNG. `ICardGenerationQueue` phải dùng background-job bền vững để reload không làm mất job.

## V. Request, response và cách implement

### API 1 - Lấy context

```http
GET /api/v1/checkouts/{checkoutId}/card-generation-context
```

```json
{
  "data": {
    "templates": [],
    "sizes": [],
    "dailyRemaining": 7,
    "checkoutRemaining": 2
  }
}
```

Implementation:

1. Lấy `userId` từ `ICurrentUserService`.
2. Kiểm tra Checkout tồn tại, đúng owner, chưa hoàn tất.
3. Lấy `card_templates` active và chưa xóa.
4. Lấy Size/giá từ Core Config.
5. Đếm quota từ `generated_cards` có status `running/generated`.

### API 2 - Tạo/Tạo lại thiệp

```http
POST /api/v1/checkouts/{checkoutId}/card-generations
Content-Type: multipart/form-data
Idempotency-Key: {unique-key}
```

```text
templateId, sizeKey, formType,
senderName, receiverName, messageContent,
attachedImage? (PNG/JPG/JPEG, tối đa 10 MB)
```

```json
{
  "data": {
    "generationId": "C01",
    "status": "running",
    "imageUrl": null,
    "historyId": null
  }
}
```

Implementation core:

```text
1. Validate Checkout/owner/source hiện còn khả dụng.
2. Validate từ: sender <= 20, receiver <= 20, message <= 100.
3. Validate file, Template, Size và giá hiện hành.
4. Trong transaction có lock:
   - tìm (user_id, idempotency_key); có rồi thì trả job cũ;
   - kiểm tra quota ngày < 10;
   - kiểm tra running/generated của checkout_id < 3;
   - tạo generated_cards C01, status = running, lưu input_snapshot.
5. Đẩy C01 vào background job; trả 202 ngay.
6. AI thành công:
   - update C01: image_url, status = generated;
   - tạo H01: type = card, base_id = source_id, output_id = C01;
   - hai thao tác nằm trong cùng transaction.
7. AI thất bại hết retry:
   - update C01 status = failed;
   - không tạo History;
   - quota tự được hoàn vì chỉ đếm running/generated.
```

Snapshot tối thiểu của C01:

```json
{
  "schemaVersion": 1,
  "checkoutId": "CK01",
  "source": { "type": "generated_flower", "id": "F01" },
  "template": { "id": "T01", "name": "Template A", "imageUrl": "https://..." },
  "size": { "key": "S", "width": 10, "height": 15, "basePrice": 100000 },
  "priceRule": { "formType": "calligraphy", "extraPrice": 39000 }
}
```

Quy tắc ảnh:

- `go_may`: render nguyên văn sender, receiver, message.
- `calligraphy`: không render ba nội dung trên ảnh, nhưng vẫn lưu trong `generated_cards`.
- Ảnh đính kèm chỉ được scale đồng dạng/thêm padding.
- Output là PNG, không logo.

Giá backend tự tính lại:

```text
go_may      = size_base_price
calligraphy = size_base_price
              + 0       (0-35 từ)
              + 39.000  (36-70 từ)
              + 69.000  (71-100 từ)
```

### API 3 - Lấy trạng thái job

```http
GET /api/v1/card-generations/{generationId}
```

```json
{
  "data": {
    "generationId": "C01",
    "status": "generated",
    "imageUrl": "https://.../C01.png",
    "historyId": "H01"
  }
}
```

Implementation:

```text
generated_cards.id = generationId AND user_id = currentUser
→ lấy status/image_url
→ nếu generated, tìm client_histories:
   type = card AND output_id = generationId
```

API này cần vì reload không được hủy hoặc tạo lại AI Job.

### API 4 - Xác nhận card cho Checkout

```http
PUT /api/v1/checkouts/{checkoutId}/selected-card
Content-Type: application/json

{
  "outputId": "C01"
}
```

```json
{
  "data": {
    "checkoutId": "CK01",
    "selectedCardId": "C01",
    "currentPrice": 139000
  }
}
```

Implementation:

1. Kiểm tra Checkout/owner/chưa hoàn tất.
2. Kiểm tra C01 thuộc user, `status = generated`, có History và ảnh còn tồn tại.
3. Tính lại giá hiện hành theo Size + hình thức.
4. Ghi đè `Checkout.selectedCardId = C01` trong storage tạm.
5. Không gọi AI, không tạo History, không đổi quota.

Nếu bước 4 lỗi: giữ card/History/quota và giữ thiệp cũ trong Checkout.

### Controller mapping

```csharp
[HttpGet("{checkoutId:guid}/card-generation-context")]
public Task<IActionResult> GetContext(Guid checkoutId);

[HttpPost("{checkoutId:guid}/card-generations")]
public Task<IActionResult> GenerateCard(
    Guid checkoutId,
    [FromForm] Request.GenerateCardRequest request,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey);

[HttpGet("/api/v1/card-generations/{generationId:guid}")]
public Task<IActionResult> GetGeneration(Guid generationId);

[HttpPut("{checkoutId:guid}/selected-card")]
public Task<IActionResult> SelectCard(
    Guid checkoutId,
    [FromBody] Request.SelectCardRequest request);
```

Controller inject `HoaTheoMua.Service.CardGeneration.IService`; response JSON bọc bằng `ApiResponseFactory.Base(...)`.

## VI. Ba điểm cần hình dung

### 1. `generated_cards` và `client_histories` khác nhau thế nào?

```text
generated_cards = job + input snapshot + output AI
client_histories = mục lịch sử chỉ xuất hiện khi output thành công
```

Vì vậy job `running/failed` có thể tồn tại trong `generated_cards` nhưng không có History.

### 2. `base_id` và `output_id` lưu gì?

Ví dụ card C01 được tạo dựa trên mẫu hoa F01:

```text
H01.type      = card
H01.base_id   = F01
H01.output_id = C01
```

Truy vấn:

```text
type = card → generated_cards.id = output_id
```

Mỗi lần Tạo lại sinh C02/C03 và H02/H03 mới, nhưng cùng `base_id = F01`.

### 3. Quota được tính thế nào?

```text
Quota ngày:
user_id + created_at trong ngày Asia/Ho_Chi_Minh
+ status IN (running, generated) < 10

Quota phiên đặt hàng:
checkout_id + status IN (running, generated) < 3
```

Phải lock/transaction khi đếm và tạo C01 để hai request đồng thời không cùng vượt quota.

## VII. Điểm cần BA/Lead chốt

1. Storage và contract Checkout của ST-038.
2. Schema Config cho Size/giá và tên `group/key`.
3. Các field/index còn thiếu của `generated_cards` ở mục III.
4. `is_confirmed` được bỏ hay đổi ý nghĩa.
5. Một card được tái sử dụng cho nhiều Order nhưng `generated_cards.order_id` chỉ giữ được một ID; quan hệ Card–Order phải được ST-039 chốt riêng.
6. System-test nhắc `AC-015..019`, `EXC-010..012`, `ALT-03` nhưng STORY-035 hiện chỉ có `AC-001..014`, `EXC-01..09`, `ALT-01..02`; BA cần đồng bộ mã trace.

```csharp
checkouts
  - id uuid PK
  - user_id uuid
  - source_type varchar          // product | generated_flower
  - source_id uuid
  - selected_card_id uuid null
  - status enum                  // active | completed | abandoned | expired
  - expires_at timestamptz
  - created_at timestamptz
  - updated_at timestamptz
  
  Bảng này dùng để:

  - Kiểm tra owner.
  - Biết mẫu hoa nguồn.
  - Lưu thiệp đang chọn.
  - Giữ Checkout khi refresh.
  - Làm scope giới hạn 3 lần tạo thiệp.
  - Chưa cần tạo Order.  
```
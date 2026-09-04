# ST-033 - Tạo mẫu hoa bằng AI

## 0. Nguồn dùng để viết tài liệu

- US: `docs-us/STORY-033-toa-mau-hoa-bang-AI/STORY-033-tao-mau-hoa-bang-AI.md`.
- System test: toàn bộ test của ST-033, kể cả trạng thái `Đang duyệt`.
- Database: `diagram/db_tong.md`.
- Code hiện có trong solution.
- Luồng bản nháp và cách gom nhóm: `code-introduction/ST-030-introduction.md`.

> Code hiện tại chưa có service gọi AI. Tech Lead sẽ implement phần gọi AI và trả ảnh. Tài liệu này define contract đầu vào/đầu ra để các phần còn lại có thể triển khai thống nhất.

## I. Hiểu ngắn gọn US-33

US-33 nhận một bản nháp đã tạo ở ST-030, gửi snapshot của bản nháp cho AI và lưu từng kết quả tạo ảnh.

Luồng chính:

1. Khách hàng mở bản nháp của ST-030.
2. Hệ thống render lại tên, combo, vật liệu và mockup từ snapshot.
3. Khách hàng nhấn `Tạo bó hoa AI ngay`.
4. Hệ thống kiểm tra quyền sở hữu, dữ liệu, tồn kho combo, quota và job đang chạy.
5. Hệ thống tạo `generated_flowers` mới có trạng thái `running`.
6. Hệ thống tạo `client_histories` mới trỏ tới output trên.
7. Background job gọi hàm AI do Tech Lead implement.
8. Thành công: lưu `image_url`, chuyển trạng thái thành `generated`.
9. Thất bại sau tối đa 3 lần gọi: chuyển thành `failed`, lượt quota được hoàn.
10. Khi `Tạo lại`, hệ thống tạo output mới và giữ nguyên các ảnh cũ.

### Quy tắc dữ liệu đã chốt

- `generated_flowers` bản nháp gốc là nguồn đầu vào, ví dụ `F01`.
- Mỗi lần gọi AI tạo một record mới: `F02`, `F03`, ...
- Tất cả history cùng yêu cầu giữ `base_id = F01`.
- `output_id` trỏ đến record cụ thể của từng lần tạo.
- Không thêm bảng `flower_requests`, `client_history_inputs`, `ai_jobs` hoặc bảng quota.

| Hành động | History | `base_id` | `output_id` | Trạng thái output |
|---|---:|---:|---:|---|
| Lưu bản nháp | H01 | F01 | F01 | `draft` |
| Tạo lần đầu | H02 | F01 | F02 | `running/generated/failed` |
| Tạo lại lần 1 | H03 | F01 | F03 | `running/generated/failed` |
| Tạo lại lần 2 | H04 | F01 | F04 | `running/generated/failed` |

## II. Danh sách API

### 1. `GET /api/v1/client-histories/{historyId}`

API đã được define ở ST-030, cần mở rộng response để:

- Kiểm tra quyền sở hữu.
- Render tên, combo, vật liệu và mockup từ `input_snapshot`.
- Trả trạng thái của output hiện tại.
- Trả các thông tin bổ sung của ST-033 nếu đã lưu.

### 2. `GET /api/v1/flower-generations/quota`

- Trả số lượt tạo hoa AI còn lại trong ngày.
- Frontend disable nút tạo khi quota bằng `0`.
- Quota flower độc lập với quota card.

### 3. `POST /api/v1/client-histories/{historyId}/flower-generations`

Dùng chung cho `Tạo bó hoa AI ngay` và `Tạo lại`:

- Kiểm tra toàn bộ điều kiện trước khi nhận request.
- Tạo output `running` và history cùng nhóm.
- Đưa `outputId` cho background job.
- Trả `202 Accepted`, không chờ AI tạo xong ảnh.

### 4. `GET /api/v1/client-histories/{historyId}/flower-generations`

- Poll trạng thái sau khi reload hoặc quay lại màn hình.
- Trả toàn bộ các lần tạo cùng nhóm.
- Giữ và hiển thị các ảnh tạo thành công trước đó.
```HTML
- Some comments for above API
GET /api/v1/client-histories/{historyId}/flower-generations

dùng để lấy các record generated_flowers cùng nhóm và đọc trạng thái của chúng.

Ví dụ H01 có base_id = F01, API sẽ:

1. Lấy base_id = F01 từ H01.
2. Tìm các client_histories cùng base_id = F01.
3. Dùng từng output_id truy vấn generated_flowers.
4. Trả trạng thái và ảnh của F02, F03...

{
    "baseId": "F01",
    "items": [
        {
            "outputId": "F02",
            "status": "generated",
            "imageUrl": "image-f02.jpg"
        },
        {
            "outputId": "F03",
            "status": "running",
            "imageUrl": null
        }
    ]
}

Mục đích chính là để FE biết:

- running: AI vẫn đang tạo → hiển thị loading.
- generated: đã tạo xong → hiển thị ảnh.
- failed: tạo thất bại → cho phép thử lại.
- Khi reload trang, tiếp tục biết tiến trình hiện tại.
- Hiển thị những ảnh cũ trong cùng nhóm.

```



## III. Database/code hiện có và phần còn thiếu

### 1. Database tổng đã có đủ các field chính

| Bảng/field | Vai trò |
|---|---|
| `generated_flowers.input_snapshot` | Snapshot đầu vào của từng lần tạo. |
| `generated_flowers.image_url` | URL ảnh sau khi AI xử lý thành công. |
| `generated_flowers.flower_generation_status` | `draft`, `running`, `generated`, `failed`. |
| `generated_flowers.user_id` | Kiểm tra chủ sở hữu. |
| `client_histories.base_id` | ID bản nháp gốc, dùng để gom nhóm. |
| `client_histories.output_id` | ID output cụ thể trong `generated_flowers`. |
| `client_histories.type` | `flower` để xác định bảng cần truy vấn. |
| `system_prompts.type` | Chọn system prompt loại `flower`. |

### 2. Entity và DbSet chưa có trong solution

Database tổng có bảng nhưng code hiện chưa có đầy đủ:

- `GeneratedFlower`.
- `ClientHistory`.
- `Mockup`.
- `SystemPrompt`.
- Các `DbSet` tương ứng trong `AppDbContext`.

Phải implement phần entity/DbSet đã define ở ST-030 trước ST-033.

### 3. ST-030 chưa thu thập đủ input của ST-033

US-33 đề cập thêm:

- Dịp tặng.
- Phong cách.
- Ngân sách.
- Kích thước.
- Ghi chú.
- Giấy gói.
- Ruy-băng.

BA cần chốt:

- Trường nào bắt buộc.
- Giá trị tự do hay lấy từ danh mục.
- Ngân sách có phải giá combo hay giá trị riêng.
- Kích thước, giấy gói và ruy-băng có cần kiểm tra tồn kho không.

Trong khi chưa chốt, có thể lưu nullable trong `input_snapshot`; không được tự sinh dữ liệu giả.

### 4. Chưa có rule chọn `system_prompts`

Bảng `system_prompts` có thể có nhiều record `type = flower`, nhưng schema không có `is_active`, `version` hoặc `created_at`.

Cần xác định một trong hai rule:

- Mỗi `type` chỉ tồn tại đúng một record.
- Hoặc bổ sung cách xác định prompt đang hoạt động.

Không dùng `FirstOrDefault()` khi chưa có rule sắp xếp/chọn prompt.

### 5. Chưa có service xử lý và lưu ảnh

US yêu cầu validate ảnh, gắn logo và lưu storage. Code hiện chưa có service này.

Contract thuận tiện nhất là hàm của Tech Lead trả URL ảnh đã:

- Validate thành công.
- Gắn logo.
- Upload storage.

Nếu Tech Lead chỉ trả byte ảnh thô, dự án phải thêm image-processing/storage service.

### 6. Background job chưa được cấu hình

Solution có package Quartz nhưng chưa có job/configuration cho flower AI.

Không gọi AI trực tiếp trong HTTP request vì:

- API phải trả response nhận job nhanh.
- Reload hoặc rời trang không được hủy job.
- Trạng thái phải được lưu trong database.

## IV. Snapshot của một lần tạo

Tái sử dụng `ComboSnapshot`, `MockupSnapshot` của ST-030 và mở rộng:

```csharp
public sealed record FlowerGenerationSnapshot
{
    public required string Name { get; init; }
    public required ComboSnapshot Combo { get; init; }
    public required MockupSnapshot Mockup { get; init; }

    public string? Occasion { get; init; }
    public string? Style { get; init; }
    public decimal? Budget { get; init; }
    public string? Size { get; init; }
    public string? Note { get; init; }
    public string? WrappingPaper { get; init; }
    public string? Ribbon { get; init; }
}
```

Mỗi output AI phải giữ một bản snapshot riêng:

- F01 giữ snapshot bản nháp.
- F02 giữ snapshot dùng để tạo ảnh lần đầu.
- F03 giữ snapshot dùng để tạo lại lần tiếp theo.

Không đọc lại bảng nguồn để render lịch sử cũ vì tên, ảnh hoặc thành phần nguồn có thể đã thay đổi.

## V. API 1 - Lấy context để render

### Request

```http
GET /api/v1/client-histories/{historyId}
Authorization: Bearer {token}
```

### Response

```json
{
  "data": {
    "historyId": "H01",
    "baseId": "F01",
    "outputId": "F01",
    "status": "draft",
    "name": "Bó hoa kỷ niệm",
    "combo": {},
    "mockup": {},
    "occasion": null,
    "style": null,
    "budget": null,
    "size": null,
    "note": null,
    "wrappingPaper": null,
    "ribbon": null
  }
}
```

### Hàm service

```csharp
Task<FlowerHistoryDetailResponse> GetHistoryDetailAsync(
    Guid historyId,
    CancellationToken cancellationToken = default);
```

### Logic chính

```csharp
var userId = _currentUser.GetRequiredUserId(); // Hàm có sẵn.

var history = await _dbContext.ClientHistories
    .AsNoTracking()
    .SingleOrDefaultAsync(x =>
        x.Id == historyId &&
        x.UserId == userId &&
        x.Type == GenerationType.Flower &&
        !x.IsDeleted,
        cancellationToken);

if (history is null)
    throw new KeyNotFoundException("Không tìm thấy lịch sử tạo hoa.");

var flower = await _dbContext.GeneratedFlowers
    .AsNoTracking()
    .SingleOrDefaultAsync(x =>
        x.Id == history.OutputId &&
        x.UserId == userId &&
        !x.IsDeleted,
        cancellationToken);

if (flower is null)
    throw new KeyNotFoundException("Không tìm thấy mẫu hoa.");

var snapshot = JsonSerializer.Deserialize<FlowerGenerationSnapshot>(
    flower.InputSnapshot,
    _jsonOptions)
    ?? throw new InvalidOperationException("Snapshot mẫu hoa không hợp lệ.");
```

Không cần foreign key đa hình. Khi `type = flower`, dùng `output_id` truy vấn `generated_flowers.id`.

## VI. API 2 - Lấy quota flower AI

### Request

```http
GET /api/v1/flower-generations/quota
Authorization: Bearer {token}
```

### Response

```json
{
  "data": {
    "limit": 3,
    "used": 1,
    "remaining": 2,
    "resetAt": "2026-09-01T00:00:00+07:00"
  }
}
```

### DTO và interface

```csharp
public sealed record FlowerGenerationQuotaResponse(
    int Limit,
    int Used,
    int Remaining,
    DateTimeOffset ResetAt);

public interface IFlowerGenerationQuotaService
{
    Task<FlowerGenerationQuotaResponse> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
```

### Cách tính bằng database hiện tại

Không cần bảng quota. Một lượt đã sử dụng thỏa mãn:

- `client_histories.type = flower`.
- `client_histories.created_at` nằm trong ngày hiện tại theo `Asia/Ho_Chi_Minh`.
- `output_id` trỏ đến `generated_flowers` có status `running` hoặc `generated`.

`draft` không tốn quota. `failed` không được tính, tương đương hoàn quota đúng một lần.

```csharp
private const int DailyFlowerLimit = 3;

var used = await (
    from history in _dbContext.ClientHistories.AsNoTracking()
    join flower in _dbContext.GeneratedFlowers.AsNoTracking()
        on history.OutputId equals flower.Id
    where history.UserId == userId
        && history.Type == GenerationType.Flower
        && !history.IsDeleted
        && !flower.IsDeleted
        && history.CreatedAt >= startUtc
        && history.CreatedAt < endUtc
        && (flower.Status == FlowerGenerationStatus.Running
            || flower.Status == FlowerGenerationStatus.Generated)
    select history.Id
).CountAsync(cancellationToken);

var remaining = Math.Max(0, DailyFlowerLimit - used);
```

`startUtc` và `endUtc` là biên ngày `00:00` tại `Asia/Ho_Chi_Minh`, sau đó đổi sang UTC để query.

Retry nội bộ không tạo output/history mới nên không trừ quota lần nữa. Khi output chuyển `running -> failed`, nó tự không còn được tính trong quota.

## VII. API 3 - Tạo hoặc tạo lại ảnh

### Request

```http
POST /api/v1/client-histories/{historyId}/flower-generations
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "occasion": "Kỷ niệm",
  "style": "Thanh lịch",
  "budget": 800000,
  "size": "Medium",
  "note": "Tông màu ấm",
  "wrappingPaper": "Kraft sáng",
  "ribbon": "Kem"
}
```

Không nhận lại `comboId`, `mockupId` và `name` từ client. Chúng phải được lấy từ snapshot của history đã chọn.

### Response

```json
{
  "data": {
    "historyId": "H02",
    "baseId": "F01",
    "outputId": "F02",
    "status": "running",
    "remainingQuota": 2
  }
}
```

HTTP status: `202 Accepted`.

### Lỗi chính

| HTTP | Trường hợp |
|---:|---|
| `400` | Snapshot thiếu dữ liệu bắt buộc. |
| `403` | History không thuộc khách hàng hiện tại. |
| `404` | History, output, combo hoặc mockup không tồn tại. |
| `409` | Nhóm đang có output `running`. |
| `409` | Combo không còn đủ tồn kho. |
| `429` | Hết 3 lượt tạo flower AI trong ngày. |

### DTO

```csharp
public sealed record CreateFlowerGenerationRequest
{
    public string? Occasion { get; init; }
    public string? Style { get; init; }
    public decimal? Budget { get; init; }
    public string? Size { get; init; }
    public string? Note { get; init; }
    public string? WrappingPaper { get; init; }
    public string? Ribbon { get; init; }
}

public sealed record CreateFlowerGenerationResponse(
    Guid HistoryId,
    Guid BaseId,
    Guid OutputId,
    FlowerGenerationStatus Status,
    int RemainingQuota);
```

### Interface service

```csharp
public interface IFlowerGenerationService
{
    Task<CreateFlowerGenerationResponse> CreateAsync(
        Guid sourceHistoryId,
        CreateFlowerGenerationRequest request,
        CancellationToken cancellationToken = default);

    Task<FlowerGenerationGroupResponse> GetGroupAsync(
        Guid historyId,
        CancellationToken cancellationToken = default);
}
```

### Logic `CreateAsync`

```text
1. Lấy userId từ ICurrentUserService.
2. Mở database transaction.
3. Lấy sourceHistory theo historyId + userId + type flower.
4. rootId = sourceHistory.base_id.
5. Lock generated_flowers có id = rootId.
6. Kiểm tra trong nhóm rootId có output running không.
7. Tính quota trong ngày; nếu remaining = 0 thì từ chối.
8. Đọc snapshot từ sourceHistory.output_id.
9. Kiểm tra lại combo và số lượng vật liệu có thể bán.
10. Kiểm tra mockup còn active và chưa bị xóa.
11. Copy snapshot và ghép input mới của ST-033.
12. Tạo generated_flowers mới: outputId, status running, image_url null.
13. Tạo client_histories mới: base_id rootId, output_id outputId, type flower.
14. Commit transaction.
15. Schedule background job bằng outputId.
16. Trả 202 Accepted.
```

### Chống hai request đồng thời

Nếu chỉ dùng `AnyAsync(status == running)`, hai request có thể cùng đọc được `false` và cùng tạo output.

Tất cả request trong nhóm phải lock record gốc `F01`:

```sql
SELECT id
FROM generated_flowers
WHERE id = :baseId
FOR UPDATE;
```

Sau khi request thứ nhất commit, request thứ hai mới chạy tiếp và sẽ thấy output `running`.

```csharp
var hasRunningJob = await (
    from history in _dbContext.ClientHistories
    join flower in _dbContext.GeneratedFlowers
        on history.OutputId equals flower.Id
    where history.UserId == userId
        && history.Type == GenerationType.Flower
        && history.BaseId == rootId
        && flower.Status == FlowerGenerationStatus.Running
        && !history.IsDeleted
        && !flower.IsDeleted
    select flower.Id
).AnyAsync(cancellationToken);
```

### Kiểm tra lại tồn kho combo

Hàm có thật trong `Product.IService`:

```csharp
Task<ComboSpecificationResponse?> GetComboSpecificationAsync(Guid id);
```

```csharp
var combo = await _productService.GetComboSpecificationAsync(snapshot.Combo.Id);

if (combo is null)
    throw new KeyNotFoundException("Combo không tồn tại.");

if (combo.MaxComboPossible < 1)
    throw new InvalidOperationException("Combo không còn đủ nguyên liệu.");
```

Không gọi `GetLimitCoreProduct`: hàm này chỉ nằm trong `Order.Service`, chưa được expose qua `Order.IService`.

## VIII. Contract gọi AI do Tech Lead implement

```csharp
public interface IFlowerAiGenerator
{
    Task<FlowerAiImageResult> GenerateAsync(
        FlowerAiImageRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record FlowerAiImageRequest(
    Guid OutputId,
    string SystemPrompt,
    string UserPrompt,
    string ComboImageUrl,
    string MockupImageUrl);

public sealed record FlowerAiImageResult(string ImageUrl);
```

Trong contract trên, `ImageUrl` là ảnh cuối cùng đã validate, gắn logo và upload storage.

Nếu Tech Lead trả byte ảnh:

```csharp
public sealed record FlowerAiImageResult(
    byte[] Content,
    string ContentType);
```

Khi đó phải thêm service gắn logo/upload ảnh. Không lưu base64 vào `generated_flowers.image_url`.

### Thứ tự ưu tiên prompt

1. `Combo`: ràng buộc cứng về loại hoa và số lượng.
2. `Mockup`: bối cảnh, góc đặt hoa và bố cục.
3. `Note`: yêu cầu bổ sung của khách hàng.
4. `Style`: ưu tiên thấp nhất, không được làm sai combo.

Prompt mẫu:

```text
[HARD CONSTRAINT - COMBO]
Chỉ dùng các loại hoa/vật liệu và số lượng trong snapshot.

[SCENE - MOCKUP]
Đặt bó hoa theo bối cảnh, bố cục và góc chụp của mockup.

[CUSTOMER NOTE]
Áp dụng ghi chú nếu không mâu thuẫn với combo và mockup.

[STYLE]
Áp dụng phong cách nếu không làm thay đổi các ràng buộc trên.
```

## IX. Background job và retry

### Dispatcher

```csharp
public interface IFlowerGenerationDispatcher
{
    Task EnqueueAsync(
        Guid outputId,
        CancellationToken cancellationToken = default);
}
```

Implementation dùng Quartz. API chỉ truyền `outputId`; job tự đọc snapshot và prompt từ database.

### Job

```csharp
public sealed class FlowerGenerationJob : IJob
{
    private readonly AppDbContext _dbContext;
    private readonly IFlowerAiGenerator _aiGenerator;

    public FlowerGenerationJob(
        AppDbContext dbContext,
        IFlowerAiGenerator aiGenerator)
    {
        _dbContext = dbContext;
        _aiGenerator = aiGenerator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var outputId = Guid.Parse(
            context.MergedJobDataMap.GetString("outputId")!);

        // 1. Load generated_flower running theo outputId.
        // 2. Load snapshot và system prompt loại flower.
        // 3. Gọi AI tối đa 3 lần: lần đầu + 2 retry.
        // 4. Thành công: set ImageUrl và Generated.
        // 5. Hết retry: set Failed.
        // 6. SaveChanges; retry không tạo history/output mới.
    }
}
```

### Retry

```csharp
const int maxAttempts = 3;

for (var attempt = 1; attempt <= maxAttempts; attempt++)
{
    try
    {
        var result = await _aiGenerator.GenerateAsync(aiRequest);

        flower.ImageUrl = result.ImageUrl;
        flower.Status = FlowerGenerationStatus.Generated;
        flower.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return;
    }
    catch when (attempt < maxAttempts)
    {
        // Ghi log và thử lại; không tạo output/history, không trừ thêm quota.
    }
}

flower.Status = FlowerGenerationStatus.Failed;
flower.UpdatedAt = DateTime.UtcNow;
await _dbContext.SaveChangesAsync();
```

Không dùng `CancellationToken` của HTTP request cho background job. Người dùng reload hoặc đóng trang không được hủy job.

## X. API 4 - Lấy trạng thái và kết quả cùng nhóm

### Request

```http
GET /api/v1/client-histories/{historyId}/flower-generations
Authorization: Bearer {token}
```

### Response

```json
{
  "data": {
    "baseId": "F01",
    "isRunning": false,
    "items": [
      {
        "historyId": "H03",
        "outputId": "F03",
        "status": "generated",
        "imageUrl": "https://.../f03.webp",
        "createdAt": "2026-08-31T09:30:00Z"
      },
      {
        "historyId": "H02",
        "outputId": "F02",
        "status": "generated",
        "imageUrl": "https://.../f02.webp",
        "createdAt": "2026-08-31T09:20:00Z"
      }
    ]
  }
}
```

### Query

```csharp
var source = await _dbContext.ClientHistories
    .AsNoTracking()
    .SingleOrDefaultAsync(x =>
        x.Id == historyId &&
        x.UserId == userId &&
        x.Type == GenerationType.Flower &&
        !x.IsDeleted,
        cancellationToken);

if (source is null)
    throw new KeyNotFoundException("Không tìm thấy lịch sử tạo hoa.");

var rootId = source.BaseId;

var items = await (
    from history in _dbContext.ClientHistories.AsNoTracking()
    join flower in _dbContext.GeneratedFlowers.AsNoTracking()
        on history.OutputId equals flower.Id
    where history.UserId == userId
        && history.Type == GenerationType.Flower
        && history.BaseId == rootId
        && !history.IsDeleted
        && !flower.IsDeleted
    orderby history.CreatedAt descending
    select new FlowerGenerationItemResponse(
        history.Id,
        flower.Id,
        flower.Status,
        flower.ImageUrl,
        history.CreatedAt)
).ToListAsync(cancellationToken);
```

Frontend xử lý:

- `running`: hiển thị loading và poll API.
- `generated`: hiển thị ảnh và các action hợp lệ.
- `failed`: hiển thị lỗi và cho phép tạo lại.
- `draft`: không xem là kết quả AI.

US ghi failed thì không tạo history item. Với thiết kế hiện tại, history vật lý được tạo lúc nhận job để giữ `base_id/output_id`. Record failed không được hiển thị trong danh sách kết quả hoàn thành, nhưng vẫn giữ trong database để audit và tính quota đúng.

## XI. Ba vấn đề dễ nhầm

### 1. Làm sao biết các lần generate cùng nhóm?

Dùng `client_histories.base_id`:

- Bản nháp: `H01.base_id = F01`.
- Tạo lần đầu: `H02.base_id = F01`.
- Tạo lại từ F02: đọc `H02.base_id`, vẫn nhận được F01.
- History tiếp theo tiếp tục lưu `base_id = F01`.

Không lấy `F02` làm base mới.

### 2. `output_id` dùng để làm gì?

- `base_id`: record thuộc nhóm nào.
- `output_id`: history đang nói đến output cụ thể nào.

Khi `type = flower`, lấy `output_id` truy vấn `generated_flowers.id`. Không cần foreign key đa hình.

### 3. Render lại combo, mockup, tên và lựa chọn từ đâu?

Đọc `generated_flowers.input_snapshot` của `output_id`:

- F01 giữ snapshot bản nháp.
- F02/F03 giữ snapshot bất biến của từng lần AI chạy.
- Không dùng `base_id` để render một kết quả cụ thể.
- Không đọc bảng `products`/`mockup` để render lịch sử cũ.
- Chỉ đọc bảng nguồn để kiểm tra điều kiện hiện tại trước lần tạo mới.

## XII. Các điểm BA/Tech Lead cần xác nhận

1. Dịp, phong cách, ngân sách, kích thước, giấy gói và ruy-băng có bắt buộc không.
2. Các trường trên là text hay ID từ danh mục.
3. Cách xác định `system_prompt` flower đang active.
4. Hàm AI trả URL ảnh hoàn chỉnh hay byte ảnh thô.
5. Ai chịu trách nhiệm validate, gắn logo và upload ảnh.
6. Quartz dùng memory store hay persistent store; production cần phục hồi job `running` khi server restart.
7. Quota card cần lifecycle/status riêng; không trộn quota card với flower.

## XIII. Checklist system test

- [ ] Lần đầu thành công: quota `3 -> 2`, `running -> generated`, đúng một ảnh.
- [ ] Prompt nhận đúng snapshot và ưu tiên `Combo > Mockup > Note > Style`.
- [ ] Kết quả có cảnh báo ảnh chỉ mang tính minh họa.
- [ ] Tạo lại sinh output/history mới và giữ ảnh cũ.
- [ ] Reload/rời trang không hủy job.
- [ ] Hết quota thì disable/từ chối; reset lúc `00:00 Asia/Ho_Chi_Minh`.
- [ ] Hai request đồng thời trong cùng nhóm chỉ một request được nhận.
- [ ] AI gọi tối đa 3 lần; failed hoàn quota đúng một lần.
- [ ] Sai chủ sở hữu/không tồn tại không trừ quota và không lộ dữ liệu.
- [ ] Combo hết tồn kho bị chặn trước khi gọi AI và không trừ quota.
- [ ] Quota flower và card độc lập.

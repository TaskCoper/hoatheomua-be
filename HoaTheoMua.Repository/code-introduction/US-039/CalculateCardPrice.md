# CalculateCardPrice

## Mục đích

Đây là hàm BE dùng để báo giá thiệp cho FE và được `CreateOrder` gọi lại để chốt tiền thật.

Hàm này **không** tạo Order, không gọi AI và không tạo `GeneratedCard`. Nó chỉ trả lời một việc:

```text
Với loại thiệp, nội dung, Size, hình thức chữ và số lượng khách đã chọn,
giá thiệp là bao nhiêu?
```

FE gọi hàm để hiển thị tiền. Khi khách bấm đặt đơn, BE phải gọi lại cùng hàm này; không tin giá FE gửi lên.

---

## Dữ liệu hàm nhận

```csharp
public class CardPriceInput
{
    // handmade | ai | history
    public required string CardMode { get; set; }

    // Template khách chọn. Dùng để xác định đúng loại Template.
    public required Guid CardTemplateId { get; set; }

    // Chỉ dùng với ai hoặc history.
    public string? SizeName { get; set; }

    // Chỉ dùng với ai hoặc history: go_may | calligraphy.
    // handmade luôn được BE gán là calligraphy.
    public string? FormType { get; set; }

    public string? MessageContent { get; set; }

    // Số bản in của đúng thiết kế thiệp này.
    public int Quantity { get; set; }
}
```

`source`, `attachedImageUrl`, `systemPrompt` và các thông tin tạo AI không làm thay đổi giá. Chúng được `CreateOrder` dùng để ghép thành `SnapshotCard` sau khi giá đã được tính xong.

---

## Ba cách tính giá

| Loại thiệp | Size | Hình thức chữ | Công thức một thiệp |
|---|---|---|---|
| `handmade` (thiệp thường) | Không dùng Size | Luôn `calligraphy` | `extraPrice` từ rule số từ |
| `ai` | Bắt buộc chọn Size | `go_may` | `size.base_price` |
| `ai` | Bắt buộc chọn Size | `calligraphy` | `size.base_price + rule.extra_price` |
| `history` | Bắt buộc chọn Size | `go_may` | `size.base_price` |
| `history` | Bắt buộc chọn Size | `calligraphy` | `size.base_price + rule.extra_price` |

Giá cả dòng thiệp là:

```text
lineTotal = unitPrice × quantity
```

Ví dụ: một thiệp AI Size Lớn giá Size `15.000`, viết tay phụ thu `39.000`, đặt 2 bản:

```text
unitPrice = 15.000 + 39.000 = 54.000
lineTotal = 54.000 × 2 = 108.000
```

---

## Flow bên trong hàm

```text
Nhận input
→ Đếm số từ trong messageContent
→ Lấy Template để xác định đúng loại thiệp
→ Đi vào nhánh Handmade hoặc AI/History
→ Nếu cần, đọc Size Config và Price Config
→ Chọn rule giá đúng với số từ
→ Tính unitPrice và lineTotal
→ Trả phần giá đã resolve cho FE hoặc CreateOrder
```

### Nhánh 1 — Thiệp thường (`handmade`)

```text
Không đọc Size Config
→ formType được BE cố định là calligraphy
→ đếm số từ
→ đọc Price Config
→ tìm rule chứa wordCount
→ basePrice = 0
→ extraPrice = rule.extra_price
→ unitPrice = extraPrice
```

Ví dụ lời chúc có 5 từ và rule `0–35 từ` có phụ thu `0`:

```text
basePrice  = 0
extraPrice = 0
unitPrice  = 0
```

### Nhánh 2 — Thiệp AI hoặc thiệp từ lịch sử (`ai` / `history`)

```text
Đọc Size Config theo sizeName khách chọn
→ basePrice = size.base_price
→ nếu go_may:
     cardConfig = null
     extraPrice = 0
→ nếu calligraphy:
     đếm số từ
     kiểm tra không vượt size.max_words
     đọc Price Config
     tìm rule chứa wordCount
     cardConfig = rule đã chọn
     extraPrice = rule.extra_price
→ unitPrice = basePrice + extraPrice
```

---

## Code logic đầy đủ để hình dung

Đây vẫn là code thiết kế trong tài liệu, chưa phải code đã có trong Service. Tuy nhiên, mọi hàm được gọi bên dưới đều được định nghĩa ngay trong phần này để có thể đọc trọn mạch xử lý.

```csharp
private async Task<CardPriceQuote> CalculateCardPrice(CardPriceInput input)
{
    EnsureSupportedCardMode(input.CardMode);
    EnsurePositiveQuantity(input.Quantity);

    var wordCount = CountWords(input.MessageContent);
    var template = await GetUsableCardTemplate(input.CardTemplateId);

    if (input.CardMode == "handmade")
    {
        EnsureTemplateType(template, "handmade");

        var priceRule = await GetPriceRule(wordCount);
        var unitPrice = priceRule.ExtraPrice;

        return new CardPriceQuote
        {
            CardMode = "handmade",
            FormType = "calligraphy",
            CardSize = null,
            CardConfig = priceRule,
            WordCount = wordCount,
            BasePrice = 0,
            ExtraPrice = priceRule.ExtraPrice,
            UnitPrice = unitPrice,
            Quantity = input.Quantity,
            TotalPrice = unitPrice * input.Quantity
        };
    }

    EnsureTemplateType(template, "ai");

    var size = await GetCardSize(input.SizeName);
    EnsureAiOrHistoryFormType(input.FormType);

    if (input.FormType == "go_may")
    {
        return new CardPriceQuote
        {
            CardMode = input.CardMode,
            FormType = "go_may",
            CardSize = size,
            CardConfig = null,
            WordCount = wordCount,
            BasePrice = size.BasePrice,
            ExtraPrice = 0,
            UnitPrice = size.BasePrice,
            Quantity = input.Quantity,
            TotalPrice = size.BasePrice * input.Quantity
        };
    }

    // Chỉ viết tay mới tính/phụ thu theo nội dung, nên chỉ nhánh này mới kiểm tra số từ theo Size.
    EnsureMessageDoesNotExceedSize(wordCount, size.MaxWords);
    var calligraphyRule = await GetPriceRule(wordCount);
    var calligraphyUnitPrice = size.BasePrice + calligraphyRule.ExtraPrice;

    return new CardPriceQuote
    {
        CardMode = input.CardMode,
        FormType = "calligraphy",
        CardSize = size,
        CardConfig = calligraphyRule,
        WordCount = wordCount,
        BasePrice = size.BasePrice,
        ExtraPrice = calligraphyRule.ExtraPrice,
        UnitPrice = calligraphyUnitPrice,
        Quantity = input.Quantity,
        TotalPrice = calligraphyUnitPrice * input.Quantity
    };
}

private static void EnsureSupportedCardMode(string cardMode)
{
    if (cardMode is not ("handmade" or "ai" or "history"))
    {
        throw new BadRequestException("Loại thiệp không hợp lệ");
    }
}

private static void EnsurePositiveQuantity(int quantity)
{
    if (quantity <= 0)
    {
        throw new BadRequestException("Số lượng thiệp phải lớn hơn 0");
    }
}

private static int CountWords(string? messageContent)
{
    if (string.IsNullOrWhiteSpace(messageContent))
    {
        return 0;
    }

    return messageContent
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        .Length;
}

private async Task<CardTemplate> GetUsableCardTemplate(Guid cardTemplateId)
{
    var template = await _dbContext.CardTemplates
        .AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == cardTemplateId && !x.IsDeleted);

    if (template == null)
    {
        throw new NotFoundException("Không tìm thấy mẫu thiệp");
    }

    if (!template.IsActive)
    {
        throw new BadRequestException("Mẫu thiệp đang tạm ngưng");
    }

    return template;
}

private static void EnsureTemplateType(CardTemplate template, string expectedType)
{
    var actualType = template.TemplateType == CardTemplateType.Ai
        ? "ai"
        : "handmade";

    if (actualType != expectedType)
    {
        throw new BadRequestException("Loại Template không phù hợp với loại thiệp");
    }
}

private static void EnsureAiOrHistoryFormType(string? formType)
{
    if (formType is not ("go_may" or "calligraphy"))
    {
        throw new BadRequestException("Hình thức chữ phải là go_may hoặc calligraphy");
    }
}

private async Task<CardSizeQuote> GetCardSize(string? sizeName)
{
    if (string.IsNullOrWhiteSpace(sizeName))
    {
        throw new BadRequestException("Thiệp AI hoặc thiệp từ lịch sử phải chọn Size");
    }

    var config = await GetActiveCardConfig("card_config.sizes");
    var sizes = ParseConfigItems<CardSizeQuote>(config.Value);

    var size = sizes.SingleOrDefault(x =>
        string.Equals(x.Name, sizeName, StringComparison.OrdinalIgnoreCase));

    if (size == null)
    {
        throw new BadRequestException("Không tìm thấy Size thiệp đã chọn");
    }

    return size;
}

private async Task<CardPriceRuleQuote> GetPriceRule(int wordCount)
{
    var config = await GetActiveCardConfig("card_config.prices");
    var rules = ParseConfigItems<CardPriceRuleQuote>(config.Value);

    var rule = rules.SingleOrDefault(x =>
        x.MinWords <= wordCount && wordCount <= x.MaxWords);

    if (rule == null)
    {
        throw new BadRequestException("Không có bậc giá phù hợp với số từ của lời chúc");
    }

    return rule;
}

private async Task<Config> GetActiveCardConfig(string key)
{
    var configs = await _dbContext.Configs
        .AsNoTracking()
        .Where(x =>
            x.Group == "card_config" &&
            x.Key == key &&
            x.Kind == "Setting" &&
            x.IsPublic &&
            !x.IsDeleted)
        .ToListAsync();

    if (configs.Count == 0)
    {
        throw new NotFoundException("Không tìm thấy cấu hình thiệp");
    }

    if (configs.Count > 1)
    {
        throw new BadRequestException("Cấu hình thiệp đang bị trùng");
    }

    return configs[0];
}

private static List<TItem> ParseConfigItems<TItem>(string configValue)
{
    var wrapper = JObject.Parse(configValue);
    var innerJson = wrapper["value"]?.Value<string>();

    if (string.IsNullOrWhiteSpace(innerJson))
    {
        throw new BadRequestException("Cấu hình thiệp không đúng cấu trúc");
    }

    var items = JObject.Parse(innerJson)["items"]?.ToObject<List<TItem>>();
    if (items == null || items.Count == 0)
    {
        throw new BadRequestException("Cấu hình thiệp không có dữ liệu");
    }

    return items;
}

private static void EnsureMessageDoesNotExceedSize(int wordCount, int maxWords)
{
    if (wordCount > maxWords)
    {
        throw new BadRequestException("Lời chúc vượt số từ tối đa của Size thiệp");
    }
}

private sealed class CardSizeQuote
{
    public string Name { get; set; } = string.Empty;
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    [JsonProperty("base_price")]
    public decimal BasePrice { get; set; }
    [JsonProperty("max_words")]
    public int MaxWords { get; set; }
}

private sealed class CardPriceRuleQuote
{
    [JsonProperty("min_words")]
    public int MinWords { get; set; }
    [JsonProperty("max_words")]
    public int MaxWords { get; set; }
    [JsonProperty("extra_price")]
    public decimal ExtraPrice { get; set; }
}

private sealed class CardPriceQuote
{
    public string CardMode { get; set; } = string.Empty;
    public string FormType { get; set; } = string.Empty;
    public CardSizeQuote? CardSize { get; set; }
    public CardPriceRuleQuote? CardConfig { get; set; }
    public int WordCount { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ExtraPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
```

Khi đưa phần này vào code thật, file Service cần dùng các namespace quen thuộc của project:

```csharp
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
```

---

## Response cho FE

```json
{
  "items": [
    {
      "cardMode": "ai",
      "formType": "calligraphy",
      "cardSize": {
        "name": "Lớn",
        "width": 15,
        "height": 25,
        "base_price": 15000,
        "max_words": 100
      },
      "cardConfig": {
        "min_words": 36,
        "max_words": 70,
        "extra_price": 39000
      },
      "wordCount": 40,
      "basePrice": 15000,
      "extraPrice": 39000,
      "unitPrice": 54000,
      "quantity": 2,
      "totalPrice": 108000
    }
  ],
  "cardTotal": 108000
}
```

`cardTotal` là tổng tiền thiệp. FE cộng nó với tiền hoa, ship và thuế để hiển thị tổng đơn dự kiến.

---

## Khi CreateOrder chạy

```text
CreateOrder nhận dữ liệu thiệp
→ gọi lại CalculateCardPrice
→ lấy kết quả giá do BE tính
→ ghép thêm source, Template snapshot, attachedImageUrl, Quantity, System Prompt và validatedAt
→ lưu các object đã hoàn chỉnh thành mảng JSON OrderDetails.SnapshotCard
→ cộng cardTotal vào Order.TotalFee
```

Sau bước này, giá trong SnapshotCard đã được chốt. Khi thanh toán thành công, Card BE dùng SnapshotCard để tạo ảnh; không gọi lại `CalculateCardPrice` và không lấy giá Config hiện hành nữa.

# US-039 — API tính giá thiệp trước thanh toán

## 1. Rule FE

Với mỗi bó hoa / mỗi OrderDetail, FE chọn đúng một option:

```text
Thiệp thường (HandMade)
HOẶC
Thiệp Custom AI
HOẶC
Thiệp từ History
```

Nếu chọn `history`, FE vẫn được chọn/nhập lại toàn bộ thông tin Card như tạo mới:

```text
Template, formType, người gửi, người nhận, lời nhắn
```

Card History chỉ là nguồn để backend kiểm tra Card đó thuộc customer và đúng Product gốc của bó hoa. Giá và dữ liệu tạo Card mới không lấy cố định từ Card cũ.

## 2. API quote giá

```http
POST /api/v1/card-price
```

Request chung cho cả ba option:

```json
{
  "type": "history",
  "sourceGeneratedCardId": "11111111-1111-1111-1111-111111111111",
  "cardTemplateId": "22222222-2222-2222-2222-222222222222",
  "formType": "calligraphy",
  "senderName": "Minh",
  "receiverName": "Lan",
  "messageContent": "Chúc bạn luôn hạnh phúc"
}
```

`sourceGeneratedCardId`:

- Bắt buộc khi `type = history`.
- Không gửi khi `type = standard` hoặc `type = custom_ai`.
- Không dùng để lấy giá cũ.

Response:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "value": {
    "basePrice": 20000,
    "extraPrice": 39000,
    "totalPrice": 59000
  }
}
```

## 3. Cách tính giá

| Template được chọn hiện tại | Form | `basePrice` | `extraPrice` | `totalPrice` |
|---|---|---:|---:|---:|
| HandMade | Luôn viết tay | `0` | Bậc giá theo số từ hiện tại | `extraPrice` |
| AI | Gõ máy | Giá Size hiện tại | `0` | `basePrice` |
| AI | Viết tay | Giá Size hiện tại | Bậc giá theo số từ hiện tại | `basePrice + extraPrice` |

Tất cả đều lấy Size Config và Calligraphy Price Config **hiện tại**. Không dùng `GeneratedCard.BasePrice`, `ExtraPrice` hay `TotalPrice` cũ.

Thiệp thường là Template `handmade`, luôn viết tay. Mỗi bó hoa chỉ được tối đa một thiệp HandMade; History chọn lại với Template HandMade cũng tính vào rule này.

## 4. Code đơn giản

```csharp
public enum CardChoiceType
{
    Standard,
    CustomAi,
    History
}

public class CalculateCardPriceRequest
{
    public CardChoiceType Type { get; set; }
    public Guid? SourceGeneratedCardId { get; set; } // chỉ History
    public Guid? CardTemplateId { get; set; }
    public CardFormType? FormType { get; set; }
    public string? SenderName { get; set; }
    public string? ReceiverName { get; set; }
    public string? MessageContent { get; set; }
}

public class CardPriceResponse
{
    public decimal BasePrice { get; set; }
    public decimal ExtraPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public async Task<CardPriceResponse> CalculateCardPrice(
    CalculateCardPriceRequest request)
{
    // History chỉ validate Card nguồn. Không lấy giá hoặc nội dung cũ.
    if (request.Type == CardChoiceType.History)
    {
        await ValidateOwnedHistoryCard(request.SourceGeneratedCardId);
    }

    // Cả ba option đều dùng Template và nội dung do FE chọn hiện tại.
    var template = await GetActiveTemplate(request.CardTemplateId);
    var size = await GetCurrentCardSize();
    var priceRules = await GetCurrentCalligraphyPriceRules();

    // Standard bắt buộc dùng template HandMade.
    if (request.Type == CardChoiceType.Standard &&
        template.TemplateType != CardTemplateType.Handmade)
    {
        throw new BadRequestException("STANDARD_CARD_TEMPLATE_MUST_BE_HANDMADE");
    }

    // Custom AI bắt buộc dùng template AI.
    if (request.Type == CardChoiceType.CustomAi &&
        template.TemplateType != CardTemplateType.Ai)
    {
        throw new BadRequestException("CUSTOM_AI_TEMPLATE_MUST_BE_AI");
    }

    // History được chọn lại hoàn toàn, nên Template hiện tại quyết định AI/HandMade.
    var isAi = template.TemplateType == CardTemplateType.Ai;

    // HandMade luôn viết tay. AI cho chọn gõ máy hoặc viết tay.
    var formType = isAi
        ? request.FormType
            ?? throw new BadRequestException("CARD_FORM_TYPE_REQUIRED")
        : CardFormType.Calligraphy;

    var message = request.MessageContent?.Trim() ?? string.Empty;
    var wordCount = CountWords(message);

    if (wordCount > size.MaxWords)
        throw new BadRequestException("CARD_MESSAGE_EXCEEDS_MAX_WORDS");

    var isCalligraphy = !isAi || formType == CardFormType.Calligraphy;

    var extraPrice = 0m;
    if (isCalligraphy)
    {
        var rule = priceRules.SingleOrDefault(x =>
            x.MinWords <= wordCount && wordCount <= x.MaxWords);

        if (rule == null)
            throw new BadRequestException("CALLIGRAPHY_PRICE_RULE_NOT_FOUND");

        extraPrice = rule.ExtraPrice;
    }

    var basePrice = isAi ? size.BasePrice : 0m;

    return new CardPriceResponse
    {
        BasePrice = basePrice,
        ExtraPrice = extraPrice,
        TotalPrice = basePrice + extraPrice
    };
}

private static int CountWords(string message) =>
    string.IsNullOrWhiteSpace(message)
        ? 0
        : message.Split(
            [' ', '\t', '\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries).Length;
```

## 5. Dùng trong CreateOrder

FE gọi API để preview. Khi customer bấm tạo Order, backend gọi lại đúng `CalculateCardPrice` bằng dữ liệu request để chốt tiền:

```csharp
var cardPrice = await CalculateCardPrice(request.Card);
var finalProductFee = productFee + cardPrice.TotalPrice;
```

Trước payment, Order chỉ lưu lựa chọn Card và giá vừa quote. Không tạo AI, ảnh hoặc `GeneratedCard`.

Sau `Payment SUCCESS`:

```text
Đọc snapshot Card từ Order
→ tạo Card mới bằng dữ liệu customer đã chọn
→ nhận GeneratedCardId + imageUrl mới
→ gắn GeneratedCardId mới vào OrderDetails.GeneratedCardIds
```

using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HoaTheoMua.Service.Card;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public Service(AppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    //Handmade not call AI
    public async Task<Response.CreateCardResponse> CreateHandMadeCard(Request.CreateCardRequest request)
    {
        if (request.ProductId.HasValue == request.GeneratedFlowerId.HasValue)
            throw new BadRequestException("Chỉ chọn Product hoặc GeneratedFlower.");

        var userId = _currentUser.GetRequiredUserId();
        var template = await _dbContext.CardTemplates
            .FirstOrDefaultAsync(x => x.Id == request.CardTemplateId && !x.IsDeleted);
        if (template == null) throw new NotFoundException("Không tìm thấy CardTemplate.");
        if (!template.IsActive || template.TemplateType != CardTemplateType.Handmade || string.IsNullOrWhiteSpace(template.ImageUrl))
            throw new ConflictException("CardTemplate không hợp lệ cho HandMade.");

        Guid baseId;
        Guid rootId;
        if (request.ProductId.HasValue)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId && !x.IsDeleted)
                ?? throw new NotFoundException("Không tìm thấy Product.");
            if (!product.IsActive || product.AvailableForSale.GetValueOrDefault() <= 0)
                throw new ConflictException("Product không còn bán được.");
            baseId = rootId = product.Id;
        }
        else
        {
            var flower = await _dbContext.GeneratedFlowers.FirstOrDefaultAsync(x => x.Id == request.GeneratedFlowerId && x.UserId == userId)
                ?? throw new NotFoundException("Không tìm thấy GeneratedFlower.");
            if (string.IsNullOrWhiteSpace(flower.ImageUrl))
                throw new ConflictException("GeneratedFlower thiếu ảnh hợp lệ.");

            var flowerHistory = await _dbContext.ClientHistories.SingleOrDefaultAsync(x =>
                x.UserId == userId && x.Type == ClientHistoryType.Flower && x.OutputId == flower.Id)
                ?? throw new ConflictException("GeneratedFlower không có lineage hợp lệ.");
            if (flowerHistory.RootId == Guid.Empty)
                throw new ConflictException("GeneratedFlower không có Product gốc hợp lệ.");
            baseId = flower.Id;
            rootId = flowerHistory.RootId;
        }

        if (request.MessageContent is null)
            throw new BadRequestException("MessageContent là bắt buộc.");

        var sizeConfig = await LoadCardConfigAsync("card_config.sizes");
        var priceConfig = await LoadCardConfigAsync("card_config.prices");
        var sizes = DeserializeConfig<ConfigItems<SizeConfigItem>>(sizeConfig).Items;
        var priceRules = DeserializeConfig<ConfigItems<PriceConfigItem>>(priceConfig).Items;
        if (sizes.Count == 0 || priceRules.Count == 0)
            throw new ConflictException("Card Config không có dữ liệu sử dụng được.");

        var sizeName = request.SizeName?.Trim();
        var size = sizes.FirstOrDefault(x => string.Equals(x.Name, sizeName, StringComparison.Ordinal));
        if (size == null)
            throw new BadRequestException("Size không tồn tại trong Card Config.");

        var wordCount = request.MessageContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount > 100 || wordCount > size.MaxWords)
            throw new BadRequestException("MessageContent vượt quá giới hạn số từ.");

        var matchingRules = priceRules.Where(x => x.MinWords <= wordCount && wordCount <= x.MaxWords).ToList();
        if (matchingRules.Count != 1)
            throw new ConflictException("Không tìm thấy duy nhất Price Rule phù hợp.");
        var priceRule = matchingRules[0];

        var card = new GeneratedCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            Content = request.MessageContent,
            ImageUrl = template.ImageUrl,
            RawImage = null,
            CardType = GeneratedCardType.Handmade,
            FormType = CardFormType.Calligraphy,
            MessageContent = request.MessageContent,
            AttachedImageUrl = null,
            SizeName = size.Name,
            SizeWidth = size.Width,
            SizeHeight = size.Height,
            SizeBasePrice = size.BasePrice,
            SizeMaxWords = size.MaxWords,
            WordCount = wordCount,
            WordConfigSnapshot = JsonSerializer.Serialize(priceRule),
            BasePrice = 0,
            ExtraPrice = priceRule.ExtraPrice,
            TotalPrice = priceRule.ExtraPrice
        };

        var history = new ClientHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ClientHistoryType.HandmadeCard,
            BaseId = baseId,
            RootId = rootId,
            OutputId = card.Id,
            CreatedAt = card.CreatedAt,
            SystemForm = null,
            Metadata = JsonSerializer.Serialize(new
            {
                sourceType = request.ProductId.HasValue ? "product" : "generated_flower",
                originProductId = rootId,
                generatedFlowerId = request.GeneratedFlowerId,
                provenanceStatus = "available",
                cardTemplate = new
                {
                    id = template.Id,
                    name = template.Name,
                    templateType = "handmade"
                },
                sizeConfig = new
                {
                    group = "card_config",
                    key = "card_config.sizes"
                },
                priceConfig = new
                {
                    group = "card_config",
                    key = "card_config.prices"
                },
                validatedAt = DateTimeOffset.UtcNow
            })
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.GeneratedCards.Add(card);
        _dbContext.ClientHistories.Add(history);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return ToResponse(card);
    }

    private async Task<JsonElement> LoadCardConfigAsync(string key)
    {
        var configs = await _dbContext.Configs
            .Where(x => x.Group == "card_config" && x.Key == key && !x.IsDeleted && x.Kind == "Setting")
            .ToListAsync();
        if (configs.Count != 1)
            throw new ConflictException($"Card Config {key} không tồn tại duy nhất.");
        if (!configs[0].IsPublic)
            throw new ConflictException($"Card Config {key} không active/public.");

        try
        {
            using var outer = JsonDocument.Parse(configs[0].Value);
            if (!outer.RootElement.TryGetProperty("value", out var value))
                throw new JsonException();
            if (value.ValueKind == JsonValueKind.String)
            {
                using var inner = JsonDocument.Parse(value.GetString()!);
                return inner.RootElement.Clone();
            }
            return value.Clone();
        }
        catch (JsonException)
        {
            throw new ConflictException($"Card Config {key} không hợp lệ.");
        }
    }

    private static T DeserializeConfig<T>(JsonElement value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw new ConflictException("Card Config không thể parse.");
        }
    }

    private sealed class ConfigItems<T>
    {
        public List<T> Items { get; set; } = new();
    }

    private sealed class SizeConfigItem
    {
        public string Name { get; set; } = "";
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("base_price")]
        public decimal BasePrice { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
    }

    private sealed class PriceConfigItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("min_words")]
        public int MinWords { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("extra_price")]
        public decimal ExtraPrice { get; set; }
    }

    public async Task<Response.CreateCardResponse> CreateCardAi(Request.CreateCardAiRequest request)
    {
        if (request.OrderDetailId.HasValue == request.SourceCardId.HasValue)
            throw new BadRequestException("Chỉ chọn OrderDetail hoặc sourceCard.");

        var userId = _currentUser.GetRequiredUserId();
        if (request.SourceCardId.HasValue)
        {
            var sourceCard = await _dbContext.GeneratedCards
                .FirstOrDefaultAsync(x => x.Id == request.SourceCardId && x.UserId == userId)
                ?? throw new NotFoundException("Không tìm thấy source Card.");
            if (sourceCard.CardType != GeneratedCardType.Ai)
                throw new ConflictException("HandMade Card không thể regenerate AI.");

            return await CreateFromSourceCardAsync(request, sourceCard);
        }

        if (request.MessageContent is not null)
            throw new BadRequestException("Create Card AI không được ghi đè SnapshotCard.");

        var orderDetail = await _dbContext.OrderDetails
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == request.OrderDetailId
                && x.Order.UserId == userId
                && !x.IsDeleted
                && !x.Order.IsDeleted)
            ?? throw new NotFoundException("Không tìm thấy OrderDetail.");

        var cardSnapshot = ParseCardSnapshotArray(orderDetail.SnapshotCard);
        var (baseId, rootId) = await ResolveLineageAsync(cardSnapshot, userId);

        // AI demo hook: replace only this result with the real AI function later.
        var aiResult = new AiResult(
            "Demo AI card",
            "https://example.com/cards/demo-final.png",
            "https://example.com/cards/demo-raw.png");

        var card = MapCard(cardSnapshot, userId, aiResult);
        var history = new ClientHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ClientHistoryType.Card,
            BaseId = baseId,
            RootId = rootId,
            OutputId = card.Id,
            CreatedAt = card.CreatedAt,
            SystemForm = JsonSerializer.Serialize(cardSnapshot.SystemPrompt),
            Metadata = BuildHistoryMetadata(cardSnapshot, baseId, rootId)
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.GeneratedCards.Add(card);
        _dbContext.ClientHistories.Add(history);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToResponse(card);
    }

    private async Task<Response.CreateCardResponse> CreateFromSourceCardAsync(Request.CreateCardAiRequest request, GeneratedCard sourceCard)
    {
        var sourceHistory = await _dbContext.ClientHistories
            .FirstOrDefaultAsync(x =>
                x.UserId == sourceCard.UserId &&
                x.Type == ClientHistoryType.Card &&
                x.OutputId == sourceCard.Id)
            ?? throw new ConflictException("Không tìm thấy History nguồn.");

        var sourceMetadata = ParseSourceHistoryMetadata(sourceHistory.Metadata);
        var sourceTemplateId = sourceMetadata.CardTemplate?.Id ?? Guid.Empty;
        if (sourceTemplateId == Guid.Empty)
            throw new ConflictException("History nguồn không có Template hợp lệ.");

        var selectedTemplate = sourceMetadata.CardTemplate;
        var templateChanged = request.CardTemplateId.HasValue &&
            request.CardTemplateId.Value != sourceTemplateId;

        if (templateChanged)
        {
            var newTemplate = await _dbContext.CardTemplates
                .FirstOrDefaultAsync(x =>
                    x.Id == request.CardTemplateId &&
                    !x.IsDeleted)
                ?? throw new NotFoundException("Không tìm thấy Template.");

            if (!newTemplate.IsActive)
                throw new ConflictException("Template không active.");

            if (newTemplate.TemplateType != CardTemplateType.Ai)
                throw new ConflictException("Template phải là loại AI.");

            if (string.IsNullOrWhiteSpace(newTemplate.ImageUrl))
                throw new ConflictException("Template thiếu ảnh hợp lệ.");

            selectedTemplate = new SourceHistoryTemplate
            {
                Id = newTemplate.Id,
                Name = newTemplate.Name,
                TemplateType = "ai",
                ImageUrl = newTemplate.ImageUrl
            };
        }

        var systemPrompt = await LoadSystemPromptCardAsync();
        var systemPromptSource = "current";
        if (string.IsNullOrWhiteSpace(systemPrompt) &&
            !string.IsNullOrWhiteSpace(sourceHistory.SystemForm))
        {
            systemPrompt = sourceHistory.SystemForm;
            systemPromptSource = "source_card_fallback";
        }

        if (string.IsNullOrWhiteSpace(systemPrompt))
            throw new ConflictException("Không có SystemPrompt dùng được.");

        var messageContent = string.IsNullOrWhiteSpace(request.MessageContent)
            ? sourceCard.MessageContent
            : request.MessageContent;
        var wordCount = messageContent
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        if (wordCount > 100 || wordCount > sourceCard.SizeMaxWords)
            throw new BadRequestException("MessageContent vượt quá giới hạn từ.");

        var source = sourceMetadata.Source!;
        var template = selectedTemplate!;

        // AI demo hook: replace only this result with the real AI function later.
        var aiResult = new AiResult(
            "Demo AI card",
            "https://example.com/cards/demo-final.png",
            "https://example.com/cards/demo-raw.png");

        if (string.IsNullOrWhiteSpace(aiResult.ImageUrl) ||
            string.IsNullOrWhiteSpace(aiResult.RawImage))
            throw new ConflictException("AI không trả đủ ảnh kết quả.");

        var card = new GeneratedCard
        {
            Id = Guid.NewGuid(),
            UserId = sourceCard.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            Content = aiResult.Content,
            ImageUrl = aiResult.ImageUrl,
            RawImage = aiResult.RawImage,
            CardType = GeneratedCardType.Ai,
            FormType = sourceCard.FormType,
            MessageContent = messageContent,
            AttachedImageUrl = sourceCard.AttachedImageUrl,
            SizeName = sourceCard.SizeName,
            SizeWidth = sourceCard.SizeWidth,
            SizeHeight = sourceCard.SizeHeight,
            SizeBasePrice = sourceCard.SizeBasePrice,
            SizeMaxWords = sourceCard.SizeMaxWords,
            WordCount = wordCount,
            WordConfigSnapshot = sourceCard.WordConfigSnapshot,
            BasePrice = sourceCard.BasePrice,
            ExtraPrice = sourceCard.ExtraPrice,
            TotalPrice = sourceCard.TotalPrice
        };

        var history = new ClientHistory
        {
            Id = Guid.NewGuid(),
            UserId = sourceCard.UserId,
            Type = ClientHistoryType.Card,
            BaseId = sourceHistory.BaseId,
            RootId = sourceHistory.RootId,
            OutputId = card.Id,
            CreatedAt = card.CreatedAt,
            SystemForm = systemPrompt,
            Metadata = JsonSerializer.Serialize(new
            {
                operation = "regenerate",
                sourceType = sourceMetadata.SourceType,
                originProductId = sourceHistory.RootId,
                generatedFlowerId = sourceMetadata.GeneratedFlowerId,
                provenanceStatus = "available",
                source = new
                {
                    type = source.Type,
                    id = source.Id,
                    imageUrl = source.ImageUrl
                },
                cardTemplate = new
                {
                    id = template.Id,
                    name = template.Name,
                    templateType = template.TemplateType,
                    imageUrl = template.ImageUrl
                },
                regeneratedFromCardId = sourceCard.Id,
                systemPromptSource,
                validatedAt = DateTimeOffset.UtcNow
            })
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.GeneratedCards.Add(card);
        _dbContext.ClientHistories.Add(history);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToResponse(card);
    }

    private async Task<string?> LoadSystemPromptCardAsync()
    {
        var prompt = await _dbContext.SystemPrompts
            .FirstOrDefaultAsync(x =>
                x.Type == SystemPromptType.Card &&
                !x.IsDeleted);

        return prompt == null
            ? null
            : JsonSerializer.Serialize(new
            {
                type = "card",
                content = prompt.Content
            });
    }

    private static SourceHistoryMetadata ParseSourceHistoryMetadata(string metadata)
    {
        try
        {
            var result = JsonSerializer.Deserialize<SourceHistoryMetadata>(metadata, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null || result.CardTemplate == null || result.Source == null)
                throw new JsonException();

            return result;
        }
        catch (JsonException)
        {
            throw new ConflictException("History nguồn chứa Metadata không hợp lệ.");
        }
    }

    private static CardSnapshot ParseCardSnapshotArray(string? cardSnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(cardSnapshotJson))
            throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "OrderDetail.SnapshotCard không có dữ liệu.");

        try
        {
            var snapshots = JsonSerializer.Deserialize<List<CardSnapshot>>(cardSnapshotJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var snapshot = snapshots?.FirstOrDefault(IsOriginalAiSnapshot)
                ?? snapshots?.FirstOrDefault(IsAiSnapshot)
                ?? throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "OrderDetail.SnapshotCard không có snapshot AI phù hợp.");

            if (!IsValidSnapshot(snapshot))
                throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "OrderDetail.SnapshotCard chứa snapshot AI không hợp lệ.");

            return snapshot;
        }
        catch (JsonException)
        {
            throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "OrderDetail.SnapshotCard chứa JSON không hợp lệ.");
        }
    }

    private static bool IsOriginalAiSnapshot(CardSnapshot snapshot) =>
        snapshot.RegeneratedFromCardId == null && IsAiSnapshot(snapshot);

    private static bool IsAiSnapshot(CardSnapshot snapshot) =>
        string.Equals(snapshot.CardTemplate.TemplateType, "ai", StringComparison.OrdinalIgnoreCase);

    private static bool IsValidSnapshot(CardSnapshot snapshot) =>
        snapshot.Source.Id != Guid.Empty
        && !string.IsNullOrWhiteSpace(snapshot.Source.ImageUrl)
        && snapshot.Source.Type is "product" or "generated_flower"
        && snapshot.CardTemplate.TemplateId != Guid.Empty
        && snapshot.CardTemplate.TemplateType == "ai"
        && !string.IsNullOrWhiteSpace(snapshot.CardTemplate.TemplateImageUrl)
        && !string.IsNullOrWhiteSpace(snapshot.CardSize.Name)
        && snapshot.CardSize.Width > 0
        && snapshot.CardSize.Height > 0
        && snapshot.CardSize.MaxWords > 0
        && !string.IsNullOrWhiteSpace(snapshot.MessageContent)
        && snapshot.SystemPrompt.Type == "card"
        && !string.IsNullOrWhiteSpace(snapshot.SystemPrompt.Content)
        && TryParseFormType(snapshot.FormType, out _);

    private async Task<(Guid BaseId, Guid RootId)> ResolveLineageAsync(CardSnapshot snapshot, Guid userId)
    {
        if (snapshot.Source.Type == "product")
            return (snapshot.Source.Id, snapshot.Source.Id);

        var history = await _dbContext.ClientHistories.SingleOrDefaultAsync(x =>
            x.UserId == userId && x.Type == ClientHistoryType.Flower && x.OutputId == snapshot.Source.Id);
        if (history == null || history.RootId == Guid.Empty)
            throw new ConflictException("Không xác định được Product gốc của Generated Flower.");

        return (snapshot.Source.Id, history.RootId);
    }

    private static GeneratedCard MapCard(CardSnapshot snapshot, Guid userId, AiResult aiResult) => new()
    {
        Id = Guid.NewGuid(), 
        UserId = userId, 
        CreatedAt = DateTimeOffset.UtcNow,
        Content = aiResult.Content, 
        ImageUrl = aiResult.ImageUrl, 
        RawImage = aiResult.RawImage,
        CardType = GeneratedCardType.Ai,
        FormType = ParseFormType(snapshot.FormType),
        MessageContent = snapshot.MessageContent,
        AttachedImageUrl = snapshot.AttachedImageUrl,
        SizeName = snapshot.CardSize.Name,
        SizeWidth = snapshot.CardSize.Width,
        SizeHeight = snapshot.CardSize.Height,
        SizeBasePrice = snapshot.CardSize.BasePrice,
        SizeMaxWords = snapshot.CardSize.MaxWords,
        WordCount = snapshot.WordCount,
        WordConfigSnapshot = snapshot.CardConfig is { ValueKind: not JsonValueKind.Null } config ? config.GetRawText() : null,
        BasePrice = snapshot.BasePrice,
        ExtraPrice = snapshot.ExtraPrice,
        TotalPrice = snapshot.TotalPrice
    };

    private static string BuildHistoryMetadata(CardSnapshot snapshot, Guid baseId, Guid rootId) => JsonSerializer.Serialize(new
    {
        operation = "create",
        sourceType = snapshot.Source.Type,
        originProductId = rootId,
        generatedFlowerId = snapshot.Source.Type == "generated_flower" ? baseId : (Guid?)null,
        provenanceStatus = "available",
        source = snapshot.Source,
        cardTemplate = new
        {
            id = snapshot.CardTemplate.TemplateId,
            name = snapshot.CardTemplate.TemplateName,
            templateType = "ai",
            imageUrl = snapshot.CardTemplate.TemplateImageUrl
        },
        regeneratedFromCardId = snapshot.RegeneratedFromCardId,
        systemPromptSource = "order_detail_snapshot",
        validatedAt = snapshot.ValidatedAt
    });

    private sealed record AiResult(string Content, string ImageUrl, string RawImage);

    private sealed class CardSnapshot
    {
        public SnapshotSource Source { get; set; } = new();
        public Guid? RegeneratedFromCardId { get; set; }
        public SnapshotTemplate CardTemplate { get; set; } = new();
        public SnapshotSize CardSize { get; set; } = new();
        public JsonElement? CardConfig { get; set; }
        public string MessageContent { get; set; } = "";
        public string FormType { get; set; } = "";
        public string? AttachedImageUrl { get; set; }
        public int WordCount { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ExtraPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public SnapshotPrompt SystemPrompt { get; set; } = new();
        public DateTimeOffset ValidatedAt { get; set; }
    }

    private sealed class SnapshotSource
    {
        public string Type { get; set; } = "";
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    private sealed class SnapshotTemplate
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = "";
        public string TemplateType { get; set; } = "";
        public string TemplateImageUrl { get; set; } = "";
    }

    private sealed class SnapshotSize
    {
        public string Name { get; set; } = "";
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("base_price")]
        public decimal BasePrice { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
    }

    private sealed class SnapshotPrompt
    {
        public string Type { get; set; } = "";
        public string Content { get; set; } = "";
    }

    private sealed class SourceHistoryMetadata
    {
        public string SourceType { get; set; } = "";
        public Guid? GeneratedFlowerId { get; set; }
        public SourceHistorySource? Source { get; set; }
        public SourceHistoryTemplate? CardTemplate { get; set; }
    }

    private sealed class SourceHistorySource
    {
        public string Type { get; set; } = "";
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    private sealed class SourceHistoryTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string TemplateType { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }

    private static bool TryParseFormType(string value, out CardFormType formType)
    {
        if (value.Equals("calligraphy", StringComparison.OrdinalIgnoreCase))
        {
            formType = CardFormType.Calligraphy;
            return true;
        }

        if (value.Equals("go_may", StringComparison.OrdinalIgnoreCase))
        {
            formType = CardFormType.GoMay;
            return true;
        }

        formType = default;
        return false;
    }

    private static CardFormType ParseFormType(string value) =>
        TryParseFormType(value, out var formType)
            ? formType
            : throw new ConflictException("SnapshotCard có formType không hợp lệ.");

    private static Response.CreateCardResponse ToResponse(GeneratedCard card) => new()
    {
        Id = card.Id, 
        ImageUrl = card.ImageUrl, 
        RawImage = card.RawImage, 
        CardType = card.CardType == GeneratedCardType.Ai ? "ai" : "handmade",
        FormType = card.FormType == CardFormType.Calligraphy ? "calligraphy" : "go_may", 
        MessageContent = card.MessageContent,
        AttachedImageUrl = card.AttachedImageUrl, 
        SizeName = card.SizeName, 
        SizeWidth = (int)card.SizeWidth, 
        SizeHeight = (int)card.SizeHeight,
        SizeBasePrice = (int)card.SizeBasePrice, 
        SizeMaxWords = card.SizeMaxWords, 
        WordCount = card.WordCount,
        WordConfigSnapshot = string.IsNullOrWhiteSpace(card.WordConfigSnapshot) ? new Response.WordConfigSnapshot() : JsonSerializer.Deserialize<Response.WordConfigSnapshot>(card.WordConfigSnapshot)!,
        BasePrice = (int)card.BasePrice, 
        ExtraPrice = (int)card.ExtraPrice, 
        TotalPrice = (int)card.TotalPrice
    };
}

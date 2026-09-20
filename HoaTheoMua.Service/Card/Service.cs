using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ai = HoaTheoMua.Service.AiService;
using static HoaTheoMua.Service.Card.MappingCard;

namespace HoaTheoMua.Service.Card;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContext;
    private readonly Ai.IService _ai;

    public Service(
        AppDbContext dbContext,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContext,
        Ai.IService ai)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _httpContext = httpContext;
        _ai = ai;
    }

    public async Task<HoaTheoMua.Service.Models.BasePaginationResponse> GetAdminGeneratedCards(
        Request.AdminGeneratedCardListRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateAdminListRequest(request);
        var pageIndex = request.pageIndex <= 0 ? 1 : request.pageIndex;
        var pageSize = request.pageSize <= 0 ? 10 : Math.Min(request.pageSize, 100);

        CardFormType? formType = null;
        if (!string.IsNullOrWhiteSpace(request.formType))
        {
            if (!TryParseFormType(request.formType.Trim(), out var parsedFormType))
            {
                throw new AdminListValidationException("FormType chỉ nhận go_may hoặc calligraphy.");
            }

            formType = parsedFormType;
        }

        var size = string.IsNullOrWhiteSpace(request.size) ? null : request.size.Trim();
        var cardTemplateName = string.IsNullOrWhiteSpace(request.cardTemplateName)
            ? null
            : request.cardTemplateName.Trim();

        var query = _dbContext.GeneratedCards
            .AsNoTracking()
            .Where(card => card.CardType == GeneratedCardType.Ai);
        if (request.id.HasValue)
            query = query.Where(card => card.Id == request.id.Value);
        if (formType.HasValue)
            query = query.Where(card => card.FormType == formType.Value);

        var cards = await query
            .OrderByDescending(card => card.CreatedAt)
            .ThenByDescending(card => card.Id)
            .ToListAsync(cancellationToken);
        if (size is not null)
            cards = cards.Where(card => string.Equals(card.SizeName, size, StringComparison.OrdinalIgnoreCase)).ToList();

        var cardIds = cards.Select(card => card.Id).ToArray();
        var histories = cardIds.Length == 0
            ? []
            : await _dbContext.ClientHistories.AsNoTracking()
                .Where(history => history.Type == ClientHistoryType.Card && cardIds.Contains(history.OutputId))
                .OrderByDescending(history => history.CreatedAt)
                .ThenByDescending(history => history.Id)
                .ToListAsync(cancellationToken);
        var historyByCard = histories
            .GroupBy(history => (history.OutputId, history.UserId))
            .ToDictionary(group => group.Key, group => group.First());

        var records = cards
            .Select(card => (Card: card, History: historyByCard.GetValueOrDefault((card.Id, card.UserId))))
            .Where(record => cardTemplateName is null
                || TryGetCardTemplateName(record.History?.Metadata, out var templateName)
                && templateName.Contains(cardTemplateName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var totalCount = records.Count;
        var items = records
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(record => ToAdminResponse(record.Card, record.History))
            .ToList();
        return HoaTheoMua.Service.Models.ApiResponseFactory.BasePagination(
            items,
            pageIndex,
            pageSize,
            totalCount);
    }

    //Handmade not call AI
    public async Task<Response.CreateCardResponse> CreateHandMadeCard(Request.CreateCardRequest request)
    {
        if (request.ProductId.HasValue == request.GeneratedFlowerId.HasValue)
            throw new BadRequestException("Chỉ chọn Product hoặc GeneratedFlower.");
        if (request.ProductId == Guid.Empty || request.GeneratedFlowerId == Guid.Empty || request.CardTemplateId == Guid.Empty || string.IsNullOrWhiteSpace(request.SizeName))
            throw new BadRequestException("VALIDATION_ERROR");

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

        var sizes = (await LoadAndDeserializeCardConfigAsync<ConfigItems<SizeConfigItem>>("card_config.sizes")).Items;
        var priceRules = (await LoadAndDeserializeCardConfigAsync<ConfigItems<PriceConfigItem>>("card_config.prices")).Items;
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
            SessionId = null,
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

    private async Task<T> LoadAndDeserializeCardConfigAsync<T>(string key)
    {
        var normalizedKey = key.Trim().ToLower();

        var configs = await _dbContext.Configs
            .Where(x =>
                x.Group.Trim().ToLower() == "card_config" &&
                x.Key.Trim().ToLower() == normalizedKey &&
                !x.IsDeleted &&
                x.Kind!.Trim().ToLower() == "setting")
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
                return DeserializeConfig<T>(inner.RootElement.Clone());
            }
            return DeserializeConfig<T>(value.Clone());
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
    
    public async Task<Response.CreateCardResponse> CreateCardAi(Request.CreateCardAiRequest request)
    {
        var createMode = request.OrderDetailIdSpecified || request.VirtualIdSpecified;
        var regenerateMode = request.SourceCardIdSpecified;
        if (createMode == regenerateMode
            || (regenerateMode && (!request.SourceCardId.HasValue || request.SourceCardId == Guid.Empty))
            || (createMode && (!request.OrderDetailId.HasValue || !request.VirtualId.HasValue
                || request.OrderDetailId == Guid.Empty || request.VirtualId == Guid.Empty)))
            throw new BadRequestException("VALIDATION_ERROR");

        var userId = _currentUser.GetRequiredUserId();
        if (regenerateMode)
        {
            var sourceCard = await _dbContext.GeneratedCards
                .FirstOrDefaultAsync(x => x.Id == request.SourceCardId && x.UserId == userId)
                ?? throw new NotFoundException("Không tìm thấy source Card.");
            if (sourceCard.CardType != GeneratedCardType.Ai)
                throw new ConflictException("HandMade Card không thể regenerate AI.");

            return await CreateFromSourceCardAsync(request, sourceCard);
        }

        if (request.MessageContentSpecified || request.CardTemplateIdSpecified)
            throw new BadRequestException("Create Card AI không được ghi đè SnapshotCard.");

        var orderDetail = await _dbContext.OrderDetails
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == request.OrderDetailId
                && x.Order.UserId == userId
                && !x.IsDeleted
                && !x.Order.IsDeleted)
            ?? throw new NotFoundException("Không tìm thấy OrderDetail.");
        if (!string.Equals(orderDetail.Order.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            throw new ConflictException("ORDER_NOT_PAID", "Order chưa thanh toán.");

        var (metadata, snapshotNode, cardSnapshot, generatedFlowerId) = FindAiSnapshot(orderDetail.Metadata, request.VirtualId!.Value);
        if (!string.Equals(cardSnapshot.Status, "not yet", StringComparison.OrdinalIgnoreCase) || cardSnapshot.IdResult.Count != 0 || cardSnapshot.FinalGeneratedCardId.HasValue)
            throw new ConflictException("CARD_ALREADY_CREATED", "Snapshot Card đã được tạo hoặc không hợp lệ.");
        var (baseId, rootId) = await ResolveLineageAsync(cardSnapshot, userId, generatedFlowerId);

        var aiResult = await GenerateAiCard(cardSnapshot);

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
            SessionId = null,
            Metadata = BuildHistoryMetadata(
                cardSnapshot,
                baseId,
                rootId,
                orderDetail.Id,
                request.VirtualId.Value,
                generatedFlowerId,
                "create")
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.GeneratedCards.Add(card);
        _dbContext.ClientHistories.Add(history);
        snapshotNode["status"] = "done";
        snapshotNode["idResult"] = new JsonArray(card.Id.ToString());
        orderDetail.Metadata = metadata.ToJsonString();
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToResponse(card);
    }

    public async Task<string> UpdateFinalGeneratedCardId(Request.UpdateFinalGeneratedCardId request)
    {
        if (request.OrderDetailId == Guid.Empty || request.VirtualId == Guid.Empty || request.GeneratedCardId == Guid.Empty)
            throw new BadRequestException("VALIDATION_ERROR");

        var userId = _currentUser.GetRequiredUserId();
        var orderDetail = await _dbContext.OrderDetails
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == request.OrderDetailId
                && x.Order.UserId == userId
                && !x.IsDeleted
                && !x.Order.IsDeleted)
            ?? throw new NotFoundException("Không tìm thấy OrderDetail.");

        var (metadata, snapshotNode, snapshot, _) = FindAiSnapshot(orderDetail.Metadata, request.VirtualId);
        if (!snapshot.IdResult.Contains(request.GeneratedCardId))
            throw new ConflictException("GENERATED_CARD_NOT_IN_SNAPSHOT", "GeneratedCard không thuộc kết quả của Snapshot Card.");

        snapshotNode["finalGeneratedCardId"] = request.GeneratedCardId.ToString();
        orderDetail.Metadata = metadata.ToJsonString();
        await _dbContext.SaveChangesAsync();

        return "Final generated card updated successfully.";
    }

    public async Task<Response.OrderDetailCardPreviewResponse> GetCardByOrderDetailId(
        Guid orderDetailId,
        Guid virtualId)
    {
        if (orderDetailId == Guid.Empty || virtualId == Guid.Empty)
            throw new BadRequestException("OrderDetailId không được là GUID rỗng.");

        var userId = _currentUser.GetRequiredUserId();
        var orderDetail = await _dbContext.OrderDetails
            .AsNoTracking()
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == orderDetailId
                && x.Order.UserId == userId
                && !x.IsDeleted
                && !x.Order.IsDeleted)
            ?? throw new NotFoundException("Không tìm thấy OrderDetail.");

        var group = ParseOrderDetailCardGroups(orderDetail.Metadata)
            .SingleOrDefault(group => group.VirtualId == virtualId)
            ?? throw new NotFoundException("CARD_SESSION_NOT_FOUND");
        var cardIds = group.IdResult.ToArray();
        var cards = cardIds.Length == 0
            ? []
            : await _dbContext.GeneratedCards
                .AsNoTracking()
                .Where(x => x.UserId == userId && cardIds.Contains(x.Id))
                .ToListAsync();
        if (cards.Count != cardIds.Length)
            throw new ConflictException("ORDER_DETAIL_CARD_REFERENCE_INVALID", "OrderDetail tham chiếu Card không tồn tại hoặc không thuộc user.");

        var histories = cardIds.Length == 0
            ? []
            : await _dbContext.ClientHistories
                .AsNoTracking()
                .Where(x => x.UserId == userId
                    && (x.Type == ClientHistoryType.Card || x.Type == ClientHistoryType.HandmadeCard)
                    && cardIds.Contains(x.OutputId))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        var cardsById = cards.ToDictionary(x => x.Id);
        var historiesByCardId = histories.GroupBy(x => x.OutputId).ToDictionary(x => x.Key, x => x.First());

        return new Response.OrderDetailCardPreviewResponse
        {
            // OrderDetailId = orderDetailId,
            // VirtualId = virtualId,
            Cards = cardIds.Select(cardId =>
            {
                var history = historiesByCardId.GetValueOrDefault(cardId);
                return new Response.CardPreviewItemResponse
                {
                    Id = cardId,
                    Name = GetCardTemplateName(history, string.Empty),
                    ImageUrl = cardsById[cardId].ImageUrl
                };
            }).ToList()
        };
    }

    public async Task<Response.GeneratedCardImageResponse> GetGeneratedCardImage(Guid generatedCardId)
    {
        if (generatedCardId == Guid.Empty)
            throw new BadRequestException("GENERATED_CARD_ID_INVALID");

        var isAdmin = _httpContext.HttpContext?.User.IsInRole("Admin") == true;
        var query = _dbContext.GeneratedCards
            .AsNoTracking()
            .Where(card => card.Id == generatedCardId);

        if (!isAdmin)
        {
            var userId = _currentUser.GetRequiredUserId();
            query = query.Where(card => card.UserId == userId);
        }

        var card = await query.FirstOrDefaultAsync()
            ?? throw new NotFoundException("GENERATED_CARD_NOT_FOUND");

        if (string.IsNullOrWhiteSpace(card.ImageUrl))
            throw new ConflictException("GENERATED_CARD_IMAGE_UNAVAILABLE", "GeneratedCard chưa có ảnh để tải.");

        return new Response.GeneratedCardImageResponse
        {
            GeneratedCardId = card.Id,
            ImageUrl = card.ImageUrl
        };
    }

    public async Task<List<Response.ProductGeneratedCardsResponse>> GetGeneratedCardsByProductIds(
        Request.GetGeneratedCardsByProductIdsRequest request)
    {
        var productIds = request.ProductIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (productIds.Count == 0)
            throw new BadRequestException("PRODUCT_IDS_REQUIRED");

        var userId = _currentUser.GetRequiredUserId();
        var cardHistories = await (
                from history in _dbContext.ClientHistories.AsNoTracking()
                join card in _dbContext.GeneratedCards.AsNoTracking()
                    on history.OutputId equals card.Id
                where history.UserId == userId &&
                      history.Type == ClientHistoryType.Card &&
                      productIds.Contains(history.RootId) &&
                      card.UserId == userId &&
                      card.CardType == GeneratedCardType.Ai
                orderby history.CreatedAt descending
                select new { Card = card, History = history })
            .ToListAsync();

        return cardHistories
            .GroupBy(item => item.History.RootId)
            .Select(group => new Response.ProductGeneratedCardsResponse
            {
                ProductId = group.Key,
                GeneratedCards = group.Select(item => new Response.CardItemResponse
                {
                    Id = item.Card.Id,
                    CardType = "ai",
                    FormType = item.Card.FormType == CardFormType.GoMay ? "go_may" : "calligraphy",
                    ImageUrl = item.Card.ImageUrl,
                    RawImage = item.Card.RawImage,
                    MessageContent = item.Card.MessageContent,
                    AttachedImageUrl = item.Card.AttachedImageUrl,
                    SizeName = item.Card.SizeName,
                    SizeWidth = item.Card.SizeWidth,
                    SizeHeight = item.Card.SizeHeight,
                    TotalPrice = item.Card.TotalPrice,
                    CreatedAt = item.Card.CreatedAt
                }).ToList()
            })
            .ToList();
    }

    private static List<OrderDetailCardGroup> ParseOrderDetailCardGroups(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return [];

        try
        {
            var metadata = JsonNode.Parse(metadataJson)?.AsObject() ?? throw new JsonException();
            if (metadata["productMetadata"] is null) return [];
            var products = metadata["productMetadata"] as JsonArray ?? throw new JsonException();
            var groups = new List<OrderDetailCardGroup>();
            foreach (var productNode in products)
            {
                var product = productNode as JsonObject ?? throw new JsonException();
                if (product["snapShotCard"] is null) continue;
                var snapshotNode = product["snapShotCard"] as JsonObject ?? throw new JsonException();
                var snapshot = snapshotNode.Deserialize<CardSnapshot>(new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? throw new JsonException();
                if (snapshot.Source is null || snapshot.IdResult is null
                    || snapshot.VirtualId == Guid.Empty || string.IsNullOrWhiteSpace(snapshot.CardType)
                    || snapshot.IdResult.Any(x => x == Guid.Empty)
                    || snapshot.IdResult.Distinct().Count() != snapshot.IdResult.Count)
                    throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "Snapshot Card không hợp lệ.");
                if (snapshot.FinalGeneratedCardId.HasValue && !snapshot.IdResult.Contains(snapshot.FinalGeneratedCardId.Value))
                    throw new ConflictException("ORDER_DETAIL_CARD_REFERENCE_INVALID", "FinalGeneratedCardId không thuộc idResult.");

                var generatedFlowerId = Guid.TryParse(product["generatedFlower"]?.ToString(), out var flowerId)
                    ? flowerId
                    : (Guid?)null;
                groups.Add(new OrderDetailCardGroup(
                    snapshot.VirtualId,
                    string.IsNullOrWhiteSpace(snapshot.Source.Type) ? "product" : snapshot.Source.Type,
                    snapshot.Source.Id == Guid.Empty ? null : snapshot.Source.Id,
                    generatedFlowerId,
                    snapshot.CardType,
                    snapshot.IdResult,
                    snapshot.FinalGeneratedCardId));
            }

            if (groups.Select(x => x.VirtualId).Distinct().Count() != groups.Count)
                throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "OrderDetail có virtualId bị trùng.");
            if (groups.SelectMany(x => x.IdResult).Distinct().Count() != groups.Sum(x => x.IdResult.Count))
                throw new ConflictException("ORDER_DETAIL_CARD_REFERENCE_INVALID", "Một Card đang thuộc nhiều Snapshot Card.");
            return groups;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "OrderDetail.Metadata không đúng cấu trúc Card.");
        }
    }

    private static Response.CardItemResponse ToOrderDetailCardResponse(
        GeneratedCard card,
        ClientHistory? history,
        bool isSelected)
    {
        var (operation, regeneratedFromCardId) = ParseCardHistory(history);
        return new Response.CardItemResponse
        {
            Id = card.Id,
            CardType = card.CardType == GeneratedCardType.Ai ? "ai" : "handmade",
            FormType = card.FormType == CardFormType.Calligraphy ? "calligraphy" : "go_may",
            ImageUrl = card.ImageUrl,
            RawImage = card.RawImage,
            MessageContent = card.MessageContent,
            AttachedImageUrl = card.AttachedImageUrl,
            SizeName = card.SizeName,
            SizeWidth = card.SizeWidth,
            SizeHeight = card.SizeHeight,
            TotalPrice = card.TotalPrice,
            CreatedAt = card.CreatedAt,
            Operation = operation,
            RegeneratedFromCardId = regeneratedFromCardId,
            IsSelected = isSelected
        };
    }

    private static (string Operation, Guid? RegeneratedFromCardId) ParseCardHistory(ClientHistory? history)
    {
        if (history is null) return ("create", null);
        try
        {
            using var metadata = JsonDocument.Parse(history.Metadata);
            var root = metadata.RootElement;
            var operation = root.TryGetProperty("operation", out var operationNode)
                && operationNode.ValueKind == JsonValueKind.String
                && operationNode.GetString() is { Length: > 0 } value
                    ? value
                    : "create";
            var regeneratedFromCardId = root.TryGetProperty("regeneratedFromCardId", out var sourceNode)
                && sourceNode.ValueKind == JsonValueKind.String
                && Guid.TryParse(sourceNode.GetString(), out var sourceId)
                    ? sourceId
                    : (Guid?)null;
            return (operation, regeneratedFromCardId);
        }
        catch (JsonException)
        {
            throw new ConflictException("CARD_HISTORY_INVALID", "Card History chứa Metadata không hợp lệ.");
        }
    }

    private static string GetCardTemplateName(ClientHistory? history, string fallback)
    {
        if (history is null) return fallback;
        try
        {
            using var metadata = JsonDocument.Parse(history.Metadata);
            return metadata.RootElement.TryGetProperty("cardTemplate", out var template)
                && template.TryGetProperty("name", out var name)
                && name.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(name.GetString())
                    ? name.GetString()!
                    : fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private sealed record OrderDetailCardGroup(
        Guid VirtualId,
        string SourceType,
        Guid? ProductId,
        Guid? GeneratedFlowerId,
        string CardType,
        List<Guid> IdResult,
        Guid? FinalGeneratedCardId);

    private async Task<AiResult> GenerateAiCard(CardSnapshot snapshot)
    {
        var sourceImage = string.IsNullOrWhiteSpace(snapshot.RawImage) ? snapshot.Source.ImageUrl : snapshot.RawImage;
        if (string.IsNullOrWhiteSpace(sourceImage))
            throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "Snapshot Card thiếu ảnh nguồn.");

        
        var instruction = $"{snapshot.SystemPrompt.Content}\nTemplate: {snapshot.CardTemplate.TemplateImageUrl}\n" +
                          $"Nội dung thiệp: {snapshot.MessageContent}\nHình thức: {snapshot.FormType}.";
        
        /* REAL AI: temporarily disabled for fake.
        var output = await _ai.GenerateImage(new Ai.Request.ImageInput(
            sourceImage, "1:1", 1024, 1024, instruction,
            string.IsNullOrWhiteSpace(snapshot.RawImage) ? snapshot.CardTemplate.TemplateImageUrl : null));
        if (string.IsNullOrWhiteSpace(output.ImageUrl))
            throw new ServerException("AI không trả ảnh Card hợp lệ.");
        return new AiResult(snapshot.MessageContent, output.ImageUrl, snapshot.RawImage ?? output.ImageUrl);
        */
        var aiResult = await FakeGenerateCardAsync(snapshot.MessageContent, snapshot.RawImage);
        if (string.IsNullOrWhiteSpace(aiResult.ImageUrl) || string.IsNullOrWhiteSpace(aiResult.RawImage))
            throw new ServerException("Fake AI không trả ảnh Card hợp lệ.");
        return aiResult;
    }

    private static (JsonObject Metadata, JsonObject SnapshotNode, CardSnapshot Snapshot, Guid? GeneratedFlowerId)
        FindAiSnapshot(string? json, Guid virtualId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) throw new JsonException();
            var metadata = JsonNode.Parse(json)?.AsObject() ?? throw new JsonException();
            var products = metadata["productMetadata"]?.AsArray() ?? throw new JsonException();
            var matches = products
                .OfType<JsonObject>()
                .Select(product => (product, snapshot: product["snapShotCard"] as JsonObject))
                .Where(x => x.snapshot is not null && Guid.TryParse(x.snapshot!["virtualId"]?.GetValue<string>(), out var id) && id == virtualId)
                .ToList();
            if (matches.Count != 1) throw new ConflictException("NOT_FOUND", "Không tìm thấy virtualId trong OrderDetail.");

            var snapshot = matches[0].snapshot!.Deserialize<CardSnapshot>(new JsonSerializerOptions(JsonSerializerDefaults.Web))
                           ?? throw new JsonException();
            if (!IsValidSnapshot(snapshot) || !string.Equals(snapshot.CardType, "ai", StringComparison.OrdinalIgnoreCase))
                throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "Snapshot Card AI không hợp lệ.");
            var flowerId = Guid.TryParse(matches[0].product["generatedFlower"]?.GetValue<string>(), out var parsed) ? parsed : (Guid?)null;
            return (metadata, matches[0].snapshot!, snapshot, flowerId);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "OrderDetail.Metadata không đúng cấu trúc Card.");
        }
    }

    private async Task<Response.CreateCardResponse> CreateFromSourceCardAsync(Request.CreateCardAiRequest request, GeneratedCard sourceCard)
    {
        var sourceHistories = await _dbContext.ClientHistories
            .Where(x =>
                x.UserId == sourceCard.UserId &&
                x.Type == ClientHistoryType.Card &&
                x.OutputId == sourceCard.Id)
            .ToListAsync();
        if (sourceHistories.Count != 1)
            throw new ConflictException("CARD_SOURCE_HISTORY_INVALID", "History nguồn không hợp lệ.");
        var sourceHistory = sourceHistories[0];

        var sourceMetadata = ParseSourceHistoryMetadata(sourceHistory.Metadata);
        if (!sourceMetadata.OrderDetailId.HasValue || !sourceMetadata.VirtualId.HasValue)
            throw new ConflictException("CARD_SOURCE_HISTORY_INVALID", "History nguồn thiếu liên kết Order.");
        var orderDetail = await _dbContext.OrderDetails.Include(x => x.Order).FirstOrDefaultAsync(x =>
            x.Id == sourceMetadata.OrderDetailId && !x.IsDeleted && !x.Order.IsDeleted && x.Order.UserId == sourceCard.UserId)
            ?? throw new ConflictException("CARD_SOURCE_HISTORY_INVALID", "Order liên kết không còn hợp lệ.");
        var (metadataNode, snapshotNode, snapshot, _) = FindAiSnapshot(orderDetail.Metadata, sourceMetadata.VirtualId.Value);
        if (!snapshot.IdResult.Contains(sourceCard.Id) || (snapshot.FinalGeneratedCardId.HasValue && !snapshot.IdResult.Contains(snapshot.FinalGeneratedCardId.Value)))
            throw new ConflictException("CARD_SOURCE_HISTORY_INVALID", "Card nguồn không thuộc kết quả Snapshot.");
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
        if (systemPrompt is null)
        {
            systemPrompt = ParseSystemPrompt(sourceHistory.SystemForm);
            if (systemPrompt is not null)
                systemPromptSource = "source_card_fallback";
        }

        if (systemPrompt is null)
            throw new ConflictException("Không có SystemPrompt dùng được.");

        var messageContent = string.IsNullOrWhiteSpace(request.MessageContent)
            ? sourceCard.MessageContent
            : request.MessageContent;
        var wordCount = messageContent
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        ValidateRegeneratedWordCount(sourceCard, wordCount);

        var source = sourceMetadata.Source!;
        var template = selectedTemplate!;

        /* REAL AI: temporarily disabled for fake.
        var output = await _ai.GenerateImage(new Ai.Request.ImageInput(
            source.ImageUrl, "1:1", 1024, 1024,
            $"{systemPrompt.Content}\nTemplate: {template.ImageUrl}\nNội dung thiệp: {messageContent}\nHình thức: {sourceCard.FormType}."));
        if (string.IsNullOrWhiteSpace(output.ImageUrl))
            throw new ServerException("AI không trả ảnh Card hợp lệ.");
        var aiResult = new AiResult(messageContent, output.ImageUrl, output.ImageUrl);
        */
        var aiResult = await FakeGenerateCardAsync(messageContent, null);

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
            SystemForm = JsonSerializer.Serialize(new { type = "card", content = systemPrompt.Content }),
            SessionId = null,
            Metadata = JsonSerializer.Serialize(new
            {
                operation = "regenerate",
                orderDetailId = sourceMetadata.OrderDetailId,
                virtualId = sourceMetadata.VirtualId,
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
        var ids = snapshotNode["idResult"]?.AsArray() ?? throw new ConflictException("CARD_SOURCE_HISTORY_INVALID", "Snapshot Card thiếu idResult.");
        ids.Add(card.Id.ToString());
        orderDetail.Metadata = metadataNode.ToJsonString();
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToResponse(card);
    }

    private async Task<SnapshotPrompt?> LoadSystemPromptCardAsync()
    {
        var configs = await _dbContext.Configs.AsNoTracking()
            .Where(x => x.Key == "imagegen.card_prompt" && !x.IsDeleted)
            .ToListAsync();
        if (configs.Count == 0) return null;
        if (configs.Count != 1
            || !string.Equals(configs[0].Group?.Trim(), "imagegen", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(configs[0].Kind?.Trim(), "setting", StringComparison.OrdinalIgnoreCase))
            // || !configs[0].IsPublic)
        {
            throw new ConflictException(
                "SYSTEM_PROMPT_INVALID",
                "System prompt card Config không hợp lệ.");
        }

        try
        {
            using var value = JsonDocument.Parse(configs[0].Value);
            var root = value.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("value", out var content)
                || content.ValueKind != JsonValueKind.String)
                throw new JsonException();

            // ValidatePromptDescription(root);
            var prompt = content.GetString();
            return string.IsNullOrWhiteSpace(prompt) ? null : new SnapshotPrompt { Type = "card", Content = prompt };
        }
        catch (JsonException)
        {
            throw new ConflictException("SYSTEM_PROMPT_INVALID", "System prompt card Config không hợp lệ.");
        }
    }

    // private static void ValidatePromptDescription(JsonElement root)
    // {
    //     if (!root.TryGetProperty("description", out var description) || description.ValueKind != JsonValueKind.String)
    //         throw new JsonException();
    //     using var parsed = JsonDocument.Parse(description.GetString()!);
    //     if (parsed.RootElement.ValueKind != JsonValueKind.Object)
    //         throw new JsonException();
    // }

    private static void ValidateRegeneratedWordCount(GeneratedCard sourceCard, int wordCount)
    {
        if (wordCount > 100 || wordCount > sourceCard.SizeMaxWords)
            throw new BadRequestException("MessageContent vượt quá giới hạn từ.");

        if (sourceCard.FormType == CardFormType.GoMay)
        {
            if (!string.IsNullOrWhiteSpace(sourceCard.WordConfigSnapshot))
                throw new ConflictException("CARD_SOURCE_HISTORY_INVALID", "GoMay Card không được có WordConfigSnapshot.");
            return;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(sourceCard.WordConfigSnapshot))
                throw new JsonException();
            var rule = JsonSerializer.Deserialize<Response.WordConfigSnapshot>(sourceCard.WordConfigSnapshot!);
            if (rule is null || rule.MinWords < 0 || rule.MaxWords < rule.MinWords || rule.ExtraPrice < 0 || wordCount > rule.MaxWords)
                throw new JsonException();
        }
        catch (JsonException)
        {
            throw new ConflictException("CARD_SOURCE_HISTORY_INVALID", "Calligraphy Card có WordConfigSnapshot không hợp lệ.");
        }
    }

    private static SnapshotPrompt? ParseSystemPrompt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            var prompt = JsonSerializer.Deserialize<SnapshotPrompt>(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return prompt is { Type: var type, Content: var content }
                && string.Equals(type, "card", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(content)
                ? prompt
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
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

    private static bool IsValidSnapshot(CardSnapshot snapshot) =>
        snapshot.Source.Id != Guid.Empty
        && !string.IsNullOrWhiteSpace(snapshot.Source.ImageUrl)
        && string.Equals(snapshot.Source.Type, "product", StringComparison.OrdinalIgnoreCase)
        && snapshot.CardTemplate.TemplateId != Guid.Empty
        && snapshot.CardTemplate.TemplateType == "ai"
        && !string.IsNullOrWhiteSpace(snapshot.CardTemplate.TemplateImageUrl)
        && !string.IsNullOrWhiteSpace(snapshot.CardSize.Name)
        && snapshot.CardSize.Width > 0
        && snapshot.CardSize.Height > 0
        && snapshot.CardSize.MaxWords > 0
        && snapshot.MessageContent is not null
        && snapshot.SystemPrompt.Type == "card"
        && !string.IsNullOrWhiteSpace(snapshot.SystemPrompt.Content)
        && TryParseFormType(snapshot.FormType, out _);

    private async Task<(Guid BaseId, Guid RootId)> ResolveLineageAsync(CardSnapshot snapshot, Guid userId, Guid? generatedFlowerId)
    {
        if (!generatedFlowerId.HasValue)
            return (snapshot.Source.Id, snapshot.Source.Id);

        var flower = await _dbContext.GeneratedFlowers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == generatedFlowerId && x.UserId == userId)
            ?? throw new ConflictException("SOURCE_PRODUCT_PROVENANCE_INVALID", "GeneratedFlower không thuộc user.");
        var history = await _dbContext.ClientHistories.SingleOrDefaultAsync(x =>
            x.UserId == userId && x.Type == ClientHistoryType.Flower && x.OutputId == flower.Id);
        if (history == null || history.RootId == Guid.Empty || history.RootId != snapshot.Source.Id)
            throw new ConflictException("Không xác định được Product gốc của Generated Flower.");

        return (flower.Id, history.RootId);
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

    private static string BuildHistoryMetadata(
        CardSnapshot snapshot,
        Guid baseId,
        Guid rootId,
        Guid orderDetailId,
        Guid virtualId,
        Guid? generatedFlowerId,
        string operation) => JsonSerializer.Serialize(new
    {
        operation,
        orderDetailId,
        virtualId,
        sourceType = generatedFlowerId.HasValue ? "generated_flower" : "product",
        originProductId = rootId,
        generatedFlowerId,
        provenanceStatus = "available",
        source = snapshot.Source,
        cardTemplate = new
        {
            id = snapshot.CardTemplate.TemplateId,
            name = snapshot.CardTemplate.TemplateName,
            templateType = "ai",
            imageUrl = snapshot.CardTemplate.TemplateImageUrl
        },
        regeneratedFromCardId = (Guid?)null,
        systemPromptSource = "order_snapshot",
        validatedAt = snapshot.ValidatedAt
    });

    // FAKE AI: fixtures only; no provider, upload, or network call is made here.
    private static Task<AiResult> FakeGenerateCardAsync(string messageContent, string? historicalRawImage) => Task.FromResult(
        new AiResult(
            messageContent,
            "https://hcm.ss.bfcplatform.vn/taskcoper/general/f73b85-user-nuerbeadximuy2rqgvxkimfr-28e2a49d-afb3-455c-b174-5c74500f1763.png",
            historicalRawImage ?? "https://pos.nvncdn.com/281991-126981/ps/Daily-34_2026-1.jpg?v=1784046764"));

    private sealed record AiResult(string Content, string ImageUrl, string RawImage);

    

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

    private static void ValidateAdminListRequest(Request.AdminGeneratedCardListRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.id == Guid.Empty)
        {
            errors["id"] = ["Id không được là GUID rỗng."];
        }

        // Pagination được chuẩn hóa tại GetAdminGeneratedCards.
#if false
        if (request.pageIndex < 1)
        {
            errors["pageIndex"] = ["PageIndex phải lớn hơn hoặc bằng 1."];
        }

        // if (request.pageSize is not (5 or 10 or 20 or 30 or 40 or 50))
        // {
        //     errors["pageSize"] = ["PageSize chỉ nhận 5, 10, 20, 30, 40 hoặc 50."];
        // }

#endif
        if (request.formType is not null && string.IsNullOrWhiteSpace(request.formType))
        {
            errors["formType"] = ["FormType chỉ nhận go_may hoặc calligraphy."];
        }

        if (errors.Count > 0)
        {
            throw new AdminListValidationException(string.Join(" ", errors.SelectMany(error => error.Value)));
        }
    }

    private static Response.AdminGeneratedCardResponse ToAdminResponse(
        GeneratedCard card,
        ClientHistory? history)
    {
        JsonElement? metadata = history is null ? null : ParseJson(history.Metadata);
        return new Response.AdminGeneratedCardResponse
        {
            HistoryId = history?.Id,
            ClientHistory = history is null
                ? null
                : new Response.ClientHistoryResponse
                {
                    Id = history.Id,
                    UserId = history.UserId,
                    Type = "card",
                    SystemForm = ParseNullableJson(history.SystemForm),
                    Metadata = metadata!.Value,
                    RootId = history.RootId,
                    CreatedAt = history.CreatedAt,
                    BaseId = history.BaseId,
                    OutputId = history.OutputId,
                    SesionId = history.SessionId
                },
            GeneratedCardId = card.Id,
            ImageUrl = card.ImageUrl,
            RawImage = card.RawImage,
            UserId = card.UserId,
            CreatedAt = card.CreatedAt,
            CardType = "ai",
            FormType = card.FormType == CardFormType.Calligraphy ? "calligraphy" : "go_may",
            MessageContent = card.MessageContent,
            AttachedImageUrl = card.AttachedImageUrl,
            SizeName = card.SizeName,
            SizeWidth = card.SizeWidth,
            SizeHeight = card.SizeHeight,
            SizeBasePrice = card.SizeBasePrice,
            SizeMaxWords = card.SizeMaxWords,
            WordCount = card.WordCount,
            WordConfigSnapshot = ParseNullableJson(card.WordConfigSnapshot),
            BasePrice = card.BasePrice,
            ExtraPrice = card.ExtraPrice,
            TotalPrice = card.TotalPrice,
            CardTemplateName = metadata.HasValue ? GetCardTemplateName(metadata.Value) : null
        };
    }

    private static JsonElement ParseJson(string value)
    {
        using var document = JsonDocument.Parse(value);
        return document.RootElement.Clone();
    }

    private static JsonElement? ParseNullableJson(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : ParseJson(value);

    private static string? GetCardTemplateName(JsonElement metadata)
    {
        if (metadata.ValueKind != JsonValueKind.Object
            || !metadata.TryGetProperty("cardTemplate", out var cardTemplate)
            || cardTemplate.ValueKind != JsonValueKind.Object
            || !cardTemplate.TryGetProperty("name", out var name)
            || name.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return name.GetString();
    }

    private static bool TryGetCardTemplateName(string? metadata, out string templateName)
    {
        templateName = string.Empty;
        if (string.IsNullOrWhiteSpace(metadata)) return false;
        try
        {
            var parsed = ParseJson(metadata);
            var name = GetCardTemplateName(parsed);
            if (string.IsNullOrWhiteSpace(name)) return false;
            templateName = name;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed class AdminListValidationException(string detail)
        : AppException("Bad Request", 400, "VALIDATION_ERROR", detail);

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
        SizeWidth = card.SizeWidth, 
        SizeHeight = card.SizeHeight,
        SizeBasePrice = card.SizeBasePrice, 
        SizeMaxWords = card.SizeMaxWords, 
        WordCount = card.WordCount,
        WordConfigSnapshot = string.IsNullOrWhiteSpace(card.WordConfigSnapshot) ? null : JsonSerializer.Deserialize<Response.WordConfigSnapshot>(card.WordConfigSnapshot),
        BasePrice = card.BasePrice, 
        ExtraPrice = card.ExtraPrice, 
        TotalPrice = card.TotalPrice
    };

    /*
     * config systemPromptCard
{
  "group": "imagegen",
  "label": "Prompt thiệp",
  "value": "Tạo ảnh thiệp thật đẹp\n\n10 điểm",
  "options": [],
  "required": false,
  "inputType": "textarea",
  "validations": []
}
     */
}

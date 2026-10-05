using System.Text.Json;
using System.Text.Json.Nodes;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Ai = HoaTheoMua.Service.AiService;
using static HoaTheoMua.Service.Card.MappingCard;

namespace HoaTheoMua.Service.Card;

public record PreparedCard(Guid CardId, Guid UserId, Guid OrderDetailId, Guid VirtualId,
    Guid? SourceCardId, Guid BaseId, Guid RootId, CardSnapshot Snapshot, string HistoryMetadata)
{
    public Ai.Request.CardImageInput Input => new(Snapshot.SystemPrompt.Content, Snapshot.Source.ImageUrl,
        Snapshot.CardTemplate.TemplateImageUrl, Snapshot.MessageContent,
        Snapshot.FormType.ToLowerInvariant(), Snapshot.CardSize.Width, Snapshot.CardSize.Height,
        Snapshot.AttachedImageUrl, Snapshot.RawImage);
}

public partial class Service
{
    // Retained for internal synchronous callers. HTTP entrypoints submit durable jobs instead.
    public async Task<Response.CreateCardResponse> CreateCardAi(Request.CreateCardAiRequest request)
    {
        var ct = _httpContext.HttpContext?.RequestAborted ?? default;
        var input = await Prepare(request, ct);
        ValidateBranding();
        var image = await _ai.GenerateCardImage(input.Input, ct);
        var branded = await Brand(input, image, ct);
        await using var tx = _dbContext.Database.IsRelational() ? await _dbContext.Database.BeginTransactionAsync(ct) : null;
        var result = await AddPreparedResult(input, image, ct, branded);
        await _dbContext.SaveChangesAsync(ct);
        if (tx != null) await tx.CommitAsync(ct);
        return result;
    }

    public void ValidateBranding() => (_branding ?? throw new ServerException("Card branding is not configured")).ValidateConfiguration();

    public Task<Flower.BrandedFlowerImage> Brand(PreparedCard input, Ai.Response.GeneratedImage output, CancellationToken ct)
        => (_branding ?? throw new ServerException("Card branding is not configured")).ApplyLogoAsync(output.ImageUrl, input.CardId, ct);

    public async Task<PreparedCard> Prepare(Request.CreateCardAiRequest request, CancellationToken ct)
    {
        var userId = _currentUser.GetRequiredUserId();
        if (request.SourceCardId == Guid.Empty || request.CardTemplateId == Guid.Empty)
            throw new BadRequestException("VALIDATION_ERROR");
        if (request.SourceCardId.HasValue)
        {
            if (request.HasOrderDetailId || request.HasVirtualId)
                throw new BadRequestException("VALIDATION_ERROR");
            return await PrepareRegenerate(request, userId, ct);
        }
        if (!request.OrderDetailId.HasValue || !request.VirtualId.HasValue
            || request.OrderDetailId == Guid.Empty || request.VirtualId == Guid.Empty
            || request.HasCardTemplateId || request.HasMessageContent)
            throw new BadRequestException("VALIDATION_ERROR");

        var detail = await _dbContext.OrderDetails.AsNoTracking().Include(x => x.Order)
            .SingleOrDefaultAsync(x => x.Id == request.OrderDetailId && x.Order.UserId == userId
                && !x.IsDeleted && !x.Order.IsDeleted, ct)
            ?? throw new NotFoundException("ORDER_DETAIL_NOT_FOUND");
        if (!string.Equals(detail.Order.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            throw new ConflictException("ORDER_NOT_PAID", "Order chưa thanh toán.");
        var (_, _, snapshot, flowerId) = FindAiSnapshot(detail.Metadata, request.VirtualId.Value);
        EnsureNotCreated(snapshot);
        var (baseId, rootId) = await ResolveLineageAsync(snapshot, userId, flowerId);
        await ResolveBouquetImage(snapshot.Source, flowerId, userId, ct);
        ValidateSnapshotContent(snapshot);
        var history = BuildHistoryMetadata(snapshot, baseId, rootId, detail.Id, snapshot.VirtualId, flowerId, "create");
        return new(Guid.NewGuid(), userId, detail.Id, snapshot.VirtualId, null, baseId, rootId, snapshot, history);
    }

    private async Task<PreparedCard> PrepareRegenerate(Request.CreateCardAiRequest request, Guid userId, CancellationToken ct)
    {
        var card = await _dbContext.GeneratedCards.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.SourceCardId && x.UserId == userId, ct)
            ?? throw new NotFoundException("CARD_NOT_FOUND");
        if (card.CardType != GeneratedCardType.Ai)
            throw new ConflictException("HANDMADE_CARD_REGENERATE_NOT_SUPPORTED", "Chỉ tạo lại thiệp AI.");
        var histories = await _dbContext.ClientHistories.AsNoTracking().Where(x => x.UserId == userId
            && x.Type == ClientHistoryType.Card && x.OutputId == card.Id).ToListAsync(ct);
        if (histories.Count != 1) throw InvalidSource();
        var history = histories[0];
        var source = ParseSourceHistoryMetadata(history.Metadata);
        if (!source.OrderDetailId.HasValue || !source.VirtualId.HasValue || history.RootId == Guid.Empty)
            throw InvalidSource();
        var detail = await _dbContext.OrderDetails.AsNoTracking().Include(x => x.Order)
            .SingleOrDefaultAsync(x => x.Id == source.OrderDetailId && x.Order.UserId == userId
                && !x.IsDeleted && !x.Order.IsDeleted, ct) ?? throw InvalidSource();
        var (_, _, linked, flowerId) = FindAiSnapshot(detail.Metadata, source.VirtualId.Value);
        if (!linked.IdResult.Contains(card.Id) || (linked.FinalGeneratedCardId.HasValue
            && !linked.IdResult.Contains(linked.FinalGeneratedCardId.Value))) throw InvalidSource();
        if (source.Source!.Id != history.RootId || source.GeneratedFlowerId != flowerId
            || source.CardTemplate!.Id == Guid.Empty) throw InvalidSource();
        var template = source.CardTemplate;
        if (request.CardTemplateId.HasValue && request.CardTemplateId != template.Id)
        {
            var current = await _dbContext.CardTemplates.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.CardTemplateId, ct)
                ?? throw new NotFoundException("CARD_TEMPLATE_NOT_FOUND");
            if (current.IsDeleted) throw new ConflictException("CARD_TEMPLATE_DELETED", "Template đã xóa.");
            if (!current.IsActive) throw new ConflictException("CARD_TEMPLATE_INACTIVE", "Template không active.");
            if (current.TemplateType != CardTemplateType.Ai) throw new ConflictException("CARD_TEMPLATE_TYPE_INVALID", "Template phải là AI.");
            template = new() { Id = current.Id, Name = current.Name, TemplateType = "ai", ImageUrl = current.ImageUrl };
        }
        var prompt = await CardPromptConfig.LoadAsync(_dbContext, ct);
        var promptSource = prompt == null ? "source_card_fallback" : "current";
        prompt ??= ParseSystemPrompt(history.SystemForm);
        if (prompt == null) throw new ConflictException("SYSTEM_PROMPT_INVALID", "Không có prompt thiệp hợp lệ.");
        var message = string.IsNullOrWhiteSpace(request.MessageContent) ? card.MessageContent : request.MessageContent;
        var words = message.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        ValidateRegeneratedWordCount(card, words);
        var snapshot = new CardSnapshot
        {
            VirtualId = source.VirtualId.Value, CardType = "ai", Quantity = 1,
            Source = new() { Type = "product", Id = source.Source.Id, ImageUrl = source.Source.ImageUrl,
                ImageSourceType = source.Source.ImageSourceType },
            CardTemplate = new() { TemplateId = template.Id, TemplateName = template.Name,
                TemplateType = template.TemplateType, TemplateImageUrl = template.ImageUrl },
            CardSize = new() { Name = card.SizeName, Width = card.SizeWidth, Height = card.SizeHeight,
                BasePrice = card.SizeBasePrice, MaxWords = card.SizeMaxWords },
            CardConfig = string.IsNullOrWhiteSpace(card.WordConfigSnapshot) ? null : JsonSerializer.Deserialize<JsonElement>(card.WordConfigSnapshot),
            MessageContent = message, FormType = card.FormType == CardFormType.GoMay ? "go_may" : "calligraphy",
            AttachedImageUrl = card.AttachedImageUrl, WordCount = words, BasePrice = card.BasePrice,
            ExtraPrice = card.ExtraPrice, TotalPrice = card.TotalPrice, SystemPrompt = prompt, ValidatedAt = DateTimeOffset.UtcNow
        };
        await ResolveBouquetImage(snapshot.Source, flowerId, userId, ct);
        if (!IsValidSnapshot(snapshot)) throw InvalidSource();
        var metadata = JsonNode.Parse(BuildHistoryMetadata(snapshot, history.BaseId, history.RootId,
            detail.Id, snapshot.VirtualId, flowerId, "regenerate"))!.AsObject();
        metadata["regeneratedFromCardId"] = card.Id.ToString();
        metadata["systemPromptSource"] = promptSource;
        return new(Guid.NewGuid(), userId, detail.Id, snapshot.VirtualId, card.Id,
            history.BaseId, history.RootId, snapshot, metadata.ToJsonString());
    }

    private async Task ResolveBouquetImage(SnapshotSource source, Guid? flowerId, Guid userId, CancellationToken ct)
    {
        var expected = flowerId.HasValue ? "generated_flower" : "product";
        if (source.ImageSourceType != null && source.ImageSourceType != expected) throw InvalidSource();
        // Legacy snapshots always held Product artwork. Upgrade only those; new snapshots remain immutable.
        if (flowerId.HasValue && source.ImageSourceType == null)
            source.ImageUrl = await _dbContext.GeneratedFlowers.AsNoTracking()
                .Where(x => x.Id == flowerId && x.UserId == userId).Select(x => x.ImageUrl)
                .SingleOrDefaultAsync(ct) ?? throw InvalidSource();
        source.ImageSourceType = expected;
    }

    private static void EnsureNotCreated(CardSnapshot snapshot)
    {
        if (!string.Equals(snapshot.Status, "not yet", StringComparison.OrdinalIgnoreCase)
            || snapshot.IdResult.Count != 0 || snapshot.FinalGeneratedCardId.HasValue)
            throw new ConflictException("CARD_ALREADY_CREATED", "Thiệp đã được tạo.");
    }

    private static void ValidateSnapshotContent(CardSnapshot snapshot)
    {
        var words = snapshot.MessageContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        if (snapshot.Quantity != 1 || words == 0 || words != snapshot.WordCount || words > 100
            || words > snapshot.CardSize.MaxWords || snapshot.BasePrice < 0 || snapshot.ExtraPrice < 0
            || snapshot.TotalPrice != snapshot.BasePrice + snapshot.ExtraPrice)
            throw new ConflictException("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", "Cấu hình thiệp không hợp lệ.");
        ValidateRegeneratedWordCount(MapCard(snapshot, Guid.Empty, new("", "", "")), words);
    }

    // Called inside a short transaction (also includes job success). Lock and reload the current JSON,
    // so another card/selection committed while AI ran is preserved.
    public async Task<Response.CreateCardResponse> AddPreparedResult(PreparedCard input, Ai.Response.GeneratedImage output, CancellationToken ct, Flower.BrandedFlowerImage? branded = null)
    {
        var detail = await LockOrderDetail(input.OrderDetailId, ct);
        if (detail == null || detail.IsDeleted) throw InvalidSource();
        var order = await _dbContext.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == detail.OrderId
            && x.UserId == input.UserId && !x.IsDeleted, ct) ?? throw InvalidSource();
        var (metadata, node, snapshot, _) = FindAiSnapshot(detail.Metadata, input.VirtualId);
        if (input.SourceCardId.HasValue)
        {
            if (!snapshot.IdResult.Contains(input.SourceCardId.Value)) throw InvalidSource();
        }
        else
        {
            if (!string.Equals(order.Status, "Paid", StringComparison.OrdinalIgnoreCase)) throw InvalidSource();
            EnsureNotCreated(snapshot);
        }
        if (snapshot.FinalGeneratedCardId.HasValue && !snapshot.IdResult.Contains(snapshot.FinalGeneratedCardId.Value))
            throw InvalidSource();
        if (string.IsNullOrWhiteSpace(output.ImageUrl)) throw new ServerException("AI không trả ảnh Card hợp lệ.");
        // One provider render produces the complete card. RawImage preserves a supplied reusable image,
        // otherwise aliases the output; it is not a promise of a text-free background.
        var card = MapCard(input.Snapshot, input.UserId,
            new(input.Snapshot.MessageContent, (branded ?? throw new ServerException("Card logo is missing")).ImageUrl, input.Snapshot.RawImage ?? output.ImageUrl));
        card.Id = input.CardId;
        var historyMetadata = JsonNode.Parse(input.HistoryMetadata)!.AsObject();
        historyMetadata["branding"] = JsonSerializer.SerializeToNode(branded, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        historyMetadata["generation"] = JsonSerializer.SerializeToNode(output, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        _dbContext.GeneratedCards.Add(card);
        _dbContext.ClientHistories.Add(new ClientHistory
        {
            Id = Guid.NewGuid(), UserId = input.UserId, Type = ClientHistoryType.Card,
            BaseId = input.BaseId, RootId = input.RootId, OutputId = card.Id, CreatedAt = card.CreatedAt,
            SessionId = null, SystemForm = JsonSerializer.Serialize(input.Snapshot.SystemPrompt), Metadata = historyMetadata.ToJsonString()
        });
        node["status"] = "done";
        node["idResult"]!.AsArray().Add(card.Id.ToString());
        if (!input.SourceCardId.HasValue)
        {
            node["source"] = JsonSerializer.SerializeToNode(input.Snapshot.Source, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            node["rawImage"] = card.RawImage;
        }
        detail.Metadata = metadata.ToJsonString();
        return ToResponse(card);
    }

    private async Task<OrderDetails?> LockOrderDetail(Guid id, CancellationToken ct)
    {
        var tracked = _dbContext.ChangeTracker.Entries<OrderDetails>().FirstOrDefault(x => x.Entity.Id == id);
        if (tracked != null) tracked.State = EntityState.Detached;
        return _dbContext.Database.IsRelational()
            ? await _dbContext.OrderDetails.FromSqlInterpolated($"SELECT * FROM \"OrderDetails\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(ct)
            : await _dbContext.OrderDetails.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    private static ConflictException InvalidSource() => new("CARD_SOURCE_HISTORY_INVALID", "Nguồn thiệp hoặc liên kết Order không hợp lệ.");
}

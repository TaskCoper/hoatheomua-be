using System.Text.Json;
using System.Text.Json.Serialization;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Ai = HoaTheoMua.Service.AiService;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using ProductService = HoaTheoMua.Service.Product.IService;
using ProductServiceImplementation = HoaTheoMua.Service.Product.Service;
using FlowerReferenceEntity = HoaTheoMua.Repository.Entity.FlowerReference;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.Flower;

public class Service : IService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly Ai.IService _ai;
    private readonly ProductService _products;

    public Service(AppDbContext db, ICurrentUserService currentUser, Ai.IService ai, ProductService products)
    {
        _db = db;
        _currentUser = currentUser;
        _ai = ai;
        _products = products;
    }

    public async Task<Response.GeneratedFlowerResponse> CreateAiFlower(Request.CreateFlowerRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();
        ValidateCreateRequest(request);

        var product = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProductId && !x.IsDeleted)
            ?? throw new NotFoundException("PRODUCT_NOT_FOUND", "Không tìm thấy Product.");

        var specs = await _db.ComboSpecifications.AsNoTracking()
            .Include(x => x.Mat)
            .Where(x => x.SubId == product.Id && x.IsActive && !x.IsDeleted && !x.Mat.IsDeleted)
            .ToListAsync();
        if (specs.Count == 0)
            throw new ConflictException("PRODUCT_SPECIFICATION_NOT_FOUND", "Product chưa có công thức.");

        var materialTotals = ValidateMaterials(request.MaterialTotals, specs);
        var availability = await CalculateAvailableQty(product.Id);
        if (!availability.TryGetValue(product.Id, out var quantity)
            || quantity.UnavailableReason is not null
            || quantity.SellableQuantity is null)
            throw new FlowerException(503, "PRODUCT_AVAILABILITY_UNAVAILABLE", "Không xác minh được khả năng bán của Product.");
        if (quantity.SellableQuantity <= 0)
            throw new ConflictException("PRODUCT_OUT_OF_STOCK", "Product đã hết hàng.");

        var ribbon = await LoadAccessory(request.RibbonProductId, "RUY BĂNG", "RIBBON");
        var wrappingPaper = await LoadAccessory(request.WrappingPaperProductId, "GIẤY", "WRAPPING_PAPER");
        var bundle = await LoadReference(request.BundleId, FlowerReferenceType.Bundle, "BUNDLE");
        var mockup = await LoadReference(request.MockupId, FlowerReferenceType.Flower, "MOCKUP");
        var prompt = await LoadPrompt()
            ?? throw new ConflictException("SYSTEM_PROMPT_INVALID", "Không có System prompt flower hợp lệ.");

        var metadata = new FlowerMetadata
        {
            SubId = product.Id,
            AiCustomize = true,
            Ribbon = ToProductSnapshot(ribbon),
            WrappingPaper = ToProductSnapshot(wrappingPaper),
            Materials = specs.Select(x => ToMaterialSnapshot(x.Mat, x.Quantity!.Value, x.IsCore)).ToList(),
            AdditionCustome = materialTotals.Additions
        };

        var image = await GenerateImage(product.CoverImage, bundle.ImageUrl, mockup.ImageUrl,
            request.Name!.Trim(), prompt.Content, metadata);

        var flower = new GeneratedFlower
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ImageUrl = image.ImageUrl,
            Metadata = JsonSerializer.Serialize(metadata, JsonOptions),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var history = CreateHistory(userId, ToProductSnapshot(product), bundle, mockup, flower,
            new PromptSnapshot { Type = "flower", Content = prompt.Content }, "create", null, Guid.NewGuid());

        await using var transaction = await _db.Database.BeginTransactionAsync();
        _db.GeneratedFlowers.Add(flower);
        _db.ClientHistories.Add(history);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToResponse(flower);
    }

    public async Task<Response.GeneratedFlowerResponse> RegenerateAiFlower(
        Guid sourceFlowerId, Request.RegenerateFlowerRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();
        var source = await _db.GeneratedFlowers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sourceFlowerId && x.UserId == userId)
            ?? throw new NotFoundException("FLOWER_SOURCE_NOT_FOUND", "Không tìm thấy Flower nguồn.");

        FlowerMetadata metadata;
        HistoryMetadata sourceMetadata;
        ClientHistory sourceHistory;
        try
        {
            metadata = JsonSerializer.Deserialize<FlowerMetadata>(source.Metadata, JsonOptions)
                       ?? throw new JsonException();
            sourceHistory = await _db.ClientHistories.SingleAsync(x =>
                x.UserId == userId && x.Type == ClientHistoryType.Flower && x.OutputId == source.Id);
            sourceMetadata = JsonSerializer.Deserialize<HistoryMetadata>(sourceHistory.Metadata, JsonOptions)
                             ?? throw new JsonException();
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            throw new ConflictException("FLOWER_SOURCE_HISTORY_INVALID", "Flower nguồn thiếu History hoặc Metadata hợp lệ.");
        }

        if (sourceHistory.BaseId == Guid.Empty || sourceHistory.RootId == Guid.Empty
            || sourceHistory.BaseId != sourceHistory.RootId
            || sourceMetadata.Product is null || sourceMetadata.Bundle is null || sourceMetadata.Mockup is null
            || metadata.Ribbon is null || metadata.WrappingPaper is null || metadata.Materials is null)
            throw new ConflictException("FLOWER_SOURCE_HISTORY_INVALID", "Lineage Flower nguồn không hợp lệ.");

        var bundle = await ResolveReference(request.BundleId, sourceMetadata.Bundle, FlowerReferenceType.Bundle, "BUNDLE");
        var mockup = await ResolveReference(request.MockupId, sourceMetadata.Mockup, FlowerReferenceType.Flower, "MOCKUP");

        var prompt = await LoadPrompt();
        var promptContent = prompt?.Content;
        var promptSource = "current";
        if (string.IsNullOrWhiteSpace(promptContent))
        {
            promptContent = ParseSourcePrompt(sourceHistory.SystemForm)?.Content;
            promptSource = "source_flower_fallback";
        }
        if (string.IsNullOrWhiteSpace(promptContent))
            throw new ConflictException("FLOWER_SOURCE_HISTORY_INVALID", "Flower nguồn không có Prompt hợp lệ.");

        var image = await GenerateImage(sourceMetadata.Product.Image.Url, bundle.ImageUrl, mockup.ImageUrl,
            source.Name, promptContent, metadata);
        var flower = new GeneratedFlower
        {
            Id = Guid.NewGuid(),
            Name = source.Name,
            ImageUrl = image.ImageUrl,
            Metadata = JsonSerializer.Serialize(metadata, JsonOptions),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var history = CreateHistory(userId, sourceMetadata.Product, bundle, mockup, flower,
            new PromptSnapshot { Type = "flower", Content = promptContent }, "regenerate", source.Id,
            sourceHistory.SessionId, promptSource, sourceHistory.BaseId);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        _db.GeneratedFlowers.Add(flower);
        _db.ClientHistories.Add(history);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToResponse(flower);
    }

    private async Task<Ai.Response.GeneratedImage> GenerateImage(string? productImage, string bundleImage,
        string mockupImage, string name, string prompt, FlowerMetadata metadata)
    {
        if (string.IsNullOrWhiteSpace(productImage))
            throw new ConflictException("PRODUCT_IMAGE_INVALID", "Product không có ảnh hợp lệ.");

        var instruction = $"{prompt}\nTên bó: {name}\nMockup image: {mockupImage}\n" +
                          $"Flower data: {JsonSerializer.Serialize(metadata, JsonOptions)}\n" +
                          "Tạo ảnh bó hoa hoàn chỉnh, không thêm chữ hoặc logo.";
        /* REAL AI: temporarily disabled for fake.
        return await _ai.GenerateImage(new Ai.Request.ImageInput(
            productImage, "1:1", 1024, 1024, instruction, bundleImage));
        */
        return await FakeGenerateFlowerAsync();
    }

    // FAKE AI: fixture only; no provider, upload, or network call is made here.
    private static Task<Ai.Response.GeneratedImage> FakeGenerateFlowerAsync() => Task.FromResult(
        new Ai.Response.GeneratedImage(
            "https://pos.nvncdn.com/281991-126981/ps/Tu-cau-bo-1-bong_-2026.png?v=1777817025",
            "FAKE", "fake-flower", 0));

    private async Task<ProductEntity> LoadAccessory(Guid id, string label, string code)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (product is null) throw new NotFoundException($"{code}_PRODUCT_NOT_FOUND", "Không tìm thấy Product phụ kiện.");
        if (product.IsDeleted) throw new FlowerException(410, $"{code}_PRODUCT_DELETED", "Product phụ kiện đã bị xóa.");
        if (!product.IsActive) throw new ConflictException($"{code}_PRODUCT_INACTIVE", "Product phụ kiện không active.");
        if (!HasLabel(product.ProductLabel, label))
            throw new ConflictException($"{code}_PRODUCT_LABEL_INVALID", "Product phụ kiện sai label.");
        return product;
    }

    private async Task<FlowerReferenceEntity> LoadReference(Guid id, FlowerReferenceType type, string code)
    {
        var reference = await _db.FlowerReferences.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (reference is null) throw new NotFoundException($"{code}_NOT_FOUND", "Không tìm thấy Flower reference.");
        if (reference.Type != type) throw new ConflictException($"{code}_TYPE_INVALID", "Flower reference sai type.");
        if (reference.IsDeleted) throw new ConflictException($"{code}_DELETED", "Flower reference đã bị xóa.");
        if (!reference.IsActive) throw new ConflictException($"{code}_INACTIVE", "Flower reference không active.");
        return reference;
    }

    private async Task<FlowerReferenceEntity> ResolveReference(Guid? id, ReferenceSnapshot snapshot,
        FlowerReferenceType type, string code)
    {
        if (!id.HasValue || id.Value == snapshot.Id)
            return new FlowerReferenceEntity
            {
                Id = snapshot.Id, Name = snapshot.Name ?? "", Description = snapshot.Description,
                ImageUrl = snapshot.Image.Url ?? "", Type = type, IsActive = true
            };
        return await LoadReference(id.Value, type, code);
    }

    private async Task<PromptSnapshot?> LoadPrompt()
    {
        var configs = await _db.Configs.AsNoTracking()
            .Where(x => x.Key == "system_prompt.flower" && !x.IsDeleted)
            .ToListAsync();
        if (configs.Count == 0) return null;
        if (configs.Count != 1
            || !string.Equals(configs[0].Group?.Trim(), "system_prompt", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(configs[0].Kind?.Trim(), "setting", StringComparison.OrdinalIgnoreCase)
            || !configs[0].IsPublic)
        {
            throw new ConflictException(
                "SYSTEM_PROMPT_INVALID",
                "System prompt flower Config không hợp lệ.");
        }

        try
        {
            using var value = JsonDocument.Parse(configs[0].Value);
            var root = value.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("value", out var content)
                || content.ValueKind != JsonValueKind.String)
                throw new JsonException();

            ValidatePromptDescription(root);
            var prompt = content.GetString();
            return string.IsNullOrWhiteSpace(prompt) ? null : new PromptSnapshot { Content = prompt };
        }
        catch (JsonException)
        {
            throw new ConflictException("SYSTEM_PROMPT_INVALID", "System prompt flower Config không hợp lệ.");
        }
    }

    private static void ValidatePromptDescription(JsonElement root)
    {
        if (!root.TryGetProperty("description", out var description) || description.ValueKind != JsonValueKind.String)
            throw new JsonException();
        using var parsed = JsonDocument.Parse(description.GetString()!);
        if (parsed.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException();
    }

    private static PromptSnapshot? ParseSourcePrompt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            var prompt = JsonSerializer.Deserialize<PromptSnapshot>(value, JsonOptions);
            return prompt is { Type: var type, Content: var content }
                && string.Equals(type, "flower", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(content)
                ? prompt
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private ClientHistory CreateHistory(Guid userId, ProductSnapshot product, FlowerReferenceEntity bundle,
        FlowerReferenceEntity mockup, GeneratedFlower flower, PromptSnapshot prompt, string operation,
        Guid? regeneratedFromFlowerId, Guid? sessionId, string promptSource = "current", Guid? baseId = null) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, Type = ClientHistoryType.Flower,
        BaseId = baseId ?? product.Id, RootId = baseId ?? product.Id, OutputId = flower.Id,
        SessionId = sessionId,
        CreatedAt = flower.CreatedAt, SystemForm = JsonSerializer.Serialize(prompt, JsonOptions),
        Metadata = JsonSerializer.Serialize(new HistoryMetadata
        {
            SchemaVersion = 3, Operation = operation, Product = product,
            Bundle = ToReferenceSnapshot(bundle), Mockup = ToReferenceSnapshot(mockup),
            LogoConfig = new { }, RegeneratedFromFlowerId = regeneratedFromFlowerId,
            SystemPromptSource = promptSource, ValidatedAt = flower.CreatedAt
        }, JsonOptions)
    };

    private static (List<MaterialSnapshot> Materials, List<MaterialSnapshot> Additions) ValidateMaterials(
        List<Request.MaterialTotalRequest> requested, List<ComboSpecification> specs)
    {
        if (requested.Count != requested.Select(x => x.MatId).Distinct().Count())
            throw new FlowerException(400, "MATERIAL_TOTALS_DUPLICATE_MATERIAL", "MaterialTotals chứa MatId trùng.");
        if (requested.Any(x => x.MatId == Guid.Empty || x.Quantity < 0))
            throw new FlowerException(400, "VALIDATION_ERROR", "MaterialTotals không hợp lệ.");

        var byId = requested.ToDictionary(x => x.MatId, x => x.Quantity);
        var specIds = specs.Select(x => x.MatId).ToHashSet();
        if (specIds.Any(x => !byId.ContainsKey(x))) throw new FlowerException(400, "MATERIAL_TOTALS_MISSING_MATERIAL", "Thiếu nguyên liệu trong công thức.");
        if (byId.Keys.Any(x => !specIds.Contains(x))) throw new FlowerException(400, "MATERIAL_TOTALS_UNKNOWN_MATERIAL", "Có nguyên liệu ngoài công thức.");
        if (specs.Any(x => !x.Quantity.HasValue || x.Quantity.Value < 0 || byId[x.MatId] < x.Quantity.Value))
            throw new FlowerException(400, "VALIDATION_ERROR", "Công thức hoặc số lượng nguyên liệu không hợp lệ.");

        var materials = specs.Select(x => ToMaterialSnapshot(x.Mat, x.Quantity!.Value, x.IsCore)).ToList();
        var additions = specs.Where(x => byId[x.MatId] > x.Quantity!.Value)
            .Select(x => ToMaterialSnapshot(x.Mat, byId[x.MatId] - x.Quantity!.Value, x.IsCore)).ToList();
        return (materials, additions);
    }

    private static void ValidateCreateRequest(Request.CreateFlowerRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Name) || request.ProductId == Guid.Empty
            || request.BundleId == Guid.Empty || request.MockupId == Guid.Empty
            || request.RibbonProductId == Guid.Empty || request.WrappingPaperProductId == Guid.Empty
            || request.MaterialTotals is null || request.MaterialTotals.Count == 0)
            throw new FlowerException(400, "VALIDATION_ERROR", "Request không hợp lệ.");
    }

    private static bool HasLabel(string? labels, string expected) => labels?.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Any(x => string.Equals(x.Trim(), expected, StringComparison.OrdinalIgnoreCase)) == true;

    private static ProductSnapshot ToProductSnapshot(ProductEntity product) => new()
    {
        Id = product.Id, Name = product.ProductName,
        Attributes = new { product.ProductCode, product.ProductType, product.Size, product.Color },
        Image = new ImageSnapshot { Url = product.CoverImage },
        ObservedStatus = new { product.IsActive, product.IsDeleted }
    };

    private static ReferenceSnapshot ToReferenceSnapshot(FlowerReferenceEntity reference) => new()
    {
        Id = reference.Id, Name = reference.Name, Description = reference.Description,
        Image = new ImageSnapshot { Url = reference.ImageUrl },
        ObservedStatus = new { reference.IsActive, reference.IsDeleted }
    };

    private static MaterialSnapshot ToMaterialSnapshot(ProductEntity product, decimal quantity, bool isCore) => new()
    {
        MatId = product.Id, Quantity = quantity, Name = product.ProductName, IsCore = isCore
    };

    private static Response.GeneratedFlowerResponse ToResponse(GeneratedFlower flower) => new()
    {
        Id = flower.Id, Name = flower.Name, ImageUrl = flower.ImageUrl,
        Metadata = JsonDocument.Parse(flower.Metadata).RootElement.Clone(), CreatedAt = flower.CreatedAt
    };

    private Task<Dictionary<Guid, (int? SellableQuantity, string? StockStatus, string? UnavailableReason)>> CalculateAvailableQty(Guid productId) =>
        (_products as ProductServiceImplementation)?.CalculateAvailableQty([productId])
        ?? throw new FlowerException(503, "PRODUCT_AVAILABILITY_UNAVAILABLE", "Không xác minh được khả năng bán của Product.");

    private sealed class FlowerException(int statusCode, string messageCode, string detail)
        : AppException("Flower", statusCode, messageCode, detail);

    private class FlowerMetadata
    {
        [JsonPropertyName("subId")] 
        public Guid SubId { get; set; }
        [JsonPropertyName("aiCustomize")] 
        public bool AiCustomize { get; set; }
        [JsonPropertyName("ribbon")] 
        public ProductSnapshot? Ribbon { get; set; }
        [JsonPropertyName("wrappingPaper")] 
        public ProductSnapshot? WrappingPaper { get; set; }
        [JsonPropertyName("AdditionCustome")] 
        public List<MaterialSnapshot> AdditionCustome { get; set; } = [];
        [JsonPropertyName("materials")] 
        public List<MaterialSnapshot> Materials { get; set; } = [];
    }

    private class HistoryMetadata
    {
        [JsonPropertyName("schemaVersion")] 
        public int SchemaVersion { get; set; }
        [JsonPropertyName("operation")] 
        public string Operation { get; set; } = null!;
        [JsonPropertyName("product")] 
        public ProductSnapshot? Product { get; set; }
        [JsonPropertyName("bundle")]
        public ReferenceSnapshot? Bundle { get; set; }
        [JsonPropertyName("mockup")] 
        public ReferenceSnapshot? Mockup { get; set; }
        [JsonPropertyName("logoConfig")] 
        public object? LogoConfig { get; set; }
        [JsonPropertyName("regeneratedFromFlowerId")] 
        public Guid? RegeneratedFromFlowerId { get; set; }
        [JsonPropertyName("systemPromptSource")] 
        public string? SystemPromptSource { get; set; }
        [JsonPropertyName("validatedAt")] 
        public DateTimeOffset ValidatedAt { get; set; }
    }

    private class ProductSnapshot
    {
        [JsonPropertyName("id")] 
        public Guid Id { get; set; }
        [JsonPropertyName("name")] 
        public string? Name { get; set; }
        [JsonPropertyName("attributes")] 
        public object? Attributes { get; set; }
        [JsonPropertyName("image")] 
        public ImageSnapshot Image { get; set; } = new();
        [JsonPropertyName("observedStatus")] 
        public object? ObservedStatus { get; set; }
    }

    private class ReferenceSnapshot
    {
        [JsonPropertyName("id")] 
        public Guid Id { get; set; }
        [JsonPropertyName("name")] 
        public string? Name { get; set; }
        [JsonPropertyName("description")] 
        public string? Description { get; set; }
        [JsonPropertyName("image")] 
        public ImageSnapshot Image { get; set; } = new();
        [JsonPropertyName("observedStatus")] 
        public object? ObservedStatus { get; set; }
    }

    private class ImageSnapshot
    {
        [JsonPropertyName("url")] 
        public string? Url { get; set; }
    }

    private class MaterialSnapshot
    {
        [JsonPropertyName("matId")] 
        public Guid MatId { get; set; }
        [JsonPropertyName("quantity")] 
        public decimal Quantity { get; set; }
        [JsonPropertyName("name")] 
        public string? Name { get; set; }
        [JsonPropertyName("isCore")] 
        public bool IsCore { get; set; }
    }

    private class PromptSnapshot
    {
        [JsonPropertyName("type")] 
        public string Type { get; set; } = "flower";
        [JsonPropertyName("content")] 
        public string Content { get; set; } = null!;
    }
    
    /*
     * config systemPrompt Flower
{
  "Key": "system_prompt.flower",
  "Group": "system_prompt",
  "Kind": "Setting",
  "IsPublic": false,
  "Value": {
    "group": "system_prompt",
    "label": "Flower",
    "value": "Nội dung prompt tạo ảnh bó hoa...",
    "options": [],
    "required": false,
    "inputType": "textarea",
    "description": "{\"logo\":\"https://taskcoper.hcm.ss.bfcplatform.vn/general/u1utst-fb.webp\",\"isActive\":true}",
    "validations": []
  }
}
     */
}

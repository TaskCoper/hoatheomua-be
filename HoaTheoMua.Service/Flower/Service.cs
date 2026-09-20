using System.Text.Json;
using System.Text.Json.Nodes;
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
using static HoaTheoMua.Service.Flower.MappingFlower;

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

    public async Task<HoaTheoMua.Service.Models.BasePaginationResponse> GetCustomerGeneratedFlowers(
        Request.CustomerGeneratedFlowerListRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCustomerListRequest(request);
        var pageIndex = request.pageIndex <= 0 ? 1 : request.pageIndex;
        var pageSize = request.pageSize <= 0 ? 10 : Math.Min(request.pageSize, 100);
        var userId = _currentUser.GetRequiredUserId();

        var historyQuery = _db.ClientHistories.AsNoTracking()
            .Where(history => history.UserId == userId
                && history.Type == ClientHistoryType.Flower
                && history.SessionId.HasValue);
        if (request.sessionId.HasValue)
            historyQuery = historyQuery.Where(history => history.SessionId == request.sessionId.Value);

        var histories = await historyQuery.ToListAsync(cancellationToken);
        var outputIds = histories.Select(history => history.OutputId).Distinct().ToArray();
        var flowers = outputIds.Length == 0
            ? []
            : await _db.GeneratedFlowers.AsNoTracking()
                .Where(flower => flower.UserId == userId && outputIds.Contains(flower.Id))
                .ToListAsync(cancellationToken);
        var flowerById = flowers.ToDictionary(flower => flower.Id);

        var validItems = new List<(Guid SessionId, Response.CustomerGeneratedFlowerResponse Flower)>();
        foreach (var history in histories)
        {
            FlowerMetadata? flowerMetadata;
            HistoryMetadata? historyMetadata;
            try
            {
                flowerMetadata = JsonSerializer.Deserialize<FlowerMetadata>(flowerById
                    .GetValueOrDefault(history.OutputId)?.Metadata ?? string.Empty, JsonOptions);
                historyMetadata = JsonSerializer.Deserialize<HistoryMetadata>(history.Metadata, JsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (!history.SessionId.HasValue
                || !flowerById.TryGetValue(history.OutputId, out var flower)
                || flowerMetadata == null
                || historyMetadata == null
                || flowerMetadata.SubId != history.BaseId
                || history.RootId == Guid.Empty
                || !TryParseFlowerHistory(history.Metadata, out var operation, out var regeneratedFromFlowerId))
            {
                continue;
            }

            validItems.Add((history.SessionId.Value, new Response.CustomerGeneratedFlowerResponse
            {
                HistoryId = history.Id,
                Operation = operation,
                RegeneratedFromFlowerId = regeneratedFromFlowerId,
                ProductId = flowerMetadata.SubId,
                RootProductId = history.RootId,
                Id = flower.Id,
                Name = flower.Name,
                ImageUrl = flower.ImageUrl,
                UserId = flower.UserId,
                CreatedAt = flower.CreatedAt,
                Product = ToCustomerReference(historyMetadata.Product),
                Bundle = ToCustomerReference(historyMetadata.Bundle),
                Mockup = ToCustomerReference(historyMetadata.Mockup),
                Ribbon = ToCustomerReference(flowerMetadata.Ribbon),
                WrappingPaper = ToCustomerReference(flowerMetadata.WrappingPaper),
                Metadata = ParseJson(flower.Metadata)
            }));
        }

        var groups = validItems
            .GroupBy(item => item.SessionId)
            .Select(group => new Response.CustomerFlowerSessionResponse
            {
                SessionId = group.Key,
                LatestCreatedAt = group.Max(item => item.Flower.CreatedAt),
                Flowers = group
                    .Select(item => item.Flower)
                    .OrderByDescending(flower => flower.CreatedAt)
                    .ThenByDescending(flower => flower.Id)
                    .ToList()
            })
            .OrderByDescending(group => group.LatestCreatedAt)
            .ThenByDescending(group => group.SessionId)
            .ToList();

        var totalCount = groups.Count;
        var items = groups
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return HoaTheoMua.Service.Models.ApiResponseFactory.BasePagination(
            items,
            pageIndex,
            pageSize,
            totalCount);
    }

    public async Task<HoaTheoMua.Service.Models.BasePaginationResponse> GetAdminGeneratedFlowers(
        Request.AdminGeneratedFlowerListRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateAdminListRequest(request);
        var pageIndex = request.pageIndex <= 0 ? 1 : request.pageIndex;
        var pageSize = request.pageSize <= 0 ? 10 : Math.Min(request.pageSize, 100);
        var query = _db.GeneratedFlowers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.generatedFlowerName))
        {
            var search = request.generatedFlowerName.Trim().ToLower();
            query = query.Where(flower => flower.Name.ToLower().Contains(search));
        }

        var flowers = await query
            .OrderByDescending(flower => flower.CreatedAt)
            .ThenByDescending(flower => flower.Id)
            .ToListAsync(cancellationToken);
        if (request.productId.HasValue)
            flowers = flowers
                .Where(flower => TryGetProductId(flower.Metadata, out var productId)
                    && productId == request.productId.Value)
                .ToList();

        var totalCount = flowers.Count;
        var pagedFlowers = flowers
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var flowerIds = pagedFlowers.Select(flower => flower.Id).ToArray();
        var histories = flowerIds.Length == 0
            ? []
            : await _db.ClientHistories.AsNoTracking()
                .Where(history => history.Type == ClientHistoryType.Flower && flowerIds.Contains(history.OutputId))
                .OrderByDescending(history => history.CreatedAt)
                .ThenByDescending(history => history.Id)
                .ToListAsync(cancellationToken);
        var historyByFlower = histories
            .GroupBy(history => (history.OutputId, history.UserId))
            .ToDictionary(group => group.Key, group => group.First());
        var materialIds = pagedFlowers.SelectMany(flower => GetMaterialIds(flower.Metadata)).Distinct().ToArray();
        var materialCoverImages = materialIds.Length == 0
            ? new Dictionary<Guid, string?>()
            : await _db.Products.AsNoTracking()
                .Where(product => materialIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id, product => product.CoverImage, cancellationToken);
        var items = pagedFlowers
            .Select(flower => ToAdminResponse(
                flower,
                historyByFlower.GetValueOrDefault((flower.Id, flower.UserId)),
                EnrichMaterialCoverImages(flower.Metadata, materialCoverImages)))
            .ToList();
        return HoaTheoMua.Service.Models.ApiResponseFactory.BasePagination(
            items,
            pageIndex,
            pageSize,
            totalCount);
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

        return ToResponse(flower, history.SessionId!.Value);
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

        return ToResponse(flower, history.SessionId!.Value);
    }

    public async Task<Response.GeneratedFlowerCompositionResponse> GetGeneratedFlowerComposition(
        Guid generatedFlowerId)
    {
        if (generatedFlowerId == Guid.Empty)
            throw new BadRequestException("GENERATED_FLOWER_ID_INVALID");

        var userId = _currentUser.GetRequiredUserId();
        var flower = await _db.GeneratedFlowers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == generatedFlowerId && x.UserId == userId)
            ?? throw new NotFoundException("GENERATED_FLOWER_NOT_FOUND");

        FlowerMetadata metadata;
        try
        {
            metadata = JsonSerializer.Deserialize<FlowerMetadata>(flower.Metadata, JsonOptions)
                ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw new ConflictException(
                "GENERATED_FLOWER_METADATA_INVALID",
                "GeneratedFlower có snapshot cấu tạo không hợp lệ.");
        }

        return new Response.GeneratedFlowerCompositionResponse
        {
            Id = flower.Id,
            Name = flower.Name,
            CoverImage = flower.ImageUrl,
            SubId = metadata.SubId,
            Materials = metadata.Materials.Select(ToCompositionMaterial).ToList()
        };
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
            .Where(x => x.Key == "imagegen.flower_prompt" && !x.IsDeleted)
            .ToListAsync();
        if (configs.Count == 0) return null;
        if (configs.Count != 1
            || !string.Equals(configs[0].Group?.Trim(), "imagegen", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(configs[0].Kind?.Trim(), "setting", StringComparison.OrdinalIgnoreCase))
            //|| !configs[0].IsPublic)
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

            // ValidatePromptDescription(root);
            var prompt = content.GetString();
            return string.IsNullOrWhiteSpace(prompt) ? null : new PromptSnapshot { Content = prompt };
        }
        catch (JsonException)
        {
            throw new ConflictException("SYSTEM_PROMPT_INVALID", "System prompt flower Config không hợp lệ.");
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
        MatId = product.Id, Quantity = quantity, Name = product.ProductName, IsCore = isCore, CoverImage = product.CoverImage
    };

    private static Response.GeneratedFlowerMaterialResponse ToCompositionMaterial(MaterialSnapshot material) => new()
    {
        Id = material.MatId,
        Name = material.Name ?? string.Empty,
        CoverImage = material.CoverImage
    };

    private static Response.GeneratedFlowerResponse ToResponse(GeneratedFlower flower, Guid sessionId) => new()
    {
        Id = flower.Id, SessionId = sessionId, Name = flower.Name, ImageUrl = flower.ImageUrl,
        Metadata = JsonDocument.Parse(flower.Metadata).RootElement.Clone(), CreatedAt = flower.CreatedAt
    };

    private static void ValidateAdminListRequest(Request.AdminGeneratedFlowerListRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.productId == Guid.Empty)
        {
            errors["productId"] = ["ProductId không được là GUID rỗng."];
        }

        // Pagination được chuẩn hóa tại GetAdminGeneratedFlowers.
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
        if (errors.Count > 0)
        {
            throw new QueryValidationException(string.Join(" ", errors.SelectMany(error => error.Value)));
        }
    }

    private static void ValidateCustomerListRequest(Request.CustomerGeneratedFlowerListRequest request)
    {
        var errors = new List<string>();
        if (request.sessionId == Guid.Empty)
            errors.Add("SessionId không được là GUID rỗng.");
        // Pagination được chuẩn hóa tại GetCustomerGeneratedFlowers.
#if false
        if (request.pageIndex < 1)
            errors.Add("PageIndex phải lớn hơn hoặc bằng 1.");
        // if (request.pageSize is not (5 or 10 or 20 or 30 or 40 or 50))
        //     errors.Add("PageSize chỉ nhận 5, 10, 20, 30, 40 hoặc 50.");
#endif
        if (errors.Count > 0)
            throw new QueryValidationException(string.Join(" ", errors));
    }

    private static Response.AdminGeneratedFlowerResponse ToAdminResponse(
        GeneratedFlower flower,
        ClientHistory? history,
        JsonElement metadata) => new()
    {
        Id = flower.Id,
        Name = flower.Name,
        ImageUrl = flower.ImageUrl,
        UserId = flower.UserId,
        CreatedAt = flower.CreatedAt,
        Metadata = metadata,
        ClientHistory = history is null
            ? null
            : new Response.ClientHistoryResponse
            {
                Id = history.Id,
                UserId = history.UserId,
                Type = "flower",
                SystemForm = ParseNullableJson(history.SystemForm),
                Metadata = ParseJson(history.Metadata),
                RootId = history.RootId,
                CreatedAt = history.CreatedAt,
                BaseId = history.BaseId,
                OutputId = history.OutputId,
                SesionId = history.SessionId
            }
    };

    private static JsonElement ParseJson(string value)
    {
        using var document = JsonDocument.Parse(value);
        return document.RootElement.Clone();
    }

    private static IEnumerable<Guid> GetMaterialIds(string metadata)
    {
        using var document = JsonDocument.Parse(metadata);
        if (!document.RootElement.TryGetProperty("materials", out var materials)
            || materials.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var material in materials.EnumerateArray())
            if (material.TryGetProperty("matId", out var matId)
                && Guid.TryParse(matId.GetString(), out var id))
                yield return id;
    }

    private static JsonElement EnrichMaterialCoverImages(
        string metadata,
        IReadOnlyDictionary<Guid, string?> materialCoverImages)
    {
        var root = JsonNode.Parse(metadata)?.AsObject() ?? throw new JsonException();
        if (root["materials"] is JsonArray materials)
            foreach (var material in materials.OfType<JsonObject>())
                if (Guid.TryParse(material["matId"]?.GetValue<string>(), out var matId)
                    && materialCoverImages.TryGetValue(matId, out var coverImage))
                    material["coverImage"] = coverImage;
        return JsonSerializer.SerializeToElement(root, JsonOptions);
    }

    private static JsonElement? ParseNullableJson(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : ParseJson(value);

    private static Response.CustomerFlowerReferenceResponse? ToCustomerReference(ProductSnapshot? snapshot)
    {
        if (snapshot == null)
            return null;

        return new Response.CustomerFlowerReferenceResponse
        {
            Id = snapshot.Id,
            Name = snapshot.Name,
            ImageUrl = snapshot.Image.Url
        };
    }

    private static Response.CustomerFlowerReferenceResponse? ToCustomerReference(ReferenceSnapshot? snapshot)
    {
        if (snapshot == null)
            return null;

        return new Response.CustomerFlowerReferenceResponse
        {
            Id = snapshot.Id,
            Name = snapshot.Name,
            ImageUrl = snapshot.Image.Url
        };
    }

    private static bool TryGetProductId(string metadata, out Guid productId)
    {
        productId = Guid.Empty;
        try
        {
            var parsed = ParseJson(metadata);
            return parsed.ValueKind == JsonValueKind.Object
                && parsed.TryGetProperty("subId", out var subId)
                && subId.ValueKind == JsonValueKind.String
                && Guid.TryParse(subId.GetString(), out productId)
                && productId != Guid.Empty;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryParseFlowerHistory(
        string metadata,
        out string operation,
        out Guid? regeneratedFromFlowerId)
    {
        operation = string.Empty;
        regeneratedFromFlowerId = null;
        try
        {
            var parsed = ParseJson(metadata);
            if (parsed.ValueKind != JsonValueKind.Object
                || !parsed.TryGetProperty("operation", out var operationElement)
                || operationElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            operation = operationElement.GetString() ?? string.Empty;
            if (operation is not ("create" or "regenerate")) return false;
            if (!parsed.TryGetProperty("regeneratedFromFlowerId", out var parent)
                || parent.ValueKind == JsonValueKind.Null)
            {
                return operation == "create";
            }

            return operation == "regenerate"
                && parent.ValueKind == JsonValueKind.String
                && Guid.TryParse(parent.GetString(), out var parentId)
                && parentId != Guid.Empty
                && (regeneratedFromFlowerId = parentId).HasValue;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed class QueryValidationException(string detail)
        : AppException("Bad Request", 400, "VALIDATION_ERROR", detail);

    private Task<Dictionary<Guid, (int? SellableQuantity, string? StockStatus, string? UnavailableReason)>> CalculateAvailableQty(Guid productId) =>
        (_products as ProductServiceImplementation)?.CalculateAvailableQty([productId])
        ?? throw new FlowerException(503, "PRODUCT_AVAILABILITY_UNAVAILABLE", "Không xác minh được khả năng bán của Product.");

    private sealed class FlowerException(int statusCode, string messageCode, string detail)
        : AppException("Flower", statusCode, messageCode, detail);

    /*
         * config systemPrompt Flower
    {
      "group": "imagegen",
      "label": "Prompt ảnh hoa",
      "value": "Tạo ảnh hoa thật đẹp\n\n10 điểm",
      "options": [],
      "required": false,
      "inputType": "textarea",
      "validations": []
    }
         */
}

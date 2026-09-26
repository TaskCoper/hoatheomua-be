using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Ai = HoaTheoMua.Service.AiService;
using FlowerReferenceEntity = HoaTheoMua.Repository.Entity.FlowerReference;
using static HoaTheoMua.Service.Flower.MappingFlower;

namespace HoaTheoMua.Service.Flower;

public partial class Service
{
    private const string CreateFlowerPromptKey = "imagegen.flower_prompt_create_flowers";
    private const string ApplyMockupPromptKey = "imagegen.flower_prompt_flowers_apply_mockup";

    private const string GuardRulesPromptKey = "imagegen.flower_prompt_guard_rules";

    private async Task<PromptSnapshot?> LoadFlowerPrompts()
    {
        var configs = await _db.Configs.AsNoTracking()
            .Where(x => !x.IsDeleted && (x.Key == CreateFlowerPromptKey || x.Key == ApplyMockupPromptKey || x.Key == GuardRulesPromptKey))
            .ToListAsync(_httpContext.HttpContext?.RequestAborted ?? default);
        // Regeneration may fall back only to a complete prompt set from one historical generation.
        if (configs.Count == 0) return null;
        if (configs.Count != 3 || configs.Select(x => x.Key).Distinct().Count() != 3
            || configs.Any(x => !string.Equals(x.Group?.Trim(), "imagegen", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(x.Kind?.Trim(), "setting", StringComparison.OrdinalIgnoreCase)))
            throw InvalidFlowerPrompts();
        try
        {
            string Read(string key)
            {
                using var value = JsonDocument.Parse(configs.Single(x => x.Key == key).Value);
                if (value.RootElement.ValueKind != JsonValueKind.Object
                    || !value.RootElement.TryGetProperty("value", out var content)
                    || content.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(content.GetString()))
                    throw InvalidFlowerPrompts();
                return content.GetString()!;
            }
            return new PromptSnapshot
            {
                Content = Read(CreateFlowerPromptKey), MockupContent = Read(ApplyMockupPromptKey),
                GuardContent = Read(GuardRulesPromptKey),
                GuardConfigId = configs.Single(x => x.Key == GuardRulesPromptKey).Id,
                CreateConfigId = configs.Single(x => x.Key == CreateFlowerPromptKey).Id,
                MockupConfigId = configs.Single(x => x.Key == ApplyMockupPromptKey).Id
            };
        }
        catch (JsonException) { throw InvalidFlowerPrompts(); }
    }

    private static ConflictException InvalidFlowerPrompts() => new("SYSTEM_PROMPT_INVALID",
        "Cần ba Config imagegen hợp lệ: flower_prompt_create_flowers, flower_prompt_flowers_apply_mockup và flower_prompt_guard_rules, Value dạng {\"value\":\"prompt\"}.");

    public async Task<Ai.Request.FlowerImageInput> ToGenerationInputAsync(
        PreparedFlower prepared,
        CancellationToken cancellationToken)
    {
        var (product, bundle, mockup, prompts, metadata) = (prepared.Product, prepared.Bundle, prepared.Mockup, prepared.Prompt, prepared.Metadata);
        if (metadata.Materials.Count == 0 || metadata.Ribbon == null || metadata.WrappingPaper == null
            || metadata.Materials.Select(x => x.MatId).Distinct().Count() != metadata.Materials.Count
            || metadata.AdditionCustome == null
            || metadata.AdditionCustome.Select(x => x.MatId).Distinct().Count() != metadata.AdditionCustome.Count
            || metadata.AdditionCustome.Any(x => x.Quantity < 0 || !metadata.Materials.Any(m => m.MatId == x.MatId)))
            throw new ConflictException("FLOWER_SOURCE_HISTORY_INVALID", "Công thức Flower không hợp lệ.");

        var additions = metadata.AdditionCustome.ToDictionary(x => x.MatId, x => x.Quantity);
        var materials = metadata.Materials.Select(x => new Ai.Request.FlowerMaterial(
            x.MatId, x.Name, x.CoverImage ?? "", x.Quantity, additions.GetValueOrDefault(x.MatId),
            x.Quantity + additions.GetValueOrDefault(x.MatId), x.Unit, x.Color, x.Description)).ToList();
        var comboImageUrl = await GetComboReferenceImageAsync(
            product.Id,
            product.Image.Url,
            cancellationToken);

        var input = new Ai.Request.FlowerImageInput(prompts.Content, prompts.MockupContent!, prompts.GuardContent!,
            ToAiProduct(product, comboImageUrl), materials, ToAiReference(bundle), ToAiProduct(metadata.WrappingPaper),
            ToAiProduct(metadata.Ribbon), ToAiReference(mockup));
        return input;
    }

    private async Task<string?> GetComboReferenceImageAsync(
        Guid productId,
        string? fallbackImageUrl,
        CancellationToken cancellationToken)
    {
        var parentProductId = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == productId && !x.IsDeleted && x.IsActive)
            .Select(x => x.ParentProductId)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(parentProductId) ||
            !long.TryParse(parentProductId, out var parentNhanhProductId) ||
            parentNhanhProductId <= 0)
            return fallbackImageUrl;

        var parentImageUrl = await _db.Products
            .AsNoTracking()
            .Where(x => x.NhanhProductId == parentProductId && !x.IsDeleted && x.IsActive)
            .Select(x => x.CoverImage)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(parentImageUrl) ? fallbackImageUrl : parentImageUrl;
    }

    private static Ai.Request.FlowerProduct ToAiProduct(ProductSnapshot product, string? imageUrl = null) => new(
        product.Id, product.Name, imageUrl ?? product.Image.Url ?? "", product.Color ?? ReadLegacyAttribute(product.Attributes, "color"),
        product.Description, product.Size ?? ReadLegacyAttribute(product.Attributes, "size"), product.RelatedImageUrls);

    private static string? ReadLegacyAttribute(object? attributes, string key)
    {
        var json = JsonSerializer.SerializeToElement(attributes, JsonOptions);
        return json.ValueKind == JsonValueKind.Object && json.TryGetProperty(key, out var value)
            && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static Ai.Request.FlowerReference ToAiReference(FlowerReferenceEntity reference) => new(
        reference.Id, reference.Name, reference.ImageUrl, reference.Description, reference.DescriptionPrompt);

    private static void SetGenerationMetadata(FlowerMetadata metadata, Ai.Response.FlowerGeneratedResponse result, BrandedFlowerImage branded)
    {
        metadata.FlowerImageUrl = result.FlowerImageUrl;
        metadata.FlowerWithMockupImageUrl = result.FlowerWithMockupImageUrl;
        metadata.FlowerWithLogoImageUrl = branded.ImageUrl;
        metadata.Branding = branded;
        metadata.FlowerGeneration = result.FlowerGeneration;
        metadata.MockupGeneration = result.MockupGeneration;
    }
}

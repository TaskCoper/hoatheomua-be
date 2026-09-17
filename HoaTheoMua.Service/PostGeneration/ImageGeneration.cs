using System.Text.Json;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.PostGen;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.PostGeneration;

public partial class Service
{
    private sealed record ImagePair(Guid SourceId, AspectRatioType Ratio);
    private sealed record ImageSelection(PlatformType Platform, ImagePair Pair, string SourceUrl, string Instruction);

    private async Task<List<ImageSelection>> ReadImagePlan(Guid id, Request.GenerateRequest request, CancellationToken ct)
    {
        var sources = await db.BasePostImages.AsNoTracking().Where(x => x.BasePostId == id && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ToListAsync(ct);
        var result = new List<ImageSelection>();
        var instruction = "";
        if (request.Platforms!.Any(x => x.UsePreset || x.Ratios != null))
            instruction = await ReadImageInstruction(ct);
        string? preset = null;
        if (request.Platforms!.Any(x => x.UsePreset))
        {
            var rows = await db.Configs.AsNoTracking().Where(x => x.Group == "postgen" && x.Key == "postgen.ratio-preset"
                && x.Kind != null && x.Kind.Trim().ToLower() == "setting" && !x.IsPublic && !x.IsDeleted).ToListAsync(ct);
            if (rows.Count != 1) throw new BadRequestException("Ratio preset is not configured");
            preset = rows[0].Value;
        }
        foreach (var platform in request.Platforms!)
        {
            var ratios = platform.UsePreset ? ExpandPreset(preset!, platform.Platform!.Value, sources) : platform.Ratios ?? new();
            foreach (var ratio in ratios)
            {
                var imageIds = ratio.BasePostImageIds is { Count: > 0 } ? ratio.BasePostImageIds : sources.Select(x => x.Id).ToList();
                if (imageIds.Count == 0) throw new BadRequestException("No source images are available");
                foreach (var imageId in imageIds)
                {
                    var source = sources.SingleOrDefault(x => x.Id == imageId)
                        ?? throw new BadRequestException("Image does not belong to this post or has been deleted");
                    result.Add(new(platform.Platform!.Value, new(imageId, ratio.RatioCode!.Value), source.ImageUrl, instruction));
                }
            }
        }
        if (result.Select(x => x.Pair).Distinct().Count() > 12)
            throw new ValidationExceptionError("A generation request cannot exceed 12 image pairs");
        return result;
    }

    private async Task<string> ReadImageInstruction(CancellationToken ct)
    {
        var rows = await db.Configs.AsNoTracking().Where(x => x.Group == "postgen" && x.Key == "postgen.image_instruction"
            && x.Kind != null && x.Kind.Trim().ToLower() == "setting" && !x.IsDeleted && !x.IsPublic).ToListAsync(ct);
        try
        {
            if (rows.Count != 1) throw new JsonException();
            using var json = JsonDocument.Parse(rows[0].Value);
            var instruction = ConfigValues.Unwrap(json.RootElement).GetString()!;
            if (string.IsNullOrWhiteSpace(instruction)) throw new JsonException();
            return instruction;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException)
        { throw new BadRequestException("Image instruction is not configured"); }
    }

    private static List<Request.RatioInput> ExpandPreset(string value, PlatformType platform, List<BasePostImage> sources)
    {
        try
        {
            using var json = JsonDocument.Parse(value);
            var config = json.RootElement.GetProperty(platform.ToString().ToLowerInvariant());
            var ratios = config.GetProperty("ratios");
            if (ratios.GetArrayLength() is < 1 or > 5) throw new JsonException();
            var mode = config.GetProperty("imageSelectionMode").GetString();
            var images = sources;
            if (mode == "first_n")
            {
                var count = config.GetProperty("imageCount").GetInt32();
                if (count <= 0) throw new JsonException();
                images = sources.Take(count).ToList();
            }
            else if (mode != "all_images" || config.TryGetProperty("imageCount", out _)) throw new JsonException();
            if (images.Count == 0) throw new JsonException();
            var result = new List<Request.RatioInput>();
            foreach (var item in ratios.EnumerateArray())
            {
                var name = item.GetString();
                if (!Enum.TryParse<AspectRatioType>(name, out var ratio) || !Enum.IsDefined(ratio) || ratio.ToString() != name)
                    throw new JsonException();
                result.Add(new() { RatioCode = ratio, BasePostImageIds = images.Select(x => x.Id).ToList() });
            }
            if (result.Select(x => x.RatioCode).Distinct().Count() != result.Count) throw new JsonException();
            return result;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        { throw new BadRequestException("Ratio preset is invalid for the requested platform"); }
    }

    private async Task<List<GeneratedPostImage>> GenerateImages(Guid postId, List<ImageSelection> plan, CancellationToken budget, CancellationToken caller)
    {
        using var semaphore = new SemaphoreSlim(3);
        var tasks = plan.DistinctBy(x => x.Pair).Select(async selection =>
        {
            var (width, height) = AspectRatios.Pixels(selection.Pair.Ratio);
            var image = new GeneratedPostImage
            {
                Id = Guid.NewGuid(), BasePostId = postId, SourceBasePostImageId = selection.Pair.SourceId,
                RatioCode = selection.Pair.Ratio, SourceImageUrl = selection.SourceUrl,
                PixelWidth = width, PixelHeight = height, CreatedAt = DateTimeOffset.UtcNow
            };
            var acquired = false;
            try
            {
                await semaphore.WaitAsync(budget); acquired = true;
                var output = await ai.GenerateImage(new(selection.SourceUrl, selection.Pair.Ratio.ToString(), width, height, selection.Instruction), budget);
                image.Status = ImageStatus.Succeeded; image.ImageUrl = output.ImageUrl;
                image.AiModel = output.Model; image.AiRequestId = output.RequestId; image.DurationMs = output.DurationMs;
            }
            catch (OperationCanceledException) when (!caller.IsCancellationRequested)
            { image.Status = ImageStatus.Failed; image.ErrorMessage = "Image generation budget exceeded"; }
            catch (AppException ex) when (ex is BadRequestException or BadGatewayException)
            { image.Status = ImageStatus.Failed; image.ErrorMessage = "AI image generation failed"; }
            finally { if (acquired) semaphore.Release(); }
            return image;
        });
        return (await Task.WhenAll(tasks)).ToList(); // No concurrent access to DbContext.
    }

    private async Task SelectImages(List<PlatformPost> platforms, List<ImageSelection> plan, List<GeneratedPostImage> images, CancellationToken ct)
    {
        var ids = platforms.Select(x => x.Id).ToArray();
        var slots = await db.PlatformPostImages.Where(x => ids.Contains(x.PlatformPostId)).ToListAsync(ct);
        var generated = images.ToDictionary(x => new ImagePair(x.SourceBasePostImageId, x.RatioCode));
        foreach (var platform in platforms)
        {
            var platformSlots = slots.Where(x => x.PlatformPostId == platform.Id).ToList();
            var nextOrder = platformSlots.Select(x => x.SortOrder).DefaultIfEmpty().Max();
            foreach (var selection in plan.Where(x => x.Platform == platform.Platform))
            {
                var slot = platformSlots.SingleOrDefault(x => x.SourceBasePostImageId == selection.Pair.SourceId && x.RatioCode == selection.Pair.Ratio);
                if (slot == null)
                {
                    slot = new PlatformPostImage { Id = Guid.NewGuid(), PlatformPostId = platform.Id,
                        SourceBasePostImageId = selection.Pair.SourceId, RatioCode = selection.Pair.Ratio,
                        SortOrder = ++nextOrder, CreatedAt = DateTimeOffset.UtcNow };
                    platformSlots.Add(slot); db.PlatformPostImages.Add(slot);
                }
                var image = generated[selection.Pair];
                if (slot.SelectedGeneratedPostImageId == null && image.Status == ImageStatus.Succeeded)
                { slot.SelectedGeneratedPostImageId = image.Id; slot.UpdatedAt = DateTimeOffset.UtcNow; }
            }
        }
    }
}

using System.Text.Json;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PostGen;
using HoaTheoMua.Service.PostGeneration;
using Microsoft.EntityFrameworkCore;
using static HoaTheoMua.Service.PostPublishing.Store;

namespace HoaTheoMua.Service.PostImport;

public sealed partial class Worker
{
    private sealed record Preparation(string? Active = null, string[]? Completed = null, Guid[]? OwnedTargets = null, Dictionary<string, string>? Fingerprints = null);

    // Persist intent before every paid call. Recovery never repeats an ambiguous AI request.
    private async Task PrepareAi(Guid batchId, Guid rowId, Guid actor, Guid lease, Guid postId, CancellationToken ct)
    {
        var row = await db.PostImportRows.SingleAsync(x => x.Id == rowId, ct);
        var input = Read<ImportInput>(row.Payload);
        if (!input.UseAiContent && input.Images.All(x => !x.UseAi)) return;
        var state = Read<Preparation>(row.Preparation);
        if (state.Active != null) throw AiError("IMPORT_AI_INTERRUPTED", "Lượt AI trước bị gián đoạn. Kiểm tra bài trong hệ thống; hệ thống không tự gọi lại để tránh phát sinh chi phí trùng.");
        if (ai == null) throw AiError("IMPORT_AI_UNAVAILABLE", "Dịch vụ AI chưa sẵn sàng.");
        var configs = await db.Configs.AsNoTracking().Where(x => x.Group == "postgen" && !x.IsDeleted).ToListAsync(ct);
        var enabled = configs.Where(x => x.Key == "postgen.enabled").ToArray();
        if (enabled.Length > 1) throw AiError("IMPORT_AI_DISABLED", "Cấu hình AI không hợp lệ.");
        if (enabled.Length == 1)
        {
            using var enabledJson = JsonDocument.Parse(enabled[0].Value);
            if (!ConfigValues.Unwrap(enabledJson.RootElement).GetBoolean()) throw AiError("IMPORT_AI_DISABLED", "Chức năng AI chưa được bật.");
        }
        var sources = await db.BasePostImages.Where(x => x.BasePostId == postId && !x.IsDeleted).OrderBy(x => x.SortOrder).ToListAsync(ct);
        var completed = (state.Completed ?? []).ToHashSet();
        var claimedTargets = (state.OwnedTargets ?? []).ToHashSet();
        var fingerprints = state.Fingerprints ?? new Dictionary<string, string>();
        foreach (var platform in input.Platforms)
        {
            if (await Known(postId, platform, ct) != null) continue;
            var target = await db.PlatformPosts.SingleAsync(x => x.BasePostId == postId && x.Platform == platform && !x.IsDeleted, ct);
            if (input.UseAiContent)
            {
                var key = "content:" + platform;
                var current = await db.GeneratedPosts.SingleAsync(x => x.Id == target.OfficialGenerationId, ct);
                if (!completed.Contains(key) && current.Source != ContentSource.Ai)
                {
                    var promptConfig = configs.SingleOrDefault(x => x.Key == "postgen.system-prompt." + Service.Name(platform)
                        && !x.IsPublic && string.Equals(x.Kind?.Trim(), "Setting", StringComparison.OrdinalIgnoreCase))
                        ?? throw AiError("IMPORT_AI_CONFIG_INVALID", $"Chưa cấu hình AI nội dung cho {platform}.");
                    using var promptJson = JsonDocument.Parse(promptConfig.Value);
                    var prompt = ConfigValues.PlatformPrompt(promptJson.RootElement, true);
                    if (!prompt.Active) throw AiError("IMPORT_AI_CONFIG_INVALID", $"AI cho {platform} đang tắt.");
                    await Start(key, target);
                    AiService.Response.GeneratedContent output;
                    try
                    {
                        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
                        budget.CancelAfter(TimeSpan.FromSeconds(180));
                        output = await ai.GenerateContent(new(prompt.Text, Service.Name(platform), new(input.Title, input.Content, input.Hashtag)), budget.Token);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                    catch (Exception) { throw AiError("IMPORT_AI_FAILED", $"AI nội dung cho {platform} chưa hoàn tất. Bài không được tự đăng; mở bài để kiểm tra và thử lại."); }
                    var version = new GeneratedPost { Id = Guid.NewGuid(), PlatformPostId = target.Id, Source = ContentSource.Ai,
                        Title = output.Title, Content = output.Content, Hashtag = output.Hashtag, ParentGeneratedPostId = current.Id,
                        SystemPromptConfigId = promptConfig.Id, PromptSnapshot = prompt.Text, AiModel = output.Model,
                        AiRequestId = output.RequestId, DurationMs = output.DurationMs, UserId = actor, CreatedAt = clock.GetUtcNow() };
                    db.GeneratedPosts.Add(version);
                    await db.SaveChangesAsync(ct);
                    target.OfficialGenerationId = version.Id; target.SelectedAt = clock.GetUtcNow(); target.UpdatedAt = clock.GetUtcNow();
                    await Complete(key);
                }
            }
            foreach (var selection in input.Images.Select((item, index) => (item, index))
                .Where(x => x.item.UseAi && (x.item.Platform == null || x.item.Platform == platform)))
            {
                var item = selection.item;
                var key = $"image:{platform}:{selection.index}";
                if (completed.Contains(key)) continue;
                var source = sources[selection.index];
                var slot = await db.PlatformPostImages.SingleAsync(x => x.PlatformPostId == target.Id && x.SourceBasePostImageId == source.Id, ct);
                var current = await db.GeneratedPostImages.SingleAsync(x => x.Id == slot.SelectedGeneratedPostImageId, ct);
                if (current.StorageProvider == "ImportAi") continue;
                var config = configs.SingleOrDefault(x => x.Key == "postgen.image_instruction" && !x.IsPublic
                    && string.Equals(x.Kind?.Trim(), "Setting", StringComparison.OrdinalIgnoreCase))
                    ?? throw AiError("IMPORT_AI_CONFIG_INVALID", "Chưa cấu hình hướng dẫn AI hình ảnh.");
                using var json = JsonDocument.Parse(config.Value);
                var instruction = ConfigValues.Unwrap(json.RootElement).GetString();
                if (string.IsNullOrWhiteSpace(instruction)) throw AiError("IMPORT_AI_CONFIG_INVALID", "Hướng dẫn AI hình ảnh đang trống.");
                var ratio = item.Ratio!.Value;
                var (width, height) = AspectRatios.Pixels(ratio);
                await Start(key, target);
                AiService.Response.GeneratedImage output;
                try
                {
                    using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    budget.CancelAfter(TimeSpan.FromSeconds(180));
                    output = await ai.GenerateImage(new(source.ImageUrl, ratio.ToString(), width, height, instruction, Feedback: item.Instruction), budget.Token);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception) { throw AiError("IMPORT_AI_FAILED", $"AI ảnh thứ tự {item.Order} của {platform} chưa hoàn tất. Bài không được tự đăng; mở bài để kiểm tra và thử lại."); }
                var image = new GeneratedPostImage { Id = Guid.NewGuid(), BasePostId = postId, SourceBasePostImageId = source.Id,
                    SourceImageUrl = source.ImageUrl, ImageUrl = output.ImageUrl, RatioCode = ratio, ParentGeneratedPostImageId = current.Id,
                    PixelWidth = width, PixelHeight = height, StorageProvider = "ImportAi", Status = ImageStatus.Succeeded,
                    FeedbackInput = item.Instruction, AiModel = output.Model, AiRequestId = output.RequestId,
                    DurationMs = output.DurationMs, CreatedAt = clock.GetUtcNow() };
                db.GeneratedPostImages.Add(image); await db.SaveChangesAsync(ct);
                slot.SelectedGeneratedPostImageId = image.Id; slot.UpdatedAt = clock.GetUtcNow();
                await Complete(key);
            }
            fingerprints[Service.Name(platform)] = await ChannelFingerprint(postId, platform, ct);
            target.Status = PlatformPostStatus.Ready; target.LastError = null;
            claimedTargets.Remove(target.Id);
            row.Preparation = Json(new Preparation(null, completed.ToArray(), claimedTargets.ToArray(), fingerprints));
            await db.SaveChangesAsync(ct);
        }
        async Task Start(string key, PlatformPost target)
        {
            await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
            // Reload the row after LockBatch: detaching the batch also detaches tracked dependents.
            var batch = await LockBatch(batchId, ct);
            row = await db.PostImportRows.SingleAsync(x => x.Id == rowId, ct);
            if (batch?.LeaseId != lease || batch.LeaseUntil <= clock.GetUtcNow()) throw AiError("IMPORT_LEASE_CHANGED", "Tác vụ đang được xử lý ở tiến trình khác.");
            // Serialize with normal editor/generator claims on the same base post.
            if (db.Database.IsRelational())
                await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM \"BasePost\" WHERE \"Id\" = {postId} FOR UPDATE", ct);
            var stored = await db.PlatformPosts.AsNoTracking().SingleAsync(x => x.Id == target.Id, ct);
            if (!claimedTargets.Contains(target.Id) && stored.Status == PlatformPostStatus.Generating
                || stored.OfficialGenerationId != target.OfficialGenerationId || await Known(postId, target.Platform, ct) != null)
                throw AiError("IMPORT_CONTENT_CHANGED", "Bài đang được xử lý hoặc đã thay đổi. Kiểm tra bài trước khi chạy AI.");
            target.Status = PlatformPostStatus.Generating;
            claimedTargets.Add(target.Id);
            batch.LeaseUntil = clock.GetUtcNow().AddMinutes(4);
            row.Preparation = Json(new Preparation(key, completed.ToArray(), claimedTargets.ToArray(), fingerprints));
            await db.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        async Task Complete(string key)
        {
            if (!await HasLease(batchId, lease, ct)) throw AiError("IMPORT_LEASE_CHANGED", "Lượt xử lý đã hết hạn; kiểm tra kết quả trong bài.");
            completed.Add(key); row.Preparation = Json(new Preparation(null, completed.ToArray(), claimedTargets.ToArray(), fingerprints));
            await db.SaveChangesAsync(ct);
        }
    }
    private static ImportException AiError(string code, string message) => new(409, code, message);
    private async Task<string> ChannelFingerprint(Guid postId, PlatformType platform, CancellationToken ct)
    {
        var target = await db.PlatformPosts.AsNoTracking().SingleAsync(x => x.BasePostId == postId && x.Platform == platform && !x.IsDeleted, ct);
        var content = await db.GeneratedPosts.AsNoTracking().SingleAsync(x => x.Id == target.OfficialGenerationId, ct);
        var urls = await (from slot in db.PlatformPostImages.AsNoTracking()
            join image in db.GeneratedPostImages.AsNoTracking() on slot.SelectedGeneratedPostImageId equals image.Id
            where slot.PlatformPostId == target.Id orderby slot.SortOrder, slot.Id select image.ImageUrl!).ToListAsync(ct);
        return Fingerprint(content.Title ?? "", content.Content, content.Hashtag, urls);
    }
}

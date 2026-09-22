using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.Card;

internal static class CardPromptConfig
{
    public const string Key = "imagegen.card_prompt";

    public static async Task<MappingCard.SnapshotPrompt?> LoadAsync(AppDbContext db, CancellationToken ct = default)
    {
        var configs = await db.Configs.AsNoTracking().Where(x => x.Key == Key && !x.IsDeleted).ToListAsync(ct);
        if (configs.Count == 0) return null;
        if (configs.Count != 1
            || !string.Equals(configs[0].Group?.Trim(), "imagegen", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(configs[0].Kind?.Trim(), "setting", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(configs[0].Value))
            throw Invalid();
        try
        {
            using var json = JsonDocument.Parse(configs[0].Value);
            if (json.RootElement.ValueKind != JsonValueKind.Object
                || !json.RootElement.TryGetProperty("value", out var value)
                || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
                throw Invalid();
            return new() { Type = "card", Content = value.GetString()! };
        }
        catch (JsonException) { throw Invalid(); }
    }

    private static ConflictException Invalid() => new("SYSTEM_PROMPT_INVALID", "Config imagegen.card_prompt không hợp lệ.");
}

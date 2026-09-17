using System.Text.Json;

namespace HoaTheoMua.Service.PostGeneration;

// Config.Value may be a form descriptor. Only its value is input to AI, never the UI metadata.
internal static class ConfigValues
{
    public static JsonElement Unwrap(JsonElement root) => root.ValueKind == JsonValueKind.Object
        ? root.GetProperty("value") : root;

    public static (bool Active, string Text) PlatformPrompt(JsonElement root, bool requireText)
    {
        if (root.ValueKind != JsonValueKind.Object) throw new JsonException();
        var form = root.TryGetProperty("value", out var value);
        // Legacy objects require isActive. A new form without an activation flag defaults to enabled.
        var active = root.TryGetProperty("isActive", out var flag) ? flag.GetBoolean()
            : form ? FormActive(root) : throw new JsonException();
        if (!active) return (false, "");
        if (!form && !requireText) return (true, "");
        var text = (form ? value : root.GetProperty("content")).GetString();
        if (text == null || (requireText && string.IsNullOrWhiteSpace(text))) throw new JsonException();
        return (true, requireText ? text : "");
    }

    private static bool FormActive(JsonElement root)
    {
        // Existing platform forms store optional metadata as a JSON string in description.
        if (!root.TryGetProperty("description", out var description) || description.ValueKind == JsonValueKind.Null)
            return true;
        if (description.ValueKind != JsonValueKind.String) throw new JsonException();
        var text = description.GetString()!;
        if (!text.TrimStart().StartsWith('{')) return true;
        using var metadata = JsonDocument.Parse(text);
        return !metadata.RootElement.TryGetProperty("isActive", out var flag) || flag.GetBoolean();
    }
}

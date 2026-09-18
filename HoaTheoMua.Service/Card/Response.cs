using System.Text.Json;

namespace HoaTheoMua.Service.Card;

public class Response
{
    public class CreateCardResponse
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? RawImage { get; set; }
        public string CardType { get; set; } = null!;
        public string FormType { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public string? AttachedImageUrl { get; set; }
        public string SizeName { get; set; } = null!;
        public decimal SizeWidth { get; set; }
        public decimal SizeHeight { get; set; }
        public decimal SizeBasePrice { get; set; }
        public int SizeMaxWords { get; set; }
        public int WordCount { get; set; }
        public WordConfigSnapshot? WordConfigSnapshot { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ExtraPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
    
    public class WordConfigSnapshot
    {
        [System.Text.Json.Serialization.JsonPropertyName("min_words")]
        public int MinWords { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("extra_price")]
        public decimal ExtraPrice { get; set; }
    }
}

public class MappingCard
{
    public sealed class CardSnapshot
    {
        public Guid VirtualId { get; set; }
        public string CardType { get; set; } = "";
        public SnapshotSource Source { get; set; } = new();
        public string? RawImage { get; set; }
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
        public string Status { get; set; } = "";
        public List<Guid> IdResult { get; set; } = new();
        public Guid? FinalGeneratedCardId { get; set; }
    }

    public sealed class SnapshotSource
    {
        public string Type { get; set; } = "";
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    public sealed class SnapshotTemplate
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = "";
        public string TemplateType { get; set; } = "";
        public string TemplateImageUrl { get; set; } = "";
    }

    public sealed class SnapshotSize
    {
        public string Name { get; set; } = "";
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("base_price")]
        public decimal BasePrice { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
    }

    public sealed class SnapshotPrompt
    {
        public string Type { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public sealed class SourceHistoryMetadata
    {
        public Guid? OrderDetailId { get; set; }
        public Guid? VirtualId { get; set; }
        public string SourceType { get; set; } = "";
        public Guid? GeneratedFlowerId { get; set; }
        public SourceHistorySource? Source { get; set; }
        public SourceHistoryTemplate? CardTemplate { get; set; }
    }

    public sealed class SourceHistorySource
    {
        public string Type { get; set; } = "";
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    public sealed class SourceHistoryTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string TemplateType { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }

    public sealed class ConfigItems<T>
    {
        public List<T> Items { get; set; } = new();
    }

    public sealed class SizeConfigItem
    {
        public string Name { get; set; } = "";
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("base_price")]
        public decimal BasePrice { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
    }

    public sealed class PriceConfigItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("min_words")]
        public int MinWords { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("extra_price")]
        public decimal ExtraPrice { get; set; }
    }
}
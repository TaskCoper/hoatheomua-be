using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Flower;

public class Response
{
    public class CustomerFlowerSessionResponse
    {
        public Guid SessionId { get; set; }
        public DateTimeOffset LatestCreatedAt { get; set; }
        public List<CustomerGeneratedFlowerResponse> Flowers { get; set; } = [];
    }

    public class CustomerGeneratedFlowerResponse
    {
        public Guid HistoryId { get; set; }
        public string Operation { get; set; } = null!;
        public Guid? RegeneratedFromFlowerId { get; set; }
        public Guid ProductId { get; set; }
        public Guid RootProductId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        // Snapshot cấu hình đúng tại lúc tạo version Flower này.
        public CustomerFlowerReferenceResponse? Product { get; set; }
        public CustomerFlowerReferenceResponse? Bundle { get; set; }
        public CustomerFlowerReferenceResponse? Mockup { get; set; }
        public CustomerFlowerReferenceResponse? Ribbon { get; set; }
        public CustomerFlowerReferenceResponse? WrappingPaper { get; set; }
        public JsonElement Metadata { get; set; }
    }

    public class CustomerFlowerReferenceResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class AdminGeneratedFlowerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public JsonElement Metadata { get; set; }
        public ClientHistoryResponse? ClientHistory { get; set; }
    }

    public class ClientHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = null!;
        public JsonElement? SystemForm { get; set; }
        public JsonElement Metadata { get; set; }
        public Guid RootId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid BaseId { get; set; }
        public Guid OutputId { get; set; }
        public Guid? SesionId { get; set; }
    }

    public class GeneratedFlowerResponse
    {
        public Guid Id { get; init; }
        public Guid SessionId { get; init; } // phải có dòng này
        public string Name { get; init; } = null!;
        public string ImageUrl { get; init; } = null!;
        public JsonElement Metadata { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    public class GeneratedFlowerCompositionResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string CoverImage { get; set; } = "";
        public Guid SubId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<GeneratedFlowerMaterialResponse> Materials { get; set; } = [];
    }

    public class GeneratedFlowerImageResponse
    {
        public Guid GeneratedFlowerId { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    public class GeneratedFlowerMaterialResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? CoverImage { get; set; }
        public string? Unit { get; set; }
    }

    public class GetGeneratedFlowersByMockupIdResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}

public class MappingFlower
{
    public class FlowerMetadata
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

    public class HistoryMetadata
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

    public class ProductSnapshot
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

    public class ReferenceSnapshot
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

    public class ImageSnapshot
    {
        [JsonPropertyName("url")] 
        public string? Url { get; set; }
    }

    public class MaterialSnapshot
    {
        [JsonPropertyName("matId")] 
        public Guid MatId { get; set; }
        [JsonPropertyName("quantity")] 
        public decimal Quantity { get; set; }
        [JsonPropertyName("name")] 
        public string? Name { get; set; }
        [JsonPropertyName("isCore")] 
        public bool IsCore { get; set; }
        [JsonPropertyName("coverImage")]
        public string? CoverImage { get; set; }
    }

    public class PromptSnapshot
    {
        [JsonPropertyName("type")] 
        public string Type { get; set; } = "flower";
        [JsonPropertyName("content")] 
        public string Content { get; set; } = null!;
    }
    
    private sealed class CardSnapshot
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

    private sealed class SnapshotSource
    {
        public string Type { get; set; } = "";
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    private sealed class SnapshotTemplate
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = "";
        public string TemplateType { get; set; } = "";
        public string TemplateImageUrl { get; set; } = "";
    }

    private sealed class SnapshotSize
    {
        public string Name { get; set; } = "";
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("base_price")]
        public decimal BasePrice { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
    }

    private sealed class SnapshotPrompt
    {
        public string Type { get; set; } = "";
        public string Content { get; set; } = "";
    }

    private sealed class SourceHistoryMetadata
    {
        public Guid? OrderDetailId { get; set; }
        public Guid? VirtualId { get; set; }
        public string SourceType { get; set; } = "";
        public Guid? GeneratedFlowerId { get; set; }
        public SourceHistorySource? Source { get; set; }
        public SourceHistoryTemplate? CardTemplate { get; set; }
    }

    private sealed class SourceHistorySource
    {
        public string Type { get; set; } = "";
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = "";
    }

    private sealed class SourceHistoryTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string TemplateType { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }
}

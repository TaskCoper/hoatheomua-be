using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.AiService;

public class Request
{
    public record CardImageInput(string SystemPrompt, string BouquetImageUrl, string TemplateImageUrl,
        string MessageContent, string FormType, decimal Width, decimal Height,
        string? AttachedImageUrl = null, string? RawImage = null);
    public record FlowerImageInput(string CreateSystemPrompt, string MockupSystemPrompt, string GuardSystemPrompt,
        FlowerProduct Product, IReadOnlyList<FlowerMaterial> Materials,
        FlowerReference Bundle, FlowerProduct WrappingPaper, FlowerProduct Ribbon,
        FlowerReference Mockup);
    public record FlowerProduct(Guid Id, string? Name, string ImageUrl, string? Color = null,
        string? Description = null, string? Size = null, IReadOnlyList<string>? RelatedImageUrls = null);
    // Quantity is explicit: FinalQuantity = BaseQuantity + AdditionalQuantity, never added twice.
    public record FlowerMaterial(Guid Id, string? Name, string ImageUrl, decimal BaseQuantity,
        decimal AdditionalQuantity, decimal FinalQuantity, string? Unit = null,
        string? Color = null, string? Description = null);
    public record FlowerReference(Guid Id, string? Name, string ImageUrl,
        string? Description = null, string? DescriptionPrompt = null,
        IReadOnlyList<string>? RelatedImageUrls = null);
    public record ImageInput(string SourceImageUrl, string RatioCode, int PixelWidth, int PixelHeight, string Instruction,
        string? OriginalImageUrl = null, string? Feedback = null);
    public record ContentInput(string SystemPrompt, string Platform, SourceText Source,
        SourceText? PreviousContent = null, string? Feedback = null);
    public record SourceText(string? Title, string Content, string? Hashtag);
    public record ChatCompletion(string Model, IReadOnlyList<Message> Messages);
    public record Message(string Role, IReadOnlyList<ContentPart> Content);
    // One ordered collection preserves text/image interleaving when image generation is added.
    public record ContentPart(string Type,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text = null,
        [property: JsonPropertyName("image_url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ImageUrl? ImageUrl = null);
    public record ImageUrl(string Url);
}

using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.AiService;

public class Request
{
    public record ImageInput(string SourceImageUrl, string RatioCode, int PixelWidth, int PixelHeight, string Instruction);
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

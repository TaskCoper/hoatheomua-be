using System.Text.Json;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Card;

namespace HoaTheoMua.Service.CardJobs;

public record CardGenerationRequested(Guid JobId);
public record Checkpoint(AiService.Response.GeneratedImage? Card = null, Flower.BrandedFlowerImage? Branding = null);
public record JobResponse(Guid JobId, string Type, string Status, string Stage, string DisplayName,
    Guid OrderDetailId, Guid VirtualId, string? BouquetImageUrl, string? TemplateImageUrl, Guid? SourceCardId,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, DateTimeOffset? CompletedAt,
    bool CancelRequested, string? ErrorCode, Response.CreateCardResponse? Result);

public class CardJobOptions
{
    public int MaxActivePerUser { get; set; } = 3;
    public int Concurrency { get; set; } = 2;
    public int BudgetSeconds { get; set; } = 240;
    public int LeaseSeconds { get; set; } = 60;
    public int RecoveryIntervalSeconds { get; set; } = 15;
}

public static class JobJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T Read<T>(string json) => JsonSerializer.Deserialize<T>(json, Options) ?? throw new JsonException();
    public static bool Active(CardGenerationJob job) => job.Status is "Queued" or "Running";
    public static JobResponse Response(CardGenerationJob j) => new(j.Id, j.Type, j.Status, j.Stage,
        j.DisplayName, j.OrderDetailId, j.VirtualId, j.BouquetImageUrl, j.TemplateImageUrl, j.SourceCardId,
        j.CreatedAt, j.UpdatedAt, j.CompletedAt, j.CancelRequestedAt != null, j.ErrorCode,
        j.Result == null ? null : Read<Card.Response.CreateCardResponse>(j.Result));
}

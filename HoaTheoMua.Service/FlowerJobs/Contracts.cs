using System.Text.Json;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Flower;

namespace HoaTheoMua.Service.FlowerJobs;

public record FlowerGenerationRequested(Guid JobId);
public record Checkpoint(AiService.Response.GeneratedImage? Flower = null,
    AiService.Response.GeneratedImage? Mockup = null, BrandedFlowerImage? Branded = null);
public record JobResponse(Guid JobId, string Type, string Status, string Stage, string DisplayName,
    Guid ProductId, string? ProductImageUrl, string? MockupImageUrl, Guid? SourceFlowerId,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, DateTimeOffset? CompletedAt,
    bool CancelRequested, string? ErrorCode, Response.GeneratedFlowerResponse? Result);

public class FlowerJobOptions
{
    public int MaxActivePerUser { get; set; } = 3;
    public int Concurrency { get; set; } = 2;
    public int BudgetSeconds { get; set; } = 600;
    public int LeaseSeconds { get; set; } = 60;
    public int RecoveryIntervalSeconds { get; set; } = 15;
}

public static class JobJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T Read<T>(string json) => JsonSerializer.Deserialize<T>(json, Options) ?? throw new JsonException();
    public static bool Active(FlowerGenerationJob job) => job.Status is "Queued" or "Running";
    public static JobResponse Response(FlowerGenerationJob j) => new(j.Id, j.Type, j.Status, j.Stage,
        j.DisplayName, j.ProductId, j.ProductImageUrl, j.MockupImageUrl, j.SourceFlowerId,
        j.CreatedAt, j.UpdatedAt, j.CompletedAt, j.CancelRequestedAt != null, j.ErrorCode,
        j.Result == null ? null : Read<Flower.Response.GeneratedFlowerResponse>(j.Result));
}

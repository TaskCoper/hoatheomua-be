namespace HoaTheoMua.Repository.Entity;

public class ClientHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Input { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Metadata { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid BaseId { get; set; }
    public Guid OutputId { get; set; }
    public Guid? SystemPromptId { get; set; }
    public SystemPrompt? SystemPrompt { get; set; }
}

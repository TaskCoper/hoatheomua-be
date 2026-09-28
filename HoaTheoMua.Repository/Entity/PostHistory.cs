namespace HoaTheoMua.Repository.Entity;

public class PostHistory
{
    public Guid Id { get; set; }
    public string Input { get; set; } = null!;
    public Guid BaseId { get; set; }
    public Guid OutputId { get; set; }
    public Guid SystemPromptId { get; set; }
    public string? Metadata { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

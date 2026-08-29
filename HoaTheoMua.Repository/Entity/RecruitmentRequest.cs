using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class RecruitmentRequest: BaseEntity<Guid>, IAuditableEntity
{
    public required string title { get; set; }
    
    public required string coverLetter { get; set; }
    
    public required List<string> attachments { get; set; } = new();

    public required bool isRead { get; set; } = false;

    // External user id (from taskcoper-be's token); no FK — this service has no User table.
    public Guid UserId { get; set; }
    // Author profile snapshot, captured from the JWT claims at write time.
    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }
    public string? AuthorAvatar { get; set; }

    public JobDescriptions JobDescription { get; set; }
    
    public Guid JobDescriptionId { get; set; }
        
        
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
}
using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class RecruitmentRequest: BaseEntity<Guid>, IAuditableEntity
{
    public required string title { get; set; }
    
    public required string coverLetter { get; set; }
    
    public required string attachments { get; set; }

    public required bool isRead { get; set; } = false;
    
    public User User { get; set; }
    
    public Guid UserId { get; set; }
    
    public JobDescriptions JobDescription { get; set; }
    
    public Guid JobDescriptionId { get; set; }
        
        
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
}
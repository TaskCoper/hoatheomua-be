using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class JobDescriptions : BaseEntity<Guid>, IAuditableEntity
{
    public required string title { get; set; }
    public required string location { get; set; }
    public required string avatarUrl { get; set; }
    public required string coverImageUrl { get; set; }
    public float? minExperienceYears { get; set; }
    public float? maxExperienceYears { get; set; }
    public required LevelJobDescriptions level  { get; set; }
    public required EmploymentType employmentType { get; set; } 
    public float? salaryMin { get; set; }
    public float? salaryMax { get; set; }
    public required bool salaryNegotiable { get; set; }
    public required string description { get; set; }
    public required StatusJobDescriptions status { get; set; }
    public required int openings { get; set; }
    public required string slug { get; set; }

    public ICollection<RecruitmentRequest> recruitmentRequests { get; set; } = new List<RecruitmentRequest>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
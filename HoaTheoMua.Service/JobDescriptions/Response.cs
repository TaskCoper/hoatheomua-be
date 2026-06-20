namespace HoaTheoMua.Service.JobDescriptions;

public class Response
{
    public class CreateResponse
    {
        public Guid id { get; set; }
        public required string Slug { get; set; }
        public required string Title { get; set; }
        public required string Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class JobListItem
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public required string Description { get; set; } 
        public string AvatarUrl { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EmploymentType { get; set; } = null!;
        public float? SalaryMin { get; set; }
        public float? SalaryMax { get; set; }
        public bool SalaryNegotiable { get; set; }
        public int Openings { get; set; }
        public string Status { get; set; } = null!;
    }

    public class HighestSalaryResponse
    {
        public float? Salary { get; set; }
    }

    public class JobDetailResponse
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;
        public string CoverImageUrl { get; set; } = null!;
        public float? MinExperienceYears { get; set; }
        public float? MaxExperienceYears { get; set; }
        public string Level { get; set; } = null!; 
        public string EmploymentType { get; set; } = null!;
        public float? SalaryMin { get; set; }
        public float? SalaryMax { get; set; }
        public bool SalaryNegotiable { get; set; }
        public int Openings { get; set; }
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!; 
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
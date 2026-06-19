namespace HoaTheoMua.Service.JobDescriptions;

public class Response
{
    public class CreateResponse
    {
        public Guid id { get; set; }
        public required string slug { get; set; }
        public required string title { get; set; }
        public required string status { get; set; }
        public DateTimeOffset createdAt { get; set; }
        public DateTimeOffset? updatedAt { get; set; }
    }

    public class JobListItem
    {
        public Guid id { get; set; }
        public string slug { get; set; } = null!;
        public string title { get; set; } = null!;
        public string avatarUrl { get; set; } = null!;
        public string location { get; set; } = null!;
        public string employmentType { get; set; } = null!;
        public float? salaryMin { get; set; }
        public float? salaryMax { get; set; }
        public bool salaryNegotiable { get; set; }
        public int openings { get; set; }
        public string status { get; set; } = null!;
    }

    public class HighestSalaryResponse
    {
        public float? salary { get; set; }
    }

    public class JobDetailResponse
    {
        public Guid id { get; set; }
        public string slug { get; set; } = null!;
        public string title { get; set; } = null!;
        public string location { get; set; } = null!;
        public string avatarUrl { get; set; } = null!;
        public string coverImageUrl { get; set; } = null!;
        public float? minExperienceYears { get; set; }
        public float? maxExperienceYears { get; set; }
        public string level { get; set; } = null!; 
        public string employmentType { get; set; } = null!;
        public float? salaryMin { get; set; }
        public float? salaryMax { get; set; }
        public bool salaryNegotiable { get; set; }
        public int openings { get; set; }
        public string description { get; set; } = null!;
        public string status { get; set; } = null!; 
        public DateTimeOffset createdAt { get; set; }
        public DateTimeOffset? updatedAt { get; set; }
    }
}
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.JobDescriptions;

public class Request
{
    public class CreateRequest
    {
        public required string title { get; set; }
        public required string location { get; set; }
        public required string avatarUrl { get; set; }
        public required string coverImageUrl { get; set; }
        public float? minExperienceYears { get; set; }
        public float? maxExperienceYears { get; set; }
        public required LevelJobDescriptions level { get; set; } 
        public required EmploymentType employmentType { get; set; } 
        public float? salaryMin { get; set; }
        public float? salaryMax { get; set; }
        public required bool salaryNegotiable { get; set; }
        public required string description { get; set; }
        public required StatusJobDescriptions status { get; set; } 
        public int openings { get; set; } = 1;
        public string? slug { get; set; } 
    }
    
    public class UpdateRequest : CreateRequest
    {
    }

    public class GetListRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; } // Nhận dạng chuỗi từ Client (ví dụ: "fullTime")
        public float? SalaryMin { get; set; }
        public float? SalaryMax { get; set; }
    }
}
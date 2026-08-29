using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.RecruitmentRequests;

public class Response
{
    // Author profile snapshot captured from the JWT at write time (no User table).
    // phoneNumber is not carried in the token, so it is always null now.
    public class UserDto
    {
        public Guid userId { get; set; }
        public string? fullName { get; set; }
        public string? email { get; set; }
        public string? phoneNumber { get; set; }
        public string? avatar { get; set; }
    }

    public class JobDto
    {
        public Guid id { get; set; }
        public string title { get; set; } = null!;
    }

    public class RecruitmentRequestItem
    {
        public Guid id { get; set; }
        public string title { get; set; } = null!;
        public string coverLetter { get; set; } = null!;
        public List<string> attachments { get; set; } = new();
        public bool isRead { get; set; }
        public DateTimeOffset createdAt { get; set; }
        public UserDto user { get; set; } = null!;
        public JobDto jobDescription { get; set; } = null!;
    }

    public class MyRecruitmentRequestItem
    {
        public Guid id { get; set; }
        public string title { get; set; } = null!;
        public string coverLetter { get; set; } = null!;
        public List<string> attachments { get; set; } = new();
        public bool isRead { get; set; }
        public DateTimeOffset createdAt { get; set; }
        public JobDto jobDescription { get; set; } = null!;
    }

    public class MarkAsReadResponse
    {
        public Guid id { get; set; }
        public bool isRead { get; set; }
        public DateTimeOffset? updatedAt { get; set; }
    }

    public class CreateRecruitmentResponse
    {
        public Guid id { get; set; }
        public Guid jobDescriptionId { get; set; }
        public string title { get; set; } = null!;
        public List<string> attachments { get; set; } = new();
        public bool isRead { get; set; }
        public DateTimeOffset createdAt { get; set; }
    }
}

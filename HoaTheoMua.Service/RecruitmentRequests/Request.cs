namespace HoaTheoMua.Service.RecruitmentRequests;

public class Request
{
    public class CreateRecruitmentRequest
    {
        public Guid jobDescriptionId { get; set; }
        public string title { get; set; } = null!;
        public string coverLetter { get; set; } = null!;
        public string attachments { get; set; } = null!;
    }
}

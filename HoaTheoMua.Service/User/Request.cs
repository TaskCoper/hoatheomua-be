namespace HoaTheoMua.Service.User;

public class Request
{
    public class ContactRequest
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Topic { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
}
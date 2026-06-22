namespace HoaTheoMua.Service.User;

public class Request
{
    public class ContactRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Topic { get; set; }
        public string Content { get; set; }
        public string Phone { get; set; }
    }
}
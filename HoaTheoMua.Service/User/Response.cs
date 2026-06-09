using System.ComponentModel.DataAnnotations;

namespace HoaTheoMua.Service.User;

public class Response
{
    public class GetMeResponse
    {
        public required String FirstName { get; set; }
        public required String LastName { get; set; }
        public required String Email { get; set; }
        public String? Avatar { get; set; }
    }
}
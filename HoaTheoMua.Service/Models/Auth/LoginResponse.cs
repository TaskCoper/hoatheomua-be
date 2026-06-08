using System.Text.Json.Serialization;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.Models.Auth;

public class LoginResponse
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Role Role { get; set; }

    public bool IsVerify { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}
namespace HoaTheoMua.Service.Models.Auth;

public class LoginResult
{
    public bool Success { get; set; }

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public static LoginResult Ok(
        string accessToken,
        string refreshToken,
        string message = "Đăng nhập thành công")
    {
        return new LoginResult
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public static LoginResult Fail(int statusCode, string message)
    {
        return new LoginResult
        {
            Success = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
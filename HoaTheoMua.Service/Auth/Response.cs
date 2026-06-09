namespace HoaTheoMua.Service.Auth;

public class Response
{
    public string? AccessToken { get; set; } 

    public string? RefreshToken { get; set; } 

    public static Response Ok(
        string accessToken,
        string refreshToken
        )
    {
        return new Response
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
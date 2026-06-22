namespace HoaTheoMua.Extentions;

public static class CookieExtensions
{
    // Kept so JwtExtensions' OnMessageReceived can read a forwarded access-token cookie.
    // This service no longer issues tokens, so there are no write/delete helpers.
    public const string AccessTokenCookieName = "HTM-Access-Token";
}
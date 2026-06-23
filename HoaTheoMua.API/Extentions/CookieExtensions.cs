namespace HoaTheoMua.Extentions;

public static class CookieExtensions
{
    // Must match taskcoper-be's AuthCookieHelper — that service issues the tokens and writes
    // this cookie; hoatheomua-be only reads the forwarded access-token cookie (no write/delete).
    public const string AccessTokenCookieName = "accessToken";
}
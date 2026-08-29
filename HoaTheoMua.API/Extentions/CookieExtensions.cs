namespace HoaTheoMua.Extentions;

public static class CookieExtensions
{
    // Must match taskcoper-be's AuthCookieHelper — that service issues the tokens and writes
    // this cookie; hoatheomua-be only reads the forwarded access-token cookie (no write/delete).
    public const string AccessTokenCookieName = "accessToken";

    // Also read-only here: used solely to tell a *missing* access token (the caller still
    // holds a refresh cookie and can silently refresh) apart from an *invalid* one. Never written/deleted.
    public const string RefreshTokenCookieName = "refreshToken";
}
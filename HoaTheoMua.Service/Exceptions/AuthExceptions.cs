namespace HoaTheoMua.Service.Exceptions;

// MessageCode/detail strings are kept byte-identical to taskcoper-be's JwtExtensions OnChallenge
// so a shared frontend reacts the same way regardless of which backend issued the 401.
public class MissingAccessTokenException : AppException
{
    public MissingAccessTokenException()
        : base("Unauthorized", 401, "MissingAccessToken",
            "The access token cookie is missing.") { }
}

public class ExpiredAccessTokenException : AppException
{
    public ExpiredAccessTokenException()
        : base("Unauthorized", 401, "ExpiredAccessToken",
            "The access token has expired.") { }
}

public class InvalidAccessTokenException : AppException
{
    public InvalidAccessTokenException()
        : base("Unauthorized", 401, "InvalidAccessToken",
            "The access token is invalid or missing.") { }
}

public class ExpiredRefreshTokenException : AppException
{
    public ExpiredRefreshTokenException()
        : base("Unauthorized",401,  "EXPIRED_REFRESH_TOKEN", 
            "Refresh token has expired. Please login again.") { }
}
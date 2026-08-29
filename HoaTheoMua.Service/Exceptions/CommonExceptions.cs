namespace HoaTheoMua.Service.Exceptions;

public class ServerException : AppException
{
    public ServerException(string detail)
        : base(title: "Internal Server Error", statusCode: 500, messageCode: "INTERNAL_SERVER_ERROR", detail: detail) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string detail)
        : base("Not Found",404, "NOT_FOUND", detail) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string detail)
        : base("Forbidden", 403, "FORBIDDEN", detail) { }
}

public class BadRequestException : AppException
{
    public BadRequestException(string detail)
        : base( "Bad Request", 400, "BAD_REQUEST", detail) { }
}

public class TooManyRequestException : AppException
{
    public TooManyRequestException(string detail)
        : base("Too many request", 429, "TOO_MANY_REQUEST", detail) { }
}

public class ConflictException : AppException
{
    public ConflictException(string detail)
        : base("Conflict", 409, "CONFLICT", detail) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string detail)
        : base("Unauthorized", 401, "UNAUTHORIZED", detail) { }
}

public class ArgumentException : AppException
{
    public ArgumentException(string detail)
        : base("Argument Exception", 400, "ARGUMENT_EXCEPTION", detail) { }
}

public class BadGatewayException : AppException
{
    public BadGatewayException(string detail)
        : base("Bad Gateway", 502, "BAD_GATEWAY", detail) { }
}
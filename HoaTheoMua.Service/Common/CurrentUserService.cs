using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;

namespace HoaTheoMua.Service.Common;

/// <summary>
/// Reads the current identity from the validated JWT claims issued by taskcoper-be.
/// This service has no User table, so the token IS the source of truth.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    string? Avatar { get; }
    bool IsVerified { get; }
    bool CanUseStorefront => IsVerified;

    /// <summary>UserId from the token, or throws Unauthorized when absent.</summary>
    Guid GetRequiredUserId();
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string? Claim(string type) =>
        _httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value;

    public Guid? UserId =>
        Guid.TryParse(Claim("UserId"), out var id) ? id : null;

    public string? Email => Claim("Email");
    public string? FullName => Claim("FullName");
    public string? Avatar => Claim("Avatar");
    private bool IsForgotSession => string.Equals(Claim("IsForgotPassword"), "true", StringComparison.OrdinalIgnoreCase);
    public bool IsVerified => !IsForgotSession && string.Equals(Claim("IsVerified"), "true", StringComparison.OrdinalIgnoreCase);
    public bool CanUseStorefront => !IsForgotSession && (IsVerified ||
        string.Equals(Claim("CanUseStorefront"), "true", StringComparison.OrdinalIgnoreCase));

    public Guid GetRequiredUserId() =>
        UserId ?? throw new UnauthorizedException("User not found");
}

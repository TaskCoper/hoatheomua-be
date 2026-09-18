using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;

namespace HoaTheoMua.Service.PlatformAccount;

public sealed class AccountException(int status, string code, string message) : AppException("Platform account", status, code, message);

// These types are internal service/provider contracts, never controller response DTOs.
public sealed class Credentials
{
    public string AccessToken { get; set; } = "";
    public string? RefreshToken { get; set; }
}
public sealed class ProviderAccount
{
    public string ExternalId { get; set; } = "";
    public string? PageId { get; set; }
    public string Name { get; set; } = "";
    public string[] Scopes { get; set; } = [];
    public Credentials Credentials { get; set; } = new();
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RefreshExpiresAt { get; set; }
}
public interface IOAuthProvider
{
    string AuthorizeUrl(PlatformType platform, string state, string verifier);
    Task<List<ProviderAccount>> Exchange(PlatformType platform, string code, string verifier, CancellationToken ct);
    Task<ProviderAccount> Refresh(PlatformType platform, string externalId, string? pageId, Credentials credentials, CancellationToken ct);
}

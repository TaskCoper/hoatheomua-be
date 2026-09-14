namespace HoaTheoMua.Service.PlatformAccount;

public static class Response
{
    public sealed class Account
    {
        public Guid Id { get; set; }
        public string Platform { get; set; } = "";
        public string ExternalAccountId { get; set; } = "";
        public string? PageId { get; set; }
        public string DisplayName { get; set; } = "";
        public string Status { get; set; } = "";
        public string[] GrantedScopes { get; set; } = [];
        public DateTimeOffset? AccessTokenExpiresAt { get; set; }
        public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
        public DateTimeOffset? LastValidatedAt { get; set; }
        public string? LastErrorCode { get; set; }
        public Guid ConnectedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public long Revision { get; set; }
    }
    public sealed record Connection(Guid SessionId, string AuthorizationUrl, DateTimeOffset ExpiresAt);
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AccountEntity = HoaTheoMua.Repository.Entity.PlatformAccount;

namespace HoaTheoMua.Service.PlatformAccount;

public class Service(AppDbContext db, ICurrentUserService user, IOAuthProvider provider,
    TokenProtector protector, IOptions<PlatformAccountOptions> settings, TimeProvider clock) : IService, IPublicationTokenRefresher
{
    public async Task<List<Response.Account>> List(string? platform = null, CancellationToken ct = default)
    {
        user.GetRequiredUserId();
        var query = db.PlatformAccounts.AsNoTracking().Where(x => !x.IsDeleted);
        if (platform != null) { var parsed = Parse(platform); query = query.Where(x => x.Platform == parsed); }
        // Explicit projection keeps credentials out of read DTOs and SELECT results.
        return await query.OrderBy(x => x.Platform).ThenBy(x => x.DisplayName).ThenBy(x => x.Id).Select(x => new Response.Account
        {
            Id = x.Id, Platform = x.Platform.ToString().ToLower(), ExternalAccountId = x.ExternalAccountId,
            PageId = x.PageId, DisplayName = x.DisplayName, Status = x.Status, GrantedScopes = x.GrantedScopes,
            AccessTokenExpiresAt = x.AccessTokenExpiresAt, RefreshTokenExpiresAt = x.RefreshTokenExpiresAt,
            LastValidatedAt = x.LastValidatedAt, LastErrorCode = x.LastErrorCode, ConnectedBy = x.ConnectedBy,
            CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt, Revision = x.Revision
        }).ToListAsync(ct);
    }

    public async Task<Response.Connection> Connect(string platform, string browserProof, Guid? accountId = null, CancellationToken ct = default)
    {
        var actor = user.GetRequiredUserId(); var parsed = Parse(platform);
        settings.Value.RequireConfigured(parsed);
        if (browserProof.Length is < 32 or > 128) throw InvalidState();
        AccountEntity? account = null;
        if (accountId.HasValue)
        {
            account = await db.PlatformAccounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == accountId && !x.IsDeleted, ct) ?? throw Missing();
            if (account.Platform != parsed) throw new BadRequestException("Tài khoản không thuộc nền tảng yêu cầu.");
        }
        var state = Random(); var verifier = Random(); var now = clock.GetUtcNow();
        var session = new PlatformOAuthSession
        {
            Id = Guid.NewGuid(), Platform = parsed, StateHash = Hash(state), BrowserHash = Hash(browserProof), RequestedBy = actor,
            TargetAccountId = account?.Id, TargetRevision = account?.Revision, CreatedAt = now,
            ExpiresAt = now.AddMinutes(settings.Value.OAuthStateMinutes)
        };
        session.ProtectedVerifier = protector.Protect(verifier, SessionPurpose(session.Id));
        var url = provider.AuthorizeUrl(parsed, state, verifier);
        db.PlatformOAuthSessions.Add(session);
        if (db.Database.IsRelational())
            await db.PlatformOAuthSessions.Where(x => x.ExpiresAt < now.AddDays(-7)).ExecuteDeleteAsync(ct);
        await db.SaveChangesAsync(ct);
        return new(session.Id, url, session.ExpiresAt);
    }

    public async Task<Response.Connection> Reconnect(Guid id, string browserProof, CancellationToken ct = default)
    {
        user.GetRequiredUserId();
        var platform = await db.PlatformAccounts.AsNoTracking().Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => (PlatformType?)x.Platform).SingleOrDefaultAsync(ct) ?? throw Missing();
        return await Connect(platform.ToString().ToLowerInvariant(), browserProof, id, ct);
    }

    public async Task<List<Response.Account>> Callback(string platform, string? state, string? code, string? error, string? browserProof, CancellationToken ct = default)
    {
        var parsed = Parse(platform);
        if (state?.Length is not (>= 32 and <= 128) || browserProof?.Length is not (>= 32 and <= 128)) throw InvalidState();
        var hash = Hash(state);
        PlatformOAuthSession session;
        string verifier;
        await using (var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            session = (db.Database.IsRelational()
                ? (await db.PlatformOAuthSessions.FromSqlInterpolated($"SELECT * FROM \"PlatformOAuthSession\" WHERE \"StateHash\" = {hash} FOR UPDATE").ToListAsync(ct)).SingleOrDefault()
                : await db.PlatformOAuthSessions.SingleOrDefaultAsync(x => x.StateHash == hash, ct)) ?? throw InvalidState();
            if (session.Platform != parsed || session.UsedAt != null || session.ExpiresAt <= clock.GetUtcNow()
                || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(session.BrowserHash), Encoding.ASCII.GetBytes(Hash(browserProof)))) throw InvalidState();
            verifier = protector.Unprotect(session.ProtectedVerifier!, SessionPurpose(session.Id));
            session.UsedAt = clock.GetUtcNow(); session.ProtectedVerifier = null;
            await db.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        // Consume state even on denial; a retry must start a new authorization flow.
        if (!string.IsNullOrEmpty(error)) throw new AccountException(400, "PLATFORM_OAUTH_DENIED", "Quyền kết nối đã bị từ chối; hãy bắt đầu kết nối lại.");
        if (string.IsNullOrWhiteSpace(code) || code.Length > 8192) throw new BadRequestException("Thiếu hoặc sai authorization code.");
        List<ProviderAccount> candidates;
        try { candidates = await provider.Exchange(parsed, code, verifier, ct); }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        { throw new AccountException(502, "PLATFORM_INVALID_RESPONSE", "Phản hồi nền tảng không đúng contract."); }
        if (candidates.Count == 0 || candidates.Count > 2000 || candidates.Select(x => x.ExternalId).Distinct().Count() != candidates.Count)
            throw new AccountException(502, "PLATFORM_INVALID_RESPONSE", "Danh sách tài khoản nền tảng không hợp lệ.");
        foreach (var candidate in candidates) Validate(candidate);
        await using var persist = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var result = new List<Response.Account>();
        // A reconnect can update only the original account, never silently switch it to another Page/OA.
        AccountEntity? target = null;
        if (session.TargetAccountId.HasValue)
        {
            target = await LockAccount(session.TargetAccountId.Value, ct);
            if (target.Revision != session.TargetRevision) throw Changed();
            candidates = candidates.Where(x => x.ExternalId == target.ExternalAccountId).ToList();
            if (candidates.Count != 1) throw new AccountException(400, "PLATFORM_ACCOUNT_MISMATCH", "Quyền vừa cấp không thuộc tài khoản cần kết nối lại.");
        }
        foreach (var candidate in candidates.OrderBy(x => x.ExternalId, StringComparer.Ordinal))
        {
            AccountEntity? account = target;
            if (account == null)
            {
                if (db.Database.IsRelational())
                {
                    var key = $"postgen-account:{parsed}:{candidate.ExternalId}";
                    await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))", ct);
                }
                var existing = await db.PlatformAccounts.AsNoTracking().SingleOrDefaultAsync(x => x.Platform == parsed && x.ExternalAccountId == candidate.ExternalId, ct);
                if (existing != null)
                {
                    account = await LockAccount(existing.Id, ct);
                    if ((account.UpdatedAt ?? account.CreatedAt) >= session.CreatedAt || account.RefreshLeaseId != null) throw Changed();
                    // Disabled accounts require explicit reconnect, so an old/general consent cannot revive them.
                    if (account.Status == "Disabled") throw new AccountException(409, "PLATFORM_ACCOUNT_DISABLED", "Dùng thao tác kết nối lại cho tài khoản đã ngắt kết nối.");
                }
            }
            if (account == null)
            {
                account = new() { Id = Guid.NewGuid(), Platform = parsed, ExternalAccountId = candidate.ExternalId, CreatedAt = clock.GetUtcNow() };
                db.PlatformAccounts.Add(account);
            }
            Apply(account, candidate);
            account.ConnectedBy = session.RequestedBy;
            account.Revision++; account.UpdatedAt = clock.GetUtcNow();
            result.Add(ToResponse(account));
        }
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw Changed(); }
        if (persist != null) await persist.CommitAsync(ct);
        return result;
    }

    public async Task<Response.Account> Refresh(Guid id, CancellationToken ct = default)
    {
        user.GetRequiredUserId();
        return await RefreshCore(id, false, ct);
    }
    public async Task RefreshForPublication(Guid accountId, CancellationToken ct)
        => _ = await RefreshCore(accountId, true, ct);

    private async Task<Response.Account> RefreshCore(Guid id, bool onlyIfDue, CancellationToken ct)
    {
        AccountEntity account; Credentials credentials; var lease = Guid.NewGuid();
        await using (var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            account = await LockAccount(id, ct);
            if (onlyIfDue && (account.Platform != PlatformType.Zalo || account.Status != "Connected"
                || account.AccessTokenExpiresAt > clock.GetUtcNow().AddMinutes(5)))
            { if (tx != null) await tx.CommitAsync(ct); return ToResponse(account); }
            settings.Value.RequireConfigured(account.Platform);
            if (account.Status != "Connected") throw new AccountException(409, "PLATFORM_REAUTH_REQUIRED", "Tài khoản cần kết nối lại trước khi làm mới token.");
            if (account.Platform == PlatformType.Zalo && await db.PostPublications.AnyAsync(x => x.PlatformAccountId == id
                && x.LeaseId != null && x.LeaseUntil > clock.GetUtcNow(), ct))
                throw new AccountException(409, "PLATFORM_REFRESH_IN_PROGRESS", "Tài khoản đang được dùng để đăng bài; thử lại sau.");
            if (account.RefreshLeaseId != null)
            {
                if (account.RefreshLeaseUntil > clock.GetUtcNow()) throw new AccountException(409, "PLATFORM_REFRESH_IN_PROGRESS", "Tài khoản đang được làm mới token.");
                // A crashed worker may have already consumed a one-use refresh token. Do not resend it.
                RequireReauth(account, "PLATFORM_REFRESH_INTERRUPTED");
                await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
                return ToResponse(account);
            }
            credentials = JsonSerializer.Deserialize<Credentials>(protector.Unprotect(account.ProtectedCredentials!, AccountPurpose(id)))
                ?? throw new AccountException(503, "PLATFORM_TOKEN_UNAVAILABLE", "Thông tin kết nối không hợp lệ.");
            if (account.RefreshTokenExpiresAt <= clock.GetUtcNow())
            {
                RequireReauth(account, "PLATFORM_REAUTH_REQUIRED");
                await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
                return ToResponse(account);
            }
            account.RefreshLeaseId = lease;
            account.RefreshLeaseUntil = clock.GetUtcNow().AddSeconds(settings.Value.RequestTimeoutSeconds * 3 + 30);
            account.Revision++; account.UpdatedAt = clock.GetUtcNow();
            await db.SaveChangesAsync(ct); if (tx != null) await tx.CommitAsync(ct);
        }
        ProviderAccount? refreshed = null; string? failure = null; bool cancelled = false;
        try
        {
            refreshed = await provider.Refresh(account.Platform, account.ExternalAccountId, account.PageId, credentials, ct);
            // Refresh responses can omit unchanged names and scopes (Zalo token endpoint).
            refreshed.Name = string.IsNullOrWhiteSpace(refreshed.Name) ? account.DisplayName : refreshed.Name;
            if (refreshed.Scopes.Length == 0 && account.Platform == PlatformType.Zalo) refreshed.Scopes = account.GrantedScopes;
            Validate(refreshed);
            if (refreshed.ExternalId != account.ExternalAccountId) throw new AccountException(400, "PLATFORM_ACCOUNT_MISMATCH", "Tài khoản token không khớp.");
        }
        catch (AccountException ex) { failure = ex.MessageCode; }
        catch (OperationCanceledException) { failure = "PLATFORM_REFRESH_INTERRUPTED"; cancelled = true; }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException or HttpRequestException)
        { failure = "PLATFORM_INVALID_RESPONSE"; }
        using var persistence = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var saveToken = persistence.Token;
        await using var finish = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(saveToken) : null;
        account = await LockAccount(id, saveToken);
        if (account.RefreshLeaseId != lease) throw Changed(); // Disconnect/reconnect wins over a stale response.
        if (failure != null)
        {
            if (account.Platform == PlatformType.Zalo || failure is "PLATFORM_REAUTH_REQUIRED" or "PLATFORM_ACCOUNT_MISMATCH")
                RequireReauth(account, failure);
            else { account.LastErrorCode = failure; account.RefreshLeaseId = null; account.RefreshLeaseUntil = null; account.Revision++; account.UpdatedAt = clock.GetUtcNow(); }
        }
        else
        {
            Apply(account, refreshed!); account.Revision++; account.UpdatedAt = clock.GetUtcNow();
            if (account.AccessTokenExpiresAt <= clock.GetUtcNow()) RequireReauth(account, "PLATFORM_REAUTH_REQUIRED");
        }
        await db.SaveChangesAsync(saveToken); if (finish != null) await finish.CommitAsync(saveToken);
        if (cancelled) throw new OperationCanceledException(ct);
        return ToResponse(account);
    }

    public async Task<Response.Account> Disconnect(Guid id, CancellationToken ct = default)
    {
        user.GetRequiredUserId();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var account = await LockAccount(id, ct);
        if (await db.PlatformPosts.AnyAsync(x => x.PlatformAccountId == id && !x.IsDeleted
            && (x.Status == PlatformPostStatus.Scheduled || x.Status == PlatformPostStatus.Publishing), ct)
            || await db.PostPublications.AnyAsync(x => x.PlatformAccountId == id && PostPublishing.Store.ActiveStates.Contains(x.Status), ct))
            throw new AccountException(409, "PLATFORM_ACCOUNT_IN_USE", "Hủy lịch hoặc chờ tác vụ đăng kết thúc trước khi ngắt kết nối.");
        if (account.Status != "Disabled")
        {
            account.Status = "Disabled"; account.ProtectedCredentials = null;
            account.RefreshLeaseId = null; account.RefreshLeaseUntil = null;
            account.LastErrorCode = null; account.Revision++; account.UpdatedAt = clock.GetUtcNow();
            await db.SaveChangesAsync(ct);
        }
        if (tx != null) await tx.CommitAsync(ct);
        return ToResponse(account);
    }

    private async Task<AccountEntity> LockAccount(Guid id, CancellationToken ct)
    {
        // Avoid tracked values masking updates committed by another connection between provider calls.
        var tracked = db.ChangeTracker.Entries<AccountEntity>().FirstOrDefault(x => x.Entity.Id == id);
        if (tracked != null) tracked.State = EntityState.Detached;
        return (db.Database.IsRelational()
            ? (await db.PlatformAccounts.FromSqlInterpolated($"SELECT * FROM \"PlatformAccount\" WHERE \"Id\" = {id} AND NOT \"IsDeleted\" FOR UPDATE").ToListAsync(ct)).SingleOrDefault()
            : await db.PlatformAccounts.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)) ?? throw Missing();
    }
    private void Apply(AccountEntity account, ProviderAccount candidate)
    {
        account.PageId = candidate.PageId; account.DisplayName = candidate.Name;
        account.GrantedScopes = candidate.Scopes; account.AccessTokenExpiresAt = candidate.ExpiresAt;
        account.RefreshTokenExpiresAt = candidate.RefreshExpiresAt;
        account.ProtectedCredentials = protector.Protect(JsonSerializer.Serialize(candidate.Credentials), AccountPurpose(account.Id));
        account.Status = "Connected"; account.LastErrorCode = null; account.LastValidatedAt = clock.GetUtcNow();
        account.RefreshLeaseId = null; account.RefreshLeaseUntil = null;
    }
    private void RequireReauth(AccountEntity account, string code)
    {
        account.Status = "ReauthRequired"; account.LastErrorCode = code; account.ProtectedCredentials = null;
        account.RefreshLeaseId = null; account.RefreshLeaseUntil = null; account.Revision++; account.UpdatedAt = clock.GetUtcNow();
    }
    private static void Validate(ProviderAccount x)
    {
        if (string.IsNullOrWhiteSpace(x.ExternalId) || x.ExternalId.Length > 128 || x.PageId?.Length > 128
            || string.IsNullOrWhiteSpace(x.Name) || x.Name.Length > 300 || string.IsNullOrWhiteSpace(x.Credentials.AccessToken)
            || x.Credentials.AccessToken.Length > 32768 || x.Credentials.RefreshToken?.Length > 32768
            || x.Scopes.Length > 200 || x.Scopes.Any(v => string.IsNullOrWhiteSpace(v) || v.Length > 200))
            throw new AccountException(502, "PLATFORM_INVALID_RESPONSE", "Thông tin tài khoản nền tảng không hợp lệ.");
    }
    private static Response.Account ToResponse(AccountEntity x) => new()
    {
        Id = x.Id, Platform = x.Platform.ToString().ToLowerInvariant(), ExternalAccountId = x.ExternalAccountId,
        PageId = x.PageId, DisplayName = x.DisplayName, Status = x.Status, GrantedScopes = x.GrantedScopes,
        AccessTokenExpiresAt = x.AccessTokenExpiresAt, RefreshTokenExpiresAt = x.RefreshTokenExpiresAt,
        LastValidatedAt = x.LastValidatedAt, LastErrorCode = x.LastErrorCode, ConnectedBy = x.ConnectedBy,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt, Revision = x.Revision
    };
    public static PlatformType Parse(string platform) => platform.ToLowerInvariant() switch
    { "facebook" => PlatformType.Facebook, "instagram" => PlatformType.Instagram, "zalo" => PlatformType.Zalo, _ => throw new BadRequestException("Nền tảng phải là facebook, instagram hoặc zalo.") };
    public static string Random() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    internal static string AccountPurpose(Guid id) => $"postgen:account:{id}";
    private static string SessionPurpose(Guid id) => $"postgen:oauth:{id}";
    private static AccountException InvalidState() => new(400, "PLATFORM_OAUTH_STATE_INVALID", "Phiên kết nối không hợp lệ, hết hạn hoặc không thuộc trình duyệt này.");
    private static AccountException Changed() => new(409, "PLATFORM_ACCOUNT_CHANGED", "Thông tin kết nối đã thay đổi; tải lại và kết nối lại nếu cần.");
    private static NotFoundException Missing() => new("PLATFORM_ACCOUNT_NOT_FOUND", "Không tìm thấy tài khoản nền tảng.");
}

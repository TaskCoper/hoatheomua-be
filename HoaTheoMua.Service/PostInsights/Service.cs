using System.Text.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.PostInsights;

public interface IService
{
    Task<Response.Insights> Get(Guid publicationId, CancellationToken ct = default);
    Task<Response.AccountInsights> GetAccount(Guid accountId, AccountInsightsRequest request, CancellationToken ct = default);
}

public sealed partial class Service(AppDbContext db, ICurrentUserService user, TokenProtector protector,
    MetaInsightsClient client, IOptions<PlatformAccountOptions> settings, TimeProvider clock) : IService
{
    public async Task<Response.Insights> Get(Guid publicationId, CancellationToken ct = default)
    {
        user.GetRequiredUserId();
        var job = await db.PostPublications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == publicationId, ct)
            ?? throw new NotFoundException("Publication not found");
        if (job.Status != "Succeeded" || string.IsNullOrWhiteSpace(job.ExternalPostId))
            throw Error(409, "INSIGHTS_PUBLICATION_NOT_PUBLISHED", "Chỉ đọc chỉ số của bài đã đăng thành công.");
        PublicationSnapshot snapshot;
        try { snapshot = Store.Read<PublicationSnapshot>(job.Snapshot); }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        { throw Error(409, "INSIGHTS_PUBLICATION_INVALID", "Dữ liệu lần đăng không hợp lệ."); }
        if (snapshot.BasePostId != job.BasePostId || snapshot.PlatformPostId != job.PlatformPostId || snapshot.AccountId != job.PlatformAccountId)
            throw Error(409, "INSIGHTS_PUBLICATION_INVALID", "Dữ liệu lần đăng không hợp lệ.");
        if (snapshot.Platform is not (PlatformType.Facebook or PlatformType.Instagram))
            throw Error(400, "INSIGHTS_PLATFORM_UNSUPPORTED", "Chỉ hỗ trợ Facebook và Instagram.");
        var account = await db.PlatformAccounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == job.PlatformAccountId, ct)
            ?? throw Error(409, "PLATFORM_REAUTH_REQUIRED", "Cần kết nối lại tài khoản nền tảng.");
        if (account.IsDeleted || account.Status != "Connected" || account.AccessTokenExpiresAt <= clock.GetUtcNow()
            || string.IsNullOrWhiteSpace(account.ProtectedCredentials))
            throw Error(409, "PLATFORM_REAUTH_REQUIRED", "Cần kết nối lại tài khoản nền tảng.");
        if (account.Platform != snapshot.Platform || account.ExternalAccountId != snapshot.ExternalAccountId || account.PageId != snapshot.PageId)
            throw Error(409, "PLATFORM_ACCOUNT_MISMATCH", "Tài khoản không khớp với lần đăng.");
        if (account.RefreshLeaseId != null)
            throw Error(409, "PLATFORM_REFRESH_IN_PROGRESS", "Tài khoản đang được làm mới; thử lại sau.");
        var required = snapshot.Platform == PlatformType.Facebook
            ? new[] { "pages_read_engagement", "read_insights" }
            : new[] { "pages_read_engagement", "instagram_basic", "instagram_manage_insights" };
        if (required.Except(account.GrantedScopes).Any())
            throw Error(409, "INSIGHTS_PERMISSION_REQUIRED", "Kết nối lại tài khoản và cấp quyền: " + string.Join(", ", required));
        var options = settings.Value;
        if (!options.Meta.Enabled || !Regex.IsMatch(options.Meta.GraphApiVersion, @"\Av[0-9]+\.[0-9]+\z")
            || options.RequestTimeoutSeconds is < 1 or > 60)
            throw Error(503, "INSIGHTS_NOT_CONFIGURED", "Cấu hình đọc Insights chưa hợp lệ.");
        Credentials credentials;
        try
        {
            credentials = JsonSerializer.Deserialize<Credentials>(protector.Unprotect(account.ProtectedCredentials,
                PlatformAccount.Service.AccountPurpose(account.Id))) ?? throw new JsonException();
            if (string.IsNullOrWhiteSpace(credentials.AccessToken)) throw new JsonException();
        }
        catch (Exception ex) when (ex is AccountException or JsonException)
        { throw Error(409, "PLATFORM_TOKEN_UNAVAILABLE", "Không thể đọc thông tin xác thực tài khoản."); }
        // Read current metrics using the currently configured API version, not a retired publication version.
        snapshot = snapshot with { ApiVersion = options.Meta.GraphApiVersion };
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(options.RequestTimeoutSeconds));
        IReadOnlyDictionary<string, Response.Metric> metrics;
        try { metrics = await client.Get(snapshot, job.ExternalPostId, credentials, budget.Token); }
        catch (MetaPublishingException ex)
        {
            ct.ThrowIfCancellationRequested();
            var code = ex.Result(false).ErrorCode!;
            var status = code switch
            {
                "META_TOKEN_INVALID" or "META_PERMISSION_DENIED" => 409,
                "META_RATE_LIMITED" => 429,
                "META_TIMEOUT" => 504,
                _ => 502
            };
            throw Error(status, code, "Không thể đọc chỉ số từ Meta. Kiểm tra quyền tài khoản hoặc thử lại sau.");
        }
        return new(job.Id, job.BasePostId, account.Id, snapshot.Platform.ToString().ToLowerInvariant(), job.ExternalPostId,
            job.ExternalPostUrl, clock.GetUtcNow(), "lifetime", snapshot.ApiVersion,
            metrics.Values.Any(x => x.Value == null),
            new(metrics["views"].Value, metrics["reach"].Value, metrics["likes"].Value, metrics["comments"].Value,
                metrics["saved"].Value, metrics["shares"].Value, metrics["total_interactions"].Value), metrics);
    }

    private static PublicationException Error(int status, string code, string message) => new(status, code, message);
}

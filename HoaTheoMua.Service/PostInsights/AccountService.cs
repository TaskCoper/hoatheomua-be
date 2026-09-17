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

public sealed partial class Service
{
    public async Task<Response.AccountInsights> GetAccount(Guid accountId, AccountInsightsRequest request, CancellationToken ct = default)
    {
        user.GetRequiredUserId();
        var (since, until, metrics) = request.Validate(clock.GetUtcNow());
        var account = await db.PlatformAccounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == accountId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Platform account not found");
        if (account.Platform is not (PlatformType.Facebook or PlatformType.Instagram))
            throw Error(400, "INSIGHTS_PLATFORM_UNSUPPORTED", "Chỉ hỗ trợ Facebook và Instagram.");
        if (account.Platform == PlatformType.Facebook && request.MetricType != null)
            throw new BadRequestException("metricType chỉ dành cho Instagram.");
        if (account.IsDeleted || account.Status != "Connected" || account.AccessTokenExpiresAt <= clock.GetUtcNow()
            || string.IsNullOrWhiteSpace(account.ProtectedCredentials))
            throw Error(409, "PLATFORM_REAUTH_REQUIRED", "Cần kết nối lại tài khoản nền tảng.");
        if (account.RefreshLeaseId != null)
            throw Error(409, "PLATFORM_REFRESH_IN_PROGRESS", "Tài khoản đang được làm mới; thử lại sau.");
        var required = account.Platform == PlatformType.Facebook
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

        var snapshot = new PublicationSnapshot(Guid.Empty, Guid.Empty, account.Id, account.ExternalAccountId,
            account.PageId, account.Platform, options.Meta.GraphApiVersion, "", Guid.Empty, null, "", null, "", []);
        var metricType = account.Platform == PlatformType.Instagram ? request.MetricType ?? "total_value" : null;
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(ct);
        budget.CancelAfter(TimeSpan.FromSeconds(options.RequestTimeoutSeconds));
        IReadOnlyDictionary<string, Response.AccountMetric> result;
        try { result = await client.GetAccount(snapshot, credentials, metrics, request.Period, metricType, since, until, budget.Token); }
        catch (MetaPublishingException ex)
        {
            ct.ThrowIfCancellationRequested();
            var code = ex.Result(false).ErrorCode!;
            var status = code switch
            {
                "META_TOKEN_INVALID" or "META_PERMISSION_DENIED" => 409,
                "META_RATE_LIMITED" => 429, "META_TIMEOUT" => 504, _ => 502
            };
            throw Error(status, code, "Không thể đọc chỉ số từ Meta. Kiểm tra quyền tài khoản hoặc thử lại sau.");
        }
        return new(account.Id, account.Platform.ToString().ToLowerInvariant(), account.ExternalAccountId,
            request.FromDate!.Value, request.ToDate!.Value, "UTC", request.Period, metricType,
            snapshot.ApiVersion, clock.GetUtcNow(), result.Values.Any(x => x.Status != "Available"), result);
    }
}

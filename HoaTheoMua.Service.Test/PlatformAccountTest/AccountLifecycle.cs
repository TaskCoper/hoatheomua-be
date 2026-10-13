using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.PlatformAccount;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using AccountService = HoaTheoMua.Service.PlatformAccount.Service;

namespace HoaTheoMua.Service.Test.PlatformAccountTest;

public class AccountLifecycle
{
    [Fact]
    public async Task BackgroundRefreshRotatesOnlyWhenDueAndWithoutHttpUser()
    {
        await using var db = Database(); var clock = new Clock(); var provider = Provider();
        var setup = Create(db, provider.Object, clock); var proof = AccountService.Random();
        var connection = await setup.Connect("zalo", proof);
        var account = Assert.Single(await setup.Callback("zalo", State(connection), "code", null, proof));
        var stored = await db.PlatformAccounts.SingleAsync(); stored.AccessTokenExpiresAt = clock.Now.AddHours(1); await db.SaveChangesAsync();
        var user = new Mock<ICurrentUserService>(MockBehavior.Strict);
        var service = new AccountService(db, user.Object, provider.Object, new(Options()), Options(), clock);
        await service.RefreshForPublication(account.Id, default);
        provider.Verify(x => x.Refresh(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>()), Times.Never);
        clock.Now = clock.Now.AddMinutes(58);
        provider.Setup(x => x.Refresh(PlatformType.Zalo, "page", It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProviderAccount { ExternalId = "page", Credentials = new() { AccessToken = "new-access", RefreshToken = "new-refresh" }, ExpiresAt = clock.Now.AddHours(25) });
        await service.RefreshForPublication(account.Id, default);
        await service.RefreshForPublication(account.Id, default);
        provider.Verify(x => x.Refresh(PlatformType.Zalo, "page", It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>()), Times.Once);
        user.VerifyNoOtherCalls();
        Assert.Contains("new-refresh", new TokenProtector(Options()).Unprotect((await db.PlatformAccounts.SingleAsync()).ProtectedCredentials!, $"postgen:account:{account.Id}"));
    }

    [Fact]
    public async Task CallbackEncryptsTokensConsumesStateAndListDoesNotExposeSecrets()
    {
        await using var db = Database(); var clock = new Clock(); var provider = Provider(); var service = Create(db, provider.Object, clock);
        var proof = AccountService.Random(); var connection = await service.Connect("facebook", proof);
        var state = State(connection);
        Assert.DoesNotContain(state, JsonSerializer.Serialize(await db.PlatformOAuthSessions.SingleAsync()));
        var account = Assert.Single(await service.Callback("facebook", state, "code", null, proof));
        Assert.Equal("Connected", account.Status); Assert.Equal("page", account.ExternalAccountId);
        var stored = await db.PlatformAccounts.SingleAsync();
        Assert.DoesNotContain("access-secret", stored.ProtectedCredentials!);
        Assert.DoesNotContain("refresh-secret", stored.ProtectedCredentials!);
        Assert.Equal("access-secret", JsonSerializer.Deserialize<Credentials>(new TokenProtector(Options()).Unprotect(stored.ProtectedCredentials!, $"postgen:account:{stored.Id}"))!.AccessToken);
        Assert.Null((await db.PlatformOAuthSessions.SingleAsync()).ProtectedVerifier);
        var json = JsonSerializer.Serialize(await service.List());
        Assert.DoesNotContain("Protected", json); Assert.DoesNotContain("access-secret", json);
        var replay = await Assert.ThrowsAsync<AccountException>(() => service.Callback("facebook", state, "code", null, proof));
        Assert.Equal("PLATFORM_OAUTH_STATE_INVALID", replay.MessageCode);
        provider.Verify(x => x.Exchange(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("browser")]
    [InlineData("platform")]
    [InlineData("expired")]
    [InlineData("denied")]
    public async Task InvalidCallbacksNeverCallProvider(string scenario)
    {
        await using var db = Database(); var clock = new Clock(); var provider = Provider(); var service = Create(db, provider.Object, clock);
        var proof = AccountService.Random(); var connection = await service.Connect("facebook", proof);
        if (scenario == "expired") clock.Now = clock.Now.AddHours(1);
        var exception = await Assert.ThrowsAsync<AccountException>(() => service.Callback(scenario == "platform" ? "zalo" : "facebook",
            State(connection), "code", scenario == "denied" ? "access_denied secret" : null, scenario == "browser" ? AccountService.Random() : proof));
        Assert.Equal(scenario == "denied" ? "PLATFORM_OAUTH_DENIED" : "PLATFORM_OAUTH_STATE_INVALID", exception.MessageCode);
        Assert.DoesNotContain("secret", exception.Message);
        provider.Verify(x => x.Exchange(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(await db.PlatformAccounts.ToListAsync());
    }

    [Fact]
    public async Task DisconnectKeepsIdentityErasesCredentialsAndReconnectReusesId()
    {
        await using var db = Database(); var clock = new Clock(); var service = Create(db, Provider().Object, clock);
        var proof = AccountService.Random(); var c = await service.Connect("facebook", proof);
        var initial = Assert.Single(await service.Callback("facebook", State(c), "code", null, proof));
        var disconnected = await service.Disconnect(initial.Id);
        Assert.Equal("Disabled", disconnected.Status); Assert.Null((await db.PlatformAccounts.SingleAsync()).ProtectedCredentials);
        Assert.Equal(disconnected.Revision, (await service.Disconnect(initial.Id)).Revision);
        clock.Now = clock.Now.AddSeconds(1);
        var reconnect = await service.Reconnect(initial.Id, proof);
        var connected = Assert.Single(await service.Callback("facebook", State(reconnect), "code", null, proof));
        Assert.Equal(initial.Id, connected.Id); Assert.Equal("Connected", connected.Status); Assert.Single(await db.PlatformAccounts.ToListAsync());
    }

    [Fact]
    public async Task ReconnectCannotSwitchAccountOrReviveAfterConcurrentDisconnect()
    {
        await using var db = Database(); var clock = new Clock(); var provider = Provider(); var service = Create(db, provider.Object, clock);
        var proof = AccountService.Random(); var c = await service.Connect("facebook", proof);
        var account = Assert.Single(await service.Callback("facebook", State(c), "code", null, proof));
        var reconnect = await service.Reconnect(account.Id, proof);
        await service.Disconnect(account.Id);
        Assert.Equal("PLATFORM_ACCOUNT_CHANGED", (await Assert.ThrowsAsync<AccountException>(() => service.Callback("facebook", State(reconnect), "code", null, proof))).MessageCode);
        reconnect = await service.Reconnect(account.Id, proof);
        provider.Setup(x => x.Exchange(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([Candidate("other-page")]);
        Assert.Equal("PLATFORM_ACCOUNT_MISMATCH", (await Assert.ThrowsAsync<AccountException>(() => service.Callback("facebook", State(reconnect), "code", null, proof))).MessageCode);
        Assert.Equal("Disabled", (await db.PlatformAccounts.SingleAsync()).Status);
    }

    [Fact]
    public async Task RefreshRotatesZaloPairAndFailedAttemptRequiresReauth()
    {
        await using var db = Database(); var clock = new Clock(); var provider = Provider(); var service = Create(db, provider.Object, clock);
        var proof = AccountService.Random(); var c = await service.Connect("zalo", proof);
        var account = Assert.Single(await service.Callback("zalo", State(c), "code", null, proof));
        provider.Setup(x => x.Refresh(PlatformType.Zalo, "page", It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProviderAccount { ExternalId = "page", Credentials = new() { AccessToken = "rotated-access", RefreshToken = "rotated-refresh" }, ExpiresAt = clock.Now.AddHours(25) });
        Assert.Equal("Connected", (await service.Refresh(account.Id)).Status);
        var stored = await db.PlatformAccounts.SingleAsync();
        var plaintext = new TokenProtector(Options()).Unprotect(stored.ProtectedCredentials!, $"postgen:account:{account.Id}");
        Assert.Contains("rotated-refresh", plaintext); Assert.DoesNotContain("refresh-secret", plaintext);
        provider.Setup(x => x.Refresh(PlatformType.Zalo, "page", It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccountException(504, "PLATFORM_PROVIDER_TIMEOUT", "timeout"));
        Assert.Equal("ReauthRequired", (await service.Refresh(account.Id)).Status);
        Assert.Null((await db.PlatformAccounts.SingleAsync()).ProtectedCredentials);
        await Assert.ThrowsAsync<AccountException>(() => service.Refresh(account.Id));
    }

    [Fact]
    public async Task ExpiredLeaseNeverResendsOneUseToken()
    {
        await using var db = Database(); var clock = new Clock(); var provider = Provider(); var service = Create(db, provider.Object, clock);
        var proof = AccountService.Random(); var c = await service.Connect("zalo", proof);
        var a = Assert.Single(await service.Callback("zalo", State(c), "code", null, proof));
        var row = await db.PlatformAccounts.SingleAsync(); row.RefreshLeaseId = Guid.NewGuid(); row.RefreshLeaseUntil = clock.Now.AddSeconds(-1); await db.SaveChangesAsync();
        Assert.Equal("ReauthRequired", (await service.Refresh(a.Id)).Status);
        provider.Verify(x => x.Refresh(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Credentials>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(PlatformPostStatus.Scheduled)]
    [InlineData(PlatformPostStatus.Publishing)]
    public async Task DisconnectRejectsActivePosts(PlatformPostStatus status)
    {
        await using var db = Database(); var service = Create(db, Provider().Object, new Clock());
        var proof = AccountService.Random(); var c = await service.Connect("facebook", proof);
        var a = Assert.Single(await service.Callback("facebook", State(c), "code", null, proof));
        db.PlatformPosts.Add(new() { Id = Guid.NewGuid(), PlatformAccountId = a.Id, Status = status }); await db.SaveChangesAsync();
        Assert.Equal("PLATFORM_ACCOUNT_IN_USE", (await Assert.ThrowsAsync<AccountException>(() => service.Disconnect(a.Id))).MessageCode);
        Assert.Equal("Connected", (await db.PlatformAccounts.SingleAsync()).Status);
    }

    [Fact]
    public void EncryptionBindsAccountAndSupportsRotationWithoutLosingOldKeys()
    {
        var options = Options(); var protector = new TokenProtector(options);
        var value = protector.Protect("token", "account-a");
        Assert.NotEqual(value, protector.Protect("token", "account-a"));
        Assert.Throws<AccountException>(() => protector.Unprotect(value, "account-b"));
        options.Value.EncryptionKeys["second"] = Convert.ToBase64String(new byte[32].Select((_, i) => (byte)(i + 1)).ToArray());
        options.Value.ActiveEncryptionKeyId = "second";
        Assert.Equal("token", protector.Unprotect(value, "account-a"));
        Assert.StartsWith("second.", protector.Protect("token", "account-a"));
        options.Value.EncryptionKeys.Remove("test");
        Assert.Throws<AccountException>(() => protector.Unprotect(value, "account-a"));
    }

    [Fact]
    public async Task DisabledOrInvalidConfigurationDoesNotCreateSession()
    {
        await using var db = Database(); var options = Options(); options.Value.Meta.Enabled = false;
        await Assert.ThrowsAsync<AccountException>(() => Create(db, Provider().Object, new Clock(), options).Connect("facebook", AccountService.Random()));
        options.Value.Meta.Enabled = true; options.Value.Meta.FacebookCallbackUrl = "https://bad.example/callback?redirect=https://evil.example";
        await Assert.ThrowsAsync<AccountException>(() => Create(db, Provider().Object, new Clock(), options).Connect("facebook", AccountService.Random()));
        Assert.Empty(await db.PlatformOAuthSessions.ToListAsync());
    }

    internal static AppDbContext Database() => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    internal static IOptions<PlatformAccountOptions> Options() => Microsoft.Extensions.Options.Options.Create(new PlatformAccountOptions
    {
        ActiveEncryptionKeyId = "test", EncryptionKeys = new() { ["test"] = Convert.ToBase64String(new byte[32]) },
        Meta = new() { Enabled = true, AppId = "test-app", AppSecret = "test-secret", GraphApiVersion = "v23.0",
            FacebookCallbackUrl = "https://backend.example/api/v1/post-gen/platform-accounts/facebook/callback", InstagramCallbackUrl = "https://backend.example/api/v1/post-gen/platform-accounts/instagram/callback" },
        Zalo = new() { Enabled = true, AppId = "test-oa-app", AppSecret = "test-oa-secret", CallbackUrl = "https://backend.example/api/v1/post-gen/platform-accounts/zalo/callback" }
    });
    internal static AccountService Create(AppDbContext db, IOAuthProvider provider, Clock clock, IOptions<PlatformAccountOptions>? options = null)
    {
        options ??= Options(); var user = new Mock<ICurrentUserService>(); user.Setup(x => x.GetRequiredUserId()).Returns(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        return new(db, user.Object, provider, new(options), options, clock);
    }
    internal static ProviderAccount Candidate(string id = "page") => new() { ExternalId = id, PageId = id, Name = "Test account", Scopes = ["pages_manage_posts"], Credentials = new() { AccessToken = "access-secret", RefreshToken = "refresh-secret" } };
    internal static Mock<IOAuthProvider> Provider()
    {
        var mock = new Mock<IOAuthProvider>();
        mock.Setup(x => x.AuthorizeUrl(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns((PlatformType p, string s, string v) => $"https://provider.example/authorize?state={s}");
        mock.Setup(x => x.Exchange(It.IsAny<PlatformType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([Candidate()]);
        return mock;
    }
    internal static string State(Response.Connection c) => QueryHelpers.ParseQuery(new Uri(c.AuthorizationUrl).Query)["state"].ToString();
    internal sealed class Clock : TimeProvider { public DateTimeOffset Now = new(2026, 9, 14, 12, 0, 0, TimeSpan.Zero); public override DateTimeOffset GetUtcNow() => Now; }
}

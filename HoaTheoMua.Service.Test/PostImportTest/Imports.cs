using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.PlatformAccount;
using HoaTheoMua.Service.PostImport;
using HoaTheoMua.Service.PostPublishing;
using HoaTheoMua.Service.Test.PostPublishingTest;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using Moq;
using Xunit;
using ImportService = HoaTheoMua.Service.PostImport.Service;
using ImportWorker = HoaTheoMua.Service.PostImport.Worker;
using PublishService = HoaTheoMua.Service.PostPublishing.Service;
using PublishWorker = HoaTheoMua.Service.PostPublishing.Worker;
using ImportRequest = HoaTheoMua.Service.PostImport.Request;
using ImportResponse = HoaTheoMua.Service.PostImport.Response;

namespace HoaTheoMua.Service.Test.PostImportTest;

public class Imports
{
    internal sealed class User(Guid id) : ICurrentUserService
    {
        public Guid? UserId => id == Guid.Empty ? null : id;
        public string? Email => null; public string? FullName => null; public string? Avatar => null; public bool IsVerified => true;
        public Guid GetRequiredUserId() => UserId ?? throw new InvalidOperationException("Background worker must not require HttpContext");
    }
    internal sealed class Adapter(PlatformType platform) : IPublishingAdapter
    {
        public PlatformType Platform => platform;
        public int Sends; public int Preflights;
        public IReadOnlyList<string> Validate(PublicationSnapshot s) => s.Images.Length is < 1 or > 10 ? ["PUBLICATION_IMAGE_COUNT_INVALID"] : [];
        public Task<IReadOnlyList<string>> Preflight(PublicationSnapshot s, Credentials c, CancellationToken ct) { Preflights++; return Task.FromResult<IReadOnlyList<string>>([]); }
        public Task<PublicationResult> Execute(PublicationOperation op, PublicationSnapshot s, PublicationCheckpoint cp, Credentials c, string? candidate, CancellationToken ct)
        {
            if (op == PublicationOperation.Prepare) return Task.FromResult(new PublicationResult(PublicationOutcome.Continue) { Checkpoint = new() { ReadyToPublish = true }, RetryAfterSeconds = 1 });
            Sends++; return Task.FromResult(new PublicationResult(PublicationOutcome.Succeeded) { ExternalPostId = "test-published-id", PublishMayHaveOccurred = true });
        }
    }
    internal sealed record Setup(ImportService Service, ImportWorker Worker, PublishWorker Publisher, WorkbookReader Reader,
        Lifecycle.Clock Clock, IOptions<PlatformAccountOptions> Accounts, Guid Actor, Adapter[] Adapters);
    internal static Setup Create(AppDbContext db, Guid? actor = null, IPublishEndpoint? endpoint = null, IPublicationTokenRefresher? refresher = null)
    {
        var who = actor ?? Guid.NewGuid();
        var clock = new Lifecycle.Clock { Now = DateTimeOffset.Parse("2026-10-23T07:00:00Z") };
        var accounts = Options.Create(new PlatformAccountOptions { Zalo = new() { Enabled = true }, Meta = new() { GraphApiVersion = "v25.0" },
            ActiveEncryptionKeyId = "test", EncryptionKeys = new() { ["test"] = Convert.ToBase64String(new byte[32]) } });
        var settings = Options.Create(new PublishingOptions { ZaloEnabled = true, FacebookEnabled = true, InstagramEnabled = true });
        var adapters = new[] { new Adapter(PlatformType.Zalo), new Adapter(PlatformType.Facebook), new Adapter(PlatformType.Instagram) };
        var store = new Store(db, clock);
        var publisher = new PublishWorker(db, store, adapters, settings, new(accounts), clock);
        var publishing = new PublishService(db, store, new User(Guid.Empty), endpoint ?? new Mock<IPublishEndpoint>().Object, adapters, settings, accounts, clock, publisher, refresher);
        var reader = new WorkbookReader(settings, clock);
        return new(new(db, new User(who), reader, clock), new(db, publishing, clock), publisher, reader, clock, accounts, who, adapters);
    }
    internal static async Task<Guid> Account(AppDbContext db, Setup s, PlatformType platform)
    {
        var id = Guid.NewGuid();
        db.PlatformAccounts.Add(new() { Id = id, Platform = platform, ExternalAccountId = "456", PageId = platform == PlatformType.Zalo ? null : "456",
            Status = "Connected", DisplayName = "Test account", ConnectedBy = s.Actor, AccessTokenExpiresAt = s.Clock.Now.AddDays(5), Revision = 1,
            GrantedScopes = ["pages_manage_posts", "pages_read_engagement", "instagram_basic", "instagram_content_publish"],
            ProtectedCredentials = new TokenProtector(s.Accounts).Protect(Store.Json(new Credentials { AccessToken = "test-only-token" }), $"postgen:account:{id}") });
        await db.SaveChangesAsync(); return id;
    }
    internal static Dictionary<string, object?> Post(string code = "HTM-001", string channels = "Zalo OA", object? date = null, string content = "Nội dung hoàn chỉnh\n\nDòng thứ hai") => new()
    { ["Mã bài"] = code, ["Tiêu đề"] = "Bó Mộc Tana", ["Nội dung"] = content, ["Hashtag"] = "#HoaTheoMua", ["Kênh đăng"] = channels, ["Ngày giờ đăng"] = date };
    internal static Dictionary<string, object?> Image(string code = "HTM-001", object? order = null, string url = "https://example.com/one.jpg") => new()
    { ["Mã bài"] = code, ["Thứ tự"] = order ?? 1, ["Đường dẫn ảnh"] = url };
    internal static async Task<byte[]> Book(Dictionary<string, object?>[]? posts = null, Dictionary<string, object?>[]? images = null)
    {
        using var stream = new MemoryStream();
        await MiniExcel.SaveAsAsync(stream, new Dictionary<string, object> { ["Bai_dang"] = posts ?? [Post()], ["Anh"] = images ?? [Image()] }, excelType: ExcelType.XLSX);
        return stream.ToArray();
    }
    internal static async Task<ImportResponse.Summary> Upload(Setup s, byte[] bytes, string? key = null) =>
        await s.Service.Import(new MemoryStream(bytes), "mau.xlsx", key ?? Guid.NewGuid().ToString());
    private static async Task Configure(Setup s, params (PlatformType Platform, Guid Id)[] values) =>
        await s.Service.SaveSettings(new() { Accounts = values.Select(x => new ImportRequest.Account { Platform = x.Platform.ToString().ToLowerInvariant(), AccountId = x.Id }).ToArray() });

    [Fact]
    public async Task LinksArbitraryImageRowsInOrderWithoutClippingAtNine()
    {
        await using var db = Lifecycle.Db(); var s = Create(db);
        var bytes = await Book(images: Enumerable.Range(1, 15).Reverse().Select(i => Image(order: i, url: $"https://example.com/{i}.jpg")).ToArray());
        var result = s.Reader.Read(new MemoryStream(bytes));
        Assert.Empty(result.Issues); var row = Assert.Single(result.Rows); Assert.Empty(row.Issues);
        Assert.Equal(Enumerable.Range(1, 15), row.Input.Images.Select(x => x.Order));
        Assert.Equal("Nội dung hoàn chỉnh\n\nDòng thứ hai", row.Input.Content);
    }
    [Theory]
    [InlineData("25/10/2026 09:00", "2026-10-25T02:00:00Z")]
    [InlineData("2026-10-25 09:00", "2026-10-25T02:00:00Z")]
    public async Task VietnameseTextDatesAreExplicitlyConvertedToUtc(string date, string utc)
    {
        await using var db = Lifecycle.Db(); var s = Create(db);
        var row = Assert.Single(s.Reader.Read(new MemoryStream(await Book(posts: [Post(date: date)]))).Rows);
        Assert.Empty(row.Issues); Assert.Equal(DateTimeOffset.Parse(utc), row.Input.ScheduledAt);
    }
    [Fact]
    public async Task NativeExcelDatesAreConvertedWithoutDependingOnServerTimezone()
    {
        await using var db = Lifecycle.Db(); var s = Create(db);
        var row = Assert.Single(s.Reader.Read(new MemoryStream(await Book(posts: [Post(date: new DateTime(2026, 10, 25, 9, 0, 0))]))).Rows);
        Assert.Empty(row.Issues); Assert.Equal(DateTimeOffset.Parse("2026-10-25T02:00:00Z"), row.Input.ScheduledAt);
    }
    [Theory]
    [InlineData("23/10/2026 13:00")] [InlineData("11/12/2026 09:00")] [InlineData("tomorrow")]
    public async Task InvalidDatesAreRowErrorsAndOtherRowsSurvive(string date)
    {
        await using var db = Lifecycle.Db(); var s = Create(db);
        var parsed = s.Reader.Read(new MemoryStream(await Book([Post(date: date), Post("HTM-002")], [Image(), Image("HTM-002")])));
        Assert.Contains(parsed.Rows[0].Issues, x => x.Code == "IMPORT_SCHEDULE_INVALID"); Assert.Empty(parsed.Rows[1].Issues);
    }
    [Fact]
    public async Task DuplicateCodesUnknownLinksAndDuplicateOrdersAreReportedAtTheirExcelRows()
    {
        await using var db = Lifecycle.Db(); var s = Create(db);
        var parsed = s.Reader.Read(new MemoryStream(await Book([Post(" htm-001 "), Post("HTM-001"), Post("HTM-002")],
            [Image(), Image(order: 1, url: "https://example.com/two.jpg"), Image("UNKNOWN"), Image("HTM-002")])));
        Assert.All(parsed.Rows.Take(2), x => Assert.Contains(x.Issues, e => e.Code == "IMPORT_CODE_DUPLICATE"));
        Assert.Contains(parsed.Rows[0].Issues, x => x.Sheet == "Anh" && x.RowNumber == 3 && x.Code == "IMPORT_IMAGE_ORDER_INVALID");
        Assert.Equal(4, Assert.Single(parsed.Issues).RowNumber); Assert.Empty(parsed.Rows[2].Issues);
    }
    [Theory]
    [InlineData("http://127.0.0.1/image.jpg")] [InlineData("https://example.com/private.jpg?access_token=secret", false)]
    [InlineData("file:///tmp/photo.jpg")] [InlineData("http://169.254.169.254/latest/meta-data")]
    public async Task InvalidImageUrlsNeverBecomeAcceptedJobs(string url, bool invalid = true)
    {
        await using var db = Lifecycle.Db(); var s = Create(db);
        var parsed = s.Reader.Read(new MemoryStream(await Book(images: [Image(url: url)])));
        Assert.Equal(invalid, parsed.Rows[0].Issues.Any(x => x.Code == "IMPORT_IMAGE_URL_INVALID"));
    }
    [Fact]
    public async Task FormulasAreRejectedWithoutEvaluatingCachedValues()
    {
        await using var db = Lifecycle.Db(); var s = Create(db); var original = await Book();
        using var stream = new MemoryStream(); stream.Write(original); stream.Position = 0;
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Update, true))
        {
            var entry = zip.GetEntry("xl/worksheets/sheet1.xml")!; XDocument doc;
            using (var input = entry.Open()) doc = XDocument.Load(input);
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            doc.Descendants(ns + "c").Single(x => (string?)x.Attribute("r") == "C2").AddFirst(new XElement(ns + "f", "WEBSERVICE(\"https://example.com\")"));
            entry.Delete(); using var output = zip.CreateEntry("xl/worksheets/sheet1.xml").Open(); doc.Save(output);
        }
        stream.Position = 0;
        Assert.Contains(Assert.Single(s.Reader.Read(stream).Rows).Issues, x => x.Code == "IMPORT_FORMULA_NOT_ALLOWED");
    }
    [Fact]
    public async Task SameRequestKeyReturnsOneBatchAndRejectsDifferentFiles()
    {
        await using var db = Lifecycle.Db(); var s = Create(db); var bytes = await Book();
        var first = await Upload(s, bytes, "same-request"); var again = await Upload(s, bytes, "same-request");
        Assert.Equal(first.Id, again.Id); Assert.Single(await db.PostImportBatches.ToListAsync());
        var changed = await Book(posts: [Post(content: "Changed")]);
        var error = await Assert.ThrowsAsync<ImportException>(() => Upload(s, changed, "same-request"));
        Assert.Equal("IMPORT_IDEMPOTENCY_CONFLICT", error.MessageCode);
        Assert.Empty(await db.PostPublications.ToListAsync()); Assert.All(s.Adapters, x => Assert.Equal(0, x.Sends));
    }
    [Fact]
    public async Task BackgroundQueueUsesSavedAccountSnapshotAndPreservesManualContent()
    {
        await using var db = Lifecycle.Db(); var s = Create(db); var first = await Account(db, s, PlatformType.Zalo); var second = await Account(db, s, PlatformType.Zalo);
        await Configure(s, (PlatformType.Zalo, first)); var batch = await Upload(s, await Book());
        await Configure(s, (PlatformType.Zalo, second));
        Assert.True(await s.Worker.ProcessOne(batch.Id));
        var job = Assert.Single(await db.PostPublications.ToListAsync());
        Assert.Equal(first, job.PlatformAccountId); Assert.Equal(s.Actor, job.RequestedBy); Assert.Equal("Queued", job.Status);
        var snapshot = Store.Read<PublicationSnapshot>(job.Snapshot);
        Assert.Equal("Nội dung hoàn chỉnh\n\nDòng thứ hai", snapshot.Content);
        Assert.All(await db.GeneratedPosts.ToListAsync(), x => { Assert.Equal(ContentSource.Manual, x.Source); Assert.Null(x.AiModel); });
        Assert.All(await db.GeneratedPostImages.ToListAsync(), x => { Assert.Equal("ImportedUrl", x.StorageProvider); Assert.Null(x.AiModel); });
        Assert.All(s.Adapters, x => { Assert.Equal(0, x.Sends); Assert.Equal(0, x.Preflights); });
    }
    [Fact]
    public async Task OneInvalidRowAndMissingAccountDoNotBlockValidChannels()
    {
        await using var db = Lifecycle.Db(); var s = Create(db); var account = await Account(db, s, PlatformType.Facebook);
        await Configure(s, (PlatformType.Facebook, account));
        var batch = await Upload(s, await Book([Post(content: ""), Post("HTM-002", "Zalo OA, Facebook")], [Image(), Image("HTM-002")]));
        await s.Worker.ProcessOne(batch.Id);
        var detail = await s.Service.Get(batch.Id);
        Assert.Equal("Invalid", detail.Rows[0].Status);
        Assert.Contains(detail.Rows[1].Channels, x => x.Platform == "zalo" && x.ErrorCode == "IMPORT_ACCOUNT_REQUIRED");
        Assert.Contains(detail.Rows[1].Channels, x => x.Platform == "facebook" && x.Status == "Queued");
        Assert.Single(await db.PostPublications.ToListAsync());
    }
    [Fact]
    public async Task TooManyImagesAreRejectedPerChannelWithoutDroppingImagesOrSendingAnything()
    {
        await using var db = Lifecycle.Db(); var s = Create(db); var account = await Account(db, s, PlatformType.Zalo);
        await Configure(s, (PlatformType.Zalo, account));
        var batch = await Upload(s, await Book(images: Enumerable.Range(1, 15).Select(i => Image(order:i,url:$"https://example.com/{i}.jpg")).ToArray()));
        await s.Worker.ProcessOne(batch.Id);
        var detail = await s.Service.Get(batch.Id);
        Assert.Equal("PUBLICATION_IMAGE_COUNT_INVALID", Assert.Single(detail.Rows[0].Channels).ErrorCode);
        Assert.Equal(15, await db.BasePostImages.CountAsync()); Assert.Empty(await db.PostPublications.ToListAsync());
    }
    [Fact]
    public async Task ReimportOfPublishedChannelAttachesExistingJobWithoutCreatingOrPublishingAgain()
    {
        await using var db = Lifecycle.Db(); var s = Create(db); var account = await Account(db, s, PlatformType.Zalo);
        await Configure(s, (PlatformType.Zalo, account)); var bytes = await Book();
        var first = await Upload(s, bytes); await s.Worker.ProcessOne(first.Id);
        var job = await db.PostPublications.SingleAsync();
        await s.Publisher.Process(job.Id, job.Revision, default); s.Clock.Now = s.Clock.Now.AddMinutes(1);
        await s.Publisher.Process(job.Id, (await db.PostPublications.AsNoTracking().SingleAsync()).Revision, default);
        Assert.Equal("Succeeded", (await db.PostPublications.AsNoTracking().SingleAsync()).Status);
        var second = await Upload(s, bytes); await s.Worker.ProcessOne(second.Id);
        var result = await s.Service.Get(second.Id);
        Assert.Equal(1, result.Batch.Existing); Assert.Equal(1, result.Batch.Succeeded);
        Assert.Single(await db.BasePosts.ToListAsync()); Assert.Single(await db.ImportedPostIdentities.ToListAsync()); Assert.Single(await db.PostPublications.ToListAsync());
        Assert.Equal(1, s.Adapters[0].Sends);
    }
    [Fact]
    public async Task ScheduledJobsBlockChangedContentButARejectedJobCanBeCorrectedInPlace()
    {
        await using var db = Lifecycle.Db(); var s = Create(db); var account = await Account(db, s, PlatformType.Zalo);
        await Configure(s, (PlatformType.Zalo, account));
        var first = await Upload(s, await Book(posts: [Post(date: "25/10/2026 09:00")])); await s.Worker.ProcessOne(first.Id);
        var job = await db.PostPublications.SingleAsync(); var oldSnapshot = job.Snapshot;
        Assert.Equal(DateTimeOffset.Parse("2026-10-25T02:00:00Z"), job.ScheduledAt);
        var second = await Upload(s, await Book(posts: [Post(content: "Changed")])); await s.Worker.ProcessOne(second.Id);
        Assert.Contains((await s.Service.Get(second.Id)).Rows[0].Issues, x => x.Code == "IMPORT_CONTENT_CHANGED");
        Assert.Equal(oldSnapshot, (await db.PostPublications.SingleAsync()).Snapshot);
        job = await db.PostPublications.SingleAsync(); job.Status = "Failed"; job.PublishMayHaveOccurred = false; await db.SaveChangesAsync();
        var third = await Upload(s, await Book(posts: [Post(content: "Changed")], images:[Image(url:"https://example.com/fixed.jpg")]));
        await s.Worker.ProcessOne(third.Id);
        Assert.Single(await db.BasePosts.ToListAsync()); Assert.Equal(2, await db.PostPublications.CountAsync());
        Assert.Equal(oldSnapshot, (await db.PostPublications.SingleAsync(x => x.Id == job.Id)).Snapshot);
        Assert.Equal("Changed", (await db.BasePosts.SingleAsync()).Content);
    }
    [Fact]
    public async Task ReplayAfterJobCommitBeforeChannelCheckpointDoesNotCreateAnotherJob()
    {
        await using var db = Lifecycle.Db(); var s = Create(db); var account = await Account(db, s, PlatformType.Zalo);
        await Configure(s, (PlatformType.Zalo, account)); var batch = await Upload(s, await Book()); await s.Worker.ProcessOne(batch.Id);
        var channel = await db.PostImportChannels.SingleAsync(); channel.Status = "Pending"; channel.PublicationId = null;
        (await db.PostImportRows.SingleAsync()).Status = "Queued";
        var record = await db.PostImportBatches.SingleAsync(); record.Status = "Processing"; record.LeaseId = Guid.NewGuid(); record.LeaseUntil = s.Clock.Now.AddMinutes(-1); record.ProcessedAt = null;
        await db.SaveChangesAsync(); await s.Worker.ProcessOne(batch.Id);
        Assert.Single(await db.PostPublications.ToListAsync()); Assert.NotNull((await db.PostImportChannels.SingleAsync()).PublicationId);
    }
    [Fact]
    public async Task BatchAccessAndDefaultsAreScopedToTheAuthenticatedUser()
    {
        await using var db = Lifecycle.Db(); var first = Create(db); var second = Create(db);
        var account = await Account(db, first, PlatformType.Zalo); await Configure(first, (PlatformType.Zalo, account));
        var batch = await Upload(first, await Book());
        Assert.Empty((await second.Service.GetSettings()).Accounts); Assert.Empty(await second.Service.List());
        Assert.Equal(404, (await Assert.ThrowsAsync<ImportException>(() => second.Service.Get(batch.Id))).StatusCode);
        Assert.Equal(404, (await Assert.ThrowsAsync<ImportException>(() => second.Service.Export(batch.Id))).StatusCode);
    }
    [Fact]
    public async Task ResultsExportIsARealWorkbookWithRowAndChannelErrors()
    {
        await using var db = Lifecycle.Db(); var s = Create(db);
        var batch = await Upload(s, await Book([Post(content: ""), Post("HTM-002")], [Image(),Image("HTM-002")]));
        await s.Worker.ProcessOne(batch.Id);
        using var stream = new MemoryStream(await s.Service.Export(batch.Id));
        var records = MiniExcel.Query(stream, useHeaderRow:true).Cast<IDictionary<string, object>>().ToArray();
        Assert.Contains(records, x => x["Mã lỗi"]?.ToString() == "IMPORT_CONTENT_INVALID");
        Assert.Contains(records, x => x["Mã lỗi"]?.ToString() == "IMPORT_ACCOUNT_REQUIRED");
        Assert.All(records, x => Assert.True(x.ContainsKey("Dòng")));
    }

    [Fact]
    public async Task ExpiredZaloTokenRefreshesInBackgroundAndBusyRefreshDoesNotConsumeRetryBudget()
    {
        await using var db = Lifecycle.Db(); var refresher = new Mock<IPublicationTokenRefresher>();
        var s = Create(db, refresher: refresher.Object); var account = await Account(db, s, PlatformType.Zalo);
        await Configure(s, (PlatformType.Zalo, account));
        var stored = await db.PlatformAccounts.SingleAsync(); stored.AccessTokenExpiresAt = s.Clock.Now.AddMinutes(-1); await db.SaveChangesAsync();
        var busy = true;
        refresher.Setup(x => x.RefreshForPublication(account, It.IsAny<CancellationToken>())).Returns(async () =>
        {
            if (busy) throw new AccountException(409, "PLATFORM_REFRESH_IN_PROGRESS", "Another worker holds the refresh lease.");
            var current = await db.PlatformAccounts.SingleAsync(); current.AccessTokenExpiresAt = s.Clock.Now.AddHours(4); await db.SaveChangesAsync();
        });
        var batch = await Upload(s, await Book());
        Assert.False(await s.Worker.ProcessOne(batch.Id)); Assert.Equal(0, (await db.PostImportRows.SingleAsync()).Attempts);
        Assert.Empty(await db.PostPublications.ToListAsync());
        busy = false; Assert.True(await s.Worker.ProcessOne(batch.Id));
        Assert.Equal("Queued", (await db.PostPublications.SingleAsync()).Status);
        refresher.Verify(x => x.RefreshForPublication(account, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}

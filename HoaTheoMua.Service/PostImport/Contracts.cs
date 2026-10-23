using System.ComponentModel.DataAnnotations;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;

namespace HoaTheoMua.Service.PostImport;

public sealed class ImportException(int status, string code, string message) : AppException("Post import", status, code, message);
public record ImportIssue(string Sheet, int RowNumber, string Code, string Message);
public record ImportImage(int Order, string Url);
public record ImportInput(string Code, string Title, string Content, string? Hashtag,
    PlatformType[] Platforms, DateTimeOffset? ScheduledAt, ImportImage[] Images);
public record ParsedRow(int RowNumber, ImportInput Input, IReadOnlyList<ImportIssue> Issues);
public record ParsedWorkbook(IReadOnlyList<ParsedRow> Rows, IReadOnlyList<ImportIssue> Issues);

public class Request
{
    public class Account
    {
        [Required] public string Platform { get; set; } = "";
        public Guid AccountId { get; set; }
    }
    public class Settings
    {
        [Required, MaxLength(3)] public Account[] Accounts { get; set; } = [];
    }
}

public class Response
{
    public record Account(string Platform, Guid AccountId);
    public record Settings(IReadOnlyList<Account> Accounts);
    public record Summary(Guid Id, string FileName, DateTimeOffset CreatedAt, string Status, int TotalRows,
        int ProcessedRows, int InvalidRows, int TotalChannels, int Succeeded, int Failed, int Pending,
        int Scheduled, int NeedsReview, int Existing);
    public record Channel(string Platform, string Status, string? ErrorCode, Guid? PublicationId,
        string? ExternalPostId, string? ExternalPostUrl, bool Existing);
    public record Row(int RowNumber, string Code, string Title, string Status, Guid? BasePostId,
        IReadOnlyList<ImportIssue> Issues, IReadOnlyList<Channel> Channels);
    public record Detail(Summary Batch, IReadOnlyList<ImportIssue> Issues, IReadOnlyList<Row> Rows,
        int PageIndex, int PageSize, int TotalRows);
}

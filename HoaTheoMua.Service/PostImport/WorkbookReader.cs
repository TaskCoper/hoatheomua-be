using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.PostPublishing;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.PostImport;

// Read OpenXML values only. No formulas, macros, external links, or embedded images are executed.
public sealed class WorkbookReader(IOptions<PublishingOptions> publishing, TimeProvider clock)
{
    public const int MaxFileBytes = 10 * 1024 * 1024;
    public const int MaxPosts = 500;
    public const int MaxImages = 10000;
    private static readonly XNamespace S = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly string[] PostColumns = ["Mã bài", "Tiêu đề", "Nội dung", "Hashtag", "Kênh đăng", "Ngày giờ đăng"];
    private static readonly string[] ImageColumns = ["Mã bài", "Thứ tự", "Đường dẫn ảnh"];
    private static readonly string[] PostOptions = ["AI nội dung"];
    private static readonly string[] ImageOptions = ["Nền tảng", "Xử lý ảnh", "Tỉ lệ đầu ra", "Yêu cầu AI"];
    private sealed record Value(string Text, string Type, bool Formula);
    private sealed record ExcelRow(int Number, Dictionary<string, Value> Values);
    public static string NormalizeCode(string code) => code.Trim().Normalize(NormalizationForm.FormKC).ToUpperInvariant();
    private static ImportException Invalid(string message) => new(400, "IMPORT_WORKBOOK_INVALID", message);

    public ParsedWorkbook Read(Stream stream, CancellationToken ct = default)
    {
        try
        {
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            if (zip.Entries.Count > 512 || zip.Entries.Sum(x => x.Length) > 64L * 1024 * 1024
                || zip.Entries.Any(x => x.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase)))
                throw Invalid("File quá lớn sau giải nén hoặc chứa macro. Chỉ sử dụng mẫu .xlsx.");
            var workbook = Load(zip, "xl/workbook.xml");
            var date1904 = (string?)workbook.Root?.Element(S + "workbookPr")?.Attribute("date1904") is "1" or "true";
            var relations = Load(zip, "xl/_rels/workbook.xml.rels").Root!.Elements()
                .ToDictionary(x => (string)x.Attribute("Id")!, x => x);
            var shared = zip.GetEntry("xl/sharedStrings.xml") == null ? []
                : Load(zip, "xl/sharedStrings.xml").Root!.Elements(S + "si")
                    .Select(x => string.Concat(x.Descendants(S + "t").Select(t => t.Value))).ToArray();
            List<ExcelRow> ReadSheet(string name, string[] columns, int limit, string[]? optional = null)
            {
                var sheet = workbook.Descendants(S + "sheet").SingleOrDefault(x => (string?)x.Attribute("name") == name)
                    ?? throw Invalid($"Thiếu sheet {name}. Sử dụng file mẫu Excel gồm Bai_dang và Anh.");
                var id = (string?)sheet.Attribute(R + "id");
                if (id == null || !relations.TryGetValue(id, out var relation) || (string?)relation.Attribute("TargetMode") == "External")
                    throw Invalid("Liên kết sheet không hợp lệ.");
                var target = (string?)relation.Attribute("Target") ?? "";
                var path = target.StartsWith('/') ? target.TrimStart('/') : "xl/" + target;
                if (path.Contains("..", StringComparison.Ordinal) || path.Contains('\\')) throw Invalid("Đường dẫn sheet không hợp lệ.");
                var rows = Load(zip, path).Descendants(S + "sheetData").Elements(S + "row");
                Dictionary<string, string>? header = null;
                var result = new List<ExcelRow>();
                foreach (var row in rows)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!int.TryParse((string?)row.Attribute("r"), out var number)) throw Invalid("Dòng Excel không có số dòng hợp lệ.");
                    var cells = new Dictionary<string, Value>(StringComparer.Ordinal);
                    foreach (var cell in row.Elements(S + "c"))
                    {
                        var reference = (string?)cell.Attribute("r") ?? "";
                        var column = new string(reference.TakeWhile(char.IsLetter).ToArray());
                        var type = (string?)cell.Attribute("t") ?? "n";
                        var value = type == "inlineStr" ? string.Concat(cell.Descendants(S + "t").Select(t => t.Value)) : cell.Element(S + "v")?.Value ?? "";
                        if (type == "s")
                        {
                            if (!int.TryParse(value, out var index) || index < 0 || index >= shared.Length) throw Invalid("Giá trị chuỗi Excel không hợp lệ.");
                            value = shared[index];
                        }
                        if (!cells.TryAdd(column, new(value, type, cell.Element(S + "f") != null))) throw Invalid("Có ô Excel bị trùng địa chỉ.");
                    }
                    if (number == 1)
                    {
                        header = cells.Where(x => !string.IsNullOrWhiteSpace(x.Value.Text)).ToDictionary(x => x.Key, x => x.Value.Text.Trim());
                        if (header.Values.Distinct(StringComparer.Ordinal).Count() != header.Count || columns.Any(c => !header.Values.Contains(c))
                            || header.Values.Any(c => !columns.Contains(c) && !(optional ?? []).Contains(c)) || cells.Values.Any(x => x.Formula))
                            throw Invalid($"Tên cột sheet {name} chưa đúng. Dùng nguyên các cột trong mẫu Excel.");
                        continue;
                    }
                    if (header == null) throw Invalid($"Sheet {name} thiếu tiêu đề ở dòng 1.");
                    if (cells.Values.All(x => string.IsNullOrWhiteSpace(x.Text) && !x.Formula)) continue;
                    if (number < 2 || result.Count >= limit) throw Invalid($"Sheet {name} vượt giới hạn {limit} dòng dữ liệu mỗi file.");
                    result.Add(new(number, header.ToDictionary(x => x.Value, x => cells.GetValueOrDefault(x.Key) ?? new Value("", "inlineStr", false))));
                }
                if (header == null) throw Invalid($"Sheet {name} thiếu tiêu đề.");
                return result;
            }
            var posts = ReadSheet("Bai_dang", PostColumns, MaxPosts, PostOptions);
            var images = ReadSheet("Anh", ImageColumns, MaxImages, ImageOptions);
            if (posts.Count == 0) throw Invalid("Sheet Bai_dang chưa có bài nào. Bắt đầu nhập từ dòng 2.");
            var rows = new List<ParsedRow>(); var fileIssues = new List<ImportIssue>();
            var imageGroups = images.GroupBy(x => NormalizeCode(x.Values["Mã bài"].Text)).ToDictionary(x => x.Key, x => x.ToArray());
            var postCodes = posts.Select(x => NormalizeCode(x.Values["Mã bài"].Text)).ToHashSet();
            foreach (var orphan in images.Where(x => !postCodes.Contains(NormalizeCode(x.Values["Mã bài"].Text)) || string.IsNullOrWhiteSpace(x.Values["Mã bài"].Text)))
                fileIssues.Add(new("Anh", orphan.Number, "IMPORT_IMAGE_POST_UNKNOWN", "Mã bài của ảnh không có trong sheet Bai_dang."));
            var duplicates = posts.GroupBy(x => NormalizeCode(x.Values["Mã bài"].Text)).Where(x => x.Count() > 1).Select(x => x.Key).ToHashSet();
            foreach (var post in posts)
            {
                var issues = new List<ImportIssue>();
                void Error(string code, string message) => issues.Add(new("Bai_dang", post.Number, code, message));
                var code = NormalizeCode(post.Values["Mã bài"].Text);
                var title = post.Values["Tiêu đề"].Text;
                var content = post.Values["Nội dung"].Text;
                var hashtag = post.Values["Hashtag"].Text;
                var aiText = post.Values.GetValueOrDefault("AI nội dung")?.Text.Trim().ToLowerInvariant() ?? "";
                var useAiContent = aiText is "có" or "true" or "1";
                if (aiText is not ("" or "có" or "không" or "true" or "false" or "1" or "0"))
                    Error("IMPORT_AI_CONTENT_INVALID", "AI nội dung chỉ nhận Có / Không (hoặc ô checkbox TRUE / FALSE).");
                if (code.Length is < 1 or > 100 || code.Any(char.IsControl)) Error("IMPORT_CODE_INVALID", "Mã bài bắt buộc, tối đa 100 ký tự và không có ký tự điều khiển.");
                if (duplicates.Contains(code)) Error("IMPORT_CODE_DUPLICATE", "Mã bài xuất hiện nhiều lần trong sheet Bai_dang.");
                if (string.IsNullOrWhiteSpace(title) || title.Length > 500) Error("IMPORT_TITLE_INVALID", "Tiêu đề bắt buộc và tối đa 500 ký tự; giới hạn từng kênh được kiểm tra riêng.");
                if (string.IsNullOrWhiteSpace(content) || content.Length > 100000) Error("IMPORT_CONTENT_INVALID", "Nội dung bắt buộc và tối đa 100.000 ký tự.");
                if (hashtag.Length > 1000 || hashtag.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Any(x => !x.StartsWith('#')))
                    Error("IMPORT_HASHTAG_INVALID", "Hashtag tối đa 1.000 ký tự; mỗi hashtag phải bắt đầu bằng #.");
                if (post.Values.Values.Any(x => x.Formula || x.Type == "e")) Error("IMPORT_FORMULA_NOT_ALLOWED", "Không sử dụng công thức hoặc ô lỗi trong dữ liệu nhập.");
                var platforms = new List<PlatformType>();
                foreach (var name in post.Values["Kênh đăng"].Text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    var platform = name.ToLowerInvariant() switch { "zalo oa" or "zalo" => (PlatformType?)PlatformType.Zalo, "facebook" => PlatformType.Facebook, "instagram" => PlatformType.Instagram, _ => null };
                    if (platform == null || platforms.Contains(platform.Value)) Error("IMPORT_CHANNEL_INVALID", "Chọn các kênh Zalo OA, Facebook, Instagram, phân cách bằng dấu phẩy và không lặp lại.");
                    else platforms.Add(platform.Value);
                }
                if (platforms.Count == 0) Error("IMPORT_CHANNEL_INVALID", "Bài phải có ít nhất một kênh đăng.");
                DateTimeOffset? scheduled = null;
                var date = post.Values["Ngày giờ đăng"];
                if (!string.IsNullOrWhiteSpace(date.Text))
                {
                    if (!TryDate(date.Text, date.Type, date1904, out var time)) Error("IMPORT_SCHEDULE_INVALID", "Ngày giờ phải có dạng dd/MM/yyyy HH:mm theo giờ Việt Nam hoặc là ô ngày giờ Excel.");
                    else if (time < clock.GetUtcNow().AddSeconds(publishing.Value.MinScheduleLeadSeconds) || time > clock.GetUtcNow().AddDays(publishing.Value.MaxScheduleDays))
                        Error("IMPORT_SCHEDULE_INVALID", $"Lịch phải cách hiện tại ít nhất {publishing.Value.MinScheduleLeadSeconds} giây và không quá {publishing.Value.MaxScheduleDays} ngày.");
                    else scheduled = time;
                }
                var ordered = new List<ImportImage>(); var usedOrders = new HashSet<(PlatformType, int)>();
                foreach (var image in imageGroups.GetValueOrDefault(code) ?? [])
                {
                    void ImageError(string error, string message) => issues.Add(new("Anh", image.Number, error, message));
                    var orderText = image.Values["Thứ tự"].Text;
                    var url = image.Values["Đường dẫn ảnh"].Text.Trim();
                    var channelText = image.Values.GetValueOrDefault("Nền tảng")?.Text.Trim().ToLowerInvariant() ?? "";
                    PlatformType? imagePlatform = channelText switch { "facebook" => PlatformType.Facebook, "instagram" => PlatformType.Instagram, "zalo" or "zalo oa" => PlatformType.Zalo, _ => null };
                    if (image.Values.ContainsKey("Nền tảng") && (imagePlatform == null || !platforms.Contains(imagePlatform.Value)))
                        ImageError("IMPORT_IMAGE_CHANNEL_INVALID", "Chọn một nền tảng thuộc Kênh đăng của bài. Muốn dùng ảnh cho nhiều nền tảng, thêm một dòng cho mỗi nền tảng.");
                    var mode = image.Values.GetValueOrDefault("Xử lý ảnh")?.Text.Trim().ToLowerInvariant() ?? "";
                    var useAi = mode == "xử lý bằng ai";
                    if (useAi && imagePlatform == null) ImageError("IMPORT_IMAGE_CHANNEL_INVALID", "Ảnh AI cần chọn rõ một nền tảng.");
                    if (mode is not ("" or "giữ nguyên" or "xử lý bằng ai"))
                        ImageError("IMPORT_IMAGE_MODE_INVALID", "Xử lý ảnh chỉ nhận Giữ nguyên / Xử lý bằng AI.");
                    var ratioText = image.Values.GetValueOrDefault("Tỉ lệ đầu ra")?.Text.Trim() ?? "";
                    AspectRatioType? ratio = ratioText switch { "1:1" => AspectRatioType.Square1x1, "4:5" => AspectRatioType.Portrait4x5,
                        "3:4" => AspectRatioType.Portrait3x4, "9:16" => AspectRatioType.Story9x16, "16:9" => AspectRatioType.Landscape16x9, _ => null };
                    if (useAi && ratio == null || !useAi && ratioText is not ("" or "Gốc"))
                        ImageError("IMPORT_IMAGE_RATIO_INVALID", "Ảnh AI cần tỉ lệ 1:1, 4:5, 3:4, 9:16 hoặc 16:9. Giữ nguyên chỉ nhận Gốc hoặc để trống; hệ thống không tự cắt ảnh.");
                    var instruction = image.Values.GetValueOrDefault("Yêu cầu AI")?.Text.Trim();
                    if (instruction?.Length > 4000 || !useAi && !string.IsNullOrWhiteSpace(instruction))
                        ImageError("IMPORT_IMAGE_INSTRUCTION_INVALID", "Yêu cầu AI tối đa 4.000 ký tự và chỉ dùng khi chọn Xử lý bằng AI.");
                    if (image.Values.Values.Any(x => x.Formula || x.Type == "e")) ImageError("IMPORT_FORMULA_NOT_ALLOWED", "Không sử dụng công thức hoặc ô lỗi trong dữ liệu ảnh.");
                    if (!decimal.TryParse(orderText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var order)
                        || order < 1 || order > int.MaxValue || order != decimal.Truncate(order) || (imagePlatform.HasValue ? new[] { imagePlatform.Value } : platforms.ToArray()).Any(p => !usedOrders.Add((p, (int)order))))
                        ImageError("IMPORT_IMAGE_ORDER_INVALID", "Thứ tự ảnh phải là số nguyên dương và không trùng trong cùng bài và nền tảng.");
                    else if (!PublishingMediaClient.Url(url)) ImageError("IMPORT_IMAGE_URL_INVALID", "Cần đường dẫn ảnh http/https công khai, tối đa 2.048 ký tự và không trỏ đến địa chỉ nội bộ.");
                    else ordered.Add(new((int)order, url, imagePlatform, ratio, useAi, instruction));
                }
                foreach (var platform in platforms)
                    if (!ordered.Any(x => x.Platform == null || x.Platform == platform))
                        Error("IMPORT_CHANNEL_IMAGES_REQUIRED", $"Thiếu ảnh cho nền tảng {platform}.");
                if (ordered.Count(x => x.UseAi) > 12)
                    Error("IMPORT_AI_IMAGE_LIMIT", "Mỗi bài tối đa 12 dòng ảnh dùng AI.");
                if (ordered.Count == 0) Error("IMPORT_IMAGES_REQUIRED", "Bài cần ít nhất một ảnh hợp lệ trong sheet Anh, liên kết đúng Mã bài.");
                rows.Add(new(post.Number, new(code.Length > 100 ? code[..100] : code, title.Length > 500 ? title[..500] : title,
                    content, hashtag, platforms.ToArray(), scheduled, ordered.OrderBy(x => x.Order).ToArray(), useAiContent), issues));
            }
            return new(rows, fileIssues);
        }
        catch (Exception ex) when (ex is InvalidDataException or XmlException or ArgumentException or OverflowException or InvalidOperationException or KeyNotFoundException)
        { throw Invalid("Không đọc được cấu trúc .xlsx. Hãy lưu lại bằng Excel và sử dụng đúng mẫu Excel."); }
    }

    private static XDocument Load(ZipArchive zip, string path)
    {
        var entry = zip.GetEntry(path) ?? throw Invalid("File Excel thiếu thành phần bắt buộc.");
        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 64L * 1024 * 1024 });
        return XDocument.Load(reader);
    }
    private static bool TryDate(string value, string type, bool date1904, out DateTimeOffset time)
    {
        time = default; DateTime local;
        try
        {
            if (type == "n" && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial)) local = DateTime.FromOADate(serial + (date1904 ? 1462 : 0));
            else if (!DateTime.TryParseExact(value.Trim(), ["dd/MM/yyyy HH:mm", "dd/MM/yyyy H:mm", "yyyy-MM-dd HH:mm", "yyyy-MM-dd'T'HH:mm:ss"], CultureInfo.InvariantCulture, DateTimeStyles.None, out local)) return false;
            time = new DateTimeOffset(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeSpan.FromHours(7)).ToUniversalTime();
            return true;
        }
        catch (ArgumentException) { return false; }
    }
}

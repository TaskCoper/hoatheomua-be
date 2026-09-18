using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.AiService;

public partial class Service
{
    public async Task<Response.GeneratedImage> GenerateImage(Request.ImageInput input, CancellationToken cancellationToken = default)
    {
        var options = new AiOption();
        configuration.GetSection(nameof(AiOption)).Bind(options);
        if (!Uri.TryCreate(options.ChatCompletionsUrl, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("https" or "http") || !string.IsNullOrEmpty(uri.UserInfo)
            || string.IsNullOrWhiteSpace(options.Model) || options.TimeoutSeconds is < 1 or > 180)
            throw new ServerException("AI service configuration is invalid");
        // A shared image must not depend on which platform requested this pair first.
        var parts = new List<Request.ContentPart>();
        if (input.OriginalImageUrl != null)
        {
            parts.Add(new("text", "Ảnh sản phẩm gốc để đối chiếu, giữ nguyên chi tiết nhận diện:"));
            parts.Add(new("image_url", ImageUrl: new(input.OriginalImageUrl)));
        }
        parts.Add(new("text", input.OriginalImageUrl == null ? "Ảnh sản phẩm gốc cần dựng lại:" : "Ảnh phiên bản được chọn để chỉnh sửa tiếp:"));
        parts.Add(new("image_url", ImageUrl: new(input.SourceImageUrl)));
        parts.Add(new("text", input.Instruction));
        if (!string.IsNullOrWhiteSpace(input.Feedback)) parts.Add(new("text", "Yêu cầu chỉnh sửa lần này: " + input.Feedback));
        parts.Add(new("text", $"Tạo một ảnh mới từ ảnh trên, tỷ lệ {input.RatioCode}, kích thước mục tiêu {input.PixelWidth}x{input.PixelHeight} pixel. " +
            "Giữ nguyên sản phẩm và mọi chi tiết nhận diện. Chỉ điều chỉnh bố cục hoặc mở rộng nền; không thay sản phẩm, không thêm chữ hay logo. " +
            "Thực hiện tạo ảnh ngay và trả đúng một ảnh dưới dạng Markdown ![generated image](URL)."));
        var payload = new Request.ChatCompletion(options.Model, new[] { new Request.Message("user", parts) });
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = JsonContent.Create(payload, options: JsonOptions) };
        if (!string.IsNullOrWhiteSpace(options.ApiKey)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));
        var watch = Stopwatch.StartNew();
        using var json = await SendAsync(request, timeout.Token, cancellationToken);
        try
        {
            var root = json.RootElement;
            var choices = root.GetProperty("choices");
            if (choices.GetArrayLength() != 1 || choices[0].GetProperty("finish_reason").GetString() != "stop") throw InvalidResponse();
            var content = choices[0].GetProperty("message").GetProperty("content").GetString();
            if (string.IsNullOrWhiteSpace(content)) throw InvalidResponse();
            var matches = Regex.Matches(content, @"!\[[^\]\r\n]*\]\((?<url>https?://[^\s<>()]+)\)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
            if (matches.Count != 1) throw InvalidResponse();
            var url = matches[0].Groups["url"].Value;
            if (url.Length > 2048 || !Uri.TryCreate(url, UriKind.Absolute, out var imageUri)
                || imageUri.Scheme is not ("https" or "http") || string.IsNullOrWhiteSpace(imageUri.Host)
                || !string.IsNullOrEmpty(imageUri.UserInfo)) throw InvalidResponse();
            var model = OptionalString(root, "model"); var requestId = OptionalString(root, "id");
            if (model?.Length > 100 || requestId?.Length > 200) throw InvalidResponse();
            return new(url, model, requestId, (int)watch.ElapsedMilliseconds);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or RegexMatchTimeoutException)
        { throw InvalidResponse(); }
    }
}

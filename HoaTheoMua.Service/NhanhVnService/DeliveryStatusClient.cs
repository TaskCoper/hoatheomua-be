using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.NhanhVnService;

public interface INhanhVnDeliveryStatusClient
{
    Task MarkDeliveredAsync(int orderId, CancellationToken cancellationToken = default);
}

public class NhanhVnDeliveryStatusClient(HttpClient httpClient, AppDbContext dbContext,
    IConfiguration configuration) : INhanhVnDeliveryStatusClient
{
    public async Task MarkDeliveredAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var options = configuration.GetSection(nameof(NhanhVnOption)).Get<NhanhVnOption>() ?? new();
        var configs = await ConfigHelper.GetConfigsByGroupsAsync(dbContext, "nhanhvn");
        var baseUrl = (configs.GetValue("nhanhvn.base_url", options.BaseUrl) ?? "").TrimEnd('/');
        var appId = configs.GetValue("nhanhvn.app_id", options.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", options.BusinessId);
        var token = configs.GetValue("nhanhvn.token", options.Token);
        if (orderId <= 0 || appId <= 0 || businessId <= 0 || string.IsNullOrWhiteSpace(token)
            || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new FulfillmentPermanentException("NhanhVN delivery status configuration is invalid");

        // Reconcile before editing: an earlier attempt may have succeeded remotely and crashed
        // before committing locally. NhanhVN refuses edits of already-completed orders.
        using var current = await SendAsync("list", new { filters = new { ids = new[] { orderId } } });
        if (!current.RootElement.TryGetProperty("data", out var orders) || orders.ValueKind != JsonValueKind.Array)
            throw new FulfillmentTransientException("NhanhVN order/list returned invalid data");
        var matches = orders.EnumerateArray().Where(item => item.TryGetProperty("info", out var info)
            && ReadInt(info, "id") == orderId).ToList();
        if (matches.Count != 1)
            throw new FulfillmentTransientException("NhanhVN order/list did not return the requested order");
        var status = ReadInt(matches[0].GetProperty("info"), "status");
        if (status == 60) return;
        if (status is null or 58 or 61 or 63 or 64 or 68 or 71 or 72 or 74)
            throw new FulfillmentPermanentException("NhanhVN order is missing a status or is cancelled/failed/returning");

        using var edited = await SendAsync("edit", new { info = new { id = orderId, status = 60 } });
        if (!edited.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object
            || ReadInt(data, "id") != orderId)
            throw new FulfillmentTransientException("NhanhVN order/edit returned a mismatched order");

        async Task<JsonDocument> SendAsync(string action, object payload)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"{baseUrl}/order/{action}?appId={appId}&businessId={businessId}");
            request.Headers.Add("Authorization", token);
            request.Content = JsonContent.Create(payload);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
                    || (int)response.StatusCode >= 500)
                    throw new FulfillmentTransientException($"NhanhVN order/{action} HTTP {(int)response.StatusCode}");
                throw new FulfillmentPermanentException($"NhanhVN order/{action} HTTP {(int)response.StatusCode}");
            }
            JsonDocument document;
            try { document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken)); }
            catch (JsonException) { throw new FulfillmentTransientException("NhanhVN returned invalid JSON"); }
            if (ReadInt(document.RootElement, "code") != 1)
            {
                document.Dispose();
                // Retry also reconciles a concurrent completion by another integration.
                throw new FulfillmentTransientException($"NhanhVN order/{action} rejected the request (code != 1)");
            }
            return document;
        }
    }

    private static int? ReadInt(JsonElement element, string name)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
           && int.TryParse(value.ToString(), out var number) ? number : null;
}

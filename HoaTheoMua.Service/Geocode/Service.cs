using System.Net.Http.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.Geocode;

public class Service : IService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _appDbContext;
    private readonly VietMapOption _vietMapOption;

    private static readonly Regex FocusRegex = new(@"^-?\d+(\.\d+)?,-?\d+(\.\d+)?$", RegexOptions.Compiled);

    public Service(HttpClient httpClient, AppDbContext appDbContext, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _appDbContext = appDbContext;
        _vietMapOption = new VietMapOption();
        configuration.GetSection(nameof(VietMapOption)).Bind(_vietMapOption);
    }

    public async Task<List<Response.AutocompleteResult>> Autocomplete(Request.AutocompleteQuery query)
    {
        var configs = await ConfigHelper.GetConfigsByGroupsAsync(_appDbContext, "vietmap");
        var apiKey = configs.GetValue("vietmap.api_key", _vietMapOption.ApiKey) ?? string.Empty;
        var baseUrl = configs.GetValue("vietmap.base_url", _vietMapOption.BaseUrl) ?? _vietMapOption.BaseUrl ?? "https://maps.vietmap.vn/api";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new VietMapException("server-not-configured", 503, "VietMap API Key is not configured.");
        }

        var text = query.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
        {
            throw new VietMapException("invalid-query", 400, "Text must be at least 2 characters.");
        }

        if (!string.IsNullOrWhiteSpace(query.Focus) && !FocusRegex.IsMatch(query.Focus))
        {
            throw new VietMapException("invalid-query", 400, "Focus coordinate is invalid.");
        }

        var url = $"{baseUrl.TrimEnd('/')}/autocomplete/v4?apikey={Uri.EscapeDataString(apiKey)}&text={Uri.EscapeDataString(text)}&display_type=6";
        if (!string.IsNullOrWhiteSpace(query.Focus))
        {
            url += $"&focus={Uri.EscapeDataString(query.Focus)}";
        }

        List<Response.VietMapAutocompleteItem>? items;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new VietMapException("autocomplete-upstream-error", 502, $"VietMap returned {response.StatusCode}.");
            }

            items = await response.Content.ReadFromJsonAsync<List<Response.VietMapAutocompleteItem>>();
        }
        catch (VietMapException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new VietMapException("upstream-unreachable", 502, $"Failed to connect to VietMap: {ex.Message}");
        }

        if (items == null)
        {
            return new List<Response.AutocompleteResult>();
        }

        // Apply mapping and ward/city filtering
        var results = new List<Response.AutocompleteResult>();
        var normWard = RemoveAdministrativePrefix(query.Ward);
        var normCity = RemoveAdministrativePrefix(query.City);

        foreach (var item in items)
        {
            // Map values, prefer data_new (display_type=6 recommendation)
            var refId = item.DataNew?.RefId ?? item.RefId;
            var name = item.DataNew?.Name ?? item.Name;
            var address = item.DataNew?.Address ?? item.Address;
            var display = item.DataNew?.Display ?? item.Display;

            // Filter if ward/city is passed
            if (!string.IsNullOrEmpty(normWard))
            {
                var normAddr = RemoveVietnameseAccents(address);
                if (!normAddr.Contains(normWard)) continue;
            }

            if (!string.IsNullOrEmpty(normCity))
            {
                var normAddr = RemoveVietnameseAccents(address);
                if (!normAddr.Contains(normCity)) continue;
            }

            results.Add(new Response.AutocompleteResult
            {
                RefId = refId,
                Name = name,
                Address = address,
                Display = display
            });

            // Limit to 8 items max
            if (results.Count >= 8) break;
        }

        return results;
    }

    public async Task<Response.PlaceResult?> GetPlaceDetail(string refId)
    {
        var configs = await ConfigHelper.GetConfigsByGroupsAsync(_appDbContext, "vietmap");
        var apiKey = configs.GetValue("vietmap.api_key", _vietMapOption.ApiKey) ?? string.Empty;
        var baseUrl = configs.GetValue("vietmap.base_url", _vietMapOption.BaseUrl) ?? _vietMapOption.BaseUrl ?? "https://maps.vietmap.vn/api";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new VietMapException("server-not-configured", 503, "VietMap API Key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(refId))
        {
            throw new VietMapException("invalid-query", 400, "RefId cannot be empty.");
        }

        var url = $"{baseUrl.TrimEnd('/')}/place/v4?apikey={Uri.EscapeDataString(apiKey)}&refid={Uri.EscapeDataString(refId)}";

        Response.VietMapPlaceResponse? placeResponse;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new VietMapException("place-upstream-error", 502, $"VietMap returned {response.StatusCode}.");
            }

            placeResponse = await response.Content.ReadFromJsonAsync<Response.VietMapPlaceResponse>();
        }
        catch (VietMapException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new VietMapException("upstream-unreachable", 502, $"Failed to connect to VietMap: {ex.Message}");
        }

        if (placeResponse == null || placeResponse.Lat == 0 || placeResponse.Lng == 0)
        {
            return null;
        }

        return new Response.PlaceResult
        {
            Latitude = placeResponse.Lat,
            Longitude = placeResponse.Lng,
            MapId = refId,
            Display = placeResponse.Display ?? string.Empty
        };
    }

    public static string RemoveVietnameseAccents(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var normalizedString = text.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                if (c == 'đ') stringBuilder.Append('d');
                else if (c == 'Đ') stringBuilder.Append('d');
                else stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).Trim();
    }

    public static string RemoveAdministrativePrefix(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        string result = RemoveVietnameseAccents(name);

        string[] adminPrefixes = new[]
        {
            "thanh pho ",
            "thi tran ",
            "thi xa ",
            "phuong ",
            "quan ",
            "huyen ",
            "tinh ",
            "xa "
        };

        foreach (var prefix in adminPrefixes)
        {
            if (result.StartsWith(prefix))
            {
                result = result.Substring(prefix.Length).Trim();
                break;
            }
        }

        return result;
    }
}

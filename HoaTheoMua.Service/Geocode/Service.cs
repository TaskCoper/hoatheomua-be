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

    public async Task<Response.AutocompleteEnvelope> Autocomplete(Request.AutocompleteQuery query)
    {
        try
        {
            var apiKey = _vietMapOption.ApiKey;
            var baseUrl = _vietMapOption.BaseUrl;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new Response.AutocompleteEnvelope { StatusCode = 503, Reason = "server-not-configured" };
            }

            var text = query.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
            {
                return new Response.AutocompleteEnvelope { StatusCode = 400, Reason = "invalid-query" };
            }

            if (!string.IsNullOrWhiteSpace(query.Focus) && !FocusRegex.IsMatch(query.Focus))
            {
                return new Response.AutocompleteEnvelope { StatusCode = 400, Reason = "invalid-query" };
            }

            var url = $"{baseUrl.TrimEnd('/')}/autocomplete/v4?apikey={Uri.EscapeDataString(apiKey)}&text={Uri.EscapeDataString(text)}&display_type=6";
            if (!string.IsNullOrWhiteSpace(query.Focus))
            {
                url += $"&focus={Uri.EscapeDataString(query.Focus)}";
            }

            List<Response.VietMapAutocompleteItem>? items;
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return new Response.AutocompleteEnvelope { StatusCode = 502, Reason = "autocomplete-upstream-error" };
            }

            items = await response.Content.ReadFromJsonAsync<List<Response.VietMapAutocompleteItem>>();
            if (items == null)
            {
                return new Response.AutocompleteEnvelope { Results = [] };
            }

            var results = new List<Response.AutocompleteResult>();
            var normWard = RemoveAdministrativePrefix(query.Ward);
            var normCity = RemoveAdministrativePrefix(query.City);

            foreach (var item in items)
            {
                var refId = item.DataNew?.RefId ?? item.RefId;
                var name = item.DataNew?.Name ?? item.Name;
                var address = item.DataNew?.Address ?? item.Address;
                var display = item.DataNew?.Display ?? item.Display;

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

                if (results.Count >= 8) break;
            }

            return new Response.AutocompleteEnvelope { Results = results };
        }
        catch (Exception)
        {
            return new Response.AutocompleteEnvelope { StatusCode = 502, Reason = "upstream-unreachable" };
        }
    }

    public async Task<Response.PlaceEnvelope> GetPlaceDetail(string refId)
    {
        try
        {
            var apiKey = _vietMapOption.ApiKey;
            var baseUrl = _vietMapOption.BaseUrl ?? "https://maps.vietmap.vn/api";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new Response.PlaceEnvelope { StatusCode = 503, Reason = "server-not-configured" };
            }

            if (string.IsNullOrWhiteSpace(refId))
            {
                return new Response.PlaceEnvelope { StatusCode = 400, Reason = "invalid-query" };
            }

            var url = $"{baseUrl.TrimEnd('/')}/place/v4?apikey={Uri.EscapeDataString(apiKey)}&refid={Uri.EscapeDataString(refId)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return new Response.PlaceEnvelope { StatusCode = 502, Reason = "place-upstream-error" };
            }

            var placeResponse = await response.Content.ReadFromJsonAsync<Response.VietMapPlaceResponse>();
            if (placeResponse == null || placeResponse.Lat == 0 || placeResponse.Lng == 0)
            {
                return new Response.PlaceEnvelope { Result = null };
            }

            return new Response.PlaceEnvelope
            {
                Result = new Response.PlaceResult
                {
                    Latitude = placeResponse.Lat,
                    Longitude = placeResponse.Lng,
                    MapId = refId,
                    Display = placeResponse.Display ?? string.Empty
                }
            };
        }
        catch (Exception)
        {
            return new Response.PlaceEnvelope { StatusCode = 502, Reason = "upstream-unreachable" };
        }
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

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.XanhSm;

public class Service : IService
{
    private sealed record CachedToken(XanhSmTokenResponse Token, DateTimeOffset ExpiresAt);

    private static readonly ConcurrentDictionary<string, CachedToken> TokenCache = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> TokenLocks = new();

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly XanhSmOptions _options = new();

    public Service(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        configuration.GetSection(nameof(XanhSmOptions)).Bind(_options);
    }

    public async Task<XanhSmTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        ValidateConfiguration(includeCredentials: true);

        var cacheKey = $"{_options.TokenUrl}|{_options.ClientId}|{_options.Scope}";
        if (TryGetCachedToken(cacheKey, out var cachedToken))
        {
            return cachedToken;
        }

        var tokenLock = TokenLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (TryGetCachedToken(cacheKey, out cachedToken))
            {
                return cachedToken;
            }

            var tokenParameters = new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["grant_type"] = "client_credentials"
            };
            if (!string.IsNullOrWhiteSpace(_options.Scope))
            {
                tokenParameters["scope"] = _options.Scope;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl)
            {
                Content = new FormUrlEncodedContent(tokenParameters)
            };

            var token = await SendAsync<XanhSmTokenResponse>(request, requiresAuthentication: false, cancellationToken);
            if (string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new BadGatewayException("XanhSM token response did not contain an access token");
            }

            var cacheLifetime = TimeSpan.FromSeconds(Math.Max(token.ExpiresIn - 300, 60));
            TokenCache[cacheKey] = new CachedToken(token, DateTimeOffset.UtcNow.Add(cacheLifetime));
            return token;
        }
        finally
        {
            tokenLock.Release();
        }
    }

    public async Task<XanhSmApiResponse<List<XanhSmAvailableServiceItem>>> GetAvailableServicesAsync(
        XanhSmAvailableServicesRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await PostAsync<XanhSmAvailableServicesRequest, XanhSmApiResponse<List<XanhSmAvailableServiceItem>>>(
            "list-available-services",
            request,
            cancellationToken);
    }

    public Task<XanhSmApiResponse<XanhSmEstimateData>> EstimateOrderAsync(
        XanhSmEstimateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateDoorToDoorPoints(request.Points);
        var doorToDoorAddon = CreateDoorToDoorAddon();
        foreach (var service in request.ServiceIds)
        {
            service.Addons = doorToDoorAddon;
        }
        return PostAsync<XanhSmEstimateOrderRequest, XanhSmApiResponse<XanhSmEstimateData>>(
            "estimate/order",
            request,
            cancellationToken);
    }

    public Task<XanhSmApiResponse<XanhSmDelivery>> BookDeliveryAsync(
        XanhSmBookDeliveryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateDoorToDoorPoints(request.Points);
        request.Addons = CreateDoorToDoorAddon();
        return PostAsync<XanhSmBookDeliveryRequest, XanhSmApiResponse<XanhSmDelivery>>(
            "create-order",
            request,
            cancellationToken);
    }

    public async Task<XanhSmApiResponse<XanhSmDelivery>> GetDeliveryDetailsAsync(
        string orderId,
        bool includeDriver = true,
        bool includePath = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new System.ArgumentException("Order ID is required", nameof(orderId));
        }

        var components = new List<string>();
        if (includeDriver) components.Add("driver");
        if (includePath) components.Add("path");

        var relativeUrl = $"orders/{Uri.EscapeDataString(orderId)}";
        if (components.Count > 0)
        {
            relativeUrl += $"?extra_components={string.Join(',', components)}";
        }

        using var request = CreateApiRequest(HttpMethod.Get, relativeUrl);
        return await SendAsync<XanhSmApiResponse<XanhSmDelivery>>(request, true, cancellationToken);
    }

    public async Task<XanhSmApiResponse<List<XanhSmDelivery>>> GetDeliveryListAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = CreateApiRequest(HttpMethod.Get, "list-user-orders");
        return await SendAsync<XanhSmApiResponse<List<XanhSmDelivery>>>(request, true, cancellationToken);
    }

    public Task<XanhSmApiResponse<XanhSmCancelDeliveryData>> CancelDeliveryAsync(
        XanhSmCancelDeliveryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return PostAsync<XanhSmCancelDeliveryRequest, XanhSmApiResponse<XanhSmCancelDeliveryData>>(
            "cancel-order",
            request,
            cancellationToken);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string relativeUrl,
        TRequest body,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        using var request = CreateApiRequest(HttpMethod.Post, relativeUrl);
        request.Content = JsonContent.Create(body, options: JsonOptions);
        return await SendAsync<TResponse>(request, true, cancellationToken);
    }

    private HttpRequestMessage CreateApiRequest(HttpMethod method, string relativeUrl)
    {
        ValidateConfiguration(includeCredentials: false);
        return CreateRequest(method, $"{_options.BaseUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}");
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("Accept-Language", _options.AcceptLanguage);
        return request;
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpRequestMessage request,
        bool requiresAuthentication,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        if (requiresAuthentication)
        {
            var token = await GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            ThrowProviderException(response.StatusCode, responseBody);
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseBody, JsonOptions)
                   ?? throw new BadGatewayException("XanhSM returned null or empty data");
        }
        catch (JsonException exception)
        {
            throw new BadGatewayException($"XanhSM returned an invalid response: {exception.Message}");
        }
    }

    private static void ThrowProviderException(HttpStatusCode statusCode, string responseBody)
    {
        XanhSmErrorResponse? error = null;
        try
        {
            error = JsonSerializer.Deserialize<XanhSmErrorResponse>(responseBody, JsonOptions);
        }
        catch (JsonException)
        {
            // Do not leak a raw provider response; it can contain personal delivery information.
        }

        var providerCode = error is not null &&
                           error.Code.ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null)
            ? error.Code.ToString()
            : null;
        var providerMessage = error?.Message ?? error?.ErrorDescription ?? error?.Error;
        var detail = $"XanhSM API HTTP Error ({(int)statusCode})";
        if (!string.IsNullOrWhiteSpace(providerCode)) detail += $" [{providerCode}]";
        if (!string.IsNullOrWhiteSpace(providerMessage)) detail += $": {providerMessage}";

        throw statusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestException(detail),
            HttpStatusCode.NotFound => new NotFoundException(detail),
            HttpStatusCode.TooManyRequests => new TooManyRequestException(detail),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                new BadGatewayException("XanhSM authentication failed"),
            _ => new BadGatewayException(detail)
        };
    }

    private bool TryGetCachedToken(string cacheKey, out XanhSmTokenResponse token)
    {
        if (TokenCache.TryGetValue(cacheKey, out var cached) && cached.ExpiresAt > DateTimeOffset.UtcNow)
        {
            token = cached.Token;
            return true;
        }

        TokenCache.TryRemove(cacheKey, out _);
        token = null!;
        return false;
    }

    private void ValidateConfiguration(bool includeCredentials)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl) ||
            string.IsNullOrWhiteSpace(_options.TokenUrl) ||
            _options.DoorToDoorAddonId <= 0)
        {
            throw new ServerException("XanhSmOptions endpoint or Door-to-door configuration is missing");
        }

        if (includeCredentials &&
            (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret)))
        {
            throw new ServerException("XanhSmOptions credentials are missing");
        }
    }

    private IReadOnlyList<XanhSmAddonRequest> CreateDoorToDoorAddon()
    {
        return
        [
            new XanhSmAddonRequest
            {
                Id = _options.DoorToDoorAddonId,
                Apply = true
            }
        ];
    }

    private static void ValidateDoorToDoorPoints(IReadOnlyCollection<XanhSmDeliveryPointRequest>? points)
    {
        if (points is null || points.Count == 0)
        {
            throw new BadRequestException("XanhSM delivery points are required");
        }

        var pickUpCount = points.Count(point =>
            string.Equals(point.Type, XanhSmConstants.PickUpPoint, StringComparison.OrdinalIgnoreCase));
        var dropOffCount = points.Count(point =>
            string.Equals(point.Type, XanhSmConstants.DropOffPoint, StringComparison.OrdinalIgnoreCase));

        if (pickUpCount < 1 || dropOffCount != 1 || points.Count != pickUpCount + dropOffCount)
        {
            throw new BadRequestException(
                "XanhSM Door-to-door requires at least one PICK UP point and exactly one DROP OFF point");
        }
    }

}

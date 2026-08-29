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
        foreach (var service in request.ServiceIds)
        {
            ValidateDoorToDoorAddon(service.Addons);
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
        ValidateDoorToDoorAddon(request.Addons);
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

    public async Task<XanhSmDoorToDoorQuote> EstimateDoorToDoorAsync(
        IReadOnlyCollection<XanhSmDeliveryPointRequest> points,
        long orderTime,
        CancellationToken cancellationToken = default)
    {
        ValidateDoorToDoorPoints(points);

        var pointList = points.ToList();
        var availableServices = await GetAvailableServicesAsync(
            new XanhSmAvailableServicesRequest
            {
                Pickups = pointList
                    .Where(point => string.Equals(
                        point.Type,
                        XanhSmConstants.PickUpPoint,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(ToCoordinatePoint)
                    .ToList(),
                Dropoffs = pointList
                    .Where(point => string.Equals(
                        point.Type,
                        XanhSmConstants.DropOffPoint,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(ToCoordinatePoint)
                    .ToList(),
                ServiceTypes = [XanhSmConstants.ExpressOnDemandServiceType],
                OrderTime = orderTime
            },
            cancellationToken);

        var availableService = availableServices.Data
            .Select(item => new
            {
                Item = item,
                DoorToDoorAddon = ResolveDoorToDoorAddon(item.Addons)
            })
            .FirstOrDefault(candidate =>
                candidate.Item.Service.Enable is not false &&
                candidate.Item.Service.DuringService is not false &&
                string.Equals(
                    candidate.Item.Service.ServiceType,
                    XanhSmConstants.ExpressOnDemandServiceType,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    candidate.Item.Service.TravelMode,
                    XanhSmConstants.MotorcycleTravelMode,
                    StringComparison.OrdinalIgnoreCase) &&
                candidate.DoorToDoorAddon is not null);

        if (availableService is null)
        {
            throw new BadRequestException("XanhSM Door-to-door service is not available for this route");
        }

        ConfigurePickupPackage(pointList, availableService.Item.Service);

        var serviceId = availableService.Item.Service.Id > 0
            ? availableService.Item.Service.Id
            : availableService.Item.Service.ServiceId.GetValueOrDefault();
        if (serviceId <= 0)
        {
            throw new BadGatewayException("XanhSM returned an invalid service ID");
        }

        var addonId = availableService.DoorToDoorAddon!.Id;
        if (addonId <= 0)
        {
            throw new BadGatewayException("XanhSM returned an invalid Door-to-door addon ID");
        }

        var estimate = await EstimateOrderAsync(
            new XanhSmEstimateOrderRequest
            {
                ServiceIds =
                [
                    new XanhSmEstimateServiceRequest
                    {
                        Id = serviceId,
                        Addons = CreateDoorToDoorAddon(addonId)
                    }
                ],
                OrderTime = orderTime,
                Points = pointList
            },
            cancellationToken);

        var serviceEstimate = estimate.Data.ServiceEstimates.FirstOrDefault(item =>
            !item.FeeUnavailable &&
            item.EstimateInfo?.Estimate is not null &&
            !string.IsNullOrWhiteSpace(item.EstimateInfo.FeeId));
        if (serviceEstimate?.EstimateInfo?.Estimate is null ||
            string.IsNullOrWhiteSpace(serviceEstimate.EstimateInfo.FeeId))
        {
            var errorMessage = estimate.Data.ServiceEstimates
                .Select(item => item.ErrorMessage)
                .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));
            throw new BadRequestException(
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "XanhSM could not estimate this Door-to-door delivery"
                    : $"XanhSM could not estimate this Door-to-door delivery: {errorMessage}");
        }

        return new XanhSmDoorToDoorQuote
        {
            ServiceId = serviceId,
            AddonId = addonId,
            ServiceType = availableService.Item.Service.ServiceType!,
            FeeId = serviceEstimate.EstimateInfo.FeeId,
            TotalFee = serviceEstimate.EstimateInfo.Estimate.TotalFee,
            ExpiresAt = serviceEstimate.EstimateInfo.ExpiresAt
        };
    }

    public async Task<XanhSmDelivery> BookDoorToDoorAsync(
        IReadOnlyCollection<XanhSmDeliveryPointRequest> points,
        long orderTime,
        long? goodsValue,
        string partnerTripId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partnerTripId))
        {
            throw new System.ArgumentException("Partner trip ID is required", nameof(partnerTripId));
        }

        var quote = await EstimateDoorToDoorAsync(points, orderTime, cancellationToken);
        var response = await BookDeliveryAsync(
            new XanhSmBookDeliveryRequest
            {
                OrderTime = orderTime,
                ServiceId = quote.ServiceId,
                ServiceType = quote.ServiceType,
                FeeId = quote.FeeId,
                Points = points.ToList(),
                Addons = CreateDoorToDoorAddon(quote.AddonId),
                GoodsValue = goodsValue,
                PartnerTripId = partnerTripId
            },
            cancellationToken);

        if (string.IsNullOrWhiteSpace(response.Data.Id) && string.IsNullOrWhiteSpace(response.Data.OrderId))
        {
            throw new BadGatewayException("XanhSM booking response did not contain an order ID");
        }

        return response.Data;
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
            string.IsNullOrWhiteSpace(_options.TokenUrl))
        {
            throw new ServerException("XanhSmOptions endpoint configuration is missing");
        }

        if (includeCredentials &&
            (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret)))
        {
            throw new ServerException("XanhSmOptions credentials are missing");
        }
    }

    private XanhSmAvailableAddon? ResolveDoorToDoorAddon(
        IReadOnlyCollection<XanhSmAvailableAddon>? addons)
    {
        if (addons is null || addons.Count == 0)
        {
            return null;
        }

        var addonByGroup = addons.FirstOrDefault(addon =>
            addon.Enable is not false &&
            addon.Id > 0 &&
            string.Equals(
                addon.GroupName,
                XanhSmConstants.DoorToDoorGroup,
                StringComparison.OrdinalIgnoreCase));
        if (addonByGroup is not null)
        {
            return addonByGroup;
        }

        return null;
    }

    private static IReadOnlyList<XanhSmAddonRequest> CreateDoorToDoorAddon(int addonId)
    {
        if (addonId <= 0)
        {
            throw new BadGatewayException("XanhSM returned an invalid Door-to-door addon ID");
        }

        return
        [
            new XanhSmAddonRequest
            {
                Id = addonId,
                Apply = true
            }
        ];
    }

    private static void ValidateDoorToDoorAddon(IReadOnlyCollection<XanhSmAddonRequest>? addons)
    {
        if (addons is null || addons.Count != 1 || addons.Any(addon => addon.Id <= 0 || !addon.Apply))
        {
            throw new BadRequestException(
                "Exactly one applied XanhSM Door-to-door addon returned by available services is required");
        }
    }

    private static XanhSmCoordinatePoint ToCoordinatePoint(XanhSmDeliveryPointRequest point)
    {
        return new XanhSmCoordinatePoint
        {
            Latitude = point.Latitude,
            Longitude = point.Longitude
        };
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

        foreach (var pickup in points.Where(point =>
                     string.Equals(point.Type, XanhSmConstants.PickUpPoint, StringComparison.OrdinalIgnoreCase)))
        {
            if (pickup.PackageWeight is null or <= 0)
            {
                throw new BadRequestException("XanhSM package_weight is required at every PICK UP point");
            }

            if (string.IsNullOrWhiteSpace(pickup.PackageSize))
            {
                throw new BadRequestException("XanhSM package_size is required at every PICK UP point");
            }

            if (pickup.PackageTypes is null ||
                pickup.PackageTypes.Count == 0 ||
                pickup.PackageTypes.Any(string.IsNullOrWhiteSpace))
            {
                throw new BadRequestException("XanhSM package_types is required at every PICK UP point");
            }
        }
    }

    private static void ConfigurePickupPackage(
        IEnumerable<XanhSmDeliveryPointRequest> points,
        XanhSmServiceInfo service)
    {
        if (!string.IsNullOrWhiteSpace(service.ExpressPackageWeightUnit) &&
            !string.Equals(service.ExpressPackageWeightUnit, "kg", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadGatewayException(
                $"XanhSM returned unsupported package weight unit '{service.ExpressPackageWeightUnit}'");
        }

        var acceptedLargeSize = service.ExpressPackageSizes.FirstOrDefault(size =>
            string.Equals(size, XanhSmConstants.LargePackageSize, StringComparison.OrdinalIgnoreCase));
        if (acceptedLargeSize is null)
        {
            throw new BadRequestException("The selected XanhSM motorcycle service does not accept LARGE packages");
        }

        var acceptedFlowerType = service.ExpressPackageTypes.FirstOrDefault(type =>
            string.Equals(type, XanhSmConstants.FlowersPackageType, StringComparison.OrdinalIgnoreCase));
        var acceptedOtherType = service.ExpressPackageTypes.FirstOrDefault(type =>
            string.Equals(type, XanhSmConstants.OtherPackageType, StringComparison.OrdinalIgnoreCase));
        var packageType = acceptedFlowerType ?? acceptedOtherType;
        if (packageType is null)
        {
            throw new BadRequestException(
                "The selected XanhSM motorcycle service does not accept flowers or other package types");
        }

        var serviceMaxWeight = service.ExpressPackageMaxWeight is > 0
            ? service.ExpressPackageMaxWeight.Value
            : XanhSmConstants.StandardMotorcycleMaxWeightKg;
        var maximumWeight = Math.Min(serviceMaxWeight, XanhSmConstants.StandardMotorcycleMaxWeightKg);

        foreach (var pickup in points.Where(point =>
                     string.Equals(point.Type, XanhSmConstants.PickUpPoint, StringComparison.OrdinalIgnoreCase)))
        {
            if (pickup.PackageWeight > maximumWeight)
            {
                throw new BadRequestException(
                    $"XanhSM motorcycle delivery supports at most {maximumWeight:0.##} kg without the bulky addon");
            }

            pickup.PackageSize = acceptedLargeSize;
            pickup.PackageTypes = [packageType];
        }
    }

}

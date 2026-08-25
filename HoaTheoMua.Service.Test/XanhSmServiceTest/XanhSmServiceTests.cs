using System.Net;
using System.Text;
using HoaTheoMua.Service.XanhSmService;
using HoaTheoMua.Service.XanhSmService.Validations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using XanhSmServiceImplementation = HoaTheoMua.Service.XanhSmService.Service;

namespace HoaTheoMua.Service.Test.XanhSmServiceTest;

/// <summary>Outbound integration tests use a message handler, so no real Xanh SM credential is ever required.</summary>
public class XanhSmServiceTests
{
    [Fact]
    public async Task GetAvailableServices_SendsBearerAndSnakeCaseBody()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.OK, """{"data":[{"service":{"id":2099,"display_name":"Van Express","service_type":"EXPRESS-ONDEMAND"}}]}"""));
        var service = CreateService(handler);

        var result = await service.GetAvailableServices(new Request.GetAvailableServicesRequest
        {
            Pickups = [new Request.Coordinate { Latitude = 10.7, Longitude = 106.7 }],
            ServiceTypes = ["EXPRESS-ONDEMAND"]
        });

        Assert.Single(result);
        Assert.Equal(2099, result[0].Service.Id);
        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Contains("service_types", handler.LastBody);
        Assert.DoesNotContain("serviceTypes", handler.LastBody);
    }

    [Fact]
    public async Task EstimateOrder_DeserializesFeeId()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.OK,
            """{"data":{"payment_method":"cash","order_time":"1770350401","service_estimates":[{"fee_unavailable":false,"service":{"id":1},"estimate_info":{"fee_id":"d98fc496-492f-41a3-be80-3c2590345df6","expires_at":1778494276}}]}}"""));
        var service = CreateService(handler);

        var result = await service.EstimateOrder(new Request.EstimateOrderRequest
        {
            ServiceIds = [new Request.EstimateService { Id = 1 }], PaymentMethod = "cash", Points = ValidPoints()
        });

        Assert.Equal("d98fc496-492f-41a3-be80-3c2590345df6", result.ServiceEstimates[0].EstimateInfo!.FeeId);
        Assert.Contains("service_ids", handler.LastBody);
    }

    [Fact]
    public async Task CreateOrder_PutsAddonsAtTopLevel()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.OK, """{"data":{"id":"xanh-order-1","status":"FINDING"}}"""));
        var service = CreateService(handler);
        var request = new Request.CreateXanhSmOrderRequest
        {
            ServiceId = 1, ServiceType = "EXPRESS-ONDEMAND", FeeId = Guid.NewGuid().ToString(),
            PaymentMethod = "cash", Points = ValidPoints(), Addons = [new Request.Addon { Id = 5, Apply = true }]
        };

        var result = await service.CreateOrder(request);

        Assert.Equal("xanh-order-1", result.Id);
        Assert.Contains("\"addons\"", handler.LastBody);
        Assert.DoesNotContain("service_ids", handler.LastBody);
    }

    [Fact]
    public async Task CancelOrder_MapsPartnerValidationError()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.BadRequest, """{"code":4079,"message":"Pick up point is required"}"""));
        var service = CreateService(handler);

        var exception = await Assert.ThrowsAsync<XanhSmException>(() => service.CancelOrder(new Request.CancelOrderRequest { OrderId = "id", CancelMessage = "reason" }));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("XANH_SM_4079", exception.MessageCode);
    }

    [Fact]
    public async Task TokenProvider_CachesTokenUntilExpiry()
    {
        var calls = 0;
        var handler = new RecordingHandler(_ =>
        {
            calls++;
            return Json(HttpStatusCode.OK, """{"access_token":"cached-token","token_type":"Bearer","expires_in":3600}""");
        });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var provider = new XanhSmTokenProvider(new HttpClient(handler), cache, CreateConfiguration(), NullLogger<XanhSmTokenProvider>.Instance);

        var first = await provider.GetAccessToken();
        var second = await provider.GetAccessToken();

        Assert.Equal("cached-token", first);
        Assert.Equal(first, second);
        Assert.Equal(1, calls);
        Assert.Contains("grant_type=client_credentials", handler.LastBody);
    }

    [Fact]
    public async Task TokenProvider_ForbiddenResponse_ReturnsClearWhitelistError()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.Forbidden, """{"message":"forbidden"}"""));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var provider = new XanhSmTokenProvider(new HttpClient(handler), cache, CreateConfiguration(),
            NullLogger<XanhSmTokenProvider>.Instance);

        var exception = await Assert.ThrowsAsync<XanhSmException>(() => provider.GetAccessToken());

        Assert.Equal(502, exception.StatusCode);
        Assert.Equal("XANH_SM_TOKEN_FORBIDDEN", exception.MessageCode);
        Assert.Contains("IP public", exception.Message);
    }

    [Fact]
    public async Task CreateOrderValidator_RejectsMismatchedCodAmount()
    {
        var validator = new CreateOrderRequestValidator();
        var result = await validator.ValidateAsync(new Request.CreateXanhSmOrderRequest
        {
            ServiceId = 1, ServiceType = "EXPRESS-ONDEMAND", FeeId = Guid.NewGuid().ToString(), PaymentMethod = "cash",
            Points = ValidPoints(collectCod: 20_000), Addons = [new Request.Addon { Id = 4, Apply = true, RemainCod = 10_000, Value = 1 }]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "COD_AMOUNT_MUST_MATCH_DROPOFF");
    }

    private static XanhSmServiceImplementation CreateService(RecordingHandler handler) => new(new HttpClient(handler), new TestTokenProvider(),
        CreateConfiguration(), NullLogger<XanhSmServiceImplementation>.Instance);

    // Dùng Configuration thật trong test để kiểm tra cùng cách bind mà API sử dụng khi chạy.
    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["XanhSmOption:BaseUrl"] = "https://xanh-sm.test",
            ["XanhSmOption:TokenUrl"] = "https://xanh-sm.test/token",
            ["XanhSmOption:ClientId"] = "client",
            ["XanhSmOption:ClientSecret"] = "secret",
            ["XanhSmOption:Scope"] = "sandbox.express.trips",
            ["XanhSmOption:TokenRefreshBufferSeconds"] = "300",
            ["XanhSmOption:TimeoutSeconds"] = "30"
        })
        .Build();

    private static List<Request.RoutePoint> ValidPoints(int? collectCod = null) =>
    [
        new Request.RoutePoint
        {
            Type = "PICK UP", Name = "Sender", PhoneNumber = "+84886901000", Latitude = 10.7, Longitude = 106.7,
            ShortAddress = "Pickup", Address = "Pickup address", PackageWeight = 1, PackageSize = "SMALL", PackageTypes = ["food"]
        },
        new Request.RoutePoint
        {
            Type = "DROP OFF", Name = "Receiver", PhoneNumber = "+84886901001", Latitude = 10.8, Longitude = 106.8,
            ShortAddress = "Dropoff", Address = "Dropoff address", Addons = collectCod is null ? null : new Request.PointAddon { CollectCod = collectCod.Value }
        }
    ];

    private static HttpResponseMessage Json(HttpStatusCode status, string json) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class TestTokenProvider : IXanhSmTokenProvider
    {
        public Task<string> GetAccessToken(CancellationToken cancellationToken = default) => Task.FromResult("test-token");
        public void Invalidate() { }
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string LastBody { get; private set; } = string.Empty;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            return responder(request);
        }
    }
}

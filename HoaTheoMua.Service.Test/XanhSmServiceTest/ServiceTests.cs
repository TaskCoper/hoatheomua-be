using System.Net;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.XanhSm;
using Microsoft.Extensions.Configuration;
using Xunit;
using XanhSmServiceImplementation = HoaTheoMua.Service.XanhSm.Service;

namespace HoaTheoMua.Service.Test.XanhSmServiceTest;

public class ServiceTests
{
    [Fact]
    public async Task Unauthorized_RefreshesTokenAndReplaysPostBody()
    {
        var tokenCalls = 0;
        var requests = new List<CapturedRequest>();
        var service = CreateService(async request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
                return JsonResponse($$"""{"access_token":"token-{{++tokenCalls}}","token_type":"Bearer","expires_in":3600}""");
            requests.Add(await CapturedRequest.FromAsync(request));
            return requests.Count == 1
                ? JsonResponse("{}", HttpStatusCode.Unauthorized)
                : JsonResponse("""{"data":[]}""");
        });

        await service.GetAvailableServicesAsync(new XanhSmAvailableServicesRequest
        {
            Pickups = [new XanhSmCoordinatePoint { Latitude = 10.8, Longitude = 106.6 }]
        });

        Assert.Equal(2, tokenCalls);
        Assert.Equal(2, requests.Count);
        Assert.Equal("token-1", requests[0].AuthorizationParameter);
        Assert.Equal("token-2", requests[1].AuthorizationParameter);
        Assert.Equal(requests[0].Body, requests[1].Body);
        Assert.Equal(requests[0].ContentType, requests[1].ContentType);
        Assert.Equal(requests[0].Uri, requests[1].Uri);
        Assert.Equal(requests[0].AcceptLanguage, requests[1].AcceptLanguage);
        Assert.Equal("token-2", (await service.GetAccessTokenAsync()).AccessToken);
        Assert.Equal(2, tokenCalls);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, 2)]
    [InlineData(HttpStatusCode.Forbidden, 1)]
    public async Task AuthenticationFailure_RetriesOnlyOneUnauthorized(HttpStatusCode status, int expectedCalls)
    {
        var tokenCalls = 0;
        var apiCalls = 0;
        var service = CreateService(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
            {
                tokenCalls++;
                return Task.FromResult(TokenResponse());
            }
            apiCalls++;
            return Task.FromResult(JsonResponse("{}", status));
        });
        await Assert.ThrowsAsync<BadGatewayException>(() => service.GetDeliveryDetailsAsync("ORDER-001"));
        Assert.Equal(expectedCalls, apiCalls);
        Assert.Equal(expectedCalls, tokenCalls);
        await service.GetAccessTokenAsync();
        Assert.Equal(status == HttpStatusCode.Unauthorized ? 3 : 1, tokenCalls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task TokenWithoutPositiveLifetime_IsNotCached(int expiresIn)
    {
        var calls = 0;
        var service = CreateService(_ =>
        {
            calls++;
            return Task.FromResult(JsonResponse($$"""{"access_token":"token","token_type":"Bearer","expires_in":{{expiresIn}}}"""));
        });
        await service.GetAccessTokenAsync();
        await service.GetAccessTokenAsync();
        Assert.Equal(2, calls);
    }

    [Theory]
    [InlineData("2126ca79-8b1d-4d93-a6fb-e610b8700ab1", 250000)]
    [InlineData("2126ca798b1d4d93a6fbe610b8700ab1", -1)]
    public async Task BookDeliveryAsync_InvalidReferenceOrGoodsValue_RejectsBeforeHttp(
        string partnerTripId, long goodsValue)
    {
        var service = CreateService(_ => throw new InvalidOperationException("HTTP must not be called"));
        await Assert.ThrowsAsync<BadRequestException>(() => service.BookDeliveryAsync(
            new XanhSmBookDeliveryRequest
            {
                OrderTime = 1770350401,
                ServiceId = 2100,
                ServiceType = "EXPRESS-ONDEMAND",
                FeeId = Guid.NewGuid().ToString(),
                Points = [CreatePoint("PICK UP"), CreatePoint("DROP OFF")],
                AddonIds = [5],
                PartnerTripId = partnerTripId,
                GoodsValue = goodsValue
            }));
    }

    [Theory]
    [InlineData("{\"order_time\":1770350401}")]
    [InlineData("{\"order_time\":\"1770350401\"}")]
    public void EstimateData_DeserializesNumericOrStringOrderTime(string json)
    {
        var result = JsonSerializer.Deserialize<XanhSmEstimateData>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(result);
        Assert.Equal(1770350401L, result.OrderTime);
    }

    [Theory]
    [InlineData("{\"order_time\":1770350401}")]
    [InlineData("{\"order_time\":\"1770350401\"}")]
    public void Delivery_DeserializesNumericOrStringOrderTime(string json)
    {
        var result = JsonSerializer.Deserialize<XanhSmDelivery>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(result);
        Assert.Equal(1770350401L, result.OrderTime);
    }

    [Fact]
    public async Task GetAccessTokenAsync_SendsFormDataAndCachesToken()
    {
        var tokenCalls = 0;
        CapturedRequest? capturedTokenRequest = null;
        var service = CreateService(async request =>
        {
            capturedTokenRequest = await CapturedRequest.FromAsync(request);
            tokenCalls++;
            return JsonResponse("""
                {
                  "access_token": "test-token",
                  "token_type": "Bearer",
                  "expires_in": 2592000,
                  "scope": "express.trips"
                }
                """);
        });

        var first = await service.GetAccessTokenAsync();
        var second = await service.GetAccessTokenAsync();

        Assert.Equal("test-token", first.AccessToken);
        Assert.Equal("test-token", second.AccessToken);
        Assert.Equal(1, tokenCalls);
        Assert.NotNull(capturedTokenRequest);
        Assert.Equal("application/x-www-form-urlencoded", capturedTokenRequest.ContentType);
        Assert.Contains("grant_type=client_credentials", capturedTokenRequest.Body);
        Assert.Contains("scope=express.trips", capturedTokenRequest.Body);
    }

    [Fact]
    public async Task GetDeliveryDetailsAsync_AddsBearerTokenAndExtraComponents()
    {
        CapturedRequest? apiRequest = null;
        var service = CreateService(async request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
            {
                return TokenResponse();
            }

            apiRequest = await CapturedRequest.FromAsync(request);
            return JsonResponse("""
                {
                  "data": {
                    "id": "ORDER-001",
                    "status": "ASSIGNED",
                    "driver": { "name": "Tài xế", "rating": 5 },
                    "path": [
                      {
                        "type": "DROP OFF",
                        "status": "COMPLETED",
                        "proof_of_work": { "photos": ["https://example.test/pod.jpg"] }
                      }
                    ]
                  }
                }
                """);
        });

        var result = await service.GetDeliveryDetailsAsync("ORDER-001");

        Assert.Equal("ORDER-001", result.Data.Id);
        Assert.Equal("Tài xế", result.Data.Driver?.Name);
        Assert.Single(result.Data.Path);
        Assert.NotNull(apiRequest);
        Assert.Equal(HttpMethod.Get, apiRequest.Method);
        Assert.Equal("Bearer", apiRequest.AuthorizationScheme);
        Assert.Equal("test-token", apiRequest.AuthorizationParameter);
        Assert.Equal("/v1/express/orders/ORDER-001", apiRequest.Uri.AbsolutePath);
        Assert.Equal("?extra_components=driver,path", apiRequest.Uri.Query);
        Assert.Equal("vi", apiRequest.AcceptLanguage);
    }

    [Fact]
    public async Task EstimateOrderAsync_UsesPartnerPayAndProvidedDoorToDoorAddon()
    {
        CapturedRequest? apiRequest = null;
        var service = CreateService(async request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
            {
                return TokenResponse();
            }

            apiRequest = await CapturedRequest.FromAsync(request);
            return JsonResponse("""
                {
                  "data": {
                    "payment_method": "partner_pay",
                    "order_time": "1770350401",
                    "points": [],
                    "service_estimates": [
                      {
                        "fee_unavailable": false,
                        "estimate_info": {
                          "fee_id": "fee-001",
                          "expires_at": 1770350500,
                          "estimate": { "total_fee": 33000, "total_pay": 33000 },
                          "trip": { "distance_estimate": 5.3, "duration_estimate": 700 }
                        }
                      }
                    ]
                  }
                }
                """);
        });

        var request = new XanhSmEstimateOrderRequest
        {
            ServiceIds =
            [
                new XanhSmEstimateServiceRequest
                {
                    Id = 2099,
                    Addons = [new XanhSmAddonRequest { Id = 5 }]
                }
            ],
            OrderTime = 1770350401,
            Points =
            [
                CreatePoint("PICK UP"),
                CreatePoint("DROP OFF")
            ]
        };

        var result = await service.EstimateOrderAsync(request);

        Assert.Equal("fee-001", result.Data.ServiceEstimates[0].EstimateInfo?.FeeId);
        Assert.NotNull(apiRequest);
        Assert.Equal("/v1/express/estimate/order", apiRequest.Uri.AbsolutePath);
        using var body = JsonDocument.Parse(apiRequest.Body!);
        var root = body.RootElement;
        Assert.Equal("partner_pay", root.GetProperty("payment_method").GetString());
        var addon = root.GetProperty("service_ids")[0].GetProperty("addons")[0];
        Assert.Equal(5, addon.GetProperty("id").GetInt32());
        Assert.True(addon.GetProperty("apply").GetBoolean());
        Assert.False(root.GetProperty("points")[1].TryGetProperty("addons", out _));
        Assert.False(root.GetProperty("points")[0].TryGetProperty("is_payment_check_point", out _));
    }

    [Fact]
    public async Task BookDeliveryAsync_UsesPartnerPayAndProvidedDoorToDoorAddon()
    {
        CapturedRequest? apiRequest = null;
        var service = CreateService(async request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
            {
                return TokenResponse();
            }

            apiRequest = await CapturedRequest.FromAsync(request);
            return JsonResponse("""
                {
                  "data": {
                    "id": "ORDER-001",
                    "status": "FINDING",
                    "path": []
                  }
                }
                """);
        });

        var result = await service.BookDeliveryAsync(new XanhSmBookDeliveryRequest
        {
            OrderTime = 1770350401,
            ServiceId = 2099,
            ServiceType = "EXPRESS-ONDEMAND",
            FeeId = "fee-001",
            AddonIds = [5],
            Points =
            [
                CreatePoint("PICK UP"),
                CreatePoint("DROP OFF")
            ]
        });

        Assert.Equal("ORDER-001", result.Data.Id);
        Assert.NotNull(apiRequest);
        using var body = JsonDocument.Parse(apiRequest.Body!);
        var root = body.RootElement;
        Assert.Equal("partner_pay", root.GetProperty("payment_method").GetString());
        Assert.Equal("[5]", root.GetProperty("addon_ids").GetRawText());
        Assert.False(root.TryGetProperty("addons", out _));
    }

    [Fact]
    public async Task EstimateOrderAsync_WithoutDiscoveredAddon_ThrowsBadRequestException()
    {
        var service = CreateService(_ => throw new InvalidOperationException("HTTP must not be called"));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.EstimateOrderAsync(
            new XanhSmEstimateOrderRequest
            {
                ServiceIds = [new XanhSmEstimateServiceRequest { Id = 2099 }],
                OrderTime = 1770350401,
                Points = [CreatePoint("PICK UP"), CreatePoint("DROP OFF")]
            }));

        Assert.Contains("addon returned by available services is required", exception.Message);
    }

    [Fact]
    public async Task BookDoorToDoorAsync_DiscoversServiceReestimatesAndBooks()
    {
        CapturedRequest? availableRequest = null;
        CapturedRequest? estimateRequest = null;
        CapturedRequest? bookingRequest = null;
        var service = CreateService(async request =>
        {
            var path = request.RequestUri!.AbsolutePath;
            if (path.EndsWith("/token"))
            {
                return TokenResponse();
            }

            if (path.EndsWith("/list-available-services"))
            {
                availableRequest = await CapturedRequest.FromAsync(request);
                return JsonResponse("""
                    {
                      "data": [
                        {
                          "service": {
                            "id": 2099,
                            "service_type": "EXPRESS-ONDEMAND",
                            "travel_mode": "Truck",
                            "enable": true,
                            "during_service": true
                          },
                          "addons": [
                            { "id": 5, "group_name": "DOOR2DOOR", "enable": true }
                          ]
                        },
                        {
                          "service": {
                            "id": 2100,
                            "service_type": "EXPRESS-ONDEMAND",
                            "travel_mode": "Motorcycle",
                            "vehicle_models": ["BIKE"],
                            "express_package_types": null,
                            "express_package_sizes": null,
                            "express_package_max_weight": 1000,
                            "express_package_weight_unit": "kg",
                            "enable": true,
                            "during_service": true
                          },
                          "addons": [
                            { "id": 12, "group_name": "DOOR2DOOR", "enable": true }
                          ]
                        }
                      ]
                    }
                    """);
            }

            if (path.EndsWith("/estimate/order"))
            {
                estimateRequest = await CapturedRequest.FromAsync(request);
                return JsonResponse("""
                    {
                      "data": {
                        "payment_method": "partner_pay",
                        "points": [],
                        "service_estimates": [
                          {
                            "fee_unavailable": false,
                            "estimate_info": {
                              "fee_id": "fresh-fee-id",
                              "expires_at": 1770350500,
                              "estimate": { "total_fee": 33000, "total_pay": 33000 }
                            }
                          }
                        ]
                      }
                    }
                    """);
            }

            bookingRequest = await CapturedRequest.FromAsync(request);
            return JsonResponse("""
                {
                  "data": {
                    "id": "XANHSM-001",
                    "status": "FINDING",
                    "tracking_url": "https://tracking.example/XANHSM-001",
                    "path": []
                  }
                }
                """);
        });

        var delivery = await service.BookDoorToDoorAsync(
            [CreatePoint("PICK UP"), CreatePoint("DROP OFF")],
            1770350401,
            150000,
            "partner-order-001");

        Assert.Equal("XANHSM-001", delivery.Id);
        Assert.Equal("https://tracking.example/XANHSM-001", delivery.TrackingUrl);
        Assert.NotNull(availableRequest);
        using (var availableBody = JsonDocument.Parse(availableRequest.Body!))
        {
            Assert.Equal(
                "EXPRESS-ONDEMAND",
                availableBody.RootElement.GetProperty("service_types")[0].GetString());
        }

        Assert.NotNull(bookingRequest);
        Assert.Equal("/v1/express/create-order", bookingRequest.Uri.AbsolutePath);
        using var bookingBody = JsonDocument.Parse(bookingRequest.Body!);
        Assert.Equal(2100, bookingBody.RootElement.GetProperty("service_id").GetInt32());
        Assert.Equal("fresh-fee-id", bookingBody.RootElement.GetProperty("fee_id").GetString());
        Assert.Equal("partner_pay", bookingBody.RootElement.GetProperty("payment_method").GetString());
        Assert.Equal("partner-order-001", bookingBody.RootElement.GetProperty("partner_trip_id").GetString());
        Assert.Equal(150000, bookingBody.RootElement.GetProperty("goods_value").GetInt64());
        Assert.Equal("[12]", bookingBody.RootElement.GetProperty("addon_ids").GetRawText());
        Assert.False(bookingBody.RootElement.TryGetProperty("addons", out _));
        var bookingPickup = bookingBody.RootElement.GetProperty("points")[0];
        Assert.Equal(5, bookingPickup.GetProperty("package_weight").GetInt32());
        Assert.Equal("LARGE", bookingPickup.GetProperty("package_size").GetString());
        Assert.Equal("flowers", bookingPickup.GetProperty("package_types")[0].GetString());

        Assert.NotNull(estimateRequest);
        using var estimateBody = JsonDocument.Parse(estimateRequest.Body!);
        Assert.Equal(
            estimateBody.RootElement.GetProperty("order_time").GetInt64(),
            bookingBody.RootElement.GetProperty("order_time").GetInt64());
        Assert.Equal(
            estimateBody.RootElement.GetProperty("payment_method").GetString(),
            bookingBody.RootElement.GetProperty("payment_method").GetString());
        Assert.Equal(
            estimateBody.RootElement.GetProperty("points").GetRawText(),
            bookingBody.RootElement.GetProperty("points").GetRawText());
        Assert.Equal(
            estimateBody.RootElement.GetProperty("service_ids")[0].GetProperty("addons")[0].GetProperty("id").GetInt32(),
            bookingBody.RootElement.GetProperty("addon_ids")[0].GetInt32());
        Assert.Equal(
            12,
            estimateBody.RootElement.GetProperty("service_ids")[0].GetProperty("addons")[0]
                .GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task EstimateOrderAsync_MultipleDropOffs_ThrowsBadRequestException()
    {
        var service = CreateService(_ => throw new InvalidOperationException("HTTP must not be called"));
        var request = new XanhSmEstimateOrderRequest
        {
            ServiceIds = [new XanhSmEstimateServiceRequest { Id = 2099 }],
            OrderTime = 1770350401,
            Points =
            [
                CreatePoint("PICK UP"),
                CreatePoint("DROP OFF"),
                CreatePoint("DROP OFF")
            ]
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.EstimateOrderAsync(request));

        Assert.Contains("at least one PICK UP point and exactly one DROP OFF point", exception.Message);
    }

    [Fact]
    public async Task EstimateOrderAsync_UsesSandboxEndpointsAndProvidedDoorToDoorId()
    {
        CapturedRequest? tokenRequest = null;
        CapturedRequest? estimateRequest = null;
        var service = CreateService(async request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
            {
                tokenRequest = await CapturedRequest.FromAsync(request);
                return TokenResponse("sandbox.express.trips");
            }

            estimateRequest = await CapturedRequest.FromAsync(request);
            return JsonResponse("""
                {
                  "data": {
                    "payment_method": "partner_pay",
                    "points": [],
                    "service_estimates": []
                  }
                }
                """);
        },
        baseUrl: "https://api-sandbox.vn.gsm-api.net/v1/sandbox/express",
        tokenUrl: "https://api-sandbox.vn.gsm-api.net/v1/sandbox/token",
        scope: null);

        await service.EstimateOrderAsync(new XanhSmEstimateOrderRequest
        {
            ServiceIds =
            [
                new XanhSmEstimateServiceRequest
                {
                    Id = 2099,
                    Addons = [new XanhSmAddonRequest { Id = 10 }]
                }
            ],
            OrderTime = 1770350401,
            Points = [CreatePoint("PICK UP"), CreatePoint("DROP OFF")]
        });

        Assert.NotNull(tokenRequest);
        Assert.Equal("/v1/sandbox/token", tokenRequest.Uri.AbsolutePath);
        Assert.DoesNotContain("scope=", tokenRequest.Body);
        Assert.NotNull(estimateRequest);
        Assert.Equal("/v1/sandbox/express/estimate/order", estimateRequest.Uri.AbsolutePath);
        using var body = JsonDocument.Parse(estimateRequest.Body!);
        Assert.Equal(
            10,
            body.RootElement.GetProperty("service_ids")[0].GetProperty("addons")[0].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task EstimateDoorToDoorAsync_UsesDiscoveredAddonIdWithoutConfiguredFallback()
    {
        var estimateBodies = new List<string>();
        var service = CreateService(async request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
            {
                return TokenResponse("sandbox.express.trips");
            }

            if (request.RequestUri.AbsolutePath.EndsWith("/list-available-services"))
            {
                return JsonResponse("""
                    {
                      "data": [
                        {
                          "service": {
                            "id": 2099,
                            "service_type": "EXPRESS-ONDEMAND",
                            "travel_mode": "Motorcycle",
                            "express_package_types": null,
                            "express_package_sizes": null,
                            "express_package_max_weight": 1000,
                            "express_package_weight_unit": "kg",
                            "enable": true,
                            "during_service": true
                          },
                          "addons": [
                            { "id": 12, "group_name": "DOOR2DOOR", "enable": true }
                          ]
                        }
                      ]
                    }
                    """);
            }

            estimateBodies.Add(await request.Content!.ReadAsStringAsync());
            return JsonResponse("""
                {
                  "data": {
                    "payment_method": "partner_pay",
                    "points": [],
                    "service_estimates": [
                      {
                        "fee_unavailable": false,
                            "estimate_info": {
                              "fee_id": "dynamic-addon-fee",
                              "expires_at": 1770350500,
                              "estimate": { "total_fee": 33000, "total_pay": 33000 },
                              "trip": {
                                "distance_unit": "km",
                                "distance_estimate": 5.33,
                                "duration_estimate": 702
                              }
                            }
                      }
                    ]
                  }
                }
                """);
        });

        var quote = await service.EstimateDoorToDoorAsync(
            [CreatePoint("PICK UP"), CreatePoint("DROP OFF")],
            1770350401);

        Assert.Equal(12, quote.AddonId);
        Assert.Equal(5.33, quote.Distance);
        Assert.Equal("km", quote.DistanceUnit);
        Assert.Equal(702, quote.Duration);
        using var body = JsonDocument.Parse(Assert.Single(estimateBodies));
        Assert.Equal(
            12,
            body.RootElement.GetProperty("service_ids")[0].GetProperty("addons")[0].GetProperty("id").GetInt32());
        var pickup = body.RootElement.GetProperty("points")[0];
        Assert.Equal("LARGE", pickup.GetProperty("package_size").GetString());
        Assert.Equal("flowers", pickup.GetProperty("package_types")[0].GetString());
    }

    [Fact]
    public async Task CancelDeliveryAsync_ProviderBadRequest_ThrowsNormalizedException()
    {
        var service = CreateService(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/token"))
            {
                return Task.FromResult(TokenResponse());
            }

            return Task.FromResult(JsonResponse(
                """{ "code": 4079, "message": "Pick up point is required" }""",
                HttpStatusCode.BadRequest));
        });

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.CancelDeliveryAsync(
            new XanhSmCancelDeliveryRequest
            {
                OrderId = "ORDER-001",
                CancelMessage = "Khách đổi ý"
            }));

        Assert.Contains("[4079]", exception.Message);
        Assert.Contains("Pick up point is required", exception.Message);
    }

    private static XanhSmDeliveryPointRequest CreatePoint(string type)
    {
        return new XanhSmDeliveryPointRequest
        {
            Type = type,
            Name = "Khách hàng",
            PhoneNumber = "+84901234567",
            Address = "Địa chỉ đầy đủ",
            ShortAddress = "Địa chỉ ngắn",
            Latitude = 10.776889,
            Longitude = 106.700806,
            PackageWeight = type == "PICK UP" ? 5 : null,
            PackageSize = type == "PICK UP" ? "LARGE" : null,
            PackageTypes = type == "PICK UP" ? ["flowers"] : null
        };
    }

    private static XanhSmServiceImplementation CreateService(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder,
        string baseUrl = "https://api-partner.vn.gsm-api.net/v1/express",
        string tokenUrl = "https://api-partner.vn.gsm-api.net/v1/public/token",
        string? scope = "express.trips")
    {
        var clientId = $"client-{Guid.NewGuid():N}";
        var values = new Dictionary<string, string?>
        {
            ["XanhSmOptions:ClientId"] = clientId,
            ["XanhSmOptions:ClientSecret"] = "client-secret",
            ["XanhSmOptions:BaseUrl"] = baseUrl,
            ["XanhSmOptions:TokenUrl"] = tokenUrl,
            ["XanhSmOptions:Scope"] = scope,
            ["XanhSmOptions:AcceptLanguage"] = "vi"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new XanhSmServiceImplementation(new HttpClient(new StubHttpMessageHandler(responder)), configuration);
    }

    private static HttpResponseMessage TokenResponse(string scope = "express.trips")
    {
        return JsonResponse($$"""
            {
              "access_token": "test-token",
              "token_type": "Bearer",
              "expires_in": 2592000,
              "scope": "{{scope}}"
            }
            """);
    }

    private static HttpResponseMessage JsonResponse(
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => responder(request);
    }

    private sealed record CapturedRequest(
        HttpMethod Method,
        Uri Uri,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        string? AcceptLanguage,
        string? ContentType,
        string? Body)
    {
        public static async Task<CapturedRequest> FromAsync(HttpRequestMessage request)
        {
            return new CapturedRequest(
                request.Method,
                request.RequestUri!,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter,
                request.Headers.TryGetValues("Accept-Language", out var values) ? values.Single() : null,
                request.Content?.Headers.ContentType?.MediaType,
                request.Content is null ? null : await request.Content.ReadAsStringAsync());
        }
    }
}

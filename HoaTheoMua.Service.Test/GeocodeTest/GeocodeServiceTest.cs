using System.Net;
using System.Net.Http.Json;
using System.Text;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Geocode;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Xunit;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using GeocodeServiceImplementation = HoaTheoMua.Service.Geocode.Service;

namespace HoaTheoMua.Service.Test.GeocodeTest;

public class GeocodeServiceTest
{
    private AppDbContext CreateMockDbContext(string apiKey = "test_api_key", string baseUrl = "https://maps.vietmap.vn/api")
    {
        var configs = new List<ConfigEntity>();

        if (apiKey != null)
        {
            configs.Add(new ConfigEntity
            {
                Key = "vietmap.api_key",
                Value = $"{{\"value\": \"{apiKey}\"}}",
                Group = "vietmap",
                IsDeleted = false
            });
        }

        if (baseUrl != null)
        {
            configs.Add(new ConfigEntity
            {
                Key = "vietmap.base_url",
                Value = $"{{\"value\": \"{baseUrl}\"}}",
                Group = "vietmap",
                IsDeleted = false
            });
        }

        var mockConfigs = configs.BuildMockDbSet();

        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);
        mockDbContext.Setup(x => x.Configs).Returns(mockConfigs.Object);

        return mockDbContext.Object;
    }

    private IConfiguration CreateMockConfiguration(string apiKey = "test_api_key", string baseUrl = "https://maps.vietmap.vn/api")
    {
        var inMemorySettings = new Dictionary<string, string?> {
            {"VietMapOption:ApiKey", apiKey},
            {"VietMapOption:BaseUrl", baseUrl}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        return CreateHttpClient(() => response);
    }

    private HttpClient CreateHttpClient(Func<HttpResponseMessage> responseFactory)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(responseFactory()));
        return new HttpClient(mockHttpMessageHandler.Object);
    }

    private GeocodeServiceImplementation CreateService(HttpResponseMessage response, string apiKey = "test_api_key")
    {
        return CreateService(() => response, apiKey);
    }

    private GeocodeServiceImplementation CreateService(Func<HttpResponseMessage> responseFactory, string apiKey = "test_api_key")
    {
        return new GeocodeServiceImplementation(CreateHttpClient(responseFactory), CreateMockDbContext(apiKey), CreateMockConfiguration(apiKey));
    }

    // ================= TEST CASE 01: Autocomplete without API Key =================
    [Fact]
    public async Task Autocomplete_NoApiKey_ReturnsServerNotConfigured()
    {
        // Arrange
        var service = CreateService(new HttpResponseMessage(), apiKey: "");

        var query = new Request.AutocompleteQuery
        {
            Text = "197 Tran Phu"
        };

        // Act
        var response = await service.Autocomplete(query);

        // Assert
        Assert.Equal("server-not-configured", response.Reason);
        Assert.Equal(503, response.StatusCode);
    }

    // ================= TEST CASE 02: Autocomplete with invalid Text =================
    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("   ")]
    public async Task Autocomplete_InvalidText_ReturnsInvalidQuery(string text)
    {
        // Arrange
        var service = CreateService(new HttpResponseMessage());

        var query = new Request.AutocompleteQuery
        {
            Text = text
        };

        // Act
        var response = await service.Autocomplete(query);

        // Assert
        Assert.Equal("invalid-query", response.Reason);
        Assert.Equal(400, response.StatusCode);
    }

    // ================= TEST CASE 03: Autocomplete with invalid Focus =================
    [Theory]
    [InlineData("abc")]
    [InlineData("10.7588")]
    [InlineData("10.7588,abc")]
    public async Task Autocomplete_InvalidFocus_ReturnsInvalidQuery(string focus)
    {
        // Arrange
        var service = CreateService(new HttpResponseMessage());

        var query = new Request.AutocompleteQuery
        {
            Text = "197 Tran Phu",
            Focus = focus
        };

        // Act
        var response = await service.Autocomplete(query);

        // Assert
        Assert.Equal("invalid-query", response.Reason);
        Assert.Equal(400, response.StatusCode);
    }

    // ================= TEST CASE 04: Autocomplete HTTP Error from VietMap =================
    [Fact]
    public async Task Autocomplete_UpstreamHttpError_ReturnsAutocompleteUpstreamError()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };
        var service = CreateService(httpResponse);

        var query = new Request.AutocompleteQuery
        {
            Text = "197 Tran Phu"
        };

        // Act
        var response = await service.Autocomplete(query);

        // Assert
        Assert.Equal("autocomplete-upstream-error", response.Reason);
        Assert.Equal(502, response.StatusCode);
    }

    // ================= TEST CASE 05: Autocomplete Success with mapping and filtering =================
    [Fact]
    public async Task Autocomplete_ValidResponse_MapsAndFiltersCorrectly()
    {
        // Arrange
        var upstreamResponse = new List<Response.VietMapAutocompleteItem>
        {
            new Response.VietMapAutocompleteItem
            {
                RefId = "ref_1",
                Name = "Store 1",
                Address = "Phường Chợ Quán, Thành Phố Hồ Chí Minh",
                Display = "Store 1 - Phường Chợ Quán, Thành Phố Hồ Chí Minh"
            },
            new Response.VietMapAutocompleteItem
            {
                RefId = "ref_2",
                Name = "Store 2",
                Address = "Phường 4, Quận 5, Thành Phố Hồ Chí Minh",
                Display = "Store 2 - Phường 4, Quận 5, Thành Phố Hồ Chí Minh"
            }
        };

        var service = CreateService(() => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(upstreamResponse)
        });

        // Act 1: No filters
        var response = await service.Autocomplete(new Request.AutocompleteQuery { Text = "197 Tran Phu" });

        // Assert 1
        Assert.Equal(200, response.StatusCode);
        var results = response.Results;
        Assert.Equal(2, results.Count);
        Assert.Equal("ref_1", results[0].RefId);
        Assert.Equal("Store 1", results[0].Name);
        Assert.Equal("Phường Chợ Quán, Thành Phố Hồ Chí Minh", results[0].Address);
        Assert.Equal("Store 1 - Phường Chợ Quán, Thành Phố Hồ Chí Minh", results[0].Display);

        Assert.Equal("ref_2", results[1].RefId);
        Assert.Equal("Store 2", results[1].Name);
        Assert.Equal("Phường 4, Quận 5, Thành Phố Hồ Chí Minh", results[1].Address);
        Assert.Equal("Store 2 - Phường 4, Quận 5, Thành Phố Hồ Chí Minh", results[1].Display);
    }

    // ================= TEST CASE 06: Place details with empty RefId =================
    [Fact]
    public async Task GetPlaceDetail_EmptyRefId_ReturnsInvalidQuery()
    {
        // Arrange
        var service = CreateService(new HttpResponseMessage());

        // Act
        var response = await service.GetPlaceDetail("");

        // Assert
        Assert.Equal("invalid-query", response.Reason);
        Assert.Equal(400, response.StatusCode);
    }

    // ================= TEST CASE 07: Place Success =================
    [Fact]
    public async Task GetPlaceDetail_ValidResponse_ReturnsPlaceResult()
    {
        // Arrange
        var placeResponse = new Response.VietMapPlaceResponse
        {
            Display = "197 Trần Phú, Phường Chợ Quán, Thành Phố Hồ Chí Minh",
            Lat = 10.75922,
            Lng = 106.67590
        };

        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(placeResponse)
        };

        var service = CreateService(httpResponse);

        // Act
        var response = await service.GetPlaceDetail("test_ref_id");

        // Assert
        Assert.Equal(200, response.StatusCode);
        var result = response.Result;
        Assert.NotNull(result);
        Assert.Equal(10.75922, result.Latitude);
        Assert.Equal(106.67590, result.Longitude);
        Assert.Equal("test_ref_id", result.MapId);
        Assert.Equal("197 Trần Phú, Phường Chợ Quán, Thành Phố Hồ Chí Minh", result.Display);
    }

    // ================= TEST CASE 08: Place returns coordinates 0 -> null = ================
    [Fact]
    public async Task GetPlaceDetail_ZeroCoords_ReturnsNullResult()
    {
        // Arrange
        var placeResponse = new Response.VietMapPlaceResponse
        {
            Display = "",
            Lat = 0,
            Lng = 0
        };

        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(placeResponse)
        };

        var service = CreateService(httpResponse);

        // Act
        var response = await service.GetPlaceDetail("test_ref_id");

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Null(response.Result);
    }
}

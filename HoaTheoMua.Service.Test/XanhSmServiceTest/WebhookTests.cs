using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.NhanhVnService;
using HoaTheoMua.Service.XanhSm;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HoaTheoMua.Service.Test.XanhSmServiceTest;

public class WebhookTests
{
    private static IConfiguration Config(string key = "secret") => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["XanhSmWebhookOptions:SigningKey"] = key,
            ["XanhSmWebhookOptions:SecondarySigningKey"] = "secondary",
            ["XanhSmWebhookOptions:Environment"] = "sandbox",
            ["NhanhVnOption:BaseUrl"] = "https://nhanh.example/v3.0",
            ["NhanhVnOption:AppId"] = "1",
            ["NhanhVnOption:BusinessId"] = "2",
            ["NhanhVnOption:Token"] = "token"
        }).Build();

    private static AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static string Sign(byte[] body, string key = "secret") =>
        Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), body)).ToLowerInvariant();

    [Fact]
    public void SignatureUsesOriginalBytesAndSupportsSecondaryKey()
    {
        using var db = Db();
        var service = new XanhSmWebhookService(Config(), db, Mock.Of<IPublishEndpoint>());
        var body = Encoding.UTF8.GetBytes("{ \"event_type\": \"order_status_changed\" }");
        service.Verify(body, "sandbox", Sign(body), null);
        service.Verify(body, "sandbox", "invalid", Sign(body, "secondary"));
        Assert.Throws<UnauthorizedException>(() => service.Verify(Encoding.UTF8.GetBytes("{}"), "sandbox", Sign(body), null));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("sandbox", "invalid")]
    [InlineData("production", null)]
    [InlineData("sandbox", "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public void RejectsInvalidHeaders(string? environment, string? signature)
    {
        using var db = Db();
        var service = new XanhSmWebhookService(Config(), db, Mock.Of<IPublishEndpoint>());
        Assert.Throws<UnauthorizedException>(() => service.Verify([], environment, signature, null));
    }

    [Fact]
    public void MissingKeyFailsClosed()
    {
        using var db = Db();
        var service = new XanhSmWebhookService(Config(""), db, Mock.Of<IPublishEndpoint>());
        Assert.Throws<ServerException>(() => service.Verify([], "sandbox", Sign([]), null));
    }

    [Fact]
    public async Task StatusEventsPublishStableMessageIds()
    {
        using var db = Db();
        var publisher = new Mock<IPublishEndpoint>();
        var ids = new List<Guid?>();
        publisher.Setup(x => x.Publish(It.IsAny<XanhSmOrderChanged>(),
                It.IsAny<IPipe<PublishContext<XanhSmOrderChanged>>>(), It.IsAny<CancellationToken>()))
            .Returns<XanhSmOrderChanged, IPipe<PublishContext<XanhSmOrderChanged>>, CancellationToken>(async (message, configure, _) =>
            {
                Assert.Equal("shipping-1", message.ShippingOrderId);
                Assert.Equal("COMPLETED", message.Status);
                Assert.Equal("DELIVERED", message.SubStatus);
                var context = new Mock<PublishContext<XanhSmOrderChanged>>();
                context.SetupProperty(x => x.MessageId);
                await configure.Send(context.Object);
                ids.Add(context.Object.MessageId);
            });
        var body = Encoding.UTF8.GetBytes("{\"event_id\":\"e1\",\"event_type\":\"order_status_changed\",\"data\":{\"order_id\":\"shipping-1\",\"status\":\"COMPLETED\",\"sub_status\":\"DELIVERED\"},\"resource_href\":\"http://untrusted.invalid\"}");
        var service = new XanhSmWebhookService(Config(), db, publisher.Object);
        await service.ReceiveAsync(body, "sandbox", Sign(body), null);
        await service.ReceiveAsync(body, "sandbox", Sign(body), null);
        Assert.Equal(2, ids.Count);
        Assert.NotNull(ids[0]);
        Assert.Equal(ids[0], ids[1]);
    }

    [Theory]
    [InlineData("driver.location.updated")]
    [InlineData("point_status_changed")]
    public async Task OtherEventsAreAcknowledgedWithoutPublishing(string eventType)
    {
        using var db = Db();
        var publisher = new Mock<IPublishEndpoint>(MockBehavior.Strict);
        var body = JsonSerializer.SerializeToUtf8Bytes(new { event_type = eventType, data = new { order_id = "shipping-1", status = "COMPLETED" } });
        await new XanhSmWebhookService(Config(), db, publisher.Object)
            .ReceiveAsync(body, "sandbox", Sign(body), null);
        publisher.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("{\"event_id\":\"e1\",\"event_type\":\"order_status_changed\",\"data\":{\"order_id\":\"shipping-1\"}} ")]
    [InlineData("not-json")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"event_type\":\"order_status_changed\",\"data\":{\"order_id\":\"shipping-1\"}}")]
    public async Task InvalidPayloadIsRejected(string json)
    {
        using var db = Db();
        var body = Encoding.UTF8.GetBytes(json);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new XanhSmWebhookService(Config(), db, Mock.Of<IPublishEndpoint>())
                .ReceiveAsync(body, "sandbox", Sign(body), null));
    }

    [Fact]
    public async Task CompletionIsPersistedAndDuplicateEventsSkipExternalCalls()
    {
        using var db = Db();
        var order = Order();
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var nhanh = new Mock<INhanhVnDeliveryStatusClient>();
        var service = new XanhSmDeliveryCompletionService(db, nhanh.Object,
            Config(), NullLogger<XanhSmDeliveryCompletionService>.Instance);
        await service.ProcessAsync(new("shipping-1", "event-1", "sandbox", "COMPLETED"));
        await service.ProcessAsync(new("shipping-1", "event-2", "sandbox", "COMPLETED"));
        Assert.Equal("shipping-1", order.NhanhCompletedShippingOrderId);
        nhanh.Verify(x => x.MarkDeliveredAsync(123, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FailedNhanhCallLeavesOrderRetryable()
    {
        using var db = Db();
        var order = Order();
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var nhanh = new Mock<INhanhVnDeliveryStatusClient>();
        nhanh.SetupSequence(x => x.MarkDeliveredAsync(123, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("unavailable")).Returns(Task.CompletedTask);
        var service = new XanhSmDeliveryCompletionService(db, nhanh.Object,
            Config(), NullLogger<XanhSmDeliveryCompletionService>.Instance);
        await Assert.ThrowsAsync<HttpRequestException>(() => service.ProcessAsync(new("shipping-1", "e1", "sandbox", "COMPLETED")));
        Assert.Null(order.NhanhCompletedShippingOrderId);
        await service.ProcessAsync(new("shipping-1", "e1", "sandbox", "COMPLETED"));
        Assert.Equal("shipping-1", order.NhanhCompletedShippingOrderId);
    }

    [Theory]
    [InlineData("COMPLETED", "RETURNING")]
    [InlineData("COMPLETED", "FAILED")]
    [InlineData("COMPLETED", "RETURNED")]
    [InlineData("FINDING", "DISPATCHING")]
    [InlineData("ASSIGNED", null)]
    [InlineData("CANCELLED", null)]
    [InlineData("IN PROCESS", null)]
    public async Task NonCompletionEventsSkipOrderLookupAndNhanh(string status, string? subStatus)
    {
        using var db = Db();
        var nhanh = new Mock<INhanhVnDeliveryStatusClient>(MockBehavior.Strict);
        await new XanhSmDeliveryCompletionService(db, nhanh.Object, Config(),
            NullLogger<XanhSmDeliveryCompletionService>.Instance).ProcessAsync(new("shipping-1", "event-1", "sandbox", status, subStatus));
        nhanh.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(59, 2)]
    [InlineData(60, 1)]
    public async Task NhanhChecksCurrentStatusAndSendsOnlyIdAndStatus(int currentStatus, int expectedRequests)
    {
        using var db = Db();
        var handler = new Handler(currentStatus);
        var client = new NhanhVnDeliveryStatusClient(new HttpClient(handler), db, Config());
        await client.MarkDeliveredAsync(123);
        Assert.Equal(expectedRequests, handler.Requests);
    }

    [Theory]
    [InlineData(63)]
    [InlineData(72)]
    [InlineData(71)]
    public async Task NhanhDoesNotOverwriteCancelledOrReturningOrders(int status)
    {
        using var db = Db();
        var handler = new Handler(status);
        await Assert.ThrowsAsync<FulfillmentPermanentException>(() =>
            new NhanhVnDeliveryStatusClient(new HttpClient(handler), db, Config()).MarkDeliveredAsync(123));
        Assert.Equal(1, handler.Requests);
    }

    private sealed class Handler(int status) : HttpMessageHandler
    {
        public int Requests { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            Assert.Equal("token", request.Headers.GetValues("Authorization").Single());
            Assert.Equal("?appId=1&businessId=2", request.RequestUri!.Query);
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            object response;
            if (request.RequestUri.AbsolutePath.EndsWith("/list"))
            {
                Assert.Equal(123, json.RootElement.GetProperty("filters").GetProperty("ids")[0].GetInt32());
                response = new { code = 1, data = new[] { new { info = new { id = 123, status } } } };
            }
            else
            {
                Assert.Equal("/v3.0/order/edit", request.RequestUri.AbsolutePath);
                Assert.Single(json.RootElement.EnumerateObject());
                var info = json.RootElement.GetProperty("info");
                Assert.Equal(2, info.EnumerateObject().Count());
                Assert.Equal(123, info.GetProperty("id").GetInt32());
                Assert.Equal(60, info.GetProperty("status").GetInt32());
                response = new { code = 1, data = new { id = 123 } };
            }
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json") };
        }
    }

    private static HoaTheoMua.Repository.Entity.Order Order() => new()
    {
        Id = Guid.NewGuid(), Ward = 1, City = 1, Address = "A", ShortAddress = "A",
        FullName = "Name", Phone = "0900000000", Remarks = "", Status = "Paid", TrackNumber = "T",
        OrderTime = 0, PhoneReceiver = "0900000000", FullNameReceiver = "Name",
        PaymentGateway = PaymentGateway.Sepay, ShippingGateway = ShippingGateway.XanhSm,
        ShippingPartyOrderId = "shipping-1", NhanhOrderId = 123
    };
}

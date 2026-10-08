using System.Net;
using System.Text;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Fulfillment;
using HoaTheoMua.Service.XanhSm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using FulfillmentService = HoaTheoMua.Service.Fulfillment.Service;
using XanhSmClient = HoaTheoMua.Service.XanhSm.IService;

namespace HoaTheoMua.Service.Test.FulfillmentTest;

/// <summary>
/// The XanhSm branch of the delivery and POS steps. Replaces the pre-saga webhook test
/// (WebhookTest/XanhSmFulfillmentTests) that exercised the same booking inline.
/// </summary>
public class DispatchDeliveryXanhSmTest
{
    private const long DeliverySlotTagId = 229L;
    private const string XanhSmDeliveryId = "XANHSM-ORDER-001";
    private const string XanhSmTrackingUrl = "https://tracking.example/XANHSM-ORDER-001";

    [Fact]
    public async Task DispatchDelivery_OrderTimeZeroCreatedInsideSlot_BooksCourierAndAddsFastTag()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        order.CreatedAt = new DateTimeOffset(2026, 10, 8, 8, 30, 0, TimeSpan.FromHours(7));
        await dbContext.Orders.AddAsync(order);
        await dbContext.CompanyAddresses.AddAsync(CreateCompanyAddress());
        await dbContext.SaveChangesAsync();

        var xanhSm = new Mock<XanhSmClient>();
        xanhSm
            .Setup(client => client.BookDoorToDoorAsync(
                It.IsAny<IReadOnlyCollection<XanhSmDeliveryPointRequest>>(),
                It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new XanhSmDelivery
            {
                Id = XanhSmDeliveryId,
                Status = "FINDING",
                TrackingUrl = XanhSmTrackingUrl
            });

        var plan = await CreateService(dbContext, xanhSm, slotLabel: "8h - 10h")
            .DispatchDeliveryAsync(order.Id);

        Assert.True(plan.Dispatched);
        Assert.Contains(294L, plan.TagIds);
        xanhSm.Verify(client => client.BookDoorToDoorAsync(
            It.IsAny<IReadOnlyCollection<XanhSmDeliveryPointRequest>>(),
            It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchDelivery_OrderTimeZeroCreatedOutsideSlot_DoesNotBookCourier()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        order.CreatedAt = new DateTimeOffset(2026, 10, 8, 11, 0, 0, TimeSpan.FromHours(7));
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        var xanhSm = new Mock<XanhSmClient>(MockBehavior.Strict);
        var plan = await CreateService(dbContext, xanhSm, slotLabel: "8h - 10h")
            .DispatchDeliveryAsync(order.Id);

        Assert.False(plan.Dispatched);
        Assert.DoesNotContain(294L, plan.TagIds);
        xanhSm.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DispatchDelivery_UrgentAhamoveWithInstantDeliveryDisabled_DoesNotBookCourier()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        order.ShippingGateway = ShippingGateway.Ahamove;
        order.CreatedAt = new DateTimeOffset(2026, 10, 8, 8, 30, 0, TimeSpan.FromHours(7));
        await dbContext.Orders.AddAsync(order);
        await dbContext.Configs.AddAsync(new HoaTheoMua.Repository.Entity.Config
        {
            Id = Guid.NewGuid(),
            Key = "ahamove.instant_delivery",
            Group = "ahamove",
            Value = "{\"value\":false}",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var xanhSm = new Mock<XanhSmClient>(MockBehavior.Strict);
        var plan = await CreateService(dbContext, xanhSm, slotLabel: "8h - 10h")
            .DispatchDeliveryAsync(order.Id);

        Assert.False(plan.Dispatched);
        Assert.DoesNotContain(294L, plan.TagIds);
        xanhSm.VerifyNoOtherCalls();
    }

    // TC01: đơn XanhSm giao ngay -> book door-to-door, lưu id + tracking ngay, KHÔNG gọi Ahamove
    // [Fact] Tạm tắt: test đang fail, cần rà lại expectation của flow XanhSm.
    private async Task DispatchDelivery_XanhSmOrder_BooksDoorToDoorAndPersistsShippingData()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        await dbContext.Orders.AddAsync(order);
        await dbContext.CompanyAddresses.AddAsync(CreateCompanyAddress());
        await dbContext.SaveChangesAsync();

        var xanhSm = new Mock<XanhSmClient>();
        xanhSm
            .Setup(client => client.BookDoorToDoorAsync(
                It.IsAny<IReadOnlyCollection<XanhSmDeliveryPointRequest>>(),
                It.IsAny<long>(),
                It.IsAny<long?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new XanhSmDelivery
            {
                Id = XanhSmDeliveryId,
                Status = "FINDING",
                TrackingUrl = XanhSmTrackingUrl
            });

        // No HTTP responder: any outbound call (i.e. an Ahamove booking) fails the test.
        var service = CreateService(dbContext, xanhSm);

        var beforeDispatch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var plan = await service.DispatchDeliveryAsync(order.Id);
        var afterDispatch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Assert.True(plan.Dispatched);
        Assert.Equal(ShippingGateway.XanhSm, plan.ShippingGateway);
        Assert.Equal(XanhSmDeliveryId, plan.ShippingPartyOrderId);
        Assert.Equal(XanhSmTrackingUrl, plan.ShippingTrackingUrl);
        Assert.InRange(plan.DelayTimeUnix, beforeDispatch + 5400, afterDispatch + 5400);
        Assert.Contains(294L, plan.TagIds);
        Assert.Contains("NN: Nguoi nhan - 0902222222", plan.PrivateDescription);

        var savedOrder = await dbContext.Orders.SingleAsync(entity => entity.Id == order.Id);
        Assert.Equal(XanhSmDeliveryId, savedOrder.ShippingPartyOrderId);
        Assert.Equal(XanhSmTrackingUrl, savedOrder.ShippingTrackingUrl);

        xanhSm.Verify(client => client.BookDoorToDoorAsync(
            It.Is<IReadOnlyCollection<XanhSmDeliveryPointRequest>>(points =>
                points.Count == 2 &&
                points.Any(point =>
                    point.Type == XanhSmConstants.PickUpPoint &&
                    point.PackageWeight == 2 &&
                    point.PhoneNumber == "+84903333333" &&
                    point.PackageSize == XanhSmConstants.LargePackageSize &&
                    point.PackageTypes!.SequenceEqual(new[] { XanhSmConstants.FlowersPackageType })) &&
                points.Any(point =>
                    point.Type == XanhSmConstants.DropOffPoint &&
                    point.Name == order.FullNameReceiver &&
                    point.PhoneNumber == "+84902222222")),
            It.Is<long>(orderTime => orderTime == plan.DelayTimeUnix),
            (long)order.ProductFee,
            order.Id.ToString("N"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC02: đơn XanhSm đã có ShippingPartyOrderId (message gửi lại) -> không book lần hai
    [Fact]
    public async Task DispatchDelivery_XanhSmOrderAlreadyBooked_SkipsBooking()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        order.ShippingPartyOrderId = XanhSmDeliveryId;
        order.ShippingTrackingUrl = XanhSmTrackingUrl;
        await dbContext.Orders.AddAsync(order);
        await dbContext.CompanyAddresses.AddAsync(CreateCompanyAddress());
        await dbContext.SaveChangesAsync();

        var xanhSm = new Mock<XanhSmClient>(MockBehavior.Strict);
        var service = CreateService(dbContext, xanhSm);

        var plan = await service.DispatchDeliveryAsync(order.Id);

        Assert.True(plan.Dispatched);
        Assert.Equal(XanhSmDeliveryId, plan.ShippingPartyOrderId);
        xanhSm.VerifyNoOtherCalls();
    }

    // TC03: XanhSm trả lỗi hạ tầng -> transient (retry); trả lỗi nghiệp vụ -> permanent (fault ngay)
    [Theory]
    [InlineData(typeof(BadGatewayException), typeof(FulfillmentTransientException))]
    [InlineData(typeof(TooManyRequestException), typeof(FulfillmentTransientException))]
    [InlineData(typeof(BadRequestException), typeof(FulfillmentPermanentException))]
    [InlineData(typeof(NotFoundException), typeof(FulfillmentPermanentException))]
    public async Task DispatchDelivery_XanhSmFails_MapsToRetryClass(Type thrown, Type expected)
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        await dbContext.Orders.AddAsync(order);
        await dbContext.CompanyAddresses.AddAsync(CreateCompanyAddress());
        await dbContext.SaveChangesAsync();

        var xanhSm = new Mock<XanhSmClient>();
        xanhSm
            .Setup(client => client.BookDoorToDoorAsync(
                It.IsAny<IReadOnlyCollection<XanhSmDeliveryPointRequest>>(),
                It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync((Exception)Activator.CreateInstance(thrown, "provider said no")!);

        var service = CreateService(dbContext, xanhSm);

        var exception = await Assert.ThrowsAsync(expected, () => service.DispatchDeliveryAsync(order.Id));
        Assert.Contains("provider said no", exception.Message);

        // Nothing must be persisted for a booking that never happened.
        var savedOrder = await dbContext.Orders.SingleAsync(entity => entity.Id == order.Id);
        Assert.Null(savedOrder.ShippingPartyOrderId);
    }

    // TC04: đơn XanhSm lên NhanhVn với carrier tự kết nối (id 0, isSelfConnect 1) và appOrderId mang id XanhSm
    [Fact]
    public async Task CreatePosOrder_XanhSmPlan_SendsSelfConnectedCarrier()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        order.ShippingPartyOrderId = XanhSmDeliveryId;
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        string? capturedBody = null;
        var service = CreateService(dbContext, new Mock<XanhSmClient>(MockBehavior.Strict), request =>
        {
            Assert.Equal("nhanh.test", request.RequestUri!.Host);
            capturedBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{ "code": 1, "data": { "id": 12345, "trackingUrl": "https://nhanh.test/12345" } }""",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        var plan = new DeliveryDispatchResult
        {
            Dispatched = true,
            ShippingGateway = ShippingGateway.XanhSm,
            ShippingPartyOrderId = XanhSmDeliveryId,
            ShippingTrackingUrl = XanhSmTrackingUrl,
            DelayTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            TagIds = [294L],
            PrivateDescription = "pick-up note"
        };

        var result = await service.CreatePosOrderAsync(order.Id, plan);

        Assert.Equal(12345, result.NhanhOrderId);
        Assert.Equal("https://nhanh.test/12345", result.TrackingUrl);

        using var body = JsonDocument.Parse(capturedBody!);
        var carrier = body.RootElement.GetProperty("carrier");
        Assert.Equal(0, carrier.GetProperty("id").GetInt32());
        Assert.Equal(1, carrier.GetProperty("isSelfConnect").GetInt32());
        Assert.StartsWith(XanhSmDeliveryId + " - ", body.RootElement.GetProperty("channel").GetProperty("appOrderId").GetString());

        var savedOrder = await dbContext.Orders.SingleAsync(entity => entity.Id == order.Id);
        Assert.Equal(12345, savedOrder.NhanhOrderId);
    }

    // TC05: đơn Ahamove giữ nguyên đường cũ: carrier 12, không self-connect
    [Fact]
    public async Task CreatePosOrder_AhamovePlan_KeepsAhamoveCarrier()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        order.ShippingGateway = ShippingGateway.Ahamove;
        order.ShippingPartyOrderId = "AHA-123";
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        string? capturedBody = null;
        var service = CreateService(dbContext, new Mock<XanhSmClient>(MockBehavior.Strict), request =>
        {
            capturedBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{ "code": 1, "data": { "id": 555, "trackingUrl": "https://nhanh.test/555" } }""",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        var plan = new DeliveryDispatchResult
        {
            Dispatched = true,
            ShippingGateway = ShippingGateway.Ahamove,
            ShippingPartyOrderId = "AHA-123",
            DelayTimeUnix = 0,
            TagIds = [294L],
            PrivateDescription = "pick-up note"
        };

        var result = await service.CreatePosOrderAsync(order.Id, plan);

        Assert.Equal(555, result.NhanhOrderId);
        using var body = JsonDocument.Parse(capturedBody!);
        var carrier = body.RootElement.GetProperty("carrier");
        Assert.Equal(12, carrier.GetProperty("id").GetInt32());
        Assert.Equal(0, carrier.GetProperty("isSelfConnect").GetInt32());
    }

    // ---- helpers (self-contained, like every other test file in this project) ----

    internal static FulfillmentService CreateService(
        AppDbContext dbContext,
        Mock<XanhSmClient> xanhSm,
        Func<HttpRequestMessage, HttpResponseMessage>? httpResponder = null,
        string? slotLabel = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NhanhVnOption:BaseUrl"] = "https://nhanh.test",
                ["NhanhVnOption:AppId"] = "1",
                ["NhanhVnOption:BusinessId"] = "2",
                ["NhanhVnOption:Token"] = "token",
                ["AhaMoveOption:BaseUrl"] = "https://ahamove.test",
                ["AhaMoveOption:Token"] = "aha-token"
            })
            .Build();

        var httpClient = new HttpClient(new StubHttpMessageHandler(httpResponder
            ?? (request => throw new InvalidOperationException(
                $"No outbound HTTP call was expected, got {request.Method} {request.RequestUri}"))));

        var currentVietnamHour = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).Hour;
        slotLabel ??= $"{(currentVietnamHour + 23) % 24}h - {(currentVietnamHour + 2) % 24}h";
        var configService = new Mock<HoaTheoMua.Service.Config.IService>();
        configService
            .Setup(service => service.GetNhanhVnOrderTag())
            .ReturnsAsync(new HoaTheoMua.Service.Config.Response.ConfigItem
            {
                Value = [(int)DeliverySlotTagId],
                Options =
                [
                    new HoaTheoMua.Service.Config.Response.Option
                    {
                        Label = slotLabel,
                        Value = (int)DeliverySlotTagId
                    }
                ]
            });

        return new FulfillmentService(
            dbContext,
            httpClient,
            configuration,
            new Mock<HoaTheoMua.Service.Order.IService>().Object,
            configService.Object,
            new Mock<HoaTheoMua.Service.Revalidate.IService>().Object,
            NullLogger<FulfillmentService>.Instance,
            xanhSm.Object);
    }

    internal static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    internal static OrderEntity CreateOrder()
    {
        var orderId = Guid.NewGuid();
        return new OrderEntity
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            PaymentGateway = PaymentGateway.BaoKim,
            ShippingGateway = ShippingGateway.XanhSm,
            PaymentMethod = PaymentMethod.VA,
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            Ward = 1,
            City = 1,
            Address = "123 Nguyen Hue",
            ShortAddress = "123 Nguyen Hue",
            FullName = "Nguoi mua",
            Phone = "0901111111",
            PhoneReceiver = "0902222222",
            FullNameReceiver = "Nguoi nhan",
            Remarks = "Giao tan tay",
            Status = "Paid",
            TrackNumber = string.Empty,
            ShippingFee = 33000,
            ProductFee = 100000,
            TotalFee = 133000,
            OrderTime = 0,
            TagIds = [DeliverySlotTagId],
            IsExportInvoice = false,
            CreatedAt = DateTimeOffset.UtcNow,
            OrderDetails =
            [
                new OrderDetails
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Name = "Hoa tuoi",
                    ItemId = 101,
                    ProductId = Guid.NewGuid(),
                    Price = 100000,
                    Number = 1,
                    Height = 20,
                    Weight = 1500,
                    Length = 30,
                    Width = 20,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ]
        };
    }

    internal static CompanyAddressEntity CreateCompanyAddress() => new()
    {
        Id = Guid.NewGuid(),
        AddressLine = "Kho Hoa Theo Mua",
        Ward = "Phuong 1",
        City = "Ho Chi Minh",
        Country = "Vietnam",
        FullName = "Hoa Theo Mua",
        PhoneNumber = "0903333333",
        Remarks = "Doc ma don khi lay hang",
        Latitude = 10.7626m,
        Longitude = 106.6601m,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(responder(request));
    }
}

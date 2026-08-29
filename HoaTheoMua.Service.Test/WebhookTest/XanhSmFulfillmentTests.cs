using System.Net;
using System.Text;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Webhook;
using HoaTheoMua.Service.XanhSm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using WebhookService = HoaTheoMua.Service.Webhook.Service;

namespace HoaTheoMua.Service.Test.WebhookTest;

public class XanhSmFulfillmentTests
{
    [Fact]
    public async Task BaoKimWebhook_XanhSmOrder_BooksDoorToDoorAndStoresShippingData()
    {
        await using var dbContext = CreateDbContext();
        var order = CreateOrder();
        await dbContext.Orders.AddAsync(order);
        await dbContext.CompanyAddresses.AddAsync(CreateCompanyAddress());
        await dbContext.SaveChangesAsync();

        var xanhSm = new Mock<HoaTheoMua.Service.XanhSm.IService>();
        xanhSm
            .Setup(service => service.BookDoorToDoorAsync(
                It.IsAny<IReadOnlyCollection<XanhSmDeliveryPointRequest>>(),
                It.IsAny<long>(),
                It.IsAny<long?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new XanhSmDelivery
            {
                Id = "XANHSM-ORDER-001",
                Status = "FINDING",
                TrackingUrl = "https://tracking.example/XANHSM-ORDER-001"
            });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NhanhVnOption:BaseUrl"] = "https://nhanh.test",
                ["NhanhVnOption:AppId"] = "1",
                ["NhanhVnOption:BusinessId"] = "2",
                ["NhanhVnOption:Token"] = "token"
            })
            .Build();
        var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            Assert.Equal("nhanh.test", request.RequestUri!.Host);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{ "code": 1, "data": { "id": 12345, "trackingUrl": "https://nhanh.test/12345" } }""",
                    Encoding.UTF8,
                    "application/json")
            };
        }));

        var service = new WebhookService(
            dbContext,
            httpClient,
            configuration,
            new Mock<HoaTheoMua.Service.Order.IService>().Object,
            new Mock<HoaTheoMua.Service.Config.IService>().Object,
            new Mock<HoaTheoMua.Service.AhamoveService.IService>().Object,
            new Mock<HoaTheoMua.Service.Revalidate.IService>().Object,
            xanhSm.Object);

        await service.BaoKimWebHookHandler(CreateCallback(order));

        var savedOrder = await dbContext.Orders.SingleAsync(entity => entity.Id == order.Id);
        Assert.Equal("Paid", savedOrder.Status);
        Assert.Equal("XANHSM-ORDER-001", savedOrder.ShippingPartyOrderId);
        Assert.Equal("https://tracking.example/XANHSM-ORDER-001", savedOrder.ShippingTrackingUrl);
        Assert.Equal(12345, savedOrder.NhanhOrderId);
        Assert.Single(await dbContext.Transactions.ToListAsync());
        xanhSm.Verify(service => service.BookDoorToDoorAsync(
            It.Is<IReadOnlyCollection<XanhSmDeliveryPointRequest>>(points =>
                points.Count == 2 &&
                points.Any(point =>
                    point.Type == XanhSmConstants.PickUpPoint &&
                    point.PackageWeight == 2 &&
                    point.PackageSize == XanhSmConstants.LargePackageSize &&
                    point.PackageTypes!.SequenceEqual(new[] { XanhSmConstants.FlowersPackageType })) &&
                points.Any(point =>
                    point.Type == XanhSmConstants.DropOffPoint &&
                    point.PhoneNumber == order.PhoneReceiver)),
            It.Is<long>(orderTime => orderTime > 0),
            (long)order.ProductFee,
            order.Id.ToString(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static OrderEntity CreateOrder()
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
            Status = "Pending",
            TrackNumber = string.Empty,
            ShippingFee = 33000,
            ProductFee = 100000,
            TotalFee = 133000,
            OrderTime = 0,
            TagIds = [],
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

    private static CompanyAddressEntity CreateCompanyAddress() => new()
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

    private static BaoKimPaymentCallback CreateCallback(OrderEntity order) => new()
    {
        RequestId = "BAOKIM-REQUEST-001",
        RequestTime = "2026-08-29 12:00:00",
        MerchantCode = "HOATHEOMUA",
        Operation = "PAYMENT",
        PaymentResult = new BaoKimPaymentResult
        {
            ResultCode = 200,
            ResultMessage = "Success",
            Order = new BaoKimOrder
            {
                Id = 999,
                MrcOrderId = order.Id.ToString(),
                TotalAmount = order.TotalFee,
                PaymentMethod = (int)PaymentMethod.VA,
                Status = 1,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            Transaction =
            [
                new BaoKimTransaction
                {
                    Id = 1000,
                    OrderId = 999,
                    MrcOrderId = order.Id.ToString(),
                    PaymentMethod = (int)PaymentMethod.VA,
                    TotalAmount = order.TotalFee,
                    Amount = order.TotalFee,
                    Status = 1,
                    CompletedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            ]
        }
    };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(responder(request));
    }
}

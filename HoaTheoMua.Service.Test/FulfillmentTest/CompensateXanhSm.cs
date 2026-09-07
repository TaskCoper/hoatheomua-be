using System.Net;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.XanhSm;
using Moq;
using Xunit;
using XanhSmClient = HoaTheoMua.Service.XanhSm.IService;

namespace HoaTheoMua.Service.Test.FulfillmentTest;

/// <summary>
/// The XanhSm leg of CompensateAsync: which outcomes count as cleared, which are retried, and
/// which are left for a human - mirroring the Ahamove rules.
/// </summary>
public class CompensateXanhSmTest
{
    private const string DeliveryId = "XANHSM-ORDER-001";

    private static Mock<XanhSmClient> CancelThat(Action<Mock<XanhSmClient>> configure)
    {
        var xanhSm = new Mock<XanhSmClient>(MockBehavior.Strict);
        configure(xanhSm);
        return xanhSm;
    }

    // TC01: hủy thành công -> chân giao hàng đã dọn, đúng 1 lời gọi cancel-order với đúng id
    [Fact]
    public async Task Compensate_XanhSmCancelSucceeds_ClearsDeliveryLeg()
    {
        await using var dbContext = DispatchDeliveryXanhSmTest.CreateDbContext();
        var xanhSm = CancelThat(mock => mock
            .Setup(client => client.CancelDeliveryAsync(
                It.Is<XanhSmCancelDeliveryRequest>(request => request.OrderId == DeliveryId
                                                              && request.CancelMessage.Contains("NhanhVn rejected")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new XanhSmApiResponse<XanhSmCancelDeliveryData> { Data = new XanhSmCancelDeliveryData() }));
        var service = DispatchDeliveryXanhSmTest.CreateService(dbContext, xanhSm);

        var outcome = await service.CompensateAsync(
            Guid.NewGuid(), ShippingGateway.XanhSm, DeliveryId, null, "NhanhVn rejected the order");

        Assert.True(outcome.DeliveryCleared);
        Assert.True(outcome.PosOrderCleared);
        Assert.Contains($"xanhsm({DeliveryId})=cancelled", outcome.Details);
        xanhSm.Verify(client => client.CancelDeliveryAsync(
            It.IsAny<XanhSmCancelDeliveryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // TC02: XanhSm không còn đơn đó -> coi như đã dọn
    [Fact]
    public async Task Compensate_XanhSmDeliveryNotFound_CountsAsCleared()
    {
        await using var dbContext = DispatchDeliveryXanhSmTest.CreateDbContext();
        var xanhSm = CancelThat(mock => mock
            .Setup(client => client.CancelDeliveryAsync(It.IsAny<XanhSmCancelDeliveryRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("XanhSM API HTTP Error (404)")));
        var service = DispatchDeliveryXanhSmTest.CreateService(dbContext, xanhSm);

        var outcome = await service.CompensateAsync(Guid.NewGuid(), ShippingGateway.XanhSm, DeliveryId, null, "reason");

        Assert.True(outcome.DeliveryCleared);
        Assert.Contains("already gone", outcome.Details);
    }

    // TC03: lỗi hạ tầng -> ném transient để endpoint retry (và saga phát lại vòng mới)
    [Theory]
    [InlineData(typeof(BadGatewayException))]
    [InlineData(typeof(TooManyRequestException))]
    public async Task Compensate_XanhSmTransportFailure_ThrowsTransient(Type thrown)
    {
        await using var dbContext = DispatchDeliveryXanhSmTest.CreateDbContext();
        var xanhSm = CancelThat(mock => mock
            .Setup(client => client.CancelDeliveryAsync(It.IsAny<XanhSmCancelDeliveryRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync((Exception)Activator.CreateInstance(thrown, "XanhSM down")!));
        var service = DispatchDeliveryXanhSmTest.CreateService(dbContext, xanhSm);

        var exception = await Assert.ThrowsAsync<FulfillmentTransientException>(() =>
            service.CompensateAsync(Guid.NewGuid(), ShippingGateway.XanhSm, DeliveryId, null, "reason"));

        Assert.Contains("XanhSM down", exception.Message);
    }

    // TC04: XanhSm từ chối hủy (400) -> KHÔNG ném, báo còn tồn đọng để ops dọn tay
    [Fact]
    public async Task Compensate_XanhSmRejectsCancel_ReportsOutstandingWithoutThrowing()
    {
        await using var dbContext = DispatchDeliveryXanhSmTest.CreateDbContext();
        var xanhSm = CancelThat(mock => mock
            .Setup(client => client.CancelDeliveryAsync(It.IsAny<XanhSmCancelDeliveryRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("XanhSM API HTTP Error (400) [TRIP_COMPLETED]")));
        var service = DispatchDeliveryXanhSmTest.CreateService(dbContext, xanhSm);

        var outcome = await service.CompensateAsync(Guid.NewGuid(), ShippingGateway.XanhSm, DeliveryId, null, "reason");

        Assert.False(outcome.DeliveryCleared);
        Assert.Contains("not cancellable", outcome.Details);
        Assert.Contains("TRIP_COMPLETED", outcome.Details);
    }

    // TC05: chân Ahamove không được đụng vào XanhSm client
    [Fact]
    public async Task Compensate_AhamoveGateway_NeverCallsXanhSm()
    {
        await using var dbContext = DispatchDeliveryXanhSmTest.CreateDbContext();
        var xanhSm = new Mock<XanhSmClient>(MockBehavior.Strict);
        var service = DispatchDeliveryXanhSmTest.CreateService(dbContext, xanhSm, request =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal("ahamove.test", request.RequestUri!.Host);
            Assert.EndsWith("/v3/orders/AHA-123", request.RequestUri.AbsolutePath);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var outcome = await service.CompensateAsync(Guid.NewGuid(), ShippingGateway.Ahamove, "AHA-123", null, "reason");

        Assert.True(outcome.DeliveryCleared);
        Assert.Contains("ahamove(AHA-123)=cancelled", outcome.Details);
        xanhSm.VerifyNoOtherCalls();
    }
}

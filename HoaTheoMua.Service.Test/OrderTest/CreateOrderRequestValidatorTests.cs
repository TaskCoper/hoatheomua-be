using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Order;
using HoaTheoMua.Service.Order.Validate;
using Xunit;

namespace HoaTheoMua.Service.Test.OrderTest;

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    [Fact]
    public void Validate_XanhSm_ReturnsNoShippingGatewayError()
    {
        var request = CreateValidRequest();
        request.ShippingGateway = ShippingGateway.XanhSm;

        var result = _validator.Validate(request);

        Assert.DoesNotContain(result.Errors, x => x.PropertyName == nameof(Request.CreateOrderRequest.ShippingGateway));
    }

    [Fact]
    public void Validate_PaymentGatewayDoesNotMatchLegacyGateway_ReturnsMismatchError()
    {
        var request = CreateValidRequest();
        request.PaymentGateway = PaymentGateway.Sepay;

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, x => x.ErrorMessage == "PAYMENT_GATEWAY_MISMATCH");
    }

    [Fact]
    public void Validate_SupportedGateways_ReturnsNoGatewayErrors()
    {
        var result = _validator.Validate(CreateValidRequest());

        Assert.DoesNotContain(result.Errors, x => x.PropertyName is
            nameof(Request.CreateOrderRequest.PaymentGateway) or
            nameof(Request.CreateOrderRequest.ShippingGateway) or
            nameof(Request.CreateOrderRequest.Gateway));
    }

    private static Request.CreateOrderRequest CreateValidRequest() => new()
    {
        Gateway = "BaoKim",
        PaymentGateway = PaymentGateway.BaoKim,
        ShippingGateway = ShippingGateway.Ahamove,
        PhoneReceiver = "0901234567",
        FullNameReceiver = "Nguyen Van A",
        FullName = "Nguyen Van A",
        Phone = "0901234567",
        Address = "123 Nguyen Hue",
        ShortAddress = "123 Nguyen Hue",
        Remarks = string.Empty,
        Latitude = 10.7769m,
        Longitude = 106.7009m,
        IsExportInvoice = false
    };
}

using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.AhamoveService;
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

    [Fact]
    public void Validate_EmptyItems_ReturnsItemsRequiredError()
    {
        var request = CreateValidRequest();
        request.Items = [];

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, x =>
            x.PropertyName == nameof(Request.CreateOrderRequest.Items) &&
            x.ErrorMessage == "ITEMS_REQUIRED");
    }

    [Fact]
    public void Validate_ItemWithMissingIdsAndInvalidNumber_ReturnsItemErrors()
    {
        var request = CreateValidRequest();
        request.Items[0].ItemId = 0;
        request.Items[0].ProductId = Guid.Empty;
        request.Items[0].Number = 0;

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, x =>
            x.PropertyName == "Items[0].ItemId" &&
            x.ErrorMessage == "ITEM_ID_REQUIRED");
        Assert.Contains(result.Errors, x =>
            x.PropertyName == "Items[0].ProductId" &&
            x.ErrorMessage == "PRODUCT_ID_REQUIRED");
        Assert.Contains(result.Errors, x =>
            x.PropertyName == "Items[0].Number" &&
            x.ErrorMessage == "NUMBER_MUST_BE_GREATER_THAN_OR_EQUAL_TO_1");
    }

    [Fact]
    public void Validate_ValidItem_ReturnsNoItemErrors()
    {
        var result = _validator.Validate(CreateValidRequest());

        Assert.DoesNotContain(result.Errors, x =>
            x.PropertyName.StartsWith("Items", StringComparison.Ordinal));
    }

    private static Request.CreateOrderRequest CreateValidRequest() => new()
    {
        Gateway = "BaoKim",
        PaymentGateway = PaymentGateway.BaoKim,
        ShippingGateway = ShippingGateway.Ahamove,
        TagIds = [229],
        PhoneReceiver = "0901234567",
        FullNameReceiver = "Nguyen Van A",
        FullName = "Nguyen Van A",
        Phone = "0901234567",
        Address = "123 Nguyen Hue",
        ShortAddress = "123 Nguyen Hue",
        Remarks = string.Empty,
        Latitude = 10.7769m,
        Longitude = 106.7009m,
        IsExportInvoice = false,
        Items =
        [
            new AhaMoveRequest.Items
            {
                ItemId = 101,
                ProductId = Guid.NewGuid(),
                Number = 1
            }
        ]
    };
}

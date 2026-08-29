using FluentValidation.TestHelper;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Order;
using HoaTheoMua.Service.Order.Validate;
using Xunit;

namespace HoaTheoMua.Service.Test.OrderTest;

public class EstimateXanhSmShippingFeeRequestValidatorTests
{
    private readonly EstimateXanhSmShippingFeeRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_Passes()
    {
        var result = _validator.TestValidate(CreateValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MissingCoordinates_Fails()
    {
        var request = CreateValidRequest();
        request.Latitude = 0;
        request.Longitude = 0;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(value => value.Latitude)
            .WithErrorMessage("LATITUDE_REQUIRED");
        result.ShouldHaveValidationErrorFor(value => value.Longitude)
            .WithErrorMessage("LONGITUDE_REQUIRED");
    }

    [Fact]
    public void InvalidPhone_Fails()
    {
        var request = CreateValidRequest();
        request.Phone = "123";

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(value => value.Phone);
    }

    [Fact]
    public void OrderTimeMoreThanSevenDaysAhead_Fails()
    {
        var request = CreateValidRequest();
        request.OrderTime = DateTimeOffset.UtcNow.AddDays(8).ToUnixTimeSeconds();

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(value => value.OrderTime)
            .WithErrorMessage("ORDER_TIME_INVALID");
    }

    [Fact]
    public void MissingItems_Fails()
    {
        var request = CreateValidRequest();
        request.Items = [];

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(value => value.Items)
            .WithErrorMessage("ITEMS_REQUIRED");
    }

    private static Request.EstimateXanhSmShippingFeeRequest CreateValidRequest()
    {
        return new Request.EstimateXanhSmShippingFeeRequest
        {
            Latitude = 10.801937m,
            Longitude = 106.712372m,
            Address = "208 Nguyễn Hữu Cảnh, TP.HCM",
            ShortAddress = "Landmark 81",
            FullName = "Nguyễn Văn A",
            Phone = "0901234567",
            Remarks = "Gọi trước khi giao",
            Items = [new AhaMoveRequest.Items { ItemId = 101, Number = 1 }]
        };
    }
}

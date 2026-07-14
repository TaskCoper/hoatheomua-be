using FluentValidation;
using HoaTheoMua.Service.Common;

namespace HoaTheoMua.Service.AhamoveService.Validate;

public class EstimateShippingFeeRequestValidator : AbstractValidator<AhaMoveRequest.EstimateShippingFeeRequest>
{
    public EstimateShippingFeeRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FULL_NAME_REQUIRED")
            .Length(1, 100).WithMessage("FULL_NAME_LENGTH_INVALID");

        RuleFor(x => x.Phone).PhoneNumberVn();

        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("REMARKS_LENGTH_INVALID");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("ADDRESS_REQUIRED")
            .Length(1, 255).WithMessage("ADDRESS_LENGTH_INVALID");

        RuleFor(x => x.ShortAddress)
            .Length(1, 255).WithMessage("ADDRESS_LENGTH_INVALID");


        RuleFor(x => x.Latitude)
            .NotEmpty().WithMessage("LATITUDE_REQUIRED");

        RuleFor(x => x.Longitude)
            .NotEmpty().WithMessage("LONGITUDE_REQUIRED");


        // RuleFor(x => x.City).GreaterThan(0).WithMessage("CITY_CODE_REQUIRED");
        // RuleFor(x => x.Ward).GreaterThan(0).WithMessage("WARD_CODE_REQUIRED");

        RuleFor(x => x.OrderTime)
            .Must(BeValidOrderTime)
            .WithMessage("ORDER_TIME_INVALID");

    }

    private bool BeValidOrderTime(long orderTime)
    {
        if (orderTime == 0)
            return true;

        var orderDate = DateTimeOffset
            .FromUnixTimeSeconds(orderTime)
            .UtcDateTime;

        var now = DateTime.UtcNow;

        return orderDate >= now &&
               orderDate <= now.AddDays(7);
    }
}
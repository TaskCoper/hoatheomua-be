using FluentValidation;
using HoaTheoMua.Service.Common;

namespace HoaTheoMua.Service.Order.Validate;

public class EstimateXanhSmShippingFeeRequestValidator
    : AbstractValidator<Request.EstimateXanhSmShippingFeeRequest>
{
    public EstimateXanhSmShippingFeeRequestValidator()
    {
        RuleFor(request => request.Latitude)
            .NotEmpty().WithMessage("LATITUDE_REQUIRED")
            .InclusiveBetween(-90, 90)
            .WithMessage("LATITUDE_INVALID");

        RuleFor(request => request.Longitude)
            .NotEmpty().WithMessage("LONGITUDE_REQUIRED")
            .InclusiveBetween(-180, 180)
            .WithMessage("LONGITUDE_INVALID");

        RuleFor(request => request.Address)
            .NotEmpty().WithMessage("ADDRESS_REQUIRED")
            .MaximumLength(512).WithMessage("ADDRESS_LENGTH_INVALID");

        RuleFor(request => request.ShortAddress)
            .NotEmpty().WithMessage("SHORT_ADDRESS_REQUIRED")
            .MaximumLength(512).WithMessage("SHORT_ADDRESS_LENGTH_INVALID");

        RuleFor(request => request.FullName)
            .NotEmpty().WithMessage("FULL_NAME_REQUIRED")
            .MaximumLength(256).WithMessage("FULL_NAME_LENGTH_INVALID");

        RuleFor(request => request.Phone).PhoneNumberVn();

        RuleFor(request => request.Remarks)
            .MaximumLength(512).WithMessage("REMARKS_LENGTH_INVALID");

        RuleFor(request => request.OrderTime)
            .Must(BeValidOrderTime)
            .WithMessage("ORDER_TIME_INVALID");

        RuleFor(request => request.Items)
            .NotEmpty().WithMessage("ITEMS_REQUIRED");

        RuleForEach(request => request.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.ItemId)
                .GreaterThan(0).WithMessage("ITEM_ID_INVALID");
            item.RuleFor(value => value.Number)
                .GreaterThan(0).WithMessage("ITEM_NUMBER_INVALID");
        });

        RuleFor(request => request.Items)
            .Must(items => items is null ||
                           items.Select(item => item.ItemId).Distinct().Count() == items.Count)
            .WithMessage("ITEM_ID_DUPLICATED");
    }

    private static bool BeValidOrderTime(long orderTime)
    {
        if (orderTime == 0)
        {
            return true;
        }

        try
        {
            var orderDate = DateTimeOffset.FromUnixTimeSeconds(orderTime);
            var now = DateTimeOffset.UtcNow;
            return orderDate >= now.AddMinutes(-1) && orderDate <= now.AddDays(7);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}

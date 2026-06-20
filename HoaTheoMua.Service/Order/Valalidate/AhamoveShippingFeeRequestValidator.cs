using FluentValidation;

namespace HoaTheoMua.Service.Order.Valalidate;

public class AhamoveShippingFeeRequestValidator: AbstractValidator<AhamoveService.Request.AhaMoveShippingFeeRequest>
{
    public AhamoveShippingFeeRequestValidator()
    {
        RuleFor(x => x.OrderTime)
            .Must(BeWithin7Days)
            .WithMessage("OrderTime must be within the next 7 days.");
    }

    private bool BeWithin7Days(long orderTime)
    {
        var orderDate = DateTimeOffset
            .FromUnixTimeSeconds(orderTime)
            .UtcDateTime;

        return orderDate <= DateTime.UtcNow.AddDays(7);
    }
}

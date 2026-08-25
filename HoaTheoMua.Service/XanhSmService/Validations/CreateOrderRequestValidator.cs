using FluentValidation;

namespace HoaTheoMua.Service.XanhSmService.Validations;

public sealed class CreateOrderRequestValidator : AbstractValidator<Request.CreateXanhSmOrderRequest>
{
    /// <summary>
    /// Kiểm tra request tạo đơn: fee_id UUID, service, order_time, goods value, partner trip id,
    /// route point và addon. Với cash, kiểm tra thêm đúng một payment checkpoint.
    /// </summary>
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ServiceId).GreaterThan(0).WithMessage("SERVICE_ID_INVALID");
        RuleFor(x => x.ServiceType).NotEmpty().WithMessage("SERVICE_TYPE_REQUIRED");
        RuleFor(x => x.FeeId).Must(x => Guid.TryParse(x, out _)).WithMessage("FEE_ID_INVALID");
        RuleFor(x => x.OrderTime).NotNull().GreaterThanOrEqualTo(0).WithMessage("ORDER_TIME_INVALID");
        RuleFor(x => x.PaymentMethod).Must(x => x is "cash" or "partner_pay").WithMessage("PAYMENT_METHOD_INVALID");
        RuleFor(x => x.GoodsValue).GreaterThanOrEqualTo(0).When(x => x.GoodsValue.HasValue).WithMessage("GOODS_VALUE_INVALID");
        RuleFor(x => x.PartnerTripId).MaximumLength(32).WithMessage("PARTNER_TRIP_ID_TOO_LONG");
        RuleForEach(x => x.Points).SetValidator(new XanhSmValidationRules.RoutePointValidator());
        RuleFor(x => x).Custom((request, context) => XanhSmValidationRules.ValidateRouteAndAddons(request.Points,
            request.Addons ?? [], request.PaymentMethod == "cash", context.AddFailure));
    }
}

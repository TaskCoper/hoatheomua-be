using FluentValidation;

namespace HoaTheoMua.Service.XanhSmService.Validations;

public sealed class EstimateOrderRequestValidator : AbstractValidator<Request.EstimateOrderRequest>
{
    /// <summary>
    /// Kiểm tra request estimate, bao gồm service cần báo giá, payment method,
    /// từng route point và các rule liên kết của addon/COD.
    /// </summary>
    public EstimateOrderRequestValidator()
    {
        RuleFor(x => x.ServiceIds).NotEmpty().WithMessage("SERVICE_ID_REQUIRED");
        RuleForEach(x => x.ServiceIds).ChildRules(service => service.RuleFor(x => x.Id).GreaterThan(0).WithMessage("SERVICE_ID_INVALID"));
        RuleFor(x => x.PaymentMethod).Must(x => x is "cash" or "partner_pay").WithMessage("PAYMENT_METHOD_INVALID");
        RuleFor(x => x.OrderTime).GreaterThanOrEqualTo(0).When(x => x.OrderTime.HasValue).WithMessage("ORDER_TIME_INVALID");
        RuleForEach(x => x.Points).SetValidator(new XanhSmValidationRules.RoutePointValidator());
        RuleFor(x => x).Custom((request, context) => XanhSmValidationRules.ValidateRouteAndAddons(request.Points,
            request.ServiceIds.SelectMany(x => x.Addons ?? []), false, context.AddFailure));
    }
}

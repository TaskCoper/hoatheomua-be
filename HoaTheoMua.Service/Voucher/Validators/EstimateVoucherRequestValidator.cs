using FluentValidation;

namespace HoaTheoMua.Service.Voucher.Validators;

public class EstimateVoucherRequestValidator : AbstractValidator<Request.EstimateVoucherRequest>
{
    public EstimateVoucherRequestValidator()
    {
        RuleFor(x => x.code)
            .NotEmpty().WithMessage("Voucher code is required");

        RuleFor(x => x.productFee)
            .GreaterThan(0).WithMessage("Product fee must be greater than 0");
    }
}

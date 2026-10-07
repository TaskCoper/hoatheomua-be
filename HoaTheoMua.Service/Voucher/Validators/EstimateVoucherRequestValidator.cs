using FluentValidation;

namespace HoaTheoMua.Service.Voucher.Validators;

public class EstimateVoucherRequestValidator : AbstractValidator<Request.EstimateVoucherRequest>
{
    public EstimateVoucherRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Voucher code is required");

        RuleFor(x => x.ProductFee)
            .GreaterThan(0).WithMessage("Product fee must be greater than 0");
    }
}

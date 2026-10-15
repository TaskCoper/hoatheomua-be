using FluentValidation;

namespace HoaTheoMua.Service.Voucher.Validators;

public class EstimateVoucherRequestValidator : AbstractValidator<Request.EstimateVoucherRequest>
{
    public EstimateVoucherRequestValidator()
    {
        RuleFor(x => x.code)
            .NotEmpty().WithMessage("Vui lòng nhập mã voucher.");

        RuleFor(x => x.productFee)
            .GreaterThan(0).WithMessage("Tổng tiền sản phẩm phải lớn hơn 0.");

        RuleFor(x => x.orderTime)
            .GreaterThanOrEqualTo(0).WithMessage("Thời gian giao hàng không hợp lệ");
    }
}

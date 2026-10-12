using FluentValidation;

namespace HoaTheoMua.Service.Voucher.Validators;

public class SetVouchersActiveRequestValidator : AbstractValidator<Request.SetVouchersActiveRequest>
{
    public SetVouchersActiveRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Vui lòng chọn ít nhất một voucher.");

        RuleFor(x => x.Ids)
            .Must(x => x.Count <= 1000).WithMessage("Mỗi lần chỉ được đổi trạng thái tối đa 1.000 voucher.");

        RuleFor(x => x.Ids)
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Danh sách đã chọn có voucher bị lặp. Vui lòng kiểm tra lại.");

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("Vui lòng chọn bật hoặc tắt voucher.");
    }
}

using FluentValidation;

namespace HoaTheoMua.Service.Voucher.Validators;

public class SetVouchersActiveRequestValidator : AbstractValidator<Request.SetVouchersActiveRequest>
{
    public SetVouchersActiveRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Ids are required");

        RuleFor(x => x.Ids)
            .Must(x => x.Count <= 1000).WithMessage("Ids must not exceed 1000");

        RuleFor(x => x.Ids)
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Duplicate voucher id");

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("Is active is required");
    }
}

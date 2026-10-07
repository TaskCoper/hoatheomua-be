using FluentValidation;

namespace HoaTheoMua.Service.VoucherProgram.Validate;

public class CreateVoucherProgramRequestValidator : AbstractValidator<Request.CreateVoucherProgramRequest>
{
    public CreateVoucherProgramRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.StartAt)
            .NotEmpty().WithMessage("Start time is required");
        RuleFor(x => x.EndAt)
            .NotEmpty().WithMessage("End time is required")
            .GreaterThan(x => x.StartAt).WithMessage("End time must be later than start time")
            .When(x => x.StartAt != default && x.EndAt != default, ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Order amount must not be negative");
        RuleFor(x => x.MaxOrderAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Order amount must not be negative")
            .GreaterThanOrEqualTo(x => x.MinOrderAmount).WithMessage("Max order amount cannot be less than min order amount")
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue, ApplyConditionTo.CurrentValidator);
    }
}

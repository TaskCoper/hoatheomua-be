using FluentValidation;

namespace HoaTheoMua.Service.VoucherProgram.Validate;

public class UpdateVoucherProgramRequestValidator : AbstractValidator<Request.UpdateVoucherProgramRequest>
{
    public UpdateVoucherProgramRequestValidator()
    {
        RuleFor(x => x).Must(x => x.Name != null || x.StartAt.HasValue || x.EndAt.HasValue
                                 || x.MinOrderAmount.HasValue || x.MaxOrderAmount.HasValue || x.IsActive.HasValue)
            .WithMessage("Nothing to update");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .When(x => x.Name != null);
        RuleFor(x => x.StartAt).NotEqual(default(DateTimeOffset))
            .WithMessage("Start time is required").When(x => x.StartAt.HasValue);
        RuleFor(x => x.EndAt).NotEqual(default(DateTimeOffset)).WithMessage("End time is required")
            .GreaterThan(x => x.StartAt).WithMessage("End time must be later than start time")
            .When(x => x.StartAt.HasValue, ApplyConditionTo.CurrentValidator)
            .When(x => x.EndAt.HasValue);
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0m)
            .WithMessage("Order amount must not be negative");
        RuleFor(x => x.MaxOrderAmount).GreaterThanOrEqualTo(0m)
            .WithMessage("Order amount must not be negative")
            .GreaterThanOrEqualTo(x => x.MinOrderAmount)
            .WithMessage("Max order amount cannot be less than min order amount")
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue, ApplyConditionTo.CurrentValidator);
    }
}

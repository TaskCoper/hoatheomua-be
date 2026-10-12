using FluentValidation;

namespace HoaTheoMua.Service.VoucherProgram.Validate;

public class UpdateVoucherProgramRequestValidator : AbstractValidator<Request.UpdateVoucherProgramRequest>
{
    public UpdateVoucherProgramRequestValidator()
    {
        RuleFor(x => x).Must(x => x.Name != null || x.StartAt != null || x.EndAt != null
                                 || x.MinOrderAmount.HasValue || x.MaxOrderAmount.HasValue || x.IsActive.HasValue)
            .WithMessage("At least one voucher program field must be provided for update");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters")
            .When(x => x.Name != null);
        RuleFor(x => x.StartAt).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Start time is required")
            .Must(value => VoucherProgramDateTime.TryParse(value, out _))
            .WithMessage("Start time must be a valid ISO 8601 date-time, for example 2026-10-15T00:00:00+07:00")
            .When(x => x.StartAt != null);
        RuleFor(x => x.EndAt).Cascade(CascadeMode.Stop)
            .Must(value => VoucherProgramDateTime.TryParse(value, out _))
            .WithMessage("End time must be a valid ISO 8601 date-time, for example 2026-10-31T23:59:59+07:00")
            .When(x => !string.IsNullOrWhiteSpace(x.EndAt));
        RuleFor(x => x.EndAt)
            .Must((request, endAt) => VoucherProgramDateTime.TryParse(request.StartAt, out var start)
                                      && VoucherProgramDateTime.TryParse(endAt, out var end) && end > start)
            .WithMessage("End time must be later than start time")
            .When(x => VoucherProgramDateTime.TryParse(x.StartAt, out _)
                       && VoucherProgramDateTime.TryParse(x.EndAt, out _));
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0m)
            .WithMessage("Min order amount must not be negative");
        RuleFor(x => x.MaxOrderAmount).GreaterThanOrEqualTo(0m)
            .WithMessage("Max order amount must not be negative")
            .GreaterThanOrEqualTo(x => x.MinOrderAmount)
            .WithMessage("Max order amount cannot be less than min order amount")
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue && x.MaxOrderAmount.Value > 0m,
                ApplyConditionTo.CurrentValidator);
    }
}

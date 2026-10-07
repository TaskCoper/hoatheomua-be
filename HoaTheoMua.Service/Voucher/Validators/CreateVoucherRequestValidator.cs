using FluentValidation;

namespace HoaTheoMua.Service.Voucher.Validators;

public class CreateVoucherRequestValidator : AbstractValidator<Request.CreateVoucherRequest>
{
    public CreateVoucherRequestValidator()
    {
        // Code: manual OR generated
        RuleFor(x => x.Code)
            .Must((x, _) => x.CodePrefix == null && x.CodeLength == null && x.CodeSuffix == null)
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Send either code or code generation fields");

        RuleFor(x => x.Code)
            .Matches("^[A-Za-z0-9]{4,20}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Code must be 4 to 20 uppercase letters or digits");

        RuleFor(x => x.CodeLength)
            .NotNull()
            .When(x => string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Code or code length is required");

        When(x => string.IsNullOrWhiteSpace(x.Code) && x.CodeLength.HasValue, () =>
        {
            RuleFor(x => x.CodeLength)
                .InclusiveBetween(5, 10).WithMessage("Code length must be between 5 and 10");

            RuleFor(x => x.CodePrefix)
                .Matches("^[A-Za-z0-9]*$").When(x => x.CodePrefix != null)
                .WithMessage("Code prefix or suffix is invalid");

            RuleFor(x => x.CodeSuffix)
                .Matches("^[A-Za-z0-9]*$").When(x => x.CodeSuffix != null)
                .WithMessage("Code prefix or suffix is invalid");

            RuleFor(x => x)
                .Must(x => (x.CodePrefix?.Length ?? 0) + x.CodeLength!.Value + (x.CodeSuffix?.Length ?? 0) <= 20)
                .OverridePropertyName("code")
                .WithMessage("Code is too long");
        });

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Visibility)
            .Must(x => x is VoucherVisibilities.Public or VoucherVisibilities.Private)
            .WithMessage("Visibility must be public or private");

        RuleFor(x => x.DiscountType)
            .Must(x => x is VoucherDiscountTypes.Cash or VoucherDiscountTypes.Percent)
            .WithMessage("Discount type must be cash or percent");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).When(x => x.DiscountType == VoucherDiscountTypes.Percent)
            .WithMessage("Discount percent must not exceed 100");

        RuleFor(x => x.MaxDiscountAmount)
            .Null().When(x => x.DiscountType != VoucherDiscountTypes.Percent)
            .WithMessage("Max discount amount is only for percent");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0).When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Max discount amount must be greater than 0");

        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0).WithMessage("Max usage count must be greater than 0");

        RuleFor(x => x.StartAt)
            .NotNull().WithMessage("Start time is required");

        RuleFor(x => x.EndAt)
            .NotNull().WithMessage("End time is required");

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt).When(x => x.StartAt.HasValue && x.EndAt.HasValue)
            .WithMessage("End time must be later than start time");

        // A voucher that has already ended can never be used
        RuleFor(x => x.EndAt)
            .Must(x => x!.Value > DateTimeOffset.UtcNow).When(x => x.EndAt.HasValue)
            .WithMessage("End time must be in the future");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThan(0).When(x => x.MinOrderAmount.HasValue)
            .WithMessage("Order amount must be greater than 0");

        RuleFor(x => x.MaxOrderAmount)
            .GreaterThan(0).When(x => x.MaxOrderAmount.HasValue)
            .WithMessage("Order amount must be greater than 0");

        RuleFor(x => x.MaxOrderAmount)
            .GreaterThanOrEqualTo(x => x.MinOrderAmount!.Value)
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue)
            .WithMessage("Max order amount cannot be less than min order amount");
    }
}

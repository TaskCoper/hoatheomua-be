using FluentValidation;

namespace HoaTheoMua.Service.Voucher.Validators;

public class UpdateVouchersRequestValidator : AbstractValidator<Request.UpdateVouchersRequest>
{
    public UpdateVouchersRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Items are required");

        RuleFor(x => x.Items)
            .Must(x => x.Count <= 1000).WithMessage("Items must not exceed 1000");

        RuleFor(x => x.Items)
            .Must(x => x.Select(item => item.Id).Distinct().Count() == x.Count).WithMessage("Duplicate voucher id");

        RuleForEach(x => x.Items).SetValidator(new UpdateVoucherItemValidator());
    }
}

public class UpdateVoucherItemValidator : AbstractValidator<Request.UpdateVoucherItem>
{
    public UpdateVoucherItemValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Description != null
                       || x.Visibility != null
                       || x.DiscountType != null
                       || x.DiscountValue != null
                       || x.MaxDiscountAmount != null
                       || x.MaxUsageCount != null
                       || x.StartAt != null
                       || x.EndAt != null
                       || x.MinOrderAmount != null
                       || x.MaxOrderAmount != null)
            .OverridePropertyName("id")
            .WithMessage("Nothing to update");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Visibility)
            .Must(x => x is VoucherVisibilities.Public or VoucherVisibilities.Private)
            .When(x => x.Visibility != null)
            .WithMessage("Visibility must be public or private");

        RuleFor(x => x.DiscountType)
            .Must(x => x is VoucherDiscountTypes.Cash or VoucherDiscountTypes.Percent)
            .When(x => x.DiscountType != null)
            .WithMessage("Discount type must be cash or percent");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).When(x => x.DiscountValue.HasValue)
            .WithMessage("Discount value must be greater than 0");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0).When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Max discount amount must be greater than 0");

        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0).When(x => x.MaxUsageCount.HasValue)
            .WithMessage("Max usage count must be greater than 0");

        RuleFor(x => x.EndAt)
            .Must(x => x!.Value > DateTimeOffset.UtcNow).When(x => x.EndAt.HasValue)
            .WithMessage("End time must be in the future");

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt).When(x => x.StartAt.HasValue && x.EndAt.HasValue)
            .WithMessage("End time must be later than start time");

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

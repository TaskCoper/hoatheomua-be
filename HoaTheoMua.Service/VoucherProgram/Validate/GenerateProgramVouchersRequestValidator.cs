using FluentValidation;

namespace HoaTheoMua.Service.VoucherProgram.Validate;

public class GenerateProgramVouchersRequestValidator : AbstractValidator<Request.GenerateProgramVouchersRequest>
{
    public GenerateProgramVouchersRequestValidator()
    {
        RuleFor(x => x.CodeLength).InclusiveBetween(5, 10)
            .WithMessage("Code length must be between 5 and 10");
        RuleFor(x => x.CodePrefix).Matches(@"\A[A-Z0-9]*\z")
            .WithMessage("Code prefix or suffix is invalid");
        RuleFor(x => x.CodeSuffix).Matches(@"\A[A-Z0-9]*\z")
            .WithMessage("Code prefix or suffix is invalid");
        RuleFor(x => x.CodeLength)
            .Must((request, length) => (long)(request.CodePrefix?.Length ?? 0) + length
                                      + (request.CodeSuffix?.Length ?? 0) <= 20)
            .WithMessage("Code is too long");
        RuleFor(x => x.Quantity).InclusiveBetween(1, 1000)
            .WithMessage("Quantity must be between 1 and 1000");
        RuleFor(x => x.MaxUsageCount).GreaterThan(0)
            .WithMessage("Max usage count must be greater than 0");
        RuleFor(x => x.DiscountType).Must(type => type is "cash" or "percent")
            .WithMessage("Discount type must be cash or percent");
        RuleFor(x => x.DiscountValue).GreaterThan(0)
            .WithMessage("Discount value must be greater than 0");
        RuleFor(x => x.DiscountValue).LessThanOrEqualTo(100)
            .When(x => x.DiscountType == "percent")
            .WithMessage("Discount percent must not exceed 100");
        RuleFor(x => x.MaxDiscountAmount).Null()
            .When(x => x.DiscountType != "percent")
            .WithMessage("Max discount amount is only for percent");
        RuleFor(x => x.MaxDiscountAmount).GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Max discount amount must be greater than 0");
        RuleFor(x => x.Description).MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");
    }
}

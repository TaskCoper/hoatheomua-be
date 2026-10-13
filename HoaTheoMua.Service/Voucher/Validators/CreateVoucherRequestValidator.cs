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
            .WithMessage("Chỉ nhập mã voucher hoặc chọn tạo mã tự động.");

        RuleFor(x => x.Code)
            .Matches("^[A-Za-z0-9]{4,20}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Mã voucher phải có từ 4 đến 20 ký tự, chỉ gồm chữ cái hoặc chữ số.");

        RuleFor(x => x.CodeLength)
            .NotNull()
            .When(x => string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Vui lòng nhập mã voucher hoặc chọn độ dài để tạo mã tự động.");

        When(x => string.IsNullOrWhiteSpace(x.Code) && x.CodeLength.HasValue, () =>
        {
            RuleFor(x => x.CodeLength)
                .InclusiveBetween(5, 10).WithMessage("Phần mã ngẫu nhiên phải dài từ 5 đến 10 ký tự.");

            RuleFor(x => x.CodePrefix)
                .Matches("^[A-Za-z0-9]*$").When(x => x.CodePrefix != null)
                .WithMessage("Phần đầu hoặc phần cuối mã chỉ được gồm chữ cái hoặc chữ số.");

            RuleFor(x => x.CodeSuffix)
                .Matches("^[A-Za-z0-9]*$").When(x => x.CodeSuffix != null)
                .WithMessage("Phần đầu hoặc phần cuối mã chỉ được gồm chữ cái hoặc chữ số.");

            RuleFor(x => x)
                .Must(x => (x.CodePrefix?.Length ?? 0) + x.CodeLength!.Value + (x.CodeSuffix?.Length ?? 0) <= 20)
                .OverridePropertyName("code")
                .WithMessage("Mã voucher không được dài quá 20 ký tự, tính cả phần đầu và phần cuối mã.");
        });

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được dài quá 500 ký tự.");

        RuleFor(x => x.Visibility)
            .Must(x => x is VoucherVisibilities.Public or VoucherVisibilities.Private)
            .WithMessage("Chế độ hiển thị chỉ chấp nhận public (công khai) hoặc private (riêng tư).");

        RuleFor(x => x.DiscountType)
            .Must(x => x is VoucherDiscountTypes.Cash or VoucherDiscountTypes.Percent)
            .WithMessage("Loại giảm giá chỉ chấp nhận cash (giảm tiền) hoặc percent (giảm theo phần trăm).");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Mức giảm giá phải lớn hơn 0.");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).When(x => x.DiscountType == VoucherDiscountTypes.Percent)
            .WithMessage("Mức giảm theo phần trăm không được vượt quá 100%.");

        RuleFor(x => x.MaxDiscountAmount)
            .Null().When(x => x.DiscountType != VoucherDiscountTypes.Percent)
            .WithMessage("Chỉ đặt mức giảm tối đa khi giảm theo phần trăm.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0).When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Mức giảm tối đa phải lớn hơn 0.");

        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0).WithMessage("Giới hạn lượt sử dụng phải lớn hơn 0.");

        RuleFor(x => x.StartAt)
            .NotNull().WithMessage("Vui lòng nhập thời gian bắt đầu.");

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt).When(x => x.StartAt.HasValue && x.EndAt.HasValue)
            .WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu.");

        // A voucher that has already ended can never be used
        RuleFor(x => x.EndAt)
            .Must(x => x!.Value > DateTimeOffset.UtcNow).When(x => x.EndAt.HasValue)
            .WithMessage("Thời gian kết thúc phải sau thời điểm hiện tại.");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Giá trị đơn hàng tối thiểu phải từ 0 trở lên.");
        // Not sent = no limit. A sent value, 0 included, is stored as is
        RuleFor(x => x.MaxOrderAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Giá trị đơn hàng tối đa phải từ 0 trở lên.")
            .GreaterThan(x => x.MinOrderAmount).WithMessage("Giá trị đơn hàng tối đa phải lớn hơn giá trị tối thiểu.")
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue, ApplyConditionTo.CurrentValidator);
    }
}

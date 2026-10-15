using FluentValidation;

namespace HoaTheoMua.Service.Voucher.Validators;

public class UpdateVouchersRequestValidator : AbstractValidator<Request.UpdateVouchersRequest>
{
    public UpdateVouchersRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Vui lòng chọn ít nhất một voucher cần cập nhật.");

        RuleFor(x => x.Items)
            .Must(x => x.Count <= 1000).WithMessage("Mỗi lần chỉ được cập nhật tối đa 1.000 voucher.");

        RuleFor(x => x.Items)
            .Must(x => x.Select(item => item.Id).Distinct().Count() == x.Count).WithMessage("Danh sách đã chọn có voucher bị lặp. Vui lòng kiểm tra lại.");

        RuleForEach(x => x.Items).SetValidator(new UpdateVoucherItemValidator());
    }
}

public class UpdateVoucherItemValidator : AbstractValidator<Request.UpdateVoucherItem>
{
    public UpdateVoucherItemValidator()
    {
        // No "at least one field" rule: an item with only its id is a real update,
        // because omitted EndAt / MinOrderAmount / MaxOrderAmount / MaxDiscountAmount clear those limits.

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được dài quá 500 ký tự.");

        RuleFor(x => x.Visibility)
            .Must(x => x is VoucherVisibilities.Public or VoucherVisibilities.Private)
            .When(x => x.Visibility != null)
            .WithMessage("Chế độ hiển thị chỉ chấp nhận public (công khai) hoặc private (riêng tư).");

        RuleFor(x => x.DiscountType)
            .Must(x => x is VoucherDiscountTypes.Cash or VoucherDiscountTypes.Percent)
            .When(x => x.DiscountType != null)
            .WithMessage("Loại giảm giá chỉ chấp nhận cash (giảm tiền) hoặc percent (giảm theo phần trăm).");

        RuleFor(x => x.DiscountValue)
            .GreaterThanOrEqualTo(0).WithMessage("Mức giảm giá không được âm.");

        RuleFor(x => x.DiscountValue)
            .PrecisionScale(18, 3, true)
            .WithMessage("Mức giảm giá chỉ được có tối đa 3 chữ số thập phân.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0).When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Mức giảm tối đa phải lớn hơn 0.");

        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0).When(x => x.MaxUsageCount.HasValue)
            .WithMessage("Giới hạn lượt sử dụng phải lớn hơn 0.");

        // "EndAt after now" is checked in UpdateVouchers, and only when EndAt actually changes

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt).When(x => x.StartAt.HasValue && x.EndAt.HasValue)
            .WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu.");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Giá trị đơn hàng tối thiểu phải từ 0 trở lên.");
        // Not sent = no limit. A sent value, 0 included, is stored as is
        RuleFor(x => x.MaxOrderAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Giá trị đơn hàng tối đa phải từ 0 trở lên.")
            .GreaterThan(x => x.MinOrderAmount).WithMessage("Giá trị đơn hàng tối đa phải lớn hơn giá trị tối thiểu.")
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue, ApplyConditionTo.CurrentValidator);
    }
}

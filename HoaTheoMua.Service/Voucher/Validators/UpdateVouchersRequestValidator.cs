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
            .WithMessage("Vui lòng nhập ít nhất một thông tin cần cập nhật.");

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
            .GreaterThan(0).When(x => x.DiscountValue.HasValue)
            .WithMessage("Mức giảm giá phải lớn hơn 0.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0).When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Mức giảm tối đa phải lớn hơn 0.");

        RuleFor(x => x.MaxUsageCount)
            .GreaterThan(0).When(x => x.MaxUsageCount.HasValue)
            .WithMessage("Giới hạn lượt sử dụng phải lớn hơn 0.");

        RuleFor(x => x.EndAt)
            .Must(x => x!.Value > DateTimeOffset.UtcNow).When(x => x.EndAt.HasValue)
            .WithMessage("Thời gian kết thúc phải sau thời điểm hiện tại.");

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt).When(x => x.StartAt.HasValue && x.EndAt.HasValue)
            .WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu.");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThan(0).When(x => x.MinOrderAmount.HasValue)
            .WithMessage("Giá trị đơn hàng phải lớn hơn 0.");

        RuleFor(x => x.MaxOrderAmount)
            .GreaterThan(0).When(x => x.MaxOrderAmount.HasValue)
            .WithMessage("Giá trị đơn hàng phải lớn hơn 0.");

        RuleFor(x => x.MaxOrderAmount)
            .GreaterThanOrEqualTo(x => x.MinOrderAmount!.Value)
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue)
            .WithMessage("Giá trị đơn hàng tối đa không được nhỏ hơn giá trị tối thiểu.");
    }
}

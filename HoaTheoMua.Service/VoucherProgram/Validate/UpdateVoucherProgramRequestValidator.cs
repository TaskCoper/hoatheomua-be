using FluentValidation;

namespace HoaTheoMua.Service.VoucherProgram.Validate;

public class UpdateVoucherProgramRequestValidator : AbstractValidator<Request.UpdateVoucherProgramRequest>
{
    public UpdateVoucherProgramRequestValidator()
    {
        RuleFor(x => x).Must(x => x.Name != null || x.StartAt != null || x.EndAt != null
                                 || x.MinOrderAmount.HasValue || x.MaxOrderAmount.HasValue || x.IsActive.HasValue)
            .WithMessage("Vui lòng nhập ít nhất một thông tin cần cập nhật cho chương trình voucher.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên chương trình voucher.")
            .MaximumLength(200).WithMessage("Tên chương trình voucher không được dài quá 200 ký tự.")
            .When(x => x.Name != null);
        RuleFor(x => x.StartAt).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Vui lòng nhập thời gian bắt đầu.")
            .Must(value => VoucherProgramDateTime.TryParse(value, out _))
            .WithMessage("Thời gian bắt đầu không đúng định dạng. Ví dụ: 2026-10-15T00:00:00+07:00.")
            .When(x => x.StartAt != null);
        RuleFor(x => x.EndAt).Cascade(CascadeMode.Stop)
            .Must(value => VoucherProgramDateTime.TryParse(value, out _))
            .WithMessage("Thời gian kết thúc không đúng định dạng. Ví dụ: 2026-10-31T23:59:59+07:00.")
            .When(x => !string.IsNullOrWhiteSpace(x.EndAt));
        RuleFor(x => x.EndAt)
            .Must((request, endAt) => VoucherProgramDateTime.TryParse(request.StartAt, out var start)
                                      && VoucherProgramDateTime.TryParse(endAt, out var end) && end > start)
            .WithMessage("Thời gian kết thúc phải sau thời gian bắt đầu.")
            .When(x => VoucherProgramDateTime.TryParse(x.StartAt, out _)
                       && VoucherProgramDateTime.TryParse(x.EndAt, out _));
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0m)
            .WithMessage("Giá trị đơn hàng tối thiểu phải từ 0 trở lên.");
        // Not sent = no limit. A sent value, 0 included, is stored as is
        RuleFor(x => x.MaxOrderAmount).GreaterThanOrEqualTo(0m).When(x => x.MaxOrderAmount.HasValue)
            .WithMessage("Giá trị đơn hàng tối đa phải từ 0 trở lên.");
        RuleFor(x => x.MaxOrderAmount).GreaterThan(x => x.MinOrderAmount!.Value)
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue)
            .WithMessage("Giá trị đơn hàng tối đa phải lớn hơn giá trị tối thiểu.");
    }
}

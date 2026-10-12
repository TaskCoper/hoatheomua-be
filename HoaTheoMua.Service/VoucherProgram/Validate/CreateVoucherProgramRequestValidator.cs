using FluentValidation;

namespace HoaTheoMua.Service.VoucherProgram.Validate;

public class CreateVoucherProgramRequestValidator : AbstractValidator<Request.CreateVoucherProgramRequest>
{
    public CreateVoucherProgramRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vui lòng nhập tên chương trình voucher.")
            .MaximumLength(200).WithMessage("Tên chương trình voucher không được dài quá 200 ký tự.");

        RuleFor(x => x.StartAt).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Vui lòng nhập thời gian bắt đầu.")
            .Must(value => VoucherProgramDateTime.TryParse(value, out _))
            .WithMessage("Thời gian bắt đầu không đúng định dạng. Ví dụ: 2026-10-15T00:00:00+07:00.");
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

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Giá trị đơn hàng tối thiểu phải từ 0 trở lên.");
        RuleFor(x => x.MaxOrderAmount)
            .GreaterThanOrEqualTo(0m).WithMessage("Giá trị đơn hàng tối đa phải từ 0 trở lên.")
            .GreaterThanOrEqualTo(x => x.MinOrderAmount).WithMessage("Giá trị đơn hàng tối đa không được nhỏ hơn giá trị tối thiểu.")
            .When(x => x.MinOrderAmount.HasValue && x.MaxOrderAmount.HasValue && x.MaxOrderAmount.Value > 0m,
                ApplyConditionTo.CurrentValidator);
    }
}

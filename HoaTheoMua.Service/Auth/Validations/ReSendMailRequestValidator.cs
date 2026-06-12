using FluentValidation;

namespace HoaTheoMua.Service.Auth.Validations;

public class ReSendMailRequestValidator: AbstractValidator<Request.ReSendMailRequest>
{
    public ReSendMailRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Định dạng email không hợp lệ (Ví dụ: xxx@xxx.com).");
    }
}
using FluentValidation;

namespace HoaTheoMua.Service.Auth.Validations;

public class VerifyEmailRequestValidator: AbstractValidator<Request.VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Mã xác thực (Token) không được để trống.")
            .NotNull().WithMessage("Mã xác thực (Token) không được phép null.");    
    }
}
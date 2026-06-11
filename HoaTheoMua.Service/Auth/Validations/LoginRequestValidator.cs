using FluentValidation;

namespace HoaTheoMua.Service.Auth.Validations;

public class LoginRequestValidator: AbstractValidator<Request.LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Định dạng email không hợp lệ (Ví dụ: xxx@xxx.com)");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .Length(6, 128).WithMessage("Mật khẩu phải từ 6 đến 128 ký tự.");
    }
}
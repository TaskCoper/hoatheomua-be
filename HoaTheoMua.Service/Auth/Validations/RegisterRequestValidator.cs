using FluentValidation;

namespace HoaTheoMua.Service.Auth.Validations;

public class RegisterRequestValidator: AbstractValidator<Request.RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Họ không được để trống.")
            .Length(1, 100).WithMessage("Họ phải từ 1 đến 100 ký tự.");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Tên không được để trống.")
            .Length(1, 100).WithMessage("Tên phải từ 1 đến 100 ký tự.");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Định dạng email không hợp lệ (Ví dụ: xxx@xxx.com).");
        
        RuleFor(x => x.Password)
            
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .Length(6, 128).WithMessage("Mật khẩu phải từ 6 đến 128 ký tự.")
            .Matches(@"[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ viết hoa.")
            .Matches(@"[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống.")
            .Equal(x => x.Password).WithMessage("Mật khẩu xác nhận không trùng khớp.");
    }
}

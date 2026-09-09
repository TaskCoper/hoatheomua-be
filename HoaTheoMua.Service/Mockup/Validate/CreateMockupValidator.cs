using FluentValidation;

namespace HoaTheoMua.Service.Mockup.Validate;

public class CreateMockupValidator : AbstractValidator<Request.CreateMockupRequest>
{
    public CreateMockupValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrEmpty(name))
            .WithMessage("Tên Mockup bắt buộc, từ 1 đến 50 ký tự.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .When(x => !string.IsNullOrEmpty(x.Name), ApplyConditionTo.CurrentValidator)
            .WithMessage("Tên Mockup không hợp lệ.")
            .MaximumLength(50)
            .WithMessage("Tên Mockup không được vượt quá 50 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Mô tả Mockup không được vượt quá 200 ký tự.");

        RuleFor(x => x.ImageUrl)
            .NotNull()
            .WithMessage("imageUrl là bắt buộc.")
            .Must(url => !string.IsNullOrWhiteSpace(url))
            .When(x => x.ImageUrl is not null, ApplyConditionTo.CurrentValidator)
            .WithMessage("imageUrl không được để trống.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl), ApplyConditionTo.CurrentValidator)
            .WithMessage("imageUrl không hợp lệ.");
    }
}

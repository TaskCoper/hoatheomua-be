using FluentValidation;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.FlowerReference.Validate;

public class CreateFlowerReferenceValidator : AbstractValidator<Request.CreateFlowerReferenceRequest>
{
    public CreateFlowerReferenceValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrEmpty(name))
            .WithMessage("Tên FlowerReference bắt buộc, từ 1 đến 50 ký tự.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .When(x => !string.IsNullOrEmpty(x.Name), ApplyConditionTo.CurrentValidator)
            .WithMessage("Tên FlowerReference không hợp lệ.")
            .MaximumLength(50)
            .WithMessage("Tên FlowerReference không được vượt quá 50 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Mô tả FlowerReference không được vượt quá 200 ký tự.");

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

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Loại FlowerReference là bắt buộc.")
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Loại FlowerReference không hợp lệ.")
            .Must(IsValidType)
            .When(x => !string.IsNullOrWhiteSpace(x.Type), ApplyConditionTo.CurrentValidator)
            .WithMessage("Loại FlowerReference không hợp lệ.");
    }

    private static bool IsValidType(string? value) =>
        !int.TryParse(value, out _) && Enum.TryParse<FlowerReferenceType>(value, true, out _);
}

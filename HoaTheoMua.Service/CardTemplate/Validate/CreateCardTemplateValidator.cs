using FluentValidation;

namespace HoaTheoMua.Service.CardTemplate.Validate;

public class CreateCardTemplateValidator : AbstractValidator<Request.CreateCartTemplateRequest>
{
    public CreateCardTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("CARD_TEMPLATE_NAME_REQUIRED")
            .MaximumLength(100).WithMessage("CARD_TEMPLATE_NAME_LENGTH_INVALID");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("CARD_TEMPLATE_DESCRIPTION_LENGTH_INVALID");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("CARD_TEMPLATE_IMAGE_REQUIRED")
            .MaximumLength(2048).WithMessage("CARD_TEMPLATE_IMAGE_URL_LENGTH_INVALID")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl), ApplyConditionTo.CurrentValidator)
            .WithMessage("CARD_TEMPLATE_IMAGE_URL_INVALID");

        RuleFor(x => x.TemplateType)
            .NotNull().WithMessage("CARD_TEMPLATE_TYPE_INVALID")
            .Must(type => type is "ai" or "handmade")
            .WithMessage("CARD_TEMPLATE_TYPE_INVALID");
    }
}

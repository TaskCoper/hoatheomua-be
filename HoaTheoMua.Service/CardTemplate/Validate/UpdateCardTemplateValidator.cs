using HoaTheoMua.Service.Common;
using FluentValidation;

namespace HoaTheoMua.Service.CardTemplate.Validate;

public class UpdateCardTemplateValidator : AbstractValidator<Request.UpdateCardTemplateRequest>
{
    public UpdateCardTemplateValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .When(x => x.Name is not null, ApplyConditionTo.CurrentValidator)
            .WithMessage("CARD_TEMPLATE_NAME_REQUIRED")
            .MaximumLength(100).WithMessage("CARD_TEMPLATE_NAME_LENGTH_INVALID");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("CARD_TEMPLATE_DESCRIPTION_LENGTH_INVALID");

        RuleFor(x => x.ImageUrl)
            .Cascade(CascadeMode.Stop)
            .Must(url => !string.IsNullOrWhiteSpace(url))
            .When(x => x.ImageUrl is not null, ApplyConditionTo.CurrentValidator)
            .WithMessage("CARD_TEMPLATE_IMAGE_REQUIRED")
            .MaximumLength(2048).WithMessage("CARD_TEMPLATE_IMAGE_URL_LENGTH_INVALID")
            .HttpUrl()
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl), ApplyConditionTo.CurrentValidator)
            .WithMessage("CARD_TEMPLATE_IMAGE_URL_INVALID");
    }
}

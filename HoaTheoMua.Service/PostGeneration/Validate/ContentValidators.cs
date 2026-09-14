using FluentValidation;

namespace HoaTheoMua.Service.PostGeneration.Validate;

public class RegenerateContentValidator : AbstractValidator<Request.RegenerateContentRequest>
{
    public RegenerateContentValidator()
    {
        RuleFor(x => x.SourceGenerationId).NotEqual(Guid.Empty).When(x => x.SourceGenerationId.HasValue);
        RuleFor(x => x.Feedback).MaximumLength(2000);
    }
}
public class EditContentValidator : AbstractValidator<Request.EditContentRequest>
{
    public EditContentValidator()
    {
        RuleFor(x => x.Title).MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.Hashtag).MaximumLength(1000);
    }
}
public class SelectContentValidator : AbstractValidator<Request.SelectContentRequest>
{
    public SelectContentValidator() => RuleFor(x => x.GenerationId).NotEmpty();
}

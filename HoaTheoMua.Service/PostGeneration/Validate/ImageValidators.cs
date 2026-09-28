using FluentValidation;

namespace HoaTheoMua.Service.PostGeneration.Validate;

public class RegenerateImageValidator : AbstractValidator<Request.RegenerateImageRequest>
{
    public RegenerateImageValidator()
    {
        RuleFor(x => x.SourceBasePostImageId).NotEmpty();
        RuleFor(x => x.RatioCode).NotNull().IsInEnum();
        RuleFor(x => x.SourceGeneratedPostImageId).NotEqual(Guid.Empty).When(x => x.SourceGeneratedPostImageId.HasValue);
        RuleFor(x => x.Feedback).MaximumLength(2000);
    }
}

public class UpdatePlatformImagesValidator : AbstractValidator<Request.UpdatePlatformImagesRequest>
{
    public UpdatePlatformImagesValidator()
    {
        RuleFor(x => x.Images).NotNull().Must(x => x == null || x.Count <= 100);
        RuleForEach(x => x.Images).NotNull().SetValidator(new PlatformImageValidator());
    }
}

public class PlatformImageValidator : AbstractValidator<Request.PlatformImageInput>
{
    public PlatformImageValidator()
    {
        RuleFor(x => x.SourceBasePostImageId).NotEmpty();
        RuleFor(x => x.RatioCode).NotNull().IsInEnum();
        RuleFor(x => x.SelectedGeneratedPostImageId).NotEqual(Guid.Empty).When(x => x.SelectedGeneratedPostImageId.HasValue);
        RuleFor(x => x.SortOrder).GreaterThan(0);
    }
}

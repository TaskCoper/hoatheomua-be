using FluentValidation;

namespace HoaTheoMua.Service.PostGeneration.Validate;

public class GenerateValidator : AbstractValidator<Request.GenerateRequest>
{
    public GenerateValidator()
    {
        RuleFor(x => x.Platforms).NotNull().Must(x => x is { Count: >= 1 and <= 3 });
        RuleForEach(x => x.Platforms).NotNull().SetValidator(new PlatformValidator());
        RuleFor(x => x.Platforms).Must(x => x == null || x.Where(p => p != null).Select(p => p.Platform).Distinct().Count() == x.Count)
            .WithMessage("Duplicate platform");
    }
}
public class PlatformValidator : AbstractValidator<Request.PlatformInput>
{
    public PlatformValidator()
    {
        RuleFor(x => x.Platform).NotNull().IsInEnum();
        RuleFor(x => x.UsePreset).Equal(false).WithMessage("Image generation is not available yet");
        RuleFor(x => x.Ratios).Null().WithMessage("Image generation is not available yet");
    }
}

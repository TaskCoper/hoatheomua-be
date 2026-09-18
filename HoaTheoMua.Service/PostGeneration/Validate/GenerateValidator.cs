using FluentValidation;

namespace HoaTheoMua.Service.PostGeneration.Validate;

public class GenerateValidator : AbstractValidator<Request.GenerateRequest>
{
    public GenerateValidator()
    {
        RuleFor(x => x.Mode).Must(x => x is "content" or "images" or "all")
            .WithMessage("Mode must be content, images or all");
        RuleFor(x => x).Must(x => x.Platforms == null || x.Platforms.All(p => p == null || (!p.UsePreset && p.Ratios == null)))
            .When(x => x.Mode == "content").WithMessage("Content mode does not accept image selections");
        RuleFor(x => x).Must(x => x.Platforms == null || x.Platforms.All(p => p == null || p.UsePreset || p.Ratios is { Count: > 0 }))
            .When(x => x.Mode == "images").WithMessage("Images mode requires ratios or usePreset for every platform");
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
        RuleFor(x => x).Must(x => !x.UsePreset || x.Ratios == null).WithMessage("UsePreset and ratios are mutually exclusive");
        When(x => x.Ratios != null, () =>
        {
            RuleFor(x => x.Ratios).Must(x => x is { Count: >= 1 and <= 5 });
            RuleFor(x => x.Ratios).Must(x => x == null || x.Where(r => r != null).Select(r => r.RatioCode).Distinct().Count() == x.Count)
                .WithMessage("Duplicate ratio");
            RuleForEach(x => x.Ratios).NotNull().SetValidator(new RatioValidator());
        });
    }
}

public class RatioValidator : AbstractValidator<Request.RatioInput>
{
    public RatioValidator()
    {
        RuleFor(x => x.RatioCode).NotNull().IsInEnum();
        RuleFor(x => x.BasePostImageIds).Must(x => x == null || x.Count <= 20);
        RuleFor(x => x.BasePostImageIds).Must(x => x == null || (!x.Contains(Guid.Empty) && x.Distinct().Count() == x.Count))
            .WithMessage("Image IDs must be nonempty and unique");
    }
}

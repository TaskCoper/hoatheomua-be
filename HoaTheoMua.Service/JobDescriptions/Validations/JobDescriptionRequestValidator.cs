using FluentValidation;

namespace HoaTheoMua.Service.JobDescriptions.Validations;

public class JobDescriptionRequestValidator : AbstractValidator<Request.CreateRequest>
{
    public JobDescriptionRequestValidator()
    {
        RuleFor(x => x.title).NotEmpty().WithMessage("TITLE_REQUIRED");
        RuleFor(x => x.location).NotEmpty().WithMessage("LOCATION_REQUIRED");
        RuleFor(x => x.avatarUrl).NotEmpty().WithMessage("AVATAR_URL_REQUIRED");
        RuleFor(x => x.coverImageUrl).NotEmpty().WithMessage("COVER_IMAGE_URL_REQUIRED");
        RuleFor(x => x.description).NotEmpty().WithMessage("DESCRIPTION_REQUIRED");
        RuleFor(x => x.openings).GreaterThan(0).WithMessage("OPENINGS_MUST_BE_GREATER_THAN_ZERO");
        RuleFor(x => x.minExperienceYears).GreaterThanOrEqualTo(0).When(x => x.minExperienceYears.HasValue).WithMessage("EXPERIENCE_CANNOT_BE_NEGATIVE");
        RuleFor(x => x.maxExperienceYears).GreaterThanOrEqualTo(0).When(x => x.maxExperienceYears.HasValue).WithMessage("EXPERIENCE_CANNOT_BE_NEGATIVE");
        RuleFor(x => x.salaryMin).GreaterThanOrEqualTo(0).When(x => x.salaryMin.HasValue).WithMessage("SALARY_CANNOT_BE_NEGATIVE");
        RuleFor(x => x.salaryMax).GreaterThanOrEqualTo(0).When(x => x.salaryMax.HasValue).WithMessage("SALARY_CANNOT_BE_NEGATIVE");
        RuleFor(x => x).Must(x => !x.minExperienceYears.HasValue || !x.maxExperienceYears.HasValue || x.minExperienceYears <= x.maxExperienceYears).WithMessage("MIN_EXPERIENCE_GREATER_THAN_MAX_EXPERIENCE");
        RuleFor(x => x).Must(x => !x.salaryMin.HasValue || !x.salaryMax.HasValue || x.salaryMin <= x.salaryMax).WithMessage("MIN_SALARY_GREATER_THAN_MAX_SALARY");
    }
}

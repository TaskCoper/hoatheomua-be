using System.Text.RegularExpressions;
using FluentValidation;

namespace HoaTheoMua.Service.RecruitmentRequests.Validations;

public class CreateRecruitmentRequestValidator : AbstractValidator<Request.CreateRecruitmentRequest>
{

    public CreateRecruitmentRequestValidator()
    {
        RuleFor(x => x.jobDescriptionId)
            .NotEqual(Guid.Empty).WithMessage("JOB_DESCRIPTION_ID_REQUIRED");

        RuleFor(x => x.title)
            .NotEmpty().WithMessage("TITLE_REQUIRED");

        RuleFor(x => x.coverLetter)
            .NotEmpty().WithMessage("COVER_LETTER_REQUIRED");
            
        RuleForEach(x => x.attachments)
            .NotEmpty().WithMessage("ATTACHMENT_INVALID");
    }

}

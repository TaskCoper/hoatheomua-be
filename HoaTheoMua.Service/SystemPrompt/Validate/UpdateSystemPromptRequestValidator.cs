using FluentValidation;

namespace HoaTheoMua.Service.SystemPrompt.Validate;

public class UpdateSystemPromptRequestValidator : AbstractValidator<Request.UpdateSystemPromptRequest>
{
    public UpdateSystemPromptRequestValidator()
    {
        RuleFor(x => x.Content)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Content không được để trống.")
            .MaximumLength(20_000).WithMessage("Content tối đa 20.000 ký tự.")
            .Must(x => x.All(c => !char.IsControl(c) || c is '\t' or '\r' or '\n'))
            .WithMessage("Content chứa ký tự điều khiển không hỗ trợ.");
    }
}
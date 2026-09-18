using FluentValidation;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.FlowerReference.Validate;

public class GetFlowerReferenceValidator : AbstractValidator<Request.GetFlowerReferenceRequest>
{
    public GetFlowerReferenceValidator()
    {
        RuleFor(x => x.Type)
            .Must(value => value is null || (!string.IsNullOrWhiteSpace(value) && IsValidType(value)))
            .WithMessage("Loại FlowerReference không hợp lệ.");
    }

    private static bool IsValidType(string value) =>
        !int.TryParse(value, out _) && Enum.TryParse<FlowerReferenceType>(value, true, out _);
}

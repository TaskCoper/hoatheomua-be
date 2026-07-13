using FluentValidation;
using HoaTheoMua.Service.Common;

namespace HoaTheoMua.Service.CompanyAddress.Validations;

public class UpdateCompanyAddressRequestValidator : AbstractValidator<Request.UpdateCompanyAddressRequest>
{
    public UpdateCompanyAddressRequestValidator()
    {
        RuleFor(x => x.AddressLine)
            .NotEmpty().WithMessage("ADDRESS_REQUIRED")
            .Length(5, 255).WithMessage("ADDRESS_LENGTH_INVALID");

        RuleFor(x => x.Ward)
            .NotEmpty().WithMessage("WARD_REQUIRED")
            .MaximumLength(100).WithMessage("WARD_LENGTH_INVALID");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("CITY_REQUIRED")
            .Length(2, 100).WithMessage("CITY_LENGTH_INVALID");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("COUNTRY_REQUIRED")
            .Length(2, 100).WithMessage("COUNTRY_LENGTH_INVALID");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("LATITUDE_OUT_OF_RANGE")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("LONGITUDE_OUT_OF_RANGE")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FULL_NAME_REQUIRED")
            .Length(2, 100).WithMessage("FULL_NAME_LENGTH_INVALID");

        RuleFor(x => x.Phone).PhoneNumberVn();

        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("MAP_ID_REQUIRED");
    }
}

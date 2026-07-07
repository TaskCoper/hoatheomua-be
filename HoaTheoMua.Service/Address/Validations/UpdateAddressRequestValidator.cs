using FluentValidation;
using HoaTheoMua.Service.Common;

namespace HoaTheoMua.Service.Address.Validations;

public class UpdateAddressRequestValidator : AbstractValidator<Request.UpdateAddressRequest>
{
    public UpdateAddressRequestValidator()
    {
        RuleFor(x => x.ReceiverName)
            .NotEmpty().WithMessage("RECEIVER_NAME_REQUIRED")
            .Length(1, 100).WithMessage("RECEIVER_NAME_LENGTH_INVALID");

        RuleFor(x => x.PhoneNumber).PhoneNumberVn();

        RuleFor(x => x.AddressLine)
            .NotEmpty().WithMessage("ADDRESS_LINE_REQUIRED")
            .Length(1, 255).WithMessage("ADDRESS_LINE_LENGTH_INVALID");

        RuleFor(x => x.Ward)
            .NotEmpty().WithMessage("WARD_REQUIRED")
            .MaximumLength(100).WithMessage("WARD_LENGTH_INVALID");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("CITY_REQUIRED")
            .Length(1, 100).WithMessage("CITY_LENGTH_INVALID");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("COUNTRY_REQUIRED")
            .Length(1, 100).WithMessage("COUNTRY_LENGTH_INVALID");

        RuleFor(x => x.MapId)
            .NotEmpty().WithMessage("MAP_ID_REQUIRED")
            .MaximumLength(255).WithMessage("MAP_ID_LENGTH_INVALID");

        RuleFor(x => x.Latitude)
            .NotNull().WithMessage("LATITUDE_REQUIRED")
            .InclusiveBetween(-90m, 90m).WithMessage("LATITUDE_OUT_OF_RANGE");

        RuleFor(x => x.Longitude)
            .NotNull().WithMessage("LONGITUDE_REQUIRED")
            .InclusiveBetween(-180m, 180m).WithMessage("LONGITUDE_OUT_OF_RANGE");
    }
}

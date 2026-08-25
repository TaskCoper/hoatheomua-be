using FluentValidation;

namespace HoaTheoMua.Service.XanhSmService.Validations;

public sealed class AvailableServicesRequestValidator : AbstractValidator<Request.GetAvailableServicesRequest>
{
    /// <summary>
    /// Kiểm tra dữ liệu tối thiểu để Xanh SM suy ra geofence: phải có pickup,
    /// mọi tọa độ hợp lệ, service type không rỗng và order_time không âm.
    /// </summary>
    public AvailableServicesRequestValidator()
    {
        RuleFor(x => x.Pickups).NotEmpty().WithMessage("PICKUP_POINT_REQUIRED");
        RuleForEach(x => x.Pickups).ChildRules(point =>
        {
            point.RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage("LATITUDE_INVALID");
            point.RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage("LONGITUDE_INVALID");
        });
        RuleForEach(x => x.Dropoffs!).ChildRules(point =>
        {
            point.RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage("LATITUDE_INVALID");
            point.RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage("LONGITUDE_INVALID");
        }).When(x => x.Dropoffs is not null);
        RuleForEach(x => x.ServiceTypes!).NotEmpty().WithMessage("SERVICE_TYPE_INVALID").When(x => x.ServiceTypes is not null);
        RuleFor(x => x.OrderTime).GreaterThanOrEqualTo(0).When(x => x.OrderTime.HasValue).WithMessage("ORDER_TIME_INVALID");
    }
}

using FluentValidation;

namespace HoaTheoMua.Service.Order.Validate;

public class CreateOrderRequestValidator: AbstractValidator<Request.CreateOrderRequest>  
{  
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.OrderTime)
            .Must(BeValidOrderTime)
            .WithMessage("OrderTime must be 0 or within the next 7 days.");
    }
    private bool BeValidOrderTime(long orderTime)
    {
        if (orderTime == 0)
            return true;

        var orderDate = DateTimeOffset
            .FromUnixTimeSeconds(orderTime)
            .UtcDateTime;

        var now = DateTime.UtcNow;

        return orderDate >= now &&
               orderDate <= now.AddDays(7);
    }
    
}
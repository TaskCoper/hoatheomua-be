using FluentValidation;

namespace HoaTheoMua.Service.Order.Validate;

public class CreateOrderRequestValidator: AbstractValidator<Request.CreateOrderRequest>  
{  
    public CreateOrderRequestValidator()  
    {        RuleFor(x => x.OrderTime)  
        .Must(BeWithin7Days)  
        .WithMessage("OrderTime must be within the next 7 days.");  
    }  
    private bool BeWithin7Days(long orderTime)  
    {        var orderDate = DateTimeOffset.FromUnixTimeSeconds(orderTime).UtcDateTime;  
        var now = DateTime.UtcNow;  
  
        return orderDate >= now && orderDate <= now.AddDays(7);  
    }
    
}
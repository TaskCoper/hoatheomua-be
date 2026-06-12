namespace HoaTheoMua.Service.Order;

public class Request
{
    public class CreateOrderRequest
    {
        public Guid UserId { get; set; }
        public dynamic Metadata { get; set; } = null!;

    }
}
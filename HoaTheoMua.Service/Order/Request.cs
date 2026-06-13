namespace HoaTheoMua.Service.Order;

public class Request
{
    public class CreateOrderRequest
    {
        public dynamic Metadata { get; set; } = null!;
    }

    public class UpdateOrderRequest
    {
        public Guid OrderId { get; set; }
        public string TrackNumber { get; set; }
    }
}
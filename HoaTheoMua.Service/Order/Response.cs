namespace HoaTheoMua.Service.Order;

public class Response
{
    public class CreateOrderResponse
    {
        public string QrCodeUrl { get; set; } = string.Empty; 
    }

    public class GetOrderResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public string TrackNumber { get; set; }
    }
}
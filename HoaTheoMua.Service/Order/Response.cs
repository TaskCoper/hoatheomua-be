namespace HoaTheoMua.Service.Order;

public class Response
{
    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string QrCodeUrl { get; set; } = string.Empty; 
    }
}
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
    
    public class UpdateOrderByIdRequest
    {
        //public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        
        public string? NhanhOrderId { get; set; } 
        public string? NhanhTrackingUrl { get; set; } 
        public string? AhahaMoveOrderId { get; set; }
        public string? AhahaMoveTrackingUrl { get; set; } 
        
        public object Metadata { get; set; } = string.Empty;
        
    }
}
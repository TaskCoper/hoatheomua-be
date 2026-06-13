using System.Transactions;
using HoaTheoMua.Repository.Entity;

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
    
    public class GetOrderByIdResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public string TrackNumber { get; set; }
        public string QrCodeUrl { get; set; } = string.Empty;
        
        public OrderTransactionResponse? Transaction { get; set; }
    }
    
    public class OrderTransactionResponse
    {
        public decimal TransferAmount { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Gateway { get; set; }        
        public DateTime TransactionDate { get; set; }
        
    }
    
}
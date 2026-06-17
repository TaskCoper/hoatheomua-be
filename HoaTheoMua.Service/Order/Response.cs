using System.Text.Json.Serialization;
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
        public int? NhanhOrderId { get; set; } 
        public string NhanhTrackingUrl { get; set; } = string.Empty;
        public string AhahaMoveOrderId { get; set; } = string.Empty;
        public string AhahaMoveTrackingUrl { get; set; } = string.Empty;
        public DateTimeOffset CreateAt { get; set; }
        public DateTimeOffset? UpdateAt { get; set; }
    }
    
    public class GetOrderByIdResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public string TrackNumber { get; set; }
        public int? NhanhOrderId { get; set; } 
        public string? NhanhTrackingUrl { get; set; } = string.Empty;
        public string? AhahaMoveOrderId { get; set; } = string.Empty;
        public string? AhahaMoveTrackingUrl { get; set; } = string.Empty;
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
    
    public class NhanhVnLocationResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public List<LocationData> Data { get; set; } = new List<LocationData>();
    }

    public class LocationData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
    
}
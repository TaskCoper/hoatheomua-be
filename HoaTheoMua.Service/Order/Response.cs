using System.Text.Json.Serialization;
using System.Transactions;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.Order;

public class Response
{
    public class GetOrderResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = null!;
        public string TrackNumber { get; set; } = null!;
        //public int? NhanhOrderId { get; set; } 
        //public string NhanhTrackingUrl { get; set; } = string.Empty;
        public string AhahaMoveOrderId { get; set; } = string.Empty;
        public string AhahaMoveTrackingUrl { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public long OrderTime { get; set; }
        public string? Taxcode { get; set; }
        public List<long>? TagsId { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public InvoiceType? InvoiceType { get; set; }
        public bool? IsExportInvoice { get; set; }
        public string ReceiptAddress { get; set; } = string.Empty;
        public string ReceiptEmail { get; set; } =  string.Empty;
        public string ReceiptFullName { get; set; } =   string.Empty;
        public string ReceiptPhoneNumber { get; set; } =   string.Empty;
        
        public DateTimeOffset CreateAt { get; set; }
        public DateTimeOffset? UpdateAt { get; set; }
    }
    
    public class GetOrderByIdResponse
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = null!;
        public string TrackNumber { get; set; } = null!;
       // public int? NhanhOrderId { get; set; } 
       // public string? NhanhTrackingUrl { get; set; } = string.Empty;
        public string? AhahaMoveOrderId { get; set; } = string.Empty;
        public string? AhahaMoveTrackingUrl { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public string? TaxCode { get; set; }
        public long OrderTime { get; set; }
        public List<long>? TagsId { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public InvoiceType? InvoiceType { get; set; }
        public bool? IsExportInvoice { get; set; }
        public string ReceiptAddress { get; set; } = string.Empty;
        public string ReceiptEmail { get; set; } =  string.Empty;
        public string ReceiptFullName { get; set; } =   string.Empty;
        public string ReceiptPhoneNumber { get; set; } =   string.Empty;
        public decimal ShippingFee { get; set; } 
        public decimal ProductFee { get; set; }
        public decimal TotalFee { get; set; }
        public decimal? VatFee { get; set; }
        public DateTimeOffset CreateAt { get; set; }
        public string? PaymentUrl { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        
        public OrderTransactionResponse? Transaction { get; set; }
        public List<OrderDetailsResponse>  Details { get; set; } = new List<OrderDetailsResponse>();
    }
    
    public class OrderTransactionResponse
    {
        public decimal TransferAmount { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Gateway { get; set; }        
        public DateTime TransactionDate { get; set; }
        
    }

    public class OrderDetailsResponse
    {
        public Guid OrderDetailsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public int Number { get; set; }
        public decimal Price { get; set; } 
        public double Weight { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
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
        public string Name { get; set; } = null!;
    }
    
    public class TaxcodeResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = null!;

        [JsonPropertyName("data")]
        public CompanyData Data { get; set; } = null!;

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; } = null!;
    }

    public class CompanyData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("internationalName")]
        public string InternationalName { get; set; } = null!;

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; } = null!;

        [JsonPropertyName("address")]
        public string Address { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    public class Metadata
    {
        [JsonPropertyName("disclaimer")]
        public string Disclaimer { get; set; } = null!;

        [JsonPropertyName("source")]
        public string Source { get; set; } = null!;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("contact")]
        public string Contact { get; set; } = null!;
    }
    
}
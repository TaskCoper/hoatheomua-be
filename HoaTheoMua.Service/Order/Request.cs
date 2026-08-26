using System.Text.Json.Serialization;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.AhamoveService;

namespace HoaTheoMua.Service.Order;

public class Request
{

    public class ItemRequest
    {
        public string ItemId { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public bool? IsCore { get; set; }
    }
    
    public class CreateOrderRequest : AhaMoveRequest.EstimateShippingFeeRequest
    {
        public required string Gateway { get; set; }
        public string? TaxCode { get; set; }
        public List<long>? TagIds { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.VA;

        public string? IdentityNumber { get; set; }

        public string? ReceiptFullName { get; set; }

        public string? ReceiptPhoneNumber { get; set; }

        public string? ReceiptAddress { get; set; }

        public string? ReceiptEmail { get; set; }

        public bool? IsExportInvoice { get; set; }
        public InvoiceType? InvoiceType { get; set; }
        // public required Guid AddressId { get; set; }
        public required string PhoneReceiver { get; set; }
        public required string FullNameReceiver { get; set; }
        // public string? Email { get; set; }
        // public string? PhoneNumber { get; set; }
        // public string? IdentityNumber { get; set; }
        // public string FullName { get; set; }
        // public string Address { get; set; }
    }

    public class LocationRequest
    {
        [JsonPropertyName("filters")]
        public LocationFilters Filters { get; set; } = null!;
    }

    public class LocationFilters
    {
        [JsonPropertyName("locationVersion")]
        public string LocationVersion { get; set; } = "v2";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "CITY";

        [JsonPropertyName("parentId")]
        public int? ParentId { get; set; }
    }

    public class UpdateAhamoveRequest
    {
        public required string OrderId { get; set; }
        public string? AhaMoveTrackingUrl { get; set; }
        public string? TrackingNumber { get; set; }
        public decimal? ShippingFee  { get; set; }
    }

}
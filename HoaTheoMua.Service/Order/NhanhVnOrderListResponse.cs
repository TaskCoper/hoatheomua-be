using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace HoaTheoMua.Service.Order;

public class OrderListRootResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("paginator")]
    public Paginator Paginator { get; set; }

    [JsonPropertyName("data")]
    [System.Text.Json.Serialization.JsonConverter(typeof(Converter))]
    public List<OrderData> Data { get; set; } = new();
    // public List<OrderEntry> Data { get; set; } = new();
}

public class Paginator
{
    [JsonPropertyName("next")]
    public PaginatorNext Next { get; set; }
}

public class PaginatorNext
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class OrderData
{
    [JsonProperty("info")]
    public OrderInfo Info { get; set; }

    [JsonProperty("channel")]
    public Channel Channel { get; set; }

    [JsonProperty("shippingAddress")]
    public ShippingAddress ShippingAddress { get; set; }

    [JsonProperty("products")]
    public List<Product> Products { get; set; }

    [JsonProperty("carrier")]
    public Carrier Carrier { get; set; }

    [JsonProperty("payment")]
    public Payment Payment { get; set; }
}

public class OrderInfo
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("depotId")]
    public int DepotId { get; set; }

    [JsonProperty("pickupDepotId")]
    public int PickupDepotId { get; set; }

    [JsonProperty("mode")]
    public int Mode { get; set; }

    [JsonProperty("exportType")]
    public int ExportType { get; set; }

    [JsonProperty("originalOrderId")]
    public int OriginalOrderId { get; set; }

    [JsonProperty("returnFromId")]
    public int ReturnFromId { get; set; }

    [JsonProperty("idReturn")]
    public int IdReturn { get; set; }

    [JsonProperty("createdById")]
    public int CreatedById { get; set; }

    [JsonProperty("createdByName")]
    public string CreatedByName { get; set; }

    [JsonProperty("saleId")]
    public int SaleId { get; set; }

    [JsonProperty("saleName")]
    public string SaleName { get; set; }

    [JsonProperty("technicalStaffId")]
    public int TechnicalStaffId { get; set; }

    [JsonProperty("packedById")]
    public int PackedById { get; set; }

    [JsonProperty("customerCareId")]
    public int CustomerCareId { get; set; }

    [JsonProperty("customerCareName")]
    public string CustomerCareName { get; set; }

    [JsonProperty("createdAt")]
    public long CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public long UpdatedAt { get; set; }

    [JsonProperty("confirmedAt")]
    public long ConfirmedAt { get; set; }

    [JsonProperty("packedAt")]
    public long PackedAt { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("privateDescription")]
    public string PrivateDescription { get; set; }

    [JsonProperty("orderIndex")]
    public int OrderIndex { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("reason")]
    public int Reason { get; set; }

    [JsonProperty("handoverIds")]
    public List<object> HandoverIds { get; set; }

    [JsonProperty("tags")]
    public List<Tag> Tags { get; set; }

    [JsonProperty("trackingUrl")]
    public string TrackingUrl { get; set; }
}

public class Tag
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("textColor")]
    public string TextColor { get; set; }

    [JsonProperty("bgColor")]
    public string BgColor { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }
}

public class Channel
{
    [JsonProperty("saleChannel")]
    public int SaleChannel { get; set; }

    [JsonProperty("trafficSource")]
    public string TrafficSource { get; set; }

    [JsonProperty("fbAdsId")]
    public int FbAdsId { get; set; }

    [JsonProperty("fbPsid")]
    public int FbPsid { get; set; }

    [JsonProperty("affiliate")]
    public Affiliate Affiliate { get; set; }

    [JsonProperty("marketing")]
    public List<object> Marketing { get; set; }
}

public class Affiliate
{
    [JsonProperty("value")]
    public decimal Value { get; set; }

    [JsonProperty("bonus")]
    public decimal Bonus { get; set; }

    [JsonProperty("bonusRate")]
    public decimal BonusRate { get; set; }
}

public class ShippingAddress
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("mobile")]
    public string Mobile { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("cityId")]
    public int CityId { get; set; }

    [JsonProperty("districtId")]
    public int DistrictId { get; set; }

    [JsonProperty("wardId")]
    public int WardId { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }
}

public class Product
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("typeId")]
    public int TypeId { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("barcode")]
    public string Barcode { get; set; }

    [JsonProperty("weight")]
    public double Weight { get; set; }

    [JsonProperty("quantity")]
    public decimal Quantity { get; set; }

    [JsonProperty("originalQuantity")]
    public decimal OriginalQuantity { get; set; }

    [JsonProperty("originalPrice")]
    public decimal OriginalPrice { get; set; }

    [JsonProperty("displaySalePrice")]
    public decimal DisplaySalePrice { get; set; }

    [JsonProperty("displayVatAmount")]
    public decimal DisplayVatAmount { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("priceVatMode")]
    public int PriceVatMode { get; set; }

    [JsonProperty("priceAfterVAT")]
    public decimal PriceAfterVAT { get; set; }

    [JsonProperty("discount1QtyAmount")]
    public decimal Discount1QtyAmount { get; set; }

    [JsonProperty("discount")]
    public decimal Discount { get; set; }

    [JsonProperty("discountAmount")]
    public decimal DiscountAmount { get; set; }

    [JsonProperty("avgCost")]
    public decimal AvgCost { get; set; }

    [JsonProperty("vat")]
    public decimal Vat { get; set; }

    [JsonProperty("vatPercent")]
    public decimal VatPercent { get; set; }

    [JsonProperty("vatPercentSub")]
    public decimal VatPercentSub { get; set; }

    [JsonProperty("vatAmount")]
    public decimal VatAmount { get; set; }

    [JsonProperty("imeiId")]
    public int ImeiId { get; set; }

    [JsonProperty("imeiIds")]
    public List<object> ImeiIds { get; set; }

    [JsonProperty("imeiCodes")]
    public List<object> ImeiCodes { get; set; }

    [JsonProperty("transactionFee")]
    public decimal TransactionFee { get; set; }

    [JsonProperty("paymentStatus")]
    public int PaymentStatus { get; set; }

    [JsonProperty("partialReturnStatus")]
    public int PartialReturnStatus { get; set; }

    [JsonProperty("usedPoints")]
    public int UsedPoints { get; set; }

    [JsonProperty("usedPointAmount")]
    public decimal UsedPointAmount { get; set; }

    [JsonProperty("image")]
    public string Image { get; set; }

    [JsonProperty("shippingWeight")]
    public string ShippingWeight { get; set; }

    [JsonProperty("unit")]
    public Unit Unit { get; set; }
}

public class Unit
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("originalQuantity")]
    public int OriginalQuantity { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("priceAfterVAT")]
    public decimal PriceAfterVAT { get; set; }
}

public class Carrier
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("service")]
    public CarrierService Service { get; set; }

    [JsonProperty("checkGoods")]
    public int CheckGoods { get; set; }

    [JsonProperty("isPartDelivery")]
    public int IsPartDelivery { get; set; }

    [JsonProperty("isDeclaredFee")]
    public int IsDeclaredFee { get; set; }

    [JsonProperty("sendCarrierType")]
    public int SendCarrierType { get; set; }

    [JsonProperty("sendCarrierAt")]
    public long SendCarrierAt { get; set; }

    [JsonProperty("deliveryAt")]
    public long DeliveryAt { get; set; }

    [JsonProperty("declaredValue")]
    public decimal DeclaredValue { get; set; }

    [JsonProperty("declaredFee")]
    public decimal DeclaredFee { get; set; }

    [JsonProperty("weight")]
    public double Weight { get; set; }

    [JsonProperty("shipFee")]
    public decimal ShipFee { get; set; }

    [JsonProperty("codFee")]
    public decimal CodFee { get; set; }

    [JsonProperty("ecomFee")]
    public decimal EcomFee { get; set; }

    [JsonProperty("overWeightShipFee")]
    public decimal OverWeightShipFee { get; set; }

    [JsonProperty("returnFee")]
    public decimal ReturnFee { get; set; }

    [JsonProperty("customerShipFee")]
    public decimal CustomerShipFee { get; set; }

    [JsonProperty("carrierPaymentStatus")]
    public int CarrierPaymentStatus { get; set; }
}

public class CarrierService
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }
}

public class Payment
{
    [JsonProperty("usedPoints")]
    public UsedPoints UsedPoints { get; set; }

    [JsonProperty("vat")]
    public Vat Vat { get; set; }

    [JsonProperty("discount")]
    public PaymentDiscount Discount { get; set; }

    [JsonProperty("deposit")]
    public Deposit Deposit { get; set; }

    [JsonProperty("transfer")]
    public Transfer Transfer { get; set; }

    [JsonProperty("credit")]
    public Credit Credit { get; set; }

    [JsonProperty("codAmount")]
    public decimal CodAmount { get; set; }

    [JsonProperty("businessPayment")]
    public decimal BusinessPayment { get; set; }

    [JsonProperty("businessPaymentStatus")]
    public int BusinessPaymentStatus { get; set; }
}

public class UsedPoints
{
    [JsonProperty("points")]
    public int Points { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }
}

public class Vat
{
    [JsonProperty("percent")]
    public string Percent { get; set; } // Dữ liệu trả về lúc là string "8%", lúc không có field này nên để string

    [JsonProperty("amount")]
    public decimal Amount { get; set; }
}

public class PaymentDiscount
{
    [JsonProperty("percent")]
    public string Percent { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }
}

public class Deposit
{
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("accountId")]
    public int AccountId { get; set; }
}

public class Transfer
{
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("accountId")]
    public int AccountId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}

public class Credit
{
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("accountId")]
    public int AccountId { get; set; }
}

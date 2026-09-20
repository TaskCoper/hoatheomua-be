namespace HoaTheoMua.Service.Webhook;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class OrderRoot
{
    [JsonPropertyName("depot")]
    public Depot Depot { get; set; } = null!;

    [JsonPropertyName("info")]
    public Info Info { get; set; } = null!;

    [JsonPropertyName("channel")]
    public Channel Channel { get; set; } = null!;

    [JsonPropertyName("carrier")]
    public Carrier Carrier { get; set; } = null!;

    [JsonPropertyName("shippingAddress")]
    public ShippingAddress ShippingAddress { get; set; } = null!;

    [JsonPropertyName("products")]
    public List<Product> Products { get; set; } = null!;

    [JsonPropertyName("payment")]
    public Payment Payment { get; set; } = null!;
}

public class Depot
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("pickupDepotId")]
    public long PickupDepotId { get; set; }
}

public class Info
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("mode")]
    public int Mode { get; set; }

    [JsonPropertyName("depotId")]
    public long DepotId { get; set; }

    [JsonPropertyName("originalOrderId")]
    public long OriginalOrderId { get; set; }

    [JsonPropertyName("returnFromId")]
    public long ReturnFromId { get; set; }

    [JsonPropertyName("idReturn")]
    public long? IdReturn { get; set; }

    [JsonPropertyName("saleId")]
    public long SaleId { get; set; }

    [JsonPropertyName("saleName")]
    public string SaleName { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("privateDescription")]
    public string PrivateDescription { get; set; } = null!;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("tagIds")]
    public List<long> TagIds { get; set; } = new List<long>();

    [JsonPropertyName("technicalStaffId")]
    public long TechnicalStaffId { get; set; }

    [JsonPropertyName("promotionId")]
    public string PromotionId { get; set; } = null!;
}

public class Channel
{
    [JsonPropertyName("trafficSourceId")]
    public int TrafficSourceId { get; set; }

    [JsonPropertyName("appOrderId")]
    public string AppOrderId { get; set; } = null!;
    
    [JsonPropertyName("sourceName")]
    public string SourceName { get; set; } = null!;

    [JsonPropertyName("pageId")]
    public string PageId { get; set; } = null!;

    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; } = null!;

    [JsonPropertyName("vpageUserId")]
    public string VpageUserId { get; set; } = null!;

    [JsonPropertyName("vpageGroupId")]
    public string VpageGroupId { get; set; } = null!;

    [JsonPropertyName("postPurchaseMessage")]
    public string PostPurchaseMessage { get; set; } = null!;

    [JsonPropertyName("fbPsId")]
    public string FbPsId { get; set; } = null!;

    [JsonPropertyName("postId")]
    public string PostId { get; set; } = null!;

    [JsonPropertyName("fbAdsId")]
    public long FbAdsId { get; set; }

    [JsonPropertyName("sourceFrom")]
    public string SourceFrom { get; set; } = null!;

    [JsonPropertyName("id")]
    public long Id { get; set; }
}

public class OrderPackageSize
{
    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("merge")]
    public double Merge { get; set; }

    [JsonPropertyName("length")]
    public double Length { get; set; }

    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }
}

public class Carrier
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("allowTest")]
    public int AllowTest { get; set; } 

    [JsonPropertyName("serviceId")]
    public int ServiceId { get; set; }

    [JsonPropertyName("serviceCode")]
    public string ServiceCode { get; set; } = null!;

    [JsonPropertyName("checkGoods")]
    public int CheckGoods { get; set; }

    [JsonPropertyName("isPartDelivery")]
    public int IsPartDelivery { get; set; }

    [JsonPropertyName("isDeclaredFee")]
    public int IsDeclaredFee { get; set; }

    [JsonPropertyName("isFeeCustomerReject")]
    public int IsFeeCustomerReject { get; set; }

    [JsonPropertyName("feeCustomerReject")]
    public decimal FeeCustomerReject { get; set; }

    [JsonPropertyName("deliveryDate")]
    public string DeliveryDate { get; set; } = null!;

    [JsonPropertyName("deliveryHour")]
    public string DeliveryHour { get; set; } = null!;

    [JsonPropertyName("deliveryMinute")]
    public string DeliveryMinute { get; set; } = null!;

    [JsonPropertyName("deliveryTime")]
    public string DeliveryTime { get; set; } = null!;

    [JsonPropertyName("carrierAccountId")]
    public long CarrierAccountId { get; set; }

    [JsonPropertyName("carrierShopId")]
    public string CarrierShopId { get; set; } = null!;

    [JsonPropertyName("declaredValue")]
    public decimal? DeclaredValue { get; set; }

    [JsonPropertyName("declaredFee")]
    public decimal DeclaredFee { get; set; }

    [JsonPropertyName("shipFee")]
    public decimal ShipFee { get; set; }

    [JsonPropertyName("codFee")]
    public decimal CodFee { get; set; }

    [JsonPropertyName("isSelfConnect")]
    public int IsSelfConnect { get; set; }

    [JsonPropertyName("carrierPromotionCode")]
    public string CarrierPromotionCode { get; set; } = null!;

    [JsonPropertyName("carrierPromotionCodes")]
    public List<string> CarrierPromotionCodes { get; set; } = new List<string>();

    [JsonPropertyName("manualShipFee")]
    public decimal ManualShipFee { get; set; }

    [JsonPropertyName("customerShipFee")]
    public decimal CustomerShipFee { get; set; }

    [JsonPropertyName("followTotalFee")]
    public decimal FollowTotalFee { get; set; }

    [JsonPropertyName("orderPackageSize")]
    public OrderPackageSize OrderPackageSize { get; set; } = null!;

    [JsonPropertyName("isBrokenGoods")]
    public int IsBrokenGoods { get; set; }

    [JsonPropertyName("isDropOff")]
    public int IsDropOff { get; set; }

    [JsonPropertyName("isReturnGoods")]
    public int IsReturnGoods { get; set; }

    [JsonPropertyName("isDocument")]
    public int IsDocument { get; set; }

    [JsonPropertyName("handDelivery")]
    public int HandDelivery { get; set; }
}

public class ShippingAddress
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!; 

    [JsonPropertyName("mobile")]
    public string Mobile { get; set; } = null!;

    [JsonPropertyName("address")]
    public string Address { get; set; } = null!;

    [JsonPropertyName("cityId")]
    public int CityId { get; set; }

    [JsonPropertyName("wardId")]
    public int WardId { get; set; }

    [JsonPropertyName("locationVersion")]
    public string LocationVersion { get; set; } = null!;
}

public class Product
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; } //int
    
    [JsonPropertyName("vat")]
    public int? Vat { get; set; }
}

public class Payment
{
    [JsonPropertyName("depositAmount")]
    public decimal DepositAmount { get; set; }

    [JsonPropertyName("depositAccountId")]
    public long DepositAccountId { get; set; }

    [JsonPropertyName("transferAmount")]
    public decimal TransferAmount { get; set; }

    [JsonPropertyName("transferAccountId")]
    public long TransferAccountId { get; set; }

    [JsonPropertyName("installmentAmount")]
    public decimal InstallmentAmount { get; set; }

    [JsonPropertyName("installmentCode")]
    public string InstallmentCode { get; set; } = null!;

    [JsonPropertyName("installmentAccountId")]
    public long InstallmentAccountId { get; set; }

    [JsonPropertyName("creditAmount")]
    public decimal CreditAmount { get; set; }

    [JsonPropertyName("creditCode")]
    public string CreditCode { get; set; } = null!;

    [JsonPropertyName("creditAccountId")]
    public long CreditAccountId { get; set; }

    [JsonPropertyName("discount")]
    public decimal Discount { get; set; }

    [JsonPropertyName("discountType")]
    public string DiscountType { get; set; } = null!;

    [JsonPropertyName("couponCode")]
    public string CouponCode { get; set; } = null!;

    [JsonPropertyName("couponMoney")]
    public decimal CouponMoney { get; set; }

    [JsonPropertyName("couponIsValid")]
    public int CouponIsValid { get; set; }

    [JsonPropertyName("usedPoints")]
    public int UsedPoints { get; set; }

    [JsonPropertyName("vatValue")]
    public double? VatValue { get; set; }

    [JsonPropertyName("vatType")]
    public string VatType { get; set; } = null!;

    [JsonPropertyName("taxCode")]
    public string TaxCode { get; set; } = null!;

    [JsonPropertyName("taxDate")]
    public string TaxDate { get; set; } = null!;

    [JsonPropertyName("debtDueDate")]
    public string DebtDueDate { get; set; } = null!;

    [JsonPropertyName("cancelAutoPoint")]
    public int CancelAutoPoint { get; set; }

    [JsonPropertyName("cancelAutoPromotion")]
    public int CancelAutoPromotion { get; set; }

    [JsonPropertyName("paymentMoMoCrm")]
    public int PaymentMoMoCrm { get; set; }

    [JsonPropertyName("moneyTransferPaymentLink")]
    public int MoneyTransferPaymentLink { get; set; }

    [JsonPropertyName("totalProductPrice")]
    public decimal TotalProductPrice { get; set; }

    [JsonPropertyName("useManualDiscount")]
    public int UseManualDiscount { get; set; }
}
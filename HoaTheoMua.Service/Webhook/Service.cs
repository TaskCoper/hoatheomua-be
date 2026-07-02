using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.AhaMoveService;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Webhook;

public partial class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly HoaTheoMua.Service.Order.IService _orderService;
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly AhaMoveOption _ahaMoveOption;
    private readonly Config.IService _configService;
    private readonly HoaTheoMua.Service.AhaMoveService.IService _ahaMoveService;

    public Service(AppDbContext dbContext, HttpClient httpClient, IConfiguration configuration, 
        HoaTheoMua.Service.Order.IService orderService,  Config.IService configService,
        HoaTheoMua.Service.AhaMoveService.IService ahaMoveService)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _orderService = orderService;
        _nhanhVnOption = new NhanhVnOption();
        _ahaMoveOption = new AhaMoveOption();
        _configService = configService;
        _ahaMoveService = ahaMoveService;
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        configuration.GetSection(nameof(AhaMoveOption)).Bind(_ahaMoveOption);
    }

    // Match "HTM" (optional separator) + a GUID. The bank/SePay strips dashes, so the GUID can
    // arrive either as 32 hex chars or in full dashed form, possibly surrounded by other text.
    [GeneratedRegex(
        @"HTM[-_]?([0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12})",
        RegexOptions.IgnoreCase)]
    private static partial Regex PaymentCodeRegex();

    private static bool TryExtractOrderId(string? value, out Guid orderId)
    {
        orderId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var match = PaymentCodeRegex().Match(value);
        if (!match.Success)
        {
            return false;
        }

        var hex = match.Groups[1].Value.Replace("-", "");
        return Guid.TryParseExact(hex, "N", out orderId);
    }

    // Same prefix as PaymentCodeRegex but matches a partial run of hex (with optional dashes),
    // used to recover the order when SePay truncates the code below a full 32-char GUID.
    [GeneratedRegex(@"HTM[-_]?([0-9a-fA-F][0-9a-fA-F-]*)", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentCodePrefixRegex();

    // Minimum hex chars before a prefix is unique enough to safely resolve a single order.
    private const int MinPrefixLength = 24;

    private static bool TryExtractOrderIdPrefix(string? value, out string dashedPrefix)
    {
        dashedPrefix = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var match = PaymentCodePrefixRegex().Match(value);
        if (!match.Success)
        {
            return false;
        }

        // Postgres renders uuid as lowercase hex with dashes; normalize to match Id::text.
        var hex = match.Groups[1].Value.Replace("-", "").ToLowerInvariant();

        // Full GUIDs are handled by TryExtractOrderId; only act on genuinely truncated codes.
        if (hex.Length < MinPrefixLength || hex.Length >= 32)
        {
            return false;
        }

        var sb = new StringBuilder(hex.Length + 4);
        for (var i = 0; i < hex.Length; i++)
        {
            if (i is 8 or 12 or 16 or 20)
            {
                sb.Append('-');
            }

            sb.Append(hex[i]);
        }

        dashedPrefix = sb.ToString();
        return true;
    }

    public async Task SepayWebhookHandler(Request.SepayWebhookRequest request)
    {
        // SePay's parsed Code can be truncated (it sometimes drops trailing chars), so prefer the
        // raw transfer memo in Content, which carries the full code, and fall back to Code.
        Repository.Entity.Order? order;

        if (TryExtractOrderId(request.Content, out var orderId) ||
            TryExtractOrderId(request.Code, out orderId))
        {
            order = await _dbContext.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
        else if (TryExtractOrderIdPrefix(request.Content, out var prefix) ||
                 TryExtractOrderIdPrefix(request.Code, out prefix))
        {
            // The code was truncated below a full GUID (e.g. "HTMd950e85c1a9b4a22947b2c8e0185e8",
            // missing the final 2 hex chars). Resolve the order by matching the GUID prefix.
            var matches = await _dbContext.Orders
                .Where(o => o.Id.ToString().StartsWith(prefix))
                .Take(2)
                .ToListAsync();

            if (matches.Count > 1)
            {
                throw new ConflictException("Ambiguous payment code");
            }

            order = matches.FirstOrDefault();
        }
        else
        {
            throw new BadRequestException("Invalid description format");
        }

        if (order == null)
        {
            throw new NotFoundException("Order not found in description");
        }

        if (order.Status != "Pending")
        {
            throw new NotFoundException("Order not on Pending");
            ;
        }

        decimal itemAmount = 0;
        decimal shippingFee = 0;

        if (order.TotalFee != request.TransferAmount)
        {
            throw new ConflictException("Transfer amount must be equal to transfer amount");
        }
        order.Status = "Paid";

        //  var newOrders = await AhaMoveCreateOrder();
        // Bat dau xu ly phan ahamove va nhanhvn

        order.UpdatedAt = DateTimeOffset.UtcNow;
        AhaMoveResponse.CreateAhaMoveOrderResponse? createAhamoveOrderResult = null;

        var companyAddress = await _dbContext.CompanyAddresses
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync();

        string privateDescription = companyAddress?.Remarks ?? "Đến nơi lấy hàng đọc mã đơn để nhận hàng";

        if (order.OrderTime > 0)
        {
            var vnDt = DateTimeOffset
                .FromUnixTimeSeconds(order.OrderTime)
                .ToOffset(TimeSpan.FromHours(7));

            privateDescription += $"\n-------------------------\nGiao ngày: {vnDt:dd/MM/yyyy}";
        }

        if (order.IsExportInvoice == true && order.InvoiceType != null)
        {
            privateDescription += "\n-------------------------";
            
            if (order.InvoiceType == InvoiceType.Business)
            {
                if (!string.IsNullOrWhiteSpace(order.TaxCode))
                {
                    try
                    {
                        var taxInfo = await _orderService.GetInfoTaxCode(order.TaxCode);
                        privateDescription += $"\n{taxInfo}";
                    }
                    catch (BadRequestException ex)
                    {
                        Console.WriteLine($"Failed to get info for TaxCode {order.TaxCode}: {ex.Message}");
                    }
                }
                privateDescription += $"\nMST: {order.TaxCode}";
                privateDescription += $"\nEmail: {order.ReceiptEmail}";
                privateDescription += $"\nSĐT: {order.ReceiptPhoneNumber}";
            }
            else if (order.InvoiceType == InvoiceType.Personal)
            {
                privateDescription += $"\nTên: {order.ReceiptFullName}";
                privateDescription += $"\nCMND/CCCD: {order.IdentityNumber}";
                privateDescription += $"\nĐC: {order.ReceiptAddress}";
                privateDescription += $"\nEmail: {order.ReceiptEmail}";
                privateDescription += $"\nSĐT: {order.ReceiptPhoneNumber}";
            }
        }
        else if(order.OrderTime == 0)
        {
            var itemsRequest = order.OrderDetails.Select(x => new AhaMoveService.AhaMoveRequest.AhamoveItem()
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Price = (int)x.Price,
                    Num = x.Number
                }
            ).ToList();

            var packageDetailRequest = order.OrderDetails.Select(x => new AhaMoveService.AhaMoveRequest.PackageDetail()
                {
                    Height = x.Height,
                    Weight = x.Weight,
                    Length = x.Length,
                    Width = x.Width,
                    Description = x.Name
                }
            ).ToList();

            var ahamoveConfigs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "ahamove");
            var ahamoveServiceDetails = await _ahaMoveService.GetServiceDetailsAhaMove();

            var createRequest = new AhaMoveService.AhaMoveRequest.AhaMoveShippingFeeRequest()
            {
                OrderTime = order.OrderTime,
                IdleUntil = order.OrderTime > 0 ? order.OrderTime : 0,
                Paths = new List<AhaMoveService.AhaMoveRequest.AhamoveLocationPoint>()
                {
                    new AhaMoveService.AhaMoveRequest.AhamoveLocationPoint() 
                    {
                        Latitude = companyAddress?.Latitude ?? 0,
                        Longitude = companyAddress?.Longitude ?? 0,
                        Address = companyAddress?.AddressLine ?? "",
                        ShortAddress = (companyAddress?.AddressLine +", "+ companyAddress?.Ward +", " + companyAddress?.City) ?? "",
                        FullName = companyAddress!.FullName,
                        Phone = companyAddress.PhoneNumber,
                        Remarks = companyAddress.Remarks!,
                    },
                    new AhaMoveService.AhaMoveRequest.AhamoveLocationPoint()
                    {
                        Latitude = order.Latitude,
                        Longitude = order.Longitude,
                        Address = order.Address,
                        ShortAddress = order.ShortAddress,
                        FullName = order.FullName,
                        Phone = order.Phone,
                        Remarks = order.Remarks,
                    },
                },
                ServiceId = "SGN-BIKE",
                Requests = new List<AhaMoveRequest.ItemsRequest>()
                {
                    // new AhaMoveRequest.ItemsRequest()
                    // {
                    //     Id = "SGN-BIKE-SMS",
                    // },
                    // new AhaMoveRequest.ItemsRequest()
                    // {
                    //     Id = "SGN-BIKE-FRAGILE",
                    // },
                    // new AhaMoveRequest.ItemsRequest()
                    // {
                    //     Id = "SGN-BIKE-BULKY",
                    //     TierCode = "TIER_2"
                    // },
                    new AhaMoveRequest.ItemsRequest()
                    {
                        Id = "SGN-BIKE-BAGA"
                    },
                },
                PaymentMethod = "CASH",
                Remarks = order.Remarks,
                Items = itemsRequest,
                PackageDetail = packageDetailRequest,
            };

            var ahamoveApi = ahamoveConfigs.GetValue("ahamove.base_url", _ahaMoveOption.BaseUrl) + "/v3/orders";
            var ahamoveToken = ahamoveConfigs.GetValue("ahamove.api_key", _ahaMoveOption.Token);
            var ahamoveHttpRequest = new HttpRequestMessage(HttpMethod.Post, ahamoveApi);
            ahamoveHttpRequest.Headers.Add("Authorization", ahamoveToken);
            ahamoveHttpRequest.Content = JsonContent.Create(createRequest);

            HttpResponseMessage ahamoveResponse = await _httpClient.SendAsync(ahamoveHttpRequest);

            if (ahamoveResponse.IsSuccessStatusCode)
            {
                createAhamoveOrderResult =
                    await ahamoveResponse.Content.ReadFromJsonAsync<AhaMoveResponse.CreateAhaMoveOrderResponse>();
            }
            else
            {
                throw new Exception($"Ahamove API HTTP Error ({ahamoveResponse.StatusCode})");
            }

            if (createAhamoveOrderResult == null)
            {
                throw new Exception("Ahamove returned null or empty data");
            }

            order.AhahaMoveOrderId = createAhamoveOrderResult.OrderId;
            order.AhahaMoveTrackingUrl = createAhamoveOrderResult.SharedLink;
        }

        var productNhanhVnRequest = new List<Product>();
        
        if (order.TaxCode != null && order.InvoiceType == InvoiceType.Business && order.IsExportInvoice == true)
        {
             productNhanhVnRequest = order.OrderDetails.Select(
                x => new Product()
                {
                    Id = x.ItemId,
                    Price = x.Price,
                    Quantity = x.Number,
                    Vat = 8,
                }
            ).ToList();
        }
        else
        { 
            productNhanhVnRequest = order.OrderDetails.Select(
                x => new Product()
                {
                    Id = x.ItemId,
                    Price = x.Price,
                    Quantity = x.Number,
                }
            ).ToList();
        }
        
        var nhanhVnOrderSources = await _configService.GetNhanhVnOrderSources();
        
        const long vatTagId = 253L;
        var tagIds = !string.IsNullOrWhiteSpace(order.TaxCode)
            ? order.TagIds!.Append(vatTagId).ToList()
            : order.TagIds!;
        
        var nhanhVnRequest = new OrderRoot
        {
            Depot = new Depot
            {
                Id = 129255,
                PickupDepotId = 0
            },
            Info = new Info
            {
                Type = 1,
                Mode = 1,
                DepotId = 129255,
                OriginalOrderId = 0,
                ReturnFromId = 0,
                IdReturn = null,
                SaleId = 0,
                SaleName = "",
                Description = order.Remarks,
                PrivateDescription = privateDescription,
                Status = 54,
                TagIds = tagIds,
                TechnicalStaffId = 0,
                PromotionId = ""
            },
            Channel = new Channel
            {
                TrafficSourceId = 996,
                AppOrderId = createAhamoveOrderResult?.OrderId != null
                    ? createAhamoveOrderResult.OrderId + order.Id.ToString()
                    : order.Id.ToString(),
                SourceName = "WEBSITE",
                PageId = "",
                ConversationId = "",
                VpageUserId = "",
                VpageGroupId = "",
                PostPurchaseMessage = "",
                FbPsId = "",
                PostId = "",
                FbAdsId = 0,
                SourceFrom = nhanhVnOrderSources.ToString(), 
                Id = 0
            },
            Carrier = new Carrier
            {
                Id = 12,
                AllowTest = 1,
                ServiceId = 0,
                ServiceCode = "",
                CheckGoods = 0,
                IsPartDelivery = 0,
                IsDeclaredFee = 0,
                IsFeeCustomerReject = 0,
                FeeCustomerReject = 0m, // Hậu tố 'm' biểu thị kiểu decimal
                DeliveryDate = "",
                DeliveryHour = "",
                DeliveryMinute = "",
                DeliveryTime = "",
                CarrierAccountId = 0,
                CarrierShopId = "",
                DeclaredValue = null,
                DeclaredFee = 0m,
                ShipFee = 0m,
                CodFee = 0m,
                IsSelfConnect = 0,
                CarrierPromotionCode = "",
                CarrierPromotionCodes = new List<string>(),
                ManualShipFee = 0m,
                CustomerShipFee = order.ShippingFee,
                FollowTotalFee = 0m,
                OrderPackageSize = new OrderPackageSize
                {
                    Height = 0,
                    Merge = 0,
                    Length = 0,
                    Weight = 0,
                    Width = 0
                },
                IsBrokenGoods = 0,
                IsDropOff = 0,
                IsReturnGoods = 0,
                IsDocument = 0,
                HandDelivery = 0
            },
            ShippingAddress = new ShippingAddress
            {
                Name = order.FullName,
                Mobile = order.Phone,
                Address = order.Address,
                CityId = order.City,
                WardId = order.Ward,
                LocationVersion = "v2"
            },
            Products = productNhanhVnRequest,
            Payment = new Payment
            {
                DepositAmount = 0m,
                DepositAccountId = 0,
                TransferAmount = order.TotalFee,
                TransferAccountId = 65086,
                InstallmentAmount = 0m,
                InstallmentCode = "",
                InstallmentAccountId = 0,
                CreditAmount = 0m,
                CreditCode = "",
                CreditAccountId = 0,
                Discount = 0m,
                DiscountType = "percent",
                CouponCode = "",
                CouponMoney = 0m,
                CouponIsValid = 0,
                UsedPoints = 0,
                VatValue = null,
                VatType = "percent",
                TaxCode = "",
                TaxDate = "",
                DebtDueDate = "",
                CancelAutoPoint = 0,
                CancelAutoPromotion = 0,
                PaymentMoMoCrm = 0,
                MoneyTransferPaymentLink = 0,
                TotalProductPrice = order.ProductFee,
                UseManualDiscount = 0
            }
        };
        
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var createNhanhOrderApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/order/add?appId=" + appId + "&businessId=" + businessId;
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, createNhanhOrderApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhVnRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        // NhanhVn replies with HTTP 200 even when it rejects the order (code != 1), and in that case
        // `data` comes back as an empty array [] instead of an object. A direct
        // ReadFromJsonAsync<OrderResponse>() then blows up with a cryptic
        // "could not be converted to OrderData. Path: $.data". Read the raw body and inspect it
        // defensively so we surface NhanhVn's real error (in `messages`) instead of a 500.
        var nhanhRawContent = await nhanhVnResponse.Content.ReadAsStringAsync();

        if (!nhanhVnResponse.IsSuccessStatusCode)
        {
            throw new Exception($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode}): {nhanhRawContent}");
        }

        using var nhanhDoc = JsonDocument.Parse(nhanhRawContent);
        var nhanhRoot = nhanhDoc.RootElement;

        var nhanhCode = nhanhRoot.TryGetProperty("code", out var nhanhCodeElement)
                        && nhanhCodeElement.ValueKind == JsonValueKind.Number
            ? nhanhCodeElement.GetInt32()
            : 0;

        // Only a code==1 response with `data` as an object is a successful order creation.
        if (nhanhCode != 1
            || !nhanhRoot.TryGetProperty("data", out var nhanhDataElement)
            || nhanhDataElement.ValueKind != JsonValueKind.Object)
        {
            throw new Exception($"NhanhVn order creation failed: {nhanhRawContent}");
        }

        var createOrderNhanhResult = nhanhDataElement.Deserialize<OrderData>();
        if (createOrderNhanhResult == null)
        {
            throw new Exception($"NhanhVn order creation failed: {nhanhRawContent}");
        }

        order.NhanhOrderId = createOrderNhanhResult.Id;
        order.NhanhTrackingUrl = createOrderNhanhResult.TrackingUrl;
        
        _dbContext.Orders.Update(order);

        var transactionEntity = new Transaction()
        {
            SePayId = request.Id,
            Gateway = request.Gateway,
            TransactionDate = DateTime.TryParse(request.TransactionDate, out DateTime parsedDate)
                ? parsedDate.ToUniversalTime()
                : DateTime.UtcNow,
            AccountNumber = request.AccountNumber,
            SubAccount = request.SubAccount,
            Code = request.Code,
            Content = request.Content,
            TransferType = request.TransferType,
            Description = request.Description,
            TransferAmount = request.TransferAmount,
            Accumulated = request.Accumulated,
            ReferenceCode = request.ReferenceCode,

            OrderId = order.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.Transactions.Add(transactionEntity);

        await _dbContext.SaveChangesAsync();

    }
}

//     
    //     List<AhaMoveResponse.ServiceResponse>? ahamoveResult = null;
    //     
    //     if (ahamoveResponse.IsSuccessStatusCode)
    //     {
    //         ahamoveResult = await ahamoveResponse.Content.ReadFromJsonAsync<List<AhaMoveResponse.ServiceResponse>>();
    //     } else
    //     {
    //         // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
    //         throw new Exception($"Ahamove API HTTP Error ({ahamoveResponse.StatusCode})");
    //     }
    //     
    //     if (ahamoveResult == null || ahamoveResult.Any() == false)
    //     {
    //         throw new Exception("Ahamove returned null or empty data");
    //     }   
    //     
    //     return new {ahamoveResult};
    // }
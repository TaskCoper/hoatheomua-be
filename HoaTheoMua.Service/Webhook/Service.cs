using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Webhook;

public partial class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly NhanhVnOption _nhanhVnOption = new();
    private readonly AhamoveOption _ahamoveOption = new();

    public Service(AppDbContext dbContext, HttpClient httpClient, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        configuration.GetSection(nameof(AhamoveOption)).Bind(_ahamoveOption);
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
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


        order.Status = "Paid";

        //  var newOrders = await AhaMoveCreateOrder();
        // Bat dau xu ly phan ahamove va nhanhvn

        var itemsRequest = order.OrderDetails.Select(x => new AhamoveService.Request.AhamoveItem()
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Price = (int)x.Price,
                Num = x.Number
            }
        ).ToList();

        var packageDetailRequest = order.OrderDetails.Select(x => new AhamoveService.Request.PackageDetail()
            {
                Height = x.Height,
                Weight = x.Weight,
                Length = x.Length,
                Width = x.Width,
                Description = "Mo ta"
            }
        ).ToList();

        var createRequest = new AhamoveService.Request.AhaMoveShippingFeeRequest()
        {
            OrderTime = 0,
            Paths = new List<AhamoveService.Request.AhamoveLocationPoint>()
            {
                new AhamoveService.Request.AhamoveLocationPoint() // Thang nay cua SHop minh, Lay tu DB ra
                {
                    // Latitude = companyAddress.Latitude ?? 0,
                    // Longitude = companyAddress.Longitude ?? 0,
                    // Address = companyAddress.AddressLine,
                    // ShortAddress = "",
                    // FullName = "",
                    // Phone = "",
                    // Remarks = "Đến nơi lấy hàng đọc mã đơn để nhận hàng",
                    Latitude = (decimal)10.8055952,
                    Longitude = (decimal)106.6776989,
                    Address = "Số 13A Đào Duy Anh, Phường 9, Phường Phú Nhuận, Thành phố Hồ Chí Minh",
                    ShortAddress = "Số 13A Đào Duy Anh, Phường 9, Phường Phú Nhuận, Thành phố Hồ Chí Minh",
                    FullName = "Hoa Theo Mùa",
                    Phone = "84908765432",
                    Remarks = "Đến nơi lấy hàng đọc mã đơn để nhận hàng",
                },
                new AhamoveService.Request.AhamoveLocationPoint()
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
            Requests = new List<AhamoveService.Request.ItemsRequest>()
            {
                new AhamoveService.Request.ItemsRequest()
                {
                    Id = "SGN-BIKE-SMS",
                },
                new AhamoveService.Request.ItemsRequest()
                {
                    Id = "SGN-BIKE-D2D",
                },
                new AhamoveService.Request.ItemsRequest()
                {
                    Id = "SGN-BIKE-FRAGILE",
                },
                new AhamoveService.Request.ItemsRequest()
                {
                    Id = "SGN-BIKE-BULKY",
                    TierCode = "TIER_2"
                },
            },
            PaymentMethod = "CASH",
            Remarks = order.Remarks,
            Items = itemsRequest,
            PackageDetail = packageDetailRequest,
        };

        // var ahamoveApi = _ahamoveOption.BaseUrl + "/v3/orders";
        var ahamoveApi = "https://partner-apistg.ahamove.com/v3/orders";
        var ahamoveToken = _ahamoveOption.Token;
        var ahamoveHttpRequest = new HttpRequestMessage(HttpMethod.Post, ahamoveApi);
        ahamoveHttpRequest.Headers.Add("Authorization", ahamoveToken);
        ahamoveHttpRequest.Content = JsonContent.Create(createRequest);

        HttpResponseMessage ahamoveResponse = await _httpClient.SendAsync(ahamoveHttpRequest);

        AhaMoveResponse.CreateAhaMoveOrderResponse? createAhamoveOrderResult = null;

        if (ahamoveResponse.IsSuccessStatusCode)
        {
            createAhamoveOrderResult =
                await ahamoveResponse.Content.ReadFromJsonAsync<AhaMoveResponse.CreateAhaMoveOrderResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"Ahamove API HTTP Error ({ahamoveResponse.StatusCode})");
        }

        if (createAhamoveOrderResult == null)
        {
            throw new BadRequestException("Ahamove returned null or empty data");
        }

        order.UpdatedAt = DateTimeOffset.UtcNow;
        order.AhahaMoveOrderId = createAhamoveOrderResult.OrderId;
        order.AhahaMoveTrackingUrl = createAhamoveOrderResult.SharedLink;
        
        var productNhanhVnRequest = order.OrderDetails.Select(
            x => new Product()
            {
                Id = x.ItemId,
                Price = x.Price,
                Quantity = x.Number
            }
        ).ToList();
        
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
                PrivateDescription = "Đến nơi lấy hàng đọc mã đơn để nhận hàng",
                Status = 54,
                TagIds = new List<long>(),
                TechnicalStaffId = 0,
                PromotionId = ""
            },
            Channel = new Channel
            {
                TrafficSourceId = 996,
                AppOrderId = createAhamoveOrderResult.OrderId,
                PageId = "",
                ConversationId = "",
                VpageUserId = "",
                VpageGroupId = "",
                PostPurchaseMessage = "",
                FbPsId = "",
                PostId = "",
                FbAdsId = 0,
                SourceFrom = "",
                Id = 0
            },
            Carrier = new Carrier
            {
                Id = 12,
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
                TotalProductPrice = order.TotalFee,
                UseManualDiscount = 0
            }
        };
        
        var appId = _nhanhVnOption.AppId;
        var businessId = _nhanhVnOption.BusinessId;
        var token = _nhanhVnOption.Token;
        var createNhanhOrderApi = _nhanhVnOption.BaseUrl + "/order/add?appId=" + appId + "&businessId=" + businessId;
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, createNhanhOrderApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhVnRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        OrderResponse? createOrderNhanhResult = null;
                
        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            createOrderNhanhResult = await nhanhVnResponse.Content.ReadFromJsonAsync<OrderResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (createOrderNhanhResult == null || createOrderNhanhResult.Data == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        
        if (createOrderNhanhResult.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }
        order.NhanhOrderId = createOrderNhanhResult.Data.Id;
        order.NhanhTrackingUrl = createOrderNhanhResult.Data.TrackingUrl;
        
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

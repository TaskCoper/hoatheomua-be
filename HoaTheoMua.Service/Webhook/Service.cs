using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
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
    private readonly HoaTheoMua.Service.Order.IService _orderService;
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly AhaMoveOption _ahaMoveOption;
    private readonly SepayOption _sepayOption;
    private readonly Config.IService _configService;
    private readonly AhamoveService.IService _ahaMoveService;

    public Service(AppDbContext dbContext, HttpClient httpClient, IConfiguration configuration,
        HoaTheoMua.Service.Order.IService orderService, Config.IService configService,
        AhamoveService.IService ahaMoveService)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _orderService = orderService;
        _nhanhVnOption = new NhanhVnOption();
        _ahaMoveOption = new AhaMoveOption();
        _sepayOption = new SepayOption();
        _configService = configService;
        _ahaMoveService = ahaMoveService;
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        configuration.GetSection(nameof(AhaMoveOption)).Bind(_ahaMoveOption);
        configuration.GetSection(nameof(SepayOption)).Bind(_sepayOption);
    }

    // SePay signs every webhook call with "Authorization: Apikey <ApiKey>". Fail closed: without a
    // configured key we cannot tell a genuine callback from a forged one, and a forged one flips an
    // order to Paid and dispatches a courier for free.
    private const string SepayAuthorizationScheme = "Apikey";

    public void VerifySepayApiKey(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(_sepayOption.ApiKey))
        {
            throw new ServerException(
                "SepayOption.ApiKey is not configured, so webhook requests cannot be verified");
        }

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            throw new UnauthorizedException("Missing Sepay Authorization header");
        }

        var parts = authorizationHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2
            || !string.Equals(parts[0], SepayAuthorizationScheme, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Invalid Sepay Authorization header");
        }

        var providedKeyBytes = Encoding.UTF8.GetBytes(parts[1]);
        var expectedKeyBytes = Encoding.UTF8.GetBytes(_sepayOption.ApiKey);

        // Lengths must match before FixedTimeEquals: it throws on mismatched-length inputs rather
        // than returning false, and comparing length first still leaks nothing an attacker can use
        // (key length isn't the secret).
        if (providedKeyBytes.Length != expectedKeyBytes.Length
            || !CryptographicOperations.FixedTimeEquals(providedKeyBytes, expectedKeyBytes))
        {
            throw new UnauthorizedException("Invalid Sepay API key");
        }
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

        if (order.TotalFee != request.TransferAmount)
        {
            throw new ConflictException("Transfer amount must be equal to transfer amount");
        }

        var transactionEntity = new Transaction()
        {
            // SePay has no request_id of its own, so its transaction id stands in as the gateway's
            // identifier for this event.
            RequestId = request.Id.ToString(),
            Gateway = request.Gateway,
            // SePay only ever reaches us via the VNPay QR bank transfer.
            paymentMethod = PaymentMethod.VNPayQR,
            TransactionDate = DateTime.TryParse(request.TransactionDate, out DateTime parsedDate)
                ? parsedDate.ToUniversalTime()
                : DateTime.UtcNow,
            AccountNumber = request.AccountNumber,
            SubAccount = request.SubAccount ?? "",
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

        await FulfillPaidOrder(order, transactionEntity);
    }

    public async Task BaoKimWebHookHandler(BaoKimPaymentCallback request)
    {
        var paymentResult = request.PaymentResult;
        if (paymentResult?.Order == null)
        {
            throw new BadRequestException("Missing payment_result.order");
        }

        var baoKimOrder = paymentResult.Order;

        // We send our own Order.Id as mrc_order_id when creating the BaoKim order (see
        // BaoKim.Service.CreateOrder), so it comes back verbatim — none of the memo-parsing
        // guesswork SePay needs.
        if (!Guid.TryParse(baoKimOrder.MrcOrderId, out var orderId))
        {
            throw new BadRequestException("Invalid mrc_order_id format");
        }

        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException("Order not found for mrc_order_id");
        }

        // Anything that isn't a completed payment (failed, cancelled, still pending at the gateway)
        // is acknowledged and dropped: the order stays Pending, and redelivering it would not
        // change the outcome.
        if (paymentResult.ResultCode != BaoKim.BaoKimConstant.CallbackSuccessResultCode
            || baoKimOrder.Status != BaoKim.BaoKimConstant.CallbackCompletedStatus)
        {
            return;
        }

        // BaoKim redelivers until it gets an ack, so a replay of an order we already fulfilled is
        // expected traffic rather than an error — ack it and skip fulfilment instead of throwing,
        // which would otherwise re-run the Ahamove/NhanhVn calls or spin the retry schedule.
        if (order.Status == "Paid")
        {
            return;
        }

        if (order.Status != "Pending")
        {
            throw new ConflictException($"Order is not Pending (current: {order.Status})");
        }

        // CreateOrder sends total_amount as (long)order.TotalFee, so compare on that same
        // truncation — comparing the raw decimal would reject a correct payment.
        if ((long)order.TotalFee != (long)baoKimOrder.TotalAmount)
        {
            throw new ConflictException("Paid amount does not match the order total");
        }

        // `transaction` is a list because BaoKim can split a payment; the completed entry is the one
        // that actually settled. The null-conditionals are load-bearing: an explicit
        // "transaction": null in the payload overwrites the property initializer with null.
        var baoKimTransaction =
            paymentResult.Transaction?.FirstOrDefault(
                t => t.Status == BaoKim.BaoKimConstant.CallbackCompletedStatus)
            ?? paymentResult.Transaction?.FirstOrDefault();

        var baoKimTransactionEntity = new Transaction()
        {
            // BaoKim's own id for this callback. Alphanumeric ("B2B6A278C9815BD7EQ4AVPVZCI"), which
            // is why the column is text rather than the long SePay used.
            RequestId = request.RequestId,
            Gateway = "BaoKim",
            // BaoKim reports how the buyer actually paid. Guard the cast: an int outside the enum
            // would otherwise be persisted as a meaningless value. We always request VA in
            // CreateOrder, so that is the honest fallback.
            paymentMethod = Enum.IsDefined(typeof(PaymentMethod), baoKimOrder.PaymentMethod)
                ? (PaymentMethod)baoKimOrder.PaymentMethod
                : PaymentMethod.VA,
            TransactionDate = ToUtcOrNow(baoKimTransaction?.CompletedAt ?? baoKimOrder.CompletedAt),
            // BaoKim settles into a virtual account it owns and reports no bank account back, so
            // these SePay-shaped columns have nothing to carry (they are NOT NULL).
            AccountNumber = string.Empty,
            SubAccount = string.Empty,
            Code = baoKimOrder.MrcOrderId,
            Content = paymentResult.ResultMessage ?? string.Empty,
            TransferType = "in",
            Description = baoKimTransaction?.Description ?? paymentResult.ResultMessage ?? string.Empty,
            // The gross the buyer paid — deliberately NOT transaction.amount, which is the net
            // settlement after fees (amount = total_amount - merchant_fee - user_fee) and can even
            // be negative: a real 160,000 payment carrying a 337,600 merchant_fee reported
            // amount = -177,600. The docs' sample happens to have zero merchant_fee, which makes
            // amount look like the order total and hides this.
            TransferAmount = baoKimOrder.TotalAmount,
            Accumulated = 0,
            ReferenceCode = baoKimOrder.Id.ToString(),

            OrderId = order.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await FulfillPaidOrder(order, baoKimTransactionEntity);
    }

    // BaoKim leaves completed_at null until a payment settles, and an empty value deserializes to
    // default(DateTime), whose Kind is Unspecified — Npgsql rejects that outright on the
    // timestamptz column, which would turn a good payment into a 500 and a retry storm.
    private static DateTime ToUtcOrNow(DateTime? value)
        => value is { } completedAt && completedAt != default
            ? DateTime.SpecifyKind(completedAt, DateTimeKind.Utc)
            : DateTime.UtcNow;

    // Shared tail for both gateways: flip the order to Paid, dispatch it to Ahamove and NhanhVn,
    // then persist the order and its gateway transaction in one SaveChanges.
    private async Task FulfillPaidOrder(Repository.Entity.Order order, Transaction transaction)
    {
        order.Status = "Paid";

        //  var newOrders = await AhaMoveCreateOrder();
        // Bat dau xu ly phan ahamove va nhanhvn

        order.UpdatedAt = DateTimeOffset.UtcNow;
        AhaMoveResponse.CreateAhaMoveOrderResponse? createAhamoveOrderResult = null;

        var companyAddress = await _dbContext.CompanyAddresses
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync();

        string privateDescription = companyAddress?.Remarks ?? "Đến nơi lấy hàng đọc mã đơn để nhận hàng";

        var vnDt = new DateTimeOffset();
        if (order.OrderTime > 0)
        {
            vnDt = DateTimeOffset
               .FromUnixTimeSeconds(order.OrderTime)
               .ToOffset(TimeSpan.FromHours(7));

            privateDescription += $"\n-------------------------\nGiao: {vnDt:dd/MM/yyyy}";
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
                privateDescription += $"\nMST:{order.TaxCode}";
                privateDescription += $"\nEmail:{order.ReceiptEmail}";
                privateDescription += $"\nSĐT:{order.ReceiptPhoneNumber}";
            }
            else if (order.InvoiceType == InvoiceType.Personal)
            {
                privateDescription += $"\nTên:{order.ReceiptFullName}";
                privateDescription += $"\nCCCD:{order.IdentityNumber}";
                privateDescription += $"\nĐC:{order.ReceiptAddress}";
                privateDescription += $"\nEmail:{order.ReceiptEmail}";
                privateDescription += $"\nSĐT:{order.ReceiptPhoneNumber}";
            }
        }

        const long vatTagId = 253L;
        var tags = order.TagIds ?? new List<long>();
        var tagIds = (order.IsExportInvoice == true && order.InvoiceType != null)
            ? tags.Append(vatTagId).ToList()
            : tags;
        
        var ahamoveConfigs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "ahamove");
        //var ahamoveServiceDetails = await _ahaMoveService.GetServiceDetailsAhaMove();
            
        var ahamoveAllowDelivery = ahamoveConfigs.GetValue("ahamove.instant_delivery", true);
        
        // Mốc unix (giây) Ahamove sẽ tới lấy hàng với đơn giao ngay = hiện tại + ahamove.idle_time.
        // Khai báo ngoài nhánh vì còn dùng lại cho DeliveryTime gửi sang NhanhVn ở dưới.
        long delayTime = 0;
        var delayCallAhamove = ahamoveConfigs.GetValue("ahamove.idle_time", 0);
        delayTime = delayCallAhamove > 0
            ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() + delayCallAhamove
            : 0L;
        
        if (order.OrderTime == 0 && tags.Count == 0 && ahamoveAllowDelivery == true)
        {
            
            var itemsRequest = order.OrderDetails.Select(x => new AhaMoveRequest.AhamoveItem()
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Price = (int)x.Price,
                Num = x.Number
            }
            ).ToList();

            var packageDetailRequest = order.OrderDetails.Select(x => new AhaMoveRequest.PackageDetail()
            {
                Height = x.Height,
                Weight = x.Weight,
                Length = x.Length,
                Width = x.Width,
                Description = x.Name
            }
            ).ToList();

            var createRequest = new AhaMoveRequest.AhaMoveShippingFeeRequest()
            {
                OrderTime = delayTime,
                IdleUntil = delayTime,
                Paths = new List<AhaMoveRequest.AhamoveLocationPoint>()
                {
                    new AhaMoveRequest.AhamoveLocationPoint()
                    {
                        Latitude = companyAddress?.Latitude ?? 0,
                        Longitude = companyAddress?.Longitude ?? 0,
                        Address =(companyAddress?.AddressLine +", "+ companyAddress?.Ward +", " + companyAddress?.City) ?? "",
                        ShortAddress = (companyAddress?.AddressLine +", "+ companyAddress?.Ward +", " + companyAddress?.City) ?? "",
                        FullName = companyAddress!.FullName,
                        Phone = companyAddress.PhoneNumber,
                        Remarks = companyAddress.Remarks!,
                    },
                    new AhaMoveRequest.AhamoveLocationPoint()
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
                throw new BadRequestException($"Ahamove API HTTP Error ({ahamoveResponse.StatusCode})");
            }

            if (createAhamoveOrderResult == null)
            {
                throw new BadRequestException("Ahamove returned null or empty data");
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

        //var nhanhVnOrderSources = await _configService.GetNhanhVnOrderSources();
        var prefixOrderId = order.Id.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

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
                //nếu admin tắt tính năng giao hàng nhanh thì đã xác nhận : ahamoveAllowDelivery == false
                //ngược lại tức là cho phép giao hàng nhanh thì 56: đã xác nhận là đúng rồi
                Status = (order.OrderTime == 0 && tags.Count == 0) ? 56 : 54,
                TagIds = tagIds,
                TechnicalStaffId = 0,
                PromotionId = ""
            },
            Channel = new Channel
            {
                TrafficSourceId = 996,
                AppOrderId = createAhamoveOrderResult?.OrderId != null
                    ? createAhamoveOrderResult.OrderId + " - " + prefixOrderId
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
                SourceFrom = "",
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
                DeliveryDate = (order.OrderTime > 0) ? vnDt.ToString("yyyy-MM-dd") : "",
                // DeliveryDate =  "2026-07-05",
                DeliveryHour = "",
                DeliveryMinute = "",
                // DeliveryTime = (order.OrderTime == 0) ? "" :  "19h",
                DeliveryTime = tags.Count > 0
                    ? await ConvertTimeDelivery(order)
                    : ConvertInstantTimeDelivery(delayTime),
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
                TransferAccountId = (order.IsExportInvoice == true && order.InvoiceType != null) ? 65087 : 65088,
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

        // var debugJson = await nhanhHttpRequest.Content.ReadAsStringAsync();
        // Console.WriteLine("NhanhVN request body: " + debugJson);
        //
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        // NhanhVn replies with HTTP 200 even when it rejects the order (code != 1), and in that case
        // `data` comes back as an empty array [] instead of an object. A direct
        // ReadFromJsonAsync<OrderResponse>() then blows up with a cryptic
        // "could not be converted to OrderData. Path: $.data". Read the raw body and inspect it
        // defensively so we surface NhanhVn's real error (in `messages`) instead of a 500.
        var nhanhRawContent = await nhanhVnResponse.Content.ReadAsStringAsync();

        if (!nhanhVnResponse.IsSuccessStatusCode)
        {
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode}): {nhanhRawContent}");
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
            throw new BadRequestException($"NhanhVn order creation failed: {nhanhRawContent}");
        }

        var createOrderNhanhResult = nhanhDataElement.Deserialize<OrderData>();
        if (createOrderNhanhResult == null)
        {
            throw new BadRequestException($"NhanhVn order creation failed: {nhanhRawContent}");
        }

        order.NhanhOrderId = createOrderNhanhResult.Id;
        order.NhanhTrackingUrl = createOrderNhanhResult.TrackingUrl;

        _dbContext.Orders.Update(order);
        _dbContext.Transactions.Add(transaction);

        await _dbContext.SaveChangesAsync();
    }

    private async Task<string> ConvertTimeDelivery(Repository.Entity.Order order)
    {
        if (order.TagIds == null || order.TagIds.Count == 0)
        {
            return string.Empty;
        }

        var orderTagConfig = await _configService.GetNhanhVnOrderTag();

        var deliveryTime = orderTagConfig.Options?
            .FirstOrDefault(x =>
                x.Value.HasValue && order.TagIds.Contains(x.Value.Value));

        if (deliveryTime == null)
        {
            throw new BadRequestException("Không tồn tại thời gian giao hàng");
        }

        var convertDeliveryTime = Regex
            .Match(deliveryTime.Label ?? string.Empty, @"\d{1,2}");

        if (!convertDeliveryTime.Success || !int.TryParse(convertDeliveryTime.Value, out var time)
                                         || time is < 0 or > 23)
        {
            return string.Empty;
        }

        return $"{time:D2}:00:00";
    }

    // Đơn giao ngay không có tag nên ConvertTimeDelivery luôn trả rỗng. Với các đơn này, giờ giao
    // hiển thị trên NhanhVn là mốc Ahamove tới lấy hàng (hiện tại + ahamove.idle_time), quy về giờ
    // VN (UTC+7) và render theo định dạng H:i:s mà NhanhVn hỗ trợ.
    private static string ConvertInstantTimeDelivery(long scheduledUnixSeconds)
    {
        if (scheduledUnixSeconds <= 0)
        {
            return string.Empty;
        }

        return DateTimeOffset
            .FromUnixTimeSeconds(scheduledUnixSeconds)
            .ToOffset(TimeSpan.FromHours(7))
            .ToString("HH:mm:ss");
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
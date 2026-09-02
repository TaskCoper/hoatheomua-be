using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AhaMoveOption = HoaTheoMua.Service.AhamoveService.AhaMoveOption;
using NhanhVnProduct = HoaTheoMua.Service.Webhook.Product;
using AhaMoveRequest = HoaTheoMua.Service.AhamoveService.AhaMoveRequest;
using NhanhVnOption = HoaTheoMua.Service.AhamoveService.NhanhVnOption;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;

namespace HoaTheoMua.Service.Fulfillment;

/// <summary>
/// Extracted verbatim from Webhook.FulfillPaidOrder, with three deliberate changes: each step now
/// persists its own result immediately, each step is idempotent, and outbound failures are split
/// into transient vs permanent so the retry policy can tell them apart.
/// </summary>
public class Service : IService
{
    /// <summary>Terminal status for an order whose payment landed but whose fulfilment could not.</summary>
    public const string FulfillmentFailedStatus = "FulfillmentFailed";

    // NhanhVn tag ids. 253 marks a VAT-invoice order, 294 marks "giao ngay".
    private const long VatTagId = 253L;
    private const long FastTagId = 294L;

    // NhanhVn depot + accounting ids, carried over unchanged from the original implementation.
    private const int NhanhDepotId = 129255;
    private const int NhanhTrafficSourceId = 996;
    private const int NhanhVatTransferAccountId = 65087;
    private const int NhanhDefaultTransferAccountId = 65088;

    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly AhaMoveOption _ahaMoveOption;
    private readonly HoaTheoMua.Service.Order.IService _orderService;
    private readonly Config.IService _configService;
    private readonly Revalidate.IService _revalidateService;
    private readonly ILogger<Service> _logger;

    public Service(
        AppDbContext dbContext,
        HttpClient httpClient,
        IConfiguration configuration,
        HoaTheoMua.Service.Order.IService orderService,
        Config.IService configService,
        Revalidate.IService revalidateService,
        ILogger<Service> logger)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _orderService = orderService;
        _configService = configService;
        _revalidateService = revalidateService;
        _logger = logger;

        _nhanhVnOption = new NhanhVnOption();
        _ahaMoveOption = new AhaMoveOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        configuration.GetSection(nameof(AhaMoveOption)).Bind(_ahaMoveOption);
    }

    public async Task<DeliveryDispatchResult> DispatchDeliveryAsync(
        Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(orderId, cancellationToken);

        // Temporary: the saga only knows Ahamove until the XanhSm branch is ported. Failing fast
        // here is safer than silently booking an Ahamove courier for a XanhSm order.
        if (order.ShippingGateway != ShippingGateway.Ahamove)
        {
            throw new FulfillmentPermanentException(
                $"Shipping gateway {order.ShippingGateway} is not supported by the saga yet");
        }

        var companyAddress = await _dbContext.CompanyAddresses
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);

        var privateDescription = await BuildPrivateDescriptionAsync(order, companyAddress?.Remarks);

        var tags = order.TagIds ?? new List<long>();
        var tagIds = (order.IsExportInvoice == true && order.InvoiceType != null)
            ? tags.Append(VatTagId).ToList()
            : tags;

        var ahamoveConfigs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "ahamove");
        var ahamoveAllowDelivery = ahamoveConfigs.GetValue("ahamove.instant_delivery", true);

        // Unix seconds Ahamove will collect the parcel. Computed once here and carried in the
        // result: the POS step must show the same moment, and recomputing it there would drift.
        var delayCallAhamove = ahamoveConfigs.GetValue("ahamove.idle_time", 0);
        var delayTime = delayCallAhamove > 0
            ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() + delayCallAhamove
            : 0L;

        var shouldDispatch = order.OrderTime == 0 && tags.Count == 0 && ahamoveAllowDelivery == true;
        if (shouldDispatch)
        {
            tagIds = tagIds.Append(FastTagId).ToList();
        }

        DeliveryDispatchResult Plan(bool dispatched, string? ahamoveOrderId, string? trackingUrl) => new()
        {
            Dispatched = dispatched,
            AhamoveOrderId = ahamoveOrderId,
            AhamoveTrackingUrl = trackingUrl,
            DelayTimeUnix = delayTime,
            TagIds = tagIds,
            PrivateDescription = privateDescription
        };

        // Scheduled order, tagged order, or instant delivery switched off in Config: no courier is
        // booked, and correspondingly there is nothing to compensate later.
        if (!shouldDispatch)
        {
            return Plan(dispatched: false, null, null);
        }

        // Idempotency: a redelivered webhook or a retried message must not book a second courier.
        if (!string.IsNullOrWhiteSpace(order.ShippingPartyOrderId))
        {
            _logger.LogInformation(
                "Order {OrderId} already has Ahamove order {AhamoveOrderId}; skipping dispatch",
                orderId, order.ShippingPartyOrderId);
            return Plan(dispatched: true, order.ShippingPartyOrderId, order.ShippingTrackingUrl);
        }

        // The original dereferenced companyAddress unconditionally here and threw a bare NRE (a 500)
        // when no company address had been configured.
        if (companyAddress == null)
        {
            throw new FulfillmentPermanentException(
                "No CompanyAddress is configured, so the Ahamove pickup point cannot be built");
        }

        var createRequest = new AhaMoveRequest.AhaMoveShippingFeeRequest
        {
            OrderTime = delayTime,
            IdleUntil = delayTime,
            Paths =
            [
                new AhaMoveRequest.AhamoveLocationPoint
                {
                    Latitude = companyAddress.Latitude ?? 0,
                    Longitude = companyAddress.Longitude ?? 0,
                    Address = companyAddress.AddressLine + ", " + companyAddress.Ward + ", " + companyAddress.City,
                    ShortAddress = companyAddress.AddressLine + ", " + companyAddress.Ward + ", " + companyAddress.City,
                    FullName = companyAddress.FullName,
                    Phone = companyAddress.PhoneNumber,
                    Remarks = companyAddress.Remarks!,
                },
                new AhaMoveRequest.AhamoveLocationPoint
                {
                    Latitude = order.Latitude,
                    Longitude = order.Longitude,
                    Address = order.Address,
                    ShortAddress = order.ShortAddress,
                    FullName = order.FullName,
                    Phone = order.Phone,
                    Remarks = order.Remarks,
                },
            ],
            ServiceId = "SGN-BIKE",
            Requests = [new AhaMoveRequest.ItemsRequest { Id = "SGN-BIKE-BAGA" }],
            PaymentMethod = "CASH",
            Remarks = order.Remarks,
            Items = order.OrderDetails.Select(x => new AhaMoveRequest.AhamoveItem
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Price = (int)x.Price,
                Num = x.Number
            }).ToList(),
            PackageDetail = order.OrderDetails.Select(x => new AhaMoveRequest.PackageDetail
            {
                Height = x.Height,
                Weight = x.Weight,
                Length = x.Length,
                Width = x.Width,
                Description = x.Name
            }).ToList(),
        };

        var ahamoveApi = ahamoveConfigs.GetValue("ahamove.base_url", _ahaMoveOption.BaseUrl) + "/v3/orders";
        var ahamoveToken = ahamoveConfigs.GetValue("ahamove.api_key", _ahaMoveOption.Token);

        using var ahamoveHttpRequest = new HttpRequestMessage(HttpMethod.Post, ahamoveApi);
        ahamoveHttpRequest.Headers.Add("Authorization", ahamoveToken);
        ahamoveHttpRequest.Content = JsonContent.Create(createRequest);

        using var ahamoveResponse = await SendAsync(ahamoveHttpRequest, "Ahamove", cancellationToken);
        if (!ahamoveResponse.IsSuccessStatusCode)
        {
            var body = await ahamoveResponse.Content.ReadAsStringAsync(cancellationToken);
            throw HttpFailure("Ahamove", ahamoveResponse.StatusCode, body);
        }

        var createAhamoveOrderResult = await ahamoveResponse.Content
            .ReadFromJsonAsync<AhaMoveResponse.CreateAhaMoveOrderResponse>(cancellationToken);
        if (createAhamoveOrderResult == null)
        {
            throw new FulfillmentTransientException("Ahamove returned null or empty data");
        }

        // Persisted before anything else can fail. In the original this stayed in the change tracker
        // until the single SaveChanges at the very end, so a later NhanhVn failure discarded the id
        // of a courier that had genuinely been booked - and the retry booked another one.
        order.ShippingPartyOrderId = createAhamoveOrderResult.OrderId;
        order.ShippingTrackingUrl = createAhamoveOrderResult.SharedLink;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Ahamove order {AhamoveOrderId} booked and persisted for order {OrderId}",
            createAhamoveOrderResult.OrderId, orderId);

        return Plan(dispatched: true, createAhamoveOrderResult.OrderId, createAhamoveOrderResult.SharedLink);
    }

    public async Task<PosOrderResult> CreatePosOrderAsync(
        Guid orderId, DeliveryDispatchResult plan, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(orderId, cancellationToken);

        if (order.NhanhOrderId.HasValue)
        {
            _logger.LogInformation(
                "Order {OrderId} already has NhanhVn order {NhanhOrderId}; skipping POS creation",
                orderId, order.NhanhOrderId);
            return new PosOrderResult
            {
                NhanhOrderId = order.NhanhOrderId.Value,
                TrackingUrl = order.NhanhTrackingUrl
            };
        }

        var isVatOrder = order.TaxCode != null
                         && order.InvoiceType == InvoiceType.Business
                         && order.IsExportInvoice == true;

        var productNhanhVnRequest = order.OrderDetails.Select(x => new NhanhVnProduct
        {
            Id = x.ItemId,
            Price = x.Price,
            Quantity = x.Number,
            Vat = isVatOrder ? 8 : null,
        }).ToList();

        // `tags` is the order's own tag list, deliberately NOT plan.TagIds: the NhanhVn status and
        // the delivery-time branch below key off whether the customer picked a delivery slot, while
        // plan.TagIds additionally carries the vat/fast markers we add ourselves.
        var tags = order.TagIds ?? new List<long>();

        var vnDt = order.OrderTime > 0
            ? DateTimeOffset.FromUnixTimeSeconds(order.OrderTime).ToOffset(TimeSpan.FromHours(7))
            : default;

        var prefixOrderId = order.Id.ToString()
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;

        var nhanhVnRequest = new OrderRoot
        {
            Depot = new Depot { Id = NhanhDepotId, PickupDepotId = 0 },
            Info = new Info
            {
                Type = 1,
                Mode = 1,
                DepotId = NhanhDepotId,
                OriginalOrderId = 0,
                ReturnFromId = 0,
                IdReturn = null,
                SaleId = 0,
                SaleName = "",
                Description = order.Remarks,
                PrivateDescription = plan.PrivateDescription,
                // 56 = "đã xác nhận" for instant orders; 54 when the shop still has to act on it.
                Status = (order.OrderTime == 0 && tags.Count == 0) ? 56 : 54,
                TagIds = plan.TagIds,
                TechnicalStaffId = 0,
                PromotionId = ""
            },
            Channel = new Channel
            {
                TrafficSourceId = NhanhTrafficSourceId,
                AppOrderId = plan.AhamoveOrderId != null
                    ? plan.AhamoveOrderId + " - " + prefixOrderId
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
                FeeCustomerReject = 0m,
                DeliveryDate = order.OrderTime > 0
                    ? vnDt.ToString("yyyy-MM-dd")
                    : ConvertInstantDateDelivery(plan.DelayTimeUnix),
                DeliveryHour = "",
                DeliveryMinute = "",
                DeliveryTime = tags.Count > 0
                    ? await ConvertTimeDeliveryAsync(order)
                    : ConvertInstantTimeDelivery(plan.DelayTimeUnix),
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
                OrderPackageSize = new OrderPackageSize { Height = 0, Merge = 0, Length = 0, Weight = 0, Width = 0 },
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
                TransferAccountId = (order.IsExportInvoice == true && order.InvoiceType != null)
                    ? NhanhVatTransferAccountId
                    : NhanhDefaultTransferAccountId,
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

        var configs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var createNhanhOrderApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl)
                                  + "/order/add?appId=" + appId + "&businessId=" + businessId;

        using var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, createNhanhOrderApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhVnRequest);

        using var nhanhVnResponse = await SendAsync(nhanhHttpRequest, "NhanhVn", cancellationToken);

        // NhanhVn replies with HTTP 200 even when it rejects the order (code != 1), and in that case
        // `data` comes back as an empty array [] instead of an object, so the body is read raw and
        // inspected defensively rather than deserialized straight into OrderResponse.
        var nhanhRawContent = await nhanhVnResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!nhanhVnResponse.IsSuccessStatusCode)
        {
            throw HttpFailure("NhanhVn", nhanhVnResponse.StatusCode, nhanhRawContent);
        }

        JsonElement nhanhRoot;
        JsonDocument nhanhDoc;
        try
        {
            nhanhDoc = JsonDocument.Parse(nhanhRawContent);
        }
        catch (JsonException ex)
        {
            // A non-JSON 200 is almost always a proxy/maintenance page, which a retry can clear.
            throw new FulfillmentTransientException($"NhanhVn returned a non-JSON body: {ex.Message}");
        }

        using (nhanhDoc)
        {
            nhanhRoot = nhanhDoc.RootElement;

            var nhanhCode = nhanhRoot.TryGetProperty("code", out var nhanhCodeElement)
                            && nhanhCodeElement.ValueKind == JsonValueKind.Number
                ? nhanhCodeElement.GetInt32()
                : 0;

            // Only a code==1 response with `data` as an object is a successful order creation.
            // Anything else is a business rejection: retrying produces the same answer.
            if (nhanhCode != 1
                || !nhanhRoot.TryGetProperty("data", out var nhanhDataElement)
                || nhanhDataElement.ValueKind != JsonValueKind.Object)
            {
                throw new FulfillmentPermanentException($"NhanhVn order creation failed: {nhanhRawContent}");
            }

            var createOrderNhanhResult = nhanhDataElement.Deserialize<OrderData>();
            if (createOrderNhanhResult == null)
            {
                throw new FulfillmentPermanentException($"NhanhVn order creation failed: {nhanhRawContent}");
            }

            order.NhanhOrderId = createOrderNhanhResult.Id;
            order.NhanhTrackingUrl = createOrderNhanhResult.TrackingUrl;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "NhanhVn order {NhanhOrderId} created and persisted for order {OrderId}",
                createOrderNhanhResult.Id, orderId);

            return new PosOrderResult
            {
                NhanhOrderId = createOrderNhanhResult.Id,
                TrackingUrl = createOrderNhanhResult.TrackingUrl
            };
        }
    }

    public async Task RevalidateOrderProductsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var nhanhProductIds = await _dbContext.OrderDetails
            .Where(x => x.OrderId == orderId)
            .Select(x => x.ItemId.ToString())
            .Distinct()
            .ToListAsync(cancellationToken);

        var orderedProducts = await _dbContext.Products
            .Where(x => !x.IsDeleted
                        && x.NhanhProductId != null
                        && nhanhProductIds.Contains(x.NhanhProductId))
            .Select(x => new { x.Id, x.ParentProductId })
            .ToListAsync(cancellationToken);

        var parentNhanhProductIds = orderedProducts
            .Where(x => !string.IsNullOrWhiteSpace(x.ParentProductId))
            .Select(x => x.ParentProductId!)
            .Distinct()
            .ToList();

        var parentProductIds = await _dbContext.Products
            .Where(x => !x.IsDeleted
                        && x.NhanhProductId != null
                        && parentNhanhProductIds.Contains(x.NhanhProductId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var productIds = orderedProducts
            .Select(x => x.Id)
            .Concat(parentProductIds)
            .Distinct()
            .Select(x => x.ToString())
            .ToList();

        var revalidateBody = JsonSerializer.SerializeToElement(new
        {
            entity = "product",
            id = productIds
        });

        await _revalidateService.RevalidateApi(revalidateBody);
    }

    public async Task<CompensationOutcome> CompensateAsync(
        Guid orderId, string? ahamoveOrderId, int? nhanhOrderId, string reason,
        CancellationToken cancellationToken = default)
    {
        var details = new List<string>();
        var retryable = false;

        var deliveryCleared = true;
        if (!string.IsNullOrWhiteSpace(ahamoveOrderId))
        {
            var outcome = await CancelAhamoveOrderAsync(ahamoveOrderId, reason, cancellationToken);
            deliveryCleared = outcome.Cleared;
            retryable |= outcome.Retryable;
            details.Add($"ahamove({ahamoveOrderId})={outcome.Detail}");
        }

        var posOrderCleared = true;
        if (nhanhOrderId.HasValue)
        {
            var outcome = await CancelNhanhCarrierAsync(nhanhOrderId.Value, cancellationToken);
            posOrderCleared = outcome.Cleared;
            retryable |= outcome.Retryable;
            details.Add($"nhanhvn({nhanhOrderId})={outcome.Detail}");
        }

        var summary = details.Count > 0 ? string.Join("; ", details) : "nothing to compensate";

        // Throw ONLY for transport-level failures, so the endpoint retry policy gets another go at
        // both legs - each cancel is idempotent, so re-running is safe. A business rejection must
        // never throw: it would loop forever instead of reaching ops.
        if (retryable)
        {
            throw new FulfillmentTransientException(
                $"Compensation for order {orderId} hit a retryable failure: {summary}");
        }

        if (!deliveryCleared || !posOrderCleared)
        {
            _logger.LogCritical(
                "Compensation for order {OrderId} left a real booking outstanding ({Summary}). Manual cleanup required.",
                orderId, summary);
        }

        return new CompensationOutcome
        {
            DeliveryCleared = deliveryCleared,
            PosOrderCleared = posOrderCleared,
            Details = Truncate(summary)
        };
    }

    public async Task MarkFulfillmentFailedAsync(
        Guid orderId, string step, string reason, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order == null)
        {
            _logger.LogError("Cannot park order {OrderId} as {Status}: order not found", orderId, FulfillmentFailedStatus);
            return;
        }

        order.Status = FulfillmentFailedStatus;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // The one log line ops actually need: the payment was taken and the refund is manual.
        _logger.LogCritical(
            "Order {OrderId} parked as {Status} after {Step}: {Reason}. Payment WAS taken - manual refund required. "
            + "ahamoveOrderId={AhamoveOrderId} nhanhOrderId={NhanhOrderId}",
            orderId, FulfillmentFailedStatus, step, reason, order.ShippingPartyOrderId, order.NhanhOrderId);
    }

    /// <summary>DELETE /v3/orders/{id} - see https://developers.ahamove.com/docs/api-reference/order-apis/cancel-order</summary>
    private async Task<CancelOutcome> CancelAhamoveOrderAsync(
        string ahamoveOrderId, string reason, CancellationToken cancellationToken)
    {
        var configs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "ahamove");
        var baseUrl = configs.GetValue("ahamove.base_url", _ahaMoveOption.BaseUrl);
        var token = configs.GetValue("ahamove.api_key", _ahaMoveOption.Token);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/v3/orders/{ahamoveOrderId}")
        {
            // `comment` is mandatory once a driver has ACCEPTED the order.
            Content = JsonContent.Create(new { comment = Truncate(reason) })
        };
        // Raw token with no "Bearer" prefix. The docs show `Bearer <token>`, but every Ahamove call
        // already in this codebase sends the JWT raw and works against the partner account, so this
        // follows the code that demonstrably works rather than the doc sample.
        request.Headers.Add("Authorization", token);

        HttpResponseMessage response;
        try
        {
            response = await SendAsync(request, "Ahamove cancel", cancellationToken);
        }
        catch (FulfillmentTransientException ex)
        {
            // Surfaced as retryable rather than thrown, so the other leg still gets attempted.
            return new CancelOutcome(false, true, ex.Message);
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Ahamove order {AhamoveOrderId} cancelled", ahamoveOrderId);
                return new CancelOutcome(true, false, "cancelled");
            }

            return response.StatusCode switch
            {
                // ORDER_NOT_FOUND - nothing outstanding, which is the state we wanted anyway.
                HttpStatusCode.NotFound => new CancelOutcome(true, false, "not found (already gone)"),

                // INVALID_ORDER_STATUS - cancellable statuses are IDLE/ASSIGNING/ACCEPTED/CONFIRMING/
                // PAYING, so this means already cancelled OR already delivered. Those two are not
                // distinguishable from the status code, and a delivered order is emphatically NOT
                // cleared, so report it and let a human look.
                HttpStatusCode.NotAcceptable =>
                    new CancelOutcome(false, false, $"not cancellable in current status: {body}"),

                _ when (int)response.StatusCode >= 500 =>
                    new CancelOutcome(false, true, $"HTTP {(int)response.StatusCode}"),

                _ => new CancelOutcome(false, false, $"HTTP {(int)response.StatusCode}: {body}")
            };
        }
    }

    /// <summary>
    /// POST /order/cancelcarrier - see https://apidocs.nhanh.vn/v3/order/cancelcarrier
    /// </summary>
    /// <remarks>
    /// IMPORTANT: this cancels the CARRIER SHIPMENT, not the NhanhVn order record. The POS order
    /// itself survives and still has to be cancelled by hand - which is why the failure log tells
    /// ops the order id rather than claiming the order was cancelled.
    /// </remarks>
    private async Task<CancelOutcome> CancelNhanhCarrierAsync(int nhanhOrderId, CancellationToken cancellationToken)
    {
        var configs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var url = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl)
                  + "/order/cancelcarrier?appId=" + appId + "&businessId=" + businessId;

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new { orderId = nhanhOrderId })
        };
        request.Headers.Add("Authorization", token);

        HttpResponseMessage response;
        try
        {
            response = await SendAsync(request, "NhanhVn cancelcarrier", cancellationToken);
        }
        catch (FulfillmentTransientException ex)
        {
            return new CancelOutcome(false, true, ex.Message);
        }

        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (int)response.StatusCode >= 500
                    ? new CancelOutcome(false, true, $"HTTP {(int)response.StatusCode}")
                    : new CancelOutcome(false, false, $"HTTP {(int)response.StatusCode}: {raw}");
            }

            // Like /order/add, this replies 200 even when it rejects the request; `code` is the
            // real verdict.
            int code;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                code = doc.RootElement.TryGetProperty("code", out var codeElement)
                       && codeElement.ValueKind == JsonValueKind.Number
                    ? codeElement.GetInt32()
                    : 0;
            }
            catch (JsonException)
            {
                return new CancelOutcome(false, true, "non-JSON response");
            }

            if (code == 1)
            {
                _logger.LogInformation("NhanhVn carrier shipment cancelled for order {NhanhOrderId}", nhanhOrderId);
                return new CancelOutcome(true, false, "carrier shipment cancelled");
            }

            // The order never reached a carrier, so there is no shipment outstanding to cancel.
            if (raw.Contains("ERR_ORDER_CARRIER_NOT_YET_SENT", StringComparison.OrdinalIgnoreCase))
            {
                return new CancelOutcome(true, false, "no carrier shipment to cancel");
            }

            // ERR_ORDER_CARRIER_NO_API_CANCEL / ERR_ORDER_CARRIER_REJECT - a real shipment survives.
            return new CancelOutcome(false, false, $"rejected: {raw}");
        }
    }

    private readonly record struct CancelOutcome(bool Cleared, bool Retryable, string Detail);

    private static string Truncate(string value) => value.Length <= 500 ? value : value[..500];

    public async Task<IReadOnlyList<FulfillmentStateView>> GetFulfillmentStatesAsync(
        string? state, int? stuckForMinutes, int limit, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OrderFulfillmentStates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(state))
        {
            query = query.Where(x => x.CurrentState == state);
        }

        if (stuckForMinutes is > 0)
        {
            var threshold = DateTimeOffset.UtcNow.AddMinutes(-stuckForMinutes.Value);
            query = query.Where(x => (x.UpdatedAt ?? x.CreatedAt) < threshold);
        }

        return await query
            .OrderBy(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(Math.Clamp(limit, 1, 500))
            .Select(x => new FulfillmentStateView
            {
                OrderId = x.CorrelationId,
                CurrentState = x.CurrentState,
                Gateway = x.Gateway,
                AhamoveOrderId = x.AhamoveOrderId,
                NhanhOrderId = x.NhanhOrderId,
                DeliveryDispatched = x.DeliveryDispatched,
                FailedStep = x.FailedStep,
                FailureReason = x.FailureReason,
                CompensationAttempts = x.CompensationAttempts,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<OrderEntity> LoadOrderAsync(Guid orderId, CancellationToken cancellationToken)
        => await _dbContext.Orders
               .Include(x => x.OrderDetails)
               .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
           ?? throw new FulfillmentPermanentException($"Order {orderId} not found");

    private async Task<string> BuildPrivateDescriptionAsync(OrderEntity order, string? companyRemarks)
    {
        var privateDescription = companyRemarks ?? "Đến nơi lấy hàng đọc mã đơn để nhận hàng";
        privateDescription += $"\n-------------------------\nNN: {order.PhoneReceiver}";

        if (order.OrderTime > 0)
        {
            var vnDt = DateTimeOffset.FromUnixTimeSeconds(order.OrderTime).ToOffset(TimeSpan.FromHours(7));
            privateDescription += $"\n-------------------------\nGiao: {vnDt:dd/MM/yyyy}";
        }

        if (order.IsExportInvoice != true || order.InvoiceType == null)
        {
            return privateDescription;
        }

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
                    // The lookup is decoration on the shop's picking note; never fail an order over it.
                    _logger.LogWarning("Failed to look up tax code {TaxCode}: {Message}", order.TaxCode, ex.Message);
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

        return privateDescription;
    }

    private async Task<string> ConvertTimeDeliveryAsync(OrderEntity order)
    {
        if (order.TagIds == null || order.TagIds.Count == 0)
        {
            return string.Empty;
        }

        var orderTagConfig = await _configService.GetNhanhVnOrderTag();

        var deliveryTime = orderTagConfig.Options?
            .FirstOrDefault(x => x.Value.HasValue && order.TagIds.Contains(x.Value.Value));

        if (deliveryTime == null)
        {
            throw new FulfillmentPermanentException("Không tồn tại thời gian giao hàng");
        }

        var convertDeliveryTime = Regex.Match(deliveryTime.Label ?? string.Empty, @"\d{1,2}");

        if (!convertDeliveryTime.Success
            || !int.TryParse(convertDeliveryTime.Value, out var time)
            || time is < 0 or > 23)
        {
            return string.Empty;
        }

        return $"{time:D2}:00:00";
    }

    // Instant orders carry no tag, so ConvertTimeDeliveryAsync always returns empty for them. The
    // delivery time NhanhVn shows is the moment Ahamove collects the parcel, rendered in VN time.
    private static string ConvertInstantTimeDelivery(long scheduledUnixSeconds)
        => scheduledUnixSeconds <= 0
            ? string.Empty
            : DateTimeOffset.FromUnixTimeSeconds(scheduledUnixSeconds)
                .ToOffset(TimeSpan.FromHours(7))
                .ToString("HH:mm:ss");

    private static string ConvertInstantDateDelivery(long scheduledUnixSeconds)
        => scheduledUnixSeconds <= 0
            ? string.Empty
            : DateTimeOffset.FromUnixTimeSeconds(scheduledUnixSeconds)
                .ToOffset(TimeSpan.FromHours(7))
                .ToString("yyyy-MM-dd");

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, string api, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FulfillmentTransientException($"{api} request timed out");
        }
        catch (HttpRequestException ex)
        {
            throw new FulfillmentTransientException($"{api} transport failure: {ex.Message}");
        }
    }

    // 5xx / 408 / 429 are worth retrying; a 4xx means the request itself was rejected and the same
    // request will be rejected again, so fault immediately and let compensation start sooner.
    private static AppException HttpFailure(string api, HttpStatusCode status, string body)
        => (int)status >= 500 || status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
            ? new FulfillmentTransientException($"{api} API HTTP Error ({status}): {body}")
            : new FulfillmentPermanentException($"{api} API HTTP Error ({status}): {body}");
}

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
using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;

namespace HoaTheoMua.Service.Fulfillment;

/// <summary>
/// Extracted verbatim from Webhook.FulfillPaidOrder, with three deliberate changes: each step now
/// persists its own result immediately, each step is idempotent, and outbound failures are split
/// into transient vs permanent so the retry policy can tell them apart.
///
/// The courier is chosen by Order.ShippingGateway: Ahamove is called over HTTP directly (as the
/// legacy code did), XanhSm through the existing XanhSm.IService client.
/// </summary>
public class Service : IService
{
    /// <summary>Terminal status for an order whose payment landed but whose fulfilment could not.</summary>
    public const string FulfillmentFailedStatus = "FulfillmentFailed";

    // NhanhVn tag ids. 253 marks a VAT-invoice order, 294 marks "giao ngay".
    private const long VatTagId = 253L;
    // private const long FastTagId = 294L;

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
    private readonly XanhSm.IService _xanhSmService;
    private readonly ILogger<Service> _logger;

    public Service(
        AppDbContext dbContext,
        HttpClient httpClient,
        IConfiguration configuration,
        HoaTheoMua.Service.Order.IService orderService,
        Config.IService configService,
        Revalidate.IService revalidateService,
        ILogger<Service> logger,
        XanhSm.IService xanhSmService)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _orderService = orderService;
        _configService = configService;
        _revalidateService = revalidateService;
        _xanhSmService = xanhSmService;
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

        if (order.ShippingGateway is not (ShippingGateway.Ahamove or ShippingGateway.XanhSm))
        {
            throw new FulfillmentPermanentException(
                $"Shipping gateway {order.ShippingGateway} is not supported");
        }

        /* Legacy auto-dispatch flow: keep this code for easy rollback if automatic courier booking returns.
        var isAhamove = order.ShippingGateway == ShippingGateway.Ahamove;

        var companyAddress = await _dbContext.CompanyAddresses
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        */

        // Legacy: privateDescription từng chứa ghi chú, thông tin người nhận,
        // lịch giao và hóa đơn. Chi tiết Order hiện được xem tại trang Admin.
        // var privateDescription = await BuildPrivateDescriptionAsync(order, companyAddress?.Remarks);
        var privateDescription = $"https://hoatheomua-dev-client.bentran1vn.com/htm/system-orders/{order.Id}?tab=snapshot";


        var tags = order.TagIds ?? new List<long>();
        var tagIds = (order.IsExportInvoice == true && order.InvoiceType != null)
            ? tags.Append(VatTagId).ToList()
            : tags;

        // Current flow: Admin books the courier manually. Keep this fulfilment step so the saga
        // can continue to create the Nhanh.vn order, but do not call Ahamove or XanhSM.
        return new DeliveryDispatchResult
        {
            Dispatched = false,
            ShippingGateway = order.ShippingGateway,
            ShippingPartyOrderId = null,
            ShippingTrackingUrl = null,
            DelayTimeUnix = 0,
            TagIds = tagIds,
            PrivateDescription = privateDescription
        };

        /* Legacy auto-dispatch flow: keep this code for easy rollback if automatic courier booking returns.
        var ahamoveConfigs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "ahamove");

        var isUrgent = await IsUrgentOrderAsync(order, tags);
        var allowInstantDelivery = !isAhamove || ahamoveConfigs.GetValue("ahamove.instant_delivery", true);

        // Unix seconds the courier will collect the parcel. Computed once here and carried in the
        // result: the POS step must show the same moment, and recomputing it there would drift.
        const int delayCallAhamove = 90 * 60;
        var delayTime = isAhamove
            ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() + delayCallAhamove
            : DateTimeOffset.UtcNow.ToUnixTimeSeconds() + XanhSm.XanhSmConstants.PreparationTimeSeconds;
        //var shouldDispatch = order.OrderTime == 0 && tags.Count == 0 && allowInstantDelivery;
        var shouldDispatch = isUrgent && allowInstantDelivery;
        if (shouldDispatch)
        {
            tagIds = tagIds.Append(FastTagId).ToList();
        }

        DeliveryDispatchResult Plan(bool dispatched, string? shippingPartyOrderId, string? trackingUrl) => new()
        {
            Dispatched = dispatched,
            ShippingGateway = order.ShippingGateway,
            ShippingPartyOrderId = shippingPartyOrderId,
            ShippingTrackingUrl = trackingUrl,
            DelayTimeUnix = delayTime,
            TagIds = tagIds,
            PrivateDescription = privateDescription
        };

        // Non-urgent orders and urgent delivery disabled in Config do not book a courier.
        if (!shouldDispatch)
        {
            return Plan(dispatched: false, null, null);
        }

        // Idempotency: a redelivered webhook or a retried message must not book a second courier.
        if (!string.IsNullOrWhiteSpace(order.ShippingPartyOrderId))
        {
            _logger.LogInformation(
                "Order {OrderId} already has {ShippingGateway} order {ShippingPartyOrderId}; skipping dispatch",
                orderId, order.ShippingGateway, order.ShippingPartyOrderId);
            return Plan(dispatched: true, order.ShippingPartyOrderId, order.ShippingTrackingUrl);
        }

        // The original dereferenced companyAddress unconditionally here and threw a bare NRE (a 500)
        // when no company address had been configured.
        if (companyAddress == null)
        {
            throw new FulfillmentPermanentException(
                "No CompanyAddress is configured, so the courier pickup point cannot be built");
        }

        var (shippingPartyOrderId, shippingTrackingUrl) = isAhamove
            ? await BookAhamoveAsync(order, companyAddress, ahamoveConfigs, delayTime, cancellationToken)
            : await BookXanhSmAsync(order, companyAddress, delayTime, cancellationToken);

        // Persisted before anything else can fail. In the original this stayed in the change tracker
        // until the single SaveChanges at the very end, so a later NhanhVn failure discarded the id
        // of a courier that had genuinely been booked - and the retry booked another one.
        order.ShippingPartyOrderId = shippingPartyOrderId;
        order.ShippingTrackingUrl = shippingTrackingUrl;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "{ShippingGateway} order {ShippingPartyOrderId} booked and persisted for order {OrderId}",
            order.ShippingGateway, shippingPartyOrderId, orderId);

        return Plan(dispatched: true, shippingPartyOrderId, shippingTrackingUrl);
        */
    }
    
    // private async Task<(string OrderId, string? TrackingUrl)> BookAhamoveAsync(
    //     OrderEntity order, CompanyAddressEntity companyAddress, GroupConfig ahamoveConfigs, long delayTime,
    //     CancellationToken cancellationToken)
    // {
    //     var createRequest = new AhaMoveRequest.AhaMoveShippingFeeRequest
    //     {
    //         OrderTime = delayTime,
    //         IdleUntil = delayTime,
    //         Paths =
    //         [
    //             new AhaMoveRequest.AhamoveLocationPoint
    //             {
    //                 Latitude = companyAddress.Latitude ?? 0,
    //                 Longitude = companyAddress.Longitude ?? 0,
    //                 Address = companyAddress.AddressLine + ", " + companyAddress.Ward + ", " + companyAddress.City,
    //                 ShortAddress = companyAddress.AddressLine + ", " + companyAddress.Ward + ", " + companyAddress.City,
    //                 FullName = companyAddress.FullName,
    //                 Phone = companyAddress.PhoneNumber,
    //                 Remarks = companyAddress.Remarks!,
    //             },
    //             new AhaMoveRequest.AhamoveLocationPoint
    //             {
    //                 Latitude = order.Latitude,
    //                 Longitude = order.Longitude,
    //                 Address = order.Address,
    //                 ShortAddress = order.ShortAddress,
    //                 FullName = order.FullName,
    //                 Phone = order.Phone,
    //                 Remarks = order.Remarks,
    //             },
    //         ],
    //         ServiceId = "SGN-BIKE",
    //         Requests = [new AhaMoveRequest.ItemsRequest { Id = "SGN-BIKE-BAGA" }],
    //         PaymentMethod = "CASH",
    //         Remarks = order.Remarks,
    //         Items = order.OrderDetails.Select(x => new AhaMoveRequest.AhamoveItem
    //         {
    //             Id = x.Id.ToString(),
    //             Name = x.Name,
    //             Price = (int)x.Price,
    //             // Ahamove only supports whole items; checkout and NhanhVN retain
    //             // the original decimal quantity.
    //             //Num = x.Number
    //             Num = checked((int)Math.Ceiling(x.Number))
    //         }).ToList(),
    //         PackageDetail = order.OrderDetails.Select(x => new AhaMoveRequest.PackageDetail
    //         {
    //             Height = x.Height,
    //             Weight = x.Weight,
    //             Length = x.Length,
    //             Width = x.Width,
    //             Description = x.Name
    //         }).ToList(),
    //     };
    
    /// <summary>POST /v3/orders - the legacy inline request, moved here unchanged.</summary>
    private async Task<(string OrderId, string? TrackingUrl)> BookAhamoveAsync(
        OrderEntity order, CompanyAddressEntity companyAddress, GroupConfig ahamoveConfigs, long delayTime,
        CancellationToken cancellationToken)
    {
        // Giữ cùng quy tắc với lúc báo giá: gộp số lượng theo Nhanh ItemId rồi
        // mới làm tròn cho Ahamove. Nhờ vậy 0.2 + 0.2 thành 1 kiện ở cả hai bước,
        // thay vì bị làm tròn thành 2 vì chúng nằm ở hai OrderDetails.
        var ahamoveItems = order.OrderDetails
            .GroupBy(detail => detail.ItemId)
            .Select(group =>
            {
                var detail = group.First();
                return new AhaMoveRequest.AhamoveItem
                {
                    Id = detail.ItemId.ToString(),
                    Name = detail.Name,
                    Price = (int)detail.Price,
                    Num = checked((int)Math.Ceiling(group.Sum(item => item.Number)))
                };
            })
            .ToList();
        //edge case: một item 2 mẫu hoa AI trong một order -> 2 orderDetails -> đều tăng 0.2 đơn vị
        //nếu không group -> 0.2 -> 1 | 0.2 -> 1 => 2 || nếu group -> 0.2*2 = 0.4 -> 1
        
        
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
            Items = ahamoveItems,
            // Items = order.OrderDetails.Select(x => new AhaMoveRequest.AhamoveItem
            // {
            //     Id = x.Id.ToString(),
            //     Name = x.Name,
            //     Price = (int)x.Price,
            //     // Ahamove only supports whole items; checkout and NhanhVN retain
            //     // the original decimal quantity.
            //     //Num = x.Number
            //     Num = checked((int)Math.Ceiling(x.Number))
            // }).ToList(),
            // PackageDetail = order.OrderDetails.Select(x => new AhaMoveRequest.PackageDetail
            // {
            //     Height = x.Height,
            //     Weight = x.Weight,
            //     Length = x.Length,
            //     Width = x.Width,
            //     Description = x.Name
            // }).ToList(),
            PackageDetail = order.OrderDetails
                .GroupBy(detail => detail.ItemId)
                .Select(group =>
                {
                    var detail = group.First();
                    return new AhaMoveRequest.PackageDetail
                    {
                        Height = detail.Height,
                        Weight = detail.Weight,
                        Length = detail.Length,
                        Width = detail.Width,
                        Description = detail.Name
                    };
                })
                .ToList(),
            
            // Items = order.OrderDetails.Select(x => new AhaMoveRequest.AhamoveItem
            // {
            //     Id = x.Id.ToString(),
            //     Name = x.Name,
            //     Price = (int)x.Price,
            //     // Ahamove only supports whole items; checkout and NhanhVN retain
            //     // the original decimal quantity.
            //     //Num = x.Number
            //     Num = checked((int)Math.Ceiling(x.Number))
            // }).ToList(),
            // PackageDetail = order.OrderDetails.Select(x => new AhaMoveRequest.PackageDetail
            // {
            //     Height = x.Height,
            //     Weight = x.Weight,
            //     Length = x.Length,
            //     Width = x.Width,
            //     Description = x.Name
            // }).ToList(),
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

        return (createAhamoveOrderResult.OrderId, createAhamoveOrderResult.SharedLink);
    }

    /// <summary>
    /// XanhSm door-to-door booking, ported from the legacy inline implementation. Reuses the point
    /// builder and the weight rule from Order.Service so what the saga books is exactly what the
    /// checkout estimate quoted.
    /// </summary>
    private async Task<(string OrderId, string? TrackingUrl)> BookXanhSmAsync(
        OrderEntity order, CompanyAddressEntity companyAddress, long delayTime, CancellationToken cancellationToken)
    {
        XanhSm.XanhSmDelivery delivery;
        try
        {
            delivery = await _xanhSmService.BookDoorToDoorAsync(
                HoaTheoMua.Service.Order.Service.CreateXanhSmPoints(
                    companyAddress,
                    order.Latitude,
                    order.Longitude,
                    order.Address,
                    order.ShortAddress,
                    order.FullNameReceiver,
                    order.PhoneReceiver,
                    order.Remarks,
                    HoaTheoMua.Service.Order.Service.CalculateXanhSmPackageWeightKg(
                        order.OrderDetails.Select(detail => (detail.Weight, detail.Number)))),
                delayTime,
                (long)Math.Round(order.ProductFee, MidpointRounding.AwayFromZero),
                order.Id.ToString("N"),
                cancellationToken);
        }
        catch (AppException ex) when (ex is not FulfillmentTransientException and not FulfillmentPermanentException)
        {
            throw MapXanhSmFailure("XanhSM booking", ex);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FulfillmentTransientException("XanhSM booking request timed out");
        }
        catch (HttpRequestException ex)
        {
            throw new FulfillmentTransientException($"XanhSM booking transport failure: {ex.Message}");
        }

        var deliveryId = delivery.Id ?? delivery.OrderId;
        if (string.IsNullOrWhiteSpace(deliveryId))
        {
            // A booking may well exist on XanhSm's side, but without its id it can be neither
            // deduplicated nor cancelled - and retrying would only book another one.
            throw new FulfillmentPermanentException("XanhSM returned a delivery without an id");
        }

        return (deliveryId, delivery.TrackingUrl ?? delivery.SharedLink);
    }

    // XanhSm.Service already normalises provider errors into AppExceptions (see its
    // ThrowProviderException); this only decides which of them are worth a retry. Without it a
    // BadRequestException would slip past the endpoint's Ignore<FulfillmentPermanentException>
    // and be retried five times for a business rejection that cannot change.
    private static AppException MapXanhSmFailure(string api, AppException ex) => ex switch
    {
        BadGatewayException or TooManyRequestException
            => new FulfillmentTransientException($"{api} failed: {ex.Message}"),
        _ => new FulfillmentPermanentException($"{api} rejected: {ex.Message}")
    };

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

        
        // var productNhanhVnRequest = order.OrderDetails.Select(x => new NhanhVnProduct
        // {
        //     Id = x.ItemId,
        //     Price = x.Price,
        //     Quantity = x.Number,
        //     Vat = isVatOrder ? 8 : null,
        // }).ToList();
        // NhanhVN chỉ cần tổng số lượng cần xuất kho theo ItemId; các OrderDetails
        // vẫn giữ riêng để snapshot từng Flower/Card không bị lẫn.
        var productNhanhVnRequest = order.OrderDetails
            .GroupBy(x => x.ItemId)
            .Select(group =>
            {
                var detail = group.First();
                return new NhanhVnProduct
                {
                    Id = detail.ItemId,
                    Price = detail.Price,
                    Quantity = group.Sum(item => item.Number),
                    Vat = isVatOrder ? 8 : null,
                };
            })
            .ToList();

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

        var voucherSnapshot = DeserializeVoucherSnapshot(order.VoucherSnapshot);

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
                //Status = (order.OrderTime == 0 && tags.Count == 0) ? 56 : 54,
                //Anh Bình confirm với chị Đào là đơn hàng luôn đã xác nhận hết vào ngày 30/9/26 lúc 16h10p
                Status = 56,
                TagIds = plan.TagIds,
                TechnicalStaffId = 0,
                PromotionId = ""
            },
            Channel = new Channel
            {
                TrafficSourceId = NhanhTrafficSourceId,
                AppOrderId = plan.ShippingPartyOrderId != null
                    ? plan.ShippingPartyOrderId + " - " + prefixOrderId
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
                // 12 is Ahamove's carrier id at NhanhVn. XanhSm is not a NhanhVn-integrated carrier, so
                // the order is flagged as self-connected (we book and track the delivery ourselves).
                Id = plan.ShippingGateway == ShippingGateway.Ahamove ? 12 : 0,
                AllowTest = 1,
                ServiceId = 0,
                ServiceCode = "",
                CheckGoods = 0,
                IsPartDelivery = 0,
                IsDeclaredFee = 0,
                IsFeeCustomerReject = 0,
                FeeCustomerReject = 0m,
                //orderTime > 0 là ngày mai cho tới ngày thứ 7
                //orderTime = 0 là ngày hôm nay
                DeliveryDate = order.OrderTime > 0
                    ? vnDt.ToString("yyyy-MM-dd")
                    : order.CreatedAt.ToOffset(TimeSpan.FromHours(7))
                            .ToString("yyyy-MM-dd"),
                // OrderTime > 0: chuyển OrderTime sang ngày yyyy-MM-dd.
                //OrderTime = 0:  thời gian hiện tại cộng delay => có thể qua ngày mới 
                DeliveryHour = "",
                DeliveryMinute = "",
                DeliveryTime = tags.Count > 0
                    ? await ConvertTimeDeliveryAsync(order)
                    : ConvertInstantTimeDelivery(plan.DelayTimeUnix),
                // Có tag: tìm label bằng TagId, ồi lấy giờ bắt đầu là 08:00:00.
                // Không có tag: lấy giờ từ thời gian hiện tại cộng delay.
                CarrierAccountId = 0,
                CarrierShopId = "",
                DeclaredValue = null,
                DeclaredFee = 0m,
                ShipFee = 0m,
                CodFee = 0m,
                IsSelfConnect = plan.ShippingGateway == ShippingGateway.XanhSm ? 1 : 0,
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
                // A voucher is a fixed amount for the whole order ("cash"); orders without one keep "percent"
                DiscountType = voucherSnapshot != null ? "cash" : "percent",
                DiscountAmount = voucherSnapshot == null
                    ? null
                    : (int)Math.Round(voucherSnapshot.DiscountAmount, MidpointRounding.AwayFromZero),
                CouponCode = voucherSnapshot?.Code ?? "",
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
        Guid orderId, ShippingGateway? shippingGateway, string? shippingPartyOrderId, int? nhanhOrderId,
        string reason, CancellationToken cancellationToken = default)
    {
        var details = new List<string>();
        var retryable = false;

        var deliveryCleared = true;
        if (!string.IsNullOrWhiteSpace(shippingPartyOrderId))
        {
            // Null gateway only on rows written before ShippingGateway existed, all of them Ahamove.
            var courier = shippingGateway ?? ShippingGateway.Ahamove;
            var outcome = courier == ShippingGateway.XanhSm
                ? await CancelXanhSmDeliveryAsync(shippingPartyOrderId, reason, cancellationToken)
                : await CancelAhamoveOrderAsync(shippingPartyOrderId, reason, cancellationToken);
            deliveryCleared = outcome.Cleared;
            retryable |= outcome.Retryable;
            details.Add($"{courier.ToString().ToLowerInvariant()}({shippingPartyOrderId})={outcome.Detail}");
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
            + "shippingGateway={ShippingGateway} shippingPartyOrderId={ShippingPartyOrderId} nhanhOrderId={NhanhOrderId}",
            orderId, FulfillmentFailedStatus, step, reason, order.ShippingGateway, order.ShippingPartyOrderId,
            order.NhanhOrderId);
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

    /// <summary>
    /// POST cancel-order through XanhSm.Service. cancel_code stays at the request type's default
    /// (9000), the same code the team's own cancel path sends.
    /// </summary>
    /// <remarks>
    /// XanhSm's error catalogue for "already cancelled" vs "already delivered" has not been confirmed
    /// against the partner docs, so a 400 is surfaced as "not cancellable" for a human to look at
    /// rather than guessed at. A 404 is treated as already gone, the same as Ahamove.
    /// </remarks>
    private async Task<CancelOutcome> CancelXanhSmDeliveryAsync(
        string deliveryId, string reason, CancellationToken cancellationToken)
    {
        try
        {
            await _xanhSmService.CancelDeliveryAsync(new XanhSm.XanhSmCancelDeliveryRequest
            {
                OrderId = deliveryId,
                CancelMessage = Truncate(reason)
            }, cancellationToken);

            _logger.LogInformation("XanhSM delivery {DeliveryId} cancelled", deliveryId);
            return new CancelOutcome(true, false, "cancelled");
        }
        catch (NotFoundException)
        {
            return new CancelOutcome(true, false, "not found (already gone)");
        }
        // Surfaced as retryable rather than thrown, so the NhanhVn leg still gets attempted.
        catch (BadGatewayException ex)
        {
            return new CancelOutcome(false, true, ex.Message);
        }
        catch (TooManyRequestException ex)
        {
            return new CancelOutcome(false, true, ex.Message);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new CancelOutcome(false, true, "XanhSM cancel request timed out");
        }
        catch (HttpRequestException ex)
        {
            return new CancelOutcome(false, true, $"XanhSM cancel transport failure: {ex.Message}");
        }
        catch (AppException ex)
        {
            return new CancelOutcome(false, false, $"not cancellable: {ex.Message}");
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
                ShippingGateway = x.ShippingGateway,
                ShippingPartyOrderId = x.ShippingPartyOrderId,
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

    #if false
    // Legacy privateDescription builder. Giữ lại để đối chiếu dữ liệu Nhanh.vn cũ.
    private async Task<string> BuildPrivateDescriptionAsync(OrderEntity order, string? companyRemarks)
    {
        var privateDescription = companyRemarks ?? "Đến nơi lấy hàng đọc mã đơn để nhận hàng";
        privateDescription += $"\n-------------------------\nNN: {order.FullNameReceiver} - {order.PhoneReceiver}";

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
    #endif

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

    private async Task<bool> IsUrgentOrderAsync(OrderEntity order, IReadOnlyCollection<long> tagIds)
    {
        if (order.OrderTime != 0)
        {
            return false;
        }

        var orderTagConfig = await _configService.GetNhanhVnOrderTag();
        var selectedSlot = orderTagConfig.Options?
            .FirstOrDefault(option =>
                option.Value.HasValue && tagIds.Contains(option.Value.Value));

        return DeliverySlotTimeHelper.IsWithinSlot(selectedSlot?.Label, order.CreatedAt);
    }

    private static HoaTheoMua.Service.Order.Request.VoucherSnapshot? DeserializeVoucherSnapshot(string? snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot))
            return null;

        try
        {
            return JsonSerializer.Deserialize<HoaTheoMua.Service.Order.Request.VoucherSnapshot>(snapshot);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Instant orders carry no tag, so ConvertTimeDeliveryAsync always returns empty for them. The
    // delivery time NhanhVn shows is the moment the courier collects the parcel, rendered in VN time.
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

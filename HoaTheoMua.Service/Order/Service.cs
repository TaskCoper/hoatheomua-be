
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.BaoKim;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Webhook;
using HoaTheoMua.Service.XanhSm;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Order;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContext;
    private readonly HttpClient _httpClient;
    private readonly HoaTheoMua.Service.Product.IService _serviceProduct;
    private readonly HoaTheoMua.Service.BaoKim.IService _baoKimService;
    private readonly HoaTheoMua.Service.XanhSm.IService? _xanhSmService;
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly AhaMoveOption _ahaMoveOption;

    public Service(AppDbContext dbContext,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContext,
        HttpClient httpClient,
        HoaTheoMua.Service.Product.IService serviceProduct,
        IConfiguration configuration,
        BaoKim.IService baoKimService,
        HoaTheoMua.Service.XanhSm.IService? xanhSmService = null)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _httpContext = httpContext;
        _httpClient = httpClient;
        _serviceProduct = serviceProduct;
        _baoKimService = baoKimService;
        _xanhSmService = xanhSmService;
        _nhanhVnOption = new NhanhVnOption();
        _ahaMoveOption = new AhaMoveOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        configuration.GetSection(nameof(AhaMoveOption)).Bind(_ahaMoveOption);
    }


    public async Task<BasePaginationResponse> GetOrderPaginationResponse(
        int pageIndex,
        int pageSize,
        string? status = null)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var userId = _currentUser.GetRequiredUserId();

        var ordersQuery = _dbContext.Orders
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLower();
            ordersQuery = ordersQuery.Where(x => x.Status.ToLower() == normalizedStatus);
        }

        var totalCount = await ordersQuery.CountAsync();

        var orders = await ordersQuery
            .Include(x => x.OrderDetails.Where(detail => !detail.IsDeleted))
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var nhanhProductIds = orders
            .SelectMany(x => x.OrderDetails)
            .Where(x => !x.IsDeleted)
            .Select(x => x.ItemId.ToString())
            .Distinct()
            .ToList();

        var productRows = new List<Response.OrderProductLookup>();
        if (nhanhProductIds.Count > 0)
        {
            productRows = await _dbContext.Products
                .AsNoTracking()
                .Where(x => x.NhanhProductId != null
                            && nhanhProductIds.Contains(x.NhanhProductId))
                .Select(x => new Response.OrderProductLookup()
                {
                    Id = x.Id,
                    NhanhProductId = x.NhanhProductId,
                    ProductName = x.ProductName,
                    CoverImage = x.CoverImage,
                    ParentNhanhProductId = x.ParentProductId
                })
                .ToListAsync();
        }

        var productsByNhanhId = productRows
            .Where(x => !string.IsNullOrWhiteSpace(x.NhanhProductId))
            .ToDictionary(x => x.NhanhProductId!, x => x);

        var parentNhanhProductIds = productRows
            .Where(x => !string.IsNullOrWhiteSpace(x.ParentNhanhProductId))
            .Select(x => x.ParentNhanhProductId!)
            .Distinct()
            .ToList();

        var parentRows = new List<Response.OrderProductLookup>();
        if (parentNhanhProductIds.Count > 0)
        {
            parentRows = await _dbContext.Products
                .AsNoTracking()
                .Where(x => x.NhanhProductId != null
                            && parentNhanhProductIds.Contains(x.NhanhProductId))
                .Select(x => new Response.OrderProductLookup()
                {
                    Id = x.Id,
                    NhanhProductId = x.NhanhProductId,
                    ProductName = x.ProductName,
                    CoverImage = x.CoverImage
                })
                .ToListAsync();
        }

        var parentsByNhanhId = parentRows
            .Where(x => !string.IsNullOrWhiteSpace(x.NhanhProductId))
            .ToDictionary(x => x.NhanhProductId!, x => x);

        var result = orders.Select(x => new Response.GetOrderResponse
        {
            OrderId = x.Id,
            Status = x.Status,
            TrackNumber = x.TrackNumber,
            Address = x.Address,
            // NhanhOrderId = x.NhanhOrderId!,
            // NhanhTrackingUrl = x.NhanhTrackingUrl!,
            ShippingPartyOrderId = x.ShippingPartyOrderId!,
            ShippingTrackingUrl = x.ShippingTrackingUrl!,
            Remark = x.Remarks,
            OrderTime = x.OrderTime,
            Taxcode = x.TaxCode,
            TagsId = x.TagIds,
            IdentityNumber = x.IdentityNumber!,
            InvoiceType = x.InvoiceType!,
            PaymentGateway = x.PaymentGateway,
            ShippingGateway = x.ShippingGateway,
            IsExportInvoice = x.IsExportInvoice,
            ReceiptAddress = x.ReceiptAddress!,
            ReceiptEmail = x.ReceiptEmail!,
            ReceiptFullName = x.ReceiptFullName!,
            ReceiptPhoneNumber = x.ReceiptPhoneNumber!,
            TotalFee = x.TotalFee,
            Items = x.OrderDetails
                .Where(detail => !detail.IsDeleted)
                .OrderBy(detail => detail.CreatedAt)
                .Select(detail =>
                {
                    productsByNhanhId.TryGetValue(detail.ItemId.ToString(), out var product);

                    Response.ProductSummaryResponse? parent = null;
                    if (!string.IsNullOrWhiteSpace(product?.ParentNhanhProductId)
                        && parentsByNhanhId.TryGetValue(product.ParentNhanhProductId, out var parentProduct))
                    {
                        parent = new Response.ProductSummaryResponse
                        {
                            ProductId = parentProduct.Id,
                            Name = parentProduct.ProductName ?? string.Empty,
                            CoverImage = parentProduct.CoverImage
                        };
                    }

                    return new Response.OrderItemSummaryResponse
                    {
                        ProductId = product?.Id ?? detail.ProductId,
                        Name = detail.Name,
                        CoverImage = product?.CoverImage,
                        Quantity = detail.Number,
                        Price = detail.Price,
                        // LineTotal = detail.Price * detail.Number,
                        Parent = parent
                    };
                })
                .ToList(),
            CreateAt = x.CreatedAt,
            UpdateAt = x.UpdatedAt,
        }).ToList();

        return ApiResponseFactory.BasePagination(result, pageIndex, pageSize, totalCount);
    }

    public async Task<Dictionary<long, int>> CalculateSlotOccupied(DateOnly date, IReadOnlyCollection<long> tagIds,
        CancellationToken cancellationToken = default)
    {
        var occupiedByTagId = tagIds
            .Distinct()
            .ToDictionary(tagId => tagId, _ => 0);

        if (occupiedByTagId.Count == 0)
        {
            return occupiedByTagId;
        }

        var vietnamOffset = TimeSpan.FromHours(7);
        var startUtc = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), vietnamOffset)
            .ToUniversalTime();
        var endUtc = startUtc.AddDays(1);
        var startUnix = startUtc.ToUnixTimeSeconds();
        var endUnix = endUtc.ToUnixTimeSeconds();

        var nhanhOrderTagIds = await _dbContext.OrderNhanhVns
            .AsNoTracking()
            .Where(order =>
                !order.IsDeleted &&
                order.DeliveryDate == date &&
                order.TrangThaiDonHang != "63" &&
                order.TrangThaiDonHang != "Khách hủy")
            .Select(order => order.TagIds)
            .ToListAsync(cancellationToken);

        var pendingOrderTagIds = await _dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                !order.IsDeleted &&
                order.Status == "Pending" &&
                ((order.OrderTime == 0 &&
                  order.CreatedAt >= startUtc &&
                  order.CreatedAt < endUtc) ||
                 (order.OrderTime > 0 &&
                  order.OrderTime >= startUnix &&
                  order.OrderTime < endUnix)))
            .Select(order => order.TagIds)
            .ToListAsync(cancellationToken);

        CountOccupied(nhanhOrderTagIds, occupiedByTagId);
        CountOccupied(pendingOrderTagIds, occupiedByTagId);

        return occupiedByTagId;
    }

    private static void CountOccupied(IEnumerable<List<long>?> orderTagIds, IDictionary<long, int> occupiedByTagId)
    {
        foreach (var tagIds in orderTagIds)
        {
            if (tagIds == null)
            {
                continue;
            }

            foreach (var tagId in tagIds.Distinct())
            {
                if (occupiedByTagId.TryGetValue(tagId, out var occupied))
                {
                    occupiedByTagId[tagId] = occupied + 1;
                }
            }
        }
    }

    public async Task<Response.DeliverySlotAvailabilityResponse> GetDeliverySlotAvailability(DateOnly date, int? tagId,
        CancellationToken cancellationToken = default)
    {
        if (date == default)
        {
            throw new BadRequestException("Ngày kiểm tra không hợp lệ");
        }

        var entity = await _dbContext.Configs
            .AsNoTracking()
            .FirstOrDefaultAsync(config =>
                    !config.IsDeleted &&
                    config.Key == "nhanhvn.order_tags" &&
                    config.Group == "nhanhvn" &&
                    config.Kind == "setting",
                cancellationToken);

        if (entity == null)
        {
            throw new BadRequestException("Không tìm thấy cấu hình của nhãn");
        }

        HoaTheoMua.Service.Config.Response.ConfigItem? config;
        try
        {
            config = System.Text.Json.JsonSerializer.Deserialize<
                HoaTheoMua.Service.Config.Response.ConfigItem>(entity.Value);
        }
        catch (System.Text.Json.JsonException)
        {
            throw new BadRequestException("Không thể convert json");
        }

        if (config?.Value == null || config.Options == null)
        {
            throw new BadRequestException("Không thể convert json");
        }

        var configuredTagIds = config.Value.Distinct().ToList();
        if (tagId.HasValue && !configuredTagIds.Contains(tagId.Value))
        {
            throw new BadRequestException($"TagId {tagId.Value} không phải là tag giao hàng theo khung giờ");
        }

        var selectedTagIds = tagId.HasValue
            ? new List<int> { tagId.Value }
            : configuredTagIds;

        var optionsByTagId = config.Options
            .Where(option => option.Value.HasValue)
            .GroupBy(option => option.Value!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        var missingOptionTagIds = selectedTagIds
            .Where(id => !optionsByTagId.ContainsKey(id))
            .ToList();
        if (missingOptionTagIds.Count > 0)
        {
            throw new BadRequestException($"TagId {missingOptionTagIds[0]} không tồn tại trong mục options");
        }

        var occupiedByTagId = await CalculateSlotOccupied(date, selectedTagIds.Select(id => (long)id).ToList(),
            cancellationToken);

        return new Response.DeliverySlotAvailabilityResponse
        {
            Date = date,
            Slots = selectedTagIds.Select(id => new Response.DeliverySlotAvailabilityItem
            {
                TagId = id,
                Label = optionsByTagId[id].Label ?? string.Empty,
                Limit = config.SlotLimits?.TryGetValue(id, out var limit) == true ? limit : null,
                Occupied = occupiedByTagId[id]
            }).ToList()
        };
    }
    private static void EnsureDeliverySlotAvailable(Response.DeliverySlotAvailabilityItem slot)
    {
        if (slot.Limit.HasValue && slot.Occupied >= slot.Limit.Value)
        {
            throw new BadRequestException($"Khung giờ {slot.Label} đã đạt giới hạn {slot.Limit.Value} đơn hàng");
        }
    }

    private static void EnsureUrgentOrderMatchesCurrentSlot(
        long orderTime,
        Response.DeliverySlotAvailabilityItem slot,
        DateTimeOffset createdAt)
    {
        if (orderTime != 0)
        {
            return;
        }

        var hourMatches = Regex.Matches(
            slot.Label,
            @"(?<!\d)(?<hour>[01]?\d|2[0-3])\s*h",
            RegexOptions.IgnoreCase);

        if (hourMatches.Count < 2 ||
            !int.TryParse(hourMatches[0].Groups["hour"].Value, out var startHour) ||
            !int.TryParse(hourMatches[1].Groups["hour"].Value, out var endHour) ||
            startHour == endHour)
        {
            throw new BadRequestException($"Không thể xác định khung giờ từ tag {slot.Label}");
        }

        var currentVietnamTime = TimeOnly.FromDateTime(
            createdAt.ToOffset(TimeSpan.FromHours(7)).DateTime);
        var startTime = new TimeOnly(startHour, 0);
        var endTime = new TimeOnly(endHour, 0);
        var isInSlot = startTime < endTime
            ? currentVietnamTime >= startTime && currentVietnamTime < endTime
            : currentVietnamTime >= startTime || currentVietnamTime < endTime;

        if (!isInSlot)
        {
            throw new BadRequestException(
                $"Đơn giao gấp chỉ được tạo khi thời gian hiện tại nằm trong khung giờ {slot.Label}");
        }
    }

    private async Task SavePendingOrderWithSlotValidation(
        Repository.Entity.Order order,
        IReadOnlyCollection<OrderDetails> orderDetails,
        DateOnly slotDate,
        int slotTagId,
        CancellationToken cancellationToken)
    {
        if (_dbContext.Database.ProviderName != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            // var availability = await GetDeliverySlotAvailability(
            //     slotDate,
            //     slotTagId,
            //     cancellationToken);
            // EnsureDeliverySlotAvailable(availability.Slots.Single());
            //
            await _dbContext.Orders.AddAsync(order, cancellationToken);
            await _dbContext.OrderDetails.AddRangeAsync(orderDetails, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({slotDate.DayNumber}, {slotTagId})",
            cancellationToken);

        var lockedAvailability = await GetDeliverySlotAvailability(
            slotDate,
            slotTagId,
            cancellationToken);
        EnsureDeliverySlotAvailable(lockedAvailability.Slots.Single());

        await _dbContext.Orders.AddAsync(order, cancellationToken);
        await _dbContext.OrderDetails.AddRangeAsync(orderDetails, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    // VAT applied to the product subtotal when the buyer requests an invoice.
    private const decimal VatRate = 0.08m;

    public async Task<CreateOrderResponse> CreateOrder(Request.CreateOrderRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();

        // Email-verified sessions and scoped phone-storefront sessions can place orders.
        if (!(_currentUser.IsVerified || _currentUser.CanUseStorefront))
        {
            throw new UnauthorizedException("User is not verified");
        }

        if (request.OrderTime < 0)
        {
            throw new BadRequestException("Thời gian giao hàng không hợp lệ");
        }

        var cancellationToken = _httpContext.HttpContext?.RequestAborted ?? CancellationToken.None;
        var createdAt = DateTimeOffset.UtcNow;
        
        // OrderTime = 0 means urgent delivery. The selected delivery-slot tag is still
        // retained for capacity counting, so the slot belongs to the current Vietnam date.
        //OrderTime = 0: đổi sang múi giờ Việt Nam UTC+7 rồi chỉ giữ phần ngày.
        // OrderTime > 0:  chuyển Unix timestamp OrderTime thành giờ Việt Nam rồi chỉ giữ phần ngày.
        var slotDate = request.OrderTime == 0
            ? DateOnly.FromDateTime(createdAt
                .ToOffset(TimeSpan.FromHours(7)).DateTime)
            : DateOnly.FromDateTime(DateTimeOffset
                .FromUnixTimeSeconds(request.OrderTime)
                .ToOffset(TimeSpan.FromHours(7)).DateTime);
        
        var slotTagId = checked((int)request.TagIds.Single());
        
        var availability = await GetDeliverySlotAvailability(
            slotDate,
            slotTagId,
            cancellationToken);
        var selectedSlot = availability.Slots.Single();
        EnsureUrgentOrderMatchesCurrentSlot(request.OrderTime, selectedSlot, createdAt);
        EnsureDeliverySlotAvailable(selectedSlot);

        var checkoutConfigs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "checkout");

        var deliveryMethodsConfig = checkoutConfigs.GetValue<string>("checkout.delivery_methods");
        if (!string.IsNullOrWhiteSpace(deliveryMethodsConfig))
        {
            var deliveryMethods = JObject.Parse(deliveryMethodsConfig);
            var selectedDeliveryMethod = deliveryMethods["items"]?
                .FirstOrDefault(x => (int?)x["gateway"] == (int)request.ShippingGateway);

            if ((bool?)selectedDeliveryMethod?["enabled"] == false)
            {
                throw new BadRequestException(
                    $"Phương thức giao hàng {request.ShippingGateway} hiện đang bị tắt");
            }
        }

        var paymentMethodsConfig = checkoutConfigs.GetValue<string>("checkout.payment_methods");
        if (!string.IsNullOrWhiteSpace(paymentMethodsConfig))
        {
            var paymentMethods = JObject.Parse(paymentMethodsConfig);
            var selectedPaymentMethod = paymentMethods["items"]?
                .FirstOrDefault(x => (int?)x["gateway"] == (int)request.PaymentGateway);

            if ((bool?)selectedPaymentMethod?["enabled"] == false)
            {
                throw new BadRequestException(
                    $"Phương thức thanh toán {request.PaymentGateway} hiện đang bị tắt");
            }
        }

        var preparedOrder = await PrepareOrderItems(request, userId);

        // Lấy Product sau Prepare để bao gồm Combo gốc, phần tăng, giấy và ribbon.
        var products = await FetchLocalProducts(preparedOrder.PurchaseItems);
        var quantityByItemId = preparedOrder.PurchaseItems
            .GroupBy(x => x.ItemId)//group rồi nên bên ahamove cũng group luôn -> tính phí và tạo đơn đồng giá
            .ToDictionary(x => x.Key, x => x.Sum(item => item.Number));
        var productByItemId = products.ToDictionary(x => x.Id);

        await EnsureItemsInStock(preparedOrder.StockRequirements);
        var metadataByLineId = await BuildOrderDetailsMetadata(preparedOrder, userId);

        var companyAddress = await _dbContext.CompanyAddresses
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync();

        // var addressExist = await _dbContext.Addresses
        //     .AnyAsync(x => 
        //         x.Id == request.AddressId &&
        //         !x.IsDeleted && x.UserId == userId);
        //
        // if (!addressExist)
        // {
        //     throw new BadRequestException("Địa chỉ người nhận không tồn tại");
        // }

        // var phoneExist = await _dbContext.Addresses
        //     .AnyAsync(x => 
        //         x.PhoneNumber == request.PhoneReceiver &&
        //         !x.IsDeleted && x.UserId == userId);
        //
        // if (!phoneExist)
        // {
        //     throw new BadRequestException("SĐT người nhận không tồn tại");
        // } //ko cần kiểm tra SDT

        var totalShippingFee = await EstimateShippingFeeAsync(request, products, quantityByItemId, companyAddress);

        var totalProductFee = preparedOrder.PurchaseItems.Sum(item =>
                                  productByItemId[item.ItemId].Prices.Retail * item.Number) +
                              metadataByLineId.Values
                                  .SelectMany(x => x.ProductMetadata)
                                  .Where(x => x.SnapShotCard != null)
                                  .Sum(x => x.SnapShotCard!.TotalPrice);
        var vatFee = string.IsNullOrEmpty(request.TaxCode) ? 0m : totalProductFee * VatRate;

        var newOrder = new Repository.Entity.Order()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = "Pending",
            TrackNumber = "",
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = request.Address,
            Ward = request.Ward,
            City = request.City,
            ShortAddress = request.ShortAddress,
            FullName = request.FullName,
            Phone = request.Phone,
            Remarks = request.Remarks,
            OrderTime = request.OrderTime,
            CreatedAt = createdAt,
            ShippingFee = totalShippingFee,
            ProductFee = totalProductFee,
            TotalFee = totalProductFee + totalShippingFee + vatFee,
            Vat = vatFee,
            PaymentMethod = request.PaymentMethod,
            PaymentGateway = request.PaymentGateway,
            ShippingGateway = request.ShippingGateway,
            PhoneReceiver = request.PhoneReceiver,
            FullNameReceiver = request.FullNameReceiver, //số điện thoại người nhận

            TagIds = request.TagIds,
            TaxCode = request.TaxCode,
            IdentityNumber = request.IdentityNumber,
            ReceiptFullName = request.ReceiptFullName,
            ReceiptPhoneNumber = request.ReceiptPhoneNumber,
            ReceiptAddress = request.ReceiptAddress,
            ReceiptEmail = request.ReceiptEmail,
            IsExportInvoice = request.IsExportInvoice,
            InvoiceType = request.InvoiceType,
        };

        var metadataByOrderDetailId = new Dictionary<Guid, Request.OrderDetailMetadata>();
        var newOrderDetails = preparedOrder.PurchaseItems.Select(item =>
        {
            var product = productByItemId[item.ItemId];
            metadataByLineId.TryGetValue(item.LineId, out var metadata);
            var orderDetail = new OrderDetails
            {
                Id = Guid.NewGuid(),
                OrderId = newOrder.Id,
                ProductId = item.ProductId,
                Name = product.Name,
                ItemId = item.ItemId,
                Price = product.Prices.Retail,
                Number = item.Number,
                Height = product.Shipping.Height,
                Weight = product.Shipping.Weight,
                Length = product.Shipping.Length,
                Width = product.Shipping.Width,
                CreatedAt = DateTime.UtcNow,
            };

            if (metadata != null)
                metadataByOrderDetailId[orderDetail.Id] = metadata;

            return orderDetail;
        }).ToList();
        
        foreach (var orderDetail in newOrderDetails)
        {
            orderDetail.Metadata = metadataByOrderDetailId.TryGetValue(orderDetail.Id, out var metadata)
                ? JsonConvert.SerializeObject(metadata)
                : null;
        }
        
        CreateOrderResponse paymentResponse;
        
        if (request.PaymentGateway == PaymentGateway.BaoKim)
        {
            var baoKimTokenResponse = await _baoKimService.CreateAccessToken();

            var createBaoKimOrderResponse = await _baoKimService.CreateOrder(
                baoKimTokenResponse.Data.AccessToken, newOrder);

            newOrder.PaymentUrl = createBaoKimOrderResponse.Data.PaymentUrl;

            await SavePendingOrderWithSlotValidation(
                newOrder,
                newOrderDetails,
                slotDate,
                slotTagId,
                cancellationToken);

            return createBaoKimOrderResponse.Data;
        }
        else if (request.PaymentGateway == PaymentGateway.Sepay)
        {
            decimal totalAmount = newOrder.TotalFee;
            string description = $"HTM-{newOrder.Id.ToString()}";
            string bankName = "MBBank";
            string bankAccount = "3085299693483";
            string qrCodeUrl = $"https://qr.sepay.vn/img?" +
                               $"acc={bankAccount}&" +
                               $"bank={bankName}&" +
                               $"amount={totalAmount}&" +
                               $"des={description}&" +
                               $"template=qronly&" +
                               "showinfo=true&" +
                               "holder=HO KINH DOANH HOA THEO MUA";

            var sepayReponse = new CreateOrderResponse()
            {
                MrcOrderId = newOrder.Id.ToString(),
                PaymentUrl = qrCodeUrl,
                OrderId = 0
            };

            newOrder.QrUrl = qrCodeUrl;

            await SavePendingOrderWithSlotValidation(
                newOrder,
                newOrderDetails,
                slotDate,
                slotTagId,
                cancellationToken);

            return sepayReponse;
        }

        throw new BadRequestException("Not support other gateway");
    }
    //Format dữ liệu -> nhanhvn + snapshot
    private async Task<Response.PreparedOrder> PrepareOrderItems(Request.CreateOrderRequest request, Guid userId)
    {
        if (request?.Items == null || request.Items.Count == 0 ||
            request.Items.Any(x => x.ItemId <= 0 || x.ProductId == Guid.Empty || x.Number <= 0))
            throw new BadRequestException("ORDER_ITEMS_INVALID");

        var flowerIds = request.Items
            .SelectMany(x => x.Bouquets ?? new List<Request.BouquetSelection>())
            .Where(x => x?.GeneratedFlowerId.HasValue == true)
            .Select(x => x.GeneratedFlowerId!.Value)
            .Distinct()
            .ToList();

        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();

        var specifications = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x => productIds.Contains(x.SubId) && !x.IsDeleted)
            .Select(x => new
            {
                x.SubId,
                x.MatId,
                x.Quantity,
                x.IsCore,
                x.IsActive,
                MaterialDeleted = x.Mat.IsDeleted,
                MaterialName = x.Mat.ProductName
            })
            .ToListAsync();

        var specificationsByCombo = specifications
            .GroupBy(x => x.SubId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var flowers = flowerIds.Count == 0
            ? new Dictionary<Guid, GeneratedFlower>()
            : await _dbContext.GeneratedFlowers
                .AsNoTracking()
                .Where(x => flowerIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

        var histories = flowerIds.Count == 0
            ? new List<ClientHistory>()
            : await _dbContext.ClientHistories
                .AsNoTracking()
                .Where(x => flowerIds.Contains(x.OutputId) &&
                            x.Type == ClientHistoryType.Flower)
                .ToListAsync();

        var historyByFlowerId = histories.ToDictionary(x => x.OutputId, x => x);

        var result = new Response.PreparedOrder();
        var sourceItems = new List<Response.PreparedOrderItem>();
        var extraItemsByProductId = new Dictionary<Guid, Response.PreparedOrderItem>();
        var stockByProductId = new Dictionary<Guid, decimal>();

        void AddStockRequirement(Guid productId, decimal quantity)
        {
            if (productId == Guid.Empty || quantity <= 0)
                throw new BadRequestException("ORDER_STOCK_QUANTITY_INVALID");

            stockByProductId.TryGetValue(productId, out var currentQuantity);
            stockByProductId[productId] = currentQuantity + quantity;
        }

        void AddExtraPurchaseItem(Guid productId, decimal quantity)
        {
            if (productId == Guid.Empty || quantity <= 0)
                throw new BadRequestException("ORDER_ITEM_QUANTITY_INVALID");

            if (!extraItemsByProductId.TryGetValue(productId, out var item))
            {
                extraItemsByProductId[productId] = new Response.PreparedOrderItem
                {
                    LineId = Guid.NewGuid(),
                    ProductId = productId,
                    Number = quantity,
                    IsSourceItem = false
                };
                return;
            }

            try
            {
                item.Number = checked(item.Number + quantity);
            }
            catch (OverflowException)
            {
                throw new BadRequestException("ORDER_ITEM_QUANTITY_INVALID");
            }
        }

        foreach (var orderItem in request.Items)
        {
            var orderLineId = Guid.NewGuid();
            // Mỗi phần tử FE gửi luôn là một dòng nguồn riêng. Không gộp ở đây,
            // vì hai Flower khác nhau có thể cùng ProductId/ItemId.
            sourceItems.Add(new Response.PreparedOrderItem
            {
                LineId = orderLineId,
                ProductId = orderItem.ProductId,
                Number = orderItem.Number,
                ItemId = orderItem.ItemId,
                IsSourceItem = true
            });

            if (orderItem.Bouquets?.Count > orderItem.Number)
                throw new BadRequestException("BOUQUET_COUNT_MISMATCH");

            var selections = orderItem.Bouquets ?? new List<Request.BouquetSelection>();

            var cards = selections
                .Where(x => x.Card != null)
                .Select(x => x.Card!)
                .ToList();

            var generatedFlowerIds = selections
                .Where(x => x.GeneratedFlowerId.HasValue)
                .Select(x => x.GeneratedFlowerId!.Value)
                .Distinct()
                .ToList();

            if (generatedFlowerIds.Count > 1)
                throw new BadRequestException("ORDER_ITEM_GENERATED_FLOWER_MISMATCH");

            // Một dòng Order chỉ đại diện cho Combo thường, hoặc một mẫu hoa AI.
            // Không cho trộn một lựa chọn có Flower với lựa chọn không có Flower,
            // vì khi đã có Flower thì Flower đó áp dụng cho toàn bộ Number của dòng.
            
            if (generatedFlowerIds.Count == 1 &&
                selections.Any(x => !x.GeneratedFlowerId.HasValue))
                throw new BadRequestException("ORDER_ITEM_GENERATED_FLOWER_MISMATCH");

            var isCombo = specificationsByCombo.TryGetValue(
                orderItem.ProductId, out var comboSpecifications);

            if (!isCombo)
            {
                //tồn kho product thường thường
                if (generatedFlowerIds.Count > 0)
                    throw new BadRequestException("GENERATED_FLOWER_SOURCE_INVALID");

                AddStockRequirement(orderItem.ProductId, orderItem.Number);

                if (cards.Count > 0)
                    result.Bouquets.Add(new Response.ResolvedBouquet
                    {
                        OrderLineId = orderLineId,
                        ItemId = orderItem.ItemId,
                        ProductId = orderItem.ProductId,
                        Number = orderItem.Number,
                        Cards = cards
                    });

                continue;
            }
            //checkout từ combo 

            if (comboSpecifications!.Count == 0 ||
                comboSpecifications.Any(x => !x.IsActive || x.MaterialDeleted ||
                                             !x.Quantity.HasValue || x.Quantity <= 0) ||
                !comboSpecifications.Any(x => x.IsCore))
                throw new BadRequestException("COMBO_SPECIFICATION_INVALID");

            var comboMaterials = comboSpecifications.Select(x =>
                new Response.ResolvedMaterial
                {
                    MatId = x.MatId,
                    Quantity = x.Quantity!.Value,
                    Name = x.MaterialName ?? string.Empty,
                    IsCore = x.IsCore
                }).ToList();

            if (generatedFlowerIds.Count == 0)
            {
                // tồn kho combo thường
                foreach (var material in comboMaterials.Where(x => x.IsCore))
                    AddStockRequirement(material.MatId, material.Quantity * orderItem.Number);

                result.Bouquets.Add(new Response.ResolvedBouquet
                {
                    OrderLineId = orderLineId,
                    ItemId = orderItem.ItemId,
                    ProductId = orderItem.ProductId,
                    Number = orderItem.Number,
                    Materials = comboMaterials,
                    Cards = cards
                });

                continue;
            }

            //có mẫu hoa AI -> lấy của nó cho combo thường -> ko bị trùng
            var flowerId = generatedFlowerIds[0];
            if (!flowers.TryGetValue(flowerId, out var flower))
                throw new BadRequestException("GENERATED_FLOWER_NOT_FOUND");
            if (flower.UserId != userId)
                throw new BadRequestException("GENERATED_FLOWER_NOT_OWNED");

            if (!historyByFlowerId.TryGetValue(flowerId, out var flowerHistory) ||
                flowerHistory.UserId != userId ||
                flowerHistory.RootId != orderItem.ProductId)
                throw new BadRequestException("GENERATED_FLOWER_SOURCE_INVALID");

            Response.FlowerOrderMetadata? flowerMetadata;
            try
            {
                flowerMetadata = JsonConvert.DeserializeObject<Response.FlowerOrderMetadata>(flower.Metadata);
            }
            catch (JsonException)
            {
                throw new BadRequestException("GENERATED_FLOWER_METADATA_INVALID");
            }

            if (flowerMetadata == null)
                throw new BadRequestException("GENERATED_FLOWER_METADATA_INVALID");

            flowerMetadata.AdditionCustome ??= new List<Response.ResolvedMaterial>();

            if (flowerMetadata.SubId != orderItem.ProductId ||
                !flowerMetadata.AiCustomize ||
                flowerMetadata.Materials is not { Count: > 0 } ||
                flowerMetadata.Ribbon == null || flowerMetadata.Ribbon.Id == Guid.Empty ||
                flowerMetadata.WrappingPaper == null || flowerMetadata.WrappingPaper.Id == Guid.Empty)
                throw new BadRequestException("GENERATED_FLOWER_METADATA_INVALID");

            result.Bouquets.Add(new Response.ResolvedBouquet
            {
                OrderLineId = orderLineId,
                ItemId = orderItem.ItemId,
                ProductId = orderItem.ProductId,
                Number = orderItem.Number,
                Name = flower.Name,
                GeneratedFlowerId = flowerId,
                GeneratedFlowerImageUrl = flower.ImageUrl,
                AiCustomize = true,
                Materials = flowerMetadata.Materials,
                AdditionCustome = flowerMetadata.AdditionCustome,
                Ribbon = flowerMetadata.Ribbon,
                WrappingPaper = flowerMetadata.WrappingPaper,
                Cards = cards
            });
            //combo nguồn của mẫu hoa hiện có 
            foreach (var material in flowerMetadata.Materials.Where(x => x.IsCore))
                AddStockRequirement(material.MatId, material.Quantity * orderItem.Number);
            // phần add thêm được xem là product thường
            foreach (var addition in flowerMetadata.AdditionCustome)
            {
                var quantity = addition.Quantity * orderItem.Number;
                AddStockRequirement(addition.MatId, quantity);
                AddExtraPurchaseItem(addition.MatId, quantity);
            }

            AddStockRequirement(flowerMetadata.WrappingPaper.Id, orderItem.Number);
            AddStockRequirement(flowerMetadata.Ribbon.Id, orderItem.Number);
            AddExtraPurchaseItem(flowerMetadata.WrappingPaper.Id, orderItem.Number);
            AddExtraPurchaseItem(flowerMetadata.Ribbon.Id, orderItem.Number);
        }
        
        result.PurchaseItems = sourceItems
            .Concat(extraItemsByProductId.Values)
            .ToList();
        
        result.StockRequirements = stockByProductId
            .Select(x => new Response.StockRequirement
            {
                ProductId = x.Key,
                Quantity = x.Value
            })
            .ToList();

        return result;
    }
    
    private async Task<List<NhanhProductData>> FetchLocalProducts(List<Response.PreparedOrderItem> items)
    {
        if (items.Count == 0 ||
            items.Any(x => x.ProductId == Guid.Empty || x.Number <= 0))
            throw new BadRequestException("ORDER_ITEMS_INVALID");

        var productIds = items.Select(x => x.ProductId).Distinct().ToList();
        var productRows = await _dbContext.Products
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.NhanhProductId,
                x.ProductName,
                x.Price,
                x.Height,
                x.Weight,
                x.Length,
                x.Width,
                x.IsActive,
                x.IsDeleted,
                
                ParentIsPurchasable =
                    string.IsNullOrWhiteSpace(x.ParentProductId) ||
                    x.ParentProductId.StartsWith("-") ||
                    _dbContext.Products.Any(parent =>
                        parent.NhanhProductId == x.ParentProductId &&
                        parent.IsActive &&
                        !parent.IsDeleted)

            })
            .ToListAsync();

        if (productRows.Count != productIds.Count ||
            productRows.Any(x => !x.IsActive || x.IsDeleted || !x.ParentIsPurchasable))
            throw new BadRequestException("ORDER_PRODUCT_UNAVAILABLE");

        if (productRows.Any(x => string.IsNullOrWhiteSpace(x.NhanhProductId) ||
                                 !int.TryParse(x.NhanhProductId, out _)) ||
            productRows.GroupBy(x => x.NhanhProductId).Any(x => x.Count() > 1))
            throw new BadRequestException("ORDER_PRODUCT_ITEM_ID_INVALID");

        var productRowsById = productRows.ToDictionary(x => x.Id);

        foreach (var item in items)
        {
            var product = productRowsById[item.ProductId];
            var itemId = int.Parse(product.NhanhProductId!);

            // ItemId của dòng Customer gửi phải khớp catalog local.
            if (item.IsSourceItem && item.ItemId != itemId)
                throw new BadRequestException("ORDER_ITEM_PRODUCT_MISMATCH");

            item.ItemId = itemId;
        }

        // Shipping chỉ cần một ProductData cho mỗi ItemId Nhanh; quantity đã
        // được cộng riêng ở CreateOrder. OrderDetails vẫn được tạo theo từng
        // PreparedOrderItem, không theo danh sách này.
        return productRows.Select(product => new NhanhProductData
        {
            Id = int.Parse(product.NhanhProductId!),
            Name = product.ProductName ?? string.Empty,
            Prices = new NhanhPrices
            {
                Retail = product.Price ?? 0m
            },
            Shipping = new NhanhShipping
            {
                Height = (double)(product.Height ?? 0m),
                Weight = (double)(product.Weight ?? 0m),
                Length = (double)(product.Length ?? 0m),
                Width = (double)(product.Width ?? 0m)
            }
        }).ToList();
    }
    
     private async Task<Dictionary<Guid, Request.OrderDetailMetadata>> BuildOrderDetailsMetadata(Response.PreparedOrder preparedOrder, Guid userId)
    {
        var productIds = preparedOrder.PurchaseItems.Select(x => x.ProductId)
            .Concat(preparedOrder.Bouquets.Select(x => x.ProductId))
            .Concat(preparedOrder.Bouquets.SelectMany(x => x.Materials).Select(x => x.MatId))
            .Concat(preparedOrder.Bouquets.SelectMany(x => x.AdditionCustome).Select(x => x.MatId))
            .Concat(preparedOrder.Bouquets.Where(x => x.Ribbon != null).Select(x => x.Ribbon!.Id))
            .Concat(preparedOrder.Bouquets.Where(x => x.WrappingPaper != null).Select(x => x.WrappingPaper!.Id))
            .Distinct()
            .ToList();

        var productSnapshots = await _dbContext.Products
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new Response.CurrentProductSnapshot
            {
                Id = x.Id,
                Name = x.ProductName ?? string.Empty,
                Price = x.Price ?? 0m,
                CoverImage = x.CoverImage,
                Label = x.ProductLabel,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync();

        if (productSnapshots.Count != productIds.Count)
            throw new BadRequestException("ORDER_PRODUCT_UNAVAILABLE");

        var productById = productSnapshots.ToDictionary(x => x.Id);

        var sourceCardIds = preparedOrder.Bouquets
            .SelectMany(x => x.Cards)
            .Where(x => x.SourceGeneratedCardId.HasValue)
            .Select(x => x.SourceGeneratedCardId!.Value)
            .Distinct()
            .ToList();

        var sourceCards = sourceCardIds.Count == 0
            ? new Dictionary<Guid, GeneratedCard>()
            : await _dbContext.GeneratedCards
                .AsNoTracking()
                .Where(x => sourceCardIds.Contains(x.Id) && x.UserId == userId)
                .ToDictionaryAsync(x => x.Id);

        if (sourceCards.Count != sourceCardIds.Count ||
            sourceCards.Values.Any(x => x.CardType != GeneratedCardType.Ai ||
                                        string.IsNullOrWhiteSpace(x.RawImage)))
            throw new BadRequestException("CARD_HISTORY_SOURCE_INVALID");

        var hasAiCard = preparedOrder.Bouquets
            .SelectMany(x => x.Cards)
            .Any(x => x.CardType == GeneratedCardType.Ai);
        
        string? systemPromptContent = null;
        if (hasAiCard)
        {
            var prompt = await Card.CardPromptConfig.LoadAsync(_dbContext)
                ?? throw new BadRequestException("CARD_SYSTEM_PROMPT_UNAVAILABLE");
            systemPromptContent = prompt.Content;
        }

        var result = new Dictionary<Guid, Request.OrderDetailMetadata>();

        Request.MaterialSnapshot ToMaterialSnapshot(Response.ResolvedMaterial material)
        {
            if (!productById.TryGetValue(material.MatId, out var product))
                throw new BadRequestException("ORDER_PRODUCT_UNAVAILABLE");

            return new Request.MaterialSnapshot
            {
                MatId = material.MatId,
                Quantity = material.Quantity,
                Name = product.Name,
                Price = product.Price,
                IsCore = material.IsCore
            };
        }

        Request.AccessorySnapshot? ToAccessorySnapshot(Response.ResolvedAccessory? accessory)
        {
            if (accessory == null)
                return null;
            if (!productById.TryGetValue(accessory.Id, out var product))
                throw new BadRequestException("ORDER_PRODUCT_UNAVAILABLE");

            return new Request.AccessorySnapshot
            {
                Id = accessory.Id,
                Name = accessory.Name ?? product.Name,
                Label = accessory.Label ?? product.Label,
                ImageUrl = accessory.ImageUrl,
                Price = product.Price,
                IsActive = accessory.IsActive,
                IsDeleted = accessory.IsDeleted
            };
        }

        async Task<Request.CardSnapshot> BuildCardSnapshotAsync(
            Response.ResolvedBouquet bouquet,
            Request.BouquetCardSelection card)
        {
            if (!productById.TryGetValue(bouquet.ProductId, out var sourceProduct) ||
                string.IsNullOrWhiteSpace(bouquet.GeneratedFlowerId.HasValue ? bouquet.GeneratedFlowerImageUrl : sourceProduct.CoverImage))
                throw new BadRequestException("CARD_SOURCE_PRODUCT_INVALID");

            if (card.CardType == GeneratedCardType.Handmade &&
                card.SourceGeneratedCardId.HasValue)
                throw new BadRequestException("HANDMADE_CARD_HISTORY_NOT_SUPPORTED");

            var price = await CalculateCardPrice(new Request.CalculateCardPriceRequest
            {
                CardType = card.CardType,
                CardTemplateId = card.CardTemplateId,
                SizeName = card.SizeName,
                FormType = card.FormType,
                MessageContent = card.MessageContent
            });

            string? rawImage = null;
            if (card.SourceGeneratedCardId.HasValue)
                rawImage = sourceCards[card.SourceGeneratedCardId.Value].RawImage;

            var snapshot = new Request.CardSnapshot
            {
                VirtualId = Guid.NewGuid(),
                CardType = price.CardType,
                Source = new Request.CardSourceSnapshot
                {
                    Type = "product",
                    Id = bouquet.ProductId,
                    ImageUrl = bouquet.GeneratedFlowerId.HasValue ? bouquet.GeneratedFlowerImageUrl! : sourceProduct.CoverImage!,
                    ImageSourceType = bouquet.GeneratedFlowerId.HasValue ? "generated_flower" : "product"
                },
                RawImage = rawImage,
                CardTemplate = new Request.CardTemplateSnapshot
                {
                    TemplateId = price.CardTemplate.TemplateId,
                    TemplateName = price.CardTemplate.TemplateName,
                    TemplateType = price.CardTemplate.TemplateType,
                    TemplateImageUrl = price.CardTemplate.TemplateImageUrl
                },
                CardSize = price.CardSize == null
                    ? null
                    : new Request.CardSizeSnapshot
                    {
                        Name = price.CardSize.Name,
                        Width = price.CardSize.Width,
                        Height = price.CardSize.Height,
                        BasePrice = price.CardSize.BasePrice,
                        MaxWords = price.CardSize.MaxWords
                    },
                CardConfig = price.CardConfig == null
                    ? null
                    : new Request.CardConfigSnapshot
                    {
                        MinWords = price.CardConfig.MinWords,
                        MaxWords = price.CardConfig.MaxWords,
                        ExtraPrice = price.CardConfig.ExtraPrice
                    },
                MessageContent = card.MessageContent ?? string.Empty,
                FormType = price.FormType,
                AttachedImageUrl = card.AttachedImageUrl,
                Quantity = 1,
                WordCount = price.WordCount,
                BasePrice = price.BasePrice,
                ExtraPrice = price.ExtraPrice,
                TotalPrice = price.TotalPrice,
                ValidatedAt = DateTimeOffset.UtcNow
            };

            if (card.CardType == GeneratedCardType.Ai)
            {
                snapshot.SystemPrompt = new Request.SystemPromptSnapshot
                {
                    Type = "card",
                    Content = systemPromptContent!
                };
                snapshot.Status = "not yet";
                snapshot.IdResult = new List<Guid>();
                snapshot.FinalGeneratedCardId = null;
            }

            return snapshot;
        }

        foreach (var bouquet in preparedOrder.Bouquets)
        {
            var metadata = new Request.OrderDetailMetadata();
            var sourceProduct = productById[bouquet.ProductId];

            Request.ProductMetadataSnapshot BuildProductMetadata(Request.CardSnapshot? cardSnapshot)
            {
                return new Request.ProductMetadataSnapshot
                {
                    Name = string.IsNullOrWhiteSpace(bouquet.Name) ? sourceProduct.Name : bouquet.Name,
                    SubId = bouquet.ProductId,
                    GeneratedFlower = bouquet.GeneratedFlowerId,
                    AiCustomize = bouquet.AiCustomize,
                    Ribbon = ToAccessorySnapshot(bouquet.Ribbon),
                    WrappingPaper = ToAccessorySnapshot(bouquet.WrappingPaper),
                    AdditionCustome = bouquet.AdditionCustome.Select(ToMaterialSnapshot).ToList(),
                    Materials = bouquet.Materials.Select(ToMaterialSnapshot).ToList(),
                    SnapShotCard = cardSnapshot
                };
            }

            if (bouquet.Cards.Count == 0)
            {
                metadata.ProductMetadata.Add(BuildProductMetadata(null));
            }
            else
            {
                foreach (var card in bouquet.Cards)
                {
                    var cardSnapshot = await BuildCardSnapshotAsync(bouquet, card);
                    metadata.ProductMetadata.Add(BuildProductMetadata(cardSnapshot));
                }
            }

            result[bouquet.OrderLineId] = metadata;
        }

        return result;
    }

    public async Task<Response.LimitCoreProductResponse> GetLimitCoreProduct(Guid comboId)
    {
        var specifications = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x =>
                x.SubId == comboId &&
                !x.IsDeleted &&
                x.IsActive &&
                !x.Mat.IsDeleted)
            .Select(x => new
            {
                x.MatId,
                Quantity = x.Quantity ?? 0m,
                x.IsCore,
                x.IsActive,
                AvailableForSale = x.Mat.AvailableForSale ?? 0m
            })
            .ToListAsync();

        var activeCoreSpecifications = specifications
            .Where(x => x.IsCore && x.IsActive)
            .ToList();

        if (activeCoreSpecifications.Count == 0 ||
            activeCoreSpecifications.Any(x => x.Quantity <= 0))
        {
            throw new BadRequestException("Tồn tại vật liệu core với số lượng <= 0");
        }

        var sellableQuantity = activeCoreSpecifications
            .Select(x => Math.Floor(x.AvailableForSale / x.Quantity))
            .Min();

        return new Response.LimitCoreProductResponse
        {
            SellableQuantityCombo = sellableQuantity,
            Materials = specifications
                .Select(x => new Response.LimitCoreMaterialResponse
                {
                    MatId = x.MatId,
                    Quantity = x.Quantity,
                    IsCore = x.IsCore,
                    IsActive = x.IsActive,
                    AvailableForSale = x.AvailableForSale
                })
                .ToList()
        };
    }

    // public async Task<CreateOrderResponse> CreateDefaultOrder()
    // {
    //     var request = new Request.CreateOrderRequest
    //     {
    //         Latitude = 10.820204590328558m,
    //         Longitude = 106.7731798123973m,
    //         Ward = 0,
    //         City = 0,
    //         Address = "Khu Dân Cư Cao Cấp An Thiên Lý, Phường Phước Long, Hồ Chí Minh",
    //         ShortAddress = "Khu Dân Cư Cao Cấp An Thiên Lý, Phường Phước Long, Hồ Chí Minh",
    //         FullName = "Tân Trần",
    //         Phone = "0979901550",
    //         Remarks = "",
    //         OrderTime = 0,
    //         Items =
    //         [
    //             new AhaMoveRequest.Items
    //             {
    //                 ItemId = 1833667,
    //                 Number = 1
    //             }
    //
    //         ],
    //         Gateway = null
    //     };
    //
    //     request.TagIds = new List<long>();
    //     request.TaxCode = null;
    //     request.IdentityNumber = null;
    //     request.ReceiptFullName = null;
    //     request.ReceiptPhoneNumber = null;
    //     request.ReceiptAddress = null;
    //     request.ReceiptEmail = null;
    //     request.IsExportInvoice = false;
    //     request.InvoiceType = null;
    //     request.PaymentMethod = Repository.Enum.PaymentMethod.VA;
    //     request.Gateway = "BaoKim";
    //
    //     var result = await CreateOrder(request);
    //
    //     return result;
    // }

    private async Task<List<NhanhProductData>> FetchLocalProductsAsync(List<AhaMoveRequest.Items> items)
    {
        var productIds = items
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var productRows = await _dbContext.Products
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.NhanhProductId,
                x.ProductName,
                x.Price,
                x.Height,
                x.Weight,
                x.Length,
                x.Width,
                x.IsActive,
                x.IsDeleted
            })
            .ToListAsync();

        if (productRows.Any(x => !x.IsActive || x.IsDeleted))
        {
            throw new BadRequestException("Đơn hàng có sản phẩm đã ngừng hoạt động");
        }

        if (productRows.Count != productIds.Count)
        {
            throw new BadRequestException("Đơn hàng có sản phẩm không tồn tại hoặc đã ngừng hoạt động");
        }

        if (productRows.Any(x =>
                string.IsNullOrWhiteSpace(x.NhanhProductId) ||
                !int.TryParse(x.NhanhProductId, out _)))
        {
            throw new BadRequestException("Đơn hàng có sản phẩm không có NhanhProductId hợp lệ");
        }

        var productRowsById = productRows.ToDictionary(x => x.Id);

        return productIds.Select(productId =>
        {
            var product = productRowsById[productId];

            return new NhanhProductData
            {
                Id = int.Parse(product.NhanhProductId!),
                Name = product.ProductName ?? string.Empty,
                Prices = new NhanhPrices
                {
                    Retail = product.Price ?? 0m
                },
                Shipping = new NhanhShipping
                {
                    Height = (double)(product.Height ?? 0m),
                    Weight = (double)(product.Weight ?? 0m),
                    Length = (double)(product.Length ?? 0m),
                    Width = (double)(product.Width ?? 0m)
                }
            };
        }).ToList();
    }

    // Legacy fallback: fetches the catalog rows for the requested items from Nhanh.vn.
    private async Task<List<NhanhProductData>> FetchNhanhProductsAsync(
        List<AhaMoveRequest.Items> items,
        GroupConfig configs,
        CancellationToken cancellationToken = default)
    {
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl)
                       + "/product/list?appId=" + appId + "&businessId=" + businessId;

        var productListRequest = new NhanhVnRequest.NhanhProductListRequest()
        {
            Filters = new NhanhVnRequest.NhanhFilters()
            {
                Id = items.Select(x => x.ItemId).ToList(),
                Status = [1, 2]
            },
            Paginator = new NhanhVnRequest.NhanhPaginator()
            {
                Size = 50,
                Sort = new Dictionary<string, string>() { { "id", "desc" } },
                Next = new Dictionary<string, int>() { { "id", 0 } },
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        httpRequest.Headers.Add("Authorization", token);
        httpRequest.Content = JsonContent.Create(productListRequest);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new BadRequestException($"NhanhVn API HTTP Error ({response.StatusCode})");
        }

        var result = await response.Content.ReadFromJsonAsync<NhanhProductListResponse>(cancellationToken);
        if (result == null || result.Data == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }

        return result.Data;
    }

    private async Task EnsureItemsInStock(List<Response.StockRequirement> requirements)
    {
        var requiredProductIds = requirements.Select(x => x.ProductId).ToList();
        var availableForSaleByProductId = await _dbContext.Products
            .AsNoTracking()
            .Where(x => requiredProductIds.Contains(x.Id) && !x.IsDeleted && x.IsActive)
            .Select(x => new
            {
                x.Id,
                AvailableForSale = x.AvailableForSale ?? 0m
            })
            .ToDictionaryAsync(x => x.Id, x => x.AvailableForSale);

        foreach (var requirement in requirements)
        {
            if (!availableForSaleByProductId.TryGetValue(requirement.ProductId, out var availableForSale))
                throw new BadRequestException("ORDER_PRODUCT_UNAVAILABLE");

            if (requirement.Quantity > availableForSale)
                throw new BadRequestException("CORE_MATERIAL_INSUFFICIENT");
        }
    }

    // Calls Ahamove to estimate the shipping fee from the shop's warehouse to the buyer.
    private async Task<decimal> EstimateShippingFeeAsync(
        Request.CreateOrderRequest request,
        List<NhanhProductData> products,
        IReadOnlyDictionary<int, decimal> quantityByItemId, //int
        Repository.Entity.CompanyAddress? companyAddress)
    {
        return request.ShippingGateway switch
        {
            ShippingGateway.Ahamove => await EstimateAhamoveShippingFeeAsync(
                request,
                products,
                quantityByItemId,
                companyAddress),
            ShippingGateway.XanhSm => await EstimateXanhSmShippingFeeAsync(
                request,
                products,
                quantityByItemId,
                companyAddress),
            _ => throw new BadRequestException($"Shipping gateway {request.ShippingGateway} is not supported")
        };
    }

    // Calls Ahamove to estimate the shipping fee from the shop's warehouse to the buyer.
    private async Task<decimal> EstimateAhamoveShippingFeeAsync(
        Request.CreateOrderRequest request,
        List<NhanhProductData> products,
        IReadOnlyDictionary<int, decimal> quantityByItemId, //int
        Repository.Entity.CompanyAddress? companyAddress)
    {
        var itemsRequest = products.Select(x => new AhaMoveRequest.AhamoveItem()
        {
            Id = x.Id.ToString(),
            Name = x.Name,
            Price = (int)x.Prices.Retail,
            // Ahamove only accepts an integer item count. Keep the fractional
            // quantity in Order/NhanhVN; round up only at this boundary.
            //Num = quantityByItemId[x.Id]
            Num = checked((int)Math.Ceiling(quantityByItemId[x.Id])) 
        }).ToList();

        var packageDetailRequest = products.Select(x => new AhaMoveRequest.PackageDetail()
        {
            Height = x.Shipping.Height,
            Weight = x.Shipping.Weight,
            Length = x.Shipping.Length,
            Width = x.Shipping.Width,
            Description = x.Name
        }).ToList();

        var estimateRequest = new AhaMoveRequest.AhaMoveShippingFeeRequest()
        {
            OrderTime = request.OrderTime,
            IdleUntil = request.OrderTime > 0 ? request.OrderTime : 0,
            Paths = new List<AhaMoveRequest.AhamoveLocationPoint>()
            {
                new AhaMoveRequest.AhamoveLocationPoint() // Điểm lấy hàng: kho của shop, lấy từ DB.
                {
                    Latitude = companyAddress?.Latitude ?? 0,
                    Longitude = companyAddress?.Longitude ?? 0,
                    Address = companyAddress?.AddressLine ?? "",
                    ShortAddress = (companyAddress?.AddressLine + ", " + companyAddress?.Ward + ", " + companyAddress?.City) ?? "",
                    FullName = companyAddress!.FullName,
                    Phone = companyAddress.PhoneNumber,
                    Remarks = companyAddress.Remarks!,
                },
                new AhaMoveRequest.AhamoveLocationPoint()
                {
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Address = request.Address,
                    ShortAddress = request.ShortAddress,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Remarks = request.Remarks,
                },
            },
            PaymentMethod = "CASH",
            Remarks = request.Remarks,
            Items = itemsRequest,
            PackageDetail = packageDetailRequest,
            Services = new List<AhaMoveRequest.ServiceRequest>()
            {
                new AhaMoveRequest.ServiceRequest()
                {
                    Id = "SGN-BIKE",
                    Requests = new List<AhaMoveRequest.ItemsRequest>()
                    {
                        new AhaMoveRequest.ItemsRequest() { Id = "SGN-BIKE-BAGA" },
                    }
                },
            }
        };

        var ahamoveConfigs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "ahamove");
        var ahamoveApi = ahamoveConfigs.GetValue("ahamove.base_url", _ahaMoveOption.BaseUrl) + "/v3/orders/estimates";
        var ahamoveToken = ahamoveConfigs.GetValue("ahamove.api_key", _ahaMoveOption.Token);
        var freeShipping = ahamoveConfigs.GetValue("ahamove.free_ship", 0);


        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ahamoveApi);
        httpRequest.Headers.Add("Authorization", ahamoveToken);
        httpRequest.Content = JsonContent.Create(estimateRequest);

        using var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            throw new BadRequestException($"Ahamove API HTTP Error ({response.StatusCode})");
        }

        var result = await response.Content.ReadFromJsonAsync<List<ServiceResponse>>();
        if (result == null || result.Count == 0)
        {
            throw new BadRequestException("Ahamove returned null or empty data");
        }

        if (freeShipping > 0 && result[0].Data.Distance <= freeShipping)
        {
            return 0;
        }

        return result[0].Data.TotalFee;
    }

    private async Task<decimal> EstimateXanhSmShippingFeeAsync(
        Request.CreateOrderRequest request,
        IReadOnlyCollection<NhanhProductData> products,
        IReadOnlyDictionary<int, decimal> quantityByItemId, //int
        Repository.Entity.CompanyAddress? companyAddress)
    {
        if (_xanhSmService is null)
        {
            throw new ServerException("XanhSM service is not configured");
        }

        if (companyAddress is null)
        {
            throw new BadRequestException("Company pickup address is not configured");
        }

        var orderTime = request.OrderTime > 0
            ? request.OrderTime
            : DateTimeOffset.UtcNow.ToUnixTimeSeconds() + XanhSmConstants.PreparationTimeSeconds;
        var quote = await _xanhSmService.EstimateDoorToDoorAsync(
            CreateXanhSmPoints(
                companyAddress,
                request.Latitude,
                request.Longitude,
                request.Address,
                request.ShortAddress,
                request.FullNameReceiver,
                request.PhoneReceiver,
                request.Remarks,
                CalculateXanhSmPackageWeightKg(products.Select(product =>
                    (product.Shipping.Weight, quantityByItemId[product.Id])))),
            orderTime);

        return await GetXanhSmBuyerShippingFeeAsync(quote);
    }

    private async Task<decimal> GetXanhSmBuyerShippingFeeAsync(XanhSmDoorToDoorQuote quote)
    {
        // ahamove.free_ship is configured in kilometers (the admin setting's unit).
        var configs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "ahamove");
        var freeShippingKm = configs.GetValue("ahamove.free_ship", 0d);
        var distanceKm = quote.DistanceUnit?.Trim().ToLowerInvariant() switch
        {
            "km" => quote.Distance,
            "m" => quote.Distance / 1000,
            _ => null
        };

        if (freeShippingKm > 0 && distanceKm is >= 0 && distanceKm <= freeShippingKm)
        {
            return 0;
        }

        return quote.TotalFee;
    }

    public async Task<Response.EstimateXanhSmShippingFeeResponse> EstimateXanhSmShippingFee(
        Request.EstimateXanhSmShippingFeeRequest request,
        CancellationToken cancellationToken = default) 
    {
        var checkoutConfigs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "checkout");
        var deliveryMethodsConfig = checkoutConfigs.GetValue<string>("checkout.delivery_methods");

        if (!string.IsNullOrWhiteSpace(deliveryMethodsConfig))
        {
            var deliveryMethods = JObject.Parse(deliveryMethodsConfig);
            var xanhSmMethod = deliveryMethods["items"]?
                .FirstOrDefault(x => (int?)x["gateway"] == 2);

            if ((bool?)xanhSmMethod?["enabled"] == false)
            {
                throw new BadRequestException("Phương thức giao hàng XanhSm hiện đang bị tắt");
            }
        }

        if (_xanhSmService is null)
        {
            throw new ServerException("XanhSM service is not configured");
        }

        var companyAddress = await _dbContext.CompanyAddresses
            .Where(address => !address.IsDeleted)
            .OrderByDescending(address => address.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (companyAddress is null)
        {
            throw new BadRequestException("Company pickup address is not configured");
        }

        var products = await FetchLocalProductsAsync(request.Items);
        var productIds = products.Select(product => product.Id).ToHashSet();
        if (request.Items.Any(item => !productIds.Contains(item.ItemId)))
        {
            throw new BadRequestException("Đơn hàng không tồn tại trong hệ thống");
        }

        var quantityByItemId = request.Items.ToDictionary(item => item.ItemId, item => item.Number);

        var orderTime = request.OrderTime > 0
            ? request.OrderTime
            : DateTimeOffset.UtcNow.ToUnixTimeSeconds() + XanhSmConstants.PreparationTimeSeconds;
        var quote = await _xanhSmService.EstimateDoorToDoorAsync(
            CreateXanhSmPoints(
                companyAddress,
                request.Latitude,
                request.Longitude,
                request.Address,
                request.ShortAddress,
                request.FullName,
                request.Phone,
                request.Remarks,
                CalculateXanhSmPackageWeightKg(products.Select(product =>
                    //(product.Shipping.Weight, quantityByItemId[product.Id]
                    (product.Shipping.Weight, (decimal)quantityByItemId[product.Id])))),
            orderTime,
            cancellationToken);

        return new Response.EstimateXanhSmShippingFeeResponse
        {
            ShippingFee = await GetXanhSmBuyerShippingFeeAsync(quote),
            Distance = quote.Distance,
            DistanceUnit = quote.DistanceUnit,
            Duration = quote.Duration,
            OrderTime = orderTime,
            ExpiresAt = quote.ExpiresAt
        };
    }

    internal static List<XanhSmDeliveryPointRequest> CreateXanhSmPoints(
        Repository.Entity.CompanyAddress companyAddress,
        decimal dropOffLatitude,
        decimal dropOffLongitude,
        string dropOffAddress,
        string dropOffShortAddress,
        string dropOffName,
        string dropOffPhone,
        string? dropOffNote,
        int packageWeightKg)
    {
        if (companyAddress.Latitude is null || companyAddress.Longitude is null)
        {
            throw new BadRequestException("Company pickup coordinates are not configured");
        }

        var pickupAddress = string.Join(", ", new[]
            {
                companyAddress.AddressLine,
                companyAddress.Ward,
                companyAddress.City,
                companyAddress.Country
            }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

        return
        [
            new XanhSmDeliveryPointRequest
            {
                Type = XanhSmConstants.PickUpPoint,
                Name = companyAddress.FullName,
                PhoneNumber = NormalizeXanhSmPhoneNumber(companyAddress.PhoneNumber),
                Address = pickupAddress,
                ShortAddress = pickupAddress,
                AddressNote = companyAddress.Remarks,
                Latitude = (double)companyAddress.Latitude.Value,
                Longitude = (double)companyAddress.Longitude.Value,
                PackageWeight = packageWeightKg,
                // Sandbox service 1878 accepts LARGE/flowers in estimate/order but
                // returns null package metadata from list-available-services.
                // The estimate endpoint remains the authoritative validator.
                PackageSize = XanhSmConstants.LargePackageSize,
                PackageTypes = [XanhSmConstants.FlowersPackageType]
            },
            new XanhSmDeliveryPointRequest
            {
                Type = XanhSmConstants.DropOffPoint,
                Name = dropOffName,
                PhoneNumber = NormalizeXanhSmPhoneNumber(dropOffPhone),
                Address = dropOffAddress,
                ShortAddress = dropOffShortAddress,
                AddressNote = dropOffNote,
                Latitude = (double)dropOffLatitude,
                Longitude = (double)dropOffLongitude
            }
        ];
    }

    internal static string NormalizeXanhSmPhoneNumber(string phoneNumber)
    {
        var normalized = new string(phoneNumber.Trim()
            .Where(character => !char.IsWhiteSpace(character) && character is not '-' and not '.' and not '(' and not ')')
            .ToArray());
        if (normalized.StartsWith("00", StringComparison.Ordinal))
            return "+" + normalized[2..];
        if (normalized.StartsWith('0'))
            return "+84" + normalized[1..];
        if (normalized.StartsWith("84", StringComparison.Ordinal))
            return "+" + normalized;
        return normalized;
    }

    internal static int CalculateXanhSmPackageWeightKg(
        IEnumerable<(double WeightInGrams, decimal Quantity)> packages) //int
    {
        var totalWeightInGrams = packages.Sum(package =>
            //Math.Max(package.WeightInGrams, 0) * Math.Max(package.Quantity, 0
            Math.Max(package.WeightInGrams, 0) * (double)Math.Max(package.Quantity, 0m));

        // Nhanh.vn/Ahamove product shipping weight is stored in grams while
        // GreenSM requires an integer total weight in kilograms.
        return Math.Max(1, (int)Math.Ceiling(totalWeightInGrams / 1000d));
    }

    public async Task<BaseResponse> GetOrderById(Guid id)
    {
        var isAdmin = _httpContext.HttpContext?.User.IsInRole("Admin") == true;
        var userId = isAdmin ? (Guid?)null : _currentUser.GetRequiredUserId();

        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.OrderDetails.Where(detail => !detail.IsDeleted))
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x =>
                x.IsDeleted == false &&
                (isAdmin || x.UserId == userId) &&
                x.Id == id);
        if (orders == null)
        {
            throw new NotFoundException("Order not found ");
        }


        var lastTransaction = orders.Transactions
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();

        var nhanhProductIds = _dbContext.OrderDetails
            .AsNoTracking()
            .Where(x => x.OrderId == id)
            .Select(x => x.ItemId.ToString())
            .Distinct();

        var findProduct = await _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.NhanhProductId != null
                        && nhanhProductIds.Contains(x.NhanhProductId))
            .Select(x => new
            { x.Id, x.CoverImage, x.NhanhProductId, x.ParentProductId })
            .ToDictionaryAsync(x => x.NhanhProductId!);

        var parentNhanhProductIds = findProduct.Values
            .Where(x => !string.IsNullOrWhiteSpace(x.ParentProductId))
            .Select(x => x.ParentProductId!)
            .Distinct()
            .ToList();

        var productParents = await _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.NhanhProductId != null
                        && parentNhanhProductIds.Contains(x.NhanhProductId))
            .Select(x => new { x.Id, x.NhanhProductId, x.ProductName })
            .ToDictionaryAsync(x => x.NhanhProductId!);


        var orderDetails = new Response.GetOrderByIdResponse
        {
            OrderId = orders.Id,
            Status = orders.Status,
            TrackNumber = orders.TrackNumber,
            TaxCode = orders.TaxCode,
            PaymentUrl = orders.PaymentUrl,
            SepayUrl = orders.QrUrl,
            PaymentMethod = orders.PaymentMethod,
            PaymentGateway = orders.PaymentGateway,
            ShippingGateway = orders.ShippingGateway,
            //NhanhOrderId = orders.NhanhOrderId,
            //NhanhTrackingUrl = orders.NhanhTrackingUrl,
            ShippingPartyOrderId = orders.ShippingPartyOrderId,
            ShippingTrackingUrl = orders.ShippingTrackingUrl,
            TagsId = orders.TagIds,
            IdentityNumber = orders.IdentityNumber!,
            ReceiptFullName = orders.ReceiptFullName!,
            ReceiptPhoneNumber = orders.ReceiptPhoneNumber!,
            ReceiptAddress = orders.ReceiptAddress!,
            ReceiptEmail = orders.ReceiptEmail!,
            IsExportInvoice = orders.IsExportInvoice,
            InvoiceType = orders.InvoiceType,
            ShippingFee = orders.ShippingFee,
            ProductFee = orders.ProductFee,
            TotalFee = orders.TotalFee,
            VatFee = orders.Vat,
            Remark = orders.Remarks,
            OrderTime = orders.OrderTime,
            PhoneReceiver = orders.PhoneReceiver,
            FullNameReceiver = orders.FullNameReceiver,
            CreateAt = orders.CreatedAt,
            Address = orders.Address,
            ShortAddress = orders.ShortAddress,
            Latitude = orders.Latitude,
            Longitude = orders.Longitude,
            Ward = orders.Ward,
            City = orders.City,
            FullName = orders.FullName,
            Phone = orders.Phone,
            UserId = orders.UserId,

            Details = orders.OrderDetails.Select(x =>
            {
                findProduct.TryGetValue(x.ItemId.ToString(), out var product);

                Guid? parentProductId = null;
                string? parentProductName = null;

                if (!string.IsNullOrWhiteSpace(product?.ParentProductId)
                    && productParents.TryGetValue(product.ParentProductId, out var parentProduct))
                {
                    parentProductId = parentProduct.Id;
                    parentProductName = parentProduct.ProductName;
                }

                return new Response.OrderDetailsResponse()
                {
                    OrderDetailsId = x.Id,
                    ProductId = product?.Id,
                    CoverImage = product?.CoverImage,
                    ParentProductId = parentProductId,
                    ParentProductName = parentProductName,
                    Name = x.Name,
                    ItemId = x.ItemId,
                    Number = x.Number,
                    Price = x.Price,
                    Weight = x.Weight,
                    Length = x.Length,
                    Width = x.Width,
                    Height = x.Height,
                    Snapshot = DeserializeOrderDetailMetadata(x.Metadata),

                };
            }).ToList(),


            Transaction = lastTransaction == null ? null : new Response.OrderTransactionResponse
            {
                TransferAmount = lastTransaction.TransferAmount,
                // TransferAmount =  itemAmount,
                Code = lastTransaction.Code,
                TransactionDate = lastTransaction.TransactionDate,
                Gateway = lastTransaction.Gateway,
            }
        };
        return ApiResponseFactory.Base(orderDetails);
    }

    private static Request.OrderDetailMetadata? DeserializeOrderDetailMetadata(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<Request.OrderDetailMetadata>(metadata);
        }
        catch (JsonException)
        {
            // Metadata cũ hoặc lỗi không được làm trang Admin mất toàn bộ Order.
            return null;
        }
    }

    public async Task<string> GetStatusOrder(Guid id)
    {
        var userId = _currentUser.GetRequiredUserId();

        if (!(_currentUser.IsVerified || _currentUser.CanUseStorefront))
        {
            throw new UnauthorizedException("User is not verified");
        }

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o =>
                o.Id == id && o.UserId == userId);
        if (order == null)
        {
            throw new NotFoundException("Order ID " + id + " not found");
        }

        return order.Status;
    }

    public async Task<OrderListRootResponse> GetOrderNhanhVn(NhanhVnOrderListRequest.OrderListRequest request)
    {
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/order/list?appId=" + appId + "&businessId=" + businessId;
        var nhanhVnOrderSrcRequest = request;
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhVnOrderSrcRequest);

        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        OrderListRootResponse? orderListRootResponse = null;

        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            orderListRootResponse = await nhanhVnResponse.Content.ReadFromJsonAsync<OrderListRootResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (orderListRootResponse == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        if (orderListRootResponse.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        // int totalRequests = orderListRootResponse.Data.Count(x => x.Info != null);    
        //
        // int actualPageIndex = pageIndex < 1 ? 1 : pageIndex;
        // int actualPageSize = pageSize < 1 ? 10 : pageSize;
        //
        // var result = orderListRootResponse.Data
        //     .Where(x => x.Info != null)
        //     .Skip((actualPageIndex - 1) * actualPageSize)
        //     .Take(actualPageSize)
        //     .ToList();
        //
        // var paginationResult = ApiResponseFactory.BasePagination(
        //     items: result, 
        //     pageIndex: actualPageIndex, 
        //     pageSize: actualPageSize, 
        //     totalCount: totalRequests
        // );

        return orderListRootResponse;

    }

    public async Task<Response.NhanhVnLocationResponse> GetShippingLocation(Request.LocationRequest filter)
    {
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/shipping/location?appId=" + appId + "&businessId=" + businessId;
        // var nhanhVnOrderSrcRequest = new Request.LocationRequest()
        // {
        //     Filters = new Request.LocationFilters()
        //     {
        //         LocationVersion = filter.LocationVersion,
        //         Type = filter.Type,
        //         ParentId = filter.ParentId,
        //     }
        // };
        var nhanhVnOrderSrcRequest = filter;
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhVnOrderSrcRequest);

        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        Response.NhanhVnLocationResponse? shippingLocation = null;

        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            shippingLocation = await nhanhVnResponse.Content.ReadFromJsonAsync<Response.NhanhVnLocationResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (shippingLocation == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        if (shippingLocation.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        return shippingLocation;
    }

    public async Task<string> GetInfoTaxCode(string taxCode)
    {
        var vietQrApiUrl = $"https://api.vietqr.io/v2/business/{taxCode}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, vietQrApiUrl);

        HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);

        Response.TaxcodeResponse? apiResponse = null;

        if (response.IsSuccessStatusCode)
        {
            apiResponse = await response.Content.ReadFromJsonAsync<Response.TaxcodeResponse>();
        }
        else
        {
            throw new BadRequestException($"VietQR API HTTP Error ({response.StatusCode})");
        }

        if (apiResponse == null || apiResponse.Data == null)
        {
            throw new BadRequestException("VietQR API returned null or empty data");
        }

        if (apiResponse.Code != "00")
        {
            throw new BadRequestException($"VietQR API Error: {apiResponse.Desc}");
        }

        return $"{apiResponse.Data.Name}\n{apiResponse.Data.Address}";

    }

    public async Task<string> CancelOrder(Guid id)
    {
        //nếu người dùng đó đặt hàng thì mới cho người dùng đó hủy đơn hàng
        var userId = _currentUser.GetRequiredUserId();

        if (!(_currentUser.IsVerified || _currentUser.CanUseStorefront))
        {
            throw new UnauthorizedException("User is not verified");
        }

        var isExistPendingOrder = await _dbContext.Orders.
            FirstOrDefaultAsync(x =>
                x.Id == id && x.Status == "Pending" && x.UserId == userId);

        if (isExistPendingOrder == null)
        {
            throw new BadRequestException("Order not found");
        }

        isExistPendingOrder.Status = "Cancelled";
        isExistPendingOrder.UpdatedAt = DateTime.UtcNow;
        _dbContext.Orders.Update(isExistPendingOrder);
        await _dbContext.SaveChangesAsync();

        return "Hủy đơn thành công";
    }

    public async Task<string> UpdateAhaMoveService(Request.UpdateAhamoveRequest request)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id.ToString() == request.OrderId);

        if (order == null)
        {
            throw new BadRequestException("Order not found");
        }

        if (request.TrackingNumber != null)
        {
            order.TrackNumber = request.TrackingNumber;
        }

        if (request.ShippingTrackingUrl != null)
        {
            order.ShippingTrackingUrl = request.ShippingTrackingUrl;
        }

        if (request.ShippingFee != null)
        {
            order.ShippingFee = request.ShippingFee.Value;
        }

        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync();

        return "Cập nhật thành công đơn hàng";
    }

    public async Task<BasePaginationResponse> GetOrderByAdmin(string? search, int pageIndex, int pageSize)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var orders = _dbContext.Orders
            .Where(x => !x.IsDeleted);

        var normalizedSearch = search?.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            // NhanhOrderId là int? nên chỉ khớp chính xác sau khi parse được số;
            // TrackNumber và ShippingPartyOrderId là chuỗi nên cho khớp một phần.
            var nhanhOrderId = int.TryParse(normalizedSearch, out var parsedNhanhOrderId)
                ? parsedNhanhOrderId
                : (int?)null;
            //? ! ??
            var isPaymentMethod = Enum.TryParse<PaymentMethod>(
                normalizedSearch, ignoreCase: true, out var paymentMethod);

            orders = orders.Where(x =>
                x.TrackNumber.ToLower().Contains(normalizedSearch) ||
                (x.ShippingPartyOrderId != null && x.ShippingPartyOrderId.ToLower().Contains(normalizedSearch)) ||
                (nhanhOrderId != null && x.NhanhOrderId == nhanhOrderId) ||
                (isPaymentMethod && x.PaymentMethod == paymentMethod) ||
                (x.Phone.Contains(normalizedSearch)) || 
                x.Id.ToString().Contains(normalizedSearch));
        }

        var totalCount = await orders.CountAsync();

        var items = orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);

        var result = items.Select(x => new Response.GetOrderAdminResponse
        {
            OrderId = x.Id,
            UserId = x.UserId,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            Ward = x.Ward,
            City = x.City,
            Address = x.Address,
            ShortAddress = x.ShortAddress,
            FullName = x.FullName,
            Phone = x.Phone,
            Remarks = x.Remarks,
            Status = x.Status,
            TrackNumber = x.TrackNumber,
            NhanhOrderId = x.NhanhOrderId,
            NhanhTrackingUrl = x.NhanhTrackingUrl,
            ShippingPartyOrderId = x.ShippingPartyOrderId,
            ShippingTrackingUrl = x.ShippingTrackingUrl,
            PaymentUrl = x.PaymentUrl,
            QrUrl = x.QrUrl,
            ShippingFee = x.ShippingFee,
            ProductFee = x.ProductFee,
            TotalFee = x.TotalFee,
            OrderTime = x.OrderTime,
            TagIds = x.TagIds,
            TaxCode = x.TaxCode,
            IdentityNumber = x.IdentityNumber,
            ReceiptFullName = x.ReceiptFullName,
            ReceiptPhoneNumber = x.ReceiptPhoneNumber,
            ReceiptAddress = x.ReceiptAddress,
            ReceiptEmail = x.ReceiptEmail,
            IsExportInvoice = x.IsExportInvoice,
            Vat = x.Vat,
            InvoiceType = x.InvoiceType,
            PaymentMethod = x.PaymentMethod,
            PaymentGateway = x.PaymentGateway,
            ShippingGateway = x.ShippingGateway,
            CreateAt = x.CreatedAt,
            UpdateAt = x.UpdatedAt,
        }).ToList();

        return ApiResponseFactory.BasePagination(result, pageIndex, pageSize, totalCount);
    }

    public async Task<Response.CalculateCardPriceResponse> CalculateCardPrice(Request.CalculateCardPriceRequest request)
    {
        if (request == null || request.CardTemplateId == Guid.Empty)
            throw new BadRequestException("CARD_INPUT_INVALID");

        var isAi = request.CardType == GeneratedCardType.Ai;
        var isHandmade = request.CardType == GeneratedCardType.Handmade;

        if (!isAi && !isHandmade)
            throw new BadRequestException("CARD_TYPE_INVALID");

        var template = await _dbContext.CardTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == request.CardTemplateId &&
                !x.IsDeleted);

        if (template == null || !template.IsActive)
            throw new BadRequestException("CARD_TEMPLATE_UNAVAILABLE");

        if ((isAi && template.TemplateType != CardTemplateType.Ai) ||
            (isHandmade && template.TemplateType != CardTemplateType.Handmade))
        {
            throw new BadRequestException("CARD_TEMPLATE_TYPE_INVALID");
        }

        var wordCount = string.IsNullOrWhiteSpace(request.MessageContent)
            ? 0
            : request.MessageContent
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Length;

        CardFormType formType;

        if (isHandmade)
        {
            // Handmade không được chọn size
            // FormType chỉ có thể là Calligraphy
            if (!string.IsNullOrWhiteSpace(request.SizeName) ||
                (request.FormType.HasValue &&
                 request.FormType != CardFormType.Calligraphy))
            {
                throw new BadRequestException("HANDMADE_CARD_INPUT_INVALID");
            }

            formType = CardFormType.Calligraphy;
        }
        else
        {
            // AI -> GoMay | Calligraphy và luôn phải có tên template
            if (request.FormType is not (
                    CardFormType.GoMay or CardFormType.Calligraphy))
            {
                throw new BadRequestException("CARD_FORM_TYPE_INVALID");
            }

            if (string.IsNullOrWhiteSpace(request.SizeName))
                throw new BadRequestException("CARD_SIZE_REQUIRED");

            formType = request.FormType.Value;
        }

        // ConfigHelper lấy field value dạng chuỗi từ wrapper Config.Value.
        var configs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "card_config");

        Response.CardSizePrice? size = null;

        if (isAi)//thì phải chọn size
        {
            var sizeJson = configs.GetValue<string>("card_config.sizes");

            if (string.IsNullOrWhiteSpace(sizeJson))
                throw new BadRequestException("CARD_SIZE_CONFIG_MISSING");

            Response.CardConfigItems<Response.CardSizePrice>? sizeConfig;

            try
            {
                sizeConfig =
                    System.Text.Json.JsonSerializer.Deserialize<Response.CardConfigItems<Response.CardSizePrice>>(sizeJson);
            }
            catch (System.Text.Json.JsonException)
            {
                throw new BadRequestException("CARD_SIZE_CONFIG_INVALID");
            }

            if (sizeConfig?.Items == null ||
                sizeConfig.Items.Count == 0 ||
                sizeConfig.Items.Any(x =>
                    string.IsNullOrWhiteSpace(x.Name) ||
                    x.Width <= 0 ||
                    x.Height <= 0 ||
                    x.BasePrice < 0 ||
                    x.MaxWords <= 0))
            {
                throw new BadRequestException("CARD_SIZE_CONFIG_INVALID");
            }
            //tìm kiếm thằng cùng size trong cái list -> ra đc chỉ 1 thằng
            var matchingSizes = sizeConfig.Items
                .Where(x =>
                    string.Equals(
                        x.Name,
                        request.SizeName!.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingSizes.Count != 1)
                throw new BadRequestException("CARD_SIZE_INVALID");

            size = matchingSizes[0];

            if (wordCount > size.MaxWords)
                throw new BadRequestException(
                    "CARD_MESSAGE_EXCEEDS_MAX_WORDS");
        }

        Response.CardWordPrice? priceRule = null;

        if (formType == CardFormType.Calligraphy)
        {
            var priceJson = configs.GetValue<string>("card_config.prices");

            if (string.IsNullOrWhiteSpace(priceJson))
                throw new BadRequestException("CARD_PRICE_CONFIG_MISSING");

            Response.CardConfigItems<Response.CardWordPrice>? priceConfig;

            try
            {
                priceConfig =
                    System.Text.Json.JsonSerializer.Deserialize<
                        Response.CardConfigItems<Response.CardWordPrice>>(
                        priceJson);
            }
            catch (System.Text.Json.JsonException)
            {
                throw new BadRequestException("CARD_PRICE_CONFIG_INVALID");
            }

            if (priceConfig?.Items == null ||
                priceConfig.Items.Count == 0 ||
                priceConfig.Items.Any(x =>
                    x.MinWords < 0 ||
                    x.MaxWords < x.MinWords ||
                    x.ExtraPrice < 0))
            {
                throw new BadRequestException("CARD_PRICE_CONFIG_INVALID");
            }

            var matchingRules = priceConfig.Items
                .Where(x =>
                    x.MinWords <= wordCount &&
                    wordCount <= x.MaxWords)
                .ToList();

            if (matchingRules.Count != 1)
            {
                throw new BadRequestException(
                    "CARD_PRICE_RULE_NOT_FOUND_OR_AMBIGUOUS");
            }

            priceRule = matchingRules[0];
        }

        var basePrice = size?.BasePrice ?? 0m;
        var extraPrice = priceRule?.ExtraPrice ?? 0m;
        var cardTypeName = isAi ? "ai" : "handmade";

        return new Response.CalculateCardPriceResponse
        {
            CardType = cardTypeName,

            FormType = formType == CardFormType.GoMay
                ? "go_may"
                : "calligraphy",

            CardTemplate = new Response.CardTemplatePrice
            {
                TemplateId = template.Id,
                TemplateName = template.Name,
                TemplateType = cardTypeName,
                TemplateImageUrl = template.ImageUrl
            },

            CardSize = size,
            CardConfig = priceRule,
            WordCount = wordCount,
            BasePrice = basePrice,
            ExtraPrice = extraPrice,
            TotalPrice = basePrice + extraPrice
        };
    }
}

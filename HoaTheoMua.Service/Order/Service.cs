
using System.Net.Http.Json;
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

    // VAT applied to the product subtotal when the buyer requests an invoice.
    private const decimal VatRate = 0.08m;

    public async Task<CreateOrderResponse> CreateOrder(Request.CreateOrderRequest request)
    {
        var userId = _currentUser.GetRequiredUserId();

        // Email-verification is now asserted by the token, not a Users-table lookup.
        if (!_currentUser.IsVerified)
        {
            throw new UnauthorizedException("User is not verified");
        }

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

        var itemIds = request.Items
            .Select(x => x.ItemId.ToString())
            .Distinct()
            .ToList();
        

        // var nhanhConfigs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        // var products = await FetchNhanhProductsAsync(request.Items, nhanhConfigs);

        var products = await FetchLocalProductsAsync(request.Items);

        // Requested quantity keyed by product id — reused for pricing and order details.
        var quantityByItemId = request.Items.ToDictionary(x => x.ItemId, x => x.Number);
        var productIdByItemId = request.Items.ToDictionary(x => x.ItemId, x => x.ProductId);

        // Every requested item must exist in the catalog before we check stock or shipping.
        var catalogIds = products.Select(x => x.Id).ToHashSet();
        if (request.Items.Any(x => !catalogIds.Contains(x.ItemId)))
        {
            throw new BadRequestException("Đơn hàng không tồn tại trong hệ thống");
        }

        var comboSnapshots = await EnsureItemsInStockAsync(request.Items);

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

        var totalProductFee = products.Sum(x => x.Prices.Retail * quantityByItemId[x.Id]);
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
            CreatedAt = DateTime.UtcNow,
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

        var newOrderDetails = products.Select(x =>
        {
            comboSnapshots.TryGetValue(x.Id, out var metadata);
            return new OrderDetails
            {
                Id = Guid.NewGuid(),
                OrderId = newOrder.Id,
                ProductId = productIdByItemId[x.Id],
                Name = x.Name,
                ItemId = x.Id,
                Price = (int)x.Prices.Retail,
                Number = quantityByItemId[x.Id],
                Height = x.Shipping.Height,
                Weight = x.Shipping.Weight,
                Length = x.Shipping.Length,
                Width = x.Shipping.Width,
                Metadata =
                     metadata == null
                     ? null
                     : JsonConvert.SerializeObject(metadata),

                CreatedAt = DateTime.UtcNow,
            };
        }).ToList();


        if (request.PaymentGateway == PaymentGateway.BaoKim)
        {
            var baoKimTokenResponse = await _baoKimService.CreateAccessToken();

            var createBaoKimOrderResponse = await _baoKimService.CreateOrder(
                baoKimTokenResponse.Data.AccessToken, newOrder);

            newOrder.PaymentUrl = createBaoKimOrderResponse.Data.PaymentUrl;

            await _dbContext.Orders.AddAsync(newOrder);
            await _dbContext.OrderDetails.AddRangeAsync(newOrderDetails);
            await _dbContext.SaveChangesAsync();

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

            await _dbContext.Orders.AddAsync(newOrder);
            await _dbContext.OrderDetails.AddRangeAsync(newOrderDetails);
            await _dbContext.SaveChangesAsync();

            return sepayReponse;
        }

        throw new BadRequestException("Not support other gateway");
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

    // Verifies every requested item has enough locally synchronized stock.
    private async Task<Dictionary<int, Request.OrderDetailMetadata>> EnsureItemsInStockAsync(List<AhaMoveRequest.Items> items)
    {
        var productIds = items
            .Select(x => x.ProductId)
            .ToHashSet();

        var specifications = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x =>
                productIds.Contains(x.SubId) &&
                !x.IsDeleted)
            .Select(x => new
            {
                x.SubId,
                x.MatId,
                x.IsCore,
                x.IsActive,
                MaterialIsDeleted = x.Mat.IsDeleted,
                MaterialName = x.Mat.ProductName,
                PriceMat = x.Mat.Price,
                Quantity = x.Quantity ?? 0m,
                x.Mat.NhanhProductId
            })
            .ToListAsync();

        var comboIds = specifications
            .Select(x => x.SubId)
            .ToHashSet();


        var materialSpecifications = specifications
            .Where(x =>
                x.IsActive &&
                !x.MaterialIsDeleted &&
                x.Quantity > 0)
            .ToLookup(x => x.SubId);

        var comboSnapshots = new Dictionary<int, Request.OrderDetailMetadata>();


        var stockItems = new List<(int ItemId, decimal Number)>();

        foreach (var item in items)
        {
            if (!comboIds.Contains(item.ProductId))
            {
                stockItems.Add((item.ItemId, item.Number));

                continue;
            }

            var coreMaterials = materialSpecifications[item.ProductId]
                .Where(x => x.IsCore)
                .ToList();

            if (coreMaterials.Count == 0)
            {
                throw new BadRequestException(
                    $"Combo ProductId={item.ItemId} không có vật liệu core hợp lệ");
            }

            var comboMaterials = materialSpecifications[item.ProductId]
                .ToList();

            if (comboMaterials.Count == 0)
            {
                throw new BadRequestException(
                    $"Combo ProductId={item.ItemId} không có vật liệu hợp lệ");
            }


            comboSnapshots[item.ItemId] = new Request.OrderDetailMetadata()
            {
                SubId = item.ProductId,
                Materials = comboMaterials
                    .Select(x => new Request.MaterialSnapshot()
                    {
                        MatId = x.MatId,
                        Quantity = x.Quantity,
                        Name = x.MaterialName ?? string.Empty,
                        Price = x.PriceMat,
                        IsCore = x.IsCore
                    })
                    .ToList()
            };


            foreach (var material in coreMaterials)
            {
                if (!int.TryParse(material.NhanhProductId, out var materialItemId))
                {
                    throw new BadRequestException(
                        $"Vật liệu MatId={material.MatId} không có NhanhProductId hợp lệ");
                }

                stockItems.Add((
                    materialItemId,
                    item.Number * material.Quantity));
            }
        }
        //step này: đã có được các ItemtId và số lượng cần mua của Item đó 
        var itemRequest = stockItems
            .GroupBy(x => x.ItemId)
            .Select(x => new
            {
                ItemId = x.Key,
                Number = x.Sum(item => item.Number)
            })
            .ToList();
        //map ra nó ItemId để vào product lấy ra được số lượng có thể bán
        // => map ra nhanhproductId với số lượng có thể bán của của ItemId đó
        // => Từ đó tạo ra thành một cái Dictionary vơi
        var nhanhProductIds = itemRequest
            .Select(x => x.ItemId.ToString())
            .ToList();

        var availableForSaleByItemId = await _dbContext.Products
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.IsActive &&
                x.NhanhProductId != null &&
                nhanhProductIds.Contains(x.NhanhProductId))
            .Select(x => new
            {
                x.NhanhProductId,
                AvailableForSale = x.AvailableForSale ?? 0m
            })
            .ToDictionaryAsync(
                x => x.NhanhProductId!,
                x => x.AvailableForSale);

        foreach (var item in itemRequest)
        {
            if (!availableForSaleByItemId.TryGetValue(
                    item.ItemId.ToString(),
                    out var availableForSale))
            {
                throw new BadRequestException(
                    $"Sản phẩm Id={item.ItemId} không tồn tại hoặc đã ngừng hoạt động");
            }

            if (item.Number > availableForSale)
            {
                throw new BadRequestException(
                    $"Không đủ hàng (Yêu cầu: {item.Number}, Kho có: {availableForSale})");
            }
        }

        return comboSnapshots;
    }

    // Calls Ahamove to estimate the shipping fee from the shop's warehouse to the buyer.
    private async Task<decimal> EstimateShippingFeeAsync(
        Request.CreateOrderRequest request,
        List<NhanhProductData> products,
        IReadOnlyDictionary<int, int> quantityByItemId,
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
        IReadOnlyDictionary<int, int> quantityByItemId,
        Repository.Entity.CompanyAddress? companyAddress)
    {
        var itemsRequest = products.Select(x => new AhaMoveRequest.AhamoveItem()
        {
            Id = x.Id.ToString(),
            Name = x.Name,
            Price = (int)x.Prices.Retail,
            Num = quantityByItemId[x.Id]
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
        IReadOnlyDictionary<int, int> quantityByItemId,
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
            : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
            : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
                    (product.Shipping.Weight, quantityByItemId[product.Id])))),
            orderTime,
            cancellationToken);

        return new Response.EstimateXanhSmShippingFeeResponse
        {
            ShippingFee = quote.TotalFee,
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
                PhoneNumber = companyAddress.PhoneNumber,
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
                PhoneNumber = dropOffPhone,
                Address = dropOffAddress,
                ShortAddress = dropOffShortAddress,
                AddressNote = dropOffNote,
                Latitude = (double)dropOffLatitude,
                Longitude = (double)dropOffLongitude
            }
        ];
    }

    internal static int CalculateXanhSmPackageWeightKg(
        IEnumerable<(double WeightInGrams, int Quantity)> packages)
    {
        var totalWeightInGrams = packages.Sum(package =>
            Math.Max(package.WeightInGrams, 0) * Math.Max(package.Quantity, 0));

        // Nhanh.vn/Ahamove product shipping weight is stored in grams while
        // GreenSM requires an integer total weight in kilograms.
        return Math.Max(1, (int)Math.Ceiling(totalWeightInGrams / 1000d));
    }

    public async Task<BaseResponse> GetOrderById(Guid id)
    {
        var isAdmin = _httpContext.HttpContext?.User.IsInRole("Admin") == true;
        var userId = isAdmin ? (Guid?)null : _currentUser.GetRequiredUserId();

        var orders = await _dbContext.Orders
            .Include(x => x.OrderDetails)
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
                    Snapshot = isAdmin && !string.IsNullOrWhiteSpace(x.Metadata)
                        ? JsonConvert.DeserializeObject<Request.OrderDetailMetadata>(x.Metadata)
                        : null,

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

    public async Task<string> GetStatusOrder(Guid id)
    {
        var userId = _currentUser.GetRequiredUserId();

        if (!_currentUser.IsVerified)
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

        if (!_currentUser.IsVerified)
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
}

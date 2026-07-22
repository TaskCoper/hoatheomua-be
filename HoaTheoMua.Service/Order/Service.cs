
using System.Net.Http.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.BaoKim;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Order;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly HttpClient _httpClient;
    private readonly HoaTheoMua.Service.Product.IService _serviceProduct;
    private readonly HoaTheoMua.Service.BaoKim.IService _baoKimService;
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly AhaMoveOption _ahaMoveOption;

    public Service(AppDbContext dbContext,
        ICurrentUserService currentUser,
        HttpClient httpClient,
        HoaTheoMua.Service.Product.IService serviceProduct,
        IConfiguration configuration, BaoKim.IService baoKimService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _httpClient = httpClient;
        _serviceProduct = serviceProduct;
        _baoKimService = baoKimService;
        _nhanhVnOption = new NhanhVnOption();
        _ahaMoveOption = new AhaMoveOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        configuration.GetSection(nameof(AhaMoveOption)).Bind(_ahaMoveOption);
    }


    public async Task<BasePaginationResponse> GetOrderPaginationResponse(int pageIndex, int pageSize)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var userId = _currentUser.GetRequiredUserId();

        var orders = _dbContext.Orders
            .Where(x => x.IsDeleted == false && x.UserId == userId);
        var totalCount = await orders.CountAsync();

        var items = await orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = items.Select(x => new Response.GetOrderResponse
        {
            OrderId = x.Id,
            Status = x.Status.ToString(),
            TrackNumber = x.TrackNumber,
            // NhanhOrderId = x.NhanhOrderId!,
            // NhanhTrackingUrl = x.NhanhTrackingUrl!,
            AhahaMoveOrderId = x.AhahaMoveOrderId!,
            AhahaMoveTrackingUrl = x.AhahaMoveTrackingUrl!,
            Remark = x.Remarks,
            OrderTime = x.OrderTime,
            Taxcode = x.TaxCode,
            TagsId = x.TagIds,
            IdentityNumber = x.IdentityNumber!,
            InvoiceType = x.InvoiceType!,
            IsExportInvoice = x.IsExportInvoice,
            ReceiptAddress = x.ReceiptAddress!,
            ReceiptEmail = x.ReceiptEmail!,
            ReceiptFullName = x.ReceiptFullName!,
            ReceiptPhoneNumber = x.ReceiptPhoneNumber!,
            CreateAt = x.CreatedAt,
            UpdateAt = x.UpdatedAt,
        }).ToList();

        return ApiResponseFactory.BasePagination(result, pageIndex, pageSize, totalCount);
    }

    // Nhanh.vn depot the shop ships from, used when the config value is absent.
    private const int DefaultDepotId = 129255;

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

        var nhanhConfigs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var depotId = nhanhConfigs.GetValue("nhanhvn.default_depot", DefaultDepotId);

        var products = await FetchNhanhProductsAsync(request.Items, nhanhConfigs);

        // Requested quantity keyed by product id — reused for pricing and order details.
        var quantityByItemId = request.Items.ToDictionary(x => x.ItemId, x => x.Number);

        // Every requested item must exist in the catalog before we check stock or shipping.
        var catalogIds = products.Select(x => x.Id).ToHashSet();
        if (request.Items.Any(x => !catalogIds.Contains(x.ItemId)))
        {
            throw new BadRequestException("Đơn hàng không tồn tại trong hệ thống");
        }

        await EnsureItemsInStockAsync(request.Items, depotId);

        var companyAddress = await _dbContext.CompanyAddresses
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync();

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

        var newOrderDetails = products.Select(x => new OrderDetails
        {
            Id = Guid.NewGuid(),
            OrderId = newOrder.Id,
            Name = x.Name,
            ItemId = x.Id,
            Price = (int)x.Prices.Retail,
            Number = quantityByItemId[x.Id],
            Height = x.Shipping.Height,
            Weight = x.Shipping.Weight,
            Length = x.Shipping.Length,
            Width = x.Shipping.Width,
            CreatedAt = DateTime.UtcNow,
        });

        var baoKimTokenResponse = await _baoKimService.CreateAccessToken();

        var createBaoKimOrderResponse = await _baoKimService.CreateOrder(
            baoKimTokenResponse.Data.AccessToken, newOrder);

        newOrder.PaymentUrl = createBaoKimOrderResponse.Data.PaymentUrl;

        await _dbContext.Orders.AddAsync(newOrder);
        await _dbContext.OrderDetails.AddRangeAsync(newOrderDetails);
        await _dbContext.SaveChangesAsync();

        return createBaoKimOrderResponse.Data;
    }

    // Fetches the catalog rows for the requested items from Nhanh.vn.
    private async Task<List<NhanhProductData>> FetchNhanhProductsAsync(
        List<AhaMoveRequest.Items> items,
        GroupConfig configs)
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

        using var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            throw new BadRequestException($"NhanhVn API HTTP Error ({response.StatusCode})");
        }

        var result = await response.Content.ReadFromJsonAsync<NhanhProductListResponse>();
        if (result == null || result.Data == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }

        return result.Data;
    }

    // Verifies every requested item has enough stock in the target depot.
    private async Task EnsureItemsInStockAsync(List<AhaMoveRequest.Items> items, int depotId)
    {
        foreach (var item in items)
        {
            var productDetailRequest = new HoaTheoMua.Service.Product.Request.ProductDetailRequest
            {
                Filters = new HoaTheoMua.Service.Product.Request.ProductDetailFilters
                {
                    Id = item.ItemId.ToString()
                }
            };

            var detail = await _serviceProduct.GetProductDetails(productDetailRequest);

            var json = JsonConvert.SerializeObject(detail);
            var depots = JObject.Parse(json)["Data"]?["Inventory"]?["Depots"] as JArray;

            if (depots == null || depots.Count == 0)
            {
                throw new BadRequestException($"Sản phẩm Id={item.ItemId} không có dữ liệu tồn kho");
            }

            var targetDepot = depots.FirstOrDefault(d => (int?)d["Id"] == depotId);
            if (targetDepot == null)
            {
                throw new BadRequestException($"Sản phẩm Id={item.ItemId} không có dữ liệu tồn kho tại depot {depotId}");
            }

            double available = (double?)targetDepot["Available"] ?? 0;
            if (item.Number > available)
            {
                throw new BadRequestException($"Không đủ hàng (Yêu cầu: {item.Number}, Kho có: {available})");
            }
        }
    }

    // Calls Ahamove to estimate the shipping fee from the shop's warehouse to the buyer.
    private async Task<decimal> EstimateShippingFeeAsync(
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

        return result[0].Data.TotalFee;
    }

    public async Task<BaseResponse> GetOrderById(Guid id)
    {
        var userId = _currentUser.GetRequiredUserId();

        var orders = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x =>
                x.IsDeleted == false &&
                x.UserId == userId &&
                x.Id == id);
        if (orders == null)
        {
            throw new NotFoundException("Order not found ");
        }

        var lastTransaction = orders.Transactions
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();

        var orderDetails = new Response.GetOrderByIdResponse
        {
            OrderId = orders.Id,
            Status = orders.Status,
            TrackNumber = orders.TrackNumber,
            TaxCode = orders.TaxCode,
            PaymentUrl = orders.PaymentUrl,
            PaymentMethod = orders.PaymentMethod,
            //NhanhOrderId = orders.NhanhOrderId,
            //NhanhTrackingUrl = orders.NhanhTrackingUrl,
            AhahaMoveOrderId = orders.AhahaMoveOrderId,
            AhahaMoveTrackingUrl = orders.AhahaMoveTrackingUrl,
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
            CreateAt = orders.CreatedAt,

            Details = orders.OrderDetails.Select(x => new Response.OrderDetailsResponse()
            {
                OrderDetailsId = x.Id,
                Name = x.Name,
                ItemId = x.ItemId,
                Number = x.Number,
                Price = x.Price,
                Weight = x.Weight,
                Length = x.Length,
                Width = x.Width,
                Height = x.Height,
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
        var isExistPendingOrder = await _dbContext.Orders.
            FirstOrDefaultAsync(x =>
                x.Id == id && x.Status == "Pending");

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
}
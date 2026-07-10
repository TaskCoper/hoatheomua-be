
using System.Net.Http.Json;
using FluentValidation;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.AhaMoveService;
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
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly AhaMoveOption _ahaMoveOption;
    private readonly IValidator<Request.CreateOrderRequest> _createOrderValidator;

    public Service(AppDbContext dbContext,
        ICurrentUserService currentUser,
        HttpClient httpClient,
        HoaTheoMua.Service.Product.IService serviceProduct,
        IValidator<Request.CreateOrderRequest> createOrderValidator,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _httpClient = httpClient;
        _serviceProduct = serviceProduct;
        _createOrderValidator = createOrderValidator;
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

    //------------
    public async Task<string> CreateOrder(Request.CreateOrderRequest request)
    {
        
        var result = await _createOrderValidator.ValidateAsync(request);
        if (!result.IsValid)
        {
            var errorMessage = string.Join(" | ", result.Errors.Select(e => e.ErrorMessage));
            throw new ValidationExceptionError(errorMessage);
        }

        var userId = _currentUser.GetRequiredUserId();

        // Email-verification is now asserted by the token, not a Users-table lookup.
        if (!_currentUser.IsVerified)
        {
            throw new UnauthorizedException("User is not verified");
        }

        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var depotId = configs.GetValue("nhanhvn.default_depot", 129255);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/product/list?appId=" + appId + "&businessId=" + businessId;
        var nhanhVnProductRequest = new NhanhVnRequest.NhanhProductListRequest()
        {
            Filters = new NhanhVnRequest.NhanhFilters()
            {
                Id = request.Items.Select(x => x.ItemId).ToList(),
                Status = [1, 2]
            },
            Paginator = new NhanhVnRequest.NhanhPaginator()
            {
                Size = 50,
                Sort = new Dictionary<string, string>()
                {
                    { "id", "desc" }
                },
                Next = new Dictionary<string, int>()
                {
                    { "id", 0 }
                },
            }
        };

        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhVnProductRequest);

        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        NhanhProductListResponse? productNhanhResult = null;

        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            productNhanhResult = await nhanhVnResponse.Content.ReadFromJsonAsync<NhanhProductListResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (productNhanhResult == null || productNhanhResult.Data == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }

        // foreach (var x in productNhanhResult.Data)
        // {
        //     var userItem = request.Items.First(item => item.ItemId == x.Id);
        //     //var targetAvailable = x.Inventory.Available;
        //     
        //     var targetAvailable =  _serviceProduct.GetProductDetails(userItem);
        //
        //     
        //     Console.WriteLine("Hhuhu: " + targetAvailable);
        //
        //     if (userItem.Number > targetAvailable)
        //     {
        //         throw new BadRequestException($"Sản phẩm '{x.Name}' không đủ hàng (Yêu cầu: {userItem.Number}, Kho có: {targetAvailable})");
        //     }
        //     
        //     
        // }

        foreach (var item in request.Items)
        {
            var productDetailRequest = new HoaTheoMua.Service.Product.Request.ProductDetailRequest
            {
                Filters = new HoaTheoMua.Service.Product.Request.ProductDetailFilters
                {
                    Id = item.ItemId.ToString()
                }
            };

            var objectItems = await _serviceProduct.GetProductDetails(productDetailRequest);

            var json = JsonConvert.SerializeObject(objectItems);
            var jObject = JObject.Parse(json);

            var depots = jObject["Data"]?["Inventory"]?["Depots"] as JArray;

            if (depots == null || depots.Count == 0)
            {
                throw new BadRequestException($"Sản phẩm Id={item.ItemId} không có dữ liệu tồn kho");
            }

            var targetDepot = depots.FirstOrDefault(d => (int?)d["Id"] == depotId);

            if (targetDepot == null)
            {
                throw new BadRequestException($"Sản phẩm Id={item.ItemId} không có dữ liệu tồn kho tại depot 129255");
            }

            double available = (double)targetDepot["Available"]!;

            if (item.Number > available)
            {
                throw new BadRequestException($"Không đủ hàng (Yêu cầu: {item.Number}, Kho có: {available})");
            }
        }

        var itemsRequest = productNhanhResult.Data.Select(
            x => new AhaMoveService.AhaMoveRequest.AhamoveItem()
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Price = (int)x.Prices.Retail,
                Num = request.Items.First(item => item.ItemId.Equals(x.Id)).Number
            }
        ).ToList();

        var packageDetailRequest = productNhanhResult.Data.Select(
            x => new AhaMoveService.AhaMoveRequest.PackageDetail()
            {
                Height = x.Shipping.Height,
                Weight = x.Shipping.Weight,
                Length = x.Shipping.Length,
                Width = x.Shipping.Width,
                Description = x.Name
            }
        ).ToList();

        var companyAddress = await _dbContext.CompanyAddresses
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync();
        
        var ahamoveConfigs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "ahamove");
        
        var createRequest = new AhaMoveService.AhaMoveRequest.AhaMoveShippingFeeRequest()
        {
            OrderTime = request.OrderTime,
            IdleUntil = request.OrderTime > 0 ? request.OrderTime : 0,
            // OrderTime = 0,
            Paths = new List<AhaMoveService.AhaMoveRequest.AhamoveLocationPoint>()
            {
                new AhaMoveService.AhaMoveRequest.AhamoveLocationPoint() // Thang nay cua SHop minh, Lay tu DB ra
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
                    Latitude =  request.Latitude,
                    Longitude =  request.Longitude,
                    Address = request.Address,
                    ShortAddress =  request.ShortAddress,
                    FullName =  request.FullName,
                    Phone =  request.Phone,
                    Remarks =  request.Remarks,
                },
            },
            PaymentMethod = "CASH",
            Remarks = request.Remarks,
            Items = itemsRequest,
            PackageDetail = packageDetailRequest,
            Services = new List<AhaMoveService.AhaMoveRequest.ServiceRequest>()
            {
                new AhaMoveService.AhaMoveRequest.ServiceRequest()
                {
                    Id = "SGN-BIKE",
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
                    }
                },
            }
        };
        Console.WriteLine("BUG CHA PA: " + createRequest.OrderTime);
        Console.WriteLine("BUG CHA PA: " + createRequest.IdleUntil);
        var ahamoveApi = ahamoveConfigs.GetValue("ahamove.base_url", _ahaMoveOption.BaseUrl) + "/v3/orders/estimates";
        var ahamoveToken = ahamoveConfigs.GetValue("ahamove.api_key", _ahaMoveOption.Token);
        var ahamoveHttpRequest = new HttpRequestMessage(HttpMethod.Post, ahamoveApi);
        ahamoveHttpRequest.Headers.Add("Authorization", ahamoveToken);
        ahamoveHttpRequest.Content = JsonContent.Create(createRequest);

        HttpResponseMessage ahamoveResponse = await _httpClient.SendAsync(ahamoveHttpRequest);

        List<ServiceResponse>? ahamoveResult = null;

        if (ahamoveResponse.IsSuccessStatusCode)
        {
            ahamoveResult = await ahamoveResponse.Content.ReadFromJsonAsync<List<ServiceResponse>>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"Ahamove API HTTP Error ({ahamoveResponse.StatusCode})");
        }

        if (ahamoveResult == null || ahamoveResult.Any() == false)
        {
            throw new BadRequestException("Ahamove returned null or empty data");
        }

        var totalShippingFee = ahamoveResult[0].Data.TotalFee;
        //var totalProductFee = productNhanhResult.Data.Select(x => x.Prices.Retail).Sum();
            var totalProductFee = productNhanhResult.Data.Sum(x =>
                x.Prices.Retail * request.Items.First(item => item.ItemId.Equals(x.Id)).Number);

            var vatFee = 0m;
            
            if (!string.IsNullOrEmpty(request.TaxCode))
            {
                vatFee = totalProductFee * 0.08m;
            }
            
            

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
            
                TagIds =  request.TagIds,
                TaxCode = request.TaxCode,
                IdentityNumber = request.IdentityNumber,
                ReceiptFullName = request.ReceiptFullName,
                ReceiptPhoneNumber = request.ReceiptPhoneNumber,
                ReceiptAddress = request.ReceiptAddress,
                ReceiptEmail = request.ReceiptEmail,
                IsExportInvoice = request.IsExportInvoice,
                InvoiceType =  request.InvoiceType,
            
            };


        var newOrderDetails = productNhanhResult.Data.Select(x => new OrderDetails
        {
            Id = Guid.NewGuid(),
            OrderId = newOrder.Id,
            Name = x.Name,
            ItemId = x.Id,
            Price = (int)x.Prices.Retail,
            Number = request.Items.First(item => item.ItemId.Equals(x.Id)).Number,
            Height = x.Shipping.Height,
            Weight = x.Shipping.Weight,
            Length = x.Shipping.Length,
            Width = x.Shipping.Width,
            CreatedAt = DateTime.UtcNow,
        });

        await _dbContext.Orders.AddAsync(newOrder);
        await _dbContext.OrderDetails.AddRangeAsync(newOrderDetails);
        await _dbContext.SaveChangesAsync();

        return newOrder.Id.ToString();
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

        // decimal totalAmount = itemAmount + shippingFee;
        decimal totalAmount = orders.TotalFee;

        string description = $"HTM-{orders.Id.ToString()}";
        string bankName = "BIDV";
        string bankAccount = "96247BENTRAN";
        string qrCodeUrl = $"https://qr.sepay.vn/img?" +
                           $"acc={bankAccount}&" +
                           $"bank={bankName}&" +
                           $"amount={totalAmount}&" +
                           $"des={description}&" +
                           $"template=qronly";

        var lastTransaction = orders.Transactions
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();

        var orderDetails = new Response.GetOrderByIdResponse
        {
            OrderId = orders.Id,
            Status = orders.Status,
            TrackNumber = orders.TrackNumber,
            TaxCode = orders.TaxCode,
            QrCodeUrl = qrCodeUrl,
            //NhanhOrderId = orders.NhanhOrderId,
            //NhanhTrackingUrl = orders.NhanhTrackingUrl,
            AhahaMoveOrderId = orders.AhahaMoveOrderId,
            AhahaMoveTrackingUrl = orders.AhahaMoveTrackingUrl,
            TagsId = orders.TagIds,
            IdentityNumber =  orders.IdentityNumber!,
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
            throw new BadRequestException($"VietQR API HTTP Error ({response.StatusCode})");        }

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
        var isExistPendingOrder =  await _dbContext.Orders.
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

using System.Net.Http.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Webhook;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Order;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly NhanhVnOption _nhanhVnOption = new();
    private readonly AhamoveOption _ahamoveAhamoveOption = new();
    private readonly IHttpContextAccessor _httpContext;
    private readonly HttpClient _httpClient;

    public Service(AppDbContext dbContext, 
        IHttpContextAccessor httpContext, 
        IConfiguration configuration, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        configuration.GetSection(nameof(AhamoveOption)).Bind(_ahamoveAhamoveOption);
        _httpClient = httpClient;
    }
    

    public async Task<BasePaginationResponse> GetOrderPaginationResponse(int pageIndex, int pageSize)  
    {        
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);
        
        var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
    
        if (user == null)  
        {            
            throw new UnauthorizedException("User not found");  
        }
        
        var userId = Guid.Parse(user);
        
    
        var orders = _dbContext.Orders
            .Where(x => x.IsDeleted == false && x.UserId == userId);
        var totalCount = await orders.CountAsync();
    
        var items = await orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    
        var result =  items.Select(x => new Response.GetOrderResponse
        {
            OrderId =  x.Id,
            Status = x.Status.ToString(),
            TrackNumber = x.TrackNumber,
            NhanhOrderId = x.NhanhOrderId!,
            NhanhTrackingUrl = x.NhanhTrackingUrl!,
            AhahaMoveOrderId = x.AhahaMoveOrderId!,
            AhahaMoveTrackingUrl = x.AhahaMoveTrackingUrl!,
            CreateAt = x.CreatedAt,
            UpdateAt = x.UpdatedAt,
        }).ToList();
        
        return ApiResponseFactory.BasePagination(result, pageIndex, pageSize, totalCount);
    }

    //------------
    public async Task<string> CreateOrder(Request.CreateOrderRequest request)
    {
        
        var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
  
        if (user == null)  
        {            
            throw new UnauthorizedException("User not found");  
        }
        var userId = Guid.Parse(user);
        
        var existUser = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsVerify);
        if (existUser == null)
        {
            throw new NotFoundException("User not found");
        }
        
        
        var appId = _nhanhVnOption.AppId;
        var businessId = _nhanhVnOption.BusinessId;
        var token = _nhanhVnOption.Token;
        var nhanhApi = _nhanhVnOption.BaseUrl + "/product/list?appId=" + appId + "&businessId=" + businessId;
        var nhanhVnProductRequest = new NhanhVnRequest.NhanhProductListRequest()
        {
            Filters = new NhanhVnRequest.NhanhFilters()
            {
                Id = request.Items.Select(x => x.ItemId).ToList(),
                Status = [1,2]
            },
            Paginator =  new NhanhVnRequest.NhanhPaginator()
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
            throw new Exception($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (productNhanhResult == null || productNhanhResult.Data == null)
        {
            throw new Exception("NhanhVnResponse returned null or empty data");
        }
        
        foreach (var x in productNhanhResult.Data)
        {
            var userItem = request.Items.First(item => item.ItemId == x.Id);
            var TargetAvailable = x.Inventory.Depots.First(d => d.Id == 129255).Available;

            if (userItem.Number > TargetAvailable)
            {
                throw new BadRequestException($"Sản phẩm '{x.Name}' không đủ hàng (Yêu cầu: {userItem.Number}, Kho có: {TargetAvailable})");
            }
        }
        var itemsRequest = productNhanhResult.Data.Select(
            x => new AhamoveService.Request.AhamoveItem()
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Price = (int)x.Prices.Retail,
                Num = request.Items.First(item => item.ItemId.Equals(x.Id)).Number
            }
        ).ToList();
        
        var packageDetailRequest = productNhanhResult.Data.Select(
            x => new AhamoveService.Request.PackageDetail()
            {
                Height = x.Shipping.Height,
                Weight = x.Shipping.Weight,
                Length = x.Shipping.Length,
                Width =  x.Shipping.Width,
                Description = "Noi that"
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
            Remarks =  request.Remarks,
            Items = itemsRequest,
            PackageDetail = packageDetailRequest,
            Services = new List<AhamoveService.Request.ServiceRequest>()
            {
                new AhamoveService.Request.ServiceRequest()
                {
                    Id = "SGN-BIKE",
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
                    }
                },
            }
        };
        
        var ahamoveApi =  _ahamoveAhamoveOption.BaseUrl +"/v3/orders/estimates";
        var ahamoveToken = _ahamoveAhamoveOption.Token;
        var ahamoveHttpRequest = new HttpRequestMessage(HttpMethod.Post, ahamoveApi);
        ahamoveHttpRequest.Headers.Add("Authorization", ahamoveToken);
        ahamoveHttpRequest.Content = JsonContent.Create(createRequest);
        
        HttpResponseMessage ahamoveResponse = await _httpClient.SendAsync(ahamoveHttpRequest);

        List<ServiceResponse>? ahamoveResult = null;
        
        if (ahamoveResponse.IsSuccessStatusCode)
        {
            ahamoveResult = await ahamoveResponse.Content.ReadFromJsonAsync<List<ServiceResponse>>();
        } else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new Exception($"Ahamove API HTTP Error ({ahamoveResponse.StatusCode})");
        }
        
        if (ahamoveResult == null || ahamoveResult.Any() == false)
        {
            throw new Exception("Ahamove returned null or empty data");
        }

        var totalShippingFee = ahamoveResult[0].Data.TotalPrice;
        var totalProductFee = productNhanhResult.Data.Select(x => x.Prices.Retail).Sum();
        
        var newOrder = new Repository.Entity.Order()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = "Pending",
            TrackNumber = "",
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = request.Address,
            Ward =  request.Ward,
            City =  request.City,
            ShortAddress = request.ShortAddress,
            FullName = request.FullName,
            Phone = request.Phone,
            Remarks = request.Remarks,
            CreatedAt = DateTime.UtcNow,
            ShippingFee = totalShippingFee,
            ProductFree = totalProductFee,
            TotalFee = totalProductFee +  totalShippingFee,
            
        };

        
        var newOrderDetails = productNhanhResult.Data.Select(x => new OrderDetails
        {
            Id =  Guid.NewGuid(),
            OrderId = newOrder.Id,
            Name = x.Name,
            ItemId = x.Id,
            Price = (int)x.Prices.Retail,
            Number = request.Items.First(item => item.ItemId.Equals(x.Id)).Number,
            Height = x.Shipping.Height,
            Weight = x.Shipping.Weight,
            Length = x.Shipping.Length,
            Width = x.Shipping.Width,
        });
        
        await _dbContext.Orders.AddAsync(newOrder);
        await _dbContext.OrderDetails.AddRangeAsync(newOrderDetails);
        await _dbContext.SaveChangesAsync();
        
        return newOrder.Id.ToString();
    }

    public async Task<BaseResponse> GetOrderById(Guid id)
    {
        
        var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
  
        if (user == null)  
        {            
            throw new UnauthorizedException("User not found");  
        }
        
        var userId = Guid.Parse(user);
        

        var orders = await _dbContext.Orders
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
            QrCodeUrl = qrCodeUrl,
            NhanhOrderId = orders.NhanhOrderId,
            NhanhTrackingUrl = orders.NhanhTrackingUrl,
            AhahaMoveOrderId = orders.AhahaMoveOrderId,
            AhahaMoveTrackingUrl = orders.AhahaMoveTrackingUrl,

            Transaction = lastTransaction == null ? null : new Response.OrderTransactionResponse
            {
                TransferAmount =  0,
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
        var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
  
        if (user == null)  
        {            
            throw new UnauthorizedException("User not found");  
        }
        var userId = Guid.Parse(user);
        
        var existUser = await _dbContext.Users
            .FirstOrDefaultAsync(x => 
                x.Id == userId && x.IsVerify);
        if (existUser == null)
        {
            throw new NotFoundException("User not found");
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

    public async Task<OrderListRootResponse?> GetOrderNhanhVn(NhanhVnOrderListRequest.OrderListRequest request)
    {
        // var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
        //
        // if (user == null)  
        // {            
        //     throw new UnauthorizedException("User not found");  
        // }
        // var userId = Guid.Parse(user);
        //
        // var existUser = await _dbContext.Users
        //     .FirstOrDefaultAsync(x => x.Id == userId && x.IsVerify);
        // if (existUser == null)
        // {
        //     throw new NotFoundException("User not found");
        // }
        
        
        var appId = _nhanhVnOption.AppId;
        var businessId = _nhanhVnOption.BusinessId;
        var token = _nhanhVnOption.Token;
        var nhanhApi = _nhanhVnOption.BaseUrl + "/order/list?appId=" + appId + "&businessId=" + businessId;
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
            throw new Exception($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (orderListRootResponse == null)
        {
            throw new Exception("NhanhVnResponse returned null or empty data");
        }

        return orderListRootResponse;
    }

    public async Task<Response.NhanhVnLocationResponse> GetShippingLocation(Request.LocationFilters filter)
    {
        var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
  
        if (user == null)  
        {            
            throw new UnauthorizedException("User not found");  
        }
        var userId = Guid.Parse(user);
        
        var existUser = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsVerify);
        if (existUser == null)
        {
            throw new NotFoundException("User not found");
        }
        
        var appId = _nhanhVnOption.AppId;
        var businessId = _nhanhVnOption.BusinessId;
        var token = _nhanhVnOption.Token;
        var nhanhApi = _nhanhVnOption.BaseUrl + "/shipping/location?appId=" + appId + "&businessId=" + businessId;
        var nhanhVnOrderSrcRequest = new Request.LocationRequest()
        {
            Filters = new Request.LocationFilters()
            {
                LocationVersion = filter.LocationVersion,
                Type = filter.Type,
                ParentId = filter.ParentId,
            }
        };
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhVnOrderSrcRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        Response.NhanhVnLocationResponse? shippingLocation = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            shippingLocation = await nhanhVnResponse.Content.ReadFromJsonAsync< Response.NhanhVnLocationResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new Exception($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (shippingLocation == null)
        {
            throw new Exception("NhanhVnResponse returned null or empty data");
        }

        return shippingLocation;
    }
}
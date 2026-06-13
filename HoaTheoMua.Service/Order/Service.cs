
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Order;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }


    // public async Task<Response.CreateOrderResponse> CreateOrderTransport(Request.CreateOrderRequest request)
    // {
    //     
    //     var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
    //
    //     if (user == null)  
    //     {            
    //         throw new UnauthorizedException("User not found");  
    //     }
    //     var userId = Guid.Parse(user);
    //     
    //     var existUser = await _dbContext.Users
    //         .FirstOrDefaultAsync(x => x.Id == userId && x.IsVerify);
    //     if (existUser == null)
    //     {
    //         throw new NotFoundException("User not found");
    //     }
    //     
    //     decimal itemAmount = 0;
    //     decimal shippingFee = 0;
    //
    //     string raw;
    //     if (request.Metadata is System.Text.Json.JsonElement je)
    //         raw = je.GetRawText(); 
    //     else if (request.Metadata is string s)
    //         raw = s;
    //     else
    //         raw = JsonConvert.SerializeObject(request.Metadata);
    //
    //     var metadataJson = JObject.Parse(raw);
    //     itemAmount  = metadataJson["subTotal"]?.Value<decimal>()    ?? 0;
    //     shippingFee = metadataJson["shippingFee"]?.Value<decimal>() ?? 0;
    //     
    //     
    //     decimal totalAmount = itemAmount + shippingFee;
    //
    //     var newOrder = new Repository.Entity.Order()
    //     {
    //         UserId = userId,
    //         Status = "Pending",
    //         TrackNumber = "",
    //         Metadata = request.Metadata.ToString(),
    //         CreatedAt = DateTime.UtcNow,
    //     };
    //     
    //     await _dbContext.Orders.AddAsync(newOrder);
    //     await _dbContext.SaveChangesAsync();
    //
    //     string description = $"HOATHEOMUA{newOrder.Id:N}";
    //     string bankName = "MBBank";
    //     string bankAccount = "0384875317";
    //     string qrCodeUrl = $"https://qr.sepay.vn/img?" +
    //                        $"acc={bankAccount}&" +
    //                        $"bank={bankName}&" +
    //                        $"amount={totalAmount}&" +
    //                        $"des={description}&" +
    //                        $"template=qronly";
    //     
    //     var result = new Response.CreateOrderResponse()
    //     {
    //         QrCodeUrl =  qrCodeUrl,
    //     };
    //
    //     return result;
    // }

    // public async Task<string> CheckingOrder(Guid orderId)
    // {
    //     var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
    //
    //     if (user == null)  
    //     {            
    //         throw new UnauthorizedException("User not found");  
    //     }
    //     var userId = Guid.Parse(user);
    //     
    //     var existUser = await _dbContext.Users
    //         .FirstOrDefaultAsync(x => 
    //             x.Id == userId && x.IsVerify);
    //     if (existUser == null)
    //     {
    //         throw new NotFoundException("User not found");
    //     }
    //     
    //     var order = await _dbContext.Orders
    //         .FirstOrDefaultAsync(o => 
    //             o.Id == orderId && o.UserId == userId);
    //     if (order == null)
    //     {
    //         throw new NotFoundException("Order ID " + orderId + " not found");
    //     }
    //     
    //     return order.Status;
    // }

    // public async Task<string> UpdateOrder(Request.UpdateOrderRequest request)
    // {
    //     var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
    //
    //     if (user == null)  
    //     {            
    //         throw new UnauthorizedException("User not found");  
    //     }
    //     
    //     var userId = Guid.Parse(user);
    //     
    //
    //     var orders = await _dbContext.Orders
    //         .FirstOrDefaultAsync(x => 
    //             x.IsDeleted == false && 
    //             x.UserId == userId &&
    //             x.Id == request.OrderId);
    //     if (orders == null)
    //     {
    //         throw new NotFoundException("Order ID " + request.OrderId + " not found");
    //     }
    //
    //     orders.TrackNumber = request.TrackNumber;
    //     orders.UpdatedAt = DateTime.UtcNow;
    //     await _dbContext.SaveChangesAsync();
    //     return "Cập nhật thành công";
    // }

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
        
        var newOrder = new Repository.Entity.Order()
        {
            UserId = userId,
            Status = "Pending",
            TrackNumber = "",
            Metadata = request.Metadata.ToString(),
            CreatedAt = DateTime.UtcNow,
        };
        await _dbContext.Orders.AddAsync(newOrder);
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
        
        decimal itemAmount = 0;
        decimal shippingFee = 0;
        string rawMeta = orders.Metadata ?? string.Empty;
        
        try
        {
            if (rawMeta.StartsWith("\"") && rawMeta.EndsWith("\""))
            {
                rawMeta = JsonConvert.DeserializeObject<string>(rawMeta) ?? rawMeta;
            }
            var metadataJson = JObject.Parse(rawMeta);
            itemAmount = metadataJson["subTotal"]?.Value<decimal>() ?? 0;
            shippingFee = metadataJson["shippingFee"]?.Value<decimal>() ?? 0;
        }
        catch
        {
            //
        }
        
        decimal totalAmount = itemAmount + shippingFee;

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
            Metadata = rawMeta,
            NhanhOrderId = orders.NhanhOrderId!,
            NhanhTrackingUrl = orders.NhanhTrackingUrl!,
            AhahaMoveOrderId = orders.AhahaMoveOrderId!,
            AhahaMoveTrackingUrl = orders.AhahaMoveTrackingUrl!,

            Transaction = lastTransaction == null ? null : new Response.OrderTransactionResponse
            {
                TransferAmount =  itemAmount,
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

    public async Task<string> UpdateOrderById(Guid id, Request.UpdateOrderByIdRequest request)
    {
        var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
  
        if (user == null)  
        {            
            throw new UnauthorizedException("User not found");  
        }
        
        var userId = Guid.Parse(user);
        

        var orders = await _dbContext.Orders
            .FirstOrDefaultAsync(x => 
                x.IsDeleted == false && 
                x.UserId == userId &&
                x.Id == id);
        if (orders == null)
        {
            throw new NotFoundException("Order ID " + id + " not found");
        }

        string raw;
        if (request.Metadata is System.Text.Json.JsonElement je)
            raw = je.GetRawText(); 
        else if (request.Metadata is string s)
            raw = s;
        else
            raw = JsonConvert.SerializeObject(request.Metadata);

        orders.TrackNumber = request.TrackingNumber;
        orders.Metadata = raw;
        orders.NhanhOrderId = request.NhanhOrderId;
        orders.AhahaMoveOrderId = request.AhahaMoveOrderId;
        orders.NhanhTrackingUrl = request.NhanhTrackingUrl;
        orders.AhahaMoveTrackingUrl = request.AhahaMoveTrackingUrl;
        orders.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return "Cập nhật thành công";
    }
}
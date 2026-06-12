
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Order;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    
    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }


    public async Task<Response.CreateOrderResponse> CreateOrderTransport(Request.CreateOrderRequest request)
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
        
        decimal itemAmount = 0;
        decimal shippingFee = 0;

        try
        {
            var metadataJson = JObject.FromObject(request.Metadata);
            itemAmount = metadataJson["subTotal"] != null ? (decimal)metadataJson["subTotal"] : 0;
            shippingFee = metadataJson["shippingFee"] != null ? (decimal)metadataJson["shippingFee"] : 0;
        }
        catch (Exception)
        {
            throw new BadRequestException("Metadata FE gửi không có field mà FE mong chờ :((");
        }
        
        
        decimal totalAmount = itemAmount + shippingFee;

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

        string description = $"HOATHEOMUA-{newOrder.Id:N}";
        string bankName = "MBBank";
        string bankAccount = "0384875317";
        string qrCodeUrl = $"https://qr.sepay.vn/img?" +
                           $"acc={bankAccount}&" +
                           $"bank={bankName}&" +
                           $"amount={totalAmount}&" +
                           $"des={description}&" +
                           $"template=qronly";
        
        var result = new Response.CreateOrderResponse()
        {
            QrCodeUrl =  qrCodeUrl,
        };

        return result;
    }

    public async Task<string> CheckingOrder(Guid orderId)
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
                o.Id == orderId && o.UserId == userId);
        if (order == null)
        {
            throw new NotFoundException("Order ID " + orderId + " not found");
        }
        
        return order.Status;
    }

    public async Task<string> UpdateOrder(Request.UpdateOrderRequest request)
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
                x.Id == request.OrderId);
        if (orders == null)
        {
            throw new NotFoundException("Order ID " + request.OrderId + " not found");
        }

        orders.TrackNumber = request.TrackNumber;
        orders.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return "Cập nhật thành công";
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
        }).ToList();
        
        return ApiResponseFactory.BasePagination(result, pageIndex, pageSize, totalCount);
    }
    
}
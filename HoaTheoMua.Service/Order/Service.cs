
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


    public async Task<Response.CreateOrderResponse> ProcessOrderTransport(Request.CreateOrderRequest request)
    {
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
            UserId = request.UserId,
            Status = "Pending",
            TrackNumber = totalAmount,
            Metadata = request.Metadata.ToString(),
            CreatedAt = DateTime.Now,
        };
        
        await _dbContext.Orders.AddAsync(newOrder);
        await _dbContext.SaveChangesAsync();

        string description = $"HOATHEOMUA-{newOrder.Id}";
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
            OrderId = newOrder.Id,
            TotalAmount =  totalAmount,
            QrCodeUrl =  qrCodeUrl,
        };

        return result;
    }

    public async Task<string> CheckingOrder(Guid orderId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            throw new NotFoundException("Order ID " + orderId + " not found");
        }
        
        return order.Status;
    }

    public async Task<BasePaginationResponse> GetOrderPaginationResponse(int pageIndex, int pageSize)  
    {        
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
            
        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }
    
}
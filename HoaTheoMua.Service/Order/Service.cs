using System.Runtime.InteropServices.JavaScript;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Order;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<Response.CreateOrderResponse> ProcessOrderTransport(Request.CreateOrderRequest request)
    {
        decimal itemAmount = 0;
        decimal shippingFee = 0;

        try
        {
            var metadataJson = JObject.FromObject(request.Metadata);
            itemAmount = metadataJson["sub_total"] != null ? (decimal)metadataJson["sub_total"] : 0;
            shippingFee = metadataJson["shipping_fee"] != null ? (decimal)metadataJson["shipping_fee"] : 0;
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

        string description = $"HOATHEOMUA-{newOrder.UserId}";
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
        
        
        throw new NotImplementedException();
    }
}
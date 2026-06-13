using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Webhook;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task SepayWebhookHandler(Request.SepayWebhookRequest request)
    {
        var description = request.Code;
        var raw = description.Replace("HOATHEOMUA", "");
        
        Guid? orderId = null;
        if (raw.Length == 32) 
        {
            var formatted = 
                $"{raw.Substring(0, 8)}-" +
                $"{raw.Substring(8, 4)}-" +
                $"{raw.Substring(12, 4)}-" +
                $"{raw.Substring(16, 4)}-" +
                $"{raw.Substring(20, 12)}";
            if (Guid.TryParse(formatted, out var guid))
            {
                orderId = guid;
            }
        }
        else
        {
            throw new BadRequestException("Invalid description format");
        }
        
        if (orderId == null)
        {
            throw new NotFoundException("Order not found in description");
        }
        
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            throw new NotFoundException("Order not found in description");
        }
        
        if (order.Status != "Pending")
        {
            throw new NotFoundException("Order not on Pending");;
        }
        
        decimal itemAmount = 0;
        decimal shippingFee = 0;
        
        var metadataJson = JObject.Parse(order.Metadata);
        itemAmount = metadataJson["subTotal"] != null ? (decimal)metadataJson["subTotal"]! : 0;
        shippingFee = metadataJson["shippingFee"] != null ? (decimal)metadataJson["shippingFee"]! : 0;
        
        decimal totalAmount = itemAmount + shippingFee;
        
        if (totalAmount != request.TransferAmount)
        {
            throw new ConflictException("Invalid transfer amount");
        }
        
        order.Status = "Paid";
        order.UpdatedAt = DateTimeOffset.UtcNow;
        _dbContext.Orders.Update(order);
        
        var transactionEntity = new Transaction()
        {
            SePayId = request.Id,
            Gateway = request.Gateway,
            TransactionDate = DateTime.TryParse(request.TransactionDate, out DateTime parsedDate) ? parsedDate.ToUniversalTime() : DateTime.UtcNow,
            AccountNumber = request.AccountNumber,
            SubAccount = request.SubAccount,
            Code = request.Code,
            Content = request.Content,
            TransferType = request.TransferType,
            Description = request.Description,
            TransferAmount = request.TransferAmount,
            Accumulated = request.Accumulated,
            ReferenceCode = request.ReferenceCode,
            
            OrderId = order.Id, 
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.Transactions.Add(transactionEntity);
        
        await _dbContext.SaveChangesAsync();
        
    }
}
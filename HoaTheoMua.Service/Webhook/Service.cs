using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Webhook;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SepayWebhookHandler(Request.SepayWebhookRequest request)
    {
        // SePay puts the parsed code in Code, but only when a payment-code pattern is
        // configured; otherwise fall back to the raw transfer memo in Content.
        var source = !string.IsNullOrWhiteSpace(request.Code) ? request.Code : request.Content;
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new BadRequestException("Invalid description format");
        }

        // Match "HTM" (optional separator) + a GUID. The bank/SePay strips dashes, so the
        // GUID can arrive either as 32 hex chars or in full dashed form, possibly surrounded
        // by other transfer text (reference numbers, account numbers, etc.).
        var match = Regex.Match(
            source,
            @"HTM[-_]?([0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12})",
            RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            throw new BadRequestException("Invalid description format");
        }

        var hex = match.Groups[1].Value.Replace("-", "");
        if (!Guid.TryParseExact(hex, "N", out var orderId))
        {
            throw new BadRequestException("Invalid description format");
        }

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            throw new NotFoundException("Order not found in description");
        }

        if (order.Status != "Pending")
        {
            throw new NotFoundException("Order not on Pending"); ;
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
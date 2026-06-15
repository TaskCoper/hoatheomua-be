using System.Text;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace HoaTheoMua.Service.Webhook;

public partial class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Match "HTM" (optional separator) + a GUID. The bank/SePay strips dashes, so the GUID can
    // arrive either as 32 hex chars or in full dashed form, possibly surrounded by other text.
    [GeneratedRegex(
        @"HTM[-_]?([0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12})",
        RegexOptions.IgnoreCase)]
    private static partial Regex PaymentCodeRegex();

    private static bool TryExtractOrderId(string? value, out Guid orderId)
    {
        orderId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var match = PaymentCodeRegex().Match(value);
        if (!match.Success)
        {
            return false;
        }

        var hex = match.Groups[1].Value.Replace("-", "");
        return Guid.TryParseExact(hex, "N", out orderId);
    }

    // Same prefix as PaymentCodeRegex but matches a partial run of hex (with optional dashes),
    // used to recover the order when SePay truncates the code below a full 32-char GUID.
    [GeneratedRegex(@"HTM[-_]?([0-9a-fA-F][0-9a-fA-F-]*)", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentCodePrefixRegex();

    // Minimum hex chars before a prefix is unique enough to safely resolve a single order.
    private const int MinPrefixLength = 24;

    private static bool TryExtractOrderIdPrefix(string? value, out string dashedPrefix)
    {
        dashedPrefix = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var match = PaymentCodePrefixRegex().Match(value);
        if (!match.Success)
        {
            return false;
        }

        // Postgres renders uuid as lowercase hex with dashes; normalize to match Id::text.
        var hex = match.Groups[1].Value.Replace("-", "").ToLowerInvariant();

        // Full GUIDs are handled by TryExtractOrderId; only act on genuinely truncated codes.
        if (hex.Length < MinPrefixLength || hex.Length >= 32)
        {
            return false;
        }

        var sb = new StringBuilder(hex.Length + 4);
        for (var i = 0; i < hex.Length; i++)
        {
            if (i is 8 or 12 or 16 or 20)
            {
                sb.Append('-');
            }
            sb.Append(hex[i]);
        }

        dashedPrefix = sb.ToString();
        return true;
    }

    public async Task SepayWebhookHandler(Request.SepayWebhookRequest request)
    {
        // SePay's parsed Code can be truncated (it sometimes drops trailing chars), so prefer the
        // raw transfer memo in Content, which carries the full code, and fall back to Code.
        Repository.Entity.Order? order;

        if (TryExtractOrderId(request.Content, out var orderId) ||
            TryExtractOrderId(request.Code, out orderId))
        {
            order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        }
        else if (TryExtractOrderIdPrefix(request.Content, out var prefix) ||
                 TryExtractOrderIdPrefix(request.Code, out prefix))
        {
            // The code was truncated below a full GUID (e.g. "HTMd950e85c1a9b4a22947b2c8e0185e8",
            // missing the final 2 hex chars). Resolve the order by matching the GUID prefix.
            var matches = await _dbContext.Orders
                .Where(o => o.Id.ToString().StartsWith(prefix))
                .Take(2)
                .ToListAsync();

            if (matches.Count > 1)
            {
                throw new ConflictException("Ambiguous payment code");
            }

            order = matches.FirstOrDefault();
        }
        else
        {
            throw new BadRequestException("Invalid description format");
        }

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
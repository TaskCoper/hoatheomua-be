using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Fulfillment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace HoaTheoMua.Service.Webhook;

public partial class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly SepayOption _sepayOption;
    private readonly Fulfillment.IService _fulfillmentService;
    private readonly IPublishEndpoint _publishEndpoint;

    public Service(AppDbContext dbContext, IConfiguration configuration,
        Fulfillment.IService fulfillmentService, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _fulfillmentService = fulfillmentService;
        _publishEndpoint = publishEndpoint;
        _sepayOption = new SepayOption();
        configuration.GetSection(nameof(SepayOption)).Bind(_sepayOption);
    }

    // SePay signs every webhook call with "Authorization: Apikey <ApiKey>". Fail closed: without a
    // configured key we cannot tell a genuine callback from a forged one, and a forged one flips an
    // order to Paid and dispatches a courier for free.
    private const string SepayAuthorizationScheme = "Apikey";

    public void VerifySepayApiKey(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(_sepayOption.ApiKey))
        {
            throw new ServerException(
                "SepayOption.ApiKey is not configured, so webhook requests cannot be verified");
        }

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            throw new UnauthorizedException("Missing Sepay Authorization header");
        }

        var parts = authorizationHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2
            || !string.Equals(parts[0], SepayAuthorizationScheme, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Invalid Sepay Authorization header");
        }

        var providedKeyBytes = Encoding.UTF8.GetBytes(parts[1]);
        var expectedKeyBytes = Encoding.UTF8.GetBytes(_sepayOption.ApiKey);

        // Lengths must match before FixedTimeEquals: it throws on mismatched-length inputs rather
        // than returning false, and comparing length first still leaks nothing an attacker can use
        // (key length isn't the secret).
        if (providedKeyBytes.Length != expectedKeyBytes.Length
            || !CryptographicOperations.FixedTimeEquals(providedKeyBytes, expectedKeyBytes))
        {
            throw new UnauthorizedException("Invalid Sepay API key");
        }
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
            order = await _dbContext.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);
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

        if (order.PaymentGateway != PaymentGateway.Sepay)
        {
            throw new ConflictException("Payment gateway does not match SePay webhook");
        }

        // A redelivery of an order we already handled is expected traffic, not an error. BaoKim's
        // handler has always acked these; this one used to throw 404 forever instead.
        if (order.Status is PaidStatus or Fulfillment.Service.FulfillmentFailedStatus)
        {
            return;
        }

        if (order.Status != PendingStatus)
        {
            throw new ConflictException($"Order is not Pending (current: {order.Status})");
        }

        if (order.TotalFee != request.TransferAmount)
        {
            throw new ConflictException("Transfer amount must be equal to transfer amount");
        }

        var transactionEntity = new Transaction()
        {
            // SePay has no request_id of its own, so its transaction id stands in as the gateway's
            // identifier for this event.
            RequestId = request.Id.ToString(),
            Gateway = request.Gateway,
            // SePay only ever reaches us via the VNPay QR bank transfer.
            paymentMethod = PaymentMethod.VNPayQR,
            TransactionDate = DateTime.TryParse(request.TransactionDate, out DateTime parsedDate)
                ? parsedDate.ToUniversalTime()
                : DateTime.UtcNow,
            AccountNumber = request.AccountNumber,
            SubAccount = request.SubAccount ?? "",
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

        await FulfillPaidOrder(order, transactionEntity);
    }

    public async Task BaoKimWebHookHandler(BaoKimPaymentCallback request)
    {
        var paymentResult = request.PaymentResult;
        if (paymentResult?.Order == null)
        {
            throw new BadRequestException("Missing payment_result.order");
        }

        var baoKimOrder = paymentResult.Order;

        // We send our own Order.Id as mrc_order_id when creating the BaoKim order (see
        // BaoKim.Service.CreateOrder), so it comes back verbatim — none of the memo-parsing
        // guesswork SePay needs.
        if (!Guid.TryParse(baoKimOrder.MrcOrderId, out var orderId))
        {
            throw new BadRequestException("Invalid mrc_order_id format");
        }

        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException("Order not found for mrc_order_id");
        }

        if (order.PaymentGateway != PaymentGateway.BaoKim)
        {
            throw new ConflictException("Payment gateway does not match BaoKim webhook");
        }

        // Anything that isn't a completed payment (failed, cancelled, still pending at the gateway)
        // is acknowledged and dropped: the order stays Pending, and redelivering it would not
        // change the outcome.
        if (paymentResult.ResultCode != BaoKim.BaoKimConstant.CallbackSuccessResultCode
            || baoKimOrder.Status != BaoKim.BaoKimConstant.CallbackCompletedStatus)
        {
            return;
        }

        // BaoKim redelivers until it gets an ack, so a replay of an order we already fulfilled is
        // expected traffic rather than an error — ack it and skip fulfilment instead of throwing,
        // which would otherwise re-run the Ahamove/NhanhVn calls or spin the retry schedule.
        if (order.Status is PaidStatus or Fulfillment.Service.FulfillmentFailedStatus)
        {
            return;
        }

        if (order.Status != PendingStatus)
        {
            throw new ConflictException($"Order is not Pending (current: {order.Status})");
        }

        // CreateOrder sends total_amount as (long)order.TotalFee, so compare on that same
        // truncation — comparing the raw decimal would reject a correct payment.
        if ((long)order.TotalFee != (long)baoKimOrder.TotalAmount)
        {
            throw new ConflictException("Paid amount does not match the order total");
        }

        // `transaction` is a list because BaoKim can split a payment; the completed entry is the one
        // that actually settled. The null-conditionals are load-bearing: an explicit
        // "transaction": null in the payload overwrites the property initializer with null.
        var baoKimTransaction =
            paymentResult.Transaction?.FirstOrDefault(
                t => t.Status == BaoKim.BaoKimConstant.CallbackCompletedStatus)
            ?? paymentResult.Transaction?.FirstOrDefault();

        var baoKimTransactionEntity = new Transaction()
        {
            // BaoKim's own id for this callback. Alphanumeric ("B2B6A278C9815BD7EQ4AVPVZCI"), which
            // is why the column is text rather than the long SePay used.
            RequestId = request.RequestId,
            Gateway = "BaoKim",
            // BaoKim reports how the buyer actually paid. Guard the cast: an int outside the enum
            // would otherwise be persisted as a meaningless value. We always request VA in
            // CreateOrder, so that is the honest fallback.
            paymentMethod = Enum.IsDefined(typeof(PaymentMethod), baoKimOrder.PaymentMethod)
                ? (PaymentMethod)baoKimOrder.PaymentMethod
                : PaymentMethod.VA,
            TransactionDate = ToUtcOrNow(baoKimTransaction?.CompletedAt ?? baoKimOrder.CompletedAt),
            // BaoKim settles into a virtual account it owns and reports no bank account back, so
            // these SePay-shaped columns have nothing to carry (they are NOT NULL).
            AccountNumber = string.Empty,
            SubAccount = string.Empty,
            Code = baoKimOrder.MrcOrderId,
            Content = paymentResult.ResultMessage ?? string.Empty,
            TransferType = "in",
            Description = baoKimTransaction?.Description ?? paymentResult.ResultMessage ?? string.Empty,
            // The gross the buyer paid — deliberately NOT transaction.amount, which is the net
            // settlement after fees (amount = total_amount - merchant_fee - user_fee) and can even
            // be negative: a real 160,000 payment carrying a 337,600 merchant_fee reported
            // amount = -177,600. The docs' sample happens to have zero merchant_fee, which makes
            // amount look like the order total and hides this.
            TransferAmount = baoKimOrder.TotalAmount,
            Accumulated = 0,
            ReferenceCode = baoKimOrder.Id.ToString(),

            OrderId = order.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await FulfillPaidOrder(order, baoKimTransactionEntity);
    }

    // BaoKim leaves completed_at null until a payment settles, and an empty value deserializes to
    // default(DateTime), whose Kind is Unspecified — Npgsql rejects that outright on the
    // timestamptz column, which would turn a good payment into a 500 and a retry storm.
    private static DateTime ToUtcOrNow(DateTime? value)
        => value is { } completedAt && completedAt != default
            ? DateTime.SpecifyKind(completedAt, DateTimeKind.Utc)
            : DateTime.UtcNow;

    private const string PendingStatus = "Pending";
    private const string PaidStatus = "Paid";

    /// <summary>
    /// Kill switch. While false the gateway callback keeps running the legacy inline fulfilment, so
    /// a bad deploy can be rolled back by flipping a Config row instead of shipping code.
    /// </summary>
    private const string UseSagaConfigKey = "fulfillment.use_saga";

    // Shared tail for both gateways.
    private async Task FulfillPaidOrder(Repository.Entity.Order order, Transaction transaction)
    {
        var fulfillmentConfigs = await ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "fulfillment");
        var useSaga = fulfillmentConfigs.GetValue(UseSagaConfigKey, true);

        if (!useSaga)
        {
            await FulfillPaidOrderInline(order, transaction);
            return;
        }

        // The saga path does exactly one thing here: record that the money arrived, and hand the
        // rest to the bus. No Ahamove call, no NhanhVn call, nothing that can make the gateway wait
        // or make this request fail after the payment is already real.
        order.Status = PaidStatus;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.Orders.Update(order);
        _dbContext.Transactions.Add(transaction);

        // Handmade không gọi AI. Card được tạo từ snapshot đã chốt ở CreateOrder
        // và được lưu cùng lần xác nhận thanh toán này.
        await CreateHandMadeCardsAsync(order);
        await AddVoucherUsageAsync(order);

        // Bus outbox: this does not touch RabbitMQ. It stages an OutboxMessage row inside the very
        // same SaveChanges below, so the order can never be marked Paid without the fulfilment
        // message, nor the message sent for an order that failed to commit.
        await _publishEndpoint.Publish(new OrderPaid
        {
            OrderId = order.Id,
            TransactionId = transaction.Id,
            Gateway = transaction.Gateway,
            RequestId = transaction.RequestId,
            Amount = transaction.TransferAmount
        });

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsDuplicateGatewayEvent(exception))
        {
            // A concurrent redelivery won the race and already recorded this exact gateway event.
            // The unique index is the only thing that actually stops two simultaneous callbacks -
            // the status check above is read-then-act and both can pass it. Ack and drop.
            return;
        }
    }

    /// <summary>
    /// Pre-saga behaviour, kept for one release behind <see cref="UseSagaConfigKey"/>: run the three
    /// fulfilment steps inline and only then commit. Delete once the saga has run clean in prod.
    /// </summary>
    private async Task FulfillPaidOrderInline(Repository.Entity.Order order, Transaction transaction)
    {
        var plan = await _fulfillmentService.DispatchDeliveryAsync(order.Id);
        await _fulfillmentService.CreatePosOrderAsync(order.Id, plan);

        // Các bước fulfillment phía trên có thể tự SaveChanges. Vì vậy chỉ tạo
        // Handmade sau chúng, ngay trước lần commit cuối của flow legacy.
        await CreateHandMadeCardsAsync(order);

        // Status is flipped LAST, deliberately. Both fulfilment steps SaveChanges on this same
        // scoped DbContext, so assigning "Paid" up front would persist it before NhanhVn had
        // succeeded, and the gateway's next redelivery would short-circuit on the Paid guard and
        // never finish the order.
        order.Status = PaidStatus;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.Orders.Update(order);
        _dbContext.Transactions.Add(transaction);

        await AddVoucherUsageAsync(order);
        await _dbContext.SaveChangesAsync();

        await _fulfillmentService.RevalidateOrderProductsAsync(order.Id);
    }

    // A paid order that used a voucher takes one use. The change is saved by the same SaveChanges that
    // marks the order Paid, so a failed or replayed callback can never count a use twice.
    private async Task AddVoucherUsageAsync(Repository.Entity.Order order)
    {
        var snapshot = DeserializeVoucherSnapshot(order.VoucherSnapshot);
        if (snapshot == null)
        {
            return;
        }

        // No IsDeleted filter: a deleted voucher still has the use of this order
        var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(x => x.Id == snapshot.VoucherId);
        if (voucher == null)
        {
            return;
        }

        voucher.UsedCount += 1;
    }

    private static HoaTheoMua.Service.Order.Request.VoucherSnapshot? DeserializeVoucherSnapshot(string? snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<HoaTheoMua.Service.Order.Request.VoucherSnapshot>(snapshot);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task CreateHandMadeCardsAsync(Repository.Entity.Order order)
    {
        var orderDetails = await _dbContext.OrderDetails
            .Where(x => x.OrderId == order.Id && !x.IsDeleted)
            .ToListAsync();

        foreach (var orderDetail in orderDetails)
        {
            if (string.IsNullOrWhiteSpace(orderDetail.Metadata))
                continue;

            Request.OrderDetailMetadata? metadata;
            try
            {
                metadata = JsonConvert.DeserializeObject<Request.OrderDetailMetadata>(orderDetail.Metadata);
            }
            catch (JsonException)
            {
                // Metadata cũ/không liên quan không được làm thất bại webhook
                // sau khi Customer đã chuyển tiền.
                continue;
            }

            if (metadata?.ProductMetadata == null)
                continue;

            var metadataChanged = false;
            foreach (var productMetadata in metadata.ProductMetadata)
            {
                if (productMetadata == null)
                    continue;

                var snapshot = productMetadata.SnapShotCard;
                if (snapshot == null ||
                    !string.Equals(snapshot.CardType, "handmade", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Webhook retry sẽ thấy cardId đã có và không tạo lại Card/History.
                if (snapshot.CardId.HasValue)
                    continue;

                var source = snapshot.Source;
                var template = snapshot.CardTemplate;
                if (snapshot.VirtualId == Guid.Empty ||
                    source == null || source.Id == Guid.Empty ||
                    template == null || template.TemplateId == Guid.Empty ||
                    string.IsNullOrWhiteSpace(template.TemplateImageUrl))
                {
                    // Snapshot không đủ dữ liệu tối thiểu để tạo Card thì bỏ qua.
                    // Xác nhận thanh toán vẫn phải được lưu thành công.
                    continue;
                }

                var createdAt = DateTimeOffset.UtcNow;
                var rootId = source.Id;
                var baseId = productMetadata.GeneratedFlower ?? rootId;
                var card = new GeneratedCard
                {
                    Id = Guid.NewGuid(),
                    UserId = order.UserId,
                    CreatedAt = createdAt,
                    Content = snapshot.MessageContent,
                    ImageUrl = template.TemplateImageUrl,
                    RawImage = null,
                    CardType = GeneratedCardType.Handmade,
                    FormType = CardFormType.Calligraphy,
                    MessageContent = snapshot.MessageContent,
                    AttachedImageUrl = snapshot.AttachedImageUrl,

                    // Contract Handmade không có cardSize. Entity hiện tại vẫn
                    // bắt buộc các cột Size nên lưu giá trị trung tính.
                    SizeName = string.Empty,
                    SizeWidth = 0m,
                    SizeHeight = 0m,
                    SizeBasePrice = 0m,
                    SizeMaxWords = 0,
                    WordCount = snapshot.WordCount,
                    WordConfigSnapshot = snapshot.CardConfig == null
                        ? null
                        : JsonConvert.SerializeObject(snapshot.CardConfig),
                    BasePrice = snapshot.BasePrice,
                    ExtraPrice = snapshot.ExtraPrice,
                    TotalPrice = snapshot.TotalPrice
                };

                var history = new ClientHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = order.UserId,
                    Type = ClientHistoryType.HandmadeCard,
                    BaseId = baseId,
                    RootId = rootId,
                    OutputId = card.Id,
                    CreatedAt = createdAt,
                    SystemForm = null,
                    SessionId = null,
                    Metadata = JsonConvert.SerializeObject(new
                    {
                        operation = "create",
                        orderDetailId = orderDetail.Id,
                        virtualId = snapshot.VirtualId,
                        sourceType = productMetadata.GeneratedFlower.HasValue
                            ? "generated_flower"
                            : "product",
                        originProductId = rootId,
                        generatedFlowerId = productMetadata.GeneratedFlower,
                        provenanceStatus = "available",
                        source,
                        cardTemplate = new
                        {
                            id = template.TemplateId,
                            name = template.TemplateName,
                            templateType = "handmade",
                            imageUrl = template.TemplateImageUrl
                        },
                        validatedAt = snapshot.ValidatedAt
                    })
                };

                _dbContext.GeneratedCards.Add(card);
                _dbContext.ClientHistories.Add(history);
                snapshot.CardId = card.Id;
                metadataChanged = true;
            }

            if (metadataChanged)
                orderDetail.Metadata = JsonConvert.SerializeObject(metadata);
        }
    }

    // Matches the unique index on Transactions (Gateway, RequestId). RequestId carries each
    // gateway's own event id - BaoKim's request_id, SePay's transaction id as text.
    private static bool IsDuplicateGatewayEvent(DbUpdateException exception)
        => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres
           && (postgres.ConstraintName?.Contains("Gateway_RequestId", StringComparison.OrdinalIgnoreCase) ?? false);
}

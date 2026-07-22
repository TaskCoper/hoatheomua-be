using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Webhook;

// BaoKim's callback sends naive timestamps ("2026-06-09 10:46:32") — no 'T', no offset — which
// System.Text.Json's built-in DateTime handling rejects outright since it only accepts ISO 8601.
// Without this the whole payload fails to bind and the handler never runs. BaoKim is a Vietnamese
// gateway and these are ICT, so shift to UTC: Npgsql needs Kind=Utc for the timestamptz columns.
public class BaoKimCallbackDateTimeConverter : JsonConverter<DateTime>
{
    private const string CallbackFormat = "yyyy-MM-dd HH:mm:ss";
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        if (DateTime.TryParseExact(value, CallbackFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var naive))
        {
            return new DateTimeOffset(naive, VietnamOffset).UtcDateTime;
        }

        // Tolerate an offset-bearing form in case BaoKim ever sends one.
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var withOffset))
        {
            return withOffset.UtcDateTime;
        }

        throw new JsonException($"Unsupported BaoKim callback datetime: '{value}'");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(CallbackFormat, CultureInfo.InvariantCulture));
}

public class BaoKimPaymentCallback
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; } = default!;

    [JsonPropertyName("request_time")]
    public string RequestTime { get; set; } = default!;

    [JsonPropertyName("merchant_code")]
    public string MerchantCode { get; set; } = default!;

    [JsonPropertyName("operation")]
    public string Operation { get; set; } = default!;

    [JsonPropertyName("payment_result")]
    public BaoKimPaymentResult PaymentResult { get; set; } = default!;

    [JsonPropertyName("token_result")]
    public object? TokenResult { get; set; }
}

public class BaoKimPaymentResult
{
    [JsonPropertyName("result_code")]
    public int ResultCode { get; set; }

    [JsonPropertyName("result_message")]
    public string ResultMessage { get; set; } = default!;

    [JsonPropertyName("order")]
    public BaoKimOrder Order { get; set; } = default!;

    [JsonPropertyName("transaction")]
    public List<BaoKimTransaction> Transaction { get; set; } = new();
}

public class BaoKimOrder
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("mrc_order_id")]
    public string MrcOrderId { get; set; } = default!;

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("payment_method")]
    public int PaymentMethod { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("completed_at")]
    [JsonConverter(typeof(BaoKimCallbackDateTimeConverter))]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("created_at")]
    [JsonConverter(typeof(BaoKimCallbackDateTimeConverter))]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    [JsonConverter(typeof(BaoKimCallbackDateTimeConverter))]
    public DateTime UpdatedAt { get; set; }
}

public class BaoKimTransaction
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("mrc_order_id")]
    public string MrcOrderId { get; set; } = default!;

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("payment_method")]
    public int PaymentMethod { get; set; }

    [JsonPropertyName("merchant_fee")]
    public decimal MerchantFee { get; set; }

    [JsonPropertyName("user_fee")]
    public decimal UserFee { get; set; }

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("bnpl")]
    public object? Bnpl { get; set; }

    [JsonPropertyName("installment")]
    public object? Installment { get; set; }

    [JsonPropertyName("completed_at")]
    [JsonConverter(typeof(BaoKimCallbackDateTimeConverter))]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("created_at")]
    [JsonConverter(typeof(BaoKimCallbackDateTimeConverter))]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    [JsonConverter(typeof(BaoKimCallbackDateTimeConverter))]
    public DateTime UpdatedAt { get; set; }
}
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.XanhSm;

public class XanhSmWebhookEvent
{
    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }
    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
    [JsonPropertyName("data")]
    public XanhSmWebhookData? Data { get; set; }
}

public class XanhSmWebhookData
{
    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("sub_status")]
    public string? SubStatus { get; set; }
}

public record XanhSmOrderChanged(string ShippingOrderId, string EventId, string Environment,
    string Status, string? SubStatus = null);

public class XanhSmWebhookService(
    IConfiguration configuration, AppDbContext dbContext, IPublishEndpoint publishEndpoint)
{
    private readonly XanhSmWebhookOptions _options =
        configuration.GetSection(nameof(XanhSmWebhookOptions)).Get<XanhSmWebhookOptions>() ?? new();

    public async Task ReceiveAsync(byte[] rawBody, string? environment, string? signature,
        string? secondarySignature, CancellationToken cancellationToken = default)
    {
        Verify(rawBody, environment, signature, secondarySignature);

        XanhSmWebhookEvent? webhook;
        try
        {
            webhook = JsonSerializer.Deserialize<XanhSmWebhookEvent>(rawBody);
        }
        catch (JsonException)
        {
            throw new BadRequestException("Invalid XanhSM webhook JSON");
        }

        if (string.IsNullOrWhiteSpace(webhook?.EventType))
            throw new BadRequestException("Missing XanhSM event_type");

        // Point completion does not imply order completion. Only order events drive POS updates.
        if (webhook.EventType != "order_status_changed")
            return;

        if (string.IsNullOrWhiteSpace(webhook.EventId) || webhook.EventId.Length > 200
            || string.IsNullOrWhiteSpace(webhook.Data?.OrderId) || webhook.Data.OrderId.Length > 100)
            throw new BadRequestException("Missing or invalid XanhSM event_id/data.order_id");

        if (string.IsNullOrWhiteSpace(webhook.Data.Status))
            throw new BadRequestException("Missing XanhSM data.status");

        // Stable message id supports the existing consumer inbox. The database outbox commits
        // before HTTP 200; provider/network work happens in the consumer, never on this request.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"xanhsm:{environment}:{webhook.EventId}"));
        await publishEndpoint.Publish(
            new XanhSmOrderChanged(webhook.Data.OrderId, webhook.EventId, environment!,
                webhook.Data.Status, webhook.Data.SubStatus),
            context => context.MessageId = new Guid(hash.AsSpan(0, 16)), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Verify(byte[] rawBody, string? environment, string? signature, string? secondarySignature)
    {
        if (string.IsNullOrWhiteSpace(_options.SigningKey)
            || _options.Environment is not ("production" or "sandbox"))
            throw new ServerException("XanhSM webhook signing key/environment is not configured");

        if (!string.Equals(environment, _options.Environment, StringComparison.Ordinal))
            throw new UnauthorizedException("Invalid XanhSM webhook environment");

        if (!Matches(rawBody, _options.SigningKey, signature)
            && !Matches(rawBody, _options.SecondarySigningKey, secondarySignature))
            throw new UnauthorizedException("Invalid XanhSM webhook signature");
    }

    private static bool Matches(byte[] body, string? key, string? signature)
    {
        if (string.IsNullOrWhiteSpace(key) || signature?.Length != 64)
            return false;
        byte[] supplied;
        try { supplied = Convert.FromHexString(signature); }
        catch (FormatException) { return false; }
        return CryptographicOperations.FixedTimeEquals(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), body), supplied);
    }
}

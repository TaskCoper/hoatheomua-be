using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HoaTheoMua.Service.BaoKim;

public class Service : IService
{
    private readonly BaoKimOption _baoKimOption = new();
    private readonly HttpClient _httpClient;
    private readonly IHostEnvironment? _environment;

    // Emits raw UTF-8 (no \uXXXX escaping) so the bytes we sign match the bytes we send,
    // which is what BaoKim's signature verification requires.
    private static readonly JsonSerializerOptions SignedJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // `environment` is optional so tests can construct this without a host. A null environment is
    // treated as non-Development, i.e. diagnostics stay off — the safe default, since the wrong
    // guess here dumps payment details into logs.
    public Service(HttpClient httpClient, IConfiguration configuration, IHostEnvironment? environment = null)
    {
        _httpClient = httpClient;
        _environment = environment;
        configuration.GetSection(nameof(BaoKimOption)).Bind(_baoKimOption);
    }

    private bool IsDevelopment => _environment?.IsDevelopment() ?? false;

    public async Task<BaoKimResponse<TokenResponse>> CreateAccessToken()
    {
        var tokenRequest = new CreateAccessTokenRequest()
        {
            MerchantCode = _baoKimOption.MerchantCode,
            ClientId = _baoKimOption.ClientId,
            ClientSecret = _baoKimOption.ClientSecret
        };

        using var req = new HttpRequestMessage(HttpMethod.Post,
            $"{_baoKimOption.BaseUrl}{BaoKimConstant.GetTokenUrl}");
        req.Content = JsonContent.Create(tokenRequest);

        using var response = await _httpClient.SendAsync(req);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new BadRequestException($"BaoKim API HTTP Error ({(int)response.StatusCode}): {json}");
        }

        var result = JsonSerializer.Deserialize<BaoKimResponse<TokenResponse>>(json, SignedJsonOptions);
        if (result == null || result.Data == null)
        {
            throw new BadRequestException("BaoKim returned null or empty data");
        }
        if (result.Code != BaoKimConstant.SuccessCode)
        {
            throw new BadRequestException($"BaoKim get-token failed ({result.Code}): {result.Message}");
        }

        return result;
    }

    public void VerifyCallbackSignature(string rawBody, string? signature)
    {
        if (string.IsNullOrWhiteSpace(_baoKimOption.PublicKey))
        {
            // Fail closed. Without the key we cannot tell a genuine callback from a forged one, and
            // a forged one flips an order to Paid and dispatches a courier for free.
            throw new ServerException(
                "BaoKimOption.PublicKey is not configured, so callback signatures cannot be verified");
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            LogSignatureDiagnostics(rawBody, signature: "<none>");
            throw new UnauthorizedException("Missing BaoKim callback signature");
        }

        bool isValid;
        try
        {
            isValid = BaoKimSigner.Verify(rawBody, signature, _baoKimOption.PublicKey);
        }
        // System.ArgumentException, not the Exceptions/ one this file imports.
        catch (Exception ex) when (ex is System.ArgumentException or CryptographicException)
        {
            // The configured key itself is unusable. That is our misconfiguration, so it must not
            // masquerade as the caller sending a bad signature.
            throw new ServerException($"BaoKimOption.PublicKey is not a usable PEM public key: {ex.Message}");
        }

        if (!isValid)
        {
            LogSignatureDiagnostics(rawBody, signature);
            throw new UnauthorizedException("Invalid BaoKim callback signature");
        }
    }

    // Bring-up diagnostics for the BaoKim callback, matching the Console.WriteLine idiom already
    // used for gateway debugging elsewhere. Development-only and failure-path-only: the dump
    // includes the raw callback body, which carries payment details that must not reach prod logs.
    private void LogSignatureDiagnostics(string rawBody, string signature)
    {
        if (!IsDevelopment)
        {
            return;
        }

        Console.WriteLine("[BAOKIM SIG] ---------- signature diagnostics ----------");
        Console.WriteLine($"[BAOKIM SIG] body: {rawBody.Length} chars, sha256={Sha256Hex(Encoding.UTF8.GetBytes(rawBody))}");
        Console.WriteLine($"[BAOKIM SIG] raw body: {rawBody}");
        Console.WriteLine($"[BAOKIM SIG] signature header value: {signature}");

        // The decoded length identifies the scheme. 256 bytes is an RSA-2048 signature, i.e. what we
        // assume, so a failure is about the KEY. 32 bytes would mean HMAC-SHA256 — a different
        // scheme entirely, which no public key could ever fix.
        try
        {
            var signatureBytes = Convert.FromBase64String(signature);
            Console.WriteLine(
                $"[BAOKIM SIG] signature is base64 -> {signatureBytes.Length} bytes " +
                $"({(signatureBytes.Length == 256 ? "consistent with RSA-2048" : "NOT an RSA-2048 signature")})");
        }
        catch (FormatException)
        {
            Console.WriteLine("[BAOKIM SIG] signature is NOT base64 — hex, or some other encoding?");
        }

        // Fingerprints only: enough to identify which key is loaded, without printing the key.
        var configured = KeyFingerprint(_baoKimOption.PublicKey, isPrivate: false);
        var ourOwn = KeyFingerprint(_baoKimOption.PrivateKey, isPrivate: true);
        Console.WriteLine($"[BAOKIM SIG] configured PublicKey fingerprint      : {configured}");
        Console.WriteLine($"[BAOKIM SIG] public half of our own PrivateKey     : {ourOwn}");

        if (configured == ourOwn)
        {
            Console.WriteLine(
                "[BAOKIM SIG] >>> THOSE MATCH: the configured PublicKey is OUR OWN key, so we are " +
                "checking BaoKim's signature against a key BaoKim does not hold. This can never " +
                "succeed. Set BaoKimOption.PublicKey to BaoKim's public key from the merchant " +
                "dashboard.");
        }

        Console.WriteLine("[BAOKIM SIG] -------------------------------------------");
    }

    private static string Sha256Hex(byte[] data) => Convert.ToHexString(SHA256.HashData(data));

    // Fingerprints the PUBLIC half of either key, so the two are directly comparable.
    private static string KeyFingerprint(string pem, bool isPrivate)
    {
        if (string.IsNullOrWhiteSpace(pem))
        {
            return "<not configured>";
        }

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            return Sha256Hex(rsa.ExportSubjectPublicKeyInfo())[..16];
        }
        catch (Exception ex)
        {
            return $"<unusable {(isPrivate ? "private" : "public")} key: {ex.GetType().Name}>";
        }
    }

    public async Task<BaoKimResponse<CreateOrderResponse>> CreateOrder(
        string accessToken,
        Repository.Entity.Order order)
    {
        var requestId = Guid.NewGuid().ToString();

        var orderRequest = new CreateOrderRequest
        {
            RequestId = requestId,
            RequestTime = DateTimeOffset.UtcNow,
            MerchantCode = _baoKimOption.MerchantCode,
            MrcOrderId = order.Id.ToString(),
            Description = string.IsNullOrWhiteSpace(order.Remarks) ? $"Đơn hàng {order.Id}" : order.Remarks,
            TotalAmount = (long)order.TotalFee,
            UrlSuccess = $"{BaoKimConstant.PaymentSuccessUrl}?requestId={requestId}&orderId={order.Id}",
            UrlFail = $"{BaoKimConstant.PaymentFailUrl}?requestId={requestId}&orderId={order.Id}",
            // Items = orderDetails.Select(x => new OrderItem
            // {
            //     Code = x.ItemId.ToString(),cho
            //     Name = x.Name,
            //     Amount = (long)x.Price,
            //     Quantity = x.Number,
            //     Link = $"{BaoKimConstant.ProductUrl}/{x.ItemId}"
            // }).ToList(),
            CustomerInfo = new CustomerInfo
            {
                Code = order.UserId.ToString(),
                Name = order.FullName,
                Email = order.ReceiptEmail ?? "khachkhongcoemail@gmail.com",
                Phone = order.PhoneReceiver,
                Address = order.Address,
                Gender = 1
            },
            PaymentMethod = order.PaymentMethod
        };

        // Serialize once, sign those exact bytes, then send the same string.
        var body = JsonSerializer.Serialize(orderRequest, SignedJsonOptions);
        var signature = BaoKimSigner.Sign(body, _baoKimOption.PrivateKey);

        using var req = new HttpRequestMessage(HttpMethod.Post,
            $"{_baoKimOption.BaseUrl}{BaoKimConstant.CreateOrder}");
        req.Content = new StringContent(body, Encoding.UTF8, "application/json");
        req.Headers.TryAddWithoutValidation("Signature", signature);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(req);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new BadRequestException($"BaoKim API HTTP Error ({(int)response.StatusCode}): {json}");
        }

        // Check the business code before strict deserialization: on a non-success code BaoKim's
        // `data` payload is an error shape that lacks the required order fields, so parsing it into
        // CreateOrderResponse would throw a raw 500 and mask the real reason the order was rejected.
        using (var doc = JsonDocument.Parse(json))
        {
            var root = doc.RootElement;
            var code = root.TryGetProperty("code", out var codeEl) ? codeEl.GetInt32() : int.MinValue;
            if (code != BaoKimConstant.SuccessCode)
            {
                var message = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : null;
                throw new BadRequestException($"BaoKim create order failed ({code}): {message}. Raw: {json}");
            }
        }

        BaoKimResponse<CreateOrderResponse>? result;
        try
        {
            result = JsonSerializer.Deserialize<BaoKimResponse<CreateOrderResponse>>(json, SignedJsonOptions);
        }
        catch (JsonException ex)
        {
            throw new BadRequestException($"BaoKim create order returned an unexpected payload: {ex.Message}. Raw: {json}");
        }

        if (result == null || result.Data == null)
        {
            throw new BadRequestException("BaoKim returned null or empty data");
        }

        return result;
    }
}
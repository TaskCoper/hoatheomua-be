using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.BaoKim;

public class Service : IService
{
    private readonly BaoKimOption _baoKimOption = new();
    private readonly HttpClient _httpClient;

    // Emits raw UTF-8 (no \uXXXX escaping) so the bytes we sign match the bytes we send,
    // which is what BaoKim's signature verification requires.
    private static readonly JsonSerializerOptions SignedJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public Service(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        configuration.GetSection(nameof(BaoKimOption)).Bind(_baoKimOption);
    }

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

    public async Task<BaoKimResponse<CreateOrderResponse>> CreateOrder(
        string accessToken,
        Repository.Entity.Order order,
        IEnumerable<OrderDetails> orderDetails)
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
            Items = orderDetails.Select(x => new OrderItem
            {
                Code = x.ItemId.ToString(),
                Name = x.Name,
                Amount = (long)x.Price,
                Quantity = x.Number,
                Link = $"{BaoKimConstant.ProductUrl}/{x.ItemId}"
            }).ToList(),
            CustomerInfo = new CustomerInfo
            {
                Code = order.UserId.ToString(),
                Name = order.FullName,
                Email = order.ReceiptEmail ?? string.Empty,
                Phone = order.Phone,
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

        var result = JsonSerializer.Deserialize<BaoKimResponse<CreateOrderResponse>>(json, SignedJsonOptions);
        if (result == null || result.Data == null)
        {
            throw new BadRequestException("BaoKim returned null or empty data");
        }
        if (result.Code != BaoKimConstant.SuccessCode)
        {
            throw new BadRequestException($"BaoKim create order failed ({result.Code}): {result.Message}");
        }

        return result;
    }
}
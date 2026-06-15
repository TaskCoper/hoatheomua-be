using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ArgumentException = HoaTheoMua.Service.Exceptions.ArgumentException;

namespace HoaTheoMua.Service.AhamoveService;

public class Service: IService
{
    private readonly HttpClient _httpClient;
    private readonly AhamoveOption _ahamoveAhamoveOption = new();

    public Service(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        configuration.GetSection(nameof(AhamoveOption)).Bind(_ahamoveAhamoveOption);
    }

    public async Task<decimal> EstimateFee(Request.EstimateFeeRequest request)
    {
            if(request.Points == null || request.Points.Count != 2)
            {
                throw new ArgumentException("Dữ liệu không hợp lệ. Đơn hàng bắt buộc phải gồm đúng 1 điểm lấy và 1 điểm giao.");
            }

            var ahamoveRequestBody = new{
                service_id = request.ServiceId,
                payment_method = "BALANCE",
                path = request.Points.Select(p => new {
                    address = p.Address,
                    lat = double.TryParse(p.Lat, out var latVal) ? latVal : 0,
                    lng = double.TryParse(p.Lng, out var lngVal) ? lngVal : 0,
                    name = p.Name,
                    mobile = p.Mobile,
                    remarks = p.Remarks
                }).ToList()
            };

            var fullUrl  = $"{_ahamoveAhamoveOption.BaseUrl}/v3/orders/estimates";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, fullUrl);
            httpRequest.Headers.Add("Authorization", $"Bearer {_ahamoveAhamoveOption.Token}");
            httpRequest.Content = JsonContent.Create(ahamoveRequestBody);

            var httpResponse = await _httpClient.SendAsync(httpRequest);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Kết nối đối tác vận chuyển thất bại do lỗi xác thực Token.");
                }

                throw new BadGatewayException($"Bên vận chuyển Ahamove từ chối tính phí: {error}");
            }
            
            var jsonString = await httpResponse.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            decimal totalFee = 0;
            if (root.TryGetProperty("total_fee", out var totalFeeValue))
            {
                totalFee = totalFeeValue.GetDecimal();
            }

            return  totalFee;
    }
}
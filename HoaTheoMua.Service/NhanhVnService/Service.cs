using System.Net.Http.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.NhanhVnService;

public class Service: IService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _appDbContext;

    public Service(HttpClient httpClient, AppDbContext appDbContext)
    {
        _httpClient = httpClient;
        _appDbContext = appDbContext;
    }
    
    public async Task<OrderLabelResponse> GetOrderLabel(OrderLabelRequest orderLabelRequest)
    {
        var appId = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigIntAsync(_appDbContext, "nhanhvn.app_id", 0);
        var businessId = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigIntAsync(_appDbContext, "nhanhvn.business_id123123", 0);
        var token = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigStringAsync(_appDbContext, "nhanhvn.token", "");
        var nhanhApi = (await HoaTheoMua.Service.Common.ConfigHelper.GetConfigStringAsync(_appDbContext, "nhanhvn.base_url", "https://pos.open.nhanh.vn/v3.0")) + "/order/tags?appId=" + appId + "&businessId=" + businessId;  
        
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(orderLabelRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        OrderLabelResponse? nhanhProductOrderLable = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {

            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();

            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");


            nhanhProductOrderLable = await nhanhVnResponse.Content.ReadFromJsonAsync<OrderLabelResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductOrderLable == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        
        if (nhanhProductOrderLable.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        return nhanhProductOrderLable;
    }

    public async Task<OrderSourceResponse> GetOrderSource(OrderSourceRequest orderSourceRequest)
    {
        var appId = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigIntAsync(_appDbContext, "nhanhvn.app_id", 0);
        var businessId = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigIntAsync(_appDbContext, "nhanhvn.business_id123123", 0);
        var token = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigStringAsync(_appDbContext, "nhanhvn.token", "");
        var nhanhApi = (await HoaTheoMua.Service.Common.ConfigHelper.GetConfigStringAsync(_appDbContext, "nhanhvn.base_url", "https://pos.open.nhanh.vn/v3.0")) + "/order/source?appId=" + appId + "&businessId=" + businessId;  
        
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(orderSourceRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        OrderSourceResponse? nhanhProductOrderSource = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {

            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();

            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");


            nhanhProductOrderSource = await nhanhVnResponse.Content.ReadFromJsonAsync<OrderSourceResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductOrderSource == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        
        if (nhanhProductOrderSource.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        return nhanhProductOrderSource;
    }
}
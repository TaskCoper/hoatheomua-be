using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.Product;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly HttpClient _httpClient;
    private readonly NhanhVnOption _nhanhVnOption;

    public Service(AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _httpClient = httpClient;
        _nhanhVnOption = new NhanhVnOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
    }
    
    public async Task<NhanhVnProductListResponse.NhanhVnProductResponse> GetListProducts(Request.ProductListRequest request)
    {
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/product/list?appId=" + appId + "&businessId=" + businessId;
        
        
        
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(request, options);

        var newRequest = JsonSerializer.Deserialize<object>(json);

        Console.WriteLine(
            $"[DEBUG NHANH.VN RAW DATA]: {JsonSerializer.Serialize(newRequest)}"
        );
        
        
        // var nhanhProductListRequest = new Request.ProductListRequest()
        // {
        //     Filters = new Request.ProductFilters()
        //     {
        //         Ids = request.Filters?.Ids ?? new List<int>(),
        //         Status = request.Filters?.Status ?? new List<int>()
        //     },
        //     Paginator = new Request.ProductPaginator()
        //     {
        //         Size = request.Paginator?.Size ?? 50,
        //         Sort = new Request.ProductSort()
        //         {
        //             Id = request.Paginator?.Sort.Id ?? "desc"
        //         },                                                                                                                                      
        //         Next = request.Paginator?.Next != null 
        //             ? new Request.ProductNextPage { Id = request.Paginator.Next.Id }                    
        //             : null
        //     }
        // };
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(newRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        NhanhVnProductListResponse.NhanhVnProductResponse? nhanhProductList = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {

            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();

            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");


            nhanhProductList = await nhanhVnResponse.Content.ReadFromJsonAsync< NhanhVnProductListResponse.NhanhVnProductResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductList == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        
        if (nhanhProductList.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        // int totalRequests = nhanhProductList.Data.Count(x => true);
        //
        // int actualPageIndex = pageIndex < 1 ? 1 : pageIndex;
        // int actualPageSize = pageSize < 1 ? 10 : pageSize;
        //
        // var result = nhanhProductList.Data
        //     .Where(x => true)
        //     .Skip((actualPageIndex - 1) * actualPageSize)
        //     .Take(actualPageSize)
        //     .ToList();
        //
        // var paginationResult = ApiResponseFactory.BasePagination(
        //     items: result, 
        //     pageIndex: actualPageIndex, 
        //     pageSize: actualPageSize, 
        //     totalCount: totalRequests
        // );
        
        return nhanhProductList;
        
    }

    public async Task<NhanhVnProductDetailsResponse.NhanhVnProductDetailResponse> GetProductDetails(Request.ProductDetailRequest request)
    {
        
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/product/detail?appId=" + appId + "&businessId=" + businessId;
        var nhanhProductDetailsRequest = request;
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhProductDetailsRequest);
        
        
        
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);
        
        
        // var options = new JsonSerializerOptions
        // {
        //     DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        // };
        //
        // var json = JsonSerializer.Serialize(request, options);
        //
        // var newRequest = JsonSerializer.Deserialize<object>(json);
        //
        // Console.WriteLine(
        //     $"[DEBUG NHANH.VN RAW DATA]: {JsonSerializer.Serialize(newRequest)}"
        // );
        
        
        
        NhanhVnProductDetailsResponse.NhanhVnProductDetailResponse? nhanhProductDetails = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            // nhanhProductDetails = await nhanhVnResponse.Content.ReadFromJsonAsync< ResponseProductDetails.NhanhVnProductDetailResponse>();
            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();
    
            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");

            // 2. Cấu hình bắt buộc phải truyền kèm để kích hoạt Custom Converter của bạn
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                // Ép kiểu số linh hoạt nếu Nhanh.vn trả về string số
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString 
            };

            // 3. Tiến hành phân rã chuỗi JSON an toàn thông qua bộ options đã có Converter
            nhanhProductDetails = JsonSerializer.Deserialize<NhanhVnProductDetailsResponse.NhanhVnProductDetailResponse>(rawJsonString, options);
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductDetails == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        if (nhanhProductDetails.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }
        
        return nhanhProductDetails;
    }
}
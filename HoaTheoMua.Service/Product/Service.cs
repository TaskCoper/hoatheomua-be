using System.Net.Http.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.Product;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly NhanhVnOption _nhanhVnOption = new();
    private readonly AhamoveOption _ahamoveAhamoveOption = new();
    private readonly IHttpContextAccessor _httpContext;
    private readonly HttpClient _httpClient;

    public Service(AppDbContext dbContext, 
        IHttpContextAccessor httpContext, 
        IConfiguration configuration, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        configuration.GetSection(nameof(AhamoveOption)).Bind(_ahamoveAhamoveOption);
        _httpClient = httpClient;
    }
    
    public async Task<object> GetListProducts(Request.ProductListRequest request)
    {
        var user = _httpContext.HttpContext?.User.FindFirst("UserId")?.Value;  
  
        if (user == null)  
        {            
            throw new UnauthorizedException("User not found");  
        }
        var userId = Guid.Parse(user);
        
        var existUser = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsVerify);
        if (existUser == null)
        {
            throw new NotFoundException("User not found");
        }
        
        var appId = _nhanhVnOption.AppId;
        var businessId = _nhanhVnOption.BusinessId;
        var token = _nhanhVnOption.Token;
        var nhanhApi = _nhanhVnOption.BaseUrl + "/product/list?appId=" + appId + "&businessId=" + businessId;
        var nhanhProductListRequest = new Request.ProductListRequest()
        {
            Filters = new Request.ProductFilters()
            {
                Ids = request.Filters?.Ids ?? new List<int>(),
                Status = request.Filters?.Status ?? new List<int>()
            },
            Paginator = new Request.ProductPaginator()
            {
                Size = request.Paginator?.Size ?? 50,
                Sort = new Request.ProductSort()
                {
                    Id = request.Paginator?.Sort.Id ?? "desc"
                },                                                                                                                                      
                Next = request.Paginator?.Next != null 
                    ? new Request.ProductNextPage { Id = request.Paginator.Next.Id }                    
                    : null
            }
        };
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhProductListRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        Response.NhanhVnProductResponse? nhanhProductList = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            nhanhProductList = await nhanhVnResponse.Content.ReadFromJsonAsync< Response.NhanhVnProductResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new Exception($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductList == null)
        {
            throw new Exception("NhanhVnResponse returned null or empty data");
        }

        return nhanhProductList;
        
    }
}
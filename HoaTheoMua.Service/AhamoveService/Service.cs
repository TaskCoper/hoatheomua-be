using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.AhamoveService;

public class Service: IService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _appDbContext;
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly AhaMoveOption _ahaMoveOption;

    public Service(HttpClient httpClient, AppDbContext appDbContext, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _appDbContext = appDbContext;
        _nhanhVnOption = new NhanhVnOption();
        _ahaMoveOption = new AhaMoveOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        configuration.GetSection(nameof(AhaMoveOption)).Bind(_ahaMoveOption);
    }


    public async Task<object> EstimateShippingFee(AhaMoveRequest.EstimateShippingFeeRequest request)
    {
        // Call DB Tim Address cua Shop
        var companyAddress = await _appDbContext.CompanyAddresses
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync();
        
        if (companyAddress == null)
        {
            throw new Exception("Company address not found");
        }
        
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_appDbContext, "nhanhvn", "ahamove", "contact");

        // Call NhanhVn de tim gia tien cua Item
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/product/list?appId=" + appId + "&businessId=" + businessId;
        var nhanhVnProductRequest = new NhanhVnRequest.NhanhProductListRequest()
        {
            Filters = new NhanhVnRequest.NhanhFilters()
            {
                Id = request.Items.Select(x => x.ItemId).ToList(),
                Status = [1,2]
            },
            Paginator =  new NhanhVnRequest.NhanhPaginator()
            {
                Size = 50,
                Sort = new Dictionary<string, string>()
                {
                    { "id", "desc" }
                },
                Next = new Dictionary<string, int>()
                {
                    { "id", 0 }
                },
            }
        };
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhVnProductRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        NhanhProductListResponse? productNhanhResult = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            productNhanhResult = await nhanhVnResponse.Content.ReadFromJsonAsync<NhanhProductListResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (productNhanhResult == null || productNhanhResult.Data == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }

        if (productNhanhResult.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }
        
        var itemsRequest = productNhanhResult.Data.Select(
            x => new AhaMoveRequest.AhamoveItem()
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Price = (int)x.Prices.Retail,
                Num = request.Items.First(item => item.ItemId.Equals(x.Id)).Number
            }
        ).ToList();
        
        var packageDetailRequest = productNhanhResult.Data.Select(
            x => new AhaMoveRequest.PackageDetail()
            {
                Height = x.Shipping.Height,
                Weight = x.Shipping.Weight,
                Length = x.Shipping.Length,
                Width =  x.Shipping.Width,
                Description = "Noi that"
            }
        ).ToList();
        
        var createRequest = new AhaMoveRequest.AhaMoveShippingFeeRequest()
        {
            OrderTime = request.OrderTime,                               
            IdleUntil = request.OrderTime > 0 ? request.OrderTime : null,
            Paths = new List<AhaMoveRequest.AhamoveLocationPoint>()
            {
                new AhaMoveRequest.AhamoveLocationPoint() 
                {
                    Latitude = companyAddress.Latitude ?? 0,
                    Longitude = companyAddress.Longitude ?? 0,
                    Address = companyAddress.AddressLine ?? "",
                    ShortAddress = (companyAddress.AddressLine + ", " + companyAddress.Ward + ", " + companyAddress.City) ?? "",
                    FullName = companyAddress.FullName,
                    Phone = companyAddress.PhoneNumber,
                    Remarks = companyAddress.Remarks!,
                },
                new AhaMoveRequest.AhamoveLocationPoint()
                {
                    Latitude =  request.Latitude,
                    Longitude =  request.Longitude,
                    Address = request.Address,
                    ShortAddress =  request.ShortAddress,
                    FullName =  request.FullName,
                    Phone =  request.Phone,
                    Remarks =  request.Remarks,
                },
            },
            PaymentMethod = "CASH",
            Remarks =  request.Remarks,
            Items = itemsRequest,
            PackageDetail = packageDetailRequest,
            Services = new List<AhaMoveRequest.ServiceRequest>()
            {
                new AhaMoveRequest.ServiceRequest()
                {
                    Id = "SGN-BIKE",
                    Requests = new List<AhaMoveRequest.ItemsRequest>()
                    {
                        // new AhaMoveRequest.ItemsRequest()
                        // {
                        //     Id = "SGN-BIKE-SMS",
                        // },
                        // new AhaMoveRequest.ItemsRequest()
                        // {
                        //     Id = "SGN-BIKE-FRAGILE",
                        // },
                        // new AhaMoveRequest.ItemsRequest()
                        // {
                        //     Id = "SGN-BIKE-BULKY",
                        //     TierCode = "TIER_2"
                        // },
                        new AhaMoveRequest.ItemsRequest()
                        {
                            Id = "SGN-BIKE-BAGA"
                        },
                    }
                },
            }
        };

        var ahamoveApi = configs.GetValue("ahamove.base_url", _ahaMoveOption.BaseUrl) + "/v3/orders/estimates";
        var ahamoveToken = configs.GetValue("ahamove.api_key", _ahaMoveOption.Token);
        var ahamoveHttpRequest = new HttpRequestMessage(HttpMethod.Post, ahamoveApi);
        ahamoveHttpRequest.Headers.Add("Authorization", ahamoveToken);
        ahamoveHttpRequest.Content = JsonContent.Create(createRequest);
        
        HttpResponseMessage ahamoveResponse = await _httpClient.SendAsync(ahamoveHttpRequest);

        List<ServiceResponse>? ahamoveResult = null;
        
        if (ahamoveResponse.IsSuccessStatusCode)
        {
            ahamoveResult = await ahamoveResponse.Content.ReadFromJsonAsync<List<ServiceResponse>>();
        } else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"Ahamove API HTTP Error ({ahamoveResponse.StatusCode})");
        }
        
        if (ahamoveResult == null || ahamoveResult.Any() == false)
        {
            throw new BadRequestException("Ahamove returned null or empty data");
        }
        
        return new { ahamoveResult, productNhanhResult};
    }
    
    public async Task<ServiceDetailsResponse> GetServiceDetailsAhaMove()
    {
        var url = "https://partner-apistg.ahamove.com/v3/services/SGN-BIKE";
        var token = _ahaMoveOption.Token;
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", token);
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        
        string message = await response.Content.ReadAsStringAsync();
        Console.WriteLine($@"{message}");
        
        ServiceDetailsResponse? ahamoveResult = null;
        if (response.IsSuccessStatusCode)
        {
            ahamoveResult = await response.Content.ReadFromJsonAsync<ServiceDetailsResponse>();
        }
        else
        {
            throw new BadRequestException($"Ahamove API HTTP Error ({response.StatusCode})");
        }

        if (ahamoveResult == null)
        {
            throw new BadRequestException("Ahamove returned null or empty data");
        }
        return ahamoveResult;
    }

    public async Task<DepotResponse> GetDepotDetails(NhanhVnRequest.DepotRequest request)
    {
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_appDbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/business/depot?appId=" + appId + "&businessId=" + businessId;  
        
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(request);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        DepotResponse? nhanhProductDepot = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {

            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();

            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            // Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");


            nhanhProductDepot = await nhanhVnResponse.Content.ReadFromJsonAsync<DepotResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductDepot == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        
        if (nhanhProductDepot.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        return nhanhProductDepot;
    }
}
using System.Net.Http.Json;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ArgumentException = HoaTheoMua.Service.Exceptions.ArgumentException;

namespace HoaTheoMua.Service.AhamoveService;

public class Service: IService
{
    private readonly HttpClient _httpClient;
    private readonly AhamoveOption _ahamoveAhamoveOption = new();
    private readonly NhanhVnOption _nhanhVnOption = new();
    private readonly AppDbContext _appDbContext;

    public Service(HttpClient httpClient, IConfiguration configuration,  AppDbContext appDbContext)
    {
        _httpClient = httpClient;
        configuration.GetSection(nameof(AhamoveOption)).Bind(_ahamoveAhamoveOption);
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        _appDbContext = appDbContext;
    }


    public async Task<object> EstimateShippingFee(Request.EstimateShippingFeeRequest request)
    {
        // Call DB Tim Address cua Shop
        // var companyAddress = await _appDbContext.CompanyAddresses
        //     .Where(x => x.IsDeleted == false)
        //     .FirstOrDefaultAsync();
        
        // if (companyAddress == null)
        // {
        //     throw new Exception("Company address not found");
        // }
        
        // Call NhanhVn de tim gia tien cua Item
        var appId = _nhanhVnOption.AppId;
        var businessId = _nhanhVnOption.BusinessId;
        var token = _nhanhVnOption.Token;
        var nhanhApi = _nhanhVnOption.BaseUrl + "/product/list?appId=" + appId + "&businessId=" + businessId;
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
            throw new Exception($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (productNhanhResult == null || productNhanhResult.Data == null)
        {
            throw new Exception("NhanhVnResponse returned null or empty data");
        }
        
        var itemsRequest = productNhanhResult.Data.Select(
            x => new Request.AhamoveItem()
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Price = (int)x.Prices.Retail,
                Num = request.Items.First(item => item.ItemId.Equals(x.Id)).Number
            }
        ).ToList();
        
        var packageDetailRequest = productNhanhResult.Data.Select(
            x => new Request.PackageDetail()
            {
                Height = x.Shipping.Height,
                Weight = x.Shipping.Weight,
                Length = x.Shipping.Length,
                Width =  x.Shipping.Width,
                Description = "Noi that"
            }
        ).ToList();
        
        var createRequest = new Request.AhaMoveShippingFeeRequest()
        {
            OrderTime = 0,
            Paths = new List<Request.AhamoveLocationPoint>()
            {
                new Request.AhamoveLocationPoint() // Thang nay cua SHop minh, Lay tu DB ra
                {
                    // Latitude = companyAddress.Latitude ?? 0,
                    // Longitude = companyAddress.Longitude ?? 0,
                    // Address = companyAddress.AddressLine,
                    // ShortAddress = "",
                    // FullName = "",
                    // Phone = "",
                    // Remarks = "Đến nơi lấy hàng đọc mã đơn để nhận hàng",
                    Latitude = (decimal)10.8055952,
                    Longitude = (decimal)106.6776989,
                    Address = "Số 13A Đào Duy Anh, Phường 9, Phường Phú Nhuận, Thành phố Hồ Chí Minh",
                    ShortAddress = "Số 13A Đào Duy Anh, Phường 9, Phường Phú Nhuận, Thành phố Hồ Chí Minh",
                    FullName = "Hoa Theo Mùa",
                    Phone = "84908765432",
                    Remarks = "Đến nơi lấy hàng đọc mã đơn để nhận hàng",
                },
                new Request.AhamoveLocationPoint()
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
            Services = new List<Request.ServiceRequest>()
            {
                new Request.ServiceRequest()
                {
                    Id = "SGN-BIKE",
                    Requests = new List<Request.ItemsRequest>()
                    {
                        new Request.ItemsRequest()
                        {
                            Id = "SGN-BIKE-SMS",
                        },
                        new Request.ItemsRequest()
                        {
                            Id = "SGN-BIKE-D2D",
                        },
                        new Request.ItemsRequest()
                        {
                            Id = "SGN-BIKE-FRAGILE",
                        },
                        new Request.ItemsRequest()
                        {
                            Id = "SGN-BIKE-BULKY",
                            TierCode = "TIER_2"
                        },
                    }
                },
            }
        };

        var ahamoveApi =  _ahamoveAhamoveOption.BaseUrl+"/v3/orders/estimates";
        var ahamoveToken = _ahamoveAhamoveOption.Token;
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
            throw new Exception($"Ahamove API HTTP Error ({ahamoveResponse.StatusCode})");
        }
        
        if (ahamoveResult == null || ahamoveResult.Any() == false)
        {
            throw new Exception("Ahamove returned null or empty data");
        }
        
        return new { ahamoveResult, productNhanhResult};
    }
}
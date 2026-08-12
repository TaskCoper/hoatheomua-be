using System.Net.Http.Json;
using System.Text.Json;
using MiniExcelLibs;
using Microsoft.Extensions.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace HoaTheoMua.Service.Revalidate;

public class Service: IService
{
    private readonly HttpClient _httpClient;
    private readonly SecretOptions _secretOptions;

    public Service(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _secretOptions = new SecretOptions();
        configuration.GetSection(nameof(SecretOptions)).Bind(_secretOptions);
    }
    
    public async Task<string> RevalidateApi(JsonElement body)
    {
        var baseUrl = _secretOptions.BaseUrl;
        var secretKey = _secretOptions.SecretKey;
        
        var client = new HttpRequestMessage(HttpMethod.Post, baseUrl);
        client.Headers.Add("x-revalidate-secret", secretKey);
        client.Content = JsonContent.Create(body);
        
        HttpResponseMessage response = await _httpClient.SendAsync(client);
        
        var rawString = await response.Content.ReadAsStringAsync();
        
        return rawString;
    }
}
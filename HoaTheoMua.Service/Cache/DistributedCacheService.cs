using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HoaTheoMua.Service.Cache;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache; // Dịch vụ tương tác với Redis của ASP.NET Core

    private readonly ILogger<DistributedCacheService> _logger; // Ghi nhận lỗi khi thao tác cache

    private readonly TimeSpan _defaultExpiration; // Thời gian hết hạn mặc định

    // Tạo một bộ khóa Semaphore tĩnh để dùng chung cho toàn hệ thống
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);


    public DistributedCacheService(IDistributedCache cache, IConfiguration configuration,
        ILogger<DistributedCacheService> logger)
    {
        _cache = cache;

        _logger = logger;

        // Đọc số phút từ appsettings.json. Nếu không tìm thấy, mặc định lấy 60 phút
        _defaultExpiration = TimeSpan.FromMinutes(
            double.Parse(configuration["Redis:DefaultExpirationMinutes"] ?? "60"));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Đọc giá trị dạng chuỗi (String) từ Redis dựa theo Key
            var value = await _cache.GetStringAsync(key, cancellationToken);

            // 2. Nếu không tìm thấy Key hoặc giá trị trống/khoảng trắng
            //  trả về giá trị mặc định của T (null với object)
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            // 3. Giải tuần tự hóa (Deserialize) chuỗi JSON lấy được từ Redis
            // ngược lại thành đối tượng kiểu T ban đầu
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            //  4. Cơ chế Failsafe: Nếu Redis sập, ghi log lại lỗi và trả về default (null)
            // Việc này giúp ứng dụng tiếp tục chạy bình thường (lấy dữ liệu từ DB) mà không văng lỗi 500 ra Client
            _logger.LogError(ex, "Cache GET failed with key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Chuyển đổi Object kiểu T thành chuỗi văn bản JSON
            var json = JsonSerializer.Serialize(value);

            // 2. Cấu hình thời gian sống của dữ liệu đệm (Time To Live - TTL)
            var options = new DistributedCacheEntryOptions
            {
                // Nếu truyền thời gian hết hạn thì dùng, không thì lấy thời gian sống mặc định (60 phút)
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? _defaultExpiration
            };
            // 3. Ghi chuỗi JSON đó vào Redis kèm theo cấu hình thời gian hết hạn
            await _cache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache SET failed with key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Gọi lệnh xóa Key đệm trên Redis Server ngay lập tức (Eviction)
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache REMOVE failed with key {Key}", key);
        }
    }

    // Biểu thức Lambda/Hàm truy vấn dữ liệu từ Database gốc
    public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null, CancellationToken cancellationToken = default)
    {
        //Thử tìm dữ liệu từ cache trước
        var cached = await GetAsync<T>(key, cancellationToken);

        //[Cache Hit] Nếu có dữ liệu trong cache, trả về ngay lập tức (không chạy xuống DB)
        if (cached is not null)
        {
            return cached;
        }

        // Nếu Cache Miss, bắt đầu xếp hàng trước "cánh cửa" Semaphore
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // BẮT BUỘC: Kiểm tra lại cache một lần nữa (Double-Check Lock Pattern)
            // Vì trong lúc bạn xếp hàng chờ, thằng đứng trước đã xuống DB lấy dữ liệu và nạp vào Cache xong rồi!
            cached = await GetAsync<T>(key, cancellationToken);
            if (cached is not null) return cached;

            // Thằng đầu tiên đi xuyên qua cửa sẽ được quyền gọi hàm factory xuống DB
            var value = await factory(cancellationToken);

            // Nạp ngay vào cache cho các thằng đang xếp hàng phía sau hưởng sái
            await SetAsync(key, value, absoluteExpirationRelativeToNow, cancellationToken);

            return value;
        }
        finally
        {
            // Luôn giải phóng khóa trong khối 'finally' để thằng tiếp theo vào
            _semaphore.Release();
        }
    }
}
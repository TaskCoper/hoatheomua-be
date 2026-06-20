using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Redis.StackExchange;
using HoaTheoMua.Service.Jobs;

namespace HoaTheoMua.Extentions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Đọc Connection String của Redis từ file cấu hình appsettings.json
        var connectionString = configuration.GetConnectionString("Redis");

        // Cơ chế Graceful Degradation: Nếu không khai báo cấu hình Redis,
        // dừng đăng ký Hangfire và trả về ngay để tránh crash ứng dụng khi khởi động.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services;
        }

        // 2. Lấy số lượng luồng xử lý song song (Worker). Sử dụng TryParse để tránh crash nếu cấu hình sai.
        if (!int.TryParse(configuration["Hangfire:WorkerCount"], out var workerCount))
        {
            workerCount = 5; // Giá trị mặc định an toàn nếu cấu hình lỗi hoặc thiếu
        }

        // Lấy danh sách hàng đợi và độ ưu tiên. Mặc định là critical > default > low.
        var queues = configuration.GetSection("Hangfire:Queues").Get<string[]>()
            ?? ["critical", "default", "low"];
            
        // Key prefix để phân biệt dữ liệu Hangfire trong Redis (Tránh xung đột khi dùng chung Redis Server)
        var prefix = configuration["Hangfire:RedisPrefix"] ?? "hangfire:";

        // 3. Cấu hình và đăng ký Hangfire với Redis Storage
        services.AddHangfire(config =>
        {
            config
                // Thiết lập mức độ tương thích dữ liệu với phiên bản Hangfire 1.8.x
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                // Chỉ lưu tên Assembly ngắn gọn giúp tránh lỗi mismatch phiên bản class khi deploy bản build mới.
                .UseSimpleAssemblyNameTypeSerializer()
                // Cài đặt Newtonsoft.Json tối ưu hóa cho Hangfire để serialize/deserialize job.
                .UseRecommendedSerializerSettings()
                // Cấu hình Redis làm nơi lưu trữ dữ liệu thay vì RDBMS
                .UseRedisStorage(connectionString, new RedisStorageOptions
                {
                    Prefix = prefix
                });
        });

        // 4. Khởi chạy Hangfire Server xử lý ngầm các Job
        services.AddHangfireServer(options =>
        {
            // Gán số luồng (threads) xử lý job song song trên instance này.
            options.WorkerCount = workerCount;
            // Chỉ định các queue lắng nghe theo thứ tự ưu tiên cấu hình.
            options.Queues = queues;
            // Đặt tên duy nhất cho máy chủ này (TênMáy:MãGUID) giúp quản lý dễ dàng khi scale-out hệ thống.
            options.ServerName = $"{Environment.MachineName}:{Guid.NewGuid()}";
        });

        // 5. Đăng ký các Service xử lý vào DI Container làm nhiệm vụ Dependency Injection
        services.AddScoped<IJobService, HangfireJobService>();
        services.AddScoped<CleanupJob>();

        return services;
    }

    public static WebApplication UseHangfireDashboardIfEnabled(
        this WebApplication app,
        IConfiguration configuration)
    {
        // Đọc cờ bật/tắt Dashboard từ appsettings một cách an toàn
        if (!bool.TryParse(configuration["Hangfire:EnableDashboard"], out var enabled) || !enabled)
        {
            return app; // Nếu cấu hình lỗi hoặc gán bằng false thì bỏ qua middleware này
        }

        // Xác định đường dẫn truy cập Dashboard (mặc định là /hangfire)
        var path = configuration["Hangfire:DashboardPath"] ?? "/hangfire";

        // Bật middleware Dashboard với các tuỳ chọn bảo mật
        app.UseHangfireDashboard(path, new DashboardOptions
        {
            // Gán bộ lọc phân quyền truy cập Dashboard
            Authorization = [new HangfireDashboardAuthorizationFilter()],
            // Không hiển thị Redis Connection String trong Dashboard để tránh rò rỉ thông tin nhạy cảm
            DisplayStorageConnectionString = false,
            // Đặt tiêu đề hiển thị trên website dashboard
            DashboardTitle = "HoaTheoMua Background Jobs"
        });

        return app;
    }
}

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Lấy HttpContext hiện tại của request truy cập dashboard
        var httpContext = context.GetHttpContext();

        // Cho phép truy cập nếu thỏa mãn 1 trong 2 điều kiện:
        // 1. Môi trường phát triển (Development) -> Cho phép developer debug nhanh.
        // 2. Người dùng đã đăng nhập thành công vào hệ thống (IsAuthenticated == true).
        return httpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment()
            || (httpContext.User.Identity?.IsAuthenticated ?? false);
    }
}
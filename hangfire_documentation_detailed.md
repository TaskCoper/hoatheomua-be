# 🛠️ Hướng Dẫn Vận Hành & Phân Tích Hệ Thống Hangfire Background Jobs (Redis)

Tài liệu này phân tích chi tiết cơ chế hoạt động của hệ thống **Hangfire Background Jobs** đã được chuyển đổi từ PostgreSQL sang **Redis** trong dự án **HoaTheoMua**. Toàn bộ phần giải thích mã nguồn được tích hợp trực tiếp dưới dạng **code comments** bên trong các khối code để tăng tính trực quan.

---

## 📊 Sơ Đồ Quy Trình Xử Lý (Redis Workflow)

Dưới đây là sơ đồ mô tả luồng đi của một Background Job từ khi Client kích hoạt cho tới khi các Worker kéo từ Redis lên thực thi:

```mermaid
graph TD
    A["📱 API Request (Client)"] -->|Gọi phương thức| B["⚙️ IJobService (HangfireJobService)"]
    B -->|Biên dịch Expression thành JSON| C["💾 Redis Storage"]
    C -->|Đẩy ID vào Queue List| D["📥 hangfire:queue:default"]
    C -->|Đăng ký Job ID vào Metadata Hash| E["🗂️ hangfire:job:<id>"]
    
    subgraph Hangfire Server (Background Process)
        F["👷 Worker Thread 1"]
        G["👷 Worker Thread 2"]
        H["👷 Worker Thread N"]
    end
    
    D -->|BRPOP (Blocking Pop) - Phản hồi ngay tức thì| F
    F -->|Resolve Type từ DI Scope| I["🔄 DI Scope (IServiceProvider)"]
    I -->|Khởi tạo thực thể Job| J["🚀 Job Class (e.g., CleanupJob)"]
    J -->|Thực thi Method bằng Reflection| K{"❓ Kết quả thực thi"}
    
    K -->|Thành công| L["✅ Succeeded State (Cập nhật Hash)"]
    K -->|Thất bại| M["❌ Failed State (Đẩy vào hangfire:retry)"]
    M -->|Hết lượt retry| N["💀 Failed Permanent"]
    M -->|Còn lượt retry| O["🔄 Đợi Backoff delay -> Re-enqueue"]
```

---

## 1. Cấu Hình Hệ Thống (`appsettings.json`)

> [!NOTE]
> Mặc dù chuẩn JSON gốc không hỗ trợ chú thích, ASP.NET Core Configuration Builder vẫn hỗ trợ đọc định dạng JSONC (JSON with Comments) dưới đây để cấu hình tham số.

```jsonc
{
  "ConnectionStrings": {
    // 🗄️ CSDL PostgreSQL chính. Hangfire đã chuyển sang Redis nên KHÔNG tự tạo các bảng hangfire.* tại đây nữa.
    "PostgresSql": "Host=localhost:5432;Database=hoatheomua;Username=postgres;Password=MyStrongPassword123@",
    
    // ⚡ Chuỗi kết nối tới Redis Server để lưu trữ toàn bộ dữ liệu trạng thái Jobs, Queue.
    // - abortConnect=false: Nếu Redis khởi động chậm hơn API, API không bị crash mà sẽ tự động kết nối lại khi Redis sẵn sàng.
    "Redis": "hoatheomua-redis:6379,abortConnect=false"
  },
  "Redis": {
    // Tiền tố (prefix) cho các key cache nghiệp vụ thông thường (ICacheService), giúp tránh đụng độ với key của Hangfire.
    "InstanceName": "HoaTheoMua:",
    "DefaultExpirationMinutes": 60
  },
  "Hangfire": {
    // 🌐 URL dẫn tới dashboard quản trị (e.g., http://localhost:8080/hangfire)
    "DashboardPath": "/hangfire",
    // Bật giao diện UI quản lý jobs.
    "EnableDashboard": true,
    // Số luồng (threads) chạy song song trên mỗi instance để xử lý jobs từ queue.
    "WorkerCount": 5,
    // Thứ tự ưu tiên giảm dần của các hàng đợi: critical (quan trọng nhất) -> default -> low.
    "Queues": ["critical", "default", "low"],
    // Tiền tố riêng cho tất cả key của Hangfire trong Redis, tránh đụng độ với cache nghiệp vụ (HoaTheoMua:).
    "RedisPrefix": "hangfire:"
  }
}
```

---

## 2. Quản Lý NuGet Packages

Hệ thống phân tách Hangfire thành hai tầng rõ rệt để tối ưu hóa sự phụ thuộc (dependency decoupling):

### A. Tầng Service — [HoaTheoMua.Service.csproj](file:///c:/Users/Admin/Downloads/hoatheomua-be/HoaTheoMua.Service/HoaTheoMua.Service.csproj)

```xml
<!-- 
  Hangfire.Core: Chỉ chứa core interfaces/abstractions (IBackgroundJobClient, IRecurringJobManager).
  Không chứa các thư viện Web hay DB Storage giúp giữ tầng Service "sạch", độc lập và dễ dàng viết Unit Test.
-->
<PackageReference Include="Hangfire.Core" Version="1.8.17" />
```

### B. Tầng API (Web Host) — [HoaTheoMua.API.csproj](file:///c:/Users/Admin/Downloads/hoatheomua-be/HoaTheoMua.API/HoaTheoMua.API.csproj)

```xml
<!-- 
  Hangfire.AspNetCore: Tích hợp Hangfire vào ASP.NET Core DI Container và khởi chạy Background Server qua IHostedService.
-->
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.17" />

<!-- 
  Hangfire.Redis.StackExchange: Thư viện chuyển đổi Storage Provider của Hangfire từ SQL truyền thống sang bộ lưu trữ Redis.
-->
<PackageReference Include="Hangfire.Redis.StackExchange" Version="1.9.4" />
```

---

## 3. Khởi Tạo Và Đăng Ký Hệ Thống ([HangfireExtensions.cs](file:///c:/Users/Admin/Downloads/hoatheomua-be/HoaTheoMua.API/Extentions/HangfireExtensions.cs))

### 3.1. Thiết lập Dịch Vụ (`AddHangfireServices`)

```csharp
public static IServiceCollection AddHangfireServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Lấy chuỗi kết nối Redis từ cấu hình appsettings.json
    var connectionString = configuration.GetConnectionString("Redis");

    // Nếu không cấu hình Redis, bỏ qua việc đăng ký Hangfire thay vì làm crash ứng dụng khi khởi động
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return services;
    }

    // Đọc cấu hình từ appsettings.json, tự động fallback về các giá trị mặc định nếu thiếu
    var workerCount = int.Parse(configuration["Hangfire:WorkerCount"] ?? "5");
    var queues = configuration.GetSection("Hangfire:Queues").Get<string[]>()
        ?? ["critical", "default", "low"];
    var prefix = configuration["Hangfire:RedisPrefix"] ?? "hangfire:";

    // Đăng ký dịch vụ Hangfire và cấu hình lưu trữ
    services.AddHangfire(config =>
    {
        config
            // 📌 Thiết lập mức độ tương thích dữ liệu lên phiên bản 1.8.0+ để tối ưu hóa hiệu năng
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            
            // 📌 Chỉ lưu tên Class và Assembly ngắn gọn (không kèm version, culture, public key token).
            // Giúp Job không bị lỗi "Type Not Found" sau mỗi lần deploy code mới (khi version assembly thay đổi).
            .UseSimpleAssemblyNameTypeSerializer()
            
            // 📌 Sử dụng các thiết lập JSON Serializer khuyên dùng (camelCase, bỏ qua thuộc tính null)
            .UseRecommendedSerializerSettings()
            
            // 📌 Cấu hình Redis làm Storage lưu trữ thay vì PostgreSQL.
            // Mọi trạng thái của Job, Queue và distributed lock sẽ ghi trực tiếp vào bộ nhớ Redis.
            .UseRedisStorage(connectionString, new RedisStorageOptions
            {
                Prefix = prefix // Gom nhóm các key của Hangfire dưới prefix "hangfire:"
            });
    });

    // Đăng ký Background Server - Thành phần chạy nền quét Redis và giải phóng Job
    services.AddHangfireServer(options =>
    {
        options.WorkerCount = workerCount; // Giới hạn luồng xử lý song song trên server này
        options.Queues = queues;           // Hàng đợi mà máy chủ này được phép lắng nghe
        
        // Đặt tên định danh duy nhất cho Server (MachineName + GUID) để tránh xung đột khi chạy multi-instance
        options.ServerName = $"{Environment.MachineName}:{Guid.NewGuid()}";
    });

    // Đăng ký các dịch vụ nghiệp vụ bổ sung vào DI Container dưới dạng Scoped
    services.AddScoped<IJobService, HangfireJobService>(); // Wrapper API giúp gọi jobs từ tầng Service
    services.AddScoped<CleanupJob>();                      // Đăng ký chính lớp Job để DI giải quyết khi thực thi

    return services;
}
```

### 3.2. Đăng ký Dashboard (`UseHangfireDashboardIfEnabled`)

```csharp
public static WebApplication UseHangfireDashboardIfEnabled(
    this WebApplication app,
    IConfiguration configuration)
{
    // Đọc trạng thái bật/tắt dashboard
    var enabled = bool.Parse(configuration["Hangfire:EnableDashboard"] ?? "false");

    if (!enabled)
    {
        return app; // Trả về ngay lập tức nếu dashboard bị tắt
    }

    // Đọc đường dẫn URL truy cập dashboard
    var path = configuration["Hangfire:DashboardPath"] ?? "/hangfire";

    // Kích hoạt middleware hiển thị dashboard quản lý
    app.UseHangfireDashboard(path, new DashboardOptions
    {
        // Gán bộ lọc phân quyền bảo mật cho trang Dashboard
        Authorization = [new HangfireDashboardAuthorizationFilter()],
        
        // Ẩn thông tin chuỗi kết nối Redis dưới chân trang Dashboard để tránh lộ thông tin nhạy cảm
        DisplayStorageConnectionString = false,
        
        // Đặt tiêu đề hiển thị trên thanh tab trình duyệt
        DashboardTitle = "HoaTheoMua Background Jobs"
    });

    return app;
}
```

### 3.3. Phân Quyền Truy Cập Dashboard (`HangfireDashboardAuthorizationFilter`)

```csharp
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Trích xuất HttpContext của request truy cập hiện tại
        var httpContext = context.GetHttpContext();

        // Nguyên tắc phân quyền truy cập:
        return httpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment() // 1. Nếu đang chạy ở Local (Development): Cho phép truy cập tự do không cần đăng nhập
            || (httpContext.User.Identity?.IsAuthenticated ?? false); // 2. Nếu ở Production: Bắt buộc user phải Đăng nhập trước (Authenticated)
    }
}
```

---

## 4. Thứ Tự Đăng Ký Pipelines ([Program.cs](file:///c:/Users/Admin/Downloads/hoatheomua-be/HoaTheoMua.API/Program.cs))

> [!IMPORTANT]
> Middleware `UseHangfireDashboardIfEnabled` bắt buộc phải đặt dưới `UseAuthorization()`. Nếu đặt trước, thông tin đăng nhập trong `HttpContext.User` sẽ trống rỗng, khiến bộ lọc authorization chặn quyền truy cập dashboard ở môi trường Production.

```csharp
// --- ĐĂNG KÝ SERVICES ---
// Đăng ký Redis Cache trước vì các Background Jobs có thể phụ thuộc vào ICacheService
builder.Services.AddRedisCacheServices(builder.Configuration);

// Đăng ký toàn bộ cấu hình Hangfire Storage & Server
builder.Services.AddHangfireServices(builder.Configuration); 

// ...

// --- THIẾT LẬP MIDDLEWARE ---
app.UseAuthentication(); // Giải mã cookie/token xác định danh tính User
app.UseAuthorization();  // Phân quyền User

// Kích hoạt Dashboard Middleware sau khi đã giải quyết Authentication & Authorization
app.UseHangfireDashboardIfEnabled(builder.Configuration); 

app.MapControllers();
app.Run();
```

---

## 5. Cấu Trúc Khối Wrapper (`IJobService` & `HangfireJobService`)

### A. Interface [IJobService.cs](file:///c:/Users/Admin/Downloads/hoatheomua-be/HoaTheoMua.Service/Jobs/IJobService.cs)

```csharp
public interface IJobService
{
    // Fire-and-forget Job: Thực thi bất đồng bộ ngay lập tức (không chặn luồng request hiện tại)
    string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall);

    // Delayed Job: Lên lịch thực thi sau một khoảng thời gian chờ (delay)
    string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay);

    // Recurring Job: Lặp đi lặp lại định kỳ theo chu kỳ thời gian CRON
    void AddOrUpdateRecurring<TJob>(
        string recurringJobId,
        Expression<Func<TJob, Task>> methodCall,
        string cronExpression,
        string queue = "default");

    // Xóa một recurring job theo ID định danh
    void RemoveRecurring(string recurringJobId);

    // Hủy bỏ thủ công một job đang chờ xử lý hoặc đã lên lịch thông qua Job ID
    bool Delete(string jobId);
}
```

### B. Lớp Triển Khai [HangfireJobService.cs](file:///c:/Users/Admin/Downloads/hoatheomua-be/HoaTheoMua.Service/Jobs/HangfireJobService.cs)

```csharp
public class HangfireJobService : IJobService
{
    // SDK Client xử lý các tác vụ thêm/xóa/hủy job thông thường (Enqueue, Schedule, Delete)
    private readonly IBackgroundJobClient _backgroundJobClient;
    
    // SDK Client xử lý riêng các job định kỳ (Recurring)
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireJobService> _logger;

    public HangfireJobService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireJobService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall)
    {
        // Gửi thông tin biểu thức xuống Redis, đưa job vào hàng đợi thực thi ngay lập tức
        var jobId = _backgroundJobClient.Enqueue(methodCall);
        _logger.LogInformation("Hangfire job enqueued. JobId: {JobId}", jobId);
        return jobId;
    }

    public string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay)
    {
        // Ghi nhận job kèm thời điểm thực thi dự kiến vào Redis Sorted Set
        var jobId = _backgroundJobClient.Schedule(methodCall, delay);
        _logger.LogInformation("Hangfire job scheduled. JobId: {JobId}, Delay: {Delay}", jobId, delay);
        return jobId;
    }

    public void AddOrUpdateRecurring<TJob>(
        string recurringJobId,
        Expression<Func<TJob, Task>> methodCall,
        string cronExpression,
        string queue = "default")
    {
        // Đăng ký hoặc cập nhật chu kỳ thực thi định kỳ của một job
        _recurringJobManager.AddOrUpdate(
            recurringJobId, // ID định danh duy nhất cho job định kỳ này
            methodCall,     // Biểu thức Lambda gọi phương thức nghiệp vụ
            cronExpression, // Định dạng thời gian biểu CRON (e.g., "*/5 * * * *")
            new RecurringJobOptions { QueueName = queue }); // Phân bổ job chạy trên queue nào

        _logger.LogInformation(
            "Hangfire recurring job registered. JobId: {JobId}, Cron: {Cron}, Queue: {Queue}",
            recurringJobId, cronExpression, queue);
    }

    public void RemoveRecurring(string recurringJobId)
    {
        // Xóa hoàn toàn cấu hình tác vụ định kỳ ra khỏi Redis
        _recurringJobManager.RemoveIfExists(recurringJobId);
    }

    public bool Delete(string jobId)
    {
        // Hủy bỏ trạng thái chờ thực thi của Job thông qua Id
        return _backgroundJobClient.Delete(jobId);
    }
}
```

> [!TIP]
> **Tại sao Hangfire sử dụng `Expression<Func<TJob, Task>>`?**
> Hangfire không thực thi trực tiếp biểu thức lambda truyền vào. Nó phân tích cấu trúc của biểu thức (Expression Tree) để trích xuất tên Class, tên Method, và các tham số truyền vào rồi lưu chúng thành JSON xuống Redis. Điều này cho phép Job được lưu lại và thực thi bất đồng bộ sau đó bởi các Worker chạy trên các thread khác nhau hoặc thậm chí ở các server khác nhau.

---

## 6. Job Nghiệp Vụ Thực Tế ([CleanupJob.cs](file:///c:/Users/Admin/Downloads/hoatheomua-be/HoaTheoMua.Service/Jobs/CleanupJob.cs))

```csharp
public class CleanupJob
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CleanupJob> _logger;

    // Yêu cầu CacheService để tương tác ghi đè cache
    public CleanupJob(ICacheService cacheService, ILogger<CleanupJob> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    // Làm nóng cache dashboard bằng cách ghi dữ liệu trước vào Redis Cache
    public async Task WarmupCacheAsync()
    {
        _logger.LogInformation("Warming up dashboard cache");

        // Ghi key "stats:dashboard" vào Redis Cache thông thường với TTL 1 giờ.
        // Redis thực tế sẽ lưu key này là "HoaTheoMua:stats:dashboard" (do Prefix cấu hình cho cache).
        await _cacheService.SetAsync(
            "stats:dashboard",
            $"Warmup at {DateTimeOffset.UtcNow}",
            TimeSpan.FromHours(1));
    }

    // Logic dọn dẹp các dữ liệu hết hạn (hiện tại là khung sườn, trả về Task.CompletedTask)
    public Task CleanupExpiredDataAsync()
    {
        _logger.LogInformation("Cleaning expired data");
        return Task.CompletedTask;
    }
}
```

### ⚙️ Cách Kích Hoạt Chạy `CleanupJob` Định Kỳ Khi API Khởi Chạy:

Bổ sung đoạn mã này vào cuối file [Program.cs](file:///c:/Users/Admin/Downloads/hoatheomua-be/HoaTheoMua.API/Program.cs) (ngay trước dòng `app.Run()`) để khởi tạo các job nền:

```csharp
// Tạo một DI Scope tạm thời để giải quyết các dịch vụ đăng ký dạng Scoped khi API bắt đầu chạy
using (var scope = app.Services.CreateScope())
{
    // Lấy instance của IJobService từ DI Scope hiện tại
    var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();

    // Đăng ký job làm nóng cache định kỳ chạy mỗi 30 phút, đẩy vào hàng đợi "low" (ít quan trọng)
    jobService.AddOrUpdateRecurring<CleanupJob>(
        recurringJobId: "warmup-cache-dashboard",
        methodCall: job => job.WarmupCacheAsync(),
        cronExpression: "*/30 * * * *", // Định dạng CRON: Chạy sau mỗi 30 phút
        queue: "low"                   // Chạy trên hàng đợi độ ưu tiên thấp để không ảnh hưởng luồng chính
    );
}
```

---

## 7. Cơ Chế Phân Phối Hàng Đợi (Queue Priorities)

Cấu hình thứ tự ưu tiên của queue được khai báo khi khởi động Background Server:

```csharp
services.AddHangfireServer(options =>
{
    // Thứ tự khai báo từ trái sang phải quyết định độ ưu tiên:
    // Hàng đợi phía trước phải hoàn toàn TRỐNG thì hàng đợi phía sau mới được xử lý.
    options.Queues = ["critical", "default", "low"];
});
```

*   `critical`: Xử lý thanh toán đơn hàng, gửi mã OTP.
*   `default`: Tác vụ gửi mail xác nhận đơn hàng, đồng bộ trạng thái thông thường.
*   `low`: Dọn dẹp logs, dọn dữ liệu rác, nạp cache trước.

---

## 8. Bảng So Sánh Hiệu Năng: PostgreSQL vs Redis Storage

| Tiêu Chí | PostgreSQL Storage (Cũ) | Redis Storage (Mới) |
| :--- | :--- | :--- |
| **Cơ chế kiểm tra Job (Polling)** | **Active Polling**: Luồng worker liên tục gửi các câu lệnh `SELECT ... FOR UPDATE` xuống DB sau mỗi chu kỳ (mặc định 15 giây). | **Event-driven (BRPOP)**: Worker sử dụng lệnh blocking pop `BRPOP` của Redis. Luồng sẽ ngủ và được Redis đánh thức **ngay lập tức** khi có job mới được đẩy vào hàng đợi. |
| **Độ trễ kích hoạt Job (Latency)** | Lên tới **~15 giây** (tùy thuộc vào thời gian chạy chu kỳ polling tiếp theo). | Gần như **tức thì (vài mili-giây)** kể từ khi client gọi `Enqueue`. |
| **Ảnh hưởng tài nguyên DB chính** | Tạo áp lực truy vấn I/O liên tục, ghi log transaction và có nguy cơ gây deadlock bảng dữ liệu nếu số lượng worker lớn. | Không làm ảnh hưởng đến cơ sở dữ liệu chính. Toàn bộ tài nguyên PostgreSQL được giải phóng hoàn toàn dành riêng cho luồng nghiệp vụ. |
| **Cơ chế khóa phân tán (Locking)** | Dựa trên cơ chế Row-level lock của database, phản hồi chậm và dễ bị nghẽn. | Dựa trên cơ chế phân tán siêu nhanh của Redis (sử dụng các lệnh nguyên tử `SET NX PX` hoặc Redis Redlock). |
| **Rủi ro mất mát dữ liệu** | An toàn tuyệt đối nhờ cơ chế ACID của hệ quản trị cơ sở dữ liệu quan hệ. | Phụ thuộc vào cấu hình của Redis (Cần cấu hình AOF để đảm bảo dữ liệu không bị mất khi Redis restart). |

---

## 9. Chi Tiết Các Key Hangfire Tạo Ra Trong Redis

Dữ liệu của Hangfire trong Redis được lưu trữ dưới các key có tiền tố `hangfire:` phân tách bởi các kiểu dữ liệu tương ứng:

1.  **`hangfire:job:<id>`** (Kiểu: `Hash`)
    *   Lưu trữ thông tin chi tiết về job (tên class, method, tham số serialize, trạng thái hiện tại).
2.  **`hangfire:queue:<name>`** (Kiểu: `List`)
    *   Danh sách chứa ID của các job đang xếp hàng chờ. Worker thực thi lệnh `BRPOP` trên List này.
3.  **`hangfire:schedule`** (Kiểu: `Sorted Set`)
    *   Lưu các job hẹn giờ (delayed). **Score** là timestamp mốc thời gian chạy. Khi đến giờ, scheduler sẽ đẩy nó sang queue `List`.
4.  **`hangfire:recurring`** (Kiểu: `Hash`)
    *   Cấu hình của các job định kỳ (ID, Cron, Queue đích).
5.  **`hangfire:server:<id>`** (Kiểu: `Hash`)
    *   Heartbeat định kỳ của các máy chủ Hangfire hoạt động.

---

## 10. Checklist Lưu Ý Vận Hành Trên Production

> [!WARNING]
> **Thiết kế Idempotent (Bất biến)**:
> Hangfire áp dụng cơ chế phân phát **At-Least-Once** (Ít nhất một lần). Nếu worker đột tử khi đang chạy dở job, Hangfire sẽ tự động lấy ra thực thi lại.
> Do đó, code trong các job xử lý phải kiểm tra trạng thái trước khi chạy (ví dụ: gửi mail thì kiểm tra trạng thái cờ `IsMailSent`, trừ tiền thì kiểm tra `IsPaid`) để tránh việc thực hiện lặp đi lặp lại gây lỗi nghiệp vụ.

> [!IMPORTANT]
> **Bật chế độ ghi đĩa Persistence cho Redis**:
> Bắt buộc bật tính năng **AOF (Append Only File)** với tần suất `appendfsync everysec` trên Redis server chạy Hangfire để đảm bảo dữ liệu job không bị mất sạch khi Redis bị sập đột ngột.

> [!CAUTION]
> **Hạn chế truyền tham số lớn vào Lambda Expression**:
> Chỉ truyền ID định danh (như `OrderId`, `UserId`) vào lambda.
> *Không nên truyền cả đối tượng Order phức tạp hoặc ảnh Base64*. Điều này làm tăng kích thước payload lưu trong Redis và dữ liệu của Object có thể bị cũ (lỗi thời) so với dữ liệu thực tế dưới DB tại thời điểm worker chạy job.

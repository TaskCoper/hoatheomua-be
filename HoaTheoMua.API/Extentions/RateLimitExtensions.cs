using System.Globalization;
using System.Threading.RateLimiting;

namespace HoaTheoMua.Extentions;

public static class RateLimitExtensions
{
    public const string ApiPolicy = "api";
    public const string LoginPolicy = "login";
    public const string HeavyPolicy = "heavy";
    public const string OrderWritePolicy = "order-write";
    public const string ShippingEstimatePolicy = "shipping-estimate";
    public const string ImportPolicy = "import";

    public static void ConfigureRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, _) =>
            {
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var duration)
                    ? Math.Max(1, (int)Math.Ceiling(duration.TotalSeconds))
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString(CultureInfo.InvariantCulture);
                context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
                await context.HttpContext.Response
                    .WriteAsync("Too many requests. Please retry later.", cancellationToken: _);
            };

            // Baseline protection for every endpoint: 120 requests/minute per authenticated user
            // or per client IP for anonymous traffic.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserOrIpPartitionKey(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddPolicy(ApiPolicy, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserOrIpPartitionKey(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0 // fail fast
                    }));

            // Strict login policy
            options.AddPolicy(LoginPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetIpPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Heavy operation limit
            options.AddPolicy(HeavyPolicy, context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    GetUserOrIpPartitionKey(context),
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10,
                        TokensPerPeriod = 5,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(OrderWritePolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserOrIpPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(ShippingEstimatePolicy, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserOrIpPartitionKey(context),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddPolicy(ImportPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserOrIpPartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));
        });
    }

    private static string GetUserOrIpPartitionKey(HttpContext context)
    {
        var userName = context.User.Identity?.Name;
        return !string.IsNullOrWhiteSpace(userName)
            ? $"user:{userName}"
            : GetIpPartitionKey(context);
    }

    private static string GetIpPartitionKey(HttpContext context) =>
        $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}

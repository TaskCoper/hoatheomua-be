using Microsoft.Extensions.DependencyInjection;

namespace HoaTheoMua.Service.PostPublishing;

public static class Registration
{
    public static IServiceCollection AddPostGenPublishingAdapters(this IServiceCollection services)
    {
        services.AddScoped<IPublishingAdapter, FacebookPublisher>();
        services.AddScoped<IPublishingAdapter, InstagramPublisher>();
        services.AddScoped<IPublishingAdapter, ZaloPublisher>();
        services.AddHttpClient<ZaloPublishingClient>("PostGenZaloPublishing", client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.MaxResponseContentBufferSize = 1024 * 1024;
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }).RemoveAllLoggers();
        services.AddHttpClient<MetaPublishingClient>("PostGenPublishing", client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.MaxResponseContentBufferSize = 1024 * 1024;
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }).RemoveAllLoggers();
        services.AddHttpClient<PublishingMediaClient>("PostGenPublishingMedia", client => client.Timeout = Timeout.InfiniteTimeSpan)
            .ConfigurePrimaryHttpMessageHandler(PublishingMediaClient.Handler).RemoveAllLoggers();
        return services;
    }
}

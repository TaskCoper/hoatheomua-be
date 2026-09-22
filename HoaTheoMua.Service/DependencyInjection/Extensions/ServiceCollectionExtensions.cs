using System.Reflection;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.DependencyInjection.Options;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Fulfillment;
using HoaTheoMua.Service.Fulfillment.Consumers;
using HoaTheoMua.Service.Utils.JsonConverters;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace HoaTheoMua.Service.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMasstransitRabbitMqInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqOption = new RabbitMqOptions();
        configuration.GetSection(nameof(RabbitMqOptions)).Bind(rabbitMqOption);

        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumers(Assembly
                .GetExecutingAssembly());

            cfg.SetKebabCaseEndpointNameFormatter();

            // Đăng ký qua DI, KHÔNG dùng bus.ConnectConsumeObserver(new ...): instance tự new
            // không lấy được ILogger từ container.
            cfg.AddConsumeObserver<FulfillmentConsumeObserver>();

            cfg.AddSagaStateMachine<OrderFulfillmentStateMachine, OrderFulfillmentState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;   // SELECT ... FOR UPDATE
                    r.LockStatementProvider = new PostgresLockStatementProvider();
                    r.ExistingDbContext<AppDbContext>();
                });

            cfg.AddEntityFrameworkOutbox<AppDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();                                       // publish trong SaveChanges của webhook
                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
            });

            // Retry chỉ cho lỗi transient; lỗi nghiệp vụ fault ngay để saga bù trừ sớm.
            cfg.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                // Publishing commits intent/checkpoints before HTTP. A transaction around the whole
                // consumer or automatic consumer retry would erase that protection on a crash.
                // Durable recovery and DB revision/lease checks replace this endpoint's inbox retry.
                if (name == PostPublishing.PublicationConsumerDefinition.QueueName
                    || name == FlowerJobs.FlowerGenerationConsumerDefinition.QueueName
                    || name == CardJobs.CardGenerationConsumerDefinition.QueueName) return;
                // Bù trừ được saga phát lại tối đa MaxCompensationRounds vòng, nên endpoint này
                // retry ít hơn: 3 lần gọi/vòng × 3 vòng = tối đa 9 lần gọi API hủy, thay vì 18.
                var intervals = name == "compensate-fulfillment"
                    ? new[] { 1000, 5000 }
                    : new[] { 1000, 5000, 15000, 30000, 60000 };

                cfg.UseMessageRetry(r =>
                {
                    r.Intervals(intervals);
                    r.Ignore<FulfillmentPermanentException>();
                    /*
                    FulfillmentTransientException (502) — HTTP 5xx, HttpRequestException, TaskCanceledException/timeout → có retry
                    FulfillmentPermanentException (400) — HTTP 4xx, NhanhVn code != 1 (đơn bị từ chối vì nghiệp vụ) → không retry, fault ngay
                    */
                });
                cfg.UseEntityFrameworkOutbox<AppDbContext>(context);    // inbox: chống xử lý trùng
            });

            cfg.UsingRabbitMq((context, bus) =>
            {
                bus.Host(rabbitMqOption.Host, rabbitMqOption.Port, rabbitMqOption.VirtualHost, h =>
                {
                    h.Username(rabbitMqOption.Username);
                    h.Password(rabbitMqOption.Password);
                });

                // Đặt tên EXCHANGE. SetKebabCaseEndpointNameFormatter ở trên chỉ đặt tên QUEUE;
                // không có dòng này exchange vẫn là "HoaTheoMua.Service.Fulfillment:OrderPaid".
                bus.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter());

                bus.UseNewtonsoftJsonSerializer();

                bus.ConfigureNewtonsoftJsonSerializer(settings =>
                {
                    settings.Converters.Add(new TypeNameHandlingConverter(TypeNameHandling.Objects));
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    return settings;
                });

                bus.ConfigureNewtonsoftJsonDeserializer(settings =>
                {
                    settings.Converters.Add(new TypeNameHandlingConverter(TypeNameHandling.Objects));
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    return settings;
                });

                // BẮT BUỘC: không có dòng này thì không consumer/saga nào được gắn vào endpoint,
                // bus vẫn kết nối RabbitMQ bình thường nhưng không có queue nào được tạo.
                bus.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

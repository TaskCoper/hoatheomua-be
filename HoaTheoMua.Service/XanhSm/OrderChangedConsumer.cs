using MassTransit;

namespace HoaTheoMua.Service.XanhSm;

public class XanhSmOrderChangedConsumer(XanhSmDeliveryCompletionService service) : IConsumer<XanhSmOrderChanged>
{
    public Task Consume(ConsumeContext<XanhSmOrderChanged> context)
        => service.ProcessAsync(context.Message, context.CancellationToken);
}

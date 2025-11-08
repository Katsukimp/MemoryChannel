using MemoryEventBus.Domain.Events.Concrete;
namespace MemoryEventBus.Domain.Events.Interfaces.Producers
{
    public interface IOrderPaidProducer
    {
        Task PublishAsync(OrderPaidEvent @event);
    }
}

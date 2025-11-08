using MemoryEventBus.Domain.Events.Aggregate;

namespace MemoryEventBus.Domain.Events.Interfaces.Base
{
    public interface IEventConsumer<TEvent> where TEvent : DomainEvent
    {
        Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
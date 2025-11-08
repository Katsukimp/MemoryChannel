using MemoryEventBus.Domain.Events.Aggregate;

namespace MemoryEventBus.Domain.Events.Interfaces.Base
{
    public interface IEventBusErrorHandler
    {
        Task HandleErrorAsync<TEvent>(TEvent @event, Exception exception, int attemptNumber, CancellationToken cancellationToken = default) 
            where TEvent : DomainEvent;
        
        bool ShouldRetry<TEvent>(TEvent @event, Exception exception, int attemptNumber) 
            where TEvent : DomainEvent;
    }
}
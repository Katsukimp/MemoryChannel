using MemoryEventBus.Domain.Events.Aggregate;

namespace MemoryEventBus.Domain.Events.Interfaces.Base
{
    /// <summary>
    /// Defines a contract for processing events of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    public interface IEventConsumer<TEvent> where TEvent : DomainEvent
    {
        /// <summary>
        /// Processes a single event instance.
        /// </summary>
        /// <param name="event">The event to process.</param>
        /// <param name="cancellationToken">A token to cancel processing.</param>
        Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
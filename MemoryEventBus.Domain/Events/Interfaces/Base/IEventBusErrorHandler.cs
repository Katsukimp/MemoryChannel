using MemoryEventBus.Domain.Events.Aggregate;

namespace MemoryEventBus.Domain.Events.Interfaces.Base
{
    /// <summary>
    /// Handles errors that occur during event processing and can influence retry decisions.
    /// </summary>
    public interface IEventBusErrorHandler
    {
        /// <summary>
        /// Invoked when an exception occurs while processing an event.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="event">The event instance that failed.</param>
        /// <param name="exception">The thrown exception.</param>
        /// <param name="attemptNumber">The current retry attempt number.</param>
        /// <param name="cancellationToken">A token to cancel the handling operation.</param>
        Task HandleErrorAsync<TEvent>(TEvent @event, Exception exception, int attemptNumber, CancellationToken cancellationToken = default)
            where TEvent : DomainEvent;
        
        /// <summary>
        /// Determines whether the failure should be retried.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="event">The event instance that failed.</param>
        /// <param name="exception">The thrown exception.</param>
        /// <param name="attemptNumber">The current retry attempt number.</param>
        /// <returns>True if the operation should be retried; otherwise false.</returns>
        bool ShouldRetry<TEvent>(TEvent @event, Exception exception, int attemptNumber)
            where TEvent : DomainEvent;
    }
}
using System.Threading.Channels;
using MemoryEventBus.Domain.Events.Aggregate;

namespace MemoryEventBus.Domain.Events.Interfaces.Base
{
    /// <summary>
    /// Manages channels used to publish and consume domain events.
    /// </summary>
    public interface IEventChannelManager
    {
        /// <summary>
        /// Gets an existing channel for the specified event type or creates an unbounded one if missing.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        Channel<TEvent> GetOrCreateChannel<TEvent>() where TEvent : DomainEvent;

        /// <summary>
        /// Gets an existing channel for the specified event type or creates a bounded one with the given capacity.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="capacity">The bounded channel capacity.</param>
        Channel<TEvent> GetOrCreateBoundedChannel<TEvent>(int capacity) where TEvent : DomainEvent;

        /// <summary>
        /// Closes and removes the channel associated with the event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        void CloseChannel<TEvent>() where TEvent : DomainEvent;

        /// <summary>
        /// Gets the number of channels currently tracked.
        /// </summary>
        int GetChannelCount();

        /// <summary>
        /// Gets the approximate depth for the channel of the specified event type. Returns -1 if depth not supported.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        int GetChannelDepth<TEvent>() where TEvent : DomainEvent;

        /// <summary>
        /// Attempts to write an event to its channel.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="event">The event instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if written; false otherwise.</returns>
        ValueTask<bool> TryWriteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : DomainEvent;
    }
}

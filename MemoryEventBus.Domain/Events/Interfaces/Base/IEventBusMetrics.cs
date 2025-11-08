using System.Diagnostics.Metrics;

namespace MemoryEventBus.Domain.Events.Interfaces.Base
{
    /// <summary>
    /// Provides hooks to record metrics related to event publishing and consumption.
    /// </summary>
    public interface IEventBusMetrics
    {
        /// <summary>
        /// Records that an event was published successfully.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="eventType">The event type name.</param>
        void RecordEventPublished<TEvent>(string eventType);

        /// <summary>
        /// Records that an event was consumed successfully along with processing time.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="eventType">The event type name.</param>
        /// <param name="processingTime">The time spent processing the event.</param>
        void RecordEventConsumed<TEvent>(string eventType, TimeSpan processingTime);

        /// <summary>
        /// Records a failed event processing attempt.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="eventType">The event type name.</param>
        /// <param name="errorType">A classification of the error (exception type or logical reason).</param>
        void RecordEventFailed<TEvent>(string eventType, string errorType);

        /// <summary>
        /// Records a retry attempt for an event.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="eventType">The event type name.</param>
        /// <param name="attemptNumber">The number of the retry attempt.</param>
        void RecordRetryAttempt<TEvent>(string eventType, int attemptNumber);

        /// <summary>
        /// Records the current depth of a given channel.
        /// </summary>
        /// <param name="channelType">The channel (event type) name.</param>
        /// <param name="depth">The number of items queued, or -1 if depth not available.</param>
        void RecordChannelDepth(string channelType, int depth);
    }
}
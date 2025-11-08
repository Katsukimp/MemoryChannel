namespace MemoryEventBus.Domain.Events.Aggregate
{
    /// <summary>
    /// Base type for all events published through the in-memory event bus.
    /// </summary>
    public class DomainEvent
    {
        /// <summary>
        /// Unique identifier for the event instance.
        /// </summary>
        public virtual required string EventId { get; set; }

        /// <summary>
        /// Logical name of the event (used for logging/metrics).
        /// </summary>
        public virtual required string EventName { get; set; }

        /// <summary>
        /// UTC timestamp when the event occurred.
        /// </summary>
        public virtual required DateTime OccurredOn { get; set; }
    }
}

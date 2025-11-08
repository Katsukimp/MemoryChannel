namespace MemoryEventBus.Domain.Events.Configuration
{
    /// <summary>
    /// Options to configure the in-memory event bus behavior such as channel capacity and retry defaults.
    /// </summary>
    public class EventBusOptions
    {
        /// <summary>
        /// Default capacity used when creating bounded channels for events that do not specify a capacity.
        /// </summary>
        public int DefaultChannelCapacity { get; set; } = 1000;

        /// <summary>
        /// Indicates whether the bus should create bounded channels by default instead of unbounded channels.
        /// </summary>
        public bool UseBoundedChannels { get; set; } = false;

        /// <summary>
        /// Default maximum number of retry attempts for event processing when no per-event override is provided.
        /// </summary>
        public int DefaultMaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Base backoff delay used to compute retry delays (e.g., for exponential backoff).
        /// </summary>
        public TimeSpan DefaultRetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Maximum backoff delay used when computing retry delays.
        /// </summary>
        public TimeSpan DefaultRetryMaxDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Optional per-event capacity overrides. Key is the event type name, value is the channel capacity.
        /// </summary>
        public Dictionary<string, int> EventTypeChannelCapacities { get; set; } = [];

        /// <summary>
        /// Optional per-event retry attempts overrides. Key is the event type name, value is the max retry attempts.
        /// </summary>
        public Dictionary<string, int> EventTypeMaxRetryAttempts { get; set; } = [];
    }
}
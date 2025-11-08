namespace MemoryEventBus.Infrastructure.Events.Health
{
    /// <summary>
    /// Options used to evaluate the health of the event bus.
    /// </summary>
    public sealed class EventBusHealthOptions
    {
        /// <summary>
        /// Maximum depth threshold beyond which the health check may report a degraded status.
        /// </summary>
        public int MaxDepthWarningThreshold { get; set; } = 10_000;

        /// <summary>
        /// Maximum number of channels allowed before reporting a degraded status.
        /// </summary>
        public int MaxChannelsWarningThreshold { get; set; } = 100;
    }
}

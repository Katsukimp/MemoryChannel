namespace MemoryEventBus.Infrastructure.Events.Health;
public sealed class EventBusHealthOptions
{
    public int MaxDepthWarningThreshold { get; set; } = 10_000;
    public int MaxChannelsWarningThreshold { get; set; } = 100;
}

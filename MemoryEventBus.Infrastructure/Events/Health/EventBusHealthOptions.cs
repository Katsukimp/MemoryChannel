namespace MemoryEventBus.Infrastructure.Events.Health
{
 public sealed class EventBusHealthOptions
 {
 public int MaxDepthWarningThreshold { get; set; } =10_000; // warn if any channel exceeds
 public int MaxChannelsWarningThreshold { get; set; } =100; // warn if too many channels
 }
}

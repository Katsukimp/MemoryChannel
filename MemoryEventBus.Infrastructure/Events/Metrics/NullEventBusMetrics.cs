using MemoryEventBus.Domain.Events.Interfaces.Base;

namespace MemoryEventBus.Infrastructure.Events.Metrics;
internal sealed class NullEventBusMetrics : IEventBusMetrics
{
    public void RecordChannelDepth(string channelType, int depth) { }
    public void RecordEventConsumed<TEvent>(string eventType, TimeSpan processingTime) { }
    public void RecordEventFailed<TEvent>(string eventType, string errorType) { }
    public void RecordEventPublished<TEvent>(string eventType) { }
    public void RecordRetryAttempt<TEvent>(string eventType, int attemptNumber) { }
}
